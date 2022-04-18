using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class AppLogInDataGrid : AppLog
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(AppLogInDataGrid).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        [Column("建立者", CSVIndex = -1)]
        public override string Creator { get; set; }

        [Column("時間", WPFDisplayIndex = 0, WPFStringFormat = "{0:HH:mm:ss.fff}")]
        public override DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        [Column("等級", WPFDisplayIndex = 1)]
        public override string Level { get; set; }

        [Column("執行緒", "緒", WPFDisplayIndex = 2)]
        public int ThreadID { get; set; }

        [Column("訊息", WPFDisplayIndex = 5)]
        public override string Message { get; set; }

        [Column("原始碼行號", "行", WPFDisplayIndex = 3)]
        public int CallerLineNumber { get; set; }

        [Column("呼叫端方法或屬性名稱", WPFDisplayIndex = 4)]
        public string CallerMemberName { get; set; }

        public AppLogInDataGrid([CallerMemberName] string memberName = "") : base(memberName)
        {
            Creator = memberName;
            CreatedTime = DateTime.Now;
            Level = string.Empty;
            ThreadID = 0;
            Message = string.Empty;
            CallerLineNumber = 0;
            CallerMemberName = string.Empty;
        }
    }
}
