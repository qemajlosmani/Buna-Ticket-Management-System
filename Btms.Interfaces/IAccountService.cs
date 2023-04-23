using Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btms.Interfaces
{
    public interface IAccountService
    {
        LoginResponse Login(LoginRequest model, string ipAddress);
        void Signup(SignupRequest model, string origin);
        void VerifyEmail(VerifyEmailRequest model);
        void ForgotPassword(ForgotPasswordRequest model, string origin);
        void ResetPassword(ResetPasswordRequest model);
        LoginResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string token, string ipAddress);
        void ValidateResetToken(ValidateResetTokenRequest model);
    }
}
