using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using WebApiNew.Model;
using WebApiNew.StaticFunc;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ExportExcel : ControllerBase
    {
        [HttpPost]
        public IActionResult Result()
        {
            return Ok("daochu");
        }
    }
    //[Route("/[controller]")]
    //[ApiController]
    //public class ExportTest:ControllerBase { 
    //    [HttpPost]
    //    public IActionResult Result()
    //    {
    //        //延迟30分钟再可以提交
    //        MdataCenter mdataCenter = new MdataCenter();
    //        //DataTable dt = mdataCenter.WorkOrderData("20", "1",null,null,null);

    //        //创建Excel文件的对象  
    //        NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
    //        //添加一个sheet  
    //        NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet("Sheet1");
    //        //给sheet1添加第一行的头部标题  
    //        //NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
    //        ////row1.RowStyle.FillBackgroundColor = "";  
    //        //for (int i = 0; i < dt.Columns.Count; i++)
    //        //{
    //        //    row1.CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
    //        //}
    //        ////将数据逐步写入sheet1各个行
    //        //for (int i = 0; i < dt.Rows.Count; i++)
    //        //{
    //        //    NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
    //        //    for (int j = 0; j < dt.Columns.Count; j++)
    //        //    {
    //        //        rowtemp.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString().Trim());
    //        //    }
    //        //}
    //        //string strdate = DateTime.Now.ToString("yyyyMMddhhmmss");//获取当前时间  
    //        //// 写入到客户端   
    //        //System.IO.MemoryStream ms = new System.IO.MemoryStream();
    //        //book.Write(ms);
    //        //ms.Seek(0, SeekOrigin.Begin);
    //        //DateTime timer = DateTime.Now;
    //        ////xele.SetAttributeValue("content", timer);
    //        ////xele.Save(AppDomain.CurrentDomain.BaseDirectory + "Resource\\Config\\DatetimeConfig.xml");
    //        //return File(ms, "application/vnd.ms-excel", strdate + "Excel.xls");
    //    }
    //}
}