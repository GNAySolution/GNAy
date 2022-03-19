using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47.Models
{
    [Serializable]
    public class AppLog : NotifyPropertyChanged
    {
        private string _creator;
        [Column("建立者", -1)]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        [Column("日期", -1)]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間", -1)]
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
        [Column("專案", -1)]
        public string Project
        {
            get { return _project; }
            set { OnPropertyChanged(ref _project, value); }
        }

        private string _level;
        [Column("等級", -1)]
        public string Level
        {
            get { return _level; }
            set { OnPropertyChanged(ref _level, value); }
        }

        private string _message;
        [Column("訊息", -1)]
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
