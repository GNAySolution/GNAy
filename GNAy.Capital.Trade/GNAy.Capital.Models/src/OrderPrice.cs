﻿using GNAy.Tools.NET47;
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
        public enum Enum : short
        {
            [Description("限價")]
            L, //0

            [Description("市價")]
            M, //1

            [Description("範圍市價")]
            P, //2
        }

        public static readonly string L = string.Empty;
        public static readonly string M = Enum.M.ToString();
        public static readonly string P = Enum.P.ToString();

        public static ReadOnlyCollection<string> Description = new List<string>()
        {
            Enum.L.GetDescription(),
            Enum.M.GetDescription(),
            Enum.P.GetDescription(),
        }.AsReadOnly();

        public static (string, decimal) Parse(string orderPrice, decimal marketPrice, decimal reference, Market.EGroup mGroup)
        {
            if (orderPrice.StartsWith(M) || orderPrice.StartsWith(P))
            {
                if (orderPrice.Length > 1)
                {
                    decimal newPri = 0;

                    if (orderPrice.EndsWith("%"))
                    {
                        decimal offsetPct = decimal.Parse(orderPrice.Substring(1, orderPrice.Length - 2));
                        offsetPct /= 100;

                        string format = "0.00";

                        if (mGroup == Market.EGroup.Futures)
                        {
                            format = "0";
                        }

                        newPri = marketPrice + reference * offsetPct;
                        return (newPri.ToString(format), newPri);
                    }

                    decimal offset = decimal.Parse(orderPrice.Substring(1));
                    newPri = marketPrice + offset;
                    return (newPri.ToString("0.00"), newPri);
                }

                return (orderPrice, marketPrice);
            }

            return (orderPrice, decimal.Parse(orderPrice));
        }
    }
}
