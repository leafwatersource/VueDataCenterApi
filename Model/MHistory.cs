using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.StaticFunc;

namespace WebApiNew.Model
{
    public class MHistory
    {
        /// <summary>
        /// 获取用户的操作记录
        /// </summary>
        /// <returns>wapMesEventRec表</returns>
        public JObject GetUserLog(string PageSize, string CurPage, string filter, string fuzzyFilter)
        {
            var SQLWorkPlanFileds = AppSetting.TableFileds.GetValue("HistoryTableFiled").ToString();
            var mJObj = JArray.Parse(SQLWorkPlanFileds);
            string sqlStr = "";
            DataTable table = new DataTable();
            int total = GetCountLog(filter, fuzzyFilter);
            int uid = 0;
            if (Convert.ToInt32(CurPage) > 1)
            {
                uid = LastPmuid((Convert.ToInt32(CurPage) - 1) * Convert.ToInt32(PageSize), filter, fuzzyFilter);
            }
            foreach (var item in mJObj)
            {
                JObject itemObj = item.ToObject<JObject>();
                string key = itemObj.First.First.Path;
                string type = itemObj.Last.Last.Value<string>();

                if (string.IsNullOrEmpty(sqlStr))
                {
                    if (type == "datetime")
                    {
                        sqlStr += "CONVERT(varchar(100)," + key + ", 120) AS " + key;
                    }
                    else if (type == "date")
                    {
                        sqlStr += "CONVERT(varchar(100)," + key + ", 111) AS " + key;
                    }
                    else
                    {
                        sqlStr += key;
                    }
                 
                }
                else
                {
                    if (type == "datetime")
                    {
                        sqlStr += ",CONVERT(varchar(100)," + key + ", 120) AS " + key;
                    }
                    else if (type == "date")
                    {
                        sqlStr += ",CONVERT(varchar(100)," + key + ", 111) AS " + key;
                    }
                    else
                    {
                        sqlStr += "," + key;
                    }
                }
            }
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT top " + PageSize + ' ' + sqlStr + " from User_MesDailyData where TaskFinishState > '0'";
            if (Convert.ToInt32(CurPage)<2)
            {
                if (!string.IsNullOrEmpty(filter) && filter != "{}" && filter != null)
                {
                    JObject filters = JObject.Parse(filter);

                    foreach (var item in filters)
                    {
                        cmd.CommandText += " and " + item.Key + "='" + item.Value + "'";
                    }
                }
            }
            else
            {
                cmd.CommandText += " and UID > '" + uid + "'";
                if (!string.IsNullOrEmpty(filter) && filter != "{}" && filter != null)
                {
                    JObject filters = JObject.Parse(filter);

                    foreach (var item in filters)
                    {
                        cmd.CommandText += " and " + item.Key + "='" + item.Value + "'";
                    }
                }

            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                cmd.CommandText += " and (workID like '%" + fuzzyFilter + "%' or productID like '%" + fuzzyFilter + "%')";
            }
            //cmd.CommandText += " order by UID desc";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(table);
            da.Dispose();
            cmd.Connection.Dispose();
            foreach (var item in mJObj)
            {
                JObject itemObj = item.ToObject<JObject>();
                string key = itemObj.First.First.Path;
                string value = itemObj.First.First.Value<string>();
                table.Columns[key].ColumnName = value;
            }
            JObject tableData = new JObject {
                { "total", total },
                { "rows", JsonConvert.SerializeObject(table) }
            };
            JObject data = new JObject {
                { "code", "0"},
                { "data", tableData},
                { "msg", "successful"}
            };
            return data;
        }
        /// <summary>
        /// 获取操作记录的表格中的条数
        /// </summary>
        /// <returns>操作记录表格的条数</returns>
        public int GetCountLog(string filter,string fuzzyFilter)
        {
            int count = 0;
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT count(*) from User_MesDailyData where TaskFinishState > '0'";
            if (!string.IsNullOrEmpty(filter) && filter != "{}" && filter != null)
            {
                JObject filters = JObject.Parse(filter);

                foreach (var item in filters)
                {
                    cmd.CommandText += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                cmd.CommandText += " and (workID like '%" + fuzzyFilter + "%' or productID like '%" + fuzzyFilter + "%')";
            }
            count = (int)cmd.ExecuteScalar();
            return count;
        }
        /// <summary>
        /// 获取上一页的最后一个pmuid值
        /// </summary>
        /// <param name="end">前面多少条数据</param>
        /// <returns></returns>
        public int LastPmuid(int end,string filter,string fuzzyFilter)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            int min;
            cmd.CommandText = "select top " + end + " UID from User_MesDailyData where TaskFinishState > 0";
            if (!string.IsNullOrEmpty(filter) && filter != "{}" && filter != null)
            {
                JObject filters = JObject.Parse(filter);

                foreach (var item in filters)
                {
                    cmd.CommandText += " and " + item.Key + "='" + item.Value + "'";
                }
            }
            if (!string.IsNullOrEmpty(fuzzyFilter))
            {
                cmd.CommandText += " and (workID like '%" + fuzzyFilter + "%' or productID like '%" + fuzzyFilter + "%')";
            }
            //cmd.CommandText += " order by UID desc";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            min = Convert.ToInt32(table.Rows[table.Rows.Count - 1]["UID"]);
            ad.Dispose();
            cmd.Connection.Close();
            return min;
        }
    }
}
