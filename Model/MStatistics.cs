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
            cmd.CommandText = "select WorkPlanID from PMS_WorkPlans where Status = 'Released' and sysID='"+PMUser.UserSysID+ "' order by planReleaseTime";
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


                        //for (int i = 0; i < dtresrec.Rows.Count; i++)
                        //{
                        //    switch (dtresrec.Rows[i]["EventType"].ToString())
                        //    {
                        //        case "RD":
                        //            if( i == 0)
                        //            {
                        //                //设备有异常
                        //                resStopStartTime = Convert.ToDateTime(dtresrec.Rows[i]["EventTime"]);
                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "RN":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "EW":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "E":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "S":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "D":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "P":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "U":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;

                        //        case "R":
                        //            if (i == 0)
                        //            {

                        //            }
                        //            else
                        //            {

                        //            }
                        //            continue;
                        //    }                            
                        //}

                    }

                }
                dashresbeans.Add(resbean);
            }
            return dashresbeans;
        }
        public DataTable GetResData(string resName,string timeType)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select resname,timeType,fromDay,toDay,resNeedHour,resWorkHour,hourRatio from stsResWorkHour where workPlanID = '"+GetWorkPlanID()+"' and timeType = '" + timeType + "' and resname = '"+resName+"'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }
        public DataTable GetResGroup() {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select ViewID,ViewName from PMS_Views where VGlobal != 'system' and sysID = '"+ PMUser.UserSysID + "'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }

        public DataTable GetResList(string viewID) {
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
            cmd.CommandText = "select resName,fromDay,toDay,resNeedHour,resWorkHour,hourRatio,pmUID from stsResWorkHour where  workPlanID = '"+GetWorkPlanID()+"' and timeType = 'W' and resName='" + resName + "'";
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
                        columns += "'"+item+"'";
                    }
                    else
                    {
                        columns += ",'" + item + "'";
                    }
                }
                SqlCommand cmd = PmConnections.SchCmd();
                cmd.CommandText = "select resName,fromDay,toDay,resNeedHour,resWorkHour,hourRatio,pmUID from stsResWorkHour  where  workPlanID = '"+GetWorkPlanID()+"' and timeType = 'W' and resName in(" + columns + ") order by resName";
                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                ad.Fill(table);
                ad.Dispose();
                cmd.Connection.Close();
            }
            return table;
        }
    }
}
