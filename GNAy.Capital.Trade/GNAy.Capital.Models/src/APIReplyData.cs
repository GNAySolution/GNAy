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
    public class APIReplyData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(APIReplyData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        [Column("建立者", CSVIndex = -1)]
        public string Creator { get; set; }

        [Column("日期", CSVIndex = -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", WPFDisplayIndex = 0, WPFStringFormat = "{0:HH:mm:ss.fff}")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        [Column("執行緒", "緒", WPFDisplayIndex = 1)]
        public int ThreadID { get; set; }

        [Column("會員帳號", WPFDisplayIndex = 4)]
        public string UserID { get; set; }

        [Column("訊息", WPFDisplayIndex = 5)]
        public string Message { get; set; }

        [Column("原始碼行號", "行", WPFDisplayIndex = 2)]
        public int CallerLineNumber { get; set; }

        [Column("呼叫端方法或屬性名稱", WPFDisplayIndex = 3)]
        public string CallerMemberName { get; set; }

        public APIReplyData([CallerMemberName] string memberName = "")
        {
            Creator = memberName;
            CreatedTime = DateTime.Now;
            ThreadID = 0;
            UserID = string.Empty;
            Message = string.Empty;
            CallerLineNumber = 0;
            CallerMemberName = string.Empty;
        }
    }
}
