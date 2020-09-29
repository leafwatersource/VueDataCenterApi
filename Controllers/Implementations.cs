using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiNew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Implementations : ControllerBase
    {
        [HttpPost]
        public IActionResult Result([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string filte)
        {
            return Ok("111");
        }
    }
}
