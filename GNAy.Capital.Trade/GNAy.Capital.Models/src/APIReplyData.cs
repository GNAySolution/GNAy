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
            set
            {
                if (OnPropertyChanged(ref _createdTime, value))
                {
                    OnPropertyChanged(nameof(CreatedDate));
                }
            }
        }

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

        /// <summary>
        /// 呼叫端的方法或屬性名稱
        /// </summary>
        [Column("帳號", -1)]
        public string Account { get; set; }

        [Column("訊息", -1)]
        public string Message { get; set; }

        public APIReplyData()
        {
            Creator = string.Empty;
            CreatedTime = DateTime.Now;
            ThreadID = 0;
            CallerLineNumber = 0;
            CallerMemberName = string.Empty;
            Account = string.Empty;
            Message = string.Empty;
        }
    }
}
