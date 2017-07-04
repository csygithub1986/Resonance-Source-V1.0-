using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.IO;
using System.Windows.Threading;

namespace Resonance
{
    /// <summary>
    /// 连接页面。若连接成功，跳转至标定页面，若连接失败，停留在此页面
    /// </summary>
    public partial class WlanPage : Page
    {
        public WlanPage()
        {
            InitializeComponent();
            cbIP.Items.Add("192.168.0.105");//默认采用此IP
            cbIP.Items.Add("127.0.0.1");//本机测试IP
            cbIP.SelectedIndex = 0;
            cbIP.SelectionChanged+=new SelectionChangedEventHandler(cbIP_SelectionChanged);
        }

        /// <summary>
        /// IP切换
        /// </summary>
        void cbIP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Params.IP = cbIP.SelectedItem as string;
        }

        /// <summary>
        /// 启动页面同时，连接网络
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    TcpClientWithTimeout tcpWithTout = new TcpClientWithTimeout(IPAddress.Parse(Params.IP), Params.Port, 3000);
                    MeasureState.Client = tcpWithTout.Connect();
                    MeasureState.TcpBinaryReader = new BinaryReader(MeasureState.Client.GetStream());
                    MeasureState.TcpBinaryWriter = new BinaryWriter(MeasureState.Client.GetStream());
                    //网络连通之后 
                    new Thread(MeasureState.ListenThread).Start();
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                    {
                        //跳转至标定页面
                        CaliPage cp = new CaliPage();
                        NavigationService.Navigate(cp);
                    }));
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                    {
                        labelConnect.Visibility = Visibility.Hidden;
                    }));
                    return;
                }
            }).Start();
        }

        /// <summary>
        /// 手动连接按钮，当自动连接失败后，出现该按钮
        /// </summary>
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            labelConnect.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                try
                {
                    TcpClientWithTimeout tcpWithTout = new TcpClientWithTimeout(IPAddress.Parse(Params.IP), Params.Port, 3000);
                    MeasureState.Client = tcpWithTout.Connect();
                    MeasureState.TcpBinaryReader = new BinaryReader(MeasureState.Client.GetStream());
                    MeasureState.TcpBinaryWriter = new BinaryWriter(MeasureState.Client.GetStream());
                    //网络连通之后 
                    new Thread(MeasureState.ListenThread).Start();
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                    {
                        CaliPage cp = new CaliPage();
                        NavigationService.Navigate(cp);
                    }));
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                    {
                        labelConnect.Visibility = Visibility.Hidden;
                    }));
                    return;
                }
            }).Start();
        }
    }
}
