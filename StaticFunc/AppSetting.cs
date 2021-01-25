using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiNew.StaticFunc
{
    public class AppSetting
    {
        public static JObject TableFileds { get; set; }//订单查询、执行计划、事件记录的表格所展示的列表
        public static string BasePath { get; set; }//根路径
        public static DataTable MapResTable { get; set; }//是否是有效工时的匹配表
        public static JObject StatisticsHover { get; set; }//前端设备使用状态页面的鼠标hover所展示的字段
        public static JObject ResStatistic { get; set; }//产能分析的表格的列
        public static JObject FilterFileds { get; set; }//筛选字段的定义
        public static bool ChangePlan { get; set; }//前端换模计划的按钮是否是显示的
    }
}