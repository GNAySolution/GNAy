using GNAy.Capital.Models;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class FuturesRightsController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly ConcurrentQueue<string> _waitToAdd;

        private readonly SortedDictionary<string, FuturesRightsData> _dataMap;
        private readonly ObservableCollection<FuturesRightsData> _dataCollection;

        public int Count => _dataCollection.Count;
        public FuturesRightsData this[string key] => _dataMap.TryGetValue(key, out FuturesRightsData data) ? data : null;
        public FuturesRightsData this[int index] => _dataCollection[index];

        /// <summary>
        /// (時間,索引,帳號,查詢結果)
        /// </summary>
        public (DateTime, int, string, int) QuerySent { get; private set; }

        public FuturesRightsController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(FuturesRightsController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _waitToAdd = new ConcurrentQueue<string>();

            _dataMap = new SortedDictionary<string, FuturesRightsData>();
            _appCtrl.MainForm.DataGridFuturesRights.SetColumns(FuturesRightsData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _dataCollection = _appCtrl.MainForm.DataGridFuturesRights.SetViewAndGetObservation<FuturesRightsData>();

            QuerySent = (DateTime.Now, -1, string.Empty, -1);
        }

        private FuturesRightsController() : this(null)
        { }

        public void UpdateStatus(DateTime start)
        {
            const string methodName = nameof(UpdateStatus);

            while (_waitToAdd.Count > 0)
            {
                _waitToAdd.TryDequeue(out string raw);

                if (raw.StartsWith("##"))
                {
                    continue;
                }

                FuturesRightsData data = null;
                bool found = false;

                foreach (FuturesRightsData value in _dataMap.Values)
                {
                    if (value.RawInfo == raw)
                    {
                        found = true;

                        break;
                    }
                }

                if (found)
                {
                    continue;
                }

                try
                {
                    data = new FuturesRightsData(raw);
                    data.Updater = methodName;
                    data.UpdateTime = DateTime.Now;

                    _dataMap[data.Account] = data;
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }

                if (data == null)
                {
                    continue;
                }

                _appCtrl.MainForm.InvokeSync(delegate
                {
                    try
                    {
                        _dataCollection.Add(data);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                });
            }
        }

        public void AddAsync(string raw)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (raw.IndexOf("NO DATA", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return;
                }

                _waitToAdd.Enqueue(raw);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        public (DateTime, int, string, int) SendNextQuery(DateTime start)
        {
            try
            {
                if (_appCtrl.CAPCenter == null)
                {
                    return QuerySent;
                }
                else if (_appCtrl.CAPOrder.Count <= 0)
                {
                    return QuerySent;
                }

                for (int i = QuerySent.Item2 + 1; i < _appCtrl.CAPOrder.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.CAPOrder[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.CAPOrder.GetFuturesRights(acc.FullAccount);
                    QuerySent = (DateTime.Now, i, acc.FullAccount, result);

                    return QuerySent;
                }

                for (int i = 0; i < _appCtrl.CAPOrder.Count; ++i)
                {
                    OrderAccData acc = _appCtrl.CAPOrder[i];

                    if (acc.MarketType != Market.EType.Futures)
                    {
                        continue;
                    }

                    int result = _appCtrl.CAPOrder.GetFuturesRights(acc.FullAccount);
                    QuerySent = (DateTime.Now, i, acc.FullAccount, result);

                    return QuerySent;
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }

            QuerySent = (DateTime.Now, -1, string.Empty, -1);

            return QuerySent;
        }

        public decimal? SumProfit(string accounts, DateTime start)
        {
            if (string.IsNullOrWhiteSpace(accounts))
            {
                return null;
            }

            decimal sum = 0;

            foreach (string account in accounts.SplitWithoutWhiteSpace(','))
            {
                OrderAccData acc = _appCtrl.CAPOrder[account];

                if (acc == null)
                {
                    _appCtrl.LogError(start, $"查無帳號|account={account}", UniqueName);

                    return null;
                }

                FuturesRightsData data = this[acc.FullAccount];

                if (data == null)
                {
                    _appCtrl.LogError(start, $"查無帳號|FullAccount={acc.FullAccount}", UniqueName);

                    return null;
                }

                sum += data.F12;
            }

            return sum;
        }
    }
}
