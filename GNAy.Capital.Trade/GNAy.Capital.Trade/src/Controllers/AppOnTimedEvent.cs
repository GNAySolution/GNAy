using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GNAy.Capital.Trade.Controllers
{
    public partial class AppController
    {
        public DateTime SignalTimeBG { get; private set; }

        private int _openInterestInterval;
        private int _futuresRightsInterval;

        private DateTime _lastTimeToSaveQuote;

        public void OnTimedEvent(DateTime signalTime)
        {
            try
            {
                if (OpenInterest != null)
                {
                    OpenInterest.UpdateStatus(signalTime);

                    if (_openInterestInterval > 0 && (signalTime - OpenInterest.QuerySent.Item1).TotalSeconds >= _openInterestInterval)
                    {
                        OpenInterest.SendNextQuery(signalTime);

                        if (CAPOrder.Count > 0 && OpenInterest.QuerySent.Item4 != 0)
                        {
                            _openInterestInterval += 2;
                            LogWarn(signalTime, $"_openInterestInterval={_openInterestInterval}", UniqueName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(signalTime, ex, ex.StackTrace);
            }

            try
            {
                if (FuturesRights != null)
                {
                    FuturesRights.UpdateStatus(signalTime);

                    if (_futuresRightsInterval > 0 && (signalTime - FuturesRights.QuerySent.Item1).TotalSeconds >= _futuresRightsInterval)
                    {
                        FuturesRights.SendNextQuery(signalTime);

                        if (CAPOrder.Count > 0 && FuturesRights.QuerySent.Item4 != 0)
                        {
                            _futuresRightsInterval += 2;
                            LogWarn(signalTime, $"_futuresRightsInterval={_futuresRightsInterval}", UniqueName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(signalTime, ex, ex.StackTrace);
            }

            try
            {
                Strategy?.UpdateStatus(signalTime);
            }
            catch (Exception ex)
            {
                LogException(signalTime, ex, ex.StackTrace);
            }

            try
            {
                Trigger?.UpdateStatus(signalTime);
            }
            catch (Exception ex)
            {
                LogException(signalTime, ex, ex.StackTrace);
            }

            try
            {
                if (Settings.QuoteSaveInterval > 0 && (signalTime - _lastTimeToSaveQuote).TotalSeconds >= Settings.QuoteSaveInterval && CAPQuote != null && !string.IsNullOrWhiteSpace(Settings.QuoteFileClosePrefix))
                {
                    _lastTimeToSaveQuote = signalTime;
                    CAPQuote.SaveData(Config.QuoteFolder, false, Settings.QuoteFileClosePrefix);
                }
            }
            catch (Exception ex)
            {
                LogException(signalTime, ex, ex.StackTrace);
            }

        }

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/dotnet/api/system.timers.timer?view=net-6.0
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timerBG.Stop();

            SignalTimeBG = e.SignalTime;
            OnTimedEvent(e.SignalTime);

            _timerBG.Start();
        }
    }
}
