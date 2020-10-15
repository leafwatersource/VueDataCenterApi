using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiNew.Model;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ExportExcel : ControllerBase
    {
        [HttpPost]
        public IActionResult Result()
        {
            DataTable table = new DataTable();
            string path = MExportExcel.Excel(table, "测试.xlsx", "adfasdf");
            //计时30秒后删除文件
            Task.Run(() =>
            {
                Thread.Sleep(30000);
                MExportExcel.DelExcel("测试.xlsx");
            });
            return Ok(path);
        }
    }
}
