using GNAy.Capital.Trade.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        public static MainWindow Current { get; private set; }
        public static MainWindowController AppCtrl => Current.AppControl;
        public static CapitalController CapitalCtrl => Current.CapitalControl;

        public readonly DateTime StartTime;

        public readonly MainWindowController AppControl;
        public CapitalController CapitalControl { get; private set; }

        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            StartTime = DateTime.Now;
            StatusBarItemAA1.Text = StartTime.ToString("MM/dd HH:mm");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

            Current = this;

            AppControl = new MainWindowController();
            CapitalControl = null;

            //https://www.796t.com/post/MWV3bG0=.html
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            Title = $"{version.Comments} ({version.FileVersion})";
            if (version.FileMajorPart <= 0 || version.FilePrivatePart % 2 == 1)
            {
                Title = $"{Title}(BETA)";
            }
            Title = $"{Title} ({version.ProductName})({version.LegalCopyright}) ({AppCtrl.Settings.Description})";
            if (Debugger.IsAttached)
            {
                Title = $"{Title}(附加偵錯)";
            }

            _timer = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromMilliseconds(AppCtrl.Settings.TimerInterval),
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            AppCtrl.LogTrace(Title);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        /// <summary>
        /// https://docs.microsoft.com/zh-tw/windows/win32/debug/system-error-codes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //

                AppCtrl.Exit();
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);

                Thread.Sleep(1 * 1000);
                Environment.Exit(16000 + 1);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_GotMouseCapture(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //

                if (AppCtrl.Settings.AutoRun)
                {
                    //TODO: AutoRun
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            //finally
            //{
            //    AppCtrl.LogTrace("End");
            //}
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_LostMouseCapture(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            //finally
            //{
            //    AppCtrl.LogTrace("End");
            //}
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                //AppCtrl.LogTrace("End");
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            //finally
            //{
            //    AppCtrl.LogTrace("End");
            //}
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Window_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                //
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                StatusBarItemAA2.Text = (DateTime.Now - StartTime).ToString(@"hh\:mm\:ss");

                //if (Application.Current.MainWindow.IsMouseOver)
                if (IsMouseOver)
                {
                    //https://stackoverflow.com/questions/29822020/how-to-get-mouse-position-on-screen-in-wpf
                    //Point pt = Mouse.GetPosition(Application.Current.MainWindow);
                    Point pt = Mouse.GetPosition(this);
                    StatusBarItemAA3.Text = $"({pt.X},{pt.Y})";
                }

                if (DataGridAppLog.ItemsSource != null)
                {
                    StatusBarItemBA1.Text = $"({DataGridAppLog.Columns.Count},{DataGridAppLog.Items.Count})";
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            AppCtrl.LogTrace("Start");

            try
            {
                if (CapitalCtrl == null)
                {
                    CapitalControl = new CapitalController();
                    CapitalCtrl.Login(TextBoxAccount.Text, DWPBox.Password);
                }
            }
            catch (Exception ex)
            {
                AppCtrl.LogException(ex, ex.StackTrace);
            }
            finally
            {
                AppCtrl.LogTrace("End");
            }
        }
    }
}
