using System;
using System.Windows.Navigation;
using System.Windows;
using System.Windows.Media;

namespace Resonance
{
    /// <summary>
    /// 主页面
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {

        public static MainWindow _This;
        public event Action CloseWindowEvent;
        public MainWindow()
        {
            _This = this;
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
    }//end class
}
