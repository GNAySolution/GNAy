﻿using GNAy.Tools.NET48;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GNAy.Tools.WPF
{
    public static class Extensions
    {
        /// <summary>
        /// <para>https://stackoverflow.com/questions/5436349/what-happened-to-control-invokerequired-in-wpf</para>
        /// <para>https://docs.microsoft.com/zh-tw/dotnet/api/system.windows.threading.dispatcher.invoke?view=windowsdesktop-6.0</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="act"></param>
        /// <param name="priority"></param>
        public static void InvokeSync(this DispatcherObject obj, Action<object> act, in DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj); }, priority);
                return;
            }

            act(obj);
        }

        public static void InvokeSync<T>(this DispatcherObject obj, Action<object, T> act, T arg, in DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj, arg); }, priority);
                return;
            }

            act(obj, arg);
        }

        public static void InvokeSync<T1, T2>(this DispatcherObject obj, Action<object, T1, T2> act, T1 arg1, T2 arg2, in DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj, arg1, arg2); }, priority);
                return;
            }

            act(obj, arg1, arg2);
        }

        public static void InvokeSync<T1, T2, T3>(this DispatcherObject obj, Action<object, T1, T2, T3> act, T1 arg1, T2 arg2, T3 arg3, in DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.Invoke(delegate { act(obj, arg1, arg2, arg3); }, priority);
                return;
            }

            act(obj, arg1, arg2, arg3);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/60759414/delegate-is-never-executed-while-invoked-via-dispatcher-begininvoke-with-context
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="act"></param>
        /// <param name="priority"></param>
        public static void InvokeAsync(this DispatcherObject obj, Action<object> act, in DispatcherPriority priority = DispatcherPriority.Background)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.InvokeAsync(delegate { act(obj); }, priority);
                return;
            }

            act(obj);
        }

        public static void InvokeAsync<T>(this DispatcherObject obj, Action<object, T> act, T arg, in DispatcherPriority priority = DispatcherPriority.Background)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.InvokeAsync(delegate { act(obj, arg); }, priority);
                return;
            }

            act(obj, arg);
        }

        public static void InvokeAsync<T1, T2>(this DispatcherObject obj, Action<object, T1, T2> act, T1 arg1, T2 arg2, in DispatcherPriority priority = DispatcherPriority.Background)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.InvokeAsync(delegate { act(obj, arg1, arg2); }, priority);
                return;
            }

            act(obj, arg1, arg2);
        }

        public static void InvokeAsync<T1, T2, T3>(this DispatcherObject obj, Action<object, T1, T2, T3> act, T1 arg1, T2 arg2, T3 arg3, in DispatcherPriority priority = DispatcherPriority.Background)
        {
            if (!obj.CheckAccess())
            {
                obj.Dispatcher.InvokeAsync(delegate { act(obj, arg1, arg2, arg3); }, priority);
                return;
            }

            act(obj, arg1, arg2, arg3);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/5124825/generating-a-screenshot-of-a-wpf-window
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        public static void ScreenshotToFile<T>(this Visual obj, in string fileName) where T : BitmapEncoder, new ()
        {
            DrawingVisual visual = new DrawingVisual();
            Rect bounds = VisualTreeHelper.GetDescendantBounds(obj);

            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(obj), null, new Rect(new Point(), bounds.Size));
            }

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(visual);

            T bitmapEncoder = new T();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            FileInfo fInfo = new FileInfo(fileName);
            fInfo.Directory.Create();

            using (Stream stm = File.Create(fInfo.FullName))
            {
                bitmapEncoder.Save(stm);
            }
        }

        /// <summary>
        /// https://github.com/punker76/MahApps.Metro.SimpleChildWindow/issues/69
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TextBox GetEditableTextBox(this ComboBox obj)
        {
            obj.ApplyTemplate();

            return (TextBox)obj.Template.FindName("PART_EditableTextBox", obj);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/15216362/wpf-datagrid-how-to-get-binding-expression-of-a-cell
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNameMap"></param>
        public static void SetHeadersByBindings(this DataGrid obj, in IDictionary<string, string> propertyNameMap)
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

        private static void SetColumn<T>(this DataGridBoundColumn obj, in Binding bind, in T attr) where T : ColumnAttribute
        {
            obj.Header = attr.WPFName;
            obj.DisplayIndex = attr.WPFDisplayIndex;
            bind.StringFormat = attr.WPFStringFormat;
            obj.IsReadOnly = attr.WPFIsReadOnly;
            obj.Visibility = FlagOperator.ConvertTo<Visibility>(attr.WPFVisibility.ToString());
            obj.CanUserReorder = attr.WPFCanUserReorder;
            obj.CanUserSort = attr.WPFCanUserSort;

            //https://stackoverflow.com/questions/4577944/how-to-resize-wpf-datagrid-to-fit-its-content
            obj.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);

            //https://stackoverflow.com/questions/4615081/how-to-add-a-tooltip-for-a-datagrid-header-where-the-header-text-is-generated-d
            Style headerS = new Style(typeof(DataGridColumnHeader));
            headerS.Setters.Add(new Setter(ToolTipService.ToolTipProperty, $"{obj.DisplayIndex},{attr.CSVName},{bind.Path.Path},{bind.StringFormat}"));
            obj.HeaderStyle = headerS;

            //https://stackoverflow.com/questions/53961533/datagrid-columns-element-style-in-codebehind-has-no-effect
            if (obj is DataGridTextColumn col)
            {
                Style elementS = null;

                if (attr.WPFHorizontalAlignment != WPFHorizontalAlignment.Left)
                {
                    elementS = new Style();
                    elementS.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, FlagOperator.ConvertTo<HorizontalAlignment>(attr.WPFHorizontalAlignment.ToString())));
                }

                if (!string.IsNullOrWhiteSpace(attr.WPFForeground))
                {
                    if (elementS == null)
                    {
                        elementS = new Style();
                    }

                    elementS.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.DemiBold));
                    elementS.Setters.Add(new Setter(TextBlock.ForegroundProperty, new BrushConverter().ConvertFromString(attr.WPFForeground)));
                }

                if (elementS != null)
                {
                    col.ElementStyle = elementS;
                }
            }
        }

        public static void SetColumn<T>(this DataGridBoundColumn obj, in T attr) where T : ColumnAttribute
        {
            if (obj.Binding is Binding bind)
            {
                obj.SetColumn(bind, attr);
            }
        }

        public static void SetColumns<T>(this DataGrid obj, in IDictionary<string, T> propertyNameMap) where T : ColumnAttribute
        {
            HashSet<string> existedBindings = new HashSet<string>();

            foreach (DataGridColumn column in obj.Columns)
            {
                if (column is DataGridBoundColumn bound && bound.Binding is Binding bind && propertyNameMap.ContainsKey(bind.Path.Path))
                {
                    existedBindings.Add(bind.Path.Path);
                }
            }

            foreach (KeyValuePair<string, T> pair in propertyNameMap)
            {
                if (pair.Value.WPFDisplayIndex < 0 || existedBindings.Contains(pair.Key))
                {
                    continue;
                }

                //https://stackoverflow.com/questions/6885711/wpf-c-bind-datagrid-column-with-code-behind
                DataGridTextColumn column = new DataGridTextColumn()
                {
                    Binding = new Binding(pair.Key),
                };

                obj.Columns.Add(column);
            }

            foreach (DataGridColumn column in obj.Columns)
            {
                if (column is DataGridBoundColumn bound && bound.Binding is Binding bind && propertyNameMap.TryGetValue(bind.Path.Path, out T attr))
                {
                    bound.SetColumn(bind, attr);
                }
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/2160481/wpf-collectionviewsource-multiple-views
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICollectionView GetView<T>(this ObservableCollection<T> obj)
        {
            CollectionViewSource vs = new CollectionViewSource() { Source = obj, };
            return vs.View;
        }

        public static ObservableCollection<T> SetViewAndGetObservation<T>(this ItemsControl obj)
        {
            ObservableCollection<T> oc = new ObservableCollection<T>();
            obj.ItemsSource = oc.GetView();
            return oc;
        }

        public static ObservableCollection<T> SetViewAndGetObservation<T>(this ItemsControl obj, in IEnumerable<T> collection)
        {
            ObservableCollection<T> oc = new ObservableCollection<T>(collection);
            obj.ItemsSource = oc.GetView();
            return oc;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/65294707/argumentoutofrangeexception-when-calling-visualtreehelper-getchild
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ScrollViewer GetScrollViewer(this DependencyObject obj)
        {
            Decorator border = VisualTreeHelper.GetChildrenCount(obj) > 0 ? VisualTreeHelper.GetChild(obj, 0) as Decorator : null;
            return border?.Child as ScrollViewer;
        }

        /// <summary>
        /// <para>https://stackoverflow.com/questions/1027051/how-to-autoscroll-on-wpf-datagrid</para>
        /// <para>https://stackoverflow.com/questions/60378552/wpf-scrolltotop-vs-scrolltohome</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="toEnd"></param>
        /// <returns></returns>
        public static bool ScrollToBorder(this DependencyObject obj, in bool toEnd = true)
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

        public static ScrollViewer ScrollToBorderHome(this DependencyObject obj)
        {
            ScrollViewer viewer = GetScrollViewer(obj);
            viewer?.ScrollToHome();
            return viewer;
        }

        public static ScrollViewer ScrollToBorderEnd(this DependencyObject obj)
        {
            ScrollViewer viewer = GetScrollViewer(obj);
            viewer?.ScrollToEnd();
            return viewer;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/3869309/how-to-find-its-owner-datagrid-and-datagridrow-from-datagridcell-in-wpf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindOwner<T>(this DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while ((parent != null) && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }

        public static T GetItem<T>(this DataGridCell obj) where T : class
        {
            DataGridRow row = obj.FindOwner<DataGridRow>();

            return row == null ? null : (T)row.Item;
        }
    }
}
