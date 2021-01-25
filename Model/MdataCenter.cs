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
        /// <summary>
        /// 获取百分数
        /// </summary>
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

        /// <summary>
        /// 根据工单获取所有的工序的进度
        /// </summary>
        /// <param name="workID">工单号码</param>
        /// <returns></returns>
        public DataTable GetOPData(string workID)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select * from User_MESAllData where workID = '" + workID + "' order by opIndex";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
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
        /// <param name="workType">工单类型</param>
        /// <returns></returns>%
        public static DataTable GetOrder(string pageSize, string filter, int maxUid, string columns, string workType, string fuzzyFilter,string filterFirstDemandDay)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select top " + pageSize + " " + columns + " from User_WorkOrder where uid > '" + maxUid + "' and isScheduleWorkID = '1' and workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "')";
            //cmd.CommandText = "select top " + pageSize + " " + columns + " from User_WorkOrder where uid > '" + maxUid + "' and isScheduleWorkID = '1' and workPlanID = 959";
            string filterStr = "";
            string fuzzyFilterText = "";
            if (!String.IsNullOrEmpty(filter) && filter != "{}")
            {
                JObject filters = JObject.Parse(filter);
                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (workType == "EarlyPlan")
            {
                //提前的工单
                filterStr += " and delayDays < '0'";
            }
            else if (workType == "OnTimePlan")
            {
                //正常的工单
                filterStr += " and delayDays = '0'";
            }
            else if (workType == "LatePlan")
            {
                //延迟的工单
                filterStr += " and delayDays > '0'";
            }
            else if (workType == "ErrorPlan")
            {
                //异常的订单
                filterStr += " and planStartTime is null";
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                var SQLWorkOrderFileds = AppSetting.TableFileds.GetValue("SQLWorkOrderFiled").ToString();
                var mJObj = JArray.Parse(SQLWorkOrderFileds);
                foreach (var item in mJObj)
                {
                    JObject itemObj = item.ToObject<JObject>();
                    string key = itemObj.First.First.Path;
                    if (string.IsNullOrEmpty(fuzzyFilterText))
                    {
                        fuzzyFilterText += " and ( Convert(varchar," + key + ",120) like '%" + fuzzyFilter + "%'";
                    }
                    else
                    {
                        fuzzyFilterText += " or Convert(varchar," + key + ",120) like '%" + fuzzyFilter + "%'";
                    }
                }
                fuzzyFilterText += ")";
            }
            if (!string.IsNullOrEmpty(filterFirstDemandDay))
            {
                JArray filterFirstDemandDayArr = (JArray)JsonConvert.DeserializeObject(filterFirstDemandDay);
                string filterFirstDemandDayStr = "(";
                foreach (string item in filterFirstDemandDayArr)
                {
                    filterFirstDemandDayStr += filterFirstDemandDayStr == "("?"'"+item+"'":",'"+item+"'";
                }
                filterFirstDemandDayStr += ")";
                filterStr += " and firstDemandDay in " + filterFirstDemandDayStr;
            }
            cmd.CommandText += filterStr + fuzzyFilterText + " order by UID";
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
        public DataTable WorkOrderData(string pageSize, string curPage, string filter, string fuzzyFilter, string workType, string filterFirstDemandDay)
        {
            //查看要查询数据库中的哪些列
            var SQLWorkOrderFileds = AppSetting.TableFileds.GetValue("SQLWorkOrderFiled").ToString();
            var mJObj = JArray.Parse(SQLWorkOrderFileds);
            string SQLWorkOrderFiled = "";
            foreach (var item in mJObj)
            {
                JObject itemObj = item.ToObject<JObject>();
                string key = itemObj.First.First.Path;
                string type = itemObj.Last.Last.Value<string>();
                if (string.IsNullOrEmpty(SQLWorkOrderFiled))
                {
                    if (type == "datetime")
                    {
                        SQLWorkOrderFiled += "CONVERT(varchar(100)," + key + ", 120) AS " + key;
                    }
                    else if (type == "date")
                    {
                        SQLWorkOrderFiled += "CONVERT(varchar(100)," + key + ", 111) AS " + key;
                    }
                    else
                    {
                        SQLWorkOrderFiled += key;
                    }
                }
                else
                {
                    if (type == "datetime")
                    {
                        SQLWorkOrderFiled += ",CONVERT(varchar(100)," + key + ", 120) AS " + key;
                    }
                    else if (type == "date")
                    {
                        SQLWorkOrderFiled += ",CONVERT(varchar(100)," + key + ", 111) AS " + key;
                    }
                    else
                    {
                        SQLWorkOrderFiled += "," + key;
                    }
                }
            }
            int max = 0;
            if (Convert.ToInt32(curPage) > 1)
            {
                max = GetMaxUid((Convert.ToInt32(curPage) - 1) * Convert.ToInt32(pageSize), workType);
            }
            DataTable dt = GetOrder(pageSize, filter, max, SQLWorkOrderFiled, workType, fuzzyFilter, filterFirstDemandDay);
            foreach (var item in mJObj)
            {
                JObject itemObj = item.ToObject<JObject>();
                string path = itemObj.First.First.Path;
                string column = itemObj.First.First.Value<string>();
                dt.Columns[path].ColumnName = column;
                //string column = itemObj;
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
        public int GetMaxUid(int end, string workType)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            int max;
            string filterStr = "";
            if (workType == "EarlyPlan")
            {
                //提前的工单
                filterStr += " and delayDays < '0'";
            }
            else if (workType == "OnTimePlan")
            {
                //正常的工单
                filterStr += " and delayDays = '0'";
            }
            else if (workType == "LatePlan")
            {
                //延迟的工单
                filterStr += " and delayDays > '0'";
            }
            else if (workType == "ErrorPlan")
            {
                //异常的订单
                filterStr += " and planStartTime is null";
            }
            cmd.CommandText = "select top " + end + " uid from User_WorkOrder where  isScheduleWorkID = '1'  and workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "') " + filterStr + " order by UID";

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
        public int GetOrderCount(string filter, string fuzzyFilter, string workType, string filterFirstDemandDay)
        {
            int count = 0;
            string filterStr = "";
            string fuzzyFilterText = "";
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT count(*) FROM User_WorkOrder where  workPlanID in (SELECT workPlanID FROM PMS_WorkPlans where sysID = '" + PMUser.UserSysID + "' and Status = '" + PMUser.PMOcState + "') and isScheduleWorkID = '1'";
            if (!string.IsNullOrEmpty(filterFirstDemandDay))
            {
                JArray filterFirstDemandDayArr = (JArray)JsonConvert.DeserializeObject(filterFirstDemandDay);
                string filterFirstDemandDayStr = "(";
                foreach (string item in filterFirstDemandDayArr)
                {
                    filterFirstDemandDayStr += filterFirstDemandDayStr == "(" ?"'"+ item+"'" : "," + "'"+item+"'";
                }
                filterFirstDemandDayStr += ")";
                filterStr += " and firstDemandDay in " + filterFirstDemandDayStr;
            }

            if (!string.IsNullOrEmpty(filter) && filter != "{}" && filter != null)
            {
                JObject filters = JObject.Parse(filter);

                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                var SQLWorkOrderFileds = AppSetting.TableFileds.GetValue("SQLWorkOrderFiled").ToString();
                var mJObj = JArray.Parse(SQLWorkOrderFileds);
                foreach (var item in mJObj)
                {
                    JObject itemObj = item.ToObject<JObject>();
                    string key = itemObj.First.First.Path;
                    if (string.IsNullOrEmpty(fuzzyFilterText))
                    {
                        fuzzyFilterText += " and ( Convert(varchar," + key + ",120) like '%" + fuzzyFilter + "%'";
                    }
                    else
                    {
                        fuzzyFilterText += " or Convert(varchar," + key + ",120) like '%" + fuzzyFilter + "%'";
                    }
                }
                fuzzyFilterText += ")";
            }
            if (workType == "EarlyPlan")
            {
                //提前的工单
                filterStr += " and delayDays < '0'";
            }
            else if (workType == "OnTimePlan")
            {
                //正常的工单
                filterStr += " and delayDays = '0'";
            }
            else if (workType == "LatePlan")
            {
                //延迟的工单
                filterStr += " and delayDays > '0'";
            }
            else if (workType == "ErrorPlan")
            {
                //异常的订单
                filterStr += " and planStartTime is null";
            }
           
            
            cmd.CommandText += filterStr + fuzzyFilterText;
            count = (int)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return count;
        }
    }
}

