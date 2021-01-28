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
        /// <param name="PageSize">一页多少数据</param>
        /// <param name="CurPage">当前为多少页</param>
        /// <param name="filter">精确筛选</param>
        /// <param name="fuzzyFilter">模糊筛选</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Resul([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string filter,[FromForm]string fuzzyFilter)
        {
            MHistory mHistory = new MHistory();
            return Ok(mHistory.GetUserLog(PageSize, CurPage, filter, fuzzyFilter).ToString());
        }
    }
}
