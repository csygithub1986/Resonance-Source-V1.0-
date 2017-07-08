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
using Microsoft.Research.DynamicDataDisplay.Common;


namespace Resonance
{
    /// <summary>
    /// 全局测试信息总览页面
    /// </summary>
    public partial class OverviewPage : Page
    {
        public OverviewPage()
        {
            _this = this;
            InitializeComponent();
            grid1.SetValue(Panel.ZIndexProperty, 1);
        }

        public static OverviewPage _this;

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void Page_Initialized(object sender, EventArgs e)
        {
            ChartPlotter[] plos = { plotterQ1, plotterQ2, plotterQ3, plotterVol1, plotterVol2, plotterVol3 };
            foreach (var item in plos)
            {
                item.Children.Remove(item.MouseNavigation);
            }

            CableInfo cableInfo = MeasureState.CableInfo;
            ShowCableInfo(cableInfo);

            //接头
            List<JointStruct> jList = new List<JointStruct>();
            for (int i = 0; i < cableInfo.Joints.Count - 1; i++)
            {
                jList.Add(new JointStruct() { Position = cableInfo.Joints[i] + "" });
                jList.Add(new JointStruct() { Length = cableInfo.Joints[i + 1] - cableInfo.Joints[i] + "" });
            }
            jList.Add(new JointStruct() { Position = cableInfo.Length + "" });
            listViewJoint.DataContext = jList;
            //图表
            DisplayVol();
            DisplayQ();
        }

        /// <summary>
        /// （弃用）
        /// </summary>
        private void DisplayParam()
        {
            //TextBlock[] txtFre = { txtF1, txtF2, txtF3 };
            //TextBlock[] txtCap = { txtCap1, txtCap2, txtCap3 };
            //TextBlock[] txtTamp = { txtTamp1, txtTamp2, txtTamp3 };
            //TextBlock[] txtTan = { txtTan1, txtTan2, txtTan3 };
            //for (int i = 0; i < 3; i++)
            //{
            //    txtFre[i].Text=MeasureState.CableInfo.Freqs[i].ToString("F1")+" Hz"
            //    txtCap[i].Text=AnalyseState.Instance.CalibrationInfos[i].ToString("F1")+" Hz"
            //    txtTamp[i].Text=AnalyseState.Instance.CalibrationInfos[i].Attenuation.ToString("F3");
            //    txtTan[i].Text=MeasureState.CableInfo.Freqs[i].ToString("F1")+" Hz"
            //}
        }

        /// <summary>
        /// 显示测试的电压等级序列
        /// </summary>
        private void DisplayVol()
        {
            FileInfo[] files = AnalyseState.Instance.Path.GetFiles("*.zdb");
            List<double>[] hvList = new List<double>[3] { new List<double>(), new List<double>(), new List<double>() };
            string p = "ABC";
            foreach (var item in files)
            {
                string name = item.Name;
                int index = p.IndexOf(name.Substring(0, 1));
                string hvVol = name.Substring(1, 3);
                hvList[index].Add(double.Parse(hvVol));
            }
            ChartPlotter[] plotterVol = { plotterVol1, plotterVol2, plotterVol3 };
            for (int i = 0; i < 3; i++)
            {
                if (hvList[i].Count < 1)
                    continue;
                hvList[i].Sort();
                for (int j = 0; j < hvList[i].Count; j++)
                {
                    RectangleHighlight rh = new RectangleHighlight();
                    rh.Bounds = new Rect(j + 0.6, 0, 0.8, hvList[i][j]);
                    rh.Fill = Params.Brushes[i];
                    rh.StrokeThickness = 0;
                    rh.ToolTip = hvList[i][j] + "U0";
                    plotterVol[i].Children.Add(rh);
                }
                plotterVol[i].Viewport.Visible = new DataRect(0, 0, hvList[i].Count + 1, hvList[i][hvList[i].Count - 1] * 1.1);
            }
        }

