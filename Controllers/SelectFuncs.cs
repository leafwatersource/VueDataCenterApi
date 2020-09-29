using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class SelectFuncs : ControllerBase
    {
        [HttpPost]
        public IActionResult IsAdmin()
        {
            if (HttpContext.Request.Cookies["MD5"] != null)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }
    }


    [Route("/[controller]")]
    [ApiController]
    public class FunctionList : ControllerBase
    {
        [HttpPost]
        public IActionResult Result()
        {
            return Ok(PMUser.FunctionList);
        }
    }
}
