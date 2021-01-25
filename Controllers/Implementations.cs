using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiNew.Model;
using WebApiNew.StaticFunc;

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
        public IActionResult Result([FromForm] string resGroup, [FromForm] string resName)
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetResView(resGroup, resName).ToString());
        }
    }

    [Route("/[controller]")]
    [ApiController]
    public class ResWorkView : ControllerBase
    {
        /// <summary>
        /// 获取设备下的所有的工单
        /// </summary>
        /// <param name="PageSize">一页多少条数据</param>
        /// <param name="CurPage">第几页的数据</param>
        /// <param name="Resource">设备名称</param>
        /// <param name="GroupName">设备组名称</param>
        /// <param name="ChangeModel">是否是换模</param>
        /// <param name="filter">精确筛选</param>
        /// <param name="fuzzyFilter">模糊筛选的字段</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result([FromForm] string PageSize, [FromForm] string CurPage, [FromForm] string Resource, [FromForm] string GroupName, [FromForm] bool ChangeModel, [FromForm] string filter, [FromForm] string fuzzyFilter, [FromForm] string filterFirstDemandDay)
        {
            MImplementations mImplementations = new MImplementations();
            return Ok(mImplementations.GetResPlan(PageSize, CurPage, Resource, GroupName, ChangeModel, filter, fuzzyFilter, filterFirstDemandDay));
        }
    }

    [Route("/[controller]")]
    [ApiController]
    public class readShowChangePlan : ControllerBase
    {
        /// <summary>
        /// 查看换模计划是否显示 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result() {
            return Ok(AppSetting.ChangePlan);
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class switchValChange:ControllerBase{
        /// <summary>
        /// 修改换模计划按钮的显示
        /// </summary>
        /// <param name="switchVal"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result([FromForm]string switchVal) {
            MuserConfig muserConfig = new MuserConfig();
            muserConfig.changeSwitchVal(switchVal);
            return Ok(switchVal);
        }

}
}
