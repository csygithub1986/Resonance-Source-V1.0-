using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.Collections;
using Resonance.Properties;

namespace Resonance
{
    public partial class MeasurePage : Page
    {
        DataInfo _dataInfo;

        /// <summary>
        /// 经过截取后的高压数据
        /// </summary>
        short[] hvValidData;

        /// <summary>
        /// 经过截取后的局放数据
        /// </summary>
        short[] pdValidData;

        /// <summary>
        /// 经过系数转换后的高压数据，用于显示
        /// </summary>
        double[] hvShowData;

        /// <summary>
        /// 经过系数转换后的局放数据，用于显示
        /// </summary>
        double[] pdShowData;

        /// <summary>
        /// 高压数据Buffer
        /// </summary>
        short[] hvReceiveBuffer;

        /// <summary>
        /// 局放数据Buffer
        /// </summary>
        short[] pdReceiveBuffer;
        int hvReceiveIndex = 0;
        int pdReceiveIndex = 0;

        int rangeIndex;

        /// <summary>
        /// 测试的电压等级
        /// </summary>
        double[] vo;// = { 0.3, 0.5, 0.7, 0.9, 1.0, 1.1, 1.3, 1.5, 1.7, 2.0 };

        /// <summary>
        /// 测试相序
        /// </summary>
        string[] phaseStr = { "A", "B", "C" };

        /// <summary>
        /// 脉宽计算系数
        /// </summary>
        double pulseWidthPara = 1;

        /// <summary>
        /// 是否自动测试
        /// </summary>
        bool isAuto;

        /// <summary>
        /// 扫描窗口
        /// </summary>
        SweepWin _sweepWin;

        /// <summary>
        /// 下发的高压电压1.0, 1.1等
        /// </summary>
        double volLevel;
        bool needReadFile = true;//如果鼠标动作，为true，如果代码选中，则为false

        int _phase;

        /// <summary>
        /// 高压最大可接收的数据量
        /// </summary>
        private int _hvMaxCount;
        private int _pdMaxCount;
        private int _hvMaxPack;
        private int _pdMaxPack;

        public MeasurePage(int phase)
        {
            _phase = phase;
            InitializeComponent();
            Init();
            AddFiles();
        }

        /// <summary>
        /// 加载以前测试过的文件
        /// </summary>
        private void AddFiles()
        {
            FileInfo[] fis = MeasureState.CableInfo.Path.GetFiles(phaseStr[_phase] + "*.zdb");
            //文件列表
            foreach (var fi in fis)
            {
                ListBoxItem lbitem = new ListBoxItem();
                lbitem.Tag = fi;
                lbitem.Content = fi.Name.Substring(0, fi.Name.Length - 4);
                lbitem.Selected += new RoutedEventHandler(FileItem_Selected);
                lbFile.Items.Add(lbitem);
            }
        }

