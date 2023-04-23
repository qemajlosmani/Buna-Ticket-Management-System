using Btms.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Btms.API.Controllers
{
    [Controller]
    public class BaseApiController : ControllerBase
    {
        public Account Account => (Account)HttpContext.Items["Account"];

    }
}
