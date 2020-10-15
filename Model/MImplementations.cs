using Newtonsoft.Json;
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
    public class MImplementations
    {
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
        public string GetResPlan(string optionPlan, string ViewName)
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
   
        public JObject GetResView(string resGroup)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            JObject data = new JObject();
            cmd.CommandText = "SELECT resName  FROM View_PmViewGroup where sysID ='" + PMUser.UserSysID + "' and ViewName  = '" + resGroup + "'";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            da.Dispose();
            cmd.Connection.Dispose();
            if (table.Rows.Count == 0)
            {
                data.Add("resCount", table.Rows.Count);
                data.Add("resView", JsonConvert.SerializeObject(table));
                data.Add("resStatus", 0);
            }
            else
            {
                data.Add("resCount", table.Rows.Count);
                data.Add("resView", JsonConvert.SerializeObject(table));
                data.Add("resStatus", 1);
            }
            return data;
        }
    }
}
