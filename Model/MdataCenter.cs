using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Model
{
    public class MdataCenter
    {
        public  string Owner { get; set; }
        public  string WorkPlanName { get; set; }
        public  string WorkPlanId { get; set; }
        public  string ReleaseTime { get; set; }
        public  string PMOcState { get; set; }
        public int ErrorCount { get; set; }
        public int EarlyConunt { get; set; }
        public int LateCount { get; set; }
        public int OnTimeCount { get; set; }
        public int ErrorPercentage { get; set; }
        public int EarlyPercentage { get; set; }
        public int OnTimePercentage { get; set; }
        public int LatePercentage { get; set; }
        public void GetWorkPlanMessage(string colName)
        {
            SqlCommand cmd = PmConnections.SchCmd();
            if (string.IsNullOrEmpty(colName))
            {
                cmd.CommandText = "select * from PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMPlState + "'";
            }
            else
            {
                cmd.CommandText = "select " + colName + " from PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMPlState + "'";
            }
            //cmd.CommandText = "select Owner,WorkPlanId,WorkPlanName from PMS_WorkPlans where sysID = '" + AppSettings.PMSysid + "' and Status = '" + AppSettings.PMPlState + "'";
            SqlDataReader rd = cmd.ExecuteReader();
            rd.Read();
            Owner = rd["Owner"].ToString();
            WorkPlanName = rd["WorkPlanName"].ToString();
            WorkPlanId = rd["WorkPlanId"].ToString();
            DateTime tmp = DateTime.Now;
            try
            {
                Convert.ToDateTime(rd["planReleaseTime"].ToString());
            }
            catch (Exception)
            {
            }
            ReleaseTime = tmp.Year.ToString() + "/" + tmp.Month.ToString() + "/" + tmp.Day.ToString() + " " + tmp.Hour.ToString() + ":" + tmp.Minute.ToString();
            rd.Close();
            cmd.Connection.Dispose();
        }
        public void Datacenterorderdelay()
        {
            DataTable table = GetWorkOrder("delayDays", "isScheduleWorkID = '1'", string.Empty);
            foreach (DataRow item in table.Rows)
            {
                if (item[0].ToString() == "")
                {
                    //异常的数量
                    ErrorCount++;
                }
                else
                {
                    if (Convert.ToInt32(item[0]) < 0)
                    {
                        EarlyConunt++;
                    }
                    else if (Convert.ToInt32(item[0]) > 0)
                    {
                        LateCount++;
                    }
                    else
                    {
                        OnTimeCount++;
                    }
                }
            }
            //计算四个值的百分比，返回给前端页面显示
            ErrorPercentage = Convert.ToInt32((Convert.ToDouble(ErrorCount) / Convert.ToDouble(table.Rows.Count)) * 100);
            EarlyPercentage = Convert.ToInt32((Convert.ToDouble(EarlyConunt) / Convert.ToDouble(table.Rows.Count)) * 100);
            OnTimePercentage = Convert.ToInt32((Convert.ToDouble(OnTimeCount) / Convert.ToDouble(table.Rows.Count)) * 100);
            LatePercentage = Convert.ToInt32((Convert.ToDouble(LateCount) / Convert.ToDouble(table.Rows.Count)) * 100);
        }

        public static DataTable GetWorkOrder(string colName, string filter, string ordertype)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            string cmdselectstring;
            if (string.IsNullOrEmpty(colName))
            {
                cmdselectstring = "SELECT * FROM User_WorkOrder";
            }
            else
            {
                cmdselectstring = "SELECT " + colName + " FROM User_WorkOrder";
            }
            string cmdfilterstring;
            if (string.IsNullOrEmpty(filter))
            {
                cmdfilterstring = " where workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "')";
            }
            else
            {
                cmdfilterstring = " where " + filter + " and workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "')";
            }
            cmd.CommandText = cmdselectstring + cmdfilterstring;
            if (string.IsNullOrEmpty(ordertype) == false)
            {
                cmd.CommandText += "order by " + ordertype;
            }
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            da.Dispose();
            cmd.Connection.Dispose();
            return table;
        }
        public static DataTable GetOrder(string pageSize,string filter,int maxUid,string columns)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select top " + pageSize + " "+columns + " from User_WorkOrder where uid > '" + maxUid + "' and isScheduleWorkID = '1' and workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "') order by UID";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Dispose();
            return table;
        }

        /// <summary>
        /// 查询订单列表
        /// </summary>
        /// <returns></returns>
        public DataTable WorkOrderData(string pageSize,string curPage,string filter)
        {
            //查看要查询数据库中的哪些列
            JObject SQLWorkOrderFileds = AppSetting.TableFileds.SelectToken("SQLWorkOrderFiled").ToObject<JObject>();
            string SQLWorkOrderFiled = "";
            foreach (var item in SQLWorkOrderFileds)
            {
                if (string.IsNullOrEmpty(SQLWorkOrderFiled))
                {
                    SQLWorkOrderFiled += item.Key;
                }
                else
                {
                    SQLWorkOrderFiled += "," + item.Key;
                }
            }
            int max = 0;
            if (Convert.ToInt32(curPage) > 1)
            {
                max = GetMaxUid((Convert.ToInt32(curPage) - 1) * Convert.ToInt32(pageSize));
            }
            DataTable dt = GetOrder(pageSize,filter, max, SQLWorkOrderFiled);
            DataTable productId = GetOrder(pageSize, filter, max, "productID");
            foreach (var item in SQLWorkOrderFileds)
            {
                dt.Columns[item.Key].ColumnName = item.Value.Value<string>();
            }

            DataTable AttrTable = GetAttrTable(productId);
            JObject SQLAttrFiled = AppSetting.TableFileds.SelectToken("SQLAttrFiled").ToObject<JObject>();
            foreach (var item in SQLAttrFiled)
            {
                dt.Columns.Add(item.Value.Value<string>());
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int v = 0; v < AttrTable.Rows.Count; v++)
                {
                    if (AttrTable.Rows[v]["itemName"].ToString() == dt.Rows[i]["产品名称"].ToString())
                    {
                        foreach (DataColumn col in AttrTable.Columns)
                        {
                            if (col.ColumnName != "itemName")
                            {
                                dt.Rows[i][col.ColumnName] = AttrTable.Rows[v][col.ColumnName].ToString().Replace(" - ", " ").Replace("\"", "'");
                            }
                        }
                    }
                }
            }
            return dt;
        }
        /// <summary>
        /// 获取属性的列表
        /// </summary>
        /// <param name="productID">产品名称</param>
        /// <returns>属性的表格</returns>
        public DataTable GetAttrTable(DataTable productID)
        {
            DataTable table = new DataTable();
            JObject SQLFileds = AppSetting.TableFileds.SelectToken("SQLAttrFiled").ToObject<JObject>();
            string SQLFiledStr = "itemName";
            string productStr = "";
            for (int i = 0; i < productID.Rows.Count; i++)
            {
                if (i < productID.Rows.Count - 1)
                {
                    productStr += "'" + productID.Rows[i][0].ToString() + "',";
                }
                else
                {
                    productStr += "'" + productID.Rows[i][0].ToString() + "'";

                }
            }
            foreach (var item in SQLFileds)
            {
                SQLFiledStr += "," + item.Key;
            }
            SqlCommand cmd = PmConnections.ModCmd();
            cmd.CommandText = "Select " + SQLFiledStr + " from objProduct where sysID = '" + PMUser.UserSysID + "' and itemName in (" + productStr + ")";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            da.Dispose();
            cmd.Connection.Dispose();
            foreach (var item in SQLFileds)
            {
                table.Columns[item.Key].ColumnName = item.Value.Value<string>();
            }
            return table;
        }
        /// <summary>
        /// 订单列表获取最大的uid
        /// </summary>
        /// <param name="end">前几条的数据</param>
        /// <returns></returns>
        public int GetMaxUid(int end)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            int max;
            cmd.CommandText = "select top " + end + " uid from User_WorkOrder where  isScheduleWorkID = '1'  and workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "') order by UID";

           SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            max = Convert.ToInt32(table.Rows[table.Rows.Count - 1]["uid"]);
            ad.Dispose();
            cmd.Connection.Close();
            return max;
        }
        /// <summary>
        ///获取表格的总数
        /// </summary>
        /// <returns></returns>
        public int GetOrderCount()
        {
            int count = 0;
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT count(*) FROM User_WorkOrder where workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "')";
            count = (int)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return count;
        }
    }
}
