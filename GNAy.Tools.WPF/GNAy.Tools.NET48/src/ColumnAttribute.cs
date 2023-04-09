using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET48
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
        public WPFVisibility WPFVisibility { get; set; }
        public bool WPFCanUserReorder { get; set; }
        public bool WPFCanUserSort { get; set; }
        public WPFHorizontalAlignment WPFHorizontalAlignment { get; set; }
        public string WPFForeground { get; set; }

        public ColumnAttribute(string csvName, string wpfName)
        {
            CSVName = csvName;
            CSVIndex = 0;
            CSVStringFormat = string.Empty;

            WPFName = wpfName;
            WPFDisplayIndex = -1;
            WPFStringFormat = string.Empty;
            WPFIsReadOnly = true;
            WPFVisibility = WPFVisibility.Visible;
            WPFCanUserReorder = true;
            WPFCanUserSort = false;
            WPFHorizontalAlignment = WPFHorizontalAlignment.Left;
            WPFForeground = string.Empty; //"MediumBlue";
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
