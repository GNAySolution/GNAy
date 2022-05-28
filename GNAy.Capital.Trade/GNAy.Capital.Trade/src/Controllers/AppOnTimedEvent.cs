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

        private int _secondsToQueryOpenInterest;
        private DateTime _lastTimeToSaveQuote;

        public void OnTimedEvent(DateTime signalTime)
        {
            try
            {
                if (OpenInterest != null)
                {
                    OpenInterest.UpdateStatus(signalTime);

                    if ((signalTime - OpenInterest.QuerySent.Item1).TotalSeconds >= _secondsToQueryOpenInterest)
                    {
                        OpenInterest.SendNextQuery(signalTime);

                        if (CAPOrder.Count > 0 && OpenInterest.QuerySent.Item4 != 0)
                        {
                            _secondsToQueryOpenInterest += 2;
                            LogWarn(signalTime, $"_secondsToQueryOpenInterest={_secondsToQueryOpenInterest}", UniqueName);
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