        /// <summary>
        /// 扫描完成，接收扫描频率
        /// </summary>
        private void FreArrived()
        {
            if (timer != null)
            {
                timer.Dispose();
            }

            MeasureState.CableInfo.Freqs[_phase] = MeasureState.TcpBinaryReader.ReadSingle();
            pulseWidthPara = MeasureState.TcpBinaryReader.ReadInt32() * Params.HV_Coeffi;
            EnableUI(true, btnSweep);
            //计算电容
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                progressBar.Visibility = Visibility.Hidden;
                if (_sweepWin != null && _sweepWin.IsActive)
                {
                    _sweepWin.Close();
                }
            }));
            PrintPrompt("谐振频率 " + MeasureState.CableInfo.Freqs[_phase] + "Hz");
            CableInfo.WriteFile(MeasureState.CableInfo, new FileInfo(MeasureState.CableInfo.Path + "/start.info"));//扫频完成后保存保存
        }

        /// <summary>
        /// 初始化接收缓冲
        /// </summary>
        private void PrepareReceiveData()
        {
            hvReceiveIndex = 0;
            pdReceiveIndex = 0;
            hvReceiveBuffer = new short[_hvMaxCount];
            pdReceiveBuffer = new short[_pdMaxCount];
            //hasLoadedData = false;
            showProgress(true, _hvMaxPack + _pdMaxPack);
            increseProgress(0);
        }

        /// <summary>
        /// 完成所有数据的接收后，计算参数、显示、保存
        /// </summary>
        private void FinishData()
        {
            PrintPrompt("接收完成");
            //hasLoadedData = true;
            showProgress(false, 1);
            //拟合计算
            double fre;
            double cap;
            double hvPeak;
            int[] indexs;
            Algorithm.GetSinParam(hvReceiveBuffer, out fre, out cap, out hvPeak, out indexs);
            if (fre > 1000 || fre < 30 || indexs.Length < 8)
            {
                MessageBox.Show("高压没有正常运行");
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    hvShowData = hvReceiveBuffer.Select(a => a * Params.HV_Coeffi).ToArray();
                    pdShowData = pdReceiveBuffer.Select(a => a * Params.PD_Coeffi).ToArray();
                    DisplayHelper.DynamicDisplay(chartPlotter1, lineGraph1, hvShowData, 1.0 / Params.SamRateHv, 1, false);
                    DisplayHelper.DynamicDisplay(chartPlotter2, lineGraph2, pdShowData, 1.0 / Params.SamRatePd, Params.mVTopC[_phase], false);
                }));
                UIMeasuring(false);
                return;
            }

            //打包数据与保存
            _dataInfo = new DataInfo();

            int multi = (int)(Params.SamRatePd / Params.SamRateHv);
            hvValidData = new short[indexs[indexs.Length - 1] - indexs[0]];
            pdValidData = new short[hvValidData.Length * multi];
            try
            {
                for (int i = 0; i < hvValidData.Length; i++)
                {
                    hvValidData[i] = hvReceiveBuffer[indexs[0] + i];
                }
                int pdstart = indexs[0] * multi;
                //25000个点，150us数据去掉
                for (int i = 25000; i < pdValidData.Length; i++)
                {
                    pdValidData[i] = pdReceiveBuffer[pdstart + i];
                }
                _dataInfo.Indexs = new int[indexs.Length];
                for (int i = 0; i < indexs.Length; i++)
                {
                    _dataInfo.Indexs[i] = indexs[i] - indexs[0];
                }
            }
            catch (Exception)
            {
                MessageBox.Show("数据不正确，处理出错");
                UIMeasuring(false);
                return;
            }

            double maxPd = pdValidData.Max() * Params.PD_Coeffi * Params.Range[rangeIndex];
            _dataInfo.Phase = _phase;
            _dataInfo.VoltageLevel = volLevel;
            _dataInfo.MaxPd = maxPd;
            _dataInfo.RangeIndex = rangeIndex;
            _dataInfo.Fre = fre;
            DateTime testTime = DateTime.Now;
            _dataInfo.TestDate = testTime;
            _dataInfo.Cap = cap;

            FileInfo fi = new FileInfo(MeasureState.CableInfo.Path.FullName + "\\" + phaseStr[_phase] + volLevel.ToString("F1") + " " + testTime.ToString("HH-mm-ss") + ".zdb");
            DataInfo.Save(fi, _dataInfo, hvValidData, pdValidData);
            //UI
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                //文件列表
                ListBoxItem lbitem = new ListBoxItem();
                lbitem.Tag = fi;
                lbitem.Content = fi.Name.Substring(0, fi.Name.Length - 4);
                lbitem.Selected += new RoutedEventHandler(FileItem_Selected);
                lbFile.Items.Add(lbitem);
                needReadFile = false;
                lbitem.IsSelected = true;//选中该文件

                ShowInfo(hvPeak);
            }));
            UIMeasuring(false);
            //是否自动测试
            if (isAuto)
            {
                AutoMeasure();
            }
        }

        /// <summary>
        /// 选中某个测试文件，显示该文件波形
        /// </summary>
        void FileItem_Selected(object sender, RoutedEventArgs e)
        {
            if (needReadFile)
            {
                ListBoxItem item = sender as ListBoxItem;
                FileInfo fi = item.Tag as FileInfo;
                _dataInfo = DataInfo.Read(fi, out hvValidData, out pdValidData);
            }
            needReadFile = true;
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
        /// 显示各种参数
        /// </summary>
        /// <param name="actualVol">实测的高压幅值</param>
        private void ShowInfo(double actualVol)
        {
            txtPhase.Content = phaseStr[_dataInfo.Phase];
            txtCap.Content = _dataInfo.Cap.ToString("F1") + " nF";
            txtFre.Content = _dataInfo.Fre.ToString("F1") + " Hz";
            txtHvVol.Content = actualVol.ToString("F1") + " kV";
            txtTestDate.Content = _dataInfo.TestDate.ToString("HH:mm");
            txtMaxPd.Content = (_dataInfo.MaxPd * Params.mVTopC[_dataInfo.Phase]).ToString("F1") + " " + Params.UnitChar;
        }

        /// <summary>
        /// 计算波形数据的周期数，并构造周期选择控件
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
        /// 显示高压和局放波形
        /// </summary>
        /// <param name="p">相序</param>
        void ShowWave(int p)
        {
            int multi = (int)(Params.SamRatePd / Params.SamRateHv);
            if (p == 0)
            {
                hvShowData = new double[_dataInfo.Indexs[_dataInfo.Indexs.Length - 1]];
                p = 1;
            }
            else
                hvShowData = new double[_dataInfo.Indexs[p] - _dataInfo.Indexs[p - 1]];
            pdShowData = new double[hvShowData.Length * multi];
            for (int i = 0; i < hvShowData.Length; i++)
            {
                hvShowData[i] = hvValidData[_dataInfo.Indexs[p - 1] + i] * Params.HV_Coeffi;
            }
            int pdstart = _dataInfo.Indexs[p - 1] * multi;
            for (int i = 0; i < pdShowData.Length; i++)
            {
                pdShowData[i] = pdValidData[pdstart + i] * Params.PD_Coeffi * Params.Range[_dataInfo.RangeIndex];
            }

            lineGraph2.Stroke = Params.Brushes[_dataInfo.Phase];
            DisplayHelper.DynamicDisplay(chartPlotter1, lineGraph1, hvShowData, 1.0 / Params.SamRateHv, 1, false);
            DisplayHelper.DynamicDisplay(chartPlotter2, lineGraph2, pdShowData, 1.0 / Params.SamRatePd, Params.mVTopC[_phase], false);

            //double pdmax = pdShowData.Max() * 1.1;
            //if (pdmax < 10)
            //pdmax = 10;
            double len = hvShowData.Length / Params.SamRateHv;
            chartPlotter1.Visible = new Rect(0, -_dataInfo.VoltageLevel * MeasureState.CableInfo.Upp * 1.1, len, 2 * _dataInfo.VoltageLevel * MeasureState.CableInfo.Upp * 1.1);
            //chartPlotter2.Visible = new Rect(0, -pdmax, len, 2 * pdmax);
            chartPlotter2.Visible = new Rect(0, -Params.Range[_dataInfo.RangeIndex] * Params.mVTopC[_phase] * 1000, len, 2 * Params.Range[_dataInfo.RangeIndex] * Params.mVTopC[_phase] * 1000);
        }

        /// <summary>
        /// 单独查看某个周期
        /// </summary>
        void lbPeriodItem_Selected(object sender, RoutedEventArgs e)
        {
            int p = lbPeriod.Items.IndexOf(sender);// lbPeriod.SelectedIndex; //(int)((ListBoxItem)sender).Content;
            ShowWave(p);
        }

        /// <summary>
        /// 接收高压数据
        /// </summary>
        private void HvArrived()
        {
            byte[] b = MeasureState.TcpBinaryReader.ReadBytes(4096 * 2);
            if (hvReceiveIndex >= hvReceiveBuffer.Length)
            {
                return;
            }
            for (int i = 0; i < 4096; i++)
            {
                hvReceiveBuffer[hvReceiveIndex] = BitConverter.ToInt16(b, i * 2);
                hvReceiveIndex++;
            }
            increseProgress(1);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                PrintPrompt("接收数据 " + (progressBar.Value / progressBar.Maximum * 100).ToString("F1") + "%");
            }));
        }

        /// <summary>
        /// 接收局放数据
        /// </summary>
        private void PdArrived()
        {
            byte[] bb = MeasureState.TcpBinaryReader.ReadBytes(4096 * 2);
            for (int i = 0; i < 4096; i++)
            {
                pdReceiveBuffer[pdReceiveIndex] = BitConverter.ToInt16(bb, i * 2);
                pdReceiveIndex++;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                PrintPrompt("接收数据 " + (progressBar.Value / progressBar.Maximum * 100).ToString("F1") + "%");
            }));
            increseProgress(1);
        }

        /// <summary>
        /// 异步打印用户提示
        /// </summary>
        /// <param name="msg">需要打印的消息</param>
        private void PrintPrompt(string msg)
        {
            //异步方式
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                textPrompt.Content = msg;
            }));
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void Init()
        {
            txtPhaseInfo.Content = "测试数据" + "ABC".Substring(_phase, 1) + "相";
            //控制面板
            EnableUI(false, gridCtr, lbFile, gridDisplay, btnSweep);

            //进度条
            progressBar.Visibility = Visibility.Hidden;

            //波形图
            new PlotterWrap().Wrap(chartPlotter1, MainWindow.Instance);
            new PlotterWrap().Wrap(chartPlotter2, MainWindow.Instance);
            chartPlotter1.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(plotter1_PropertyChanged);
            chartPlotter2.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(plotter2_PropertyChanged);
            chartPlotter1.Viewport.Visible = new DataRect(0, -20, 50, 40);
            chartPlotter2.Viewport.Visible = new DataRect(0, -1000, 50, 2000);

            //电压等级
            vo = Settings.Default.VoltageList.Cast<string>().Select(p => double.Parse(p)).ToArray();
            foreach (var item in vo)
            {
                cbVoltage.Items.Add(item + " U0");
            }
            cbVoltage.SelectedIndex = 0;
            volLevel = 1;

            //量程
            cbRange.Items.Add("100 mV");
            cbRange.Items.Add("200 mV");
            cbRange.Items.Add("1 V");
            cbRange.Items.Add("10 V");
            cbRange.SelectionChanged += cbRange_SelectionChanged;
            cbRange.SelectedIndex = 2;
        }

        int p1;
        /// <summary>
        /// 高压波形缩放时动态绘制事件
        /// </summary>
        void plotter1_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                System.Diagnostics.Debug.WriteLine("p1更改" + p1++);
                if (hvShowData == null)
                {
                    return;
                }
                DisplayHelper.DynamicDisplay(chartPlotter1, lineGraph1, hvShowData, 1.0 / Params.SamRateHv, 1.0, false);
                chartPlotter2.Viewport.Visible = new DataRect(chartPlotter1.Viewport.Visible.X,
                    chartPlotter2.Viewport.Visible.Y, chartPlotter1.Viewport.Visible.Width, chartPlotter2.Viewport.Visible.Height);
            }
        }
        /// <summary>
        /// 局放波形缩放时动态绘制事件
        /// </summary>
        void plotter2_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visible")
            {
                if (pdShowData == null)
                {
                    return;
                }
                DisplayHelper.DynamicDisplay(chartPlotter2, lineGraph2, pdShowData, 1.0 / Params.SamRatePd, Params.mVTopC[_phase], false);
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
        /// 接收数据时进度条增加
        /// </summary>
        /// <param name="value">增加的值</param>
        private void increseProgress(int value)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                progressBar.Value = progressBar.Value + value;
            }));
        }

        /// <summary>
        /// 显示或隐藏进度条
        /// </summary>
        /// <param name="visible">为true显示进度条，反之隐藏</param>
        /// <param name="max">进度条的最大值</param>
        private void showProgress(bool visible, double max)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                progressBar.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
                progressBar.Value = 0;
                progressBar.Maximum = max;
            }));
        }

        /// <summary>
        /// 扫频按钮事件，发送扫频命令
        /// </summary>
        private void btnSweep_Click(object sender, RoutedEventArgs e)
        {
            btnSweep.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Maximum = 58;//42+16秒
            PrintPrompt("正在扫描 0%");
            progressBar.Value = 0;

            if (Properties.Settings.Default.EnableSweepWindow)
            {
                _sweepWin = new SweepWin();
                _sweepWin.Owner = MainWindow.Instance;
                _sweepWin.Show();
            }

            timer = new Timer((a) =>
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    progressBar.Value += 0.5;
                    PrintPrompt("正在扫描 " + (progressBar.Value / progressBar.Maximum * 100).ToString("F0") + "%");
                    if (progressBar.Value >= progressBar.Maximum)
                    {
                        timer.Dispose();
                    }
                }));
            }, null, 500, 500);
            new Thread(() =>
            {
                try
                {
                    MeasureState.TcpBinaryWriter.Write(Cmd.CMD_SWEEPFREQ);
                }
                catch (Exception)
                {
                    PrintPrompt("网络连接失败");
                    UIafterDisconnect();
                    return;
                }
            }).Start();
        }

        /// <summary>
        /// 向自动测试列表中添加测试相
        /// </summary>
        private void btnAddinAuto_Click(object sender, RoutedEventArgs e)
        {
            Label item = new Label();
            item.Width = 30;
            item.BorderBrush = Brushes.White;
            item.BorderThickness = new Thickness(1);
            item.Foreground = Brushes.White;
            item.Content = vo[cbVoltage.SelectedIndex];
            item.MouseEnter += new MouseEventHandler(item_MouseEnter);
            item.MouseLeave += new MouseEventHandler(item_MouseLeave);

            item.Background = Params.Brushes[_phase];
            autoPanel.Children.Add(item);
        }

        Label deleteLabel;//欲删除的lable

        /// <summary>
        /// 自动测试相鼠标进入事件，颜色变化
        /// </summary>
        void item_MouseEnter(object sender, MouseEventArgs e)
        {
            Label lable = sender as Label;
            lable.BorderBrush = Params.Brush4;
            deleteLabel = lable;
        }

        /// <summary>
        /// 自动测试相鼠标移出事件，颜色变化
        /// </summary>
        void item_MouseLeave(object sender, MouseEventArgs e)
        {
            Label lable = sender as Label;
            lable.BorderBrush = Brushes.White;
        }

        /// <summary>
        /// 自动测试按钮事件
        /// </summary>
        private void btnAutoMeasure_Click(object sender, RoutedEventArgs e)
        {
            if (autoPanel.Children.Count == 0)
            {
                MessageBox.Show("无测试序列", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            isAuto = true;
            Label lable = autoPanel.Children[0] as Label;
            volLevel = (double)lable.Content;
            StartMeasure();
        }

        /// <summary>
        /// 自动测试按钮事件
        /// </summary>
        private void AutoMeasure()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            { //删除上一次
                autoPanel.Children.RemoveAt(0);
                if (autoPanel.Children.Count == 0)
                {
                    isAuto = false;
                    MessageBox.Show("测试完毕", "消息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                Label lable = autoPanel.Children[0] as Label;
                volLevel = (double)lable.Content;
                StartMeasure();
            }));
        }

        /// <summary>
        /// 开始测试按钮事件
        /// </summary>
        private void StartMeasure()
        {
            UIMeasuring(true);
            if (MeasureState.CableInfo.Freqs[_phase] == 0)
            {
                MessageBox.Show("请先对此相电缆进行谐振扫描", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                UIMeasuring(false);
                return;
            }
            new Thread(() =>
            {
                int xishu = (int)(volLevel * MeasureState.CableInfo.Upp / pulseWidthPara * 0.25);
                if (xishu < 1)
                {
                    xishu = 1;
                }
                //算出需要多少包数据
                int pack = (int)(8 / MeasureState.CableInfo.Freqs[_phase] / 4096 * Params.SamRateHv * 1000 + 1);
                _hvMaxPack = pack;
                _pdMaxPack = pack * Params.Multi;
                _hvMaxCount = _hvMaxPack * 4096;
                _pdMaxCount = _pdMaxPack * 4096;


                byte[] temp = new byte[18];
                Array.Copy(BitConverter.GetBytes(Cmd.CMD_STARTTEST), temp, 2);
                Array.Copy(BitConverter.GetBytes(MeasureState.CableInfo.Freqs[_phase]), 0, temp, 2, 4);
                Array.Copy(BitConverter.GetBytes((int)(volLevel * MeasureState.CableInfo.Upp / Params.HV_Coeffi)), 0, temp, 6, 4);
                Array.Copy(BitConverter.GetBytes(xishu), 0, temp, 10, 4);
                Array.Copy(BitConverter.GetBytes(pack), 0, temp, 14, 4);//下发包数
                MeasureState.TcpBinaryWriter.Write(temp);
            }).Start();
        }

        /// <summary>
        /// 删除自动测试相
        /// </summary>
        private void Menu_Del(object sender, RoutedEventArgs e)
        {
            autoPanel.Children.Remove(deleteLabel);
        }

        /// <summary>
        /// 使能或禁用某个控件
        /// </summary>
        /// <param name="enable">为true时该控件可用，否则禁用</param>
        /// <param name="element">控件名称</param>
        private void EnableUI(bool enable, params UIElement[] element)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                foreach (var item in element)
                {
                    item.IsEnabled = enable;
                }
            }));
        }

        /// <summary>
        /// 断链后UI处理
        /// </summary>
        private void UIafterDisconnect()
        {
            EnableUI(false, btnSweep, gridCtr);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                progressBar.Visibility = Visibility.Hidden;
            }));
        }

        /// <summary>
        /// 开始测试按钮事件
        /// </summary>
        private void btnMeasure_Click(object sender, RoutedEventArgs e)
        {
            volLevel = vo[cbVoltage.SelectedIndex];
            StartMeasure();
        }

        /// <summary>
        /// 开始测试后/测试结束后界面的控件禁用处理
        /// </summary>
        /// <param name="measuring">为true表示正在测试，false为测试结束</param>
        private void UIMeasuring(bool measuring)
        {
            EnableUI(measuring);
            EnableUI(!measuring, gridCtr, lbFile, gridDisplay);
        }

        /// <summary>
        /// 删除测试文件
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
        /// 清空波形显示
        /// </summary>
        private void ClearPlotter()
        {
            lineGraph1.DataSource = null;
            lineGraph2.DataSource = null;
            lbPeriod.Items.Clear();
        }

        /// <summary>
        /// 打开设备按钮事件
        /// </summary>
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            EnableUI(false, btnOpen, btnBack);
            new Thread(() =>
            {
                MeasureState.TcpBinaryWriter.Write(Cmd.CMD_OPENDEVICE);
            }).Start();
            timer = new Timer((a) =>
            {
                EnableUI(true, gridCtr, lbFile, gridDisplay, btnClose, btnSweep);
            }, null, 2000, Timeout.Infinite);
            MainWindow.Instance.CloseWindowEvent += CloseDevice;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        private void CloseDevice()
        {
            try
            {
                MeasureState.TcpBinaryWriter.Write(Cmd.CMD_CLOSEDEVICE);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 因为设备打开和关闭需要数秒时间，此定时器用以延迟界面控件的使能
        /// </summary>
        Timer timer;

        /// <summary>
        /// 关闭设备按钮事件
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            EnableUI(false, gridCtr, lbFile, gridDisplay, btnClose, btnSweep);
            EnableUI(true, btnBack);
            if (timer != null)
            {
                timer.Dispose();
                progressBar.Visibility = Visibility.Hidden;//如果扫频中，关机小时进度条。
                PrintPrompt("关机");
            }
            new Thread(() =>
            {
                MeasureState.TcpBinaryWriter.Write(Cmd.CMD_CLOSEDEVICE);
            }).Start();
            timer = new Timer((a) =>
            {
                EnableUI(true, btnOpen);
                timer.Dispose();
            }, null, 2000, Timeout.Infinite);
            MainWindow.Instance.CloseWindowEvent -= CloseDevice;
        }

        /// <summary>
        /// 退出程序菜单
        /// </summary>
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.Close();
        }

        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 页面初始化时注册数据接收事件
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MeasureState.FinishData += FinishData;
            MeasureState.FreArrived += FreArrived;
            MeasureState.PrepareReceiveData += PrepareReceiveData;
            MeasureState.HvArrived += HvArrived;
            MeasureState.PdArrived += PdArrived;
            MeasureState.WarningArrived += WarningArrived;
            MeasureState.SweepDataArrived += SweepDataArrived;
            MeasureState.DisconnectEvent += DisconnectEvent;
        }

        /// <summary>
        /// 界面注销时注销数据接收事件
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MeasureState.FinishData -= FinishData;
            MeasureState.FreArrived -= FreArrived;
            MeasureState.PrepareReceiveData -= PrepareReceiveData;
            MeasureState.HvArrived -= HvArrived;
            MeasureState.PdArrived -= PdArrived;
            MeasureState.WarningArrived -= WarningArrived;
            MeasureState.DisconnectEvent -= new Action(DisconnectEvent);
        }

        /// <summary>
        /// 断链处理，如果断链返回WlanPage
        /// </summary>
        void DisconnectEvent()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                WlanPage wp = new WlanPage();
                NavigationService.Navigate(wp);
            }));
        }

        /// <summary>
        /// 系统超压、过流等警告，弹出警告并关机
        /// </summary>
        void WarningArrived()
        {
            //关机
            btnClose_Click(null, null);
            MessageBox.Show("超压警告！请检查系统", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 谐振扫描的过程数据接收
        /// </summary>
        void SweepDataArrived()
        {
            //频率float，幅值float
            float fre = MeasureState.TcpBinaryReader.ReadSingle();
            float value = MeasureState.TcpBinaryReader.ReadSingle();
            if (_sweepWin != null && _sweepWin.IsActive)
            {
                _sweepWin.AddData(new Point(fre, value));
            }
        }

        /// <summary>
        /// 定位菜单，进行粗略定位
        /// </summary>
        private void menuLocate_Click(object sender, RoutedEventArgs e)
        {
            LocationInMeaWin lmw = new LocationInMeaWin(_phase);
            lmw.Owner = MainWindow.Instance;
            lmw.ShowDialog();
        }

        /// <summary>
        /// 结束一相测试，返回标定界面
        /// </summary>
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            CaliPage cp = new CaliPage();
            NavigationService.Navigate(cp);
        }

        /// <summary>
        /// 切换量程，发送切换命令
        /// </summary>
        void cbRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rangeIndex = cbRange.SelectedIndex;
            new Thread(() =>
            {
                byte[] temp = new byte[4];
                Array.Copy(BitConverter.GetBytes(Cmd.CMD_SWITCHRANGE), temp, 2);
                Array.Copy(BitConverter.GetBytes((short)(rangeIndex + 1)), 0, temp, 2, 2);
                MeasureState.TcpBinaryWriter.Write(temp);
            }).Start();
        }
    }
}
