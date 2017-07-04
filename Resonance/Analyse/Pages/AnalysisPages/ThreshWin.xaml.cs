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

using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay;

namespace Resonance
{
    /// <summary>
    /// ThreshWin.xaml 的交互逻辑
    /// </summary>
    public partial class ThreshWin : Window
    {
        FileInfo[] fis;
        int index;

        double[] data;

        public bool Compeleted;

        public ThreshWin()
        {
            InitializeComponent();
            plotter.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(plotter1_PropertyChanged);
            //plotter.Children.Remove(plotter.HorizontalAxis);
            //plotter.Children.Remove(plotter.VerticalAxis);
            fis = MeasureState.CableInfo.Path.GetFiles("*.zdb");
            slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
        }

        void slider_ValueChanged<T>(object sender, RoutedPropertyChangedEventArgs<T> e)
        {
            threshLine.Value1 = -slider.Value;
            threshLine.Value2 = slider.Value;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowWave();
        }

        //取前两个周期
        private void ShowWave()
        {
            DataInfo di = DataInfo.ReadRestainOnly(fis[index], out data);
            double max = data.Max();
            plotter.Visible = new Rect(0, -max * 1.1, data.Length / Params.SamRatePd, 2 * max * 1.1);
            DisplayHelper.DynamicDisplay(plotter, lineGraph, data, 1 / Params.SamRatePd, 1, false);

            slider.Maximum = max;


            //FileThresh fileThresh = new FileThresh();
            string fileName = fis[index].Name;
            foreach (var item in AnalyseState.Instance.AllMapResults.Keys)
            {
                if (item.Equals(fileName))
                {
                    fileThresh.Thresh = item.Thresh;
                }
            }
            //fileThresh.Thresh = slider.Maximum - 2;
            if (fileThresh.Thresh == 0)
            {
                //峰值的均值设为阈值
                fileThresh.Thresh = AnalysisCore.AutoThresh(data);
            }

            slider.Value = fileThresh.Thresh;

            textBlock1.Text = fis[index].Name;

            if (index == fis.Length - 1)
                btnNext.Content = "完成";
            else
                btnNext.Content = "下一个";
            if (index == 0)
                btnLast.IsEnabled = false;
            else
                btnLast.IsEnabled = true;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int pulseCount = AnalysisCore.PulseCount(data, slider.Value);
            if (pulseCount > 200)//不能超过200个脉冲
            {
                MessageBox.Show("阈值过小，建议增大阈值", "信息", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            RecordCurrent();
            if (index == fis.Length - 1)
            {
                Compeleted = true;
                this.Close();
            }
            else
            {
                index++;
                ShowWave();
            }
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            //int pulseCount = AnalysisCore.PulseCount(data, slider.Value);
            //if (pulseCount > 100)
            //{
            //    MessageBox.Show("阈值过小，建议增大阈值", "信息", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    return;
            //}
            RecordCurrent();
            index--;
            ShowWave();
        }

        private void RecordCurrent()
        {
            FileThresh fileThresh = new FileThresh();
            fileThresh.FileName = fis[index].Name;
            fileThresh.Thresh = slider.Value;
            if (AnalyseState.Instance.AllMapResults.Keys.Contains(fileThresh))
            {
                AnalyseState.Instance.AllMapResults.Remove(fileThresh);
            }
            AnalyseState.Instance.AllMapResults.Add(fileThresh, new List<PulsePair>());
        }

        void plotter1_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                if (data == null)
                {
                    return;
                }
                DisplayHelper.DynamicDisplay(plotter, lineGraph, data, 1 / Params.SamRatePd, 1, false);
            }
        }



        //实验，全自动阈值
        public static void AutoAnalyse()
        {
            FileInfo[] fileInfos = MeasureState.CableInfo.Path.GetFiles("*.zdb");
            foreach (var file in fileInfos)
            {
                double[] tempData;
                DataInfo di = DataInfo.ReadRestainOnly(file, out tempData);


                double max = tempData.Max();


                FileThresh fileThresh = new FileThresh();
                fileThresh.FileName = file.Name;
                //foreach (var item in GlobalData.Instance.AllMapResults.Keys)
                //{
                //    if (item.Equals(fileThresh))
                //    {
                //        fileThresh.Thresh = item.Thresh;
                //    }
                //}
                //if (fileThresh.Thresh == 0)
                //{
                    //峰值的均值设为阈值
                    fileThresh.Thresh = AnalysisCore.AutoThresh(tempData);
                //}

                if (AnalyseState.Instance.AllMapResults.Keys.Contains(fileThresh))
                {
                    AnalyseState.Instance.AllMapResults.Remove(fileThresh);
                }
                AnalyseState.Instance.AllMapResults.Add(fileThresh, new List<PulsePair>());
            }
        }


    }
}
