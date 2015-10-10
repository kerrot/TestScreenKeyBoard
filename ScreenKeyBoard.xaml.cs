using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Globalization;
using System.Runtime.InteropServices;

namespace ScreenKeyBoard
{
    /// <summary>
    /// ScreenKeyBoard.xaml 的互動邏輯
    /// </summary>
    /// 

    public class ButtonValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2 && values[0] is bool && values[1] is Button)
            {
                Button btn = values[1] as Button;
                if (btn.Tag != null)
                {
                    string str = btn.Tag.ToString();
                    return ((bool)values[0]) ? str.ToUpper() : str.ToLower();
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public partial class ScreenKeyBoard : Window
    {
        private static Control m_ctrl;
        private static string m_ctrlText
        {
            get
            {
                if (m_ctrl is TextBox)
                {
                    return ((TextBox)m_ctrl).Text;
                }
                else if (m_ctrl is PasswordBox)
                {
                    return ((PasswordBox)m_ctrl).Password;
                }
                else return null;
            }
            set
            {
                if (m_ctrl is TextBox)
                {
                    ((TextBox)m_ctrl).Text = value;
                }
                else if (m_ctrl is PasswordBox)
                {
                    ((PasswordBox)m_ctrl).Password = value;
                }
            }
        }

        private static Brush m_oriBackground = Brushes.White;

        public static readonly DependencyProperty AttathProperty = DependencyProperty.RegisterAttached("Attath", typeof(bool), typeof(ScreenKeyBoard), new UIPropertyMetadata(default(bool), AttathPropertyChanged));

        public static RoutedCommand KeyInCmd = new RoutedCommand();
        public static RoutedCommand CloseCmd = new RoutedCommand();
        public static RoutedCommand OKCmd = new RoutedCommand();
        public static RoutedCommand BackSpaceCmd = new RoutedCommand();
        public static RoutedCommand ClearCmd = new RoutedCommand();

        private ScreenKeyBoard()
        {
            InitializeComponent();
            Closing += (sender, e) =>
                {
                    e.Cancel = true;
                    Instance.Visibility = Visibility.Hidden;
                };
        }

        public static ScreenKeyBoard Instance
        {
            get { return Singleton<ScreenKeyBoard>.Instance; }
        }

        public static void SetAttath(UIElement element, Boolean value)
        {
            element.SetValue(AttathProperty, value);
        }
        public static Boolean GetAttath(UIElement element)
        {
            return (Boolean)element.GetValue(AttathProperty);
        }

        static void AttathPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement host = sender as FrameworkElement;
            if (host != null)
            {
                host.GotFocus += new RoutedEventHandler(OnGotFocus);
                host.LostFocus += new RoutedEventHandler(OnLostFocus);
            }
        }

        static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl != null)
            {
                m_oriBackground = ctrl.Background;

                m_ctrl = ctrl;
                m_ctrl.Background = Brushes.LavenderBlush;
            }

            FrameworkElement ct = m_ctrl;
            do
            {
                Window wnd = ct as Window;
                if (wnd != null)
                {
                    wnd.LocationChanged += (obj, arg) => 
                        {
                            Control_LayoutUpdated();
                        };
                    wnd.Activated += (obj, arg) => Instance.Topmost = true;
                    wnd.Deactivated += (obj, arg) => Instance.Topmost = false;

                    Instance.Parent = wnd;
                    break;
                }
                ct = ct.Parent as FrameworkElement;
            }
            while (ct != null);

            m_ctrl.LayoutUpdated += (obj, arg) =>
            {
                Control_LayoutUpdated();
            };
        }

        static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl != null)
            {
                ctrl.Background = m_oriBackground;
            }
            m_ctrl = null;
            Instance.Close();
        }

        static void Control_LayoutUpdated()
        {
            if (m_ctrl != null)
            {
                Point virtualpoint = new Point(0, m_ctrl.ActualHeight + 3);
                Point Actualpoint = m_ctrl.PointToScreen(virtualpoint);

                if (Instance.Width + Actualpoint.X > SystemParameters.VirtualScreenWidth)
                {
                    double difference = Instance.Width + Actualpoint.X - SystemParameters.VirtualScreenWidth;
                    Instance.Left = Actualpoint.X - difference;
                }
                else if (!(Actualpoint.X > 1))
                {
                    Instance.Left = 1;
                }
                else
                    Instance.Left = Actualpoint.X;

                Instance.Top = Actualpoint.Y;
                Instance.Show();
            }
        }

        private void KeyIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Button btn = e.Parameter as Button;

            if (m_ctrl != null && btn != null)
            {
                m_ctrlText += btn.Content.ToString();
            }
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_ctrl = null;
            Instance.Close();
        }

        private void OK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_ctrl != null)
            {
                m_ctrl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void BackSpace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_ctrl != null && m_ctrlText.Length > 0)
            {
                m_ctrlText = m_ctrlText.Substring(0, m_ctrlText.Length - 1);
            }
        }

        private void Clear_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (m_ctrl != null)
            {
                m_ctrlText = "";
            }
        }
    }
}
