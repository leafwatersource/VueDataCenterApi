using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApiNew.Model;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class DataCenterController : ControllerBase
    {
        /// <summary>
        /// 查询订单列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result([FromForm]string PageSize,[FromForm]string CurPage,[FromForm]string filter)
        {
            MdataCenter mdataCenter = new MdataCenter();
            DataTable dt = mdataCenter.WorkOrderData(PageSize,CurPage,filter);
            int total = mdataCenter.GetOrderCount();
            JObject data = new JObject {
                { "code", "0"},
                { "data", JsonConvert.SerializeObject(dt)},
                {"total", total },
                { "msg", "successful"}
            };
            return Ok(data.ToString());
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class PlanMessage:ControllerBase
    {
        /// <summary>
        /// 获取的是工单的描述
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result()
        {
            MdataCenter mdataCenter = new MdataCenter();
            mdataCenter.GetWorkPlanMessage(string.Empty);
            JObject result = new JObject();
            result.Add("Owner", mdataCenter.Owner);
            result.Add("WorkPlanName", mdataCenter.WorkPlanName);
            result.Add("WorkPlanId", mdataCenter.WorkPlanId);
            result.Add("ReleaseTime", mdataCenter.ReleaseTime);
            return Ok(result.ToString());
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class GetPercentage : ControllerBase
    {
     /// <summary>
    /// 数据中心的百分比，和四个数字值
    /// </summary>
    /// <returns></returns>
        [HttpPost]
        public IActionResult Result()
        {
            MdataCenter mdataCenter = new MdataCenter();
            mdataCenter.Datacenterorderdelay();
            JObject result = new JObject();
            result.Add("ErrorCount", mdataCenter.ErrorCount);
            result.Add("EarlyConunt", mdataCenter.EarlyConunt);
            result.Add("LateCount", mdataCenter.LateCount);
            result.Add("OnTimeCount", mdataCenter.OnTimeCount);
            result.Add("ErrorPercentage", mdataCenter.ErrorPercentage);
            result.Add("EarlyPercentage", mdataCenter.EarlyPercentage);
            result.Add("OnTimePercentage", mdataCenter.OnTimePercentage);
            result.Add("LatePercentage", mdataCenter.LatePercentage);
            return Ok(result.ToString());
        }
    }
}
