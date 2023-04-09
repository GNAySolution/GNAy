using GNAy.Tools.NET48;
using GNAy.Tools.NET48.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class OrderAccData : NotifyPropertyChanged
    {
        [Column("建立者")]
        public string Creator { get; set; }

        [Column("日期")]
        public DateTime CreatedDate => CreatedTime.Date;

        private DateTime _createdTime;
        [Column("時間")]
        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { OnPropertiesChanged(ref _createdTime, value, nameof(CreatedTime), nameof(CreatedDate)); }
        }

        [Column("市場")]
        public Market.EType MarketType { get; set; }
        public string MarketName => Market.NameDescription[(int)MarketType];

        [Column("分公司")]
        public string Branch { get; set; }

        [Column("分公司代號")]
        public string BranchCode { get; set; }

        [Column("帳號")]
        public string Account { get; set; }

        [Column("身份證字號")]
        public string Identity { get; set; }

        [Column("姓名")]
        public string MemberName { get; set; }

        public string FullAccount => $"{Branch}{Account}";
        public string DisplayName => $"{Account},{MarketName},{Identity}";
        public string ToolTip => ToString();

        public OrderAccData([CallerMemberName] in string memberName = "")
        {
            Creator = memberName;
            CreatedTime = DateTime.Now;
            MarketType = Market.EType.Option;
            Branch = string.Empty;
            BranchCode = string.Empty;
            Account = string.Empty;
            Identity = string.Empty;
            MemberName = string.Empty;
        }

        public override string ToString()
        {
            return string.Join(",", MarketType, Branch, BranchCode, Account, Identity, MemberName);
        }
    }
}
