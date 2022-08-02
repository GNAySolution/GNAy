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
        private int _openInterestInterval;
        private int _futuresRightsInterval;

        private DateTime _lastTimeToSaveQuote;

        public void OnTimedEvent(in DateTime signalTime)
        {
            try
            {
                if (OpenInterest != null && _openInterestInterval > 0)
                {
                    OpenInterest.UpdateStatus(signalTime);

                    if ((signalTime - OpenInterest.QuerySent.Item1).TotalSeconds >= _openInterestInterval)
                    {
                        OpenInterest.SendNextQuery(signalTime);

                        if (CAPOrder.Count > 0 && OpenInterest.QuerySent.Item4 != 0)
                        {
                            _openInterestInterval += 2;
                            LogWarn(signalTime, $"{nameof(_openInterestInterval)}={_openInterestInterval}", UniqueName);
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
                if (FuturesRights != null && _futuresRightsInterval > 0)
                {
                    FuturesRights.UpdateStatus(signalTime);

                    if ((signalTime - FuturesRights.QuerySent.Item1).TotalSeconds >= _futuresRightsInterval)
                    {
                        FuturesRights.SendNextQuery(signalTime);

                        if (CAPOrder.Count > 0 && FuturesRights.QuerySent.Item4 != 0)
                        {
                            _futuresRightsInterval += 2;
                            LogWarn(signalTime, $"{nameof(_futuresRightsInterval)}={_futuresRightsInterval}", UniqueName);
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
    }
}
