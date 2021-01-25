using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApiNew.StaticFunc;

namespace WebApiNew.PublicFunc
{
    public class PMPublicFuncs
    {
        public static void WriteLogs(string empID, string empName, string ipaddress, string model, DateTime time, string message, string webinfo)
        {
            //写入log
            SqlCommand cmd = PmConnections.CtrlCmd();
            cmd.Parameters.Add("@empID", SqlDbType.VarChar).Value = empID;
            cmd.Parameters.Add("@empName", SqlDbType.VarChar).Value = empName;
            cmd.Parameters.Add("@ipaddress", SqlDbType.VarChar).Value = ipaddress;
            cmd.Parameters.Add("@model", SqlDbType.VarChar).Value = model;
            cmd.Parameters.Add("@time", SqlDbType.DateTime).Value = time;
            cmd.Parameters.Add("@message", SqlDbType.VarChar).Value = message;
            cmd.Parameters.Add("@webinfo", SqlDbType.VarChar).Value = webinfo;
            cmd.CommandText = "insert into wapUserlog (empID,empName,ipAddress,model,logtime,logmessage,webinfomation) values (@empID,@empName,@ipaddress,@model,@time,@message,@webinfo)";
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }
        public static string GetMd5(string str)
        {
            MD5 md5str = MD5.Create();
            byte[] s = md5str.ComputeHash(Encoding.UTF8.GetBytes(str));
            md5str.Dispose();
            return Convert.ToBase64String(s);
        }
        public static string GetSafetyPass(string userName,string userPass)
        {
            MD5 md5 = MD5.Create();
            userPass += userName;
            string Pass = "";
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(userPass.Trim()));
            for (int i = 0; i < s.Length; i++)
            {
                Pass += s[i].ToString("X");
            }
            return Pass;
        }
    }
}
