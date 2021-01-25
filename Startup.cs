using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApiNew.Middle;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            RenderConfig();
        }
        public void RenderConfig()
        {
            string filepach = AppContext.BaseDirectory;
            AppSetting.BasePath = filepach;
            XmlDocument document = new XmlDocument();
            document.Load(filepach + "appsettings.xml");
            //数据库连接字段
            XmlNode Connection = document.SelectSingleNode("AppConfig").SelectSingleNode("Connection");
            //属性定义字段
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("TableFileds");//订单查询、执行计划、设备使用记录的列
            XmlNodeList TableFiledsConfigList = TableFileConfig.ChildNodes;
            XmlNodeList ConnectionList = Connection.ChildNodes;//连接字符串
            XmlNode Pmsetting = document.SelectSingleNode("AppConfig").SelectSingleNode("PMSettings");//读取配置
            XmlNodeList SettingList = Pmsetting.ChildNodes;
            XmlNodeList StatisticsHover = document.SelectSingleNode("AppConfig").SelectSingleNode("Statistics").ChildNodes;//设备状态要显示的列
            XmlNodeList FilterFileds = document.SelectSingleNode("AppConfig").SelectSingleNode("FilterFileds").ChildNodes;//列表筛选的列的定义
            string datasource = Connection.Attributes["datasource"].Value;
            List<JObject> temp;
            AppSetting.TableFileds = new JObject();
            foreach (XmlNode item in TableFiledsConfigList)
            {
                XmlNodeList xl = item.ChildNodes;
                temp = new List<JObject>();
                foreach (XmlNode filed in xl)
                {
                    JObject props = new JObject { {
                        filed.Attributes["name"].Value.ToString(),filed.InnerText
                        },{
                        "type",filed.Attributes["type"].Value
                        },{
                        "width",filed.Attributes["width"].Value
                        },
                        { "sqlNameT", filed.Attributes["name"].Value},
                        { "showNameT",filed.InnerText},
                        { "switchNameT", filed.Attributes["filter"].Value},
                        { "indexNameT", filed.Attributes["indexNameT"].Value},
                    };
                    //temp.Add(filed.Name, filed.InnerText);
                    //temp.Add("type", filed.Attributes["type"].Value);
                    temp.Append(props);
                    temp.Add(props);
                }
                AppSetting.TableFileds.Add(item.Name.ToString(), JsonConvert.SerializeObject(temp));
            }

            foreach (XmlNode item in ConnectionList)
            {
                if (item.Name.ToLower() == "modeler") {
                    PmConnections.Modconnstr = "Data Source=" + datasource + ";Initial Catalog=" + item.InnerText + ";" + item.Attributes["security"].Value + ";User ID=" + item.Attributes["username"].Value + ";Password=" + item.Attributes["password"].Value + ";" + item.Attributes["muti"].Value;
                }
                else if (item.Name.ToLower() == "schedule")
                {
                    PmConnections.Schconnstr = "Data Source=" + datasource + ";Initial Catalog=" + item.InnerText + ";" + item.Attributes["security"].Value + ";User ID=" + item.Attributes["username"].Value + ";Password=" + item.Attributes["password"].Value + ";" + item.Attributes["muti"].Value;
                }
                else
                {
                    PmConnections.Ctrlconnstr = "Data Source=" + datasource + ";Initial Catalog=" + item.InnerText + ";" + item.Attributes["security"].Value + ";User ID=" + item.Attributes["username"].Value + ";Password=" + item.Attributes["password"].Value + ";" + item.Attributes["muti"].Value;
                }
            }

            foreach (XmlNode item in SettingList)
            {
                if (item.Name.ToLower() == "plstate")
                {
                    PMUser.PMPlState = item.InnerText;
                }
                if (item.Name.ToLower() == "ocstate")
                {
                    PMUser.PMOcState = item.InnerText;
                }
                if (item.Name.ToLower() == "changeplan")
                {
                    AppSetting.ChangePlan = Convert.ToBoolean(item.InnerText);
                }
            }
            AppSetting.StatisticsHover = new JObject();
            //初始化设备使用状态页面鼠标hover的展示信息
            SetStatisticsHover(StatisticsHover);
            //SetFilterFiled(FilterFileds);
            RenderMapResTable();
            SetResStatistics();
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(opertions =>
            {
                opertions.AddPolicy("any", bulider =>
                {
                    bulider.SetIsOriginAllowed(op => true)  
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();

                });
            });
            services.AddControllers();

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseCors("any");

            app.UseRouting();
            app.UseMiddleware<HasLoginMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        //读取MapResTable.xml,如果不存在就创建，如果存在就初始化有效工时的map表
        public void RenderMapResTable()
        {
            string filepach = AppContext.BaseDirectory;
            XmlDocument xmlDoc = new XmlDocument();
            DataTable MapResTable = new DataTable();
            SqlCommand cmd = PmConnections.SchCmd();
            cmd.CommandText = "select distinct(eventtype) from wapMesEventRec";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(MapResTable);
            cmd.Connection.Close();
            if (File.Exists(filepach + "MapResTable.xml")) {
                XmlDocument document = new XmlDocument();
                document.Load(filepach + "MapResTable.xml");
                var root = document.SelectSingleNode("Root");
                XmlNode MAPTABLE = root.SelectSingleNode("MAPTABLE");
                var resCount = Math.Floor(Math.Sqrt(MAPTABLE.ChildNodes.Count));
                if (resCount != MapResTable.Rows.Count)
                {
                    root.RemoveChild(MAPTABLE);
                    SetMapResXML(xmlDoc, MapResTable);
                }
            }
            else
            {
                SetMapResXML(xmlDoc, MapResTable);
            }
            SetMapResTable();
        }

        //初始化有效工时的map表
        public void SetMapResTable()
        {
            string filepach = AppContext.BaseDirectory;
            XmlDocument document = new XmlDocument();
            document.Load(filepach + "MapResTable.xml");
            var MAPTABLE = document.SelectSingleNode("Root").SelectSingleNode("MAPTABLE").ChildNodes;
            AppSetting.MapResTable = new DataTable();
            AppSetting.MapResTable.Columns.Add("curEventType");
            AppSetting.MapResTable.Columns.Add("nextEventType");
            AppSetting.MapResTable.Columns.Add("effective");
            foreach (XmlNode item in MAPTABLE)
            {
                DataRow row = AppSetting.MapResTable.NewRow();
                row["curEventType"] = item.Attributes["curEventType"].Value;
                row["nextEventType"] = item.Attributes["nextEventType"].Value;
                row["effective"] = item.Attributes["effective"].Value;
                AppSetting.MapResTable.Rows.Add(row);
            }
        }
        //创建MapResTable.xml并赋值默认值
        public void SetMapResXML(XmlDocument xmlDoc, DataTable MapResTable) {
            string filepach = AppContext.BaseDirectory;
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);
            XmlNode root = xmlDoc.CreateElement("Root");
            XmlNode MAPTABLE = xmlDoc.CreateElement("MAPTABLE");
            root.AppendChild(MAPTABLE);
            xmlDoc.AppendChild(root);
            foreach (DataRow item in MapResTable.Rows)
            {
                foreach (DataRow nextItem in MapResTable.Rows)
                {
                    XmlNode COLUMN = xmlDoc.CreateElement("COLUMN");
                    COLUMN.Attributes.Append(CreateAttribute(COLUMN, "curEventType", item[0].ToString()));
                    COLUMN.Attributes.Append(CreateAttribute(COLUMN, "nextEventType", nextItem[0].ToString()));
                    COLUMN.Attributes.Append(CreateAttribute(COLUMN, "effective", "false"));
                    MAPTABLE.AppendChild(COLUMN);
                }
            }

            xmlDoc.Save(filepach + "MapResTable.xml");
        }
        //xml创建节点
        public XmlAttribute CreateAttribute(XmlNode node, string attributeName, string value)
        {
            try
            {
                XmlDocument doc = node.OwnerDocument;
                XmlAttribute attr = null;
                attr = doc.CreateAttribute(attributeName);
                attr.Value = value;
                node.Attributes.SetNamedItem(attr);
                return attr;
            }
            catch (Exception err)
            {
                string desc = err.Message;
                return null;
            }
        }

        /// <summary>
        /// 初始化设备使用状态页面鼠标hover的展示信息
        /// </summary>
        public void SetStatisticsHover(XmlNodeList nodeList)
        {
            foreach (XmlNode item in nodeList)
            {
                string name = item.Name;
                int hasChild = item.ChildNodes.Count;
                if (hasChild > 1)
                {
                    JObject attr = new JObject();
                    XmlNodeList list = item.ChildNodes;
                    foreach (XmlNode itemList in list)
                    {
                        string listName = itemList.Name;
                        string listValue = itemList.InnerText;
                        if (!string.IsNullOrEmpty(listValue))
                        {
                            attr[listName] = listValue;
                        }
                    }
                    AppSetting.StatisticsHover[name] = attr;
                }
                else
                {
                    string value = item.InnerText;
                    if (!string.IsNullOrEmpty(value))
                    {
                        AppSetting.StatisticsHover[name] = value;
                    }
                }

            }
        }
        /// <summary>
        /// 设置产能分析的表格的列
        /// </summary>
        public void SetResStatistics(){
            string filepach = AppContext.BaseDirectory;
            AppSetting.BasePath = filepach;
            XmlDocument document = new XmlDocument();
            document.Load(filepach + "appsettings.xml");
            //数据库连接字段
            XmlNode ResStatistics = document.SelectSingleNode("AppConfig").SelectSingleNode("ResStatistics");
            AppSetting.ResStatistic = new JObject();
            //属性定义字段
            foreach (XmlNode item in ResStatistics)
            {
                AppSetting.ResStatistic[item.Name] = item.InnerText;
            }

      }



    }
}
