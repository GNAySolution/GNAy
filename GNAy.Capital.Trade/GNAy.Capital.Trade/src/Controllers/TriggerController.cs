using GNAy.Capital.Models;
using GNAy.Tools.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Capital.Trade.Controllers
{
    public class TriggerController
    {
        public readonly DateTime CreatedTime;

        private readonly ObservableCollection<string> TriggerCancelKinds;

        private readonly SortedDictionary<string, TriggerData> TriggerMap;
        private readonly ObservableCollection<TriggerData> TriggerCollection;

        public TriggerController()
        {
            CreatedTime = DateTime.Now;

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            TriggerCancelKinds = new ObservableCollection<string>()
            {
                Definition.TriggerCancel0.Item2,
                Definition.TriggerCancel1.Item2,
                Definition.TriggerCancel2.Item2,
                Definition.TriggerCancel3.Item2,
                Definition.TriggerCancel4.Item2,
            };
            MainWindow.Instance.ComboBoxTriggerCancel.ItemsSource = TriggerCancelKinds.GetViewSource();
            MainWindow.Instance.ComboBoxTriggerCancel.SelectedIndex = Definition.TriggerCancel0.Item1;

            TriggerMap = new SortedDictionary<string, TriggerData>();
            MainWindow.Instance.DataGridTriggerRule.SetHeadersByBindings(TriggerData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            TriggerCollection = MainWindow.Instance.DataGridTriggerRule.SetAndGetItemsSource<TriggerData>();
        }

        public void OnQuotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //
        }
    }
}
