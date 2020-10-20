using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiNew.Model;

namespace WebApiNew.Controllers
{
    /// <summary>
    /// 获取生产订单的设备组
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class ViewGroup : ControllerBase
    {
        [HttpPost]
        public IActionResult Result()
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetViewGroup());
        }
    }
    /// <summary>
    /// 设备组下的所有的设备
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class ResView : ControllerBase
    {
        [HttpPost]
        public IActionResult Result([FromForm]string resGroup,[FromForm]string resName)
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetResView(resGroup, resName).ToString());
        }
    }
    /// <summary>
    /// 获取设备下的所有的工单
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class ResWorkView : ControllerBase
    {
        [HttpPost]
        public IActionResult Result([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string Resource, [FromForm] string GroupName)
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetResPlan(PageSize,CurPage,Resource, GroupName));
        }
    }
}
