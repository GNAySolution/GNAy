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
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(AppLogInDataGrid).GetColumnAttrMapByProperty(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        [Column("執行緒", "緒", -1)]
        public int ThreadID { get; set; }

        /// <summary>
        /// 原始程式檔中的行號
        /// </summary>
        [Column("行號", "行", -1)]
        public int CallerLineNumber { get; set; }

        /// <summary>
        /// 呼叫端的方法或屬性名稱
        /// </summary>
        [Column("方法", -1)]
        public string CallerMemberName { get; set; }

        public AppLogInDataGrid()
        {
            ThreadID = 0;
            CallerLineNumber = 0;
            CallerMemberName = String.Empty;
        }
    }
}
