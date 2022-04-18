using GNAy.Capital.Models;
using SKCOMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class CapitalController
    {
        private QuoteData CreateQuote(SKSTOCKLONG raw)
        {
            QuoteData quote = new QuoteData()
            {
                Updater = nameof(CreateQuote),
                UpdateTime = DateTime.Now,
                Symbol = raw.bstrStockNo,
                Name = raw.bstrStockName,
                DealPrice = raw.nClose / (decimal)Math.Pow(10, raw.sDecimal),
                DealQty = raw.nTickQty,
                BestBuyPrice = raw.nBid / (decimal)Math.Pow(10, raw.sDecimal),
                BestBuyQty = raw.nBc,
                BestSellPrice = raw.nAsk / (decimal)Math.Pow(10, raw.sDecimal),
                BestSellQty = raw.nAc,
                //OpenPrice = raw.nOpen / (decimal)Math.Pow(10, raw.sDecimal),
                //HighPrice = raw.nHigh / (decimal)Math.Pow(10, raw.sDecimal),
                //LowPrice = raw.nLow / (decimal)Math.Pow(10, raw.sDecimal),
                Reference = raw.nRef / (decimal)Math.Pow(10, raw.sDecimal),
                Simulate = raw.nSimulate,
                TotalQty = raw.nTQty,
                TradeDateRaw = raw.nTradingDay,
                HighPriceLimit = raw.nUp / (decimal)Math.Pow(10, raw.sDecimal),
                LowPriceLimit = raw.nDown / (decimal)Math.Pow(10, raw.sDecimal),
                Index = raw.nStockIdx,
                MarketGroup = short.Parse(raw.bstrMarketNo),
                DecimalPos = raw.sDecimal,
                TotalQtyBefore = raw.nYQty,
            };

            return quote;
        }

        private bool UpdateQuote(SKSTOCKLONG raw)
        {
            if (!_quoteIndexMap.TryGetValue(raw.nStockIdx, out QuoteData quote))
            {
                _appCtrl.LogError($"!_quoteIndexMap.TryGetValue(raw.nStockIdx, out QuoteData quote)|nStockIdx={raw.nStockIdx}", UniqueName);
                return false;
            }
            else if (quote.Symbol != raw.bstrStockNo)
            {
                _appCtrl.LogError($"quote.Symbol != raw.bstrStockNo|Symbol={quote.Symbol}|bstrStockNo={raw.bstrStockNo}", UniqueName);
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
            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options) && (_appCtrl.Config.StartOnTime || quote.Recovered))
            {
                if (quote.OpenPrice == 0 && raw.nSimulate.IsRealTrading() && raw.nTickQty > 0) //開盤第一筆成交
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

            quote.Updater = nameof(UpdateQuote);
            quote.UpdateTime = DateTime.Now;

            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options) && (_appCtrl.Config.StartOnTime || quote.Recovered))
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if (quote.LowPrice > quote.DealPrice || quote.LowPrice == 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            QuoteLastUpdated = quote;

            if (firstTick && !string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileOpenPrefix))
            {
                string symbol = string.IsNullOrWhiteSpace(quote.Symbol) ? $"{quote.MarketGroup}_{quote.Index}" : quote.Symbol;
                SaveQuotes(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileOpenPrefix}{symbol}_", string.Empty, quote);
            }

            return true;
        }

        private void OnNotifyHistoryTicks(QuoteData quote, int nPtr, int nDate, int lTimehms, int lTimemillismicros, int nBid, int nAsk, int nClose, int nQty, int nSimulate)
        {
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
            if (quote.OpenPrice == 0 && nSimulate.IsRealTrading() && quote.DealQty > 0) //開盤第一筆成交
            {
                quote.OpenPrice = quote.DealPrice;
            }
            quote.Simulate = nSimulate;

            quote.Updater = nameof(OnNotifyHistoryTicks);
            quote.UpdateTime = DateTime.Now;

            if (quote.Simulate.IsRealTrading())
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if (quote.LowPrice > quote.DealPrice || quote.LowPrice == 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            QuoteLastUpdated = quote;

            if (!string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileRecoverPrefix))
            {
                string symbol = string.IsNullOrWhiteSpace(quote.Symbol) ? $"{quote.MarketGroup}_{quote.Index}" : quote.Symbol;
                SaveQuotes(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileRecoverPrefix}{symbol}_", string.Empty, quote);

                if (_capitalProductRawMap.TryGetValue(quote.Index, out SKSTOCKLONG productInfo))
                {
                    QuoteData q = CreateQuote(productInfo);

                    q.Count = nPtr;
                    q.TradeDateRaw = nDate;
                    q.MatchedTimeHHmmss = lTimehms;
                    q.MatchedTimefff = lTimemillismicros;
                    q.BestBuyPrice = nBid / (decimal)Math.Pow(10, q.DecimalPos);
                    q.BestSellPrice = nAsk / (decimal)Math.Pow(10, q.DecimalPos);
                    q.DealPrice = nClose / (decimal)Math.Pow(10, q.DecimalPos);
                    q.DealQty = nQty;
                    q.Simulate = nSimulate;

                    q.Updater = nameof(OnNotifyHistoryTicks);
                    q.UpdateTime = DateTime.Now;

                    symbol = string.IsNullOrWhiteSpace(q.Symbol) ? $"{q.MarketGroup}_{q.Index}" : q.Symbol;
                    SaveQuotes(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileRecoverPrefix}{symbol}_RAW_", string.Empty, q);
                }
            }
        }

        private void OnNotifyTicks(QuoteData quote, int nPtr, int nDate, int lTimehms, int lTimemillismicros, int nBid, int nAsk, int nClose, int nQty, int nSimulate)
        {
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
            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options) && _appCtrl.Config.StartOnTime)
            {
                if (quote.OpenPrice == 0 && nSimulate.IsRealTrading() && quote.DealQty > 0) //開盤第一筆成交
                {
                    quote.OpenPrice = quote.DealPrice;
                    firstTick = true;
                }
            }
            quote.Simulate = nSimulate;

            quote.Updater = nameof(OnNotifyTicks);
            quote.UpdateTime = DateTime.Now;

            if (IsAMMarket && (quote.MarketGroupEnum == Market.EGroup.Futures || quote.MarketGroupEnum == Market.EGroup.Options) && (_appCtrl.Config.StartOnTime || quote.Recovered))
            {
                if (quote.OpenPrice != 0)
                {
                    if (quote.HighPrice < quote.DealPrice)
                    {
                        quote.HighPrice = quote.DealPrice;
                    }
                    if (quote.LowPrice > quote.DealPrice || quote.LowPrice == 0)
                    {
                        quote.LowPrice = quote.DealPrice;
                    }
                }
            }

            QuoteLastUpdated = quote;

            if (firstTick && !string.IsNullOrWhiteSpace(_appCtrl.Settings.QuoteFileOpenPrefix))
            {
                string symbol = string.IsNullOrWhiteSpace(quote.Symbol) ? $"{quote.MarketGroup}_{quote.Index}" : quote.Symbol;
                SaveQuotes(_appCtrl.Config.QuoteFolder, true, $"{_appCtrl.Settings.QuoteFileOpenPrefix}{symbol}_", string.Empty, quote);
            }
        }

        private FUTUREORDER CreateCaptialFutureOrder(StrategyData strategy)
        {
            FUTUREORDER pFutureOrder = new FUTUREORDER()
            {
                bstrFullAccount = strategy.FullAccount,
                bstrStockNo = strategy.Symbol,
                sBuySell = strategy.BS,
                sTradeType = strategy.TradeType,
                sDayTrade = strategy.DayTrade,
                sNewClose = strategy.Position,
                bstrPrice = strategy.OrderPrice,
                nQty = strategy.OrderQuantity,
            };

            return pFutureOrder;
        }
    }
}
