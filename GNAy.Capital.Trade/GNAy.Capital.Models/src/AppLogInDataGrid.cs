using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class AppLogInDataGrid : AppLog
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(AppLogInDataGrid).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        [Column("執行緒", "緒", -1)]
        public int ThreadID { get; set; }

        [Column("原始碼行號", "行", -1)]
        public int CallerLineNumber { get; set; }

        [Column("呼叫端方法或屬性名稱", -1)]
        public string CallerMemberName { get; set; }

        public AppLogInDataGrid()
        {
            ThreadID = 0;
            CallerLineNumber = 0;
            CallerMemberName = string.Empty;
        }
    }
}
