using AutoMapper;
using Btms.API.Helpers;
using Btms.API.Middlewares;
using Btms.Data.Context;
using Btms.Data.Entities;
using Btms.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models.Account;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace Btms.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _context;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IEmailService _emailService;

        public AccountService(
            DataContext context,
            IJwtUtils jwtUtils,
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            IEmailService emailService)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _emailService = emailService;
        }



        public LoginResponse Login(LoginRequest model, string ipAddress)
        {
            var account = _context.Accounts.SingleOrDefault(x => x.email == model.email);

            if (account == null)
                throw new AppException("No account found with this email");

            if (!account.is_verified || !BC.Verify(model.password, account.password))
                throw new AppException("Email or password is incorrect");

            var jwtToken = _jwtUtils.GenerateJwtToken(account);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            account.refresh_tokens.Add(refreshToken);

            removeOldRefreshTokens(account);

            _context.Update(account);
            _context.SaveChanges();

            var response = _mapper.Map<LoginResponse>(account);
            response.access_token = jwtToken;
            response.refresh_token = refreshToken.token;
            return response;
        }
        public void Signup(SignupRequest model, string origin)
        {
            if (_context.Accounts.Any(x => x.email == model.email))
            {
                sendAlreadyRegisteredEmail(model.email, origin);
                return;
            }

            var account = _mapper.Map<Account>(model);

            // first registered account is an admin
            var isFirstAccount = _context.Accounts.Count() == 0;
            account.role = isFirstAccount ? Role.Administrator : Role.User;
            account.created = DateTime.UtcNow;
            account.verification_token = generateVerificationToken();
            account.accept_terms = true;
            account.language = "en";

            _context.Accounts.Add(account);
            _context.SaveChanges();

            // send email
            sendVerificationEmail(account, origin);
        }
        public void VerifyEmail(VerifyEmailRequest model)
        {
            var account = _context.Accounts.SingleOrDefault(x => x.verification_token == model.token);

            if (account == null)
                throw new AppException("Verification failed");

            account.verified = DateTime.UtcNow;
            account.verification_token = null;
            account.password = BC.HashPassword(model.password);

            _context.Accounts.Update(account);
            _context.SaveChanges();
        }
        public void ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var account = _context.Accounts.SingleOrDefault(x => x.email == model.email);

            // always return ok response to prevent email enumeration
            if (account == null) return;

            // create reset token that expires after 1 day
            account.reset_token = generateResetToken();
            account.reset_token_expires = DateTime.UtcNow.AddDays(1);

            _context.Accounts.Update(account);
            _context.SaveChanges();

            // send email
            sendPasswordResetEmail(account, origin);
        }
        public void ResetPassword(ResetPasswordRequest model)
        {
            var account = getAccountByResetToken(model.token);

            account.password = BC.HashPassword(model.password);
            account.password_reset = DateTime.UtcNow;
            account.reset_token = null;
            account.reset_token_expires = null;

            _context.Accounts.Update(account);
            _context.SaveChanges();
        }
        public LoginResponse RefreshToken(string token, string ipAddress)
        {
            var account = getAccountByRefreshToken(token);
            var refreshToken = account.refresh_tokens.Single(x => x.token == token);

            if (refreshToken.is_revoked)
            {
                revokeDescendantRefreshTokens(refreshToken, account, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(account);
                _context.SaveChanges();
            }

            if (!refreshToken.is_active)
                throw new AppException("Invalid token");

            var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
            account.refresh_tokens.Add(newRefreshToken);

            removeOldRefreshTokens(account);

            _context.Update(account);
            _context.SaveChanges();

            var jwtToken = _jwtUtils.GenerateJwtToken(account);

            var response = _mapper.Map<LoginResponse>(account);
            response.access_token = jwtToken;
            response.refresh_token = newRefreshToken.token;
            return response;
        }
        public void RevokeToken(string token, string ipAddress)
        {
            var account = getAccountByRefreshToken(token);
            var refreshToken = account.refresh_tokens.Single(x => x.token == token);

            if (!refreshToken.is_active)
                throw new AppException("Invalid token");

            revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
            _context.Update(account);
            _context.SaveChanges();
        }
        public void ValidateResetToken(ValidateResetTokenRequest model)
        {
            getAccountByResetToken(model.token);
        }



        // helper methods
        private Account getAccountByRefreshToken(string token)
        {
            var account = _context.Accounts.SingleOrDefault(u => u.refresh_tokens.Any(t => t.token == token));
            if (account == null) throw new AppException("Invalid token");
            return account;
        }
        private Account getAccountByResetToken(string token)
        {
            var account = _context.Accounts.SingleOrDefault(x =>
                x.reset_token == token && x.reset_token_expires > DateTime.UtcNow);
            if (account == null) throw new AppException("Invalid token");
            return account;
        }
        private string generateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string generateResetToken()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            var tokenIsUnique = !_context.Accounts.Any(x => x.reset_token == token);
            if (!tokenIsUnique)
                return generateResetToken();

            return token;
        }
        private string generateVerificationToken()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            var tokenIsUnique = !_context.Accounts.Any(x => x.verification_token == token);
            if (!tokenIsUnique)
                return generateVerificationToken();

            return token;
        }
        private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.token);
            return newRefreshToken;
        }
        private void removeOldRefreshTokens(Account account)
        {
            account.refresh_tokens.RemoveAll(x =>
                !x.is_active &&
                x.created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }
        private void revokeDescendantRefreshTokens(RefreshToken refreshToken, Account account, string ipAddress, string reason)
        {
            if (!string.IsNullOrEmpty(refreshToken.replaced_by_token))
            {
                var childToken = account.refresh_tokens.SingleOrDefault(x => x.token == refreshToken.replaced_by_token);
                if (childToken.is_active)
                    revokeRefreshToken(childToken, ipAddress, reason);
                else
                    revokeDescendantRefreshTokens(childToken, account, ipAddress, reason);
            }
        }
        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.revoked = DateTime.UtcNow;
            token.reason_revoked = reason;
            token.replaced_by_token = replacedByToken;
        }
        private void sendVerificationEmail(Account account, string origin)
        {
            string message;
            var verifyUrl = $"{origin}/account/verify-email?token={account.verification_token}";
            var htmlBody = "Please confirm you account";
            message = htmlBody;

            _emailService.SendEmail(
                "BTMS",
                "info@btms.com",
                account.email.Split(","),
                null, null,
                "BTMS - Please confirm your email address",
                $@"{message}");
        }
        private void sendAlreadyRegisteredEmail(string email, string origin)
        {
            string message;
            var htmlBody = "This email is already registered";
            message = htmlBody;

            _emailService.SendEmail(
                "BTMS",
                "info@btms.com",
                email.Split(","),
                null, null,
                "BTMS - This email is already registered",
                $@"{message}");
        }
        private void sendPasswordResetEmail(Account account, string origin)
        {
            string message;
            var resetUrl = $"{origin}/account/reset-password?token={account.reset_token}";

            var htmlBody = "Reset password...";
            htmlBody = htmlBody.Replace("{{resetUrl}}", resetUrl);
            message = htmlBody;

            _emailService.SendEmail(
                "BTMS",
                "info@btms.com",
                account.email.Split(","),
                null, null,
                "BTMS - Reset password request",
                $@"{message}"
                );
        }

    }
}
