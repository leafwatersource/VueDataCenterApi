using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.ConditionalFormatting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.Controllers;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Model
{
    public class MStatistics
    {
        public class Dashresbean
        {
            public string Resname { get; set; }
            public int Resstate { get; set; }
            public string ResComment { get; set; }
            public MPublic.COrderList Resorderbean { get; set; }
            public decimal ResStopMinutes { get; set; }
            public decimal ResTotalminutes { get; set; }
            public decimal OrderAvl { get; set; }
        }
        public int GetWorkPlanID()
        {
            int workPlanId = 0;
            DataTable table = new DataTable();
            //PMS_WorkPlans
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select WorkPlanID from PMS_WorkPlans where Status = 'Released' and sysID='" + PMUser.UserSysID + "' order by planReleaseTime";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            workPlanId = Convert.ToInt32(table.Rows[table.Rows.Count - 1][0]);
            return workPlanId;
        }
        public List<Dashresbean> GetDashResList(int empid)
        {
            //获取设备列表
            SqlCommand cmd = PmConnections.ModCmd();
            int usersysid = MPublic.GetUserSysID(empid);
            cmd.CommandText = "SELECT resourceName FROM objResource WHERE sysid = '" + usersysid + "' and workType = '0' and capacity > '0' ORDER BY resourceName";
            DataTable dtresname = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dtresname);
            da.Dispose();
            cmd.Connection.Close();

            //获取锁定设备列表
            cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT * FROM wapResLockState";
            DataTable dtreslock = new DataTable();
            da = new SqlDataAdapter(cmd);
            da.Fill(dtreslock);
            da.Dispose();

            //获取设备日历
            cmd.CommandText = "SELECT ResourceName,ResCalShift,ResCalStartTime,ResCalEndTime FROM  wapResCalendar WHERE ResCalDate = '" + DateTime.Now.Date + "'";
            DataTable dtrescal = new DataTable();
            da = new SqlDataAdapter(cmd);
            da.Fill(dtrescal);
            da.Dispose();

            //获取设备事件
            cmd.CommandText = "SELECT * FROM wapMesEventRec WHERE MesDate = '" + DateTime.Now.Date + "' ORDER BY PMUID DESC";
            DataTable dtmesrec = new DataTable();
            da = new SqlDataAdapter(cmd);
            da.Fill(dtmesrec);
            da.Dispose();
            cmd.Connection.Close();

            //循环设备列表获取设备状态
            List<Dashresbean> dashresbeans = new List<Dashresbean>();
            foreach (DataRow item in dtresname.Rows)
            {
                Dashresbean resbean = new Dashresbean
                {
                    Resname = item[0].ToString()
                };
                //查询设备是否异常
                DataRow[] drres = dtreslock.Select("ResName = '" + resbean.Resname + "' and ResEventType = 'S'");
                if (drres.Count() > 0)
                {
                    resbean.Resstate = 3;
                    resbean.ResComment = drres[0]["ResEventComment"].ToString();
                    resbean.Resorderbean = null;
                }
                else
                {
                    //查看这个设备今天事件是否存在
                    DataRow[] drresevent = dtmesrec.Select("ResName = '" + resbean.Resname + "'");
                    if (drresevent.Count() > 0)
                    {
                        string eventtype = drresevent[0]["EventType"].ToString();
                        if (eventtype.Equals("S"))
                        {
                            resbean.Resstate = 1;
                            resbean.ResComment = "换线中";
                            resbean.Resorderbean = MPublic.GetOrderBeanByID(drresevent[0]["ORDERUID"].ToString());
                        }
                        else if (eventtype.Equals("E") || eventtype.Equals("R") | eventtype.Equals("D") || eventtype.Equals("P"))
                        {
                            resbean.Resstate = 1;
                            resbean.ResComment = "生产中";
                            resbean.Resorderbean = MPublic.GetOrderBeanByID(drresevent[0]["ORDERUID"].ToString());
                        }
                        else
                        {
                            resbean.Resstate = 2;
                            resbean.ResComment = "空闲中";
                            resbean.Resorderbean = null;
                        }
                    }
                    else
                    {
                        resbean.Resstate = 2;
                        resbean.ResComment = "空闲中";
                        resbean.Resorderbean = null;
                    }
                }
                if (resbean.Resorderbean != null)
                {
                    //如果不是空值,就得查询设备利用率
                    //先获取设备的开始工作时间
                    DataRow[] drse = dtrescal.Select("ResourceName = '" + resbean.Resname + "' and ResCalStartTime <= '" + DateTime.Now + "' and ResCalEndTime >= '" + DateTime.Now + "'");
                    if (drse.Count() == 1)
                    {
                        DateTime resstarttime = Convert.ToDateTime(drse[0]["ResCalStartTime"]);
                        //设备目前总工时
                        resbean.ResTotalminutes = Convert.ToDecimal((DateTime.Now - resstarttime).TotalMinutes);
                        ////查询到现在这个时间应该做到哪个工单,
                        //cmd = PmConnections.SchCmd();
                        //cmd.CommandText = "SELECT * FROM User_MesDailyData WHERE dailyDate = '" + DateTime.Now.Date + "' and pmResName = '" + resbean.Resname + "' and TaskFinishState != 5 and planStartTime >= '" + resstarttime + "' and  planEndTime >= '" + DateTime.Now + "' order by planEndTime";
                        //da = new SqlDataAdapter(cmd);
                        //DataTable dtplan = new DataTable();
                        //da.Fill(dtplan);
                        //cmd.Connection.Close();
                        //查询设备有效工时
                        DataView dvresrec = dtmesrec.DefaultView;
                        dvresrec.Sort = "workid,eventtime ASC";
                        DataTable dtresrec = dvresrec.ToTable();

                        //定义异常停机变量
                        DateTime resStopStartTime, resStopEndTime;
                        decimal resStopMinutes;

                        //定义切换时间变量
                        DateTime orderChangeStartTime, orderChangeEndTime;
                        decimal orderChangeMinutes;

                        //定义生产工时变量
                        DateTime orderOutputStartTime, orderOutputEndTime;
                        decimal orderOutputMinutes;
                    }

                }
                dashresbeans.Add(resbean);
            }
            return dashresbeans;
        }
        public DataTable GetResData(string resName, string timeType)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select resname,timeType,fromDay,toDay,resNeedHour,resWorkHour,hourRatio from stsResWorkHour where workPlanID = '" + GetWorkPlanID() + "' and timeType = '" + timeType + "' and resname = '" + resName + "'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }
        public DataTable GetResGroup()
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select ViewID,ViewName from PMS_Views where VGlobal != 'system' and sysID = '" + PMUser.UserSysID + "'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }

        public DataTable GetResList(string viewID)
        {
            //查询设备组下的所有的设备
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select resName from PMS_Views_ResList where ViewID='" + viewID + "'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }
        public DataTable GetResDetail(string resName)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select resName,fromDay,toDay,resNeedHour,resWorkHour,hourRatio,pmUID from stsResWorkHour where  workPlanID = '" + GetWorkPlanID() + "' and timeType = 'W' and resName='" + resName + "'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }
        public DataTable GetResGroupTable(string resListstr)
        {
            DataTable table = new DataTable();
            if (!string.IsNullOrEmpty(resListstr))
            {
                string columns = "";
                string[] reslist = resListstr.Split(",");
                foreach (string item in reslist)
                {
                    string aaa = item;
                    if (string.IsNullOrEmpty(columns))
                    {
                        columns += "'" + item + "'";
                    }
                    else
                    {
                        columns += ",'" + item + "'";
                    }
                }
                SqlCommand cmd = PmConnections.SchCmd();
                cmd.CommandText = "select resName,fromDay,toDay,resNeedHour,resWorkHour,hourRatio,pmUID from stsResWorkHour  where  workPlanID = '" + GetWorkPlanID() + "' and timeType = 'W' and resName in(" + columns + ") order by resName";
                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                ad.Fill(table);
                ad.Dispose();
                cmd.Connection.Close();
            }
            return table;
        }
        /// <summary>
        /// 设备负载率
        /// </summary>
        /// <param name="curRes">设备名称</param>
        /// <returns></returns>
        public decimal GetCurResProduct(string curRes)
        {
            //planTable
            DataTable planTable = new DataTable();
            TimeSpan planHour;
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select ResCalStartTime,ResCalEndTime from wapResCalendar where ResourceName = '" + curRes + "' and '" + DateTime.Now + "' >= ResCalStartTime and '" + DateTime.Now + "' < ResCalEndTime";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(planTable);
            ad.Dispose();
            planHour = DateTime.Now - Convert.ToDateTime(planTable.Rows[0][0]);
            DataTable MesTable = new DataTable();
            cmd.CommandText = "select EventType,EventTime from wapMesEventRec where ResName = '" + curRes + "' and MesDate = '" + DateTime.Now.AddDays(0).ToShortDateString()+"'";
            SqlDataAdapter MesAd = new SqlDataAdapter(cmd);
            MesAd.Fill(MesTable);

            cmd.Connection.Close();
            TimeSpan MesHour = new TimeSpan(0);
            for (int i = 0; i < MesTable.Rows.Count; i++)
            {
                if (i!= MesTable.Rows.Count-1)
                {
                    string CurEventType = MesTable.Rows[i][0].ToString();
                    string nextEventType = MesTable.Rows[i + 1][0].ToString();
                    for (int j = 0; j < AppSetting.MapResTable.Rows.Count; j++)
                    {
                        if (AppSetting.MapResTable.Rows[j][0].ToString() == CurEventType && AppSetting.MapResTable.Rows[j][1].ToString() == nextEventType)
                        {
                            //有效工时
                            if (Convert.ToBoolean(AppSetting.MapResTable.Rows[j][2]))
                            {
                                if (MesHour == new TimeSpan(0))
                                {
                                    MesHour = Convert.ToDateTime(MesTable.Rows[i + 1][1]) - Convert.ToDateTime(MesTable.Rows[i][1]);
                                }
                                else
                                {
                                    MesHour += Convert.ToDateTime(MesTable.Rows[i + 1][1]) - Convert.ToDateTime(MesTable.Rows[i][1]);
                                }
                            }
                        }
                    }
                }
            }
            return decimal.Round(Convert.ToDecimal(MesHour / planHour) * 100, 2);
        }
        /// <summary>
        /// 获取设备达成率
        /// </summary>
        /// <param name="ResName">设备名称</param>
        /// <param name="filterStartTime">筛选条件</param>
        /// <returns></returns>
        public decimal ProductFinish(string ResName, string filterStartTime) {
            decimal ResFinishNum = 0;
            DataTable MesTable = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select * from wapMesEventRec where ResName='" + ResName + "' and MesDate='" + DateTime.Now.ToShortDateString() + "' and DayShift='1' order by EventTime";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(MesTable);
            ad.Dispose();
            double MesHour = 0;
            decimal FinishNum = 0;
            for (int i = 0; i < MesTable.Rows.Count; i++)
            {
                FinishNum += Convert.ToDecimal(MesTable.Rows[i]["FinishedQty"]);
                if (i != MesTable.Rows.Count - 1)
                {
                    DataRow[] row = AppSetting.MapResTable.Select("curEventType='" + MesTable.Rows[i]["EventType"] + "' and nextEventType='" + MesTable.Rows[i + 1]["EventType"] + "'");
                    if (Convert.ToBoolean(row[0]["effective"]))
                    {
                        DateTime end = Convert.ToDateTime(MesTable.Rows[i + 1]["EventTime"]);
                        DateTime start = Convert.ToDateTime(MesTable.Rows[i]["EventTime"]);
                        MesHour = (end - start).TotalSeconds;
                    }
                }
            }
            decimal Mesnum;
            if (FinishNum == 0 || MesHour == 0)
            {
                Mesnum = 0;
            }
            else
            {
                Mesnum = FinishNum / Convert.ToDecimal(MesHour);
            }
            DataTable PlanTable = new DataTable();
            cmd.CommandText = "Select * from User_MesDailyData where dailyDate='" + DateTime.Now.ToShortDateString() + "' and pmResName='" + ResName + "' and dayShift='1'";
            SqlDataAdapter PlanAd = new SqlDataAdapter(cmd);
            PlanAd.Fill(PlanTable);
            PlanAd.Dispose();
            decimal planQty = 0;
            decimal planHour = 0;
            foreach (DataRow row in PlanTable.Rows)
            {
                planQty += Convert.ToDecimal(row["finishedQty"]);
                planHour += Convert.ToDecimal(row["workHour"]) * 3600;
            }
            decimal PlanNum = 0;
            if (planQty == 0|| planHour==0)
            {
                PlanNum = 0;
            }
            else
            {
                PlanNum = planQty / planHour;
            }
            if (Mesnum==0 || PlanNum==0)
            {
                ResFinishNum = 0;
            }
            else
            {
                ResFinishNum = decimal.Round(Mesnum / PlanNum * 100, 2);
            }
            return ResFinishNum;
        }
        public JObject Dayshift(string resName,string startTime,string endTime)
        {
            //默认时间是当前的日期
            string now = null;
            string time = null;
            if (string.IsNullOrEmpty(startTime))
            {
                now = DateTime.Now.ToShortDateString();
                time = DateTime.Now.ToShortTimeString();
            }
            else
            {
                now = Convert.ToDateTime(startTime).ToShortDateString();
                time = Convert.ToDateTime(startTime).ToShortTimeString();
            }
            int curdayshift = 0;
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select CONVERT(varchar(100),ResCalStartTime,120) as ResCalStartTime,CONVERT(varchar(100),ResCalEndTime,120) as ResCalEndTime, ResCalShift from wapResCalendar where ResourceName='" + resName+ "' and ResCalDate='"+ now+"'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            cmd.Connection.Close();
            foreach (DataRow row in table.Rows)
            {
                if (Convert.ToDateTime(row["ResCalStartTime"]) <= DateTime.Now && Convert.ToDateTime(row["ResCalEndTime"]) >=  DateTime.Now)
                {
                    curdayshift = Convert.ToInt32(row["ResCalShift"]);
                }
            }
            JObject data = new JObject
            {
                { "curdayshift", curdayshift},
                { "dayShift",JsonConvert.SerializeObject(table)}
             };
            return data;
        }
    }
}