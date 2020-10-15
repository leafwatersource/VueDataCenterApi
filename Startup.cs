using System;
using System.Collections.Generic;
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
            JObject temp;
            AppSetting.TableFileds = new JObject();
            foreach (XmlNode item in TableFiledsConfigList)
            {
                XmlNodeList xl = item.ChildNodes;
                temp = new JObject();
                foreach (XmlNode filed in xl)
                {
                    temp.Add(filed.Name, filed.InnerText);
                }
                AppSetting.TableFileds.Add(item.Name.ToString(), temp);
            }

            foreach (XmlNode item in ConnectionList)
            {
                if (item.Name.ToLower() == "modeler") {
                    PmConnections.Modconnstr = "Data Source=" + datasource + ";Initial Catalog=" + item.InnerText + ";" + item.Attributes["security"].Value + ";User ID=" + item.Attributes["username"].Value + ";Password=" + item.Attributes["password"].Value + ";" + item.Attributes["muti"].Value;
                    PmConnections.ModName = item.InnerText+".dbo";
                }
                else if (item.Name.ToLower() == "schedule")
                {
                    PmConnections.Schconnstr = "Data Source=" + datasource + ";Initial Catalog=" + item.InnerText + ";" + item.Attributes["security"].Value + ";User ID=" + item.Attributes["username"].Value + ";Password=" + item.Attributes["password"].Value + ";" + item.Attributes["muti"].Value;
                    PmConnections.SchName = item.InnerText + ".dbo";
                }
                else
                {
                    PmConnections.Ctrlconnstr = "Data Source=" + datasource + ";Initial Catalog=" + item.InnerText + ";" + item.Attributes["security"].Value + ";User ID=" + item.Attributes["username"].Value + ";Password=" + item.Attributes["password"].Value + ";" + item.Attributes["muti"].Value;
                    PmConnections.CtrlName = item.InnerText + ".dbo";
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
    }
}
