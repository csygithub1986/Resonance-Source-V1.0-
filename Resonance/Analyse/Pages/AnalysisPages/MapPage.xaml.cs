using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace Resonance
{
    /// <summary>
    /// 定位页面
    /// </summary>
    public partial class MapPage : Page
    {
        VerticalRange _verticalCable = new VerticalRange();

        public MapPage()
        {
            _this = this;
            InitializeComponent();
        }

        public static MapPage _this;

        private void Page_Initialized(object sender, EventArgs e)
        {
            InitUI();
            InitComboVoltage();
            //plotterMapAll_Loaded(null, null);//显示一次
            DisplayMapAll();
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitUI()
        {
            plotterMapAll.Children.Remove(plotterMapAll.MouseNavigation);
            ShowJoints();

            //时间窗线
            _verticalCable.Fill = Brushes.Yellow;
            _verticalCable.Stroke = Brushes.Yellow;
            _verticalCable.Opacity = 0.2;
            _verticalCable.Visibility = Visibility.Collapsed;
            plotterLoc.Children.Add(_verticalCable);
        }

        /// <summary>
        /// 计算电压等级，并初始化下拉列表
        /// </summary>
        private void InitComboVoltage()
        {
            lbMaxVoltage.Items.Clear();
            //求出有多少电压
            List<double> voltageList = new List<double>();
            foreach (KeyValuePair<string, List<PulsePair>> pair in AnalyseState.Instance.AllMapResults)
            {
                //这里第二个判定条件可能是错的，可能要涉及PDIV，PDEV
                double voltage = double.Parse(pair.Key.Substring(1, 3));//文件名，即几倍U0
                if (voltageList.Contains(voltage) == false)
                {
                    voltageList.Add(voltage);
                }
            }
            //逆向排序
            voltageList.Sort(new Comparison<double>((x, y) =>
            {
                if (x < y)
                {
                    return 1;
                }
                else if (x == y)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }));
            for (int i = 0; i < voltageList.Count; i++)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = voltageList[i].ToString("F1") + " U0\t\t" + (voltageList[i] * MeasureState.CableInfo.Upp).ToString("F2") + " kV";
                lbi.Tag = voltageList[i];
                lbi.IsSelected = true;
                lbi.Selected += new RoutedEventHandler(lbi_Selected);
                lbi.Unselected += new RoutedEventHandler(lbi_Unselected);
                lbi.Margin = new Thickness(0, 3, 3, 0);
                lbi.BorderThickness = new Thickness(1);
                lbi.BorderBrush = Brushes.Gray;
                lbMaxVoltage.Items.Add(lbi);
                selectedVoltages.Add(voltageList[i]);
            }
        }

        /// <summary>
        /// 选择的电压等级列表
        /// </summary>
        List<double> selectedVoltages = new List<double>();

        /// <summary>
        /// 电压等级选择
        /// </summary>
        void lbi_Selected(object sender, RoutedEventArgs e)
        {
            double vol = (double)((ListBoxItem)sender).Tag;
            selectedVoltages.Add(vol);
            DisplayMapByVol(vol, true);
        }

        /// <summary>
        /// 电压等级取消选择
        /// </summary>
        void lbi_Unselected(object sender, RoutedEventArgs e)
        {
            double vol = (double)((ListBoxItem)sender).Tag;
            selectedVoltages.Remove(vol);
            DisplayMapByVol(vol, false);
        }

        /// <summary>
        /// 显示所有文件的分析结果
        /// </summary>
        public void DisplayMapAll()
        {
            plotterMapAll.Children.RemoveAll(typeof(CircleHighlight));
            plotterMapAll.Children.RemoveAll(typeof(RectangleHighlight));
            plotterMapAll.Children.RemoveAll(typeof(PolygonHighlight));

            string[] phases = new string[] { "", "A", "B", "C" };
            double max = 0;
            foreach (KeyValuePair<string, List<PulsePair>> pair in AnalyseState.Instance.AllMapResults)
            {
                //这里第二个判定条件可能是错的，可能要涉及PDIV，PDEV
                int phase = GetPhase(pair.Key);// int.Parse(pair.Key.FileName.Substring(0, 1));
                //if (pair.Key.FileName.Substring(0, 1).Equals(phases[phase]) && selectedVoltages.Contains(uptoKV))
                //{
                foreach (PulsePair pulsePair in pair.Value)
                {
                    ViewportShape rh = null;
                    pulsePair.Q = Math.Abs(pulsePair.Amplitude) * AnalyseState.Instance.CalibrationInfos[_ph].PcPerMv;
                    if (phase == 0)
                    {
                        rh = new CircleHighlight(5, new Point(pulsePair.Distance, pulsePair.Q));
                    }
                    else if (phase == 1)
                    {
                        rh = new RectangleHighlight(FixPosition.Center, new Point(pulsePair.Distance, pulsePair.Q), 9, 9);
                    }
                    else if (phase == 2)
                    {
                        rh = new PolygonHighlight(3, new Point(pulsePair.Distance, pulsePair.Q), 6);
                    }
                    rh.MouseEnter += new MouseEventHandler(rh_MouseEnter);
                    rh.MouseLeave += new MouseEventHandler(rh_MouseLeave);
                    rh.MouseDown += new MouseButtonEventHandler(rh_MouseDown);
                    rh.Tag = pulsePair;
                    rh.ToolTip = "放电量： " + pulsePair.Q + "\n位置  ： " + pulsePair.Distance;
                    Color temp = brushes[phase];
                    temp.A = 100;
                    if (max < pulsePair.Q)
                    {
                        max = pulsePair.Q;
                    }
                    rh.Fill = new SolidColorBrush(temp);
                    rh.StrokeThickness = 0.5;
                    rh.Stroke = new SolidColorBrush(brushes[phase]);
                    plotterMapAll.Children.Add(rh);
                    rh.Visibility = Visibility.Visible;
                }
                //}
            }

            //plotterMapAll.Visible = new Rect(0, 0, MeasureState.CableInfo.Length, GlobalData.Instance.AllMapResults.Values.Max(a => a.Max(b => Math.Abs(b.Amplitude))));
            plotterMapAll.Visible = new Rect(0, 0, MeasureState.CableInfo.Length, max * 1.1);
        }

        /// <summary>
        /// 按相序显示
        /// </summary>
        /// <param name="phase">相序</param>
        /// <param name="visibility">是否可见</param>
        private void DisplayByPhase(int phase, bool visibility)
        {
            if (phase == 1)
            {
                CircleHighlight[] chs = plotterMapAll.Children.OfType<CircleHighlight>().ToArray();
                foreach (var item in chs)
                {
                    double vol = GetVol(((PulsePair)item.Tag).BelongTo);
                    if (selectedVoltages.Contains(vol))
                    {
                        item.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
            else if (phase == 2)
            {
                RectangleHighlight[] chs = plotterMapAll.Children.OfType<RectangleHighlight>().ToArray();
                foreach (var item in chs)
                {
                    double vol = GetVol(((PulsePair)item.Tag).BelongTo);
                    if (selectedVoltages.Contains(vol))
                    {
                        item.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
            else if (phase == 3)
            {
                PolygonHighlight[] chs = plotterMapAll.Children.OfType<PolygonHighlight>().ToArray();
                foreach (var item in chs)
                {
                    double vol = GetVol(((PulsePair)item.Tag).BelongTo);
                    if (selectedVoltages.Contains(vol))
                    {
                        item.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// 通过文件名获得数据的相序
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>相序</returns>
        int GetPhase(string name)
        {
            return "ABC".IndexOf(name.Substring(0, 1));
        }

        /// <summary>
        /// 通过文件名获得数据的电压等级
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>电压等级</returns>
        double GetVol(string name)
        {
            return double.Parse(name.Substring(1, 3));
        }

        /// <summary>
        /// 定位图谱中打点图形鼠标进入事件
        /// </summary>
        void rh_MouseEnter(object sender, MouseEventArgs e)
        {
            ViewportShape vs = sender as ViewportShape;
            vs.StrokeThickness = 2;
            Cursor = Cursors.Hand;
            e.Handled = true;
        }

        /// <summary>
        /// 定位图谱中打点图形鼠标离开事件
        /// </summary>
        void rh_MouseLeave(object sender, MouseEventArgs e)
        {
            ViewportShape vs = sender as ViewportShape;
            vs.StrokeThickness = 0.5;
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }

        /// <summary>
        /// 选中的打点图形
        /// </summary>
        ViewportShape selectedVs;

        /// <summary>
        /// 用于显示脉冲细节的截选数据
        /// </summary>
        double[] locShowData;

        /// <summary>
        /// 当前选中的相序
        /// </summary>
        int _ph;

        /// <summary>
        /// 定位图谱中打点图形鼠标按下事件，寻找关联的数据
        /// </summary>
        void rh_MouseDown(object sender, MouseEventArgs e)
        {
            selectedVs = sender as ViewportShape;
            currentPiar = selectedVs.Tag as PulsePair;
            _ph = "ABC".IndexOf(currentPiar.BelongTo.Substring(0, 1));
            //打开波形
            FileInfo file = new FileInfo(AnalyseState.Instance.Path.FullName + "\\" + currentPiar.BelongTo);
            DataInfo.ReadRestainOnly(file, out locShowData);
            DisplayWave();
            ForeBackBtnEn();
            e.Handled = true;
        }

        /// <summary>
        /// 是否使能或禁用上一个波形和下一个波形的按钮
        /// </summary>
        private void ForeBackBtnEn()
        {
            int index = currentPiar.RTList.IndexOf(currentPiar.ReflectIndex);
            btnBackward.IsEnabled = index != 0;
            btnForward.IsEnabled = index != currentPiar.RTList.Count - 1;
        }

        /// <summary>
        /// 当前脉冲
        /// </summary>
        PulsePair currentPiar;

        /// <summary>
        /// 显示波形
        /// </summary>
        private void DisplayWave()
        {
            double timeWindow = MeasureState.CableInfo.Length * 2 / AnalyseState.Instance.CalibrationInfos[_ph].Velocity;//时间窗(us)
            int pointNum = (int)(timeWindow * 3 / 1000 * Params.SamRatePd);
            int pointStart = currentPiar.EnterIndex - pointNum / 3;//起始点
            pointStart = pointStart > 0 ? pointStart : 0;
            pointNum = pointStart + pointNum < locShowData.Length ? pointNum : locShowData.Length - pointStart;

            double[] showX = new double[pointNum];
            double[] showY = new double[pointNum];

            for (int i = 0; i < pointNum; i++)
            {
                showX[i] = (pointStart + i) / Params.SamRatePd;//换算为ms
            }
            Array.Copy(locShowData, pointStart, showY, 0, pointNum);

            DisplayHelper.StaticDisplay(lineGraphLoc, showX, 1, showY, 1);
            AddMarker(locShowData);

            ShowAtten();

            plotterLoc.FitToView();
            btnDeletePair.IsEnabled = true;
        }

        /// <summary>
        /// 显示衰减曲线
        /// </summary>
        private void ShowAtten()
        {
            //衰减曲线
            int len = currentPiar.ReflectIndex - currentPiar.EnterIndex;
            int side = (int)(len * 0.1);
            double[] attenX = new double[len + 2 * side];//一边10%
            for (int i = 0; i < attenX.Length; i++)
            {
                attenX[i] = (currentPiar.EnterIndex - side + i) / Params.SamRatePd;
            }
            double[] attenY = new double[attenX.Length];

            for (int i = 0; i < attenY.Length; i++)
            {
                attenY[i] = currentPiar.Amplitude * Math.Exp(-AnalyseState.Instance.CalibrationInfos[_ph].Attenuation * (i - side) / Params.SamRatePd*1000);
            }
            DisplayHelper.StaticDisplay(lineGraphAtten, attenX, 1, attenY, 1);
        }

        /// <summary>
        /// 清空波形
        /// </summary>
        private void ClearWave()
        {
            lineGraphLoc.DataSource = null;
            _verticalCable.Value1 = double.MaxValue;
            _verticalCable.Value2 = double.MaxValue;
            btnDeletePair.IsEnabled = false;

            plotterLoc.Children.RemoveAll(typeof(TriangleHighlight));

            //衰减和标定
            lineGraphAtten.DataSource = null;
            lineGraphCalib.DataSource = null;
        }

        TriangleHighlight thOut;

        /// <summary>
        /// 添加标记
        /// </summary>
        private void AddMarker(double[] data)
        {
            plotterLoc.Children.RemoveAll(typeof(TriangleHighlight));

            TriangleHighlight thIn = new TriangleHighlight(currentPiar.Amplitude > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(currentPiar.EnterIndex / Params.SamRatePd, data[currentPiar.EnterIndex]), 10);
            thIn.Fill = Brushes.Green;
            thIn.StrokeThickness = 0;
            plotterLoc.Children.Add(thIn);

            for (int i = 0; i < currentPiar.RTList.Count; i++)
            {
                TriangleHighlight thPoten = new TriangleHighlight(currentPiar.Amplitude > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(currentPiar.RTList[i] / Params.SamRatePd, data[currentPiar.RTList[i]]), 10);
                thPoten.Fill = Brushes.Gray;
                thPoten.StrokeThickness = 0;
                plotterLoc.Children.Add(thPoten);
            }

            thOut = new TriangleHighlight(currentPiar.Amplitude > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(currentPiar.ReflectIndex / Params.SamRatePd, data[currentPiar.ReflectIndex]), 10);
            thOut.Fill = Brushes.Red;
            thOut.StrokeThickness = 0;
            plotterLoc.Children.Add(thOut);

            //时间窗线
            _verticalCable.Visibility = switchBoundary.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            _verticalCable.Value1 = currentPiar.EnterIndex / Params.SamRatePd;
            _verticalCable.Value2 = currentPiar.EnterIndex / Params.SamRatePd + 2 * MeasureState.CableInfo.Length / AnalyseState.Instance.CalibrationInfos[_ph].Velocity / 1000;

        }

        Color[] brushes = new Color[] { Params.Color1, Params.Color2, Params.Color3 };

        /// <summary>
        /// 相序选择
        /// </summary>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (lbMaxVoltage.SelectedItems == null)
            {
                return;
            }
            CheckBox cb = sender as CheckBox;
            bool isCheck = cb.IsChecked == true;
            int phase = 0;
            if (cb.Equals(checkBoxA))
            {
                phase = 1;
            }
            else if (cb.Equals(checkBoxB))
            {
                phase = 2;
            }
            else
            {
                phase = 3;
            }
            DisplayByPhase(phase, cb.IsChecked == true);
        }

        /// <summary>
        /// 添加接头
        /// </summary>
        private void ShowJoints()
        {
            mapJoint.Marker = new RectPointMarker() { Fill = new SolidColorBrush(Colors.Black), Width = 10, Height = 16, Pen = new Pen(Brushes.Black, 0.5) };
            List<double> joints = MeasureState.CableInfo.Joints;
            EnumerableDataSource<double> baseXData = new EnumerableDataSource<double>(joints);
            EnumerableDataSource<double> baseYData = new EnumerableDataSource<double>(new double[joints.Count]);
            baseXData.SetXMapping(x => x);
            baseYData.SetYMapping(y => y);
            CompositeDataSource cds = baseXData.Join(baseYData);
            mapJoint.DataSource = cds;
        }

        /// <summary>
        /// 按电压等级显示
        /// </summary>
        /// <param name="voltage"></param>
        /// <param name="visibility"></param>
        private void DisplayMapByVol(double voltage, bool visibility)
        {
            if (checkBoxA.IsChecked == true)
            {
                CircleHighlight[] chs = plotterMapAll.Children.OfType<CircleHighlight>().ToArray();
                foreach (var item in chs)
                {
                    double vol = GetVol(((PulsePair)item.Tag).BelongTo);
                    if (vol == voltage)
                    {
                        item.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
            if (checkBoxB.IsChecked == true)
            {
                RectangleHighlight[] chs = plotterMapAll.Children.OfType<RectangleHighlight>().ToArray();
                foreach (var item in chs)
                {
                    double vol = GetVol(((PulsePair)item.Tag).BelongTo);
                    if (vol == voltage)
                    {
                        item.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
            if (checkBoxC.IsChecked == true)
            {
                PolygonHighlight[] chs = plotterMapAll.Children.OfType<PolygonHighlight>().ToArray();
                foreach (var item in chs)
                {
                    double vol = GetVol(((PulsePair)item.Tag).BelongTo);
                    if (vol == voltage)
                    {
                        item.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// 显示时间窗
        /// </summary>
        private void switchBoundary_Checked(object sender, RoutedEventArgs e)
        {
            _verticalCable.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 隐藏时间窗
        /// </summary>
        private void switchBoundary_Unchecked(object sender, RoutedEventArgs e)
        {
            _verticalCable.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 维护波峰访问顺序，当前显示的第几个
        /// </summary>
        public int _currentId = 1;

        /// <summary>
        /// 在待定反射波中选择，下一个
        /// </summary>
        private void BtnForward(object sender, RoutedEventArgs e)
        {
            int index = currentPiar.RTList.IndexOf(currentPiar.ReflectIndex);
            currentPiar.ReflectIndex = currentPiar.RTList[index + 1];
            thOut.FixPoint = new Point(currentPiar.ReflectIndex / Params.SamRatePd, locShowData[currentPiar.ReflectIndex]);
            double timeSpan = (currentPiar.ReflectIndex - currentPiar.EnterIndex) / Params.SamRatePd;
            currentPiar.Distance = MeasureState.CableInfo.Length - timeSpan * 1000 * AnalyseState.Instance.CalibrationInfos[_ph].Velocity / 2;
            ChangeVsPosition(new Point(currentPiar.Distance, currentPiar.Q));
            ForeBackBtnEn();
            ShowAtten();
        }

        /// <summary>
        /// 在待定反射波中选择，下一个
        /// </summary>
        private void BtnBackward(object sender, RoutedEventArgs e)
        {
            int index = currentPiar.RTList.IndexOf(currentPiar.ReflectIndex);
            currentPiar.ReflectIndex = currentPiar.RTList[index - 1];
            thOut.FixPoint = new Point(currentPiar.ReflectIndex / Params.SamRatePd, locShowData[currentPiar.ReflectIndex]);
            double timeSpan = (currentPiar.ReflectIndex - currentPiar.EnterIndex) / Params.SamRatePd;
            currentPiar.Distance = MeasureState.CableInfo.Length - timeSpan * 1000 * AnalyseState.Instance.CalibrationInfos[_ph].Velocity / 2;
            ChangeVsPosition(new Point(currentPiar.Distance, currentPiar.Q));
            ForeBackBtnEn();
            ShowAtten();
        }

        /// <summary>
        /// 更改了反射波以后，改变定位点的位置
        /// </summary>
        /// <param name="p">改变至的坐标</param>
        private void ChangeVsPosition(Point p)
        {
            if (selectedVs is CircleHighlight)
            {
                ((CircleHighlight)selectedVs).Center = p;
            }
            else if (selectedVs is RectangleHighlight)
            {
                ((RectangleHighlight)selectedVs).FixPoint = p;
            }
            else
            {
                ((PolygonHighlight)selectedVs).Center = p;
            }
        }

        /// <summary>
        /// 删除该定位匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDeletePair_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确认删除该放电配对吗?", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            AnalyseState.Instance.AllMapResults[currentPiar.BelongTo].Remove(currentPiar);
            plotterMapAll.Children.Remove(selectedVs);
            selectedVs = null;
            ClearWave();
        }

        /// <summary>
        /// 开始分析
        /// </summary>
        public void StartAnalyse()
        {
            //ThreshWin.AutoAnalyse();
            FileInfo[] fileInfos = MeasureState.CableInfo.Path.GetFiles("*.zdb");
            foreach (var file in fileInfos)
            {
                if (AnalyseState.Instance.AllMapResults.Keys.Contains(file.Name))
                {
                    AnalyseState.Instance.AllMapResults.Remove(file.Name);
                }
                AnalyseState.Instance.AllMapResults.Add(file.Name, new List<PulsePair>());
            }

            for (int i = 0; i < 3; i++)
            {
                AnalyseState.Instance.Prp[i].Clear();
            }

            //开始分析
            foreach (var item in AnalyseState.Instance.AllMapResults)
            {
                FileInfo fi = new FileInfo(AnalyseState.Instance.Path.FullName + "\\" + item.Key);
                double[] data;
                DataInfo.ReadRestainOnly(fi, out data);

                Pulse[] pulses;
                AnalysisCore.FilterPeaks(data, Params.GlobleThresh, out pulses);
                int phase = "ABC".IndexOf(fi.Name.Substring(0, 1));

                for (int i = 0; i < pulses.Length; i++)
                {
                    AnalyseState.Instance.Prp[phase].Add(new Point(pulses[i].Phase, pulses[i].Amplitude * AnalyseState.Instance.CalibrationInfos[phase].PcPerMv));
                }
                AutoAnalyse aa = new AutoAnalyse(AnalyseState.Instance.CalibrationInfos[phase], MeasureState.CableInfo);
                List<PulsePair> resultList = item.Value;
                aa.Do(pulses, ref resultList, item.Key);
            }

            //绘制
            InitComboVoltage();
            MapPage._this.DisplayMapAll();

            //保存
            PulsePair.WriteMapFile(new FileInfo(AnalyseState.Instance.Path.FullName + "/result.map"));
            Prp.WriteFile(new FileInfo(AnalyseState.Instance.Path.FullName + "/prpd.dat"), AnalyseState.Instance.Prp);
        }

        /// <summary>
        /// 显示衰减曲线
        /// </summary>
        private void switchAtten_Checked(object sender, RoutedEventArgs e)
        {
            lineGraphAtten.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 隐藏衰减曲线
        /// </summary>
        private void switchAtten_Unchecked(object sender, RoutedEventArgs e)
        {
            lineGraphAtten.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// （弃用）
        /// </summary>
        private void switchCalib_Checked(object sender, RoutedEventArgs e)
        {
            lineGraphCalib.Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// （弃用）
        /// </summary>
        private void switchCalib_Unchecked(object sender, RoutedEventArgs e)
        {
            lineGraphCalib.Visibility = Visibility.Hidden;
        }
    }
}
