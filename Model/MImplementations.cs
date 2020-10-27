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
        /// <param name="PageSize">一页展示多少条数据</param>
        /// <param name="CurPage">当前的页数</param>
        /// <param name="Resource">设备名称</param>
        /// <param name="GroupName">设备组的名称</param>
        /// <param name="ChangeModel">是否是换模计划</param>
        /// <returns></returns>
        public string GetResPlan(string PageSize, string CurPage,string Resource, string GroupName,bool ChangeModel,string filter,string fuzzyFilter)
        {
            int max = 0;
            if (Convert.ToInt32(CurPage) > 1)
            {
                max = MaxBarID((Convert.ToInt32(CurPage) - 1) * Convert.ToInt32(PageSize), Resource, GroupName,ChangeModel, filter, fuzzyFilter);
            }
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
            if (string.IsNullOrEmpty(Resource))
            {
                table = GetWorkPlanBars(PageSize, SQLWorkPlanFiled, " OperationID in ( select resName from View_pmViewGroup where VIewName = '" + GroupName + "') and BarID>'" + max + "'", ChangeModel, filter, fuzzyFilter);
            }
            else
            {
                table = GetWorkPlanBars(PageSize, SQLWorkPlanFiled, " OperationID = '" + Resource + "' and BarID>'" + max + "'", ChangeModel, filter, fuzzyFilter);
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
            data.Add("ImplementationCount", ResWorkCount(Resource, GroupName,ChangeModel,filter, fuzzyFilter));
            data.Add("ImplementationStatus", 1);
            return data.ToString();
        }
        /// <summary>
        /// 获取执行计划的数据
        /// </summary>
        /// <param name="PageSize">一页展示多少条数据</param>
        /// <param name="colName">要显示的列</param>
        /// <param name="filter">筛选条件</param>
        /// <param name="ChangeModel">是否是换模计划</param>
        /// <returns></returns>
        public static DataTable GetWorkPlanBars(string PageSize,string colName, string filter,bool ChangeModel,string filterClient,string fuzzyFilter)
        {
            // colName: colname1,colname2,
            // filter:colname1 = 'value1',and colname2 = 'value2'
            // ordertype colname1,colname2 DESC
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            string cmdselectstring;
            string filterStr = "";
            string fuzzyFilterText = "";
            if (string.IsNullOrEmpty(colName))
            {
                cmdselectstring = "SELECT * FROM View_WorkPlansBars";
            }
            else
            {
                cmdselectstring = "SELECT top "+ PageSize + colName + " FROM View_WorkPlansBars";
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
            if (!String.IsNullOrEmpty(filterClient) && filterClient != "{}")
            {
                JObject filters = JObject.Parse(filterClient);
                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                JObject SQLWorkOrderFileds = AppSetting.TableFileds.SelectToken("SQLWorkPlanFiled").ToObject<JObject>();
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
            if (ChangeModel)
            {
                cmdfilterstring += " and setupTime >0 ";
            }
            cmd.CommandText = cmdselectstring+ cmdfilterstring + filterStr+ fuzzyFilterText + "order by BarID";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            da.Dispose();
            cmd.Connection.Dispose();
            return table;
        }
   
        public JObject GetResView(string resGroup,string resName)
        {
            DataTable table = new DataTable();
           SqlCommand cmd = PmConnections.SchCmd();
            JObject data = new JObject();
            cmd.CommandText = "SELECT resName  FROM View_PmViewGroup where sysID ='" + PMUser.UserSysID + "' and ViewName  = '" + resGroup + "' ";
            if (!string.IsNullOrEmpty(resName))
            {
                cmd.CommandText += "and resName like '%" + resName + "%'";
            }
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
        public int MaxBarID(int end,string resource,string GroupName,bool ChangeModel,string filter,string fuzzyFilter)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            string filterStr = "";
            string fuzzyFilterText = "";
            if (string.IsNullOrEmpty(resource))
            {
                cmd.CommandText = "select top " + end + " BarID from View_WorkPlansBars where OperationID in ( select resName from View_pmViewGroup where VIewName = '" + GroupName + "') and WorkPlanID in (select WorkPlanID from PMS_WorkPlans where Status = 'Released' and sysid = '" + PMUser.UserSysID + "')" ;
            }
            else
            {
                cmd.CommandText = "select top " + end + " BarID from View_WorkPlansBars where OperationID = '" + resource + "' and WorkPlanID in (select WorkPlanID from PMS_WorkPlans where Status = 'Released' and sysid = '" + PMUser.UserSysID + "')";
            }
            if (!String.IsNullOrEmpty(filter) && filter != "{}")
            {
                JObject filters = JObject.Parse(filter);
                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                JObject SQLWorkOrderFileds = AppSetting.TableFileds.SelectToken("SQLWorkPlanFiled").ToObject<JObject>();
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
            if (ChangeModel)
            {
                cmd.CommandText += " and setupTime >0 ";
            }
           cmd.CommandText += filterStr+ fuzzyFilterText + " order by BarID";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return Convert.ToInt32(table.Rows[table.Rows.Count-1][0]);
        }
        public int ResWorkCount(string resource, string GroupName, bool ChangeModel,string fitler,string fuzzyFilter)
        {
            int count = 0;
            string filterStr = "";
            string fuzzyFilterText = "";
            SqlCommand cmd = PmConnections.SchCmd();
            if (string.IsNullOrEmpty(resource))
            {
                cmd.CommandText = "select count(WIPID) from View_WorkPlansBars where  OperationID in ( select resName from View_pmViewGroup where VIewName = '" + GroupName + "') and WorkPlanID in (select WorkPlanID from PMS_WorkPlans where Status = 'Released' and sysid = '" + PMUser.UserSysID + "')";
            }
            else
            {
                cmd.CommandText = "select count(WIPID) from View_WorkPlansBars where  OperationID = '" + resource + "' and WorkPlanID in (select WorkPlanID from PMS_WorkPlans where Status = 'Released' and sysid = '" + PMUser.UserSysID + "')";
            }
            if (!String.IsNullOrEmpty(fitler) && fitler != "{}")
            {
                JObject filters = JObject.Parse(fitler);
                foreach (var item in filters)
                {
                    filterStr += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                JObject SQLWorkOrderFileds = AppSetting.TableFileds.SelectToken("SQLWorkPlanFiled").ToObject<JObject>();
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
            cmd.CommandText += filterStr + fuzzyFilterText;
            if (ChangeModel)
            {
                cmd.CommandText += " and setupTime >0 ";
            }
            count = (int)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return count;
        }
    }
}
