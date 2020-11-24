using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Intercom.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WebApiNew.Model;

namespace WebApiNew.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ExportExcel : ControllerBase
    {
        [HttpPost]
        public  IActionResult Result()
        {
            string fileName = $"{Guid.NewGuid().ToString()}.xlsx";

            //store in memory rather than pysical directory
            var stream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");//创建worksheet
                package.Save();
            // add worksheet
            //ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Conversation Message");
                //add head
                worksheet.Cells[1, 1].Value = "From Id";
                worksheet.Cells[1, 2].Value = "To Id";
                worksheet.Cells[1, 3].Value = "Message";
                worksheet.Cells[1, 4].Value = "Time";
                worksheet.Cells[1, 5].Value = "Attachment";
                worksheet.Cells[1, 6].Value = "Conversation Id";
                worksheet.Cells[2,1].Value = "test";
                package.Save();
            }
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}