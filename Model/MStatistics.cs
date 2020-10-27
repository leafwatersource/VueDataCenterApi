using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.StaticFunc;

namespace WebApiNew.Model
{
    public class MStatistics
    {
        public class Dashresbean
        {
            public string Resname { get; set; }
            public string Resstate { get; set; }
            public string Resorderstate { get; set; }
            public int ResType { get; set; }
            public string Description { get; set; }
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
            cmd.CommandText = "SELECT * FROM wapResLockState ";
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
                //查看这个设备今天事件是否存在
                DataRow[] drresevent = dtmesrec.Select("ResName = '" + resbean.Resname + "'");
                if (drresevent.Count() > 0)
                {
                   
                    string eventtype = drresevent[0]["EventType"].ToString();
                    string desc = drresevent[0]["Description"].ToString();
                    if (eventtype.Equals("S"))
                    {
                        resbean.Resstate = "换线中";
                        resbean.Resorderstate = MPublic.GetOrderBeanByID(drresevent[0]["ORDERUID"].ToString()).WorkID;
                        resbean.ResType = 1;
                    }
                    else if (eventtype.Equals("E") || eventtype.Equals("R") | eventtype.Equals("D") || eventtype.Equals("P"))
                    {
                        resbean.Resstate = "生产中";
                        resbean.Resorderstate = MPublic.GetOrderBeanByID(drresevent[0]["ORDERUID"].ToString()).WorkID;
                        resbean.ResType = 2;
                    }
                    else
                    {
                        resbean.Resstate = "空闲中";
                        resbean.Resorderstate = string.Empty;
                        resbean.ResType = 0;
                    }
                    resbean.Description = desc;
                }
                else
                {
                    //查询设备是否异常
                    DataRow[] drres = dtreslock.Select("ResName = '" + resbean.Resname + "' and ResEventType = 'S'");
                    if (drres.Count() > 0)
                    {
                        resbean.Resstate = drres[0]["ResEventComment"].ToString();
                        resbean.Resorderstate = string.Empty;
                        resbean.ResType = 3;
                    }
                    else
                    {
                        resbean.Resstate = "空闲中";
                        resbean.Resorderstate = string.Empty;
                        resbean.ResType = 0;
                    }
                }
                dashresbeans.Add(resbean);
            }
            return dashresbeans;
        }
    }
}
