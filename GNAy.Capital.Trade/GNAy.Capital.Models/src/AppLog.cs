﻿using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class AppLog : NotifyPropertyChanged
    {
        public static readonly Dictionary<string, string> PropertyDescriptionMap = typeof(AppLog).GetPropertyDescriptionMap();

        private string _creator;
        [Description("建立者")]
        public string Creator
        {
            get { return _creator; }
            set { OnPropertyChanged(ref _creator, value); }
        }

        private DateTime _createdDate;
        [Description("日期")]
        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { OnPropertyChanged(ref _createdDate, value); }
        }

        private DateTime _createdTime;
        [Description("時間")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertyChanged(ref _createdTime, value); }
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
            CreatedDate = DateTime.Today;
            CreatedTime = DateTime.Now;
            Project = String.Empty;
            Level = String.Empty;
            Message = String.Empty;
        }
    }
}
