using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Intercom.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WebApiNew.Model;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ExportExcel : ControllerBase
    {
        [HttpPost]
        public  IActionResult Result()
        {
            return Ok("123");
        }
    }
}
