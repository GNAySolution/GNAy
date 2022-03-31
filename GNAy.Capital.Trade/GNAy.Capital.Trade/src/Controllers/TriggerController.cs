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

        private readonly ObservableCollection<string> _triggerCancelKinds;

        private readonly SortedDictionary<string, TriggerData> _triggerMap;
        private readonly ObservableCollection<TriggerData> _triggerCollection;

        public TriggerController()
        {
            CreatedTime = DateTime.Now;

            //https://www.codeproject.com/Questions/1117817/Basic-WPF-binding-to-collection-in-combobox
            _triggerCancelKinds = new ObservableCollection<string>()
            {
                Definition.TriggerCancel0.Item2,
                Definition.TriggerCancel1.Item2,
                Definition.TriggerCancel2.Item2,
                Definition.TriggerCancel3.Item2,
                Definition.TriggerCancel4.Item2,
            };
            AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.ItemsSource = _triggerCancelKinds.GetViewSource();
            AppCtrl.Instance.MainForm.ComboBoxTriggerCancel.SelectedIndex = Definition.TriggerCancel0.Item1;

            _triggerMap = new SortedDictionary<string, TriggerData>();
            AppCtrl.Instance.MainForm.DataGridTriggerRule.SetHeadersByBindings(TriggerData.PropertyMap.Values.ToDictionary(x => x.Item2.Name, x => x.Item1));
            _triggerCollection = AppCtrl.Instance.MainForm.DataGridTriggerRule.SetAndGetItemsSource<TriggerData>();
        }

        public void OnQuotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.Instance.LogException(ex, ex.StackTrace);
            }
        }

        public void AddRule()
        {
            AppCtrl.Instance.LogTrace("Start");

            try
            {
                //AppCtrl.Instance.MainForm
            }
            catch (Exception ex)
            {
                AppCtrl.Instance.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.Instance.LogTrace("End");
            }
        }
    }
}
