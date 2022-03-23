using GNAy.Tools.NET47;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace GNAy.Tools.WPF
{
    public static class Extensions
    {
        /// <summary>
        /// https://stackoverflow.com/questions/5436349/what-happened-to-control-invokerequired-in-wpf
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="act"></param>
        public static void InvokeRequired(this DispatcherObject obj, Action<object> act)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj); });
                return;
            }

            act(obj);
        }

        public static void InvokeRequired<T>(this DispatcherObject obj, Action<object, T> act, T arg)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj, arg); });
                return;
            }

            act(obj, arg);
        }

        public static void InvokeRequired<T1, T2>(this DispatcherObject obj, Action<object, T1, T2> act, T1 arg1, T2 arg2)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj, arg1, arg2); });
                return;
            }

            act(obj, arg1, arg2);
        }

        public static void InvokeRequired<T1, T2, T3>(this DispatcherObject obj, Action<object, T1, T2, T3> act, T1 arg1, T2 arg2, T3 arg3)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj, arg1, arg2, arg3); });
                return;
            }

            act(obj, arg1, arg2, arg3);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/15216362/wpf-datagrid-how-to-get-binding-expression-of-a-cell
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNameMap"></param>
        public static void SetHeadersByBindings(this DataGrid obj, IDictionary<string, string> propertyNameMap)
        {
            foreach (DataGridColumn column in obj.Columns)
            {
                if (column is DataGridBoundColumn bound && bound.Binding is Binding bind)
                {
                    if (propertyNameMap.TryGetValue(bind.Path.Path, out string columnName))
                    {
                        column.Header = columnName;
                    }
                }
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/4615081/how-to-add-a-tooltip-for-a-datagrid-header-where-the-header-text-is-generated-d
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNameMap"></param>
        public static void SetHeadersByBindings(this DataGrid obj, IDictionary<string, ColumnAttribute> propertyNameMap)
        {
            foreach (DataGridColumn column in obj.Columns)
            {
                if (column is DataGridBoundColumn bound && bound.Binding is Binding bind)
                {
                    if (propertyNameMap.TryGetValue(bind.Path.Path, out ColumnAttribute attr))
                    {
                        column.Header = attr.ShortName;

                        Style s = new Style(typeof(DataGridColumnHeader));
                        s.Setters.Add(new Setter(ToolTipService.ToolTipProperty, attr.Name));

                        column.HeaderStyle = s;
                    }
                }
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/2160481/wpf-collectionviewsource-multiple-views
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICollectionView GetViewSource<T>(this ObservableCollection<T> obj)
        {
            CollectionViewSource s = new CollectionViewSource()
            {
                Source = obj,
            };
            return s.View;
        }

        public static ObservableCollection<T> SetAndGetItemsSource<T>(this ItemsControl obj)
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();
            obj.ItemsSource = collection.GetViewSource();
            return collection;
        }

        public static ScrollViewer GetScrollViewer(this DependencyObject obj)
        {
            Decorator border = VisualTreeHelper.GetChild(obj, 0) as Decorator;
            return border?.Child as ScrollViewer;
        }

        /// <summary>
        /// <para>https://stackoverflow.com/questions/1027051/how-to-autoscroll-on-wpf-datagrid</para>
        /// <para>https://stackoverflow.com/questions/60378552/wpf-scrolltotop-vs-scrolltohome</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="toEnd"></param>
        /// <returns></returns>
        public static bool ScrollToBorder(this DependencyObject obj, bool toEnd = true)
        {
            ScrollViewer viewer = GetScrollViewer(obj);

            if (viewer != null)
            {
                if (toEnd)
                {
                    viewer.ScrollToEnd();
                }
                else
                {
                    viewer.ScrollToHome();
                }
                return true;
            }

            return false;
        }
    }
}
