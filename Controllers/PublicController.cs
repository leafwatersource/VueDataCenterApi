using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApiNew.Model;
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
        /// <param name="tableName">表格的名称（WorkOrder：订单列表，WorkPlan：计划，History：历史数据,StatisticsHover:设备使用状态展示的列,ResStatisticsTable数据分析表格的列）</param>
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
            else if (tableName == "StatisticsHover")
            {
                return Ok(AppSetting.StatisticsHover.ToString());
            }else if (tableName == "ResStatisticsTable") {
                return Ok(AppSetting.ResStatistic.ToString());
            }
            return Ok("表格的列查询错误");
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class SetTableFiled : ControllerBase
    {
        /// <summary>
        /// 修改前端表格展示的列
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="changeData">修改的数据</param>
        /// <param name="targetChange">数据库对应的字段</param>
        /// <returns></returns>
       public IActionResult Result([FromForm]string tableName,[FromForm] string changeData,[FromForm]string targetChange)
        {
            MuserConfig muserConfig = new MuserConfig();
            muserConfig.setUserTable(tableName, changeData,targetChange);
            return Ok("ok");
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class AddTableFiled : ControllerBase
    {
        /// <summary>
        /// 表格添加数据
        /// </summary>
        /// <param name="newData">要添加的数据</param>
        /// <param name="tableName">表格的名称</param>
        /// <returns></returns>
        public IActionResult Result([FromForm]string newData,[FromForm]string tableName) {
            MuserConfig muserConfig = new MuserConfig();
            muserConfig.addTableColumn(newData, tableName);
            return Ok("ok");
        }
    }
    [Route("/[controller]")]
    [ApiController]
    public class DelTableFiled : ControllerBase
    {
        public IActionResult Result([FromForm] string tableName, [FromForm] string delData)
        {
            MuserConfig muserConfig = new MuserConfig();
            muserConfig.delTableColumn(tableName, delData);
            return Ok("ok");
        }
    }
}
