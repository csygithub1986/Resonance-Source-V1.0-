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
using System.Windows.Shapes;

namespace Resonance
{
    /// <summary>
    /// 定位参数调整窗口
    /// </summary>
    public partial class LocParamWin : Window
    {
        /// <summary>
        /// 是否确认调整
        /// </summary>
        public bool IsOk;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isCalib">是否是标定时弹出的窗口。对于标定，无阈值选项</param>
        public LocParamWin(bool isCalib)
        {
            InitializeComponent();

            sliderThresh.Value = Params.GlobleThresh;
            sliderL.Value = Params.AF_L;
            sliderTcondition.Value = Params.AF_TCondition;
            if (isCalib)
            {
                gridThresh.Visibility = Visibility.Collapsed;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Params.GlobleThresh = sliderThresh.Value;
            Params.AF_L = sliderL.Value;
            Params.AF_TCondition = sliderTcondition.Value;
            IsOk = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            this.Close();
        }
    }
}
