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
using Microsoft.Research.DynamicDataDisplay;

namespace Resonance
{
    /// <summary>
    /// 标定信息总览页面
    /// </summary>
    public partial class CalibAndNoisePage : Page
    {
        /// <summary>
        /// 数据保存所在目录
        /// </summary>
        public DirectoryInfo _dire;

        /// <summary>
        /// 标定数据中的一段，用以显示脉冲匹配细节
        /// </summary>
        double[] _showData;

        /// <summary>
        /// 标定信息对象
        /// </summary>
        CalibrationInfo _caliInfo;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CalibAndNoisePage()
        {
            InitializeComponent();
            _dire = AnalyseState.Instance.Path;
        }

        /// <summary>
        /// 缩放时数据动态绘制事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Plotter_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                if (_showData == null)
                {
                    return;
                }
                DisplayHelper.DynamicDisplay(plotter, lineGraph, _showData, 1 / Params.SamRatePd, 1, false);
            }
        }

        /// <summary>
        /// 选中特定文件，进行处理和显示
        /// </summary>
        private void lbFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileInfo file = null;
            if (lbItemNoise.IsSelected)
            {
                file = new FileInfo(_dire.FullName + "/noise.dat");
                lineGraphCali.DataSource = null;
                if (file.Exists == false)
                {
                    lineGraph.DataSource = null;
                    MessageBox.Show("无噪声数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                NoiseInfo noise = NoiseInfo.ReadFile(file);
                _showData = new double[noise.NoiseData.Length];
                for (int i = 0; i < _showData.Length; i++)
                {
                    _showData[i] = noise.NoiseData[i] * Params.Range[noise.RangeIndex] * Params.PD_Coeffi;
                }

                double height = _showData.Max() - _showData.Min();
                plotter.Visible = new Rect(0, _showData.Min() - height * 0.1, _showData.Length / Params.SamRatePd, height * 1.2);
                DisplayHelper.DynamicDisplay(plotter, lineGraph, _showData, 1 / Params.SamRatePd, 1, false);
                plotter.Viewport.PropertyChanged += Plotter_PropertyChanged;
                ShowInfo(false);
            }
            else
            {
                if (lbItemA.IsSelected)
                {
                    file = new FileInfo(_dire.FullName + "/A.cal");
                }
                else if (lbItemB.IsSelected)
                {
                    file = new FileInfo(_dire.FullName + "/B.cal");
                }
                else if (lbItemC.IsSelected)
                {
                    file = new FileInfo(_dire.FullName + "/C.cal");
                }
                if (file.Exists == false)
                {
                    lineGraph.DataSource = null;
                    lineGraphCali.DataSource = null;
                    MessageBox.Show("无此相标定数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _caliInfo = CalibrationInfo.ReadFile(file);
                _showData = new double[_caliInfo.ShowData.Length];
                for (int i = 0; i < _showData.Length; i++)
                {
                    _showData[i] = _caliInfo.ShowData[i] * Params.Range[_caliInfo.RangeIndex] * Params.PD_Coeffi;
                }

                double[] showX = new double[_showData.Length];

                for (int i = 0; i < showX.Length; i++)
                {
                    showX[i] = i / Params.SamRatePd;//换算为ms
                }
                plotter.Viewport.PropertyChanged -= Plotter_PropertyChanged;
                DisplayHelper.StaticDisplay(lineGraph, showX, 1, _showData, 1);

                //衰减曲线
                int sideLen = 20;    //两边的长度
                double[] attenX = new double[_caliInfo.Index2 - _caliInfo.Index1 + sideLen * 2];//一边160ns
                for (int i = 0; i < attenX.Length; i++)
                {
                    attenX[i] = (_caliInfo.Index1 - sideLen + i) / Params.SamRatePd;
                }
                double[] attenY = new double[attenX.Length];
                for (int i = 0; i < attenY.Length; i++)
                {
                    attenY[i] = _showData[_caliInfo.Index1] * Math.Exp(-_caliInfo.Attenuation * (i - sideLen) / Params.SamRatePd * 1000);
                }
                DisplayHelper.StaticDisplay(lineGraphCali, attenX, 1, attenY, 1);

                plotter.FitToView();
                ShowInfo(true);
            }
        }

        /// <summary>
        /// 显示标定信息
        /// </summary>
        /// <param name="iscali">true代表标定数据，false代表噪声</param>
        private void ShowInfo(bool iscali)
        {
            if (iscali)
            {
                txtDischarge.Content = "放电量(pC): " + _caliInfo.Discharge.ToString("F0");
                txtAmplitude.Content = "测量幅值(mV): " + _caliInfo.Amplitude.ToString("F0");
                txtVelocity.Content = "波速(m/us): " + _caliInfo.Velocity.ToString("F1");
                txtAttenuation.Content = "衰减系数: " + _caliInfo.Attenuation.ToString("F3");
            }
            else
            {
                txtDischarge.Content = "";
                txtAmplitude.Content = "";
                txtVelocity.Content = "";
                txtAttenuation.Content = "";
            }
        }
    }
}
