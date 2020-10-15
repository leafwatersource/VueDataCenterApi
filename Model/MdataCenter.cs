using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Model
{
    public class MdataCenter
    {
        public string Owner { get; set; }
        public string WorkPlanName { get; set; }
        public string WorkPlanId { get; set; }
        public string ReleaseTime { get; set; }
        public string PMOcState { get; set; }
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
        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="filter"></param>
        /// <param name="maxUid"></param>
        /// <param name="columns"></param>
        /// <param name="fuzzyFilter"></param>
        /// <returns></returns>
        public static DataTable GetOrder(string pageSize, string filter, int maxUid, string columns)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select top " + pageSize + " " + columns + " from User_WorkOrder where uid > '" + maxUid + "' and isScheduleWorkID = '1' and workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "')";
            string filterStr = "";
            if (!String.IsNullOrEmpty(filter) && filter != "{}")
            {
                JObject filters = JObject.Parse(filter);
                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
           
          cmd.CommandText += filterStr + " order by UID";
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
        public DataTable WorkOrderData(string pageSize, string curPage, string filter, string fuzzyFilter)
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
            DataTable dt = GetOrder(pageSize, filter, max, SQLWorkOrderFiled);
            DataTable AttrTable = GetAttrTable(dt);
            foreach (var item in SQLWorkOrderFileds)
            {
                dt.Columns[item.Key].ColumnName = item.Value.Value<string>();
            }
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
        public DataTable GetAttrTable(DataTable dt)
        {
            DataTable table = new DataTable();
            JObject SQLFileds = AppSetting.TableFileds.SelectToken("SQLAttrFiled").ToObject<JObject>();
            string SQLFiledStr = "itemName";
            string productStr = "";
            SqlCommand cmd = PmConnections.ModCmd();
            foreach (var item in SQLFileds)
            {
                SQLFiledStr += "," + item.Key;
            }
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i < dt.Rows.Count - 1)
                    {
                        productStr += "'" + dt.Rows[i]["productID"].ToString() + "',";
                    }
                    else
                    {
                        productStr += "'" + dt.Rows[i]["productID"].ToString() + "'";

                    }
                }
                cmd.CommandText = "Select " + SQLFiledStr + " from objProduct where sysID = '" + PMUser.UserSysID + "' and itemName in (" + productStr + ")";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(table);
                da.Dispose();
                foreach (var item in SQLFileds)
                {
                    table.Columns[item.Key].ColumnName = item.Value.Value<string>();
                }
            }
            cmd.Connection.Dispose();
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
        public int GetOrderCount(string filter, string fuzzyFilter)
        {
            int count = 0;
            string filterStr = "";
            string fuzzyFilterText = "";
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT count(*) FROM User_WorkOrder where workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "')";
            bool aaa = string.IsNullOrEmpty(filter);
            if (!string.IsNullOrEmpty(filter) && filter != "{}" && filter !=null)
            {
                JObject filters = JObject.Parse(filter);
                
                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                JObject SQLWorkOrderFileds = AppSetting.TableFileds.SelectToken("SQLWorkOrderFiled").ToObject<JObject>();
                foreach (var item in SQLWorkOrderFileds)
                {
                if (string.IsNullOrEmpty(fuzzyFilterText))
                    {
                        fuzzyFilterText += " and ( Convert(varchar," + item.Key + ",120) like '%" + fuzzyFilter + "%'";
                    }
                    else
                    {
                        fuzzyFilterText += " or Convert(varchar," + item.Key + ",120) like '%" + fuzzyFilter + "%'";
                    }
                }
                fuzzyFilterText += ")";
            }
            cmd.CommandText += filterStr+ fuzzyFilterText;
            count = (int)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return count;
        }
        
        /// <summary>
        /// 获取设备组
        /// </summary>
        /// <returns></returns>
        public string GetViewGroup()
        {
            JObject data = new JObject();
            DataTable groupList = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT distinct viewname FROM View_PmViewGroup where SYSID ='" + PMUser.UserSysID + "' and vglobal = 'export'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(groupList);
            if (groupList.Rows.Count == 0)
            {
               
                cmd.CommandText = "SELECT distinct viewname FROM View_PmViewGroup where SYSID ='" + PMUser.UserSysID + "'";
                ad = new SqlDataAdapter(cmd);
                ad.Fill(groupList);
                ad.Dispose();
                if (groupList.Rows.Count == 0)
                {
                    data.Add("groupList", "没有为设备创建视图");
                    data.Add("listCount", 0);
                    data.Add("status", 0);
                }
                else
                {
                    data.Add("groupList", JsonConvert.SerializeObject(groupList));
                    data.Add("listCount", groupList.Rows.Count);
                    data.Add("status", 1);
                }
            }
            else
            {
                data.Add("groupList", JsonConvert.SerializeObject(groupList));
                data.Add("listCount", groupList.Rows.Count);
                data.Add("status", 1);
            }
            return data.ToString();
        }
        /// <summary>
        /// 获取设备组下的所有的工单
        /// </summary>
        /// <param name="optionPlan">目标设备</param>
        /// <param name="ViewName">设备名称</param>
        /// <returns></returns>
        public string GetResView(string optionPlan, string ViewName)
        {
            DataTable table = new DataTable();
            //查看要查询数据库中的哪些列
            JObject SQLWorkPlanFileds = AppSetting.TableFileds.SelectToken("SQLWorkPlanFiled").ToObject<JObject>();
            string SQLWorkPlanFiled = "";
            foreach (var item in SQLWorkPlanFileds)
            {
                if (string.IsNullOrEmpty(SQLWorkPlanFiled))
                {
                    SQLWorkPlanFiled += item.Key;
                }
                else
                {
                    SQLWorkPlanFiled += "," + item.Key;
                }
            }
            if (ViewName == null)
            {
                table = GetWorkPlanBars(SQLWorkPlanFiled, "OperationID = '" + optionPlan + "'", string.Empty);
            }
            else if (optionPlan == null)
            {
                table = GetWorkPlanBars(SQLWorkPlanFiled, "OperationID in ( select resName from View_pmViewGroup where VIewName = '" + ViewName + "')", string.Empty);
            }
            foreach (var item in SQLWorkPlanFileds)
            {
                table.Columns[item.Key].ColumnName = item.Value.Value<string>();
            }
            table.Columns.Add("temp", Type.GetType("System.Decimal"));
            table.Columns.Add("temp1", Type.GetType("System.Decimal"));
            for (int i = 0; i < table.Rows.Count; i++)
            {
                decimal productTime = Convert.ToDecimal(table.Rows[i][SQLWorkPlanFileds["RealWorkTime"].ToString()]) / 3600;
                productTime = Math.Round(productTime, 2);
                table.Rows[i]["temp"] = productTime;
                decimal setupTime = Convert.ToDecimal(table.Rows[i][SQLWorkPlanFileds["setupTime"].ToString()]) / 60;
                setupTime = Math.Round(setupTime, 2);
                table.Rows[i]["temp1"] = setupTime;
            }
            table.Columns.Remove(SQLWorkPlanFileds["RealWorkTime"].ToString());
            table.Columns["temp"].ColumnName = SQLWorkPlanFileds["RealWorkTime"].ToString();
            table.Columns.Remove(SQLWorkPlanFileds["setupTime"].ToString());
            table.Columns["temp1"].ColumnName = SQLWorkPlanFileds["setupTime"].ToString();
            table.AcceptChanges();
            JObject data = new JObject();
            data.Add("ImplementationData", JsonConvert.SerializeObject(table));
            data.Add("ImplementationCount", table.Rows.Count);
            data.Add("ImplementationStatus", 1);
            return data.ToString();
        }
        /// <summary>
        /// 获取执行计划的数据
        /// </summary>
        /// <param name="colName">列名</param>
        /// <param name="filter">筛选的条件</param>
        /// <param name="ordertype">排序</param>
        /// <returns></returns>
        public static DataTable GetWorkPlanBars(string colName, string filter, string ordertype)
        {
            // colName: colname1,colname2,
            // filter:colname1 = 'value1',and colname2 = 'value2'
            // ordertype colname1,colname2 DESC
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            string cmdselectstring;
            if (string.IsNullOrEmpty(colName))
            {
                cmdselectstring = "SELECT * FROM View_WorkPlansBars";
            }
            else
            {
                cmdselectstring = "SELECT " + colName + " FROM View_WorkPlansBars";
            }
            string cmdfilterstring;
            if (string.IsNullOrEmpty(filter))
            {
                cmdfilterstring = " where WorkPlanID in (select WorkPlanID from PMS_WorkPlans where Status = '" + PMUser.PMPlState + "' and sysid = '" + PMUser.UserSysID + "')";
            }
            else
            {
                cmdfilterstring = " where " + filter + " and WorkPlanID in (select WorkPlanID from PMS_WorkPlans where Status = '" + PMUser.PMPlState + "' and sysid = '" + PMUser.UserSysID + "')";
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
    }
}
