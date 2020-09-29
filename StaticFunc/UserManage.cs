using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiNew.StaticFunc
{
    public class UserManage
    {
        public class User
        {
            public string EmpID { get; set; }
            public string UserName { get; set; }
            public string UserPass { get; set; }
            public string UserWeb { get; set; }
            public string UserIpAdress { get; set; }
            public string UserGuid { get; set; }
            public string UserSysID { get; set; }
        }
        public class PMUser
        {
            public static string EmpID { get; set; }
            public static string UserName { get; set; }
            public static string UserPass { get; set; }
            public static string UserWeb { get; set; }
            public static string UserIpAdress { get; set; }
            public static string UserGuid { get; set; }
            public static string UserSysID { get; set; }
            public static List<User> UserMessage = new List<User>();
            public static List<string> FunctionList;
            public static string PMPlState{get;set;}
           
            public static string PMOcState { get; set; }
            public static string GetuserGuid(string empID)
            {
                SqlCommand cmd = PmConnections.CtrlCmd();
                cmd.Parameters.Add("@EmpID", SqlDbType.VarChar).Value = empID;
                cmd.CommandText = "select userGuid from wapUserstate where empID = @EmpID";
                SqlDataReader rd = cmd.ExecuteReader();
                rd.Read();
                string userguid = rd[0].ToString();
                rd.Close();
                cmd.Connection.Dispose();
                return userguid;
            }
        }
    }
}
