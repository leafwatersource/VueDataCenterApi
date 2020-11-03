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
        public JObject GetUserLog(string PageSize, string CurPage, string filter)
        {
            var SQLWorkPlanFileds = AppSetting.TableFileds.GetValue("HistoryTableFiled").ToString();
            var mJObj = JArray.Parse(SQLWorkPlanFileds);
            string sqlStr = "";
            DataTable table = new DataTable();
            int total = GetCountLog(filter);
            int minPmuid = 0;
            if (Convert.ToInt32(CurPage) > 1)
            {
                minPmuid = LastPmuid((Convert.ToInt32(CurPage) - 1) * Convert.ToInt32(PageSize), filter);
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
            if (Convert.ToInt32(CurPage)<2)
            {
                cmd.CommandText = "SELECT top " + PageSize + ' ' + sqlStr + " from vWapRec";
                if (!string.IsNullOrEmpty(filter))
                {
                    cmd.CommandText += " where EventTime >='" + filter + "'";
                }
            }
            else
            {
                cmd.CommandText = "SELECT top " + PageSize + ' ' + sqlStr + " from vWapRec where pmuid < '" + minPmuid + "'";
                if (!string.IsNullOrEmpty(filter))
                {
                    cmd.CommandText += " and EventTime>='" + filter + "'";
                }

            }
           
            cmd.CommandText += " order by EventTime desc";
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
        public int GetCountLog(string filter)
        {
            int count = 0;
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "SELECT count(*) from vWapRec";
            if (!string.IsNullOrEmpty(filter))
            {
                cmd.CommandText+= " where EventTime >='" + filter + "'";
            }
            count = (int)cmd.ExecuteScalar();
            return count;
        }
        /// <summary>
        /// 获取上一页的最后一个pmuid值
        /// </summary>
        /// <param name="end">前面多少条数据</param>
        /// <returns></returns>
        public int LastPmuid(int end,string filter)
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            int min;
            cmd.CommandText = "select top " + end + " PMUID from vWapRec";
            if (!string.IsNullOrEmpty(filter))
            {
                cmd.CommandText += " where EventTime >='" + filter + "'";
            }
            cmd.CommandText += " order by EventTime desc";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            min = Convert.ToInt32(table.Rows[table.Rows.Count - 1]["PMUID"]);
            ad.Dispose();
            cmd.Connection.Close();
            return min;
        }
    }
}
