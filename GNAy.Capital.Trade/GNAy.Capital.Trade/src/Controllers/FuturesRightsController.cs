using System;
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

        //private readonly ObservableCollection<FuturesRightsData> _dataCollection;

        //public int Count => _dataCollection.Count;
        //public FuturesRightsData this[int index] => _dataCollection[index];

        public FuturesRightsController(AppController appCtrl)
        {
            CreatedTime = DateTime.Now;
            UniqueName = nameof(FuturesRightsController).Replace("Controller", "Ctrl");
            _appCtrl = appCtrl;

            //_appCtrl.MainForm.DataGridFuturesRights.SetHeadersByBindings(FuturesRightsData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            //_dataCollection = _appCtrl.MainForm.DataGridFuturesRights.SetAndGetItemsSource<FuturesRightsData>();
        }

        private FuturesRightsController() : this(null)
        { }
    }
}
