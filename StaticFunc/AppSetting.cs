﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiNew.StaticFunc
{
    public class AppSetting
    {
        public static JObject TableFileds { get; set; }
        public static string BasePath { get; set; }
        public static DataTable MapResTable { get; set; }
    }
}
