using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public class OrderPrice
    {
        public enum Enum
        {
            [Description("市價")]
            M, //0

            [Description("範圍市價")]
            P, //1

            [Description("參考價")]
            R, //2

            [Description("開盤價")]
            S, //3

            [Description("最高價")]
            H, //4

            [Description("最低價")]
            L, //5

            [Description("前盤收盤價")]
            E, //6
        }

        public static readonly string M = Enum.M.ToString();
        public static readonly string P = Enum.P.ToString();
        public static readonly string R = Enum.R.ToString();
        public static readonly string S = Enum.S.ToString();
        public static readonly string H = Enum.H.ToString();
        public static readonly string L = Enum.L.ToString();
        public static readonly string E = Enum.E.ToString();

        public static readonly string[] Description =
        {
            Enum.M.GetDescription(),
            Enum.P.GetDescription(),
            Enum.R.GetDescription(),
            Enum.S.GetDescription(),
            Enum.H.GetDescription(),
            Enum.L.GetDescription(),
            Enum.E.GetDescription(),
        };

        private static (string, decimal) Parse(in string orderPrice, in decimal marketPrice, in decimal reference)
        {
            decimal newPri = 0;

            if (orderPrice.EndsWith("%"))
            {
                decimal offsetPct = decimal.Parse(orderPrice.Substring(1, orderPrice.Length - 2));
                offsetPct /= 100;
                newPri = marketPrice + reference * offsetPct;

                return (newPri.ToString("0.00"), newPri);
            }

            decimal offset = decimal.Parse(orderPrice.Substring(1));
            newPri = marketPrice + offset;

            return (newPri.ToString("0.00"), newPri);
        }

        public static (string, decimal) Parse(in string orderPrice, in decimal marketPrice, in decimal reference, in decimal startPrice, in decimal highPrice, in decimal lowPrice, in decimal lastEndPrice)
        {
            switch (orderPrice[0])
            {
                case 'M':
                case 'P':
                    return (orderPrice.Length > 1) ? Parse(orderPrice, marketPrice, reference) : (orderPrice, marketPrice);
                case 'R':
                    return (orderPrice.Length > 1) ? Parse(orderPrice, reference, reference) : (reference.ToString("0.00"), reference);
                case 'S':
                    return (orderPrice.Length > 1) ? Parse(orderPrice, startPrice, reference) : (startPrice.ToString("0.00"), startPrice);
                case 'H':
                    return (orderPrice.Length > 1) ? Parse(orderPrice, highPrice, reference) : (highPrice.ToString("0.00"), highPrice);
                case 'L':
                    return (orderPrice.Length > 1) ? Parse(orderPrice, lowPrice, reference) : (lowPrice.ToString("0.00"), lowPrice);
                case 'E':
                    return (orderPrice.Length > 1) ? Parse(orderPrice, lastEndPrice, reference) : (lastEndPrice.ToString("0.00"), lastEndPrice);
                default:
                    return (orderPrice, decimal.Parse(orderPrice));
            }
        }

        public static (string, decimal) Parse(in string orderPrice, in QuoteData quote, in decimal marketPrice)
        {
            return Parse(orderPrice, marketPrice, quote.Reference, quote.StartPrice, quote.HighPrice, quote.LowPrice, quote.LastEndPrice);
        }
    }
}
