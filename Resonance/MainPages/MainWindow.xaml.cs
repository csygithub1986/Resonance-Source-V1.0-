using System;
using System.Windows.Navigation;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;

namespace Resonance
{
    /// <summary>
    /// 主页面
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {

        public static MainWindow Instance;
        public event Action CloseWindowEvent;
        public Button SettingBtn;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Navigated += new NavigatedEventHandler(MainWindow_Navigated);

            //初始化Params
            Params.Color1 = (Color)ColorConverter.ConvertFromString("#DFA705");
            Params.Color2 = (Color)ColorConverter.ConvertFromString("#058205");
            Params.Color3 = (Color)ColorConverter.ConvertFromString("#FF0000");
            Params.Color4 = (Color)ColorConverter.ConvertFromString("#E46C0A");
            Params.Brush1 = new SolidColorBrush(Params.Color1);
            Params.Brush2 = new SolidColorBrush(Params.Color2);
            Params.Brush3 = new SolidColorBrush(Params.Color3);
            Params.Brush4 = new SolidColorBrush(Params.Color4);
            Params.Colors = new Color[] { Params.Color1, Params.Color2, Params.Color3, Params.Color4 };
            Params.Brushes = new SolidColorBrush[] { Params.Brush1, Params.Brush2, Params.Brush3, Params.Brush4 };

            this.Loaded += delegate
            {
                InitializeEvent();
            };

        }

        void MainWindow_Navigated(object sender, NavigationEventArgs e)
        {
            while (CanGoBack)
            {
                RemoveBackEntry();
            }
        }

        //关闭主窗口时释放系统所有资源
        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void NavigationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确认退出程序吗?", "退出程序", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            if (CloseWindowEvent != null)
            {
                CloseWindowEvent();
            }
        }


        #region BaseWindow
        private void InitializeEvent()
        {
            ControlTemplate baseWindowTemplate = Template;// (ControlTemplate)Application.Current.Resources["BaseWindowControlTemplate"];
            Image imgLogo = (Image)baseWindowTemplate.FindName("imgLogo", this);
            imgLogo.Source = Icon;// new System.Windows.Media.Imaging.BitmapImage(new System.Uri("/images/logo32.png", UriKind.RelativeOrAbsolute));

            TextBlock txtTitle = (TextBlock)baseWindowTemplate.FindName("txtTitle", this);
            txtTitle.Text = Title;

            SettingBtn = (Button)baseWindowTemplate.FindName("btnSetting", this);
            SettingBtn.Click += SettingBtn_Click;

            Button minBtn = (Button)baseWindowTemplate.FindName("btnMin", this);
            minBtn.Click += delegate
            {
                this.WindowState = WindowState.Minimized;
            };

            Button maxBtn = (Button)baseWindowTemplate.FindName("btnMax", this);
            Viewbox maxView = Application.Current.Resources["MaxButtonTemplate"] as Viewbox;
            Viewbox restoreView = Application.Current.Resources["RestoreButtonTemplate"] as Viewbox;

            string xaml = System.Windows.Markup.XamlWriter.Save(maxView);
            maxView = System.Windows.Markup.XamlReader.Parse(xaml) as Viewbox;
            xaml = System.Windows.Markup.XamlWriter.Save(restoreView);
            restoreView = System.Windows.Markup.XamlReader.Parse(xaml) as Viewbox;

            if (WindowState == WindowState.Maximized)
            {
                maxBtn.Content = restoreView;
            }
            else
            {
                maxBtn.Content = maxView;
            }
            maxBtn.Click += delegate
            {
                if (WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    maxBtn.Content = maxView;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    maxBtn.Content = restoreView;
                }
            };

            Button closeBtn = (Button)baseWindowTemplate.FindName("btnClose", this);
            closeBtn.Click += delegate
            {
                this.Close();
            };

            Border borderTitle = (Border)baseWindowTemplate.FindName("borderTitle", this);
            borderTitle.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };

            borderTitle.MouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
            {
                if (e.ClickCount >= 2)
                {
                    if (maxBtn.Visibility == Visibility.Visible)
                        maxBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            };
        }

        //系统参数设置
        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            SystemSetWindow setWin = new SystemSetWindow();
            setWin.Owner = App.Current.MainWindow;
            setWin.ShowDialog();
        }

        private void InitializeStyle()
        {
            //this.Style = (Style)Application.Current.Resources["BaseWindowStyle"];
            //this.MaxHeight = SystemParameters.WorkArea.Height;
        }

        public virtual void SetWindowTitle(string WindowTitle)
        {
            ControlTemplate baseWindowTemplate = (ControlTemplate)Application.Current.Resources["BaseWindowControlTemplate"];
            TextBlock txtTitle = (TextBlock)baseWindowTemplate.FindName("txtTitle", this);
            if (txtTitle != null)
                txtTitle.Text = WindowTitle;
        }
        #endregion


    }//end class
}
