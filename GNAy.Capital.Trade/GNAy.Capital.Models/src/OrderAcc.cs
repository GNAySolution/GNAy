using GNAy.Tools.NET47;
using GNAy.Tools.NET47.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    [Serializable]
    public class OrderAcc : NotifyPropertyChanged
    {
        [Column("市場", -1)]
        public string MarketKind { get; set; }

        [Column("分公司", -1)]
        public string Branch { get; set; }

        [Column("分公司代號", -1)]
        public string BranchCode { get; set; }

        [Column("帳號", -1)]
        public string Account { get; set; }

        [Column("身份證字號", -1)]
        public string Identity { get; set; }

        [Column("姓名", -1)]
        public string MemberName { get; set; }

        public string DisplayName => $"{Account},{Identity}";

        public string ToolTip => ToString();

        public OrderAcc()
        {
            MarketKind = String.Empty;
            Branch = String.Empty;
            BranchCode = String.Empty;
            Account = String.Empty;
            Identity = String.Empty;
            MemberName = String.Empty;
        }

        public override string ToString()
        {
            return string.Join(",", MarketKind, Branch, BranchCode, Account, Identity, MemberName);
        }
    }
}
