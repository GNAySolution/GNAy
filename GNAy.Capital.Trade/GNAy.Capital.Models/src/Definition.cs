﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Models
{
    public static class Definition
    {
        public const int MarketTSE = 0;
        public const int MarketOTC = 1;
        public const int MarketFutures = 2;
        public const int MarketOptions = 3;
        public const int MarketEmerging = 4;

        //public const int SimulateTrade = 1;
        public const int RealTrade = 0;

        /// <summary>
        /// 試撮
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsSimulating(this int obj)
        {
            return obj > RealTrade;
        }

        public static bool IsRealTrading(this int obj)
        {
            return obj == RealTrade;
        }

        public static (int, string, string) MarketTypeStock = (0, "TS", "證券");
        public static (int, string, string) MarketTypeFutures = (1, "TF", "期貨");
        public static (int, string, string) MarketTypeOptions = (2, "TO", "選擇權");
        public static (int, string, string) MarketTypeOverseaStock = (3, "OS", "複委託");
        public static (int, string, string) MarketTypeOverseaFutures = (4, "OF", "海外期貨");
        public static (int, string, string) MarketTypeOverseaOptions = (5, "OO", "海外選擇權");

        public static ReadOnlyCollection<string> MarketTypes = new List<string>()
        {
            MarketTypeStock.Item2,
            MarketTypeFutures.Item2,
            MarketTypeOptions.Item2,
            MarketTypeOverseaStock.Item2,
            MarketTypeOverseaFutures.Item2,
            MarketTypeOverseaOptions.Item2,
        }.AsReadOnly();

        /// <summary>
        /// 大於或等於
        /// </summary>
        public const string IsGreaterThanOrEqualTo = ">=";

        /// <summary>
        /// 大於
        /// </summary>
        public const string IsGreaterThan = ">";

        /// <summary>
        /// 等於
        /// </summary>
        public const string IsEqualTo = "=";

        /// <summary>
        /// 小於或等於
        /// </summary>
        public const string IsLessThanOrEqualTo = "<=";

        /// <summary>
        /// 小於
        /// </summary>
        public const string IsLessThan = "<";

        /// <summary>
        /// 觸價狀態，等待中
        /// </summary>
        public static (int, string) TriggerStatusWaiting = (0, "等待中");

        /// <summary>
        /// 觸價狀態，已取消
        /// </summary>
        public static (int, string) TriggerStatusCancelled = (1, "已取消");

        /// <summary>
        /// 觸價狀態，監控中
        /// </summary>
        public static (int, string) TriggerStatusMonitoring = (2, "監控中");

        /// <summary>
        /// 觸價狀態，已觸發
        /// </summary>
        public static (int, string) TriggerStatusExecuted = (3, "已觸發");

        /// <summary>
        /// https://stackoverflow.com/questions/4680035/read-only-list-in-c-sharp
        /// </summary>
        public static ReadOnlyCollection<string> TriggerStatusKinds = new List<string>()
        {
            TriggerStatusWaiting.Item2,
            TriggerStatusCancelled.Item2,
            TriggerStatusMonitoring.Item2,
            TriggerStatusExecuted.Item2,
        }.AsReadOnly();

        /// <summary>
        /// 觸價後取消監控，同代碼同欄位
        /// </summary>
        public static (int, string) TriggerCancel0 = (0, "同代碼同欄位");

        /// <summary>
        /// 觸價後取消監控，同代碼全欄位
        /// </summary>
        public static (int, string) TriggerCancel1 = (1, "同代碼全欄位");

        /// <summary>
        /// 觸價後取消監控，全代碼同欄位
        /// </summary>
        public static (int, string) TriggerCancel2 = (2, "全代碼同欄位");

        /// <summary>
        /// 觸價後取消監控，全代碼全欄位
        /// </summary>
        public static (int, string) TriggerCancel3 = (3, "全代碼全欄位");

        public static ReadOnlyCollection<string> TriggerCancelKinds = new List<string>()
        {
            TriggerCancel0.Item2,
            TriggerCancel1.Item2,
            TriggerCancel2.Item2,
            TriggerCancel3.Item2,
        }.AsReadOnly();
    }
}
