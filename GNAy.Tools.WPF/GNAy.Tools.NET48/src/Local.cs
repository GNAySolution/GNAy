﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET48
{
    public class Local
    {
        public static Encoding Big5Encoding;

        public static readonly TaiwanCalendar TWCalendar;

        public static readonly CultureInfo TWCulture;

        static Local()
        {
            Big5Encoding = null;

            TWCalendar = new TaiwanCalendar();

            TWCulture = new CultureInfo("zh-TW");
            TWCulture.DateTimeFormat.Calendar = TWCalendar;
        }
    }
}
