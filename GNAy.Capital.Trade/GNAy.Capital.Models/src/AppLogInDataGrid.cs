using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class AppLogInDataGrid : AppLog
    {
        public static readonly Dictionary<string, string> PropertyDescriptionMap = typeof(AppLogInDataGrid).GetPropertyDescriptionMap(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        private int _threadID;
        /// <summary>
        /// 執行緒
        /// </summary>
        [Description("緒")]
        public int ThreadID
        {
            get { return _threadID; }
            set { OnPropertyChanged(ref _threadID, value); }
        }

        private int _callerLineNumber;
        /// <summary>
        /// 原始程式檔中的行號
        /// </summary>
        [Description("行")]
        public int CallerLineNumber
        {
            get { return _callerLineNumber; }
            set { OnPropertyChanged(ref _callerLineNumber, value); }
        }

        private string _callerMemberName;
        /// <summary>
        /// 呼叫端的方法或屬性名稱
        /// </summary>
        [Description("方法")]
        public string CallerMemberName
        {
            get { return _callerMemberName; }
            set { OnPropertyChanged(ref _callerMemberName, value); }
        }

        public AppLogInDataGrid()
        {
            ThreadID = 0;
            CallerLineNumber = 0;
            CallerMemberName = String.Empty;
        }
    }
}
