﻿using GNAy.Capital.Models;
using GNAy.Capital.Trade.Controllers;
using GNAy.Tools.NET47;
using GNAy.Tools.WPF;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GNAy.Capital.Trade
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly DateTime StartTime;
        public readonly string UniqueName;
        private readonly AppController _appCtrl;

        private readonly Dictionary<TextBox, ComboBox> _editableCBMap;

        private readonly DispatcherTimer _timer1;
        private readonly DispatcherTimer _timer2;

        public MainWindow()
        {
            InitializeComponent();

            StartTime = DateTime.Now;
            UniqueName = GetType().Name.Replace("Controller", "Ctrl");
            _appCtrl = new AppController(this);

            //https://www.796t.com/post/MWV3bG0=.html
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            Title = $"{version.Comments} ({version.FileVersion})";
            if (version.FileMajorPart <= 0 || version.FilePrivatePart % 2 == 1)
            {
                Title = $"{Title}(BETA)";
            }
            Title = $"{Title} ({version.ProductName})({version.LegalCopyright}) (PID:{_appCtrl.ProcessID})({_appCtrl.Settings.Description})";
            if (Debugger.IsAttached)
            {
                Title = $"{Title}(附加偵錯)";
            }

            _editableCBMap = new Dictionary<TextBox, ComboBox>();
            //https://github.com/punker76/MahApps.Metro.SimpleChildWindow/issues/69
            ComboBoxOrderProduct.ApplyTemplate();
            TextBox partTB1 = (TextBox)ComboBoxOrderProduct.Template.FindName("PART_EditableTextBox", ComboBoxOrderProduct);
            partTB1.GotFocus += TextBox_GotFocus;
            _editableCBMap[partTB1] = ComboBoxOrderProduct;
            ComboBoxOrderSeqNo.ApplyTemplate();
            TextBox partTB2 = (TextBox)ComboBoxOrderSeqNo.Template.FindName("PART_EditableTextBox", ComboBoxOrderSeqNo);
            partTB2.GotFocus += TextBox_GotFocus;
            _editableCBMap[partTB2] = ComboBoxOrderSeqNo;
            ComboBoxOrderBookNo.ApplyTemplate();
            TextBox partTB3 = (TextBox)ComboBoxOrderBookNo.Template.FindName("PART_EditableTextBox", ComboBoxOrderBookNo);
            partTB3.GotFocus += TextBox_GotFocus;
            _editableCBMap[partTB3] = ComboBoxOrderBookNo;

            _timer1 = new DispatcherTimer(DispatcherPriority.ContextIdle)
            {
                Interval = TimeSpan.FromMilliseconds(_appCtrl.Settings.TimerIntervalUI1),
            };
            _timer1.Tick += Timer1_Tick;
            _timer2 = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromMilliseconds(_appCtrl.Settings.TimerIntervalUI2),
            };
            _timer2.Tick += Timer2_Tick;

            StatusBarItemAA1.Text = StartTime.ToString("MM/dd HH:mm");
            TextBoxQuoteFolderTest.Text = _appCtrl.Settings.QuoteFolderPath;
            StatusBarItemAB2.Text = $"Subscribed={_appCtrl.Config.QuoteSubscribed.Count}|Live={_appCtrl.Settings.QuoteLive.Count}";

            StatusBarItemCA4.Text = string.Empty;
            if (_appCtrl.Config.IsAMMarket(StartTime))
            {
                StatusBarItemCA4.Text = $"{_appCtrl.Settings.MarketStart[(int)Market.EDayNight.AM]:MM/dd HH:mm} ~ {_appCtrl.Settings.MarketClose[(int)Market.EDayNight.AM]:MM/dd HH:mm}";
            }
            else if (!_appCtrl.Config.IsHoliday(StartTime))
            {
                StatusBarItemCA4.Text = $"{_appCtrl.Settings.MarketStart[(int)Market.EDayNight.PM]:MM/dd HH:mm} ~ {_appCtrl.Settings.MarketClose[(int)Market.EDayNight.PM].AddDays(1):MM/dd HH:mm}";
            }

            ButtonSetOrderMaxQty.IsEnabled = false;
            ButtonSetOrderMaxCount.IsEnabled = false;

            _appCtrl.LogTrace(StartTime, Title, UniqueName);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            const string methodName = nameof(Window_Activated);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                StatusBarItemAA4.Text = methodName;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer1.Stop();
            _timer2.Stop();
            _appCtrl.Exit();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            const string methodName = nameof(Window_Deactivated);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                StatusBarItemAA4.Text = methodName;
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (sender is TextBox tb)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(100);
                        this.InvokeRequired(delegate
                        {
                            tb.SelectAll();
                            if (_editableCBMap.TryGetValue(tb, out ComboBox cb))
                            {
                                cb.IsDropDownOpen = true;
                            }
                        });
                    });
                }
                else if (sender is PasswordBox pb)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(100);
                        this.InvokeRequired(delegate { pb.SelectAll(); });
                    });
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_GotMouseCapture(object sender, MouseEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!_appCtrl.Config.StartOnTime)
                    {
                        _appCtrl.LogWarn(start, $"程式沒有在正常時間啟動", UniqueName);
                    }

                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(-3):MM/dd HH:mm}|{StartTime.AddDays(-3).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(-3))}", UniqueName);
                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(-2):MM/dd HH:mm}|{StartTime.AddDays(-2).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(-2))}", UniqueName);
                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(-1):MM/dd HH:mm}|{StartTime.AddDays(-1).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(-1))}", UniqueName);
                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(0):MM/dd HH:mm}|{StartTime.AddDays(+0).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(0))}|Today", UniqueName);
                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(1):MM/dd HH:mm}|{StartTime.AddDays(+1).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(1))}", UniqueName);
                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(2):MM/dd HH:mm}|{StartTime.AddDays(+2).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(2))}", UniqueName);
                    _appCtrl.LogTrace(start, $"{StartTime.AddDays(3):MM/dd HH:mm}|{StartTime.AddDays(+3).DayOfWeek}|IsHoliday={_appCtrl.Config.IsHoliday(StartTime.AddDays(3))}", UniqueName);

                    _appCtrl.LogTrace(start, $"{_appCtrl.Config.Archive.FullName}", UniqueName);
                    _appCtrl.LogTrace(start, $"{_appCtrl.Config.Archive.Name}|Version={_appCtrl.Config.Version}|Exists={_appCtrl.Config.Archive.Exists}", UniqueName);

                    _appCtrl.LogTrace(start, $"AutoRun={_appCtrl.Settings.AutoRun}", UniqueName);
                }
                catch (Exception ex)
                {
                    _appCtrl.LogException(start, ex, ex.StackTrace);
                }
                finally
                {
                    _appCtrl.EndTrace(start, UniqueName);
                }
            });

            try
            {
                if (!_appCtrl.Config.Archive.Exists)
                {
                    //https://docs.microsoft.com/zh-tw/dotnet/desktop/wpf/windows/how-to-open-message-box?view=netdesktop-6.0

                    string caption = $"第一次產生設定檔{_appCtrl.Config.Archive.Name}";
                    string messageBoxText = $"請確認檔案內容\r\n{_appCtrl.Config.Archive.FullName}";

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    _appCtrl.Exit(caption, LogLevel.Warn);
                    return;
                }
                else if (_appCtrl.Settings.AutoRun)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1 * 1000);
                        this.InvokeRequired(delegate { Timer2_Tick(null, null); });
                    });
                }

                _timer1.Start();

                if (!_appCtrl.Settings.AutoRun)
                {
                    _timer2.Start();
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_LostMouseCapture(object sender, MouseEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //https://stackoverflow.com/questions/24214804/does-wpf-have-mouse-wheel-scrolling-up-and-down-event
                if (e.Delta == 0)
                {
                    return;
                }

                ComboBox cb = null;

                if (ComboBoxTriggerProduct.IsMouseOver && !ComboBoxTriggerProduct.IsFocused)
                {
                    cb = ComboBoxTriggerProduct;
                }
                else if (ComboBoxTriggerColumn.IsMouseOver && !ComboBoxTriggerColumn.IsFocused)
                {
                    cb = ComboBoxTriggerColumn;
                }
                else if (ComboBoxTriggerCancel.IsMouseOver && !ComboBoxTriggerCancel.IsFocused)
                {
                    cb = ComboBoxTriggerCancel;
                }
                else if (ComboBoxOrderAccs.IsMouseOver && !ComboBoxOrderAccs.IsFocused)
                {
                    cb = ComboBoxOrderAccs;
                }
                else if (ComboBoxOrderProduct.IsMouseOver && !ComboBoxOrderProduct.IsFocused)
                {
                    cb = ComboBoxOrderProduct;
                }
                else if (ComboBoxOrderBuySell.IsMouseOver && !ComboBoxOrderBuySell.IsFocused)
                {
                    cb = ComboBoxOrderBuySell;
                }
                else if (ComboBoxOrderTradeType.IsMouseOver && !ComboBoxOrderTradeType.IsFocused)
                {
                    cb = ComboBoxOrderTradeType;
                }
                else if (ComboBoxOrderDayTrade.IsMouseOver && !ComboBoxOrderDayTrade.IsFocused)
                {
                    cb = ComboBoxOrderDayTrade;
                }
                else if (ComboBoxOrderPositionKind.IsMouseOver && !ComboBoxOrderPositionKind.IsFocused)
                {
                    cb = ComboBoxOrderPositionKind;
                }

                if (cb == null)
                {
                    return;
                }

                int offset = cb.SelectedIndex + ((e.Delta > 0) ? -1 : 1);

                if (offset >= 0 && offset < cb.Items.Count)
                {
                    cb.SelectedIndex = offset;
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //_appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Window_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //https://stackoverflow.com/questions/9565740/display-duration-in-milliseconds
                string elapsed = (start - StartTime).ToString("hh':'mm':'ss");
                StatusBarItemAA2.Text = $"{elapsed}";

                //if (Application.Current.MainWindow.IsMouseOver)
                if (IsMouseOver)
                {
                    //https://stackoverflow.com/questions/29822020/how-to-get-mouse-position-on-screen-in-wpf
                    //Point pt = Mouse.GetPosition(Application.Current.MainWindow);
                    Point pt = Mouse.GetPosition(this);
                    StatusBarItemAA3.Text = $"({Width},{Height},{pt.X},{pt.Y})";

                    if (StatusBarItemAA1.IsMouseOver || StatusBarItemBA1.IsMouseOver || StatusBarItemCA1.IsMouseOver) //GridAA.IsMouseOver || GridBA.IsMouseOver || GridCA.IsMouseOver
                    {
                        if (ColumnDef.Width.Value < Width - 350)
                        {
                            ColumnDef.Width = new GridLength(ColumnDef.Width.Value + 50, ColumnDef.Width.GridUnitType);
                        }
                    }
                    else if (StatusBarItemAA2.IsMouseOver || StatusBarItemBA2.IsMouseOver || StatusBarItemCA2.IsMouseOver) //GridAB.IsMouseOver || GridBB.IsMouseOver || GridCB.IsMouseOver
                    {
                        if (ColumnDef.Width.Value > 350)
                        {
                            ColumnDef.Width = new GridLength(ColumnDef.Width.Value - 50, ColumnDef.Width.GridUnitType);
                        }
                    }
                }

                if (TabControlBA.SelectedIndex == 0 && DataGridAPIReply.ItemsSource != null)
                {
                    StatusBarItemBA1.Text = $"({DataGridAPIReply.Columns.Count},{DataGridAPIReply.Items.Count})";
                }

                if (TabControlCA.SelectedIndex == 0 && DataGridAppLog.ItemsSource != null)
                {
                    StatusBarItemCA1.Text = $"({DataGridAppLog.Columns.Count},{DataGridAppLog.Items.Count})";
                }

                if (_appCtrl.Capital != null)
                {
                    StatusBarItemBA3.Text = $"{_appCtrl.Capital.UserIDTimer.Item1:mm:ss}|{_appCtrl.Capital.UserIDTimer.Item2}";
                    StatusBarItemBA4.Text = _appCtrl.Capital.QuoteTimer;
                    StatusBarItemAB5.Text = _appCtrl.Capital.QuoteStatusStr;
                    StatusBarItemAB3.Text = $"{_appCtrl.Capital.QuoteLastUpdated.Name}|{_appCtrl.Capital.QuoteLastUpdated.UpdateTime:mm:ss.fff}|{_appCtrl.Capital.QuoteLastUpdated.Updater}";

                    elapsed = (start - _appCtrl.Capital.QuoteLastUpdated.UpdateTime).ToString("mm':'ss'.'fff");
                    StatusBarItemAA2.Text = $"{StatusBarItemAA2.Text}|{elapsed}";

                    StatusBarItemCB3.Text = _appCtrl.Capital.OrderNotice;
                }

                StatusBarItemCA3.Text = $"BG={_appCtrl.SignalTimeBG:ss.fff}|Trigger={_appCtrl.SignalTimeTrigger:ss.fff}|Strategy={_appCtrl.SignalTimeStrategy:ss.fff}";

                if (TabControlAB.SelectedIndex == 0 && DataGridQuoteSubscribed.ItemsSource != null)
                {
                    StatusBarItemAB1.Text = $"({DataGridQuoteSubscribed.Columns.Count},{DataGridQuoteSubscribed.Items.Count})";
                }

                if (TabControlBB.SelectedIndex == 0 && DataGridTriggerRule.ItemsSource != null)
                {
                    StatusBarItemBB1.Text = $"({DataGridTriggerRule.Columns.Count},{DataGridTriggerRule.Items.Count})";
                }
                else if (TabControlBB.SelectedIndex == 1 && DataGridStrategyRule.ItemsSource != null)
                {
                    StatusBarItemBB1.Text = $"({DataGridStrategyRule.Columns.Count},{DataGridStrategyRule.Items.Count})";
                }

                if (_appCtrl.Strategy != null)
                {
                    StatusBarItemBB3.Text = _appCtrl.Strategy.Notice;
                }

                if (TabControlCB.SelectedIndex == 0 && DataGridOrderDetail.ItemsSource != null)
                {
                    StatusBarItemCB1.Text = $"({DataGridOrderDetail.Columns.Count},{DataGridOrderDetail.Items.Count})";
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            _timer2.Stop();

            DateTime start = _appCtrl.StartTrace();
            int reConnect = 0;

            try
            {
                StatusBarItemCA2.Text = $"{start:HH:mm:ss}|IsHoliday={_appCtrl.Config.IsHoliday(start)}";
                ButtonSaveQuotesTest_Click(null, null);

                foreach (DateTime timeToClose in _appCtrl.Settings.MarketClose)
                {
                    if (start.Hour == timeToClose.Hour && start.Minute >= (timeToClose.Minute + 2) && start.Minute <= (timeToClose.Minute + 4))
                    {
                        _timer1.Stop();
                        Thread.Sleep(3 * 1000);
                        _appCtrl.Exit($"Time to exit.");
                        return;
                    }
                }

                if (_appCtrl.Settings.AutoRun && _appCtrl.Capital == null)
                {
                    reConnect = 1 + StatusCode.BaseTraceValue;
                }
                //2020 SK_WARNING_PRECHECK_RESULT_FAIL Precheck 失敗(EX:RCode)
                //2021 SK_WARNING_PRECHECK_RESULT_EMPTY Precheck結果回傳空值
                //3002 SK_SUBJECT_CONNECTION_DISCONNECT 斷線
                //3021 SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK 連線失敗(網路異常等)
                //3022 SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL Solace底層連線錯誤
                //3033 SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR Solace Sessio down錯誤
                else if (_appCtrl.Capital != null && (_appCtrl.Capital.QuoteStatus == 2020 ||
                    _appCtrl.Capital.QuoteStatus == 2021 ||
                    _appCtrl.Capital.QuoteStatus == 3002 ||
                    _appCtrl.Capital.QuoteStatus == 3021 ||
                    _appCtrl.Capital.QuoteStatus == 3022 ||
                    _appCtrl.Capital.QuoteStatus == 3033))
                {
                    reConnect = _appCtrl.Capital.QuoteStatus + StatusCode.BaseErrorValue;
                }
                else if (_appCtrl.Capital != null && _appCtrl.Capital.QuoteStatus > StatusCode.BaseTraceValue)
                {
                    reConnect = _appCtrl.Capital.QuoteStatus;
                }

                if (reConnect == 0)
                {
                    _timer2.Start();
                    return;
                }

                _appCtrl.Log(reConnect, $"Retry to connect quote service.|reConnect={reConnect}", UniqueName, DateTime.Now - start);
                Task.Factory.StartNew(() =>
                {
                    if (_appCtrl.Capital != null)
                    {
                        Thread.Sleep(1 * 1000);
                        _appCtrl.Capital.Disconnect();
                    }

                    Thread.Sleep(2 * 1000);
                    this.InvokeRequired(delegate { ButtonLoginUser_Click(null, null); });

                    Thread.Sleep(4 * 1000);
                    SpinWait.SpinUntil(() => _appCtrl.Capital.LoginUserResult == 0 || (_appCtrl.Capital.LoginUserResult >= 600 && _appCtrl.Capital.LoginUserResult <= 699), 1 * 60 * 1000);
                    this.InvokeRequired(delegate
                    {
                        ButtonLoginQuote_Click(null, null);
                        ButtonReadCertification_Click(null, null);
                    });

                    Thread.Sleep(4 * 1000);
                    SpinWait.SpinUntil(() => _appCtrl.Capital.QuoteStatus == StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY, 1 * 60 * 1000);
                    if (_appCtrl.Capital.QuoteStatus != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY) //Timeout
                    {
                        //TODO: Send alert mail.
                        _appCtrl.Capital.Disconnect();
                        this.InvokeRequired(delegate { _timer2.Start(); }); //Retry to connect quote service.
                        return;
                    }

                    Thread.Sleep(4 * 1000);
                    this.InvokeRequired(delegate
                    {
                        ButtonIsConnected_Click(null, null);
                        //ButtonPrintProductList_Click(null, null);
                        ButtonGetProductInfo_Click(null, null);
                        ButtonSubQuotes_Click(null, null);
                        ButtonGetOrderAccs_Click(null, null);
                    });

                    Thread.Sleep(4 * 1000);
                    SpinWait.SpinUntil(() => _appCtrl.Capital.OrderAccCount > 0, 8 * 1000);
                    _appCtrl.Capital.GetOpenInterestAsync();
                    _appCtrl.Capital.UnlockOrder();
                    _appCtrl.Capital.SetOrderMaxQty();
                    _appCtrl.Capital.SetOrderMaxCount();
                    _appCtrl.Trigger.RecoverSetting();
                    _appCtrl.Strategy.RecoverSetting();

                    Thread.Sleep(2 * 1000);
                    this.InvokeRequired(delegate { _timer2.Start(); });
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _timer2.Start();
            }
        }

        private void ButtonLoginUser_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (string.IsNullOrWhiteSpace(TextBoxUserID.Text) && string.IsNullOrWhiteSpace(DWPBox.Password))
                {
                    FileInfo dwpFile = new FileInfo($"{_appCtrl.ProcessName}.dwp.config");

                    if (dwpFile.Exists)
                    {
                        foreach (string line in File.ReadAllLines(dwpFile.FullName, TextEncoding.UTF8WithoutBOM))
                        {
                            if (line.StartsWith("userid=", StringComparison.OrdinalIgnoreCase))
                            {
                                TextBoxUserID.Text = line.Substring("userid=".Length).Trim().ToUpper();
                            }
                            else if (line.StartsWith("dwp=", StringComparison.OrdinalIgnoreCase))
                            {
                                DWPBox.Password = line.Substring("dwp=".Length).Trim();
                            }
                        }
                    }
                }

                _appCtrl.InitialCapital();
                _appCtrl.Capital.LoginUser(TextBoxUserID.Text, DWPBox.Password);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonLoginQuote_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.LoginQuoteAsync(DWPBox.Password);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonIsConnected_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                (LogLevel, string) apiMsg = _appCtrl.Capital.IsConnected();
                StatusBarItemAB4.Text = apiMsg.Item1 == LogLevel.Trace ? apiMsg.Item2 : $"{apiMsg.Item1}|{apiMsg.Item2}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.Disconnect();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonPrintProductList_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.PrintProductList();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonGetProductInfo_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.GetProductInfo();
                StatusBarItemAB2.Text = $"Sub={_appCtrl.Config.QuoteSubscribed.Count}|Live={_appCtrl.Settings.QuoteLive.Count}|QuoteFile={_appCtrl.Capital.QuoteFileNameBase}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSubQuotes_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.SubQuotesAsync();
                _appCtrl.SetTriggerRule();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonRecoverQuotes_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.RecoverQuotesAsync(TextBoxRecoverQuotes.Text);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonRequestKLine_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.RequestKLine();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSaveQuotesTest_Click(object sender, RoutedEventArgs e)
        {
            if (_appCtrl.Capital == null)
            {
                return;
            }
            else if (string.IsNullOrWhiteSpace(TextBoxQuoteFolderTest.Text))
            {
                return;
            }

            DateTime start = _appCtrl.StartTrace();

            try
            {
                DirectoryInfo folder = new DirectoryInfo(TextBoxQuoteFolderTest.Text);
                folder.Create();
                folder.Refresh();
                _appCtrl.Capital.SaveQuotesAsync(folder);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonReadCertification_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.ReadCertification();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonGetOrderAccs_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.GetOrderAccs();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonUnlockOrder_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.UnlockOrder();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonGetOpenInterest_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                OrderAccData acc = (OrderAccData)ComboBoxOrderAccs.SelectedItem;

                if (acc.MarketType != Market.EType.Futures)
                {
                    return;
                }

                _appCtrl.Capital.GetOpenInterestAsync(acc.FullAccount);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSetOrderMaxQty_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.SetOrderMaxQty();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSetOrderMaxCount_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.SetOrderMaxCount();
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonCancelTrigger_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (TabControlBB.SelectedIndex != 0)
                {
                    TabControlBB.SelectedIndex = 0;
                    _appCtrl.LogWarn(start, "防呆，再次確認，避免看錯", UniqueName);
                    StatusBarItemBB2.Text = "防呆，再次確認，避免看錯";
                    return;
                }

                _appCtrl.Trigger.Cancel(TextBoxTriggerPrimaryKey.Text);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSaveTriggerRule_Click(object sender, RoutedEventArgs e)
        {
            const string methodName = nameof(ButtonSaveTriggerRule_Click);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (TabControlBB.SelectedIndex != 0)
                {
                    TabControlBB.SelectedIndex = 0;
                    _appCtrl.LogWarn(start, "防呆，再次確認，避免看錯", UniqueName);
                    StatusBarItemBB2.Text = "防呆，再次確認，避免看錯";
                    return;
                }

                QuoteData selectedQuote = (QuoteData)ComboBoxTriggerProduct.SelectedItem;

                TriggerData trigger = new TriggerData((TradeColumnTrigger)ComboBoxTriggerColumn.SelectedItem)
                {
                    PrimaryKey = TextBoxTriggerPrimaryKey.Text,
                    Quote = selectedQuote,
                    Symbol = selectedQuote.Symbol,
                    Rule = TextBoxTriggerRuleValue.Text,
                    Cancel = ComboBoxTriggerCancel.SelectedIndex,
                    StrategyOR = TextBoxTriggerStrategyOR.Text,
                    StrategyAND = TextBoxTriggerStrategyAND.Text,
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                _appCtrl.Trigger.AddRule(trigger, TextBoxTriggerTimeDuration.Text);

                string primaryKey = TextBoxTriggerPrimaryKey.Text.Replace(" ", string.Empty);

                if (!decimal.TryParse(primaryKey, out decimal pk))
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_appCtrl.Settings.TimerIntervalTrigger * 3);
                    if (_appCtrl.Trigger[primaryKey] == null)
                    {
                        return;
                    }

                    this.InvokeRequired(delegate
                    {
                        if (pk < _appCtrl.Trigger.Count)
                        {
                            pk = _appCtrl.Trigger.Count;
                        }

                        ++pk;

                        if (_appCtrl.Trigger[$"{pk}"] == null)
                        {
                            TextBoxTriggerPrimaryKey.Text = $"{pk}";
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void DataGridTriggerRuleCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                TriggerData trigger = ((DataGridCell)sender).GetItem<TriggerData>();
                _appCtrl.LogTrace(start, trigger.ToLog(), UniqueName);

                ComboBoxTriggerProduct.SelectedIndex = -1;
                for (int i = 0; i < ComboBoxTriggerProduct.Items.Count; ++i)
                {
                    ComboBoxTriggerProduct.SelectedIndex = i;
                    if (ComboBoxTriggerProduct.SelectedItem is QuoteData quote && quote == trigger.Quote)
                    {
                        if (quote.Symbol == trigger.Symbol)
                        {
                            break;
                        }
                        else
                        {
                            _appCtrl.LogError(start, $"Trigger|觸價關聯報價代碼錯誤|quote.Symbol{quote.Symbol} != trigger.Symbol{trigger.Symbol}|{trigger.ToLog()}", UniqueName);
                            ComboBoxTriggerProduct.SelectedIndex = -1;
                            break;
                        }
                    }
                }
                if (ComboBoxTriggerProduct.SelectedIndex < 0)
                {
                    _appCtrl.LogError(start, $"Trigger|觸價關聯報價代碼錯誤|{trigger.ToLog()}", UniqueName);
                }

                ComboBoxTriggerColumn.SelectedIndex = -1;
                for (int i = 0; i < ComboBoxTriggerColumn.Items.Count; ++i)
                {
                    ComboBoxTriggerColumn.SelectedIndex = i;
                    if (ComboBoxTriggerColumn.SelectedItem is TradeColumnTrigger column && column.Property == trigger.Column.Property)
                    {
                        if (column.Property.Name == trigger.ColumnProperty)
                        {
                            break;
                        }
                        else
                        {
                            _appCtrl.LogError(start, $"Trigger|觸價關聯報價欄位錯誤|column.Property.Name{column.Property.Name} != trigger.ColumnProperty{trigger.ColumnProperty}|{trigger.ToLog()}", UniqueName);
                            ComboBoxTriggerColumn.SelectedIndex = -1;
                            break;
                        }
                    }
                }
                if (ComboBoxTriggerColumn.SelectedIndex < 0)
                {
                    _appCtrl.LogError(start, $"Trigger|觸價關聯報價欄位錯誤|{trigger.ToLog()}", UniqueName);
                }

                ComboBoxTriggerCancel.SelectedIndex = trigger.Cancel;
                TextBoxTriggerPrimaryKey.Text = trigger.PrimaryKey;
                TextBoxTriggerRuleValue.Text = $"{trigger.Rule}{trigger.TargetValue:0.00####}";
                TextBoxTriggerStrategyOR.Text = trigger.StrategyOR;
                TextBoxTriggerStrategyAND.Text = trigger.StrategyAND;

                TextBoxTriggerTimeDuration.Text = string.Empty;
                if (trigger.StartTime.HasValue)
                {
                    TextBoxTriggerTimeDuration.Text = $"{trigger.StartTime.Value:HHmmss}";
                }
                if (trigger.EndTime.HasValue)
                {
                    TextBoxTriggerTimeDuration.Text = $"{TextBoxTriggerTimeDuration.Text}~{trigger.EndTime.Value:HHmmss}";
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void DataGridStrategyRuleCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                StrategyData strategy = ((DataGridCell)sender).GetItem<StrategyData>();
                _appCtrl.LogTrace(start, strategy.ToLog(), UniqueName);
                
                TextBoxStrategyPrimaryKey.Text = strategy.PrimaryKey;
                TextBoxStrategyStopLoss.Text = strategy.StopLossBefore;
                TextBoxStrategyStopWin.Text = strategy.StopWinBefore;
                TextBoxStrategyMoveStopWin.Text = strategy.MoveStopWinBefore;

                ComboBoxOrderAccs.SelectedIndex = -1;
                for (int i = 0; i < ComboBoxOrderAccs.Items.Count; ++i)
                {
                    ComboBoxOrderAccs.SelectedIndex = i;
                    if (ComboBoxOrderAccs.SelectedItem is OrderAccData orderAcc && orderAcc.FullAccount == strategy.FullAccount)
                    {
                        break;
                    }
                }
                if (ComboBoxOrderAccs.SelectedIndex < 0)
                {
                    _appCtrl.LogError(start, $"Strategy|策略關聯帳號錯誤|{strategy.ToLog()}", UniqueName);
                }

                ComboBoxOrderProduct.SelectedIndex = -1;
                for (int i = 0; i < ComboBoxOrderProduct.Items.Count; ++i)
                {
                    ComboBoxOrderProduct.SelectedIndex = i;
                    if (ComboBoxOrderProduct.SelectedItem is string symbol && symbol == strategy.Symbol)
                    {
                        break;
                    }
                }
                if (ComboBoxOrderProduct.SelectedIndex < 0)
                {
                    _appCtrl.LogError(start, $"Strategy|策略關聯報價代碼錯誤|{strategy.ToLog()}", UniqueName);
                }

                ComboBoxOrderBuySell.SelectedIndex = strategy.BS;
                ComboBoxOrderTradeType.SelectedIndex = strategy.TradeType;
                ComboBoxOrderDayTrade.SelectedIndex = strategy.DayTrade;
                ComboBoxOrderPositionKind.SelectedIndex = strategy.Position;
                TextBoxOrderPrice.Text = strategy.OrderPriceBefore;
                TextBoxOrderQuantity.Text = $"{strategy.OrderQty}";
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void DataGridOrderDetailCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonCancelStrategy_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (TabControlBB.SelectedIndex != 1)
                {
                    TabControlBB.SelectedIndex = 1;
                    _appCtrl.LogWarn(start, "防呆，再次確認，避免看錯", UniqueName);
                    StatusBarItemBB2.Text = "防呆，再次確認，避免看錯";
                    return;
                }

                _appCtrl.Strategy.Cancel(TextBoxStrategyPrimaryKey.Text);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSaveStrategyRule_Click(object sender, RoutedEventArgs e)
        {
            const string methodName = nameof(ButtonSaveStrategyRule_Click);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (TabControlBB.SelectedIndex != 1)
                {
                    TabControlBB.SelectedIndex = 1;
                    _appCtrl.LogWarn(start, "防呆，再次確認，避免看錯", UniqueName);
                    StatusBarItemBB2.Text = "防呆，再次確認，避免看錯";
                    return;
                }

                OrderAccData acc = (OrderAccData)ComboBoxOrderAccs.SelectedItem;

                StrategyData strategy = new StrategyData()
                {
                    PrimaryKey = TextBoxStrategyPrimaryKey.Text,
                    MarketType = acc.MarketType,
                    Branch = acc.Branch,
                    Account = acc.Account,
                    Symbol = ComboBoxOrderProduct.Text,
                    BS = (short)ComboBoxOrderBuySell.SelectedIndex,
                    TradeType = (short)ComboBoxOrderTradeType.SelectedIndex,
                    DayTrade = (short)ComboBoxOrderDayTrade.SelectedIndex,
                    Position = (short)ComboBoxOrderPositionKind.SelectedIndex,
                    OrderPriceBefore = TextBoxOrderPrice.Text,
                    OrderQty = int.Parse(TextBoxOrderQuantity.Text),
                    StopLossBefore = TextBoxStrategyStopLoss.Text,
                    StopWinBefore = TextBoxStrategyStopWin.Text,
                    MoveStopWinBefore = TextBoxStrategyMoveStopWin.Text,
                    TriggerAfterStopLoss = TextBoxTriggerAfterStopLoss.Text,
                    StrategyAfterStopLoss = TextBoxStrategyAfterStopLoss.Text,
                    //TODO
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                _appCtrl.Strategy.AddRuleAsync(strategy);

                if (!decimal.TryParse(TextBoxStrategyPrimaryKey.Text.Replace(" ", string.Empty), out decimal pk))
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_appCtrl.Settings.TimerIntervalStrategy * 3);
                    if (_appCtrl.Strategy[strategy.PrimaryKey] == null)
                    {
                        return;
                    }

                    this.InvokeRequired(delegate
                    {
                        if (pk < _appCtrl.Strategy.Count)
                        {
                            pk = _appCtrl.Strategy.Count;
                        }

                        ++pk;

                        if (_appCtrl.Strategy[$"{pk}"] == null)
                        {
                            TextBoxStrategyPrimaryKey.Text = $"{pk}";
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonSendFutureOrder_Click(object sender, RoutedEventArgs e)
        {
            const string methodName = nameof(ButtonSendFutureOrder_Click);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                OrderAccData acc = (OrderAccData)ComboBoxOrderAccs.SelectedItem;

                StrategyData order = new StrategyData()
                {
                    MarketType = acc.MarketType,
                    Branch = acc.Branch,
                    Account = acc.Account,
                    Symbol = ComboBoxOrderProduct.Text,
                    BS = (short)ComboBoxOrderBuySell.SelectedIndex,
                    TradeType = (short)ComboBoxOrderTradeType.SelectedIndex,
                    DayTrade = (short)ComboBoxOrderDayTrade.SelectedIndex,
                    Position = (short)ComboBoxOrderPositionKind.SelectedIndex,
                    OrderPriceBefore = TextBoxOrderPrice.Text,
                    OrderQty = int.Parse(TextBoxOrderQuantity.Text),
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                _appCtrl.Capital.SendFutureOrderAsync(order);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonStartFutureStartegyNow_Click(object sender, RoutedEventArgs e)
        {
            const string methodName = nameof(ButtonStartFutureStartegyNow_Click);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (TabControlBB.SelectedIndex != 1)
                {
                    TabControlBB.SelectedIndex = 1;
                    _appCtrl.LogWarn(start, "防呆，再次確認，避免看錯", UniqueName);
                    StatusBarItemBB2.Text = "防呆，再次確認，避免看錯";
                    return;
                }

                if (DataGridStrategyRule.SelectedCells.Count > 0 && DataGridStrategyRule.SelectedCells[0].Item is StrategyData strategy)
                {
                    if (strategy.PrimaryKey == TextBoxStrategyPrimaryKey.Text.Trim() && strategy.StatusEnum == StrategyStatus.Enum.Waiting)
                    {
                        _appCtrl.Strategy.StartNow(strategy.PrimaryKey);
                        return;
                    }
                }

                OrderAccData acc = (OrderAccData)ComboBoxOrderAccs.SelectedItem;

                strategy = new StrategyData()
                {
                    PrimaryKey = TextBoxStrategyPrimaryKey.Text,
                    MarketType = acc.MarketType,
                    Branch = acc.Branch,
                    Account = acc.Account,
                    Symbol = ComboBoxOrderProduct.Text,
                    BS = (short)ComboBoxOrderBuySell.SelectedIndex,
                    TradeType = (short)ComboBoxOrderTradeType.SelectedIndex,
                    DayTrade = (short)ComboBoxOrderDayTrade.SelectedIndex,
                    Position = (short)ComboBoxOrderPositionKind.SelectedIndex,
                    OrderPriceBefore = TextBoxOrderPrice.Text,
                    OrderQty = int.Parse(TextBoxOrderQuantity.Text),
                    StopLossBefore = TextBoxStrategyStopLoss.Text,
                    StopWinBefore = TextBoxStrategyStopWin.Text,
                    MoveStopWinBefore = TextBoxStrategyMoveStopWin.Text,
                    TriggerAfterStopLoss = TextBoxTriggerAfterStopLoss.Text,
                    StrategyAfterStopLoss = TextBoxStrategyAfterStopLoss.Text,
                    //TODO
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                _appCtrl.Strategy.StartFutureStartegyAsync(strategy);

                if (!decimal.TryParse(TextBoxStrategyPrimaryKey.Text.Replace(" ", string.Empty), out decimal pk))
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_appCtrl.Settings.TimerIntervalStrategy * 3);
                    if (_appCtrl.Strategy[strategy.PrimaryKey] == null)
                    {
                        return;
                    }

                    this.InvokeRequired(delegate
                    {
                        if (pk < _appCtrl.Strategy.Count)
                        {
                            pk = _appCtrl.Strategy.Count;
                        }

                        ++pk;

                        if (_appCtrl.Strategy[$"{pk}"] == null)
                        {
                            TextBoxStrategyPrimaryKey.Text = $"{pk}";
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonCancelOrderBySeqNo_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Capital.CancelOrderBySeqNo((OrderAccData)ComboBoxOrderAccs.SelectedItem, ComboBoxOrderSeqNo.Text);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }

        private void ButtonCancelOrderByBookNo_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                //
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
            finally
            {
                _appCtrl.EndTrace(start, UniqueName);
            }
        }
    }
}
