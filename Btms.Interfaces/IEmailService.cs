using Btms.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Btms.Interfaces
{
    public interface IEmailService
    {
        GeneralApiResponse SendEmail(string from, string from_email, string[] toEmail, string[]? toCC, string[]? toBCC, string? subject, string? text, List<IFormFile>? attachment = null);
    }
}
