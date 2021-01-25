using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.DataValidation;
using WebApiNew.Model;
using static WebApiNew.Model.MStatistics;

namespace WebApiNew.Controllers
{ 
    [Route("/[controller]")]
    [ApiController]
    public class GetResStatusController : ControllerBase
    {
        public IActionResult Result([FromForm] int empid)
        {
            MStatistics mStatistics = new MStatistics();
            return Ok(mStatistics.GetDashResList(empid));
        }
    }

    [Route("/[controller]")]
    [ApiController]
    public class GetResStatistics : ControllerBase
    {
        public IActionResult Result([FromForm] string resName, [FromForm] string timeType)
        {
            MStatistics mStatistics = new MStatistics();
            JObject data = new JObject {
                { "resData", JsonConvert.SerializeObject(mStatistics.GetResData(resName,timeType)) },
            };
            return Ok(data.ToString());
        }

    }
    [Route("/[controller]")]
    [ApiController]
    public class GetResGroup : ControllerBase
    {
        /// <summary>
        /// 查询设备组
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result()
        {
            MStatistics mStatistics = new MStatistics();
            JObject data = new JObject{
                { "resGroup", JsonConvert.SerializeObject(mStatistics.GetResGroup())} };
            return Ok(data.ToString());
        }
    }
}
[Route("/[controller]")]
[ApiController]
public class GetResList : ControllerBase
{
    /// <summary>
    /// 查询设备组下的所有设备
    /// </summary>
    /// <param name="viewId">设备组的id</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Result([FromForm] string viewId)
    {
        MStatistics mStatistics = new MStatistics();
        JObject data = new JObject {
                { "resList", JsonConvert.SerializeObject(mStatistics.GetResList(viewId)) },
            };
        return Ok(data.ToString());
    }
}

[Route("/[controller]")]
[ApiController]
public class GetResDetail : ControllerBase
{
    //查询设备的统计信息
    public IActionResult Result([FromForm]string resName) {
        MStatistics mStatistics = new MStatistics();
        JObject data = new JObject {
                { "resData", JsonConvert.SerializeObject(mStatistics.GetResDetail(resName)) },
            };
        return Ok(data.ToString());
    }
}


[Route("/[controller]")]
[ApiController]
public class GetResGroupTable : ControllerBase
{
    public IActionResult Result([FromForm] string resList)
    {
        MStatistics mStatistics = new MStatistics();
        JObject data = new JObject {
                { "resGroupData", JsonConvert.SerializeObject(mStatistics.GetResGroupTable(resList)) },
            };
        return Ok(data.ToString());
    }
}


[Route("/[controller]")]
[ApiController]
public class GetCurResProduct:ControllerBase
{
    [HttpPost]
    public IActionResult Result([FromForm] string CurResName)
    {
        MStatistics mStatistics = new MStatistics();
        decimal num =  mStatistics.GetCurResProduct(CurResName);
        return Ok(num.ToString());
    }
}
[Route("/[controller]")]
[ApiController]
public class GetProductFinish:ControllerBase
{
    /// <summary>
    /// 设备达成率
    /// </summary>
    /// <param name="ResName">设备名称</param>
    /// <param name="filterStartTime">搜索条件</param>
    /// <returns></returns>
    public IActionResult Result([FromForm]string ResName,[FromForm]string filterStartTime)
    {
        MStatistics mStatistics = new MStatistics();
        return Ok(mStatistics.ProductFinish(ResName, filterStartTime).ToString());
    } 
}
[Route("/[controller]")]
[ApiController]
public class GetDataShift:ControllerBase
{
    /// <summary>
    /// 设备状态页面查询设备的班次
    /// </summary>
    /// <param name="ResName"></param>
    /// <returns></returns>
    public IActionResult Result([FromForm]string ResName,[FromForm]string startTime,[FromForm]string endTime)
    {
        MStatistics mStatistics = new MStatistics();
        return Ok(mStatistics.Dayshift(ResName,startTime,endTime).ToString());
    }
}
[Route("/[controller]")]
[ApiController]
public class GetResEventData : ControllerBase
{
    /// <summary>
    /// 设备状态页面查询点击的设备的事件操作记录
    /// </summary>
    /// <param name="ResName"></param>
    /// <returns></returns>
    public IActionResult Result([FromForm]string ResName)
    {
        MStatistics mStatistics = new MStatistics();
        DataTable table = mStatistics.ResEvent(ResName);
        return Ok("hello");
    }
}