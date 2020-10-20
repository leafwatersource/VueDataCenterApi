using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApiNew.Model;
using WebApiNew.PublicFunc;
using static WebApiNew.Model.Mlogin;
using static WebApiNew.StaticFunc.UserManage;

namespace WebApiNew.Controllers
{
    /// <summary>
    /// 登录
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        Mlogin login;
        [HttpGet]
        [HttpPost]
        public IActionResult Result([FromForm] string empID, [FromForm] string pwd, [FromForm] string adminstate)
        {
            if (login == null)
            {
                login = new Mlogin();
            }
            string UserIpAdress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            string UserWeb = Request.Headers["User-Agent"];
            return Ok(login.LoginState(empID, pwd, adminstate, UserIpAdress, UserWeb));
        }
    }

    /// <summary>
    /// 强制登录
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class LogOut : ControllerBase
    {
        Mlogin login;
        [HttpPost]
        public IActionResult Result([FromForm] string empID, [FromForm] string userPass, [FromForm] string adminstate)
        {
            if (login == null)
            {
                login = new Mlogin();
            }
            PMUser.EmpID = empID;
            PMUser.UserPass = userPass;
            PMUser.UserIpAdress = HttpContext.Connection.LocalIpAddress.ToString();
            PMUser.UserWeb = Request.Headers["User-Agent"];
            if (login.ForceLogout(empID) == 1)
            {
                string UserIpAdress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string UserWeb = Request.Headers["User-Agent"];
                if (PMUser.EmpID != null)
                {
                    string aaa = PMUser.EmpID;
                    Response.Cookies.Append("EmpID", PMUser.EmpID);
                }
                if (PMUser.UserGuid != null)
                {
                    Response.Cookies.Append("token", PMUser.UserGuid);
                }
                return Ok(login.LoginState(empID, userPass, adminstate, UserIpAdress, UserWeb));
            }
            else
            {
                LoginMessage errMessage = new LoginMessage();
                errMessage.LoginState = "0";
                errMessage.Message = "强制退出失败，请联系管理员。";
                errMessage.EmpID = login.GetempName(empID);
                return Ok(errMessage);
            }
        }
    }
    /// <summary>
    /// 获取用户信息
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class UserMessage:ControllerBase{
        [HttpPost]
        public IActionResult Result()
        {
            Mlogin mlogin = new Mlogin();
            return Ok(JsonConvert.SerializeObject(mlogin.GetUserMessage()));
        }
    }
}
