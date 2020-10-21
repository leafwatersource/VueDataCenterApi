using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.StaticFunc;

namespace WebApiNew.Model
{
    public class MPublic
    {
        public class COrderList
        {
            public string MesResName { get; internal set; }
            public string MesOpName { get; internal set; }
            public string MesOperator { get; internal set; }
            public string WorkID { get; internal set; }
            public object PlanStartTime { get; internal set; }
            public object Planendtime { get; internal set; }
            public string PmResName { get; internal set; }
            public string PmOpName { get; internal set; }
            public int TaskFinishState { get; internal set; }
            public double Plannedqty { get; internal set; }
            public string ProductID { get; internal set; }
            public double FinishedQty { get; internal set; }
            public double FailedQty { get; internal set; }
            public string ItemAttr2 { get; internal set; }
            public string ItemAttr5 { get; internal set; }
            public double AllFinishedQty { get; internal set; }
            public double JobQty { get; internal set; }
            public string ItemAttr1 { get; internal set; }
            public string ItemAttr4 { get; internal set; }
            public string ItemAttr3 { get; internal set; }
            public string ItemAttr6 { get; internal set; }
            public string ItemAttr7 { get; internal set; }
            public string ItemAttr9 { get; internal set; }
            public string ItemAttr8 { get; internal set; }
            public string ItemAttr10 { get; internal set; }
            public string JobAttr2 { get; internal set; }
            public string JobAttr1 { get; internal set; }
            public double WorkHours { get; internal set; }
            public int DayShift { get; internal set; }
            public string ItemDesp { get; internal set; }
            public string ParentGUID { get; internal set; }
            public decimal PrepTime { get; internal set; }
            public string OrderPauseComment { get; internal set; }
            public bool IsDownOrder { get; internal set; }
            public DateTime MesStartTime { get; internal set; }
            public double UnitPrice { get; internal set; }
            public object JobDemandDay { get; internal set; }
            public DateTime MesSetupStartTime { get; internal set; }
            public double BomComused { get; internal set; }
            public string UserComment { get; internal set; }
            public DateTime MesEndTime { get; internal set; }
            public DateTime MesSetupEndTime { get; internal set; }
            public double ScrappedQty { get; internal set; }
            public string ChangeResName { get; internal set; }
            public int OrderUID { get; internal set; }
            public double SetupTime { get; internal set; }
            public DateTime ReportTime { get; internal set; }
            public bool CanReport { get; internal set; }
            public bool Ajustment { get; internal set; }
            public double CanReportQty { get; internal set; }
        }
        public static COrderList GetOrderBeanByID(string orderUID)
        {
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT * FROM User_MesDailyData WHERE UID = '" + orderUID + "'";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable temptable = new DataTable();
            da.Fill(temptable);
            da.Dispose();
            cmd.Connection.Close();
            if (temptable.Rows.Count == 1)
            {
                DataRow checkeddata = CheckEmptyVal(temptable.Clone(), temptable.Rows[0]);
                COrderList li = new COrderList
                {
                    MesResName = checkeddata["MesResName"].ToString(),
                    MesOpName = checkeddata["MesOpName"].ToString(),
                    MesOperator = checkeddata["bgPerson"].ToString(),
                    WorkID = checkeddata["WorkID"].ToString(),
                    PlanStartTime = ForMatDateTimeStr(Convert.ToDateTime(checkeddata["PlanStartTime"]), 1),
                    Planendtime = ForMatDateTimeStr(Convert.ToDateTime(checkeddata["Planendtime"]), 1),
                    PmResName = checkeddata["PmResName"].ToString(),
                    PmOpName = checkeddata["PmOpName"].ToString(),
                    ProductID = checkeddata["ProductID"].ToString(),
                    TaskFinishState = Convert.ToInt32(checkeddata["TaskFinishState"]),
                    FinishedQty = Convert.ToDouble(checkeddata["FinishedQty"]),
                    Plannedqty = Convert.ToDouble(checkeddata["Plannedqty"]),
                    FailedQty = Convert.ToDouble(checkeddata["FailQty"]),
                    AllFinishedQty = Convert.ToDouble(checkeddata["AllFinishedQty"]),
                    JobQty = Convert.ToDouble(checkeddata["JobQty"]),
                    ItemAttr1 = checkeddata["ItemAttr1"].ToString(),
                    ItemAttr2 = checkeddata["ItemAttr2"].ToString(),
                    ItemAttr3 = checkeddata["ItemAttr3"].ToString(),
                    ItemAttr4 = checkeddata["ItemAttr4"].ToString(),
                    ItemAttr5 = checkeddata["ItemAttr5"].ToString(),
                    ItemAttr6 = checkeddata["ItemAttr6"].ToString(),
                    ItemAttr7 = checkeddata["ItemAttr7"].ToString(),
                    ItemAttr8 = checkeddata["ItemAttr8"].ToString(),
                    ItemAttr9 = checkeddata["ItemAttr9"].ToString(),
                    ItemAttr10 = checkeddata["ItemAttr10"].ToString(),
                    JobAttr1 = checkeddata["JobAttr1"].ToString(),
                    JobAttr2 = checkeddata["JobAttr2"].ToString(),
                    DayShift = Convert.ToInt32(checkeddata["DayShift"]),
                    ItemDesp = checkeddata["itemDesp"].ToString(),
                    WorkHours = Convert.ToDouble(checkeddata["workHour"]),
                    SetupTime = Convert.ToDouble(checkeddata["setupTime"]),
                    OrderUID = Convert.ToInt32(checkeddata["UID"]),
                    BomComused = Convert.ToDouble(checkeddata["AllJobTaskqty"]) / Convert.ToDouble(checkeddata["JobQty"]),
                    CanReport = true,
                    CanReportQty = Convert.ToDouble(checkeddata["AllJobTaskqty"]) - Convert.ToDouble(checkeddata["AllFinishedQty"]) - Convert.ToDouble(checkeddata["FailQty"]),
                    ChangeResName = string.Empty,
                    ReportTime = Convert.ToDateTime(checkeddata["updateDatetime"]),
                    JobDemandDay = ForMatDateTimeStr(Convert.ToDateTime(checkeddata["jobDemandDay"]), 3),
                    ScrappedQty = Convert.ToDouble(checkeddata["ScrappedQty"]),
                    Ajustment = Convert.ToBoolean(checkeddata["adjustment"]),
                    UnitPrice = Convert.ToDouble(checkeddata["unitPrice"]),
                    MesSetupStartTime = Convert.ToDateTime(checkeddata["setupStartTime"]),
                    MesSetupEndTime = Convert.ToDateTime(checkeddata["setupEndTime"]),
                    MesStartTime = Convert.ToDateTime(checkeddata["startDateTime"]),
                    MesEndTime = Convert.ToDateTime(checkeddata["endDateTime"]),
                    UserComment = checkeddata["userComment"].ToString(),
                    IsDownOrder = Convert.ToBoolean(checkeddata["isMesDown"]),
                    OrderPauseComment = checkeddata["orderPauseComment"].ToString(),
                    PrepTime = Convert.ToDecimal(checkeddata["prepTime"]),
                    ParentGUID = checkeddata["parentGUID"].ToString()
                };
                return li;
            }
            else
            {
                return null;
            }
        }

