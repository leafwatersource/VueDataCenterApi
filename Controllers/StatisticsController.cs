using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiNew.Model;
using static WebApiNew.Model.MStatistics;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class GetResStatusController : ControllerBase
    {
        public IActionResult Result([FromForm]int empid)
        {
            MStatistics mStatistics = new MStatistics();
            //List<Dashresbean> list = mStatistics.GetDashResList(empid);
            return Ok(mStatistics.GetDashResList(empid));
        }
    }
}
