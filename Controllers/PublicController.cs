using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApiNew.StaticFunc;

namespace WebApiNew.Controllers
{
 
    [Route("/[controller]")]
    [ApiController]
    public class TableFiled : ControllerBase
    {
        /// <summary>
        /// 获取表格的列
        /// </summary>
        /// <param name="tableName">表格的名称（WorkOrder：订单列表，WorkPlan：计划，History：历史数据）</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Result([FromForm] string tableName)
        {
            if (tableName == "WorkOrder")
            {
                return Ok(AppSetting.TableFileds.GetValue("SQLWorkOrderFiled").ToString());
            }
            else if (tableName == "WorkPlan")
            {
                return Ok(AppSetting.TableFileds.GetValue("SQLWorkPlanFiled").ToString());
            }
            else if (tableName == "History")
            {
                return Ok(AppSetting.TableFileds.GetValue("HistoryTableFiled").ToString());
            }
            return Ok("表格的列查询错误");
        }
    }
}
