﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Description("設定檔")]
    [Serializable]
    public class AppSettings
    {
        public string Version { get; set; }
        public string Description { get; set; }

        public int Big5EncodingCodePage { get; set; }
        public int DataGridAppLogRowsMax { get; set; }
        public int TimerInterval { get; set; }

        [Description("排程啟動自動執行")]
        public bool AutoRun { get; set; }

        [Description("測試或跑回測時，不會實際下單")]
        public bool SendOrder { get; set; }

        public AppSettings()
        {
            Version = "0.22.308.1";
            Description = "測試用設定";

            Big5EncodingCodePage = 950; //"big5"
            DataGridAppLogRowsMax = 500;
            TimerInterval = 50;

            AutoRun = false;

            SendOrder = false;
        }
    }
}
