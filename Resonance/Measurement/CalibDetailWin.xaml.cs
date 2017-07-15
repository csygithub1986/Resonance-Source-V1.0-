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
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 标定处理窗口
    /// </summary>
    public partial class CalibDetailWin : Window
    {
        /// <summary>
        /// 表示标定是否成功
        /// </summary>
        public bool IsOk;

        /// <summary>
        /// 标定数据接收Buffer
        /// </summary>
        short[] pdReceiveBuffer;

        /// <summary>
        /// 标定数据接收索引
        /// </summary>
        int pdReceiveIndex = 0;

        /// <summary>
        /// 标定信息
        /// </summary>
        public CalibrationInfo CalInfo;

        /// <summary>
        /// 所有接收到的标定数据
        /// </summary>
        double[] _dataAll;

        /// <summary>
        /// 截取的一段数据，用以显示脉冲匹配细节
        /// </summary>
        double[] _dataShow;

        /// <summary>
        /// 阈值的拖动圆点控件
        /// </summary>
        DraggablePoint _dragThresh;

        /// <summary>
        /// 阈值水平线
        /// </summary>
        HorizontalRange _hr;

        /// <summary>
        /// 用以记录选取的脉冲
        /// </summary>
        Pulse[] _pulses;

        TriangleHighlight _thIn;
        TriangleHighlight _thOut;

        int currentIndex = 1;

        /// <summary>
        /// 量程index，与Params里的量程对应
        /// </summary>
        int rangeIndex;
        double _discharge;

        DraggablePoint _dragPoint1;
        DraggablePoint _dragPoint2;
        VerticalLine _vl1;
        VerticalLine _vl2;

        VerticalRange _verticalRange;
        DraggablePoint _dragRange;
        double _analyseBegin = 0;//ms
        int _analyseLen = 125000;
        bool isNoise;

        public CalibDetailWin(CalibrationInfo cdata, bool first)
        {
            InitializeComponent();
            InitComboBox();

            plotterAll.Children.Remove(plotterAll.MouseNavigation);
            plotterDetail.Children.Remove(plotterDetail.MouseNavigation);

            if (first)
            {
                //InitThresh();
                gridAnalyse.IsEnabled = false;
                InitManualDrag();
            }
            else
            {
                CalInfo = cdata;
                _dataAll = cdata.AllData.Select(a => a * Params.PD_Coeffi * Params.Range[cdata.RangeIndex]).ToArray();
                _dataShow = cdata.ShowData.Select(a => a * Params.PD_Coeffi * Params.Range[cdata.RangeIndex]).ToArray();
                CalInfo.Amplitude = _dataAll.Max();
                for (int i = 0; i < discharges.Length; i++)
                {
                    if (cdata.Discharge == discharges[i])
                    {
                        cbDischarge.SelectedIndex = i;
                        break;
                    }
                }
                cbScale.SelectedIndex = cdata.RangeIndex;
                btnCancel.IsEnabled = false;
                gridControl.IsEnabled = false;
                ShowSavedCali();
                ShowAll();
                ShowInfo();
            }
        }

        /// <summary>
        /// 初始化量程、电荷量
        /// </summary>
        private void InitComboBox()
        {
            cbScale.Items.Add("100 mV");
            cbScale.Items.Add("200 mV");
            cbScale.Items.Add("1 V");
            cbScale.Items.Add("10 V");
            cbScale.SelectionChanged += cbScale_SelectionChanged;
            cbScale.SelectedIndex = 2;



            cbDischarge.Items.Add("50 pC");
            cbDischarge.Items.Add("100 pC");
            cbDischarge.Items.Add("500 pC");
            cbDischarge.Items.Add("1 nC");
            cbDischarge.Items.Add("5 nC");
            cbDischarge.Items.Add("10 nC");
            cbDischarge.Items.Add("20 nC");
            cbDischarge.Items.Add("50 nC");
            cbDischarge.Items.Add("100 nC");
            cbDischarge.SelectionChanged += cbDischarge_SelectionChanged;
            cbDischarge.SelectedIndex = 2;

        }

        double[] discharges = { 50, 100, 500, 1000, 5000, 10000, 20000, 50000, 100000 };

        void cbDischarge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _discharge = discharges[cbDischarge.SelectedIndex];
        }

        /// <summary>
        /// 发送量程切换命令
        /// </summary>
        void cbScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rangeIndex = cbScale.SelectedIndex;
            new Thread(() =>
            {
                byte[] temp = new byte[4];
                Array.Copy(BitConverter.GetBytes(Cmd.CMD_SWITCHRANGE), temp, 2);
                Array.Copy(BitConverter.GetBytes((short)(rangeIndex + 1)), 0, temp, 2, 2);//固定以200Hz
                MeasureState.TcpBinaryWriter.Write(temp);
            }).Start();
        }

        /// <summary>
        /// 初始化阈值控件
        /// </summary>
        private void InitThresh()
        {
            _dragThresh = new DraggablePoint(new Point(10, 5));
            plotterAll.Children.Add(_dragThresh);
            _dragThresh.PositionChanged += DpThresh_PositionChanged;

            _hr = new HorizontalRange();
            _hr.Value1 = 0;
            _hr.Value2 = 0;
            _hr.Fill = new SolidColorBrush(Color.FromArgb(80, 0, 255, 0));
            _hr.Stroke = Brushes.Green;
            plotterAll.Children.Add(_hr);

            _verticalRange = new VerticalRange();
            _verticalRange.Fill = Brushes.Yellow;
            _verticalRange.Value1 = 0;
            _verticalRange.Value2 = 1;//1ms //6 * MeasureState.CableInfo.Length / 170 / 1000;
            plotterAll.Children.Add(_verticalRange);

            _dragRange = new DraggablePoint(new Point(0.5, 0));
            plotterAll.Children.Add(_dragRange);
            _dragRange.PositionChanged += DragRange_PositionChanged;
        }

        /// <summary>
        /// 分析区域选择变化及相应限制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DragRange_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            if (_dragRange.Position.X < 0.5)
            {
                _dragRange.Position = new Point(0.5, _dragRange.Position.Y);
            }
            if (_dragRange.Position.X >= 29.5)
            {
                _dragRange.Position = new Point(29.5, _dragRange.Position.Y);
            }
            _verticalRange.Value1 = _dragRange.Position.X - 0.5;
            _verticalRange.Value2 = _dragRange.Position.X + 0.5;
            _analyseBegin = _dragRange.Position.X - 0.5;//ms
        }

        /// <summary>
        /// 手动寻找脉冲的控件初始化
        /// </summary>
        private void InitManualDrag()
        {
            _dragPoint1 = new DraggablePoint(new Point(0, 0));
            _dragPoint1.Visibility = Visibility.Collapsed;
            plotterDetail.Children.Add(_dragPoint1);
            _dragPoint1.PositionChanged += Dp1_ValueChanged;

            _dragPoint2 = new DraggablePoint(new Point(0, 0));
            _dragPoint2.Visibility = Visibility.Collapsed;
            plotterDetail.Children.Add(_dragPoint2);
            _dragPoint2.PositionChanged += Dp2_ValueChanged;

            _vl1 = new VerticalLine();
            _vl1.Stroke = Brushes.Green;
            _vl1.Visibility = Visibility.Collapsed;
            plotterDetail.Children.Add(_vl1);

            _vl2 = new VerticalLine();
            _vl2.Stroke = Brushes.Red;
            _vl2.Visibility = Visibility.Collapsed;
            plotterDetail.Children.Add(_vl2);
        }

        /// <summary>
        /// 手动调整入射波位置
        /// </summary>
        void Dp1_ValueChanged(object sender, PositionChangedEventArgs e)
        {
            //最小分辨率处理
            int index = (int)Math.Round(_dragPoint1.Position.X * Params.SamRatePd);
            _dragPoint1.Position = new Point(_dragPoint1.Position.X, 0);
            _vl1.Value = _dragPoint1.Position.X;
            _thIn.FixPoint = new Point(index / Params.SamRatePd, _dataShow[index]);
            _thIn.FixPostion = _thIn.FixPoint.Y > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up;
            CalInfo.Amplitude = Math.Abs(_thIn.FixPoint.Y);

            if (_thIn.FixPoint.Y * _thOut.FixPoint.Y <= 0)
            {
                CalInfo.Attenuation = double.NaN;
                CalInfo.Velocity = 0;
                lineGraphCali.DataSource = null;
                ShowInfo();
                return;
            }

            ShowAtten((int)(_thIn.FixPoint.X * Params.SamRatePd), (int)(_thOut.FixPoint.X * Params.SamRatePd));
            ShowInfo();
        }

        /// <summary>
        /// 手动调整反射波位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Dp2_ValueChanged(object sender, PositionChangedEventArgs e)
        {
            int index = (int)Math.Round(_dragPoint2.Position.X * Params.SamRatePd);
            _dragPoint2.Position = new Point(_dragPoint2.Position.X, 0);
            _vl2.Value = _dragPoint2.Position.X;
            _thOut.FixPoint = new Point(index / Params.SamRatePd, _dataShow[index]);
            _thOut.FixPostion = _thOut.FixPoint.Y > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up;

            if (_thIn.FixPoint.Y * _thOut.FixPoint.Y <= 0)
            {
                CalInfo.Attenuation = double.NaN;
                CalInfo.Velocity = 0;
                lineGraphCali.DataSource = null;
                ShowInfo();
                return;
            }

            ShowAtten((int)(_thIn.FixPoint.X * Params.SamRatePd), (int)(_thOut.FixPoint.X * Params.SamRatePd));
            ShowInfo();
        }

        /// <summary>
        /// 手动调整阈值
        /// </summary>
        void DpThresh_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            if (CalInfo.Amplitude < 1)
            {
                return;
            }
            if (_dragThresh.Position.Y < 1)//最小1mV
            {
                _dragThresh.Position = new Point(_dragThresh.Position.X, 1);
            }
            if (_dragThresh.Position.Y > CalInfo.Amplitude)
            {
                _dragThresh.Position = new Point(_dragThresh.Position.X, CalInfo.Amplitude);
            }
            _hr.Value1 = _dragThresh.Position.Y;
            _hr.Value2 = -_hr.Value1;
        }

        /// <summary>
        /// 截取配对脉冲附近区域的数据作为细节显示
        /// </summary>
        private void GetShowData()
        {
            //找出datashow
            if (_dataAll == null)
            {
                return;
            }
            int indexBegin = (int)(_analyseBegin * Params.SamRatePd);

            double max = 0;
            int index = 0;
            for (int i = indexBegin; i < indexBegin + _analyseLen; i++)
            {
                if (CalInfo.AllData[i] > max)
                {
                    max = CalInfo.AllData[i];
                    index = i;
                }
            }
            //取i附近 长度*2/速度的三倍的 窗口(默认速度170附近)
            int len = (int)(MeasureState.CableInfo.Length * 2 / 170 * 3 * Params.SamRatePd / 1000);
            int startIndex = 0 < index - len / 3 ? index - len / 3 : 0;
            if (startIndex + len > CalInfo.AllData.Length)
            {
                startIndex = CalInfo.AllData.Length - len;
            }
            CalInfo.ShowData = new short[len];
            for (int i = 0; i < len; i++)
            {
                CalInfo.ShowData[i] = CalInfo.AllData[startIndex + i];
            }
            _dataShow = CalInfo.ShowData.Select(a => a * Params.PD_Coeffi * Params.Range[rangeIndex]).ToArray();
            //去除偏移
            CalInfo.Amplitude = _dataShow.Max();
        }

        /// <summary>
        /// 自动分析，找匹配脉冲
        /// </summary>
        private void Analyse()
        {
            //找出datashow
            if (_dataAll == null)
            {
                return;
            }
            GetShowData();
            //plotterDetail.Children.RemoveAll(typeof(TriangleHighlight));
            double[] showX = new double[_dataShow.Length];
            for (int i = 0; i < showX.Length; i++)
            {
                showX[i] = i / Params.SamRatePd;//换算为ms
            }
            DisplayHelper.StaticDisplay(lineGraphDetail, showX, 1, _dataShow, 1);

            //找脉冲点
            Pulse[] pulses;
            AnalysisCore.FilterPeaks(_dataShow, _dragThresh.Position.Y, out pulses);
            if (pulses.Length < 2)
            {
                MessageBox.Show("无法检测出反射脉冲", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //按峰值排序
            Array.Sort(pulses, (a, b) =>
            {
                if (a.Amplitude < b.Amplitude)
                    return 1;//可以降序排列
                if (a.Amplitude > b.Amplitude)
                    return -1;
                else return 0;
            });
            //波速范围150到190
            int minSpace = (int)(MeasureState.CableInfo.Length * 2 / 190 / 1000 * Params.SamRatePd) + pulses[0].Index;
            int maxSpace = (int)(MeasureState.CableInfo.Length * 2 / 150 / 1000 * Params.SamRatePd) + pulses[0].Index;
            List<Pulse> pulseList = new List<Pulse>();
            foreach (var item in pulses)
            {
                if (item.Index == pulses[0].Index || (item.Index < maxSpace && item.Index > minSpace))
                {
                    pulseList.Add(item);
                }
            }
            pulses = pulseList.ToArray();

            if (pulses.Length < 2)
            {
                MessageBox.Show("无法检测出反射脉冲", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                btnOk.IsEnabled = true;
                return;
            }
            _pulses = pulses;
            btnOk.IsEnabled = true;
            ShowCali();
        }

        /// <summary>
        /// 当标定完成后，作为细节查看时，显示标定细节
        /// </summary>
        private void ShowSavedCali()
        {
            double[] showX = new double[_dataShow.Length];

            for (int i = 0; i < showX.Length; i++)
            {
                showX[i] = i / Params.SamRatePd;//换算为ms
            }
            DisplayHelper.StaticDisplay(lineGraphDetail, showX, 1, _dataShow, 1);
            double amplitude1 = _dataShow[CalInfo.Index1];
            double amplitude2 = _dataShow[CalInfo.Index2];


            _thIn = new TriangleHighlight(amplitude1 > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(CalInfo.Index1 / Params.SamRatePd, amplitude1), 10);
            _thIn.Fill = Brushes.Green;
            _thIn.StrokeThickness = 0;
            plotterDetail.Children.Add(_thIn);

            _thOut = new TriangleHighlight(amplitude2 > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(CalInfo.Index2 / Params.SamRatePd, amplitude2), 10);
            _thIn.Fill = Brushes.Red;
            _thIn.StrokeThickness = 0;
            plotterDetail.Children.Add(_thOut);

            int sideLen = 20;    //两边的长度
            double[] attenX = new double[CalInfo.Index2 - CalInfo.Index1 + sideLen * 2];//一边20ns
            for (int i = 0; i < attenX.Length; i++)
            {
                attenX[i] = (CalInfo.Index1 - sideLen + i) / Params.SamRatePd;
            }
            double[] attenY = new double[attenX.Length];
            for (int i = 0; i < attenY.Length; i++)
            {
                attenY[i] = amplitude1 * Math.Exp(-CalInfo.Attenuation * (i - sideLen) / Params.SamRatePd * 1000);
            }
            DisplayHelper.StaticDisplay(lineGraphCali, attenX, 1, attenY, 1);
        }

        /// <summary>
        /// 显示所有标定数据
        /// </summary>
        private void ShowAll()
        {
            double height = _dataAll.Max() - _dataAll.Min();
            plotterAll.Visible = new Rect(0, _dataAll.Min() - height * 0.1, _dataAll.Length / Params.SamRatePd, height * 1.2);
            DisplayHelper.DynamicDisplay(plotterAll, lineGraphAll, _dataAll, 1 / Params.SamRatePd, 1, false);
            plotterAll.Viewport.PropertyChanged += PlotterAll_PropertyChanged;
        }

        /// <summary>
        /// 波形Zooming时动态绘制数据事件
        /// </summary>
        void PlotterAll_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                if (_dataAll == null)
                {
                    return;
                }
                DisplayHelper.DynamicDisplay(plotterAll, lineGraphAll, _dataAll, 1 / Params.SamRatePd, 1, false);
            }
        }

        /// <summary>
        /// 当标定时，显示标定细节（与标定后查看细节对应）
        /// </summary>
        private void ShowCali()
        {
            plotterDetail.Children.RemoveAll(typeof(TriangleHighlight));
            _thIn = new TriangleHighlight(_pulses[0].Amplitude > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(_pulses[0].Index / Params.SamRatePd, _pulses[0].Amplitude), 10);
            _thIn.Fill = Brushes.Green;
            _thIn.StrokeThickness = 0;
            plotterDetail.Children.Add(_thIn);
            _thOut = new TriangleHighlight(_pulses[currentIndex].Amplitude > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(_pulses[currentIndex].Index / Params.SamRatePd, _pulses[currentIndex].Amplitude), 10);
            _thOut.Fill = Brushes.Red;
            _thOut.StrokeThickness = 0;
            plotterDetail.Children.Add(_thOut);

            if (_pulses.Length < 3)
            {
                btnDown.IsEnabled = false;
            }
            ShowAtten(_pulses[0].Index, _pulses[currentIndex].Index);
            plotterDetail.FitToView();
            ShowInfo();
        }

        /// <summary>
        /// 显示实时的标定参数
        /// </summary>
        private void ShowInfo()
        {
            txtAmplitude.Content = "测量幅值(mV): " + CalInfo.Amplitude.ToString("F0");
            txtVelocity.Content = "波速(m/us): " + CalInfo.Velocity.ToString("F1");
            txtAttenuation.Content = "衰减系数: " + CalInfo.Attenuation.ToString("F3");
        }

        /// <summary>
        /// 显示衰减曲线
        /// </summary>
        private void ShowAtten(int index1, int index2)
        {
            CalInfo.Attenuation = Math.Log(_dataShow[index1] / _dataShow[index2]) * Params.SamRatePd / 1000 / (index2 - index1);
            if (!(CalInfo.Attenuation < double.MaxValue || CalInfo.Attenuation > double.MinValue))
            {
                lineGraphCali.DataSource = null;
                return;
            }
            CalInfo.Velocity = MeasureState.CableInfo.Length * 2 * Params.SamRatePd / 1000 / (index2 - index1);
            CalInfo.Index1 = index1;
            CalInfo.Index2 = index2;
            int sideLen = 20;    //两边的长度
            double[] attenX = new double[index2 - index1 + sideLen * 2];//一边20ns
            for (int i = 0; i < attenX.Length; i++)
            {
                attenX[i] = (index1 - sideLen + i) / Params.SamRatePd;
            }
            double[] attenY = new double[attenX.Length];
            for (int i = 0; i < attenY.Length; i++)
            {
                attenY[i] = _dataShow[index1] * Math.Exp(-CalInfo.Attenuation * (i - sideLen) / Params.SamRatePd * 1000);
            }
            DisplayHelper.StaticDisplay(lineGraphCali, attenX, 1, attenY, 1);
        }

        /// <summary>
        /// 选择反射脉冲（前一个）
        /// </summary>
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            currentIndex--;
            if (currentIndex <= 1)
            {
                btnUp.IsEnabled = false;
            }
            btnDown.IsEnabled = true;

            ShowAtten(_pulses[0].Index, _pulses[currentIndex].Index);
            ShowInfo();
        }

        /// <summary>
        /// 选择反射脉冲（后一个）
        /// </summary>
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            currentIndex++;
            if (currentIndex >= _pulses.Length - 1)
            {
                btnDown.IsEnabled = false;
            }
            btnUp.IsEnabled = true;

            ShowAtten(_pulses[0].Index, _pulses[currentIndex].Index);
            ShowInfo();
        }

        /// <summary>
        /// 取消标定
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            this.Close();
        }

        /// <summary>
        /// 确认标定
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CalInfo == null)
            {
                MessageBox.Show("无匹配脉冲信息", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CalInfo.Velocity < Properties.Settings.Default.MinCalibVelocity || CalInfo.Velocity > Properties.Settings.Default.MaxCalibVelocity)
            {
                MessageBox.Show("脉冲传播速度应在" + Properties.Settings.Default.MinCalibVelocity + "~" + Properties.Settings.Default.MaxCalibVelocity + "m/us之间", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (CalInfo.Attenuation > 5 || CalInfo.Attenuation < 0)//e^-5=0.6%
            {
                MessageBox.Show("衰减系数超出正常范围", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            IsOk = true;
            this.Close();
        }

        /// <summary>
        /// 分析按钮
        /// </summary>
        private void btnAnalyse_Click(object sender, RoutedEventArgs e)
        {
            Analyse();
        }

        /// <summary>
        /// 分析参数设置
        /// </summary>
        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            LocParamWin lpw = new LocParamWin(true);
            lpw.Owner = MainWindow.Instance;
            lpw.ShowDialog();
        }

        /// <summary>
        /// 窗口加载时，注册各种数据接收事件
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MeasureState.FinishData += FinishData;
            MeasureState.PrepareReceiveData += PrepareReceiveData;
            MeasureState.HvArrived += HvArrived;
            MeasureState.PdArrived += PdArrived;
            MeasureState.DisconnectEvent += new Action(DisconnectEvent);
        }

        /// <summary>
        /// 网络断链处理，关闭该窗口
        /// </summary>
        void DisconnectEvent()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                this.Close();
            }));
        }

        /// <summary>
        /// 窗口关闭时，注销各种数据接收事件
        /// </summary>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MeasureState.FinishData -= FinishData;
            MeasureState.PrepareReceiveData -= PrepareReceiveData;
            MeasureState.HvArrived -= HvArrived;
            MeasureState.PdArrived -= PdArrived;
            MeasureState.DisconnectEvent -= new Action(DisconnectEvent);
        }

        /// <summary>
        /// 接收数据完成时，处理界面显示，数据转换和保存任务
        /// </summary>
        private void FinishData()
        {
            //若采用50Hz标定输入，所以取30ms一定有一个脉冲，取10ms到40ms之间
            int beginMs = 1;     //起始 ms
            int lenMs = 30; //长度
            short[] calibData = new short[(int)(lenMs * Params.SamRatePd)];
            Array.Copy(pdReceiveBuffer, (int)(beginMs * Params.SamRatePd), calibData, 0, calibData.Length);
            //绝度值比较及反向
            //double caliMax = calibData.Max();
            //double caliMin = calibData.Min();
            //if (Math.Abs(caliMin) > Math.Abs(caliMax))
            //{
            //    calibData = calibData.Select(a => (short)-a).ToArray();//反向
            //}
            //去除偏移
            short avg = (short)calibData.Average(a => a);
            calibData = calibData.Select(a => (short)(a - avg)).ToArray();
            CalInfo = new CalibrationInfo();
            CalInfo.Discharge = _discharge;
            CalInfo.RangeIndex = rangeIndex;
            CalInfo.AllData = calibData;
            _dataAll = CalInfo.AllData.Select(a => a * Params.PD_Coeffi * Params.Range[rangeIndex]).ToArray();

            if (isNoise)
            {
                SaveNoise();//保存噪声
                isNoise = false;
            }
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                gridControl.IsEnabled = true;
                hint.Visibility = Visibility.Collapsed;
                ShowAll();
                if (_dragThresh == null)
                    InitThresh();
                else
                    _dragThresh.Position = new Point(10, 0);
                gridAnalyse.IsEnabled = true;
                //清空分析界面和值
                lineGraphDetail.DataSource = null;
                lineGraphCali.DataSource = null;
                //plotterDetail.Children.RemoveAll(typeof(TriangleHighlight));
                plotterDetail.Children.Remove(_thIn);
                plotterDetail.Children.Remove(_thOut);
                btnOk.IsEnabled = false;

                ShowInfo();
                checkBoxManual.IsChecked = false;

            }));
        }

        /// <summary>
        /// 保存噪声数据
        /// </summary>
        private void SaveNoise()
        {
            FileInfo noiseFile = new FileInfo(MeasureState.CableInfo.Path.FullName + "/noise.dat");
            NoiseInfo noiseInfo = new NoiseInfo();
            noiseInfo.RangeIndex = CalInfo.RangeIndex;
            noiseInfo.NoiseData = CalInfo.AllData;
            NoiseInfo.WriteFile(noiseInfo, noiseFile);
        }

        /// <summary>
        /// 因为硬件并未对标定做特殊处理，标定采用的数据接收流程和测试采集时一样，会接收高压数据。此时的高压数据接收后丢弃即可
        /// </summary>
        private void HvArrived()
        {
            byte[] b = MeasureState.TcpBinaryReader.ReadBytes(4096 * 2 * Params.CalibPack);
        }

        /// <summary>
        /// 接收标定数据
        /// </summary>
        private void PdArrived()
        {
            byte[] bb = MeasureState.TcpBinaryReader.ReadBytes(4096 * 2 * Params.CalibPack);
            for (int i = 0; i < 4096; i++)
            {
                pdReceiveBuffer[pdReceiveIndex] = BitConverter.ToInt16(bb, 2 * i);
                pdReceiveIndex++;
            }
        }

        /// <summary>
        /// 初始化Buffer和索引
        /// </summary>
        private void PrepareReceiveData()
        {
            pdReceiveIndex = 0;
            pdReceiveBuffer = new short[4096 * Params.CalibPack * 1000];
        }

        /// <summary>
        /// 点击标定按钮，发送标定命令
        /// </summary>
        private void btnCalib_Click(object sender, RoutedEventArgs e)
        {
            gridControl.IsEnabled = false;
            hint.Text = "正在标定，请稍候......";
            hint.Visibility = Visibility.Visible;
            new Thread(() =>
            {
                byte[] temp = new byte[18];
                Array.Copy(BitConverter.GetBytes(Cmd.CMD_STARTTEST), temp, 2);
                Array.Copy(BitConverter.GetBytes(200), 0, temp, 2, 4);//此参数无意义
                Array.Copy(BitConverter.GetBytes(0), 0, temp, 6, 4);//高压0V
                Array.Copy(BitConverter.GetBytes(1.0), 0, temp, 10, 4);
                Array.Copy(BitConverter.GetBytes(Params.CalibPack), 0, temp, 14, 4);//标定的数据量包数
                MeasureState.TcpBinaryWriter.Write(temp);
            }).Start();
        }

        /// <summary>
        /// 手动标定
        /// </summary>
        private void checkBoxManual_Checked(object sender, RoutedEventArgs e)
        {
            if (lineGraphDetail.DataSource == null || _thIn == null)
            {
                if (_dataAll == null)
                {
                    return;
                }
                GetShowData();
                plotterDetail.Children.RemoveAll(typeof(TriangleHighlight));
                double[] showX = new double[_dataShow.Length];
                for (int i = 0; i < showX.Length; i++)
                {
                    showX[i] = i / Params.SamRatePd;//换算为ms
                }
                DisplayHelper.StaticDisplay(lineGraphDetail, showX, 1, _dataShow, 1);

                //找最大值
                double max = 0;
                int maxIndex = 0;
                for (int i = 0; i < _dataShow.Length; i++)
                {
                    if (max < _dataShow[i])
                    {
                        max = _dataShow[i];
                        maxIndex = i;
                    }
                }
                int outIndex = maxIndex + 100;//0.8us，68m

                _thIn = new TriangleHighlight(max > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(maxIndex / Params.SamRatePd, max), 10);
                _thIn.Fill = Brushes.Green;
                _thIn.StrokeThickness = 0;
                plotterDetail.Children.Add(_thIn);
                _thOut = new TriangleHighlight(_dataShow[outIndex] > 0 ? TriangleFixPosition.Down : TriangleFixPosition.Up, new Point(outIndex / Params.SamRatePd, _dataShow[outIndex]), 10);
                _thOut.Fill = Brushes.Red;
                _thOut.StrokeThickness = 0;
                plotterDetail.Children.Add(_thOut);
            }

            _dragPoint1.Position = new Point(_thIn.FixPoint.X, 0);
            _dragPoint2.Position = new Point(_thOut.FixPoint.X, 0);


            _dragPoint1.Visibility = Visibility.Visible;
            _dragPoint2.Visibility = Visibility.Visible;
            _vl1.Visibility = Visibility.Visible;
            _vl2.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 取消手动标定
        /// </summary>
        private void checkBoxManual_Unchecked(object sender, RoutedEventArgs e)
        {
            _dragPoint1.Visibility = Visibility.Collapsed;
            _dragPoint2.Visibility = Visibility.Collapsed;
            _vl1.Visibility = Visibility.Collapsed;
            _vl2.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 采集噪声
        /// </summary>
        private void btnNoise_Click(object sender, RoutedEventArgs e)
        {
            isNoise = true;
            hint.Text = "正在采集噪声...";
            hint.Visibility = Visibility.Visible;

            new Thread(() =>
            {
                byte[] temp = new byte[18];
                Array.Copy(BitConverter.GetBytes(Cmd.CMD_STARTTEST), temp, 2);
                Array.Copy(BitConverter.GetBytes(200), 0, temp, 2, 4);//固定以200Hz
                Array.Copy(BitConverter.GetBytes(0), 0, temp, 6, 4);
                Array.Copy(BitConverter.GetBytes(1.0), 0, temp, 10, 4);
                Array.Copy(BitConverter.GetBytes(Params.CalibPack), 0, temp, 14, 4);//标定的数据量包数
                MeasureState.TcpBinaryWriter.Write(temp);
            }).Start();
        }
    }
}
