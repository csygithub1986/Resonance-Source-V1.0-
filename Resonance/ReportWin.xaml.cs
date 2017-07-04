using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Tools;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

using Microsoft.Research.DynamicDataDisplay.Charts;

namespace Resonance
{
    /// <summary>
    /// 生成报告窗口
    /// </summary>
    public partial class ReportWin : Window
    {
        public ReportWin()
        {
            InitializeComponent();
            prpPlotters = new ChartPlotter[] { plotterPrp1, plotterPrp2, plotterPrp3 };
            prpLine = new LineGraph[] { lineGraphPrp1, lineGraphPrp2, lineGraphPrp3 };

            Display(AnalyseState.Instance.Prp);
            DisplayHV();
        }

        ChartPlotter[] prpPlotters;
        LineGraph[] prpLine;

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            FileInfo file = FileTools.SaveFile("Report" + DateTime.Now.ToString("yyyyMMdd HHmmss"), "pdf");
            if (file == null)
            {
                return;
            }
            CreatePDF(file);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 创建PDF报表
        /// </summary>
        /// <param name="fileinfo">文件名</param>
        /// <returns></returns>
        private bool CreatePDF(FileInfo fileinfo)
        {
            Document doc = new Document(); //iTextSharp document
            try
            {
                PdfWriter pwa = PdfWriter.GetInstance(doc, fileinfo.Create());
                BaseFont songti = BaseFont.CreateFont(@"C:\Windows\Fonts\STSONG.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font zw = new Font(songti, 12);
                Font boldZw = new Font(songti, 12, Font.BOLD);
                doc.Open();
                //标题
                Paragraph p = new Paragraph("电缆局放测试报告", new Font(songti, 20, Font.BOLD));
                p.Alignment = (int)AlignmentX.Center;
                doc.Add(p);

                p = new Paragraph(30);//行距30
                p.Add(new Chunk("变电站：", boldZw));
                p.Add(new Chunk(MeasureState.CableInfo.Station, zw));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk("电缆长度：", boldZw));
                p.Add(new Chunk(MeasureState.CableInfo.Length.ToString() + " m", zw));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk("额定电压：", boldZw));
                p.Add(new Chunk(MeasureState.CableInfo.U0.ToString() + " kV", zw));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk("测试时间：", boldZw));
                p.Add(new Chunk(MeasureState.CableInfo.Date.ToString("yyyy年MM月dd日"), zw));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk("备注信息：", boldZw));
                p.Add(new Chunk(MeasureState.CableInfo.Comment, zw));
                doc.Add(p);

