using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class OpenInterestController
    {
        public readonly DateTime CreatedTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly SortedDictionary<string, OpenInterestData> _oiMap;
        private readonly ObservableCollection<OpenInterestData> _oiCollection;

        public int Count => _oiMap.Count;
        public OpenInterestData this[string key] => _oiMap.TryGetValue(key, out OpenInterestData data) ? data : null;
        public IReadOnlyList<OpenInterestData> OrderDetailCollection => _oiCollection;

        public OpenInterestController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(OpenInterestController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            _oiMap = new SortedDictionary<string, OpenInterestData>();
            _appCtrl.MainForm.DataGridOpenInterest.SetHeadersByBindings(OpenInterestData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _oiCollection = _appCtrl.MainForm.DataGridOpenInterest.SetAndGetItemsSource<OpenInterestData>();
        }

        private OpenInterestController() : this(null)
        { }
    }
}
