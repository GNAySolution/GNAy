using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class TradeColumnAttribute : ColumnAttribute
    {
        /// <summary>
        /// 欄位值可以被設定觸價通知
        /// </summary>
        public bool Trigger { get; set; }

        public TradeColumnAttribute(string name, string shortName, int index) : base(name, shortName, index)
        {
            Trigger = false;
        }

        public TradeColumnAttribute(string name, int index) : this(name, name, index)
        {
            //
        }

        public TradeColumnAttribute() : this(string.Empty, string.Empty, -1)
        {
            //
        }
    }
}
