using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class TradeType
    {
        /// <summary>
        /// https://www.sinotrade.com.tw/richclub/freshman/%E5%8F%B0%E8%82%A1%E4%B8%8B%E5%96%AE%E5%8A%9F%E8%83%BD-ROD---IOC---FOK-%E6%9C%89%E7%94%9A%E9%BA%BC%E5%B7%AE%E5%88%A5-%E5%A6%82%E4%BD%95%E8%B2%B7%E8%82%A1%E7%A5%A8%E7%9C%8B%E9%80%99%E7%AF%87%E5%B0%B1%E6%87%82---2022%E5%B9%B4%E6%9B%B4%E6%96%B0-%E6%A2%9D%E4%BB%B6%E5%96%AE%E5%8A%9F%E8%83%BD---60ebea84aefbe326a0c75390
        /// </summary>
        public enum Enum : short
        {
            /// <summary>
            /// 當日有效(Rest of Day)
            /// </summary>
            [Description("ROD")]
            ROD, //0

            /// <summary>
            /// 立即成交否則取消(Immediate-or-Cancel)
            /// </summary>
            [Description("IOC")]
            IOC, //1

            /// <summary>
            /// 全部成交否則取消(Fill-or-Kill)
            /// </summary>
            [Description("FOK")]
            FOK, //2
        }

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.ROD.GetDescription(),
            Enum.IOC.GetDescription(),
            Enum.FOK.GetDescription(),
        }.AsReadOnly();
    }
}
