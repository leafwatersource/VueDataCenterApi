using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiNew.StaticFunc
{
    public class PmConnections
    {
        /// <summary>
        /// 三个字段是连接数据
        /// </summary>
        public static string Modconnstr { get; set; }
        public static string Schconnstr { get; set; }
        public static string Ctrlconnstr { get; set; }
        public static SqlCommand SchCmd()
        {
            SqlConnection conn = new SqlConnection(Schconnstr);
            SqlCommand cmd = null;
            if (conn.State == ConnectionState.Closed)
            {
                try
                {
                    conn.Open();
                    cmd = conn.CreateCommand();
                }
                catch (Exception e)
                {
                }
            }
            return cmd;
        }
        //Modeler 数据库cmd；
        public static SqlCommand ModCmd()
        {
            SqlConnection conn = new SqlConnection(Modconnstr);
            SqlCommand cmd = new SqlCommand();
            if (conn.State == ConnectionState.Closed)
            {
                try
                {
                    conn.Open();
                    cmd = conn.CreateCommand();
                }
                catch (SqlException e)
                {
                }

            }
            return cmd;
        }
        //Control 数据库cmd；
        public static SqlCommand CtrlCmd()
        {
            SqlConnection conn = new SqlConnection(Ctrlconnstr);
            SqlCommand cmd = null;
            if (conn.State == ConnectionState.Closed)
            {
                try
                {
                    conn.Open();
                    cmd = conn.CreateCommand();
                }
                catch (SqlException e)
                {

                }
            }
            return cmd;
        }
    }
}