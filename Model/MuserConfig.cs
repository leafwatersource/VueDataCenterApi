using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using WebApiNew.StaticFunc;
namespace WebApiNew.Model
{
    public class MuserConfig
    {
        /// <summary>
        /// 修改前端表格的属性，后端的appsetting.xml文件
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="changeData">修改的数据</param>
        /// <param name="targetChange">数据库对应的字段</param>
        public void setUserTable(string tableName, string changeData, string targetChange)
        {
            XmlDocument document = new XmlDocument();
            JObject targetChangeData = JObject.Parse(changeData);
            document.Load(AppSetting.BasePath + "appsettings.xml");
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("TableFileds");//订单查询、执行计划、设备使用记录的列
            XmlElement TargetTable = (XmlElement)TableFileConfig.SelectSingleNode(tableName).SelectSingleNode("filedItem[@name='" + targetChange + "']");
            TargetTable.InnerText = targetChangeData.GetValue("showNameT").ToString();
            TargetTable.SetAttribute("name", targetChangeData["sqlNameT"].ToString());
            TargetTable.SetAttribute("width", targetChangeData["width"].ToString());
            TargetTable.SetAttribute("filter", targetChangeData["switchNameT"].ToString());
            TargetTable.SetAttribute("indexNameT", targetChangeData["indexNameT"].ToString());
            document.Save(AppSetting.BasePath + "appsettings.xml");
            renderTable();
        }
        /// <summary>
        /// 重新渲染内存里面的对象
        /// </summary>
        public void renderTable()
        {
            AppSetting.TableFileds = new JObject();
            XmlDocument document = new XmlDocument();
            document.Load(AppSetting.BasePath + "appsettings.xml");
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("TableFileds");//订单查询、执行计划、设备使用记录的列
            XmlNodeList TableFiledsConfigList = TableFileConfig.ChildNodes;
            List<JObject> temp;
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
                    temp.Append(props);
                    temp.Add(props);
                }
                AppSetting.TableFileds.Add(item.Name.ToString(), JsonConvert.SerializeObject(temp));
            }
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
        /// 增加节点
        /// </summary>
        /// <param name="newData">新增的数据</param>
        /// <param name="tableName">表格的名称</param>
        public void addTableColumn(string newData,string tableName)
        {
            XmlDocument document = new XmlDocument();
            document.Load(AppSetting.BasePath + "appsettings.xml");
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("TableFileds");//订单查询、执行计划、设备使用记录的列
            XmlNode TargetNode = TableFileConfig.SelectSingleNode(tableName);
            XmlElement filedItem = document.CreateElement("filedItem");
            JObject newColumn = JObject.Parse(newData);
            filedItem.SetAttribute("type", newColumn["type"].ToString());
            filedItem.SetAttribute("name", newColumn["sqlNameT"].ToString());
            filedItem.SetAttribute("width", newColumn["width"].ToString());
            filedItem.InnerText = newColumn["showNameT"].ToString();
            TargetNode.AppendChild(filedItem);
            document.Save(AppSetting.BasePath + "appsettings.xml");
            renderTable();
     }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="tableName">表格名称</param>
        /// <param name="delData">删除的数据</param>
        public void delTableColumn(string tableName,string delData) 
        {
            XmlDocument document = new XmlDocument();
            document.Load(AppSetting.BasePath + "appsettings.xml");
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("TableFileds");//订单查询、执行计划、设备使用记录的列
            XmlNode targetTable = TableFileConfig.SelectSingleNode(tableName);
            XmlElement targetRow = (XmlElement)targetTable.SelectSingleNode("filedItem[@name='" + delData + "']");
            targetTable.RemoveChild(targetRow);
            document.Save(AppSetting.BasePath + "appsettings.xml");
            renderTable();
        }
        public void changeSwitchVal(string val)
        {

            XmlDocument document = new XmlDocument();
            document.Load(AppSetting.BasePath + "appsettings.xml");
            XmlNode TableFileConfig = document.SelectSingleNode("AppConfig").SelectSingleNode("PMSettings");//订单查询、执行计划、设备使用记录的列
            XmlElement TargetEl = (XmlElement)TableFileConfig.SelectSingleNode("changePlan");
            TargetEl.InnerText = val;
            AppSetting.ChangePlan = Convert.ToBoolean(val);
            document.Save(AppSetting.BasePath + "appsettings.xml");
        }
    }
}
