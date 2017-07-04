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
using System.IO;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay;

namespace Resonance
{
    /// <summary>
    /// 在测试阶段作简略定位
    /// </summary>
    public partial class LocationInMeaWin : Window
    {
        private int _phase;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="phase">电缆相序</param>
        public LocationInMeaWin(int phase)
        {
            _phase = phase;
            InitializeComponent();
            txtPhase.Content = "ABC".Substring(phase, 1);
        }

        /// <summary>
        /// 分析按钮
        /// </summary>
        private void btnAnalyse_Click(object sender, RoutedEventArgs e)
        {
            FileInfo[] fileInfos = MeasureState.CableInfo.Path.GetFiles("ABC".Substring(_phase, 1) + "*.zdb");
            List<PulsePair> allList = new List<PulsePair>();
            foreach (var file in fileInfos)
            {
                double[] data;
                DataInfo.ReadRestainOnly(file, out data);

                Pulse[] pulses;
                AnalysisCore.FilterPeaks(data, Params.GlobleThresh, out pulses);

                AutoAnalyse aa = new AutoAnalyse(MeasureState.CalibInfos[_phase], MeasureState.CableInfo);
                List<PulsePair> resultList = new List<PulsePair>();
                aa.Do(pulses, ref resultList, file.Name);
                allList.AddRange(resultList);
            }
            DisplayMap(allList);
        }

        /// <summary>
        /// 绘制定位图谱
        /// </summary>
        /// <param name="allList">定位数据</param>
        public void DisplayMap(List<PulsePair> allList)
        {
            plotter.Children.RemoveAll(typeof(CircleHighlight));
            plotter.Children.RemoveAll(typeof(RectangleHighlight));
            plotter.Children.RemoveAll(typeof(PolygonHighlight));

            double max = 0;
            foreach (PulsePair pulsePair in allList)
            {
                ViewportShape rh = null;
                pulsePair.Q = Math.Abs(pulsePair.Amplitude) * MeasureState.CalibInfos[_phase].Discharge / MeasureState.CalibInfos[_phase].Amplitude;
                if (_phase == 0)
                {
                    rh = new CircleHighlight(5, new Point(pulsePair.Distance, pulsePair.Q));
                }
                else if (_phase == 1)
                {
                    rh = new RectangleHighlight(FixPosition.Center, new Point(pulsePair.Distance, pulsePair.Q), 9, 9);
                }
                else if (_phase == 2)
                {
                    rh = new PolygonHighlight(3, new Point(pulsePair.Distance, pulsePair.Q), 6);
                }
                rh.ToolTip = "放电量： " + pulsePair.Q + "\n位置  ： " + pulsePair.Distance;
                Color temp = Params.Colors[_phase];
                temp.A = 100;
                if (max < pulsePair.Q)
                {
                    max = pulsePair.Q;
                }
                rh.Fill = new SolidColorBrush(temp);
                rh.StrokeThickness = 0.5;
                rh.Stroke = Params.Brushes[_phase];
                plotter.Children.Add(rh);
            }
            plotter.Visible = new Rect(0, 0, MeasureState.CableInfo.Length, max * 1.1);
        }

        /// <summary>
        /// 改变分析阈值
        /// </summary>
        private void sliderThresh_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Params.GlobleThresh = sliderThresh.Value;
        }

        /// <summary>
        /// 改变积分长度
        /// </summary>
        private void sliderL_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Params.AF_L = sliderL.Value;
        }

        /// <summary>
        /// 改变脉宽系数
        /// </summary>
        private void sliderTcondition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Params.AF_TCondition = sliderTcondition.Value;
        }
    }
}
