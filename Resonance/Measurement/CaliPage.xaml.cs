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
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace Resonance
{
    /// <summary>
    /// 标定页面
    /// </summary>
    public partial class CaliPage : Page
    {
        /// <summary>
        /// 本页当前读出的，或者刚标定完成的CalibrationInfo
        /// </summary>
        CalibrationInfo _caliInfo;

        public CaliPage()
        {
            InitializeComponent();

            cbPhase.Items.Add("A");
            cbPhase.Items.Add("B");
            cbPhase.Items.Add("C");

            cbPhase.SelectionChanged += CbPhase_SelectionChanged;
            cbPhase.SelectedIndex = 0;
        }

        /// <summary>
        /// 选择标定的相序。通过保存的文件信息检查相应相序是否已经标定，如果已经标定，显示标定信息，且可以进行下一步测试。如果未标定，强行用户先进行标定
        /// </summary>
        public void CbPhase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string file = (string)cbPhase.SelectedItem + ".cal";
            FileInfo fileInfo = new FileInfo(MeasureState.CableInfo.Path.FullName + "/" + file);
            if (!fileInfo.Exists)
            {
                txtHint.Text = "未标定！";
                txtHint.Foreground = Brushes.Red;
                btnCalib.Content = "标定";
                btnDetail.IsEnabled = false;

                txtDischarge.Text = "";
                txtAmplitude.Text = "";
                txtVelocity.Text = "";
                txtAttenuation.Text = "";

                btnNext.IsEnabled = false;
                return;
            }
            _caliInfo = CalibrationInfo.ReadFile(fileInfo);

            txtHint.Text = "已标定！";
            txtHint.Foreground = Brushes.Green;
            btnCalib.Content = "重新标定";
            btnDetail.IsEnabled = true;

            ShowInfo();

            btnNext.IsEnabled = true;
        }

        /// <summary>
        /// 显示标定信息
        /// </summary>
        private void ShowInfo()
        {
            txtDischarge.Text = "放电量(pc): \t" + _caliInfo.Discharge.ToString("F0");
            txtAmplitude.Text = "测量幅值(mV): \t" + _caliInfo.Amplitude.ToString("F0");
            txtVelocity.Text = "波速(m/us): \t" + _caliInfo.Velocity.ToString("F1");
            txtAttenuation.Text = "衰减系数: \t" + _caliInfo.Attenuation.ToString("F3");
        }


        /// <summary>
        /// 查看标定详情
        /// </summary>
        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            CalibDetailWin cdw = new CalibDetailWin(_caliInfo, false);
            cdw.Owner = MainWindow.Instance;
            cdw.ShowDialog();
        }

        /// <summary>
        /// 点击标定按钮，弹出进行标定的窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalib_Click(object sender, RoutedEventArgs e)
        {
            CalibDetailWin cdw = new CalibDetailWin(null, true);
            cdw.Owner = MainWindow.Instance;
            cdw.ShowDialog();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                if (cdw.IsOk)
                {
                    txtHint.Text = "已标定";
                    txtHint.Foreground = Brushes.Green;
                    btnCalib.Content = "重新标定";
                    btnNext.IsEnabled = true;
                    btnDetail.IsEnabled = true;

                    _caliInfo = cdw.CalInfo;
                    ShowInfo();
                    CalibrationInfo.WriteFile(_caliInfo, new FileInfo(MeasureState.CableInfo.Path.FullName + "/" + cbPhase.SelectedItem + ".cal"));
                }
            }));
        }

        /// <summary>
        /// 下一步测试按钮，进入测试页面
        /// </summary>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            MeasureState.CalibInfos[cbPhase.SelectedIndex] = _caliInfo;
            MeasurePage mp = new MeasurePage(cbPhase.SelectedIndex);
            NavigationService.Navigate(mp);
        }

        /// <summary>
        /// 页面初始化，注册网络断链后的处理事件
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MeasureState.DisconnectEvent += new Action(DisconnectEvent);
        }

        /// <summary>
        /// 页面撤销时，注销网络断链后的处理事件
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MeasureState.DisconnectEvent -= new Action(DisconnectEvent);
        }

        /// <summary>
        /// 网络断链处理：显示WlanPage
        /// </summary>
        void DisconnectEvent()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                WlanPage wp = new WlanPage();
                NavigationService.Navigate(wp);
            }));
        }
    }
}
