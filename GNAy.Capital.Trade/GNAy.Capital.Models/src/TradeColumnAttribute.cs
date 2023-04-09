using GNAy.Tools.NET48;
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
        public bool IsTrigger { get; set; }

        /// <summary>
        /// 欄位值(通常是DateTime)轉換為數值時的格式設定
        /// </summary>
        public string TriggerFormat { get; set; }

        public TradeColumnAttribute(string csvName, string wpfName) : base(csvName, wpfName)
        {
            IsTrigger = false;
            TriggerFormat = string.Empty;
        }

        public TradeColumnAttribute(string csvName) : this(csvName, csvName)
        {
            //
        }

        public TradeColumnAttribute() : this(string.Empty, string.Empty)
        {
            //
        }
    }
}
