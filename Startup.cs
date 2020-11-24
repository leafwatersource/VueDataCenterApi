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
            AppSetting.BasePath = AppContext.BaseDirectory;
            XmlDocument document = new XmlDocument();
            document.Load(filepach + "appsettings.xml");
            //数据库连接字段
            XmlNode Connection = document.SelectSingleNode("AppConfig").SelectSingleNode("Connection");
            //属性定义字段
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("TableFileds");
            XmlNodeList TableFiledsConfigList = TableFileConfig.ChildNodes;
            XmlNodeList ConnectionList = Connection.ChildNodes;
            XmlNode Pmsetting = document.SelectSingleNode("AppConfig").SelectSingleNode("PMSettings");
            XmlNodeList SettingList = Pmsetting.ChildNodes;
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
                        filed.Name,filed.InnerText
                        },{
                        "type",filed.Attributes["type"].Value
                        } };
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
                string bbb = item.Name.ToLower();
                if (item.Name.ToLower() == "plstate")
                {
                    string aaa = item.InnerText;
                    PMUser.PMPlState = item.InnerText;
                }
                if (item.Name.ToLower() == "ocstate")
                {
                    PMUser.PMOcState = item.InnerText;
                }
            }
            RenderMapResTable();
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

            app.UseHttpsRedirection();

            app.UseCors("any");

            app.UseRouting();
            app.UseMiddleware<HasLoginMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

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
        public void SetMapResXML(XmlDocument xmlDoc,DataTable MapResTable) {
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
    }
}
