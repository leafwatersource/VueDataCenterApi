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
    public class Implementations : ControllerBase
    {
        [HttpPost]
        public IActionResult Result([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string filte)
        {
            return Ok("111");
        }
    }
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
        public IActionResult Result([FromForm]string resGroup)
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetResView(resGroup).ToString());
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
        public IActionResult Result([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string Resource, [FromForm] string ViewName)
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetResPlan(PageSize,CurPage,Resource, ViewName));
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class AllResView : ControllerBase
    {
        /// <summary>
        /// 获取设备组下的所有设备的工单
        /// </summary>
        /// <param name="ResGroup">设备组的名称</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result([FromForm]string ResGroup)
        {
            return Ok("111");
        }
    }
}