        private static object ForMatDateTimeStr(DateTime time, int timetype)
        {
            string returndata;
            string monthstr, daystr, hourstr, minutestr, secondstr;
            monthstr = time.Month.ToString();
            if (monthstr.Length < 2)
            {
                monthstr = "0" + monthstr;
            }
            daystr = time.Day.ToString();
            if (daystr.Length < 2)
            {
                daystr = "0" + daystr;
            }
            hourstr = time.Hour.ToString();
            if (hourstr.Length < 2)
            {
                hourstr = "0" + hourstr;
            }
            minutestr = time.Minute.ToString();
            if (minutestr.Length < 2)
            {
                minutestr = "0" + minutestr;
            }
            secondstr = time.Second.ToString();
            if (secondstr.Length < 2)
            {
                secondstr = "0" + secondstr;
            }
            if (timetype == 0)
            {
                returndata = time.Year + "/" + monthstr + "/" + daystr + " " + hourstr + ":" + minutestr + ":" + secondstr;
            }
            else if (timetype == 1)
            {
                returndata = monthstr + "/" + daystr + " " + hourstr + ":" + minutestr + ":" + secondstr;
            }
            else if (timetype == 2)
            {

                returndata = monthstr + "/" + daystr + " " + hourstr + ":" + minutestr;
            }
            else if (timetype == 3)
            {
                returndata = time.Year + "/" + monthstr + "/" + daystr;
            }
            else
            {
                returndata = "1900/01/01 00:00:00";
            }
            return returndata;
        }

        private static DataRow CheckEmptyVal(DataTable table, DataRow data)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                string datatype = table.Columns[i].DataType.ToString().ToUpper();
                if (datatype == "SYSTEM.INT32" || datatype == "SYSTEM.INT64")
                {
                    try
                    {
                        Convert.ToInt32(data[i]);
                    }
                    catch (Exception)
                    {

                        data[i] = -1;
                    }
                }
                if (datatype == "SYSTEM.DATETIME")
                {
                    try
                    {
                        Convert.ToDateTime(data[i]);
                    }
                    catch (Exception)
                    {

                        data[i] = Convert.ToDateTime("1900-01-01 00:00:00");
                    }
                }
                if (datatype == "SYSTEM.DECIMAL")
                {
                    try
                    {
                        Convert.ToDecimal(data[i]);
                    }
                    catch (Exception)
                    {

                        data[i] = -0.00;
                    }
                }
                if (datatype == "SYSTEM.BOOLEAN")
                {
                    try
                    {
                        Convert.ToBoolean(data[i]);
                    }
                    catch (Exception)
                    {
                        data[i] = false;
                    }
                }
                if (datatype == "SYSTEM.STRING")
                {
                    try
                    {
                        Convert.ToString(data[i]);
                    }
                    catch (Exception)
                    {
                        data[i] = string.Empty;
                    }
                }
            }
            return data;
        }
        public static int GetUserSysID(int empid)
        {
            SqlCommand cmd = PmConnections.ModCmd();
            cmd.CommandText = "SELECT sysid FROM wapEmpList WHERE empID = '" + empid + "'";
            SqlDataReader rd = cmd.ExecuteReader();
            int usersysid = -1;
            if (rd.Read())
            {
                usersysid = Convert.ToInt32(rd[0]);
            }
            rd.Close();
            cmd.Connection.Close();
            return usersysid;
        }
    }
}
