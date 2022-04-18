using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        public string CSVName { get; set; }
        public int CSVIndex { get; set; }
        public string CSVStringFormat { get; set; }

        public string WPFName { get; set; }
        public int WPFDisplayIndex { get; set; }
        public string WPFStringFormat { get; set; }
        public bool WPFIsReadOnly { get; set; }
        public int WPFVisibility { get; set; }
        public bool WPFCanUserReorder { get; set; }
        public bool WPFCanUserSort { get; set; }

        public ColumnAttribute(string csvName, string wpfName)
        {
            CSVName = csvName;
            CSVIndex = 0;
            CSVStringFormat = string.Empty;

            WPFName = wpfName;
            WPFDisplayIndex = -1;
            WPFStringFormat = string.Empty;
            WPFIsReadOnly = true;
            WPFVisibility = 0;
            WPFCanUserReorder = true;
            WPFCanUserSort = false;
        }

        public ColumnAttribute(string csvName) : this(csvName, csvName)
        {
            //
        }

        public ColumnAttribute() : this(string.Empty, string.Empty)
        {
            //
        }
    }
}