                //////////////////////
                p = new Paragraph(30);
                p.Add(new Chunk("测试概况：", new Font(songti, 14, Font.BOLD)));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk("A相：", boldZw));
                doc.Add(p);

                iTextSharp.text.Image info1 = iTextSharp.text.Image.GetInstance(GetImageBytes(OverviewPage._this.grid1));
                info1.ScaleToFit(522f, 522f);
                doc.Add(info1);


                p = new Paragraph();
                p.Add(new Chunk("B相：", boldZw));
                doc.Add(p);

                iTextSharp.text.Image info2 = iTextSharp.text.Image.GetInstance(GetImageBytes(OverviewPage._this.grid2));
                info2.ScaleToFit(522f, 522f);
                doc.Add(info2);

                p = new Paragraph();
                p.Add(new Chunk("C相：", boldZw));
                doc.Add(p);

                iTextSharp.text.Image info3 = iTextSharp.text.Image.GetInstance(GetImageBytes(OverviewPage._this.grid3));
                info3.ScaleToFit(522f, 522f);
                doc.Add(info3);

                doc.NewPage();

                p = new Paragraph(30);
                p.Add(new Chunk("定位图：", new Font(songti, 14, Font.BOLD)));
                doc.Add(p);

                iTextSharp.text.Image mapImage = iTextSharp.text.Image.GetInstance(GetImageBytes(MapPage._this.plotterMapAll));
                mapImage.ScaleToFit(400f, 400f);
                mapImage.Alignment = 1;
                doc.Add(mapImage);

                p = new Paragraph(30);
                p.Add(new Chunk("PRPD谱图", new Font(songti, 14, Font.BOLD)));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk("A相：", boldZw));
                doc.Add(p);

                iTextSharp.text.Image prp1 = iTextSharp.text.Image.GetInstance(GetImageBytes(borderPrp1));
                prp1.ScaleToFit(300f, 300f);
                prp1.Alignment = 1;
                doc.Add(prp1);

                p = new Paragraph();
                p.Add(new Chunk("B相：", boldZw));
                doc.Add(p);

                iTextSharp.text.Image prp2 = iTextSharp.text.Image.GetInstance(GetImageBytes(borderPrp2));
                prp2.ScaleToFit(300f, 300f);
                prp2.Alignment = 1;
                doc.Add(prp2);

                p = new Paragraph();
                p.Add(new Chunk("C相：", boldZw));
                doc.Add(p);

                iTextSharp.text.Image prp3 = iTextSharp.text.Image.GetInstance(GetImageBytes(borderPrp3));
                prp3.ScaleToFit(300f, 300f);
                prp3.Alignment = 1;
                doc.Add(prp3);

                p = new Paragraph(30);
                p.Add(new Chunk("测试结果：", new Font(songti, 14, Font.BOLD)));
                doc.Add(p);

                p = new Paragraph();
                p.Add(new Chunk(txtResult.Text, boldZw));
                doc.Add(p);

                p = new Paragraph(30);
                p.Add(new Chunk("分析及建议", new Font(songti, 14, Font.BOLD)));
                doc.Add(p);
                p = new Paragraph();
                p.Add(new Chunk(txtSuggestion.Text, boldZw));
                doc.Add(p);

                MessageBox.Show("导出报表完成", "消息", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                doc.Close();
            }
        }

        /// <summary>
        /// 获得控件的图像数据
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        private byte[] GetImageBytes(FrameworkElement ele)
        {
            int dpiRate = 2;
            MemoryStream ms = new MemoryStream();
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(ele.ActualWidth * dpiRate) + 1, (int)(ele.ActualHeight * dpiRate) + 1, 96 * dpiRate, 96 * dpiRate, PixelFormats.Pbgra32);
            bmp.Render(ele);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(ms);
            return ms.GetBuffer();
        }

        /// <summary>
        /// 显示需要报表显示的图形。（为了可以控制图像的大小，在固定大小的报表窗口重新绘制，并且隐藏起来）
        /// </summary>
        /// <param name="prpPairs">PRP数据</param>
        private void Display(List<Point>[] prpPairs)
        {
            for (int i = 0; i < 3; i++)
            {
                List<Point> prpPair = prpPairs[i];
                prpPlotters[i].Children.RemoveAll(typeof(RectangleHighlight));
                if (prpPair.Count==0)
                {
                    continue;
                }
                foreach (var item in prpPair)
                {
                    RectangleHighlight rh = new RectangleHighlight(FixPosition.Center, item, 2, 2);
                    //rh.Tag = pulsePair;
                    //rh.ToolTip = "放电量： " + pulsePair.Amplitude + "\n位置  ： " + pulsePair.Distance;
                    Color temp = brushes[i];
                    temp.A = 200;
                    rh.Fill = new SolidColorBrush(temp);
                    rh.StrokeThickness = 0;
                    //rh.Stroke = new SolidColorBrush(temp);// new SolidColorBrush(brushes[phase - 1]);
                    prpPlotters[i].Children.Add(rh);
                }
                double max = prpPair.Max(a => Math.Abs(a.Y));
                prpPlotters[i].Visible = new Rect(0, -max * 1.1, 360, 2.2 * max);
            }

        }
        Color[] brushes = new Color[] { Params.Color1, Params.Color2, Params.Color3 };

        /// <summary>
        /// 画参考电压
        /// </summary>
        void DisplayHV()
        {
            for (int ii = 0; ii < 3; ii++)
            {
                double[] xx = new double[361];
                double[] yy = new double[361];
                for (int i = 0; i < 361; i++)
                {
                    xx[i] = i;
                    yy[i] = prpPlotters[ii].Viewport.Visible.Height * 0.9 / 2 * Math.Sin(i / 360.0 * 2 * Math.PI);// +
                    // plotterPrp.Viewport.Visible.Height / 2+plotterPrp.Viewport.Visible.Y;
                }
                CompositeDataSource cds = new CompositeDataSource();
                EnumerableDataSource<double> baseXData = new EnumerableDataSource<double>(xx);
                EnumerableDataSource<double> baseYData = new EnumerableDataSource<double>(yy);
                baseXData.SetXMapping(x => x);//(必须)
                baseYData.SetYMapping(y => y);
                cds = baseXData.Join(baseYData);
                prpLine[ii].DataSource = cds;
                //prpPlotters[ii].FitToView();
                //prpPlotters[ii].Viewport.Visible = new Rect(-10, prpPlotters[ii].Viewport.Visible.Y,
                //   380, prpPlotters[ii].Viewport.Visible.Height);
            }
        }

    }
}
