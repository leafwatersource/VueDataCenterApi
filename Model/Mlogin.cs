using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApiNew.PublicFunc;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Model
{
    public class LoginMessage
    {
        private string loginState;
        public string LoginState { get { return loginState; } set { loginState = value; } }
        private string message;
        public string Message { get { return message; } set { message = value; } }
        private string empid;
        public string EmpID { get { return empid; } set { empid = value; } }
        private string empName;
        public string EmpName { get { return empName; } set { empName = value; } }
        private string userguid;
        public string UserGuid { get { return userguid; } set { userguid = value; } }
    }
    public class Mlogin
    {
        public LoginMessage LoginMessage()
        {
            List<string> loginInfo = new List<string>();
            string empName = GetempName(PMUser.EmpID);
            int errortimes = 0;
            DateTime errortime = new DateTime();
            SqlCommand cmd = PmConnections.CtrlCmd();

            cmd.CommandText = "select *  from wapUserstate where empID = '" + PMUser.EmpID + "'";
            DataTable DtuserState = new DataTable();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
            dataAdapter.Fill(DtuserState);
            dataAdapter.Dispose();
            cmd.Connection.Close();
            LoginMessage loginMessage = new LoginMessage();
            if (DtuserState.Rows.Count > 0)
            {
                errortimes = Convert.ToInt32(DtuserState.Rows[0]["errortimes"]);
                errortime = Convert.ToDateTime(DtuserState.Rows[0]["errortime"]);
                string online = DtuserState.Rows[0]["online"].ToString();
                string ipaddress = DtuserState.Rows[0]["userIpaddress"].ToString();
                if (online == "0")
                {
                    //如果已经大于5分钟了，删除数据库记录
                    if ((DateTime.Now - errortime).Minutes > 5)
                    {
                        cmd = PmConnections.CtrlCmd();
                        cmd.CommandText = "delete from wapUserstate where empID = '" + PMUser.EmpID + "' and online = '0'";
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                    }
                }
                else
                {
                    loginMessage.LoginState = "2";
                    loginMessage.Message = "用户已经在IP:" + ipaddress + " 上登陆。";
                    loginMessage.EmpID = PMUser.EmpID;
                    loginMessage.EmpName = empName;
                    return loginMessage;
                }
            }

            //判断用户是否被锁定
            if (errortimes < 3)
            {
                cmd = PmConnections.ModCmd();
                cmd.CommandText = "select * from wapEmpList where empID = '" + PMUser.EmpID + "'";
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read() != true)
                {
                    loginMessage.LoginState = "0";
                    loginMessage.Message = "登陆失败，没有这个用户名。";
                    loginMessage.EmpID = PMUser.EmpID;
                    loginMessage.EmpName = empName;
                    reader.Close();
                }
                else
                {
                    //存在用户名，验证密码
                    string dbpass = reader["password"].ToString();
                    PMUser.UserSysID = reader["sysID"].ToString();
                    reader.Close();
                    if (PMUser.UserPass != dbpass)
                    {
                        //如果密码错误，去查询库里是不是第一次错误，如果是，计入错误记录和计数                 
                        DataRow[] dr;
                        if ((dr = DtuserState.Select("empID = '" + PMUser.EmpID + "'")).Count() > 0)
                        {
                            errortimes = Convert.ToInt32(dr[0][6]) + 1;
                            cmd = PmConnections.CtrlCmd();
                            cmd.CommandText = "update wapUserstate set userpass = '" + PMUser.UserPass + "',errortimes = '" + errortimes + "', errorTime = '" + DateTime.Now + "' where empID = '" + PMUser.EmpID + "'";
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                        else
                        {
                            errortimes = 1;
                            cmd = PmConnections.CtrlCmd();
                            cmd.CommandText = "insert into wapUserstate (empID,empName,userPass,userIpaddress,onLine,errorTimes,errorTime,message) values ('" + PMUser.EmpID + "','" + empName + "','" + PMUser.UserPass + "','" + PMUser.UserIpAdress + "','0','" + errortimes + "','" + DateTime.Now + "','用户密码错误')";
                            cmd.ExecuteNonQuery();
                            cmd.Connection.Close();
                        }
                        if (errortimes <= 3)
                        {
                            loginMessage.LoginState = "0";
                            int interr = 3 - errortimes;
                            if (interr != 0)
                            {
                                loginMessage.Message = "用户密码错误！再输入" + (3 - errortimes).ToString() + "次错误密码后，账号将被锁定5分钟。";
                            }
                            else
                            {
                                loginMessage.Message = "用户被锁定，请在" + (3000 - (DateTime.Now - errortime).TotalSeconds).ToString() + "秒后登陆。";
                            }
                            loginMessage.EmpID = PMUser.EmpID;
                            loginMessage.EmpName = empName;
                            loginInfo.Add(PMUser.EmpID);
                            loginInfo.Add(empName);
                        }
                    }
                    else
                    {
                        cmd = PmConnections.CtrlCmd();
                        string userguid = Guid.NewGuid().ToString();
                        //查询是否有相同登陆记录，如果有，是否推出。
                        if ((_ = DtuserState.Select("empID = '" + PMUser.EmpID + "'")).Count() > 0)
                        {
                            cmd.CommandText = "update wapUserstate set userpass = '" + PMUser.UserPass + "',errortimes = '0',errortime = '" + DateTime.Now + "',online = '1',message = '登陆成功',userGuid = '" + userguid + "',useripaddress = '" + PMUser.UserIpAdress + "' where empID = '" + PMUser.EmpID + "'";
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            cmd.CommandText = "insert into wapUserstate (empID,empName,userPass,userIpaddress,onLine,errorTimes,errorTime,message,userGuid) values ('" + PMUser.EmpID + "','" + empName + "','" + PMUser.UserPass + "','" + PMUser.UserIpAdress + "','1','0','" + DateTime.Now + "','登陆成功','" + userguid + "')";
                            cmd.ExecuteNonQuery();
                        }
                        cmd.Connection.Close();
                        loginMessage.LoginState = "1";
                        loginMessage.Message = "登陆成功！";
                        loginMessage.EmpID = PMUser.EmpID;
                        loginMessage.EmpName = empName;
                        loginMessage.UserGuid = userguid;
                    }
                }
            }
            else
            {
                loginMessage.LoginState = "0";
                loginMessage.Message = "用户被锁定，请在" + (3000 - (DateTime.Now - errortime).TotalSeconds).ToString() + "秒后登陆。";
                loginMessage.EmpID = PMUser.EmpID;
                loginMessage.EmpName = empName;
            }
            return loginMessage;
        }
        public string GetempName(string empid)
        {

            string empname = string.Empty;
            SqlCommand cmd = PmConnections.ModCmd();
            cmd.CommandText = "select empName from wapEmpList where empID = '" + empid + "'";
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                empname = rd["empName"].ToString();
            }
            rd.Close();
            cmd.Connection.Close();
            return empname;
        }
        public List<string> GetuserGroup(string empID)
        {
            List<string> tmp = new List<string>();
            SqlCommand cmd = PmConnections.ModCmd();
            cmd.CommandText = "select userName from wapEmpUserMap where empID = '" + empID + "'";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable DtUsermap = new DataTable();
            da.Fill(DtUsermap);
            da.Dispose();

            cmd.CommandText = "select *  from  wapUser";
            da = new SqlDataAdapter(cmd);
            DataTable DtWapuser = new DataTable();
            da.Fill(DtWapuser);
            da.Dispose();
            cmd.Connection.Close();

            if (DtUsermap.Rows.Count > 0)   //一定会有数据，防呆
            {
                foreach (DataRow item in DtUsermap.Rows)
                {
                    string username = item["userName"].ToString();
                    DataRow[] dr = DtWapuser.Select("userName = '" + username + "'");
                    if (dr.Count() > 0)
                    {
                        string addstring = dr[0][1].ToString();
                        if (tmp.Contains(addstring) == false)
                        {
                            tmp.Add(addstring);
                        }
                    }
                }
            }
            return tmp;
        }
        public LoginMessage LoginState(string userName,string userPass, string adminstate,string UserIpAdress,string UserWeb)
        {
            string safePass = PMPublicFuncs.GetSafetyPass(userName, userPass);
            User user = new User();
            user.EmpID = userName;
            user.UserPass = safePass;
            user.UserIpAdress = UserIpAdress;
            user.UserWeb = UserWeb;
            PMUser.EmpID = userName;
            PMUser.UserPass = safePass;
            PMUser.UserIpAdress = UserIpAdress;
            PMUser.UserWeb = UserWeb;
            LoginMessage userMsg = LoginMessage();
            if (userMsg.LoginState == "1")
            {
                List<string> userGroup = GetuserGroup(userName);
                if (userGroup.Count < 1)
                {
                    userMsg.LoginState = "0";
                    userMsg.Message = "该员工没有分配用户组，请联系管理员分配。";
                }
                else
                {
                    user.UserGuid = userMsg.UserGuid;
                    user.UserName = GetempName(userName);

                    PMUser.UserGuid = userMsg.UserGuid;
                    PMUser.UserName = GetempName(userName);
                    //Response.Cookies.Append("EmpID", user.EmpID);
                    //Response.Cookies.Append("EmpID", PMUser.EmpID);
                    //Response.Cookies.Append("UserGuid", PMUser.UserGuid);
                }

                if (adminstate == "1")
                {
                    if (userGroup.Contains("ADMIN") == false)
                    {
                        userMsg.LoginState = "0";
                        userMsg.Message = "请不要使用非管理员账户越权操作!";
                        PMPublicFuncs.WriteLogs(userName, GetempName(userName), PMUser.UserIpAdress, "越权登陆", DateTime.Now, "用户越权使用管理员登陆。", PMUser.UserWeb);
                    }
                    else
                    {
                        string md5Guid = Guid.NewGuid().ToString();
                        //Response.Cookies.Append("MD5", PMPublicFuncs.GetMd5("ADMIN" + md5Guid));
                        //Response.Cookies.Append("MD5", PMPublicFuncs.GetMd5("ADMIN" + md5Guid), new CookieOptions() { IsEssential = true });
                        PMPublicFuncs.WriteLogs(userName, GetempName(userName), PMUser.UserIpAdress, "管理员登录", DateTime.Now, "管理员登陆成功。", PMUser.UserWeb);
                        //管理员登录成功
                    }
                }
                else
                {
                    //判断该用户具有的功能模块权限，如果只有一个权限，直接跳入页面，如果有多个权限，给出选择
                    if (PMUser.FunctionList == null)
                    {
                        PMUser.FunctionList = new List<string>();
                    }
                    PMUser.FunctionList.Clear();
                    foreach (string item in userGroup)
                    {
                        if (item == "ADMIN")
                        {
                            PMUser.FunctionList.Add("datacenter");
                            break;
                        }
                        else if (item == "CFM")
                        {
                            PMUser.FunctionList.Add("systemsetting");
                        }
                        else if (item == "REP")
                        {
                            PMUser.FunctionList.Add("reportsystem");
                        }
                        else if (item == "VIEW")
                        {
                            PMUser.FunctionList.Add("datacenter");
                        }
                        else if (item == "BOARD")
                        {
                            PMUser.FunctionList.Add("planboard");
                        }
                    }
                    PMUser.UserMessage.Add(user);
                    //登录成功
                    PMPublicFuncs.WriteLogs(userName, GetempName(userName), PMUser.UserIpAdress, "用户登陆", DateTime.Now, "用户登陆成功。", PMUser.UserWeb);
                }
            }
            return userMsg;
        }
        public int ForceLogout(string empID)
        {
            SqlCommand cmd = PmConnections.CtrlCmd();
            PMUser.UserName = GetempName(empID);
            cmd.CommandText = "delete from wapUserstate where empID = '" + empID + "'";
            int state = cmd.ExecuteNonQuery();
            PMPublicFuncs.WriteLogs(empID, PMUser.UserName, PMUser.UserIpAdress, "强制登出", DateTime.Now, "用户选择强制登出。", PMUser.UserWeb);
            return state;
        }
        
        public DataTable GetUserMessage()
        {
            DataTable table = new DataTable();
            SqlCommand cmd = PmConnections.ModCmd();
            cmd.CommandText = "SELECT empID,empWorkID,empName,dept,phoneNum,email,sysID,cusID,shopUserGroupID FROM wapEmpList where empID = '" + PMUser.EmpID + "'";
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            ad.Fill(table);
            ad.Dispose();
            cmd.Connection.Close();
            return table;
        }
        public JObject UserHasLogin(string empid,string userGuid)
        {
            
            JObject data = new JObject();
            SqlCommand cmd = PmConnections.CtrlCmd();
            cmd.CommandText = "select count(empid) from wapUserstate where empID = '" + empid + "' and userGuid = '" + userGuid + "'";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.Connection.Close();
            if (count==1)
            {
                data.Add("LoginStatus", 1);
            }
            else
            {
                data.Add("LoginStatus", 0);
            }
            return data;
        }
    }
}
