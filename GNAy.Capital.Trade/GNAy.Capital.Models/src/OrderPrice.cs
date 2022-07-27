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
    public class OrderPrice
    {
        public enum Enum
        {
            [Description("市價")]
            M, //0

            [Description("範圍市價")]
            P, //1

            [Description("漲停價")]
            H, //2

            [Description("跌停價")]
            L, //3
        }

        public static readonly string M = Enum.M.ToString();
        public static readonly string P = Enum.P.ToString();
        public static readonly string H = Enum.H.ToString();
        public static readonly string L = Enum.L.ToString();

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.M.GetDescription(),
            Enum.P.GetDescription(),
            Enum.H.GetDescription(),
            Enum.L.GetDescription(),
        }.AsReadOnly();

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

        public static (string, decimal) Parse(in string orderPrice, in decimal marketPrice, in decimal reference, in decimal highPrice, in decimal lowPrice)
        {
            if (orderPrice.StartsWith(M) || orderPrice.StartsWith(P))
            {
                return (orderPrice.Length > 1) ? Parse(orderPrice, marketPrice, reference) : (orderPrice, marketPrice);
            }
            else if (orderPrice.StartsWith(H))
            {
                return (orderPrice.Length > 1) ? Parse(orderPrice, highPrice, reference) : (highPrice.ToString("0.00"), highPrice);
            }
            else if (orderPrice.StartsWith(L))
            {
                return (orderPrice.Length > 1) ? Parse(orderPrice, lowPrice, reference) : (lowPrice.ToString("0.00"), lowPrice);
            }

            return (orderPrice, decimal.Parse(orderPrice));
        }

        public static (string, decimal) Parse(in string orderPrice, in QuoteData quote, in decimal marketPrice = 0)
        {
            return Parse(orderPrice, marketPrice == 0 ? quote.DealPrice : marketPrice, quote.Reference, quote.HighPriceLimit, quote.LowPriceLimit);
        }
    }
}
