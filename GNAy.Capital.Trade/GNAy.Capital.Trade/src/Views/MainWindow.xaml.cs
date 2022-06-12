using GNAy.Capital.Models;
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

        public readonly string ProcessName;
        public readonly int ProcessID;

        private readonly AppController _appCtrl;

        private readonly Dictionary<TextBox, ComboBox> _editableCBMap;

        private readonly DispatcherTimer _timer1;
        private readonly DispatcherTimer _timer2;

        public MainWindow()
        {
            StartTime = DateTime.Now;
            UniqueName = nameof(MainWindow);

            Process ps = Process.GetCurrentProcess();
            ProcessName = ps.ProcessName.Replace(".vshost", string.Empty);
            ProcessID = ps.Id;

            foreach (Process other in Process.GetProcessesByName(ProcessName))
            {
                try
                {
                    if (other.MainModule.FileName == ps.MainModule.FileName && other.Id != ps.Id) //同路徑的執行檔只能存在一個實體
                    {
                        Environment.Exit(0);
                    }
                }
                catch
                { }
            }

            InitializeComponent();

            //https://www.796t.com/post/MWV3bG0=.html
            FileInfo assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(assemblyFile.FullName);

            _appCtrl = new AppController(this, ps);

            Title = $"{version.Comments} ({version.FileVersion})";
            if (version.FileMajorPart <= 0 || version.FilePrivatePart % 2 == 1)
            {
                Title = $"{Title}(BETA)";
            }
            Title = $"{Title} ({version.ProductName})({version.LegalCopyright}) ({assemblyFile.Directory.Name}\\{assemblyFile.Name})({ps.Id})({_appCtrl.Settings.Description})";
            if (Debugger.IsAttached)
            {
                Title = $"{Title}(附加偵錯)";
            }

            _editableCBMap = new Dictionary<TextBox, ComboBox>();
            if (ComboBoxTriggerProduct1.IsEditable)
            {
                TextBox partTB = ComboBoxTriggerProduct1.GetEditableTextBox();
                partTB.GotFocus += TextBox_GotFocus;
                _editableCBMap[partTB] = ComboBoxTriggerProduct1;
            }
            if (ComboBoxTriggerProduct2.IsEditable)
            {
                TextBox partTB = ComboBoxTriggerProduct2.GetEditableTextBox();
                partTB.GotFocus += TextBox_GotFocus;
                _editableCBMap[partTB] = ComboBoxTriggerProduct2;
            }
            if (ComboBoxOrderProduct.IsEditable)
            {
                TextBox partTB = ComboBoxOrderProduct.GetEditableTextBox();
                partTB.GotFocus += TextBox_GotFocus;
                _editableCBMap[partTB] = ComboBoxOrderProduct;
            }
            if (ComboBoxOrderSeqNo.IsEditable)
            {
                TextBox partTB = ComboBoxOrderSeqNo.GetEditableTextBox();
                partTB.GotFocus += TextBox_GotFocus;
                _editableCBMap[partTB] = ComboBoxOrderSeqNo;
            }

            StatusBarItemAA1.Text = StartTime.ToString("MM/dd HH:mm");
            TextBoxQuoteFolderTest.Text = _appCtrl.Settings.QuoteFolderPath;
            StatusBarItemAB2.Text = $"Subscribed={_appCtrl.Config.QuoteSubscribed.Count}|Live={_appCtrl.Settings.QuoteLive.Count}";

            ButtonSetOrderMaxQty.IsEnabled = false;
            ButtonSetOrderMaxCount.IsEnabled = false;

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
            string caption = $"確定關閉？";
            string messageBoxText = $"確定關閉？";

            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

            if (result != MessageBoxResult.OK)
            {
                e.Cancel = true;
                return;
            }

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
                        this.InvokeAsync(delegate
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
                        this.InvokeAsync(delegate { pb.SelectAll(); });
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

            Task.Factory.StartNew(() => _appCtrl.SelfTest());

            try
            {
                _appCtrl.LogTrace(start, $"{_appCtrl.Config.Archive.FullName}", UniqueName);
                _appCtrl.LogTrace(start, $"{_appCtrl.Config.Archive.Name}|Version={_appCtrl.Config.Version}|Exists={_appCtrl.Config.Archive.Exists}", UniqueName);

                _appCtrl.LogTrace(start, $"AutoRun={_appCtrl.Config.AutoRun}", UniqueName);

                CheckBoxSendRealOrder.IsChecked = _appCtrl.Settings.SendRealOrder;

                if (_appCtrl.Settings.LiveMode)
                {
                    TextBoxUserID.Visibility = Visibility.Collapsed;
                    ComboBoxOrderAccs.Visibility = Visibility.Collapsed;
                }

                CheckBoxStartFromOpenInterest.IsChecked = _appCtrl.Settings.StartFromOpenInterest;

                if (!_appCtrl.Config.Archive.Exists)
                {
                    //https://docs.microsoft.com/zh-tw/dotnet/desktop/wpf/windows/how-to-open-message-box?view=netdesktop-6.0

                    string caption = $"第一次產生設定檔{_appCtrl.Config.Archive.Name}";
                    string messageBoxText = $"請確認檔案內容\r\n{_appCtrl.Config.Archive.FullName}";

                    MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                    _appCtrl.Exit(caption, LogLevel.Warn);

                    return;
                }
                else if (_appCtrl.Config.AutoRun)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1 * 1000);
                        this.InvokeAsync(delegate { Timer2_Tick(null, null); });
                    });
                }

                _timer1.Start();

                if (!_appCtrl.Config.AutoRun)
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

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (_appCtrl.CAPQuote == null)
                {
                    return;
                }
                else if (sender is ComboBox cb)
                {
                    string[] cells = cb.Text.Split(',');

                    if (cells.Length <= 0 || cells.Length > 1 || string.IsNullOrWhiteSpace(cells[0]))
                    {
                        return;
                    }

                    (int, SKCOMLib.SKSTOCKLONG) product = _appCtrl.CAPQuote.GetProductInfo(cells[0], start);

                    if (product.Item1 != 0)
                    {
                        return;
                    }

                    cb.Text = $"{product.Item2.bstrStockNo},{product.Item2.bstrStockName},{(Market.EGroup)(product.Item2.bstrMarketNo[0] - '0')}";
                }
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
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

        private void ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                ComboBox_LostFocus(sender, e);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
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

                if (ComboBoxTriggerProduct1.IsMouseOver && !ComboBoxTriggerProduct1.IsFocused)
                {
                    cb = ComboBoxTriggerProduct1;
                }
                else if (ComboBoxTriggerProduct2.IsMouseOver && !ComboBoxTriggerProduct2.IsFocused)
                {
                    cb = ComboBoxTriggerProduct2;
                }
                else if (ComboBoxTriggerColumn.IsMouseOver && !ComboBoxTriggerColumn.IsFocused)
                {
                    cb = ComboBoxTriggerColumn;
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
                if (!_appCtrl.CallTimedEventInBG)
                {
                    _appCtrl.OnTimedEvent(start);
                }

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

                if (_appCtrl.CAPQuote != null)
                {
                    elapsed = (start - _appCtrl.CAPQuote.LastData.UpdateTime).ToString("mm':'ss'.'fff");
                    StatusBarItemAA2.Text = $"{StatusBarItemAA2.Text}|{elapsed}";

                    StatusBarItemBA3.Text = $"{_appCtrl.CAPCenter.UserIDTimer.Item1:mm:ss}|{_appCtrl.CAPCenter.UserIDTimer.Item2}";
                    StatusBarItemBA4.Text = _appCtrl.CAPQuote.Timer;

                    StatusBarItemAB5.Text = _appCtrl.CAPQuote.StatusStr;
                    StatusBarItemAB3.Text = $"{_appCtrl.CAPQuote.LastData.Name}|{_appCtrl.CAPQuote.LastData.UpdateTime:mm:ss.fff}|{_appCtrl.CAPQuote.LastData.Updater}";
                    StatusBarItemCB3.Text = _appCtrl.CAPOrder.Notice;
                }

                StatusBarItemCA3.Text = $"BG={_appCtrl.SignalTimeBG:ss.fff}";

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

                if (_appCtrl.OrderDetail != null)
                {
                    StatusBarItemCB3.Text = _appCtrl.OrderDetail.Notice;
                }

                if (TabControlCB.SelectedIndex == 0 && DataGridOrderDetail.ItemsSource != null)
                {
                    StatusBarItemCB1.Text = $"({DataGridOrderDetail.Columns.Count},{DataGridOrderDetail.Items.Count})";
                }
                else if (TabControlCB.SelectedIndex == 1 && DataGridOpenInterest.ItemsSource != null)
                {
                    StatusBarItemCB1.Text = $"({DataGridOpenInterest.Columns.Count},{DataGridOpenInterest.Items.Count})";
                }
                else if (TabControlCB.SelectedIndex == 2 && DataGridFuturesRights.ItemsSource != null)
                {
                    StatusBarItemCB1.Text = $"({DataGridFuturesRights.Columns.Count},{DataGridFuturesRights.Items.Count})";
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

            try
            {
                bool isHoliday = _appCtrl.Config.IsHoliday(start);
                int reConnect = 0;

                StatusBarItemCA2.Text = $"{start:HH:mm:ss}|isHoliday={isHoliday}";
                //ButtonSaveQuotesTest_Click(null, null);

                foreach (DateTime timeToClose in _appCtrl.Settings.MarketClose)
                {
                    if (start.Hour == timeToClose.Hour && start.Minute >= (timeToClose.Minute + 2) && start.Minute <= (timeToClose.Minute + 4))
                    {
                        _timer1.Stop();
                        Thread.Sleep(1 * 1000);
                        _appCtrl.Exit($"Time to exit.");

                        return;
                    }
                }

                if (_appCtrl.Config.AutoRun && _appCtrl.CAPQuote == null)
                {
                    reConnect = 1 + StatusCode.BaseTraceValue;
                }
                else if (_appCtrl.CAPQuote != null)
                {
                    if (_appCtrl.CAPQuote.Status == StatusCode.SK_WARNING_PRECHECK_RESULT_FAIL ||
                    _appCtrl.CAPQuote.Status == StatusCode.SK_WARNING_PRECHECK_RESULT_EMPTY ||
                    _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_DISCONNECT ||
                    _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK ||
                    _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL ||
                    _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR)
                    {
                        reConnect = _appCtrl.CAPQuote.Status + StatusCode.BaseErrorValue;
                    }
                    else if (_appCtrl.CAPQuote.Status > StatusCode.BaseTraceValue)
                    {
                        reConnect = _appCtrl.CAPQuote.Status;
                    }
                    else if (_appCtrl.CAPQuote.DataIndexErrorCount > 4)
                    {
                        _timer1.Stop();
                        Thread.Sleep(1 * 1000);
                        _appCtrl.Exit($"報價商品索引錯誤|_appCtrl.CAPQuote.DataIndexErrorCount({_appCtrl.CAPQuote.DataIndexErrorCount}) > 4", LogLevel.Error);

                        return;
                    }
                }

                if (reConnect == 0)
                {
                    _timer2.Start();

                    return;
                }

                _appCtrl.Log(reConnect, $"Retry to connect quote service.|reConnect={reConnect}", UniqueName, DateTime.Now - start);
                Task.Factory.StartNew(() =>
                {
                    _appCtrl.CAPQuote?.Disconnect();

                    Thread.Sleep(2 * 1000);
                    this.InvokeSync(delegate { ButtonLoginUser_Click(null, null); });

                    Thread.Sleep(2 * 1000);
                    SpinWait.SpinUntil(() => _appCtrl.CAPCenter.LoginUserResult == 0 || (_appCtrl.CAPCenter.LoginUserResult >= 600 && _appCtrl.CAPCenter.LoginUserResult <= 699), 1 * 60 * 1000);
                    this.InvokeSync(delegate
                    {
                        ButtonLoginQuote_Click(null, null);
                        ButtonReadCertification_Click(null, null);
                    });

                    Thread.Sleep(4 * 1000);
                    SpinWait.SpinUntil(() =>
                    {
                        if (_appCtrl.CAPQuote.Status == StatusCode.SK_WARNING_PRECHECK_RESULT_FAIL ||
                            _appCtrl.CAPQuote.Status == StatusCode.SK_WARNING_PRECHECK_RESULT_EMPTY ||
                            _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_DISCONNECT ||
                            _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_FAIL_WITHOUTNETWORK ||
                            _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_SOLCLIENTAPI_FAIL ||
                            _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_SOLACE_SESSION_EVENT_ERROR)
                        {
                            return LoopResult.Break;
                        }

                        return _appCtrl.CAPQuote.Status == StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY;
                    }, 1 * 60 * 1000);

                    if (_appCtrl.CAPQuote.Status != StatusCode.SK_SUBJECT_CONNECTION_STOCKS_READY && !isHoliday) //Timeout
                    {
                        _appCtrl.CAPQuote.Disconnect();
                        //this.InvokeSync(delegate { _timer2.Start(); }); //Retry to connect quote service.

                        _timer1.Stop();
                        Thread.Sleep(1 * 1000);
                        _appCtrl.Exit($"{_appCtrl.CAPQuote.Status}|{_appCtrl.CAPQuote.StatusStr}");

                        return;
                    }

                    Thread.Sleep(2 * 1000);
                    ButtonIsConnected_Click(null, null);
                    //ButtonPrintProductList_Click(null, null);
                    ButtonGetProductInfo_Click(null, null);
                    ButtonSubQuotes_Click(null, null);
                    ButtonGetOrderAccs_Click(null, null);

                    Thread.Sleep(2 * 1000);
                    SpinWait.SpinUntil(() => _appCtrl.CAPOrder.Count > 0, 4 * 1000);
                    Thread.Sleep(4 * 1000);
                    _appCtrl.CAPOrder.Unlock();
                    _appCtrl.CAPOrder.SetMaxQty();
                    _appCtrl.CAPOrder.SetMaxCount();

                    Thread.Sleep(8 * 1000);
                    _appCtrl.Trigger.RecoverSetting();
                    _appCtrl.Strategy.RecoverSetting();

                    Thread.Sleep(2 * 1000);
                    this.InvokeSync(delegate
                    {
                        CheckBoxShowDataGrid.IsChecked = _appCtrl.Settings.ShowDataGrid;
                        CheckBoxShowDataGrid_CheckedOrNot(null, null);

                        CheckBoxLiveMode.IsChecked = _appCtrl.Settings.LiveMode;
                        CheckBoxLiveMode_CheckedOrNot(null, null);

                        _timer2.Start();
                    });
                });
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
                _timer2.Start();
            }
        }

        private void CheckBoxShowDataGrid_CheckedOrNot(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Settings.ShowDataGrid = CheckBoxShowDataGrid.IsChecked.Value;
                _appCtrl.LogTrace(start, $"ShowDataGrid={_appCtrl.Settings.ShowDataGrid}", UniqueName);

                if (_appCtrl.Settings.ShowDataGrid)
                {
                    DataGridAppLog.Visibility = Visibility.Visible;
                    DataGridQuoteSubscribed.Visibility = Visibility.Visible;
                    DataGridOrderDetail.Visibility = Visibility.Visible;
                    DataGridFuturesRights.Visibility = Visibility.Visible;
                }
                else
                {
                    DataGridAppLog.Visibility = Visibility.Collapsed;
                    DataGridQuoteSubscribed.Visibility = Visibility.Collapsed;
                    DataGridOrderDetail.Visibility = Visibility.Collapsed;
                    DataGridFuturesRights.Visibility = Visibility.Collapsed;
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

        private void CheckBoxSendRealOrder_CheckedOrNot(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Settings.SendRealOrder = CheckBoxSendRealOrder.IsChecked.Value;
                _appCtrl.LogTrace(start, $"SendRealOrder={_appCtrl.Settings.SendRealOrder}", UniqueName);
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

        private void CheckBoxLiveMode_CheckedOrNot(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Settings.LiveMode = CheckBoxLiveMode.IsChecked.Value;
                _appCtrl.LogTrace(start, $"LiveMode={_appCtrl.Settings.LiveMode}", UniqueName);

                if (_appCtrl.Settings.LiveMode)
                {
                    TextBoxUserID.Visibility = Visibility.Collapsed;
                    ComboBoxOrderAccs.Visibility = Visibility.Collapsed;

                    DataGridAPIReply.Columns[APIReplyData.PropertyMap[nameof(APIReplyData.UserID)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    DataGridAPIReply.Columns[APIReplyData.PropertyMap[nameof(APIReplyData.Message)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;

                    //DataGridAppLog.Columns[AppLogInDataGrid.PropertyMap[nameof(AppLogInDataGrid.Message)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;

                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.Symbol1)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.ColumnName)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.ColumnProperty)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.ColumnValue)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.TargetValue)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.Symbol2)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;

                    DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.FullAccount)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.Symbol)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.BSDes)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.AccountsWinLossClose)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;

                    DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.FullAccount)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.Symbol)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    //DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.BSDes)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;

                    DataGridOpenInterest.Columns[OpenInterestData.PropertyMap[nameof(OpenInterestData.Account)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;

                    DataGridFuturesRights.Columns[FuturesRightsData.PropertyMap[nameof(FuturesRightsData.UserID)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                    DataGridFuturesRights.Columns[FuturesRightsData.PropertyMap[nameof(FuturesRightsData.Account)].Item1.WPFDisplayIndex].Visibility = Visibility.Hidden;
                }
                else
                {
                    TextBoxUserID.Visibility = Visibility.Visible;
                    ComboBoxOrderAccs.Visibility = Visibility.Visible;

                    DataGridAPIReply.Columns[APIReplyData.PropertyMap[nameof(APIReplyData.UserID)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    DataGridAPIReply.Columns[APIReplyData.PropertyMap[nameof(APIReplyData.Message)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;

                    //DataGridAppLog.Columns[AppLogInDataGrid.PropertyMap[nameof(AppLogInDataGrid.Message)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;

                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.Symbol1)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.ColumnName)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.ColumnProperty)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.ColumnValue)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.TargetValue)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridTriggerRule.Columns[TriggerData.PropertyMap[nameof(TriggerData.Symbol2)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;

                    DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.FullAccount)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.Symbol)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.BSDes)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    DataGridStrategyRule.Columns[StrategyData.PropertyMap[nameof(StrategyData.AccountsWinLossClose)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;

                    DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.FullAccount)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.Symbol)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    //DataGridOrderDetail.Columns[StrategyData.PropertyMap[nameof(StrategyData.BSDes)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;

                    DataGridOpenInterest.Columns[OpenInterestData.PropertyMap[nameof(OpenInterestData.Account)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;

                    DataGridFuturesRights.Columns[FuturesRightsData.PropertyMap[nameof(FuturesRightsData.UserID)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
                    DataGridFuturesRights.Columns[FuturesRightsData.PropertyMap[nameof(FuturesRightsData.Account)].Item1.WPFDisplayIndex].Visibility = Visibility.Visible;
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

        private void CheckBoxStartFromOpenInterest_CheckedOrNot(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.Settings.StartFromOpenInterest = CheckBoxStartFromOpenInterest.IsChecked.Value;
                _appCtrl.LogTrace(start, $"StartFromOpenInterest={_appCtrl.Settings.StartFromOpenInterest}", UniqueName);
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

        private void ButtonLoginUser_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                if (string.IsNullOrWhiteSpace(TextBoxUserID.Text) && string.IsNullOrWhiteSpace(DWPBox.Password))
                {
                    FileInfo dwpFile = new FileInfo($"{ProcessName}.dwp.config");

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
                _appCtrl.CAPCenter.LoginUser(TextBoxUserID.Text, DWPBox.Password);

                string version = _appCtrl.CAPCenter.GetSKAPIVersion();

                _appCtrl.LogTrace(start, $"SKAPIVersionAndBit={version}", UniqueName);
                StatusBarItemBA2.Text = $"SKAPIVersionAndBit={version}";
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
                _appCtrl.CAPQuote.LoginAsync(DWPBox.Password);
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
                (LogLevel, string) apiMsg = _appCtrl.CAPQuote.IsConnected();

                this.InvokeAsync(delegate { StatusBarItemAB4.Text = apiMsg.Item1 == LogLevel.Trace ? apiMsg.Item2 : $"{apiMsg.Item1}|{apiMsg.Item2}"; });
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
                _appCtrl.CAPQuote.Disconnect();
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
                _appCtrl.CAPQuote.PrintProductList();
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

            this.InvokeSync(delegate
            {
                try
                {
                    _appCtrl.CAPQuote.GetProductInfo();

                    string changeFutures = _appCtrl.Config.DateToChangeFutures.Date == DateTime.Today ? $"轉倉日" : $"{_appCtrl.Config.DateToChangeFutures.DayOfWeek}";

                    if (DateTime.Today.AddDays(6) >= _appCtrl.Config.DateToChangeFutures.Date)
                    {
                        changeFutures = $"接近轉倉日";
                    }

                    StatusBarItemCA4.Text = $"{_appCtrl.CAPQuote.MarketStartTime:MM/dd HH:mm} ~ {_appCtrl.CAPQuote.MarketCloseTime:MM/dd HH:mm}|({changeFutures}) {_appCtrl.Config.DateToChangeFutures:MM/dd}";
                    StatusBarItemAB2.Text = $"Sub={_appCtrl.Config.QuoteSubscribed.Count}|Live={_appCtrl.Settings.QuoteLive.Count}|QuoteFile={_appCtrl.CAPQuote.FileNameBase}";
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
        }

        private void ButtonSubQuotes_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.CAPQuote.SubscribeAsync();

                this.InvokeAsync(delegate
                {
                    ComboBoxTriggerProduct1.SetAndGetItemsSource(_appCtrl.CAPQuote.DataCollection.Select(x => $"{x.Symbol},{x.Name},{x.MarketGroupEnum}"));
                    ComboBoxTriggerProduct1.Text = "2330";
                    ComboBoxTriggerProduct2.SetAndGetItemsSource(_appCtrl.CAPQuote.DataCollection.Select(x => $"{x.Symbol},{x.Name},{x.MarketGroupEnum}"));
                    ComboBoxTriggerProduct2.Text = "2330";
                    ComboBoxOrderProduct.SetAndGetItemsSource(_appCtrl.CAPQuote.DataCollection.Select(x => $"{x.Symbol},{x.Name},{x.MarketGroupEnum}"));
                    ComboBoxOrderProduct.Text = "2330";
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

        private void ButtonRecoverQuotes_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.CAPQuote.RecoverDataAsync(TextBoxRecoverQuotes.Text);
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
                _appCtrl.CAPQuote.RequestKLine();
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
            if (_appCtrl.CAPCenter == null)
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

                _appCtrl.CAPQuote.SaveDataAsync(folder);
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
                _appCtrl.CAPOrder.ReadCertification();
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

            this.InvokeSync(delegate
            {
                try
                {
                    _appCtrl.CAPOrder.GetAccounts();
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
        }

        private void ButtonUnlockOrder_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.CAPOrder.Unlock();
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

                _appCtrl.CAPOrder.GetOpenInterest(acc.FullAccount);
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

        private void ButtonGetFuturesRights_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                OrderAccData acc = (OrderAccData)ComboBoxOrderAccs.SelectedItem;

                if (acc.MarketType != Market.EType.Futures)
                {
                    return;
                }

                _appCtrl.CAPOrder.GetFuturesRights(acc.FullAccount);
            }
            catch (Exception ex)
            {
                _appCtrl.LogException(start, ex, ex.StackTrace);
            }
        }

        private void ButtonSetOrderMaxQty_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = _appCtrl.StartTrace();

            try
            {
                _appCtrl.CAPOrder.SetMaxQty();
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
                _appCtrl.CAPOrder.SetMaxCount();
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

                TriggerData data = new TriggerData((TradeColumnTrigger)ComboBoxTriggerColumn.SelectedItem)
                {
                    PrimaryKey = TextBoxTriggerPrimaryKey.Text,
                    Symbol1 = ComboBoxTriggerProduct1.Text.Split(',')[0],
                    Symbol2 = ComboBoxTriggerProduct2.Text.Split(',')[0],
                    Rule = TextBoxTriggerRuleValue.Text,
                    Cancel = TextBoxTriggerCancel.Text,
                    Start = TextBoxTriggerStart.Text,
                    StrategyOpenOR = TextBoxTriggerStrategyOpenOR.Text,
                    StrategyOpenAND = TextBoxTriggerStrategyOpenAND.Text,
                    StrategyCloseOR = TextBoxTriggerStrategyCloseOR.Text,
                    StrategyCloseAND = TextBoxTriggerStrategyCloseAND.Text,
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                _appCtrl.Trigger.AddRule(data, TextBoxTriggerTimeDuration.Text);

                string primaryKey = TextBoxTriggerPrimaryKey.Text.Replace(" ", string.Empty);

                if (!decimal.TryParse(primaryKey, out decimal pk))
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                    if (_appCtrl.Trigger[primaryKey] == null)
                    {
                        return;
                    }
                    else if (pk < _appCtrl.Trigger.Count)
                    {
                        pk = _appCtrl.Trigger.Count;
                    }

                    ++pk;

                    if (_appCtrl.Trigger[$"{pk}"] == null)
                    {
                        this.InvokeAsync(delegate { TextBoxTriggerPrimaryKey.Text = $"{pk}"; });
                    }
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
                TriggerData data = ((DataGridCell)sender).GetItem<TriggerData>();
                _appCtrl.LogTrace(start, data.ToLog(), UniqueName);

                ComboBoxTriggerProduct1.Text = data.Symbol1;
                ComboBoxTriggerProduct2.Text = data.Symbol2;

                ComboBoxTriggerColumn.SelectedIndex = -1;
                for (int i = 0; i < ComboBoxTriggerColumn.Items.Count; ++i)
                {
                    ComboBoxTriggerColumn.SelectedIndex = i;
                    if (ComboBoxTriggerColumn.SelectedItem is TradeColumnTrigger column && column.Property == data.Column.Property)
                    {
                        if (column.Property.Name == data.ColumnProperty)
                        {
                            break;
                        }
                        else
                        {
                            _appCtrl.LogError(start, $"Trigger|觸價關聯報價欄位錯誤|column.Property.Name{column.Property.Name} != data.ColumnProperty{data.ColumnProperty}|{data.ToLog()}", UniqueName);
                        }
                    }
                    if (i == ComboBoxTriggerColumn.Items.Count - 1)
                    {
                        ComboBoxTriggerColumn.SelectedIndex = -1;
                        _appCtrl.LogError(start, $"Trigger|觸價關聯報價欄位錯誤|{data.ToLog()}", UniqueName);
                    }
                }

                if (string.IsNullOrWhiteSpace(ComboBoxTriggerProduct2.Text))
                {
                    TextBoxTriggerRuleValue.Text = $"{data.Rule}{data.TargetValue:0.00####}";
                }
                else
                {
                    TextBoxTriggerRuleValue.Text = $"{data.Rule}P2{data.Symbol2Setting}";
                }

                TextBoxTriggerPrimaryKey.Text = data.PrimaryKey;
                TextBoxTriggerCancel.Text = data.Cancel;
                TextBoxTriggerStart.Text = data.Start;
                TextBoxTriggerStrategyOpenOR.Text = data.StrategyOpenOR;
                TextBoxTriggerStrategyOpenAND.Text = data.StrategyOpenAND;
                TextBoxTriggerStrategyCloseOR.Text = data.StrategyCloseOR;
                TextBoxTriggerStrategyCloseAND.Text = data.StrategyCloseAND;

                TextBoxTriggerTimeDuration.Text = string.Empty;
                if (data.StartTime.HasValue)
                {
                    TextBoxTriggerTimeDuration.Text = $"{data.StartTime.Value:HHmmss}";
                }
                if (data.EndTime.HasValue)
                {
                    TextBoxTriggerTimeDuration.Text = $"{TextBoxTriggerTimeDuration.Text}~{data.EndTime.Value:HHmmss}";
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
                StrategyData data = ((DataGridCell)sender).GetItem<StrategyData>();
                _appCtrl.LogTrace(start, data.ToLog(), UniqueName);

                TextBoxStrategyPrimaryKey.Text = data.PrimaryKey;
                TextBoxStrategyStopLoss.Text = data.StopLossBefore;
                TextBoxStrategyStopWin.Text = data.StopWinBefore;
                TextBoxStrategyMoveStopWin.Text = data.MoveStopWinBefore;
                TextBoxOpenTriggerAfterStopLoss.Text = data.OpenTriggerAfterStopLoss;
                TextBoxOpenStrategyAfterStopLoss.Text = data.OpenStrategyAfterStopLoss;
                TextBoxOpenTriggerAfterStopWin.Text = data.OpenTriggerAfterStopWin;
                TextBoxOpenStrategyAfterStopWin.Text = data.OpenStrategyAfterStopWin;
                TextBoxCloseTriggerAfterStopWin.Text = data.CloseTriggerAfterStopWin;
                TextBoxCloseStrategyAfterStopWin.Text = data.CloseStrategyAfterStopWin;
                TextBoxStrategyWinClose.Text = $"{data.WinCloseQty},{data.WinCloseSeconds}secs";
                TextBoxStrategyLossClose.Text = $"{data.LossCloseQty},{data.LossCloseSeconds}secs";
                TextBoxAccountsWinLossClose.Text = data.AccountsWinLossClose;

                ComboBoxOrderAccs.SelectedIndex = -1;
                for (int i = 0; i < ComboBoxOrderAccs.Items.Count; ++i)
                {
                    ComboBoxOrderAccs.SelectedIndex = i;
                    if (ComboBoxOrderAccs.SelectedItem is OrderAccData orderAcc && orderAcc.FullAccount == data.FullAccount)
                    {
                        break;
                    }
                    if (i == ComboBoxOrderAccs.Items.Count - 1)
                    {
                        ComboBoxOrderAccs.SelectedIndex = -1;
                        _appCtrl.LogError(start, $"Strategy|策略關聯帳號錯誤|{data.ToLog()}", UniqueName);
                    }
                }

                ComboBoxOrderProduct.Text = data.Symbol;
                ComboBoxOrderBuySell.SelectedIndex = (int)data.BSEnum;
                ComboBoxOrderTradeType.SelectedIndex = (int)data.TradeTypeEnum;
                ComboBoxOrderDayTrade.SelectedIndex = (int)data.DayTradeEnum;
                ComboBoxOrderPositionKind.SelectedIndex = (int)data.PositionEnum;
                TextBoxOrderPrice.Text = data.OrderPriceBefore;
                TextBoxOrderQty.Text = $"{data.OrderQty}";
                CheckBoxStrategySendReal.IsChecked = data.SendRealOrder;
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

        private void ButtonStopStrategy_Click(object sender, RoutedEventArgs e)
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

                _appCtrl.Strategy.Close(TextBoxStrategyPrimaryKey.Text);
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

                StrategyData data = new StrategyData()
                {
                    PrimaryKey = TextBoxStrategyPrimaryKey.Text,
                    MarketType = acc.MarketType,
                    Branch = acc.Branch,
                    Account = acc.Account,
                    Symbol = ComboBoxOrderProduct.Text.Split(',')[0],
                    BSEnum = (OrderBS.Enum)ComboBoxOrderBuySell.SelectedIndex,
                    TradeTypeEnum = (OrderTradeType.Enum)ComboBoxOrderTradeType.SelectedIndex,
                    DayTradeEnum = (OrderDayTrade.Enum)ComboBoxOrderDayTrade.SelectedIndex,
                    PositionEnum = (OrderPosition.Enum)ComboBoxOrderPositionKind.SelectedIndex,
                    OrderPriceBefore = TextBoxOrderPrice.Text,
                    OrderQty = int.Parse(TextBoxOrderQty.Text),
                    StopLossBefore = TextBoxStrategyStopLoss.Text,
                    StopWinBefore = TextBoxStrategyStopWin.Text,
                    MoveStopWinBefore = TextBoxStrategyMoveStopWin.Text,
                    OpenTriggerAfterStopLoss = TextBoxOpenTriggerAfterStopLoss.Text,
                    OpenStrategyAfterStopLoss = TextBoxOpenStrategyAfterStopLoss.Text,
                    OpenTriggerAfterStopWin = TextBoxOpenTriggerAfterStopWin.Text,
                    OpenStrategyAfterStopWin = TextBoxOpenStrategyAfterStopWin.Text,
                    CloseTriggerAfterStopWin = TextBoxCloseTriggerAfterStopWin.Text,
                    CloseStrategyAfterStopWin = TextBoxCloseStrategyAfterStopWin.Text,
                    AccountsWinLossClose = TextBoxAccountsWinLossClose.Text,
                    SendRealOrder = CheckBoxStrategySendReal.IsChecked.Value,
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                if (!string.IsNullOrWhiteSpace(TextBoxStrategyWinClose.Text))
                {
                    string[] winClose = TextBoxStrategyWinClose.Text.Split(',');

                    foreach (string cell in winClose)
                    {
                        string lower = cell.ToLower();

                        if (lower.Contains("secs") || lower.Contains("seconds") || lower.Contains("sec") || lower.Contains("second"))
                        {
                            string secs = lower.Replace("secs", string.Empty).Replace("seconds", string.Empty).Replace("sec", string.Empty).Replace("second", string.Empty);
                            data.WinCloseSeconds = int.Parse(secs);

                            continue;
                        }

                        data.WinCloseQty = int.Parse(cell);
                    }
                }

                if (!string.IsNullOrWhiteSpace(TextBoxStrategyLossClose.Text))
                {
                    string[] lossClose = TextBoxStrategyLossClose.Text.Split(',');

                    foreach (string cell in lossClose)
                    {
                        string lower = cell.ToLower();

                        if (lower.Contains("secs") || lower.Contains("seconds") || lower.Contains("sec") || lower.Contains("second"))
                        {
                            string secs = lower.Replace("secs", string.Empty).Replace("seconds", string.Empty).Replace("sec", string.Empty).Replace("second", string.Empty);
                            data.LossCloseSeconds = int.Parse(secs);

                            continue;
                        }

                        data.LossCloseQty = int.Parse(cell);
                    }
                }

                _appCtrl.Strategy.AddRule(data);

                if (!decimal.TryParse(TextBoxStrategyPrimaryKey.Text.Replace(" ", string.Empty), out decimal pk))
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                    if (_appCtrl.Strategy[data.PrimaryKey] == null)
                    {
                        return;
                    }
                    else if (pk < _appCtrl.Strategy.Count)
                    {
                        pk = _appCtrl.Strategy.Count;
                    }

                    ++pk;

                    if (_appCtrl.Strategy[$"{pk}"] == null)
                    {
                        this.InvokeAsync(delegate { TextBoxStrategyPrimaryKey.Text = $"{pk}"; });
                    }
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

        private void ButtonSendTWOrder_Click(object sender, RoutedEventArgs e)
        {
            const string methodName = nameof(ButtonSendTWOrder_Click);

            DateTime start = _appCtrl.StartTrace();

            try
            {
                OrderAccData acc = (OrderAccData)ComboBoxOrderAccs.SelectedItem;

                StrategyData order = new StrategyData()
                {
                    MarketType = acc.MarketType,
                    Branch = acc.Branch,
                    Account = acc.Account,
                    Symbol = ComboBoxOrderProduct.Text.Split(',')[0],
                    BSEnum = (OrderBS.Enum)ComboBoxOrderBuySell.SelectedIndex,
                    TradeTypeEnum = (OrderTradeType.Enum)ComboBoxOrderTradeType.SelectedIndex,
                    DayTradeEnum = (OrderDayTrade.Enum)ComboBoxOrderDayTrade.SelectedIndex,
                    PositionEnum = (OrderPosition.Enum)ComboBoxOrderPositionKind.SelectedIndex,
                    OrderPriceBefore = TextBoxOrderPrice.Text,
                    OrderQty = int.Parse(TextBoxOrderQty.Text),
                    SendRealOrder = CheckBoxStrategySendReal.IsChecked.Value,
                    Updater = methodName,
                    UpdateTime = DateTime.Now,
                };

                _appCtrl.CAPOrder.Send(order);
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

        private void ButtonStartStartegyNow_Click(object sender, RoutedEventArgs e)
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

                if (DataGridStrategyRule.SelectedCells.Count > 0 && DataGridStrategyRule.SelectedCells[0].Item is StrategyData data)
                {
                    if (data.PrimaryKey == TextBoxStrategyPrimaryKey.Text.Trim() && data.StatusEnum == StrategyStatus.Enum.Waiting)
                    {
                        _appCtrl.Strategy.StartNow(data.PrimaryKey);

                        return;
                    }
                }

                ButtonSaveStrategyRule_Click(null, null);

                string primaryKey = TextBoxStrategyPrimaryKey.Text.Replace(" ", string.Empty);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                        _appCtrl.Strategy.StartNow(primaryKey);
                    }
                    catch (Exception ex)
                    {
                        _appCtrl.LogException(start, ex, ex.StackTrace);
                    }
                });

                if (!decimal.TryParse(TextBoxStrategyPrimaryKey.Text.Replace(" ", string.Empty), out decimal pk))
                {
                    return;
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(_appCtrl.Settings.TimerIntervalBackground * 3);

                    if (_appCtrl.Strategy[primaryKey] == null)
                    {
                        return;
                    }
                    else if (pk < _appCtrl.Strategy.Count)
                    {
                        pk = _appCtrl.Strategy.Count;
                    }

                    ++pk;

                    if (_appCtrl.Strategy[$"{pk}"] == null)
                    {
                        this.InvokeAsync(delegate { TextBoxStrategyPrimaryKey.Text = $"{pk}"; });
                    }
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
                _appCtrl.CAPOrder.CancelBySeqNo((OrderAccData)ComboBoxOrderAccs.SelectedItem, ComboBoxOrderSeqNo.Text);
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
