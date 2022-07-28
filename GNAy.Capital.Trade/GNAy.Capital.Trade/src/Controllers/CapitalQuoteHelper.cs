using GNAy.Capital.Models;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalQuoteController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="quote">通常是斷線重連時更新Index用</param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public QuoteData CreateOrUpdate(in SKSTOCKLONG raw, QuoteData quote = null, [CallerMemberName] in string memberName = "")
        {
            if (quote == null)
            {
                quote = new QuoteData();
            }

            quote.Symbol = raw.bstrStockNo;
            quote.Name = raw.bstrStockName;
            quote.DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal);
            quote.DealQty = raw.nTickQty;
            quote.BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestBuyQty = raw.nBc;
            quote.BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestSellQty = raw.nAc;
            //quote.OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal),
            //quote.HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal),
            //quote.LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal),
            quote.Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal);
            quote.Simulate = raw.nSimulate;
            quote.TotalQty = raw.nTQty;
            quote.TradeDateRaw = raw.nTradingDay;
            quote.HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal);
            quote.LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal);
            quote.Index = raw.nStockIdx;
            quote.MarketGroup = raw.bstrMarketNo[0] - '0';
            quote.DecimalPos = raw.sDecimal;
            quote.TotalQtyBefore = raw.nYQty;
            quote.Updater = memberName;
            quote.UpdateTime = DateTime.Now;

            return quote;
        }

        private QuoteData Create(in SKSTOCKLONG raw, in int nPtr, in int nDate, in int lTimehms, in int lTimemillismicros, in int nBid, in int nAsk, in int nClose, in int nQty, in int nSimulate, [CallerMemberName] in string memberName = "")
        {
            QuoteData quote = CreateOrUpdate(raw);

            quote.Count = nPtr;
            quote.TradeDateRaw = nDate;
            quote.MatchedTimeHHmmss = lTimehms;
            quote.MatchedTimefff = lTimemillismicros;
            quote.BestBuyPrice = nBid / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.BestSellPrice = nAsk / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.DealPrice = nClose / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.DealQty = nQty;
            quote.Simulate = nSimulate;
            quote.Updater = memberName;
            quote.UpdateTime = DateTime.Now;

            return quote;
        }

        private bool Update(in SKSTOCKLONG raw, [CallerMemberName] in string memberName = "")
        {
            //https://stackoverflow.com/questions/628761/convert-a-character-digit-to-the-corresponding-integer-in-c
            if (!_dataIndexMap.TryGetValue((raw.bstrMarketNo[0] - '0') * 1000000 + raw.nStockIdx, out QuoteData quote))
            {
                _appCtrl.LogError($"!_dataIndexMap.TryGetValue((raw.bstrMarketNo[0] - '0') * 1000000 + raw.nStockIdx, out QuoteData quote)|bstrMarketNo={raw.bstrMarketNo}|nStockIdx={raw.nStockIdx}", UniqueName);
                ++DataIndexErrorCount;
                return false;
            }
            else if (quote.Symbol != raw.bstrStockNo)
            {
                _appCtrl.LogError($"quote.Symbol != raw.bstrStockNo|Symbol={quote.Symbol}|bstrMarketNo={raw.bstrMarketNo}|bstrStockNo={raw.bstrStockNo}", UniqueName);
                return false;
            }
            //else if (quote.Name != raw.bstrStockName)
            //{
            //    _appCtrl.LogError($"quote.Name != raw.bstrStockName|Name={quote.Name}|bstrStockName={raw.bstrStockName}", UniqueName);
            //    return false;
            //}
            //else if ($"{quote.MarketGroup}" != raw.bstrMarketNo)
            //{
            //    _appCtrl.LogError($"quote.MarketGroup != raw.bstrMarketNo|MarketGroup={quote.MarketGroup}|bstrMarketNo={raw.bstrMarketNo}", UniqueName);
            //    return false;
            //}

            bool firstTick = false;

            //quote.Symbol = raw.bstrStockNo;
            //quote.Name = raw.bstrStockName;
            if (quote.Page < 0 || quote.DealQty == 0) //沒有訂閱SKQuoteLib_RequestLiveTick
            {
                quote.DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal);
                quote.DealQty = raw.nTickQty;
                if (raw.nTradingDay > quote.TradeDateRaw)
                {
                    quote.TradeDateRaw = raw.nTradingDay;
                }
            }
            quote.BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestBuyQty = raw.nBc;
            quote.BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal);
            quote.BestSellQty = raw.nAc;
            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option) && (LoadedOnTime || quote.Recovered))
            {
                if (quote.OpenPrice == 0 && raw.nSimulate == QuoteData.RealTrade && raw.nTQty > quote.TotalQty) //開盤第一筆成交
                {
                    quote.OpenPrice = quote.DealPrice;
                    firstTick = true;
                }
            }
            else
            {
                quote.OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal);
                quote.HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal);
                quote.LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal);
            }
            quote.Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal);
            quote.Simulate = raw.nSimulate;
            quote.TotalQty = raw.nTQty;
            //quote.TradeDateRaw = raw.nTradingDay;
            quote.HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal);
            quote.LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal);
            //quote.Index = raw.nStockIdx;
            //quote.MarketGroup = short.TryParse(raw.bstrMarketNo, out short x) ? x : (short)-1;
            quote.DecimalPos = raw.sDecimal;
            quote.TotalQtyBefore = raw.nYQty;

            quote.Updater = memberName;
            quote.UpdateTime = DateTime.Now;

            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option) && (LoadedOnTime || quote.Recovered))
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if ((quote.LowPrice > quote.DealPrice || quote.LowPrice == 0) && quote.DealPrice != 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            LastData = quote;

            if (IsAMMarket && (raw.nSimulate != QuoteData.RealTrade || firstTick) && quote.MarketGroupEnum == Market.EGroup.Futures && !string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileOpenPrefix))
            {
                string symbol = string.IsNullOrWhiteSpace(quote.Symbol) ? $"{quote.MarketGroup}_{quote.Index}" : quote.Symbol;
                SaveData(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileOpenPrefix}{symbol}_", string.Empty, quote);

                if (_dataRawMap.TryGetValue(quote.PrimaryKey, out SKSTOCKLONG productInfo))
                {
                    QuoteData qRaw = CreateOrUpdate(productInfo);

                    qRaw.DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.DealQty = raw.nTickQty;
                    qRaw.TradeDateRaw = raw.nTradingDay;
                    qRaw.BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.BestBuyQty = raw.nBc;
                    qRaw.BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.BestSellQty = raw.nAc;
                    qRaw.OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.Simulate = raw.nSimulate;
                    qRaw.TotalQty = raw.nTQty;
                    //qRaw.TradeDateRaw = raw.nTradingDay;
                    qRaw.HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal);
                    qRaw.LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal);
                    //qRaw.Index = raw.nStockIdx;
                    //qRaw.MarketGroup = short.TryParse(raw.bstrMarketNo, out short x) ? x : (short)-1;
                    qRaw.DecimalPos = raw.sDecimal;
                    qRaw.TotalQtyBefore = raw.nYQty;

                    qRaw.Updater = memberName;
                    qRaw.UpdateTime = DateTime.Now;

                    symbol = string.IsNullOrWhiteSpace(qRaw.Symbol) ? $"{qRaw.MarketGroup}_{qRaw.Index}" : qRaw.Symbol;
                    SaveData(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileOpenPrefix}{symbol}_RAW_", string.Empty, qRaw);
                }
            }

            return true;
        }

        private void OnNotifyHistoryTicks(in QuoteData quote, in int nPtr, in int nDate, in int lTimehms, in int lTimemillismicros, in int nBid, in int nAsk, in int nClose, in int nQty, in int nSimulate)
        {
            const string methodName = nameof(OnNotifyHistoryTicks);

            quote.Count = nPtr;
            if (nDate > quote.TradeDateRaw)
            {
                quote.TradeDateRaw = nDate;
            }
            quote.MatchedTimeHHmmss = lTimehms;
            quote.MatchedTimefff = lTimemillismicros;
            quote.BestBuyPrice = nBid / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.BestSellPrice = nAsk / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.DealPrice = nClose / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.DealQty = nQty;
            if (quote.OpenPrice == 0 && nSimulate == QuoteData.RealTrade && quote.DealQty > 0) //開盤第一筆成交
            {
                quote.OpenPrice = quote.DealPrice;
            }
            quote.Simulate = nSimulate;

            quote.Updater = methodName;
            quote.UpdateTime = DateTime.Now;

            if (quote.Simulate == QuoteData.RealTrade)
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if ((quote.LowPrice > quote.DealPrice || quote.LowPrice == 0) && quote.DealPrice != 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            LastData = quote;

            if (!string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileRecoverPrefix))
            {
                string symbol = string.IsNullOrWhiteSpace(quote.Symbol) ? $"{quote.MarketGroup}_{quote.Index}" : quote.Symbol;
                SaveData(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileRecoverPrefix}{symbol}_", string.Empty, quote);

                if (_dataRawMap.TryGetValue(quote.PrimaryKey, out SKSTOCKLONG productInfo))
                {
                    QuoteData qRaw = Create(productInfo, nPtr, nDate, lTimehms, lTimemillismicros, nBid, nAsk, nClose, nQty, nSimulate);

                    symbol = string.IsNullOrWhiteSpace(qRaw.Symbol) ? $"{qRaw.MarketGroup}_{qRaw.Index}" : qRaw.Symbol;
                    SaveData(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileRecoverPrefix}{symbol}_RAW_", string.Empty, qRaw);
                }
            }
        }

        private void OnNotifyTicks(in QuoteData quote, in int nPtr, in int nDate, in int lTimehms, in int lTimemillismicros, in int nBid, in int nAsk, in int nClose, in int nQty, in int nSimulate)
        {
            const string methodName = nameof(OnNotifyTicks);

            bool firstTick = false;

            quote.Count = nPtr;
            if (nDate > quote.TradeDateRaw)
            {
                quote.TradeDateRaw = nDate;
            }
            quote.MatchedTimeHHmmss = lTimehms;
            quote.MatchedTimefff = lTimemillismicros;
            quote.BestBuyPrice = nBid / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.BestSellPrice = nAsk / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.DealPrice = nClose / (decimal)Math.Pow(10, quote.DecimalPos);
            quote.DealQty = nQty;
            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option) && LoadedOnTime)
            {
                if (quote.OpenPrice == 0 && nSimulate == QuoteData.RealTrade && nQty > 0) //開盤第一筆成交
                {
                    quote.OpenPrice = quote.DealPrice;
                    firstTick = true;
                }
            }
            quote.Simulate = nSimulate;

            quote.Updater = methodName;
            quote.UpdateTime = DateTime.Now;

            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Option) && (LoadedOnTime || quote.Recovered))
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if ((quote.LowPrice > quote.DealPrice || quote.LowPrice == 0) && quote.DealPrice != 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            LastData = quote;

            if (IsAMMarket && (nSimulate != QuoteData.RealTrade || firstTick) && quote.MarketGroupEnum == Market.EGroup.Futures && !string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileOpenPrefix))
            {
                string symbol = string.IsNullOrWhiteSpace(quote.Symbol) ? $"{quote.MarketGroup}_{quote.Index}" : quote.Symbol;
                SaveData(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileOpenPrefix}{symbol}_", string.Empty, quote);

                if (_dataRawMap.TryGetValue(quote.PrimaryKey, out SKSTOCKLONG productInfo))
                {
                    QuoteData qRaw = Create(productInfo, nPtr, nDate, lTimehms, lTimemillismicros, nBid, nAsk, nClose, nQty, nSimulate);

                    symbol = string.IsNullOrWhiteSpace(qRaw.Symbol) ? $"{qRaw.MarketGroup}_{qRaw.Index}" : qRaw.Symbol;
                    SaveData(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileOpenPrefix}{symbol}_RAW_", string.Empty, qRaw);
                }
            }
        }
    }
}