        /// <summary>
        /// 按电压等级排序，显示放电量趋势
        /// </summary>
        private void DisplayQ()
        {
            ChartPlotter[] plotterQ = { plotterQ1, plotterQ2, plotterQ3 };
            LineGraph[] lineGraphQ = { lineGraphQ1, lineGraphQ2, lineGraphQ3 };
            string p = "ABC";
            Dictionary<double, List<double>>[] dic = new Dictionary<double, List<double>>[3];
            for (int i = 0; i < 3; i++)
            {
                dic[i] = new Dictionary<double, List<double>>();
            }
            foreach (var item in AnalyseState.Instance.DataInfos)
            {
                int index = p.IndexOf(item.Key.Substring(0, 1));//ABC
                string hvVol = item.Key.Substring(1, 3);
                double v = double.Parse(hvVol);
                if (dic[index].Keys.Contains(v))
                {
                    dic[index][v].Add(item.Value.MaxPd);
                }
                else
                {
                    dic[index].Add(v, new List<double>() { item.Value.MaxPd });
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (dic[i].Count < 1)
                {
                    continue;
                }
                lineGraphQ[i].Stroke = Params.Brushes[i];
                dic[i] = dic[i].OrderBy(a => a.Key).ToDictionary(a => a.Key, b => b.Value);
                double[] x = dic[i].Keys.Select(a => a).ToArray();
                double[] y = dic[i].Values.Select(a => a.Average() * Params.HV_Coeffi * Params.mVTopC[i]).ToArray();
                DisplayHelper.StaticDisplay(lineGraphQ[i], x, 1, y, 1);

                foreach (var item in dic[i])
                {
                    RectangleHighlight rh = new RectangleHighlight();
                    double max = item.Value.Max() * Params.HV_Coeffi * Params.mVTopC[i];
                    double min = item.Value.Min() * Params.HV_Coeffi * Params.mVTopC[i];
                    double avg = item.Value.Average() * Params.HV_Coeffi * Params.mVTopC[i];
                    rh.Bounds = new Rect(item.Key - 0.02, min, 0.04, max - min);
                    rh.Fill = Params.Brushes[3];
                    rh.Stroke = Params.Brushes[3];
                    rh.StrokeThickness = 0.5;
                    rh.Opacity = 0.8;
                    rh.ToolTip = "最大：" + max + "\r\n平均：" + avg + "\r\n最小：" + min;
                    plotterQ[i].Children.Add(rh);
                }
                double top = dic[i].Values.Select(a => a.Max()).Max() * Params.HV_Coeffi * Params.mVTopC[i];
                double bottom = dic[i].Values.Select(a => a.Min()).Min() * Params.HV_Coeffi * Params.mVTopC[i];
                double height = top - bottom;
                double left = plotterQ[i].Viewport.Visible.X;
                double width = plotterQ[i].Viewport.Visible.Width;
                plotterQ[i].Viewport.Visible = new DataRect(left, bottom - height / 20, width, height * 1.1);
            }

        }

        /// <summary>
        /// 显示测试信息和电缆信息
        /// </summary>
        /// <param name="cableInfo">电缆信息</param>
        private void ShowCableInfo(CableInfo cableInfo)
        {
            Paragraph prg = new Paragraph();
            AddLine(prg, "测试日期：\t", cableInfo.Date.ToString("yyyy年MM月dd日"));
            AddLine(prg, "变电站：\t", cableInfo.Station);
            AddLine(prg, "电缆长度：  \t", cableInfo.Length + "");
            AddLine(prg, "U0：\t", cableInfo.U0 + "");
            AddLine(prg, "备注：\t", cableInfo.Comment);
            richText1.Document.Blocks.Clear();
            richText1.Document.Blocks.Add(prg);
        }

        /// <summary>
        /// 添加字符段落
        /// </summary>
        private void AddLine(Paragraph prg, string header, string data)
        {
            Run r1 = new Run(header) { FontSize = 14, FontWeight = FontWeights.Bold };
            Run r2 = new Run(data) { FontSize = 14 };
            prg.Inlines.Add(r1);
            prg.Inlines.Add(r2);
            prg.Inlines.Add(new LineBreak());
        }

        /// <summary>
        /// 切换相序
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = tabControl.SelectedIndex;
            //ChartPlotter[] plotterVol = { plotterVol1, plotterVol2, plotterVol3 };
            //ChartPlotter[] plotterQ = { plotterQ1, plotterQ2, plotterQ3 };
            //LineGraph[] lineGraphTan = { lineGraphTan1, lineGraphTan2, lineGraphTan3 };
            Grid[] grids = new Grid[] { grid1, grid2, grid3 };
            for (int i = 0; i < 3; i++)
            {
                //Visibility visi = index == i ? Visibility.Visible : Visibility.Hidden;
                //plotterVol[i].Visibility = visi;
                //plotterQ[i].Visibility = visi;
                //lineGraphTan[i].Visibility = visi;
                grids[i].SetValue(Panel.ZIndexProperty, i == index ? 1 : 0);
            }
        }
    }

    /// <summary>
    /// 电缆接头结构体
    /// </summary>
    public class JointStruct
    {
        public string Position { get; set; }
        public string Length { get; set; }
    }
}
