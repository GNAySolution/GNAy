using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47.Models
{
    [Serializable]
    public class AppLog : NotifyPropertyChanged
    {
        private string _creator;
        [Description("建立者")]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Description("日期")]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Description("時間")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set
            {
                OnPropertyChanged(ref _createdTime, value);
                OnPropertyChanged("CreatedDate");
            }
        }

        private string _project;
        [Description("專案")]
        public string Project
        {
            get { return _project; }
            set { OnPropertyChanged(ref _project, value); }
        }

        private string _level;
        [Description("等級")]
        public string Level
        {
            get { return _level; }
            set { OnPropertyChanged(ref _level, value); }
        }

        private string _message;
        [Description("訊息")]
        public string Message
        {
            get { return _message; }
            set { OnPropertyChanged(ref _message, value); }
        }

        public AppLog()
        {
            Creator = String.Empty;
            CreatedTime = DateTime.Now;
            Project = String.Empty;
            Level = String.Empty;
            Message = String.Empty;
        }
    }
}
