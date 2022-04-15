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
    public class APIReplyData : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(APIReplyData).GetColumnAttrMapByProperty<ColumnAttribute>(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        [Column("建立者", -1)]
        public string Creator { get; set; }

        [Column("日期", -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", -1)]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        [Column("執行緒", "緒", -1)]
        public int ThreadID { get; set; }

        [Column("原始碼行號", "行", -1)]
        public int CallerLineNumber { get; set; }

        [Column("呼叫端方法或屬性名稱", -1)]
        public string CallerMemberName { get; set; }

        [Column("會員帳號", -1)]
        public string UserID { get; set; }

        [Column("訊息", -1)]
        public string Message { get; set; }

        public APIReplyData()
        {
            Creator = string.Empty;
            CreatedTime = DateTime.Now;
            ThreadID = 0;
            CallerLineNumber = 0;
            CallerMemberName = string.Empty;
            UserID = string.Empty;
            Message = string.Empty;
        }
    }
}
