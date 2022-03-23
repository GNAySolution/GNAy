﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47.Models
{
    [Serializable]
    public class AppLog : NotifyPropertyChanged
    {
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
                OnPropertyChanged(ref _createdTime, value);
                OnPropertyChanged(nameof(CreatedDate));
            }
        }

        [Column("專案", -1)]
        public string Project { get; set; }

        [Column("等級", -1)]
        public string Level { get; set; }

        [Column("訊息", -1)]
        public string Message { get; set; }

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
