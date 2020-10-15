using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WebApiNew.StaticFunc;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Middle
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    //这个插件是判断用户是否是登录的状态，如果不是登录的状态就返回301，必须存有缓存的用户
    public class HasLoginMiddleware
    {
        private readonly RequestDelegate _next;

        public HasLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string agent = httpContext.Request.Path.Value;
            if (agent.IndexOf("/login") !=-1 || agent.IndexOf("LogOut") !=-1 || agent.IndexOf("UserMessage") != -1)
            {
                await _next(httpContext);
            }
            else
            {
                string empid = httpContext.Request.Headers["empid"];
                string token = httpContext.Request.Headers["token"];
                string sysID = httpContext.Request.Headers["sysID"];
                if (String.IsNullOrEmpty(empid) || String.IsNullOrEmpty(token) || String.IsNullOrEmpty(sysID))
                {
                    await httpContext.Response.WriteAsync("301");
                }
                else
                {
                    SqlCommand cmd = PmConnections.CtrlCmd();
                    cmd.CommandText = "select COUNT(empID) FROM wapUserstate where empID = '" + empid + "' and userGuid = '" + token + "'";
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Connection.Close();
                    if (count > 0)
                    {
                        PMUser.EmpID = empid;
                        PMUser.UserGuid = token;
                        PMUser.UserSysID = sysID;
                        await _next(httpContext);
                    }
                    else
                    {
                        await httpContext.Response.WriteAsync("301");
                    }
                }
            }

        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class HasLoginMiddlewareExtensions
    {
        public static IApplicationBuilder UseHasLoginMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HasLoginMiddleware>();
        }
    }
}