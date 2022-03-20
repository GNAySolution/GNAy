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
    public class APIReplyData : AppLog
    {
        public static readonly Dictionary<string, (ColumnAttribute, PropertyInfo)> PropertyMap = typeof(APIReplyData).GetColumnAttrMapByProperty(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        private int _threadID;
        [Column("執行緒", "緒", -1)]
        public int ThreadID
        {
            get { return _threadID; }
            set { OnPropertyChanged(ref _threadID, value); }
        }

        private int _callerLineNumber;
        /// <summary>
        /// 原始程式檔中的行號
        /// </summary>
        [Column("行號", "行", -1)]
        public int CallerLineNumber
        {
            get { return _callerLineNumber; }
            set { OnPropertyChanged(ref _callerLineNumber, value); }
        }

        private string _callerMemberName;
        /// <summary>
        /// 呼叫端的方法或屬性名稱
        /// </summary>
        [Column("方法", -1)]
        public string CallerMemberName
        {
            get { return _callerMemberName; }
            set { OnPropertyChanged(ref _callerMemberName, value); }
        }

        private string _account;
        /// <summary>
        /// 呼叫端的方法或屬性名稱
        /// </summary>
        [Column("帳號", -1)]
        public string Account
        {
            get { return _account; }
            set { OnPropertyChanged(ref _account, value); }
        }

        public APIReplyData()
        {
            ThreadID = 0;
            CallerLineNumber = 0;
            CallerMemberName = String.Empty;
            Account = String.Empty;
        }
    }
}
