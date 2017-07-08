using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace Resonance
{
    /// <summary>
    /// 高压和局放波形总体查看
    /// </summary>
    public partial class StepPage : Page
    {
        DataInfo _dataInfo;

        short[] hvValidData;
        short[] pdValidData;
        double[] hvShowData;
        double[] pdShowData;

        string[] phaseStr = { "A", "B", "C" };
        int currentPhase;

        ListBoxItem[] lbItems;

        public StepPage()
        {
            InitializeComponent();
            Init();
            AddFiles();
        }

        /// <summary>
        /// 加载文件列表
        /// </summary>
        private void AddFiles()
        {
            FileInfo[] fis = MeasureState.CableInfo.Path.GetFiles("*.zdb");
            //文件列表
            foreach (var fi in fis)
            {
                ListBoxItem lbitem = new ListBoxItem();
                lbitem.Tag = fi;
                lbitem.Content = fi.Name.Substring(0, fi.Name.Length - 4);
                lbitem.Selected += new RoutedEventHandler(FileItem_Selected);
                int cablePhase = "ABC".IndexOf(fi.Name.Substring(0, 1)) + 1;
                if (cablePhase == 3)
                {
                    lbFile.Items.Add(lbitem);
                }
                else
                {
                    int index = lbFile.Items.IndexOf(lbItems[cablePhase]);
                    lbFile.Items.Insert(index, lbitem);
                }
            }
        }

        /// <summary>
        /// 选则文件
        /// </summary>
        void FileItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            FileInfo fi = item.Tag as FileInfo;
            currentPhase = "ABC".IndexOf(fi.Name.Substring(0, 1));
            _dataInfo = DataInfo.Read(fi, out hvValidData, out pdValidData);
            //文件列表
            lbPeriod.Items.Clear();
            for (int i = 0; i < _dataInfo.Indexs.Length; i++)
            {
                ListBoxItem lbItem = new ListBoxItem();
                lbItem.Content = (i == 0 ? "All" : (i + ""));
                lbItem.Selected += new RoutedEventHandler(lbPeriodItem_Selected);
                lbPeriod.Items.Add(lbItem);
            }
            lbPeriod.SelectedIndex = 0;//显示
            ShowInfo(hvValidData.Max() * Params.HV_Coeffi);
        }

        /// <summary>
        /// 填充lbPeriod控件
        /// </summary>
        void ExtractPhase()
        {
            lbPeriod.Items.Clear();
            for (int i = 0; i < _dataInfo.Indexs.Length; i++)
            {
                ListBoxItem lbItem = new ListBoxItem();
                lbItem.Content = (i == 0 ? "All" : (i + ""));
                lbItem.Selected += new RoutedEventHandler(lbPeriodItem_Selected);
                lbPeriod.Items.Add(lbItem);
            }
            lbPeriod.SelectedIndex = 0;//显示
        }

        /// <summary>
        /// 绘制波形
        /// </summary>
        void ShowWave(int p, int[] indexs)
        {
            int multi = (int)(Params.SamRatePd / Params.SamRateHv);
            if (p == 0)
            {
                hvShowData = new double[indexs[indexs.Length - 1]];
                p = 1;
            }
            else
                hvShowData = new double[indexs[p] - indexs[p - 1]];
            pdShowData = new double[hvShowData.Length * multi];
            for (int i = 0; i < hvShowData.Length; i++)
            {
                hvShowData[i] = hvValidData[indexs[p - 1] + i] * Params.HV_Coeffi;
            }
            int pdstart = indexs[p - 1] * multi;
            for (int i = 0; i < pdShowData.Length; i++)
            {
                pdShowData[i] = pdValidData[pdstart + i] * Params.PD_Coeffi * Params.Range[_dataInfo.RangeIndex];
            }
            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            //{
            lineGraph2.LinePen = new Pen(Params.Brushes[_dataInfo.Phase], 1);
            DisplayHelper.DynamicDisplay(chartPlotter1, lineGraph1, hvShowData, 1.0 / Params.SamRateHv, 1, false);
            DisplayHelper.DynamicDisplay(chartPlotter2, lineGraph2, pdShowData, 1.0 / Params.SamRatePd, Params.mVTopC[currentPhase], false);
            //}));
            //double hvmax = MeasureState.CableInfo.U0 * 2;// hvShowData.Max() * 1.1;
            //double pdmax = 1000;// pdShowData.Max() * 1.1;
            double len = hvShowData.Length / Params.SamRateHv;

            //double max = hvShowData.Max(a => Math.Abs(a));
            //chartPlotter1.Visible = new Rect(0, -max * 1.1, len, 2 * max * 1.1);
            //chartPlotter2.Visible = new Rect(0, -pdmax, len, 2 * pdmax);
            chartPlotter1.Visible = new Rect(0, -_dataInfo.VoltageLevel * MeasureState.CableInfo.Upp * 1.1, len, 2 * _dataInfo.VoltageLevel * MeasureState.CableInfo.Upp * 1.1);
            chartPlotter2.Visible = new Rect(0, -Params.Range[_dataInfo.RangeIndex] * Params.mVTopC[currentPhase] * 1000, len, 2 * Params.Range[_dataInfo.RangeIndex] * Params.mVTopC[currentPhase] * 1000);
        }

        /// <summary>
        /// 选定并绘制特定周期波形
        /// </summary>
        void lbPeriodItem_Selected(object sender, RoutedEventArgs e)
        {
            int p = lbPeriod.Items.IndexOf(sender);// lbPeriod.SelectedIndex; //(int)((ListBoxItem)sender).Content;
            ShowWave(p, _dataInfo.Indexs);
        }

        /// <summary>
        /// 初始化控件
        /// </summary>
        private void Init()
        {
            //波形图
            new PlotterWrap().Wrap(chartPlotter1, MainWindow.Instance);
            new PlotterWrap().Wrap(chartPlotter2, MainWindow.Instance);
            chartPlotter1.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(plotter1_PropertyChanged);
            chartPlotter2.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(plotter2_PropertyChanged);
            chartPlotter1.Viewport.Visible = new DataRect(0, -20, 50, 40);
            chartPlotter2.Viewport.Visible = new DataRect(0, -1000, 50, 2000);

            //listboxFile
            lbItems = new ListBoxItem[] { lbItemA, lbItemB, lbItemC };
        }

        /// <summary>
        /// 高压波形缩放动态绘制事件
        /// </summary>
        void plotter1_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                if (hvShowData == null)
                {
                    return;
                }
                //chartPlotter1.Viewport.PropertyChanged -= new EventHandler<ExtendedPropertyChangedEventArgs>
                //(plotter1_PropertyChanged);
                DisplayHelper.DynamicDisplay(chartPlotter1, lineGraph1, hvShowData, 1.0 / Params.SamRateHv, 1.0, false);
                //chartPlotter1.Viewport.PropertyChanged -= new EventHandler<ExtendedPropertyChangedEventArgs>
                //      (plotter1_PropertyChanged);
                chartPlotter2.Viewport.Visible = new DataRect(chartPlotter1.Viewport.Visible.X,
                    chartPlotter2.Viewport.Visible.Y, chartPlotter1.Viewport.Visible.Width, chartPlotter2.Viewport.Visible.Height);
            }
        }

        /// <summary>
        /// 局放波形缩放动态绘制事件
        /// </summary>
        void plotter2_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                if (pdShowData == null)
                {
                    return;
                }
                //chartPlotter2.Viewport.PropertyChanged -= new EventHandler<ExtendedPropertyChangedEventArgs>
                //(plotter2_PropertyChanged);
                DisplayHelper.DynamicDisplay(chartPlotter2, lineGraph2, pdShowData, 1.0 / Params.SamRatePd, Params.mVTopC[currentPhase], false);
                //    chartPlotter2.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>
                //(plotter2_PropertyChanged);
                chartPlotter1.Viewport.Visible = new DataRect(chartPlotter2.Viewport.Visible.X,
                    chartPlotter1.Viewport.Visible.Y, chartPlotter2.Viewport.Visible.Width, chartPlotter1.Viewport.Visible.Height);
            }
        }

        /// <summary>
        /// HV纵标宽度改变
        /// </summary>
        private void HVAxis_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartPlotter1.Margin = new Thickness(35 + 30.963 - hvVerticalAxis.ActualWidth, 0, 0, 0);
        }

        /// <summary>
        /// PD纵标宽度改变
        /// </summary>
        private void PdAxis_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //30.963是初始宽度
            chartPlotter2.Margin = new Thickness(35 + 30.963 - pdVerticalAxis.ActualWidth, 0, 0, 0);
        }

        /// <summary>
        /// 删除选中文件
        /// </summary>
        private void MenuFile_Del(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("该操作不可恢复，确认删除该测试数据吗？", "删除", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (r == MessageBoxResult.OK)
            {
                ListBoxItem lbItem = lbFile.SelectedItem as ListBoxItem;
                FileInfo file = lbItem.Tag as FileInfo;
                lbFile.Items.Remove(lbItem);
                file.Delete();
                ClearPlotter();
            }
        }

        /// <summary>
        /// 清空波形界面
        /// </summary>
        private void ClearPlotter()
        {
            lineGraph1.DataSource = null;
            lineGraph2.DataSource = null;
            lbPeriod.Items.Clear();
        }

        /// <summary>
        /// 显示数据信息
        /// </summary>
        /// <param name="actualVol">实测高压幅值</param>
        private void ShowInfo(double actualVol)
        {
            txtPhase.Content = phaseStr[_dataInfo.Phase];
            txtCap.Content = _dataInfo.Cap.ToString("F1") + " nF";
            txtFre.Content = _dataInfo.Fre.ToString("F1") + " Hz";
            txtHvVol.Content = actualVol.ToString("F1") + " kV";
            txtMaxPd.Content = (_dataInfo.MaxPd * Params.mVTopC[currentPhase]).ToString("F1") + " " + Params.UnitChar;
        }
    }
}
