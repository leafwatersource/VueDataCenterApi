using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiNew.Model;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class History : ControllerBase
    {
        /// <summary>
        /// 分页展示用户的操作记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Resul([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string filter)
        {
            MHistory mHistory = new MHistory();
            return Ok(mHistory.GetUserLog(PageSize, CurPage, filter).ToString());
        }
    }
}
