using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace Resonance
{
    /// <summary>
    /// 波形显示处理类，主要是进行大数据绘制时的一些提取加速工作
    /// </summary>
    public class DisplayHelper
    {
        public const int DISPLAY_PIXEL = 3000;//数据抽取数
        public const int EXTRACT_THRESH = 7000;//多少点以后开始抽取

        public static int count = 0;

        public static void DynamicDisplay(ChartPlotter plotter, LineGraph lineGraph, double[] data, double xRate, double yRate, bool showAll)
        {
            count++;
            long min = 0;
            long max = data.Length;
            if (showAll == false)
            {
                //时间与点坐标转换
                min = (long)Math.Floor(plotter.Viewport.Visible.X / xRate);
                max = (long)Math.Ceiling(plotter.Viewport.Visible.X / xRate + plotter.Viewport.Visible.Width / xRate);
            }

            //约束边界
            if (max < 0)
            {
                return;
            }
            if (min < 0)
            {
                min = 0;
            }
            if (min > data.Length - 1)
            {
                return;
            }
            if (max > data.Length - 1)
            {
                max = data.Length - 1;
            }

            double[] dataY;
            double[] dataX;

            if (max - min < EXTRACT_THRESH)
            {
                dataY = new double[max - min + 1];
                dataX = new double[max - min + 1];
                for (int i = 0; i < max - min + 1; i++)
                {
                    dataY[i] = data[min + i] * yRate;
                    dataX[i] = (min + i) * xRate;
                }
                EnumerableDataSource<double> dataSourceX;
                EnumerableDataSource<double> dataSourceY;
                dataSourceX = dataX.AsDataSource();
                dataSourceY = dataY.AsDataSource();
                CompositeDataSource compositeDataSource = new CompositeDataSource(dataSourceX, dataSourceY);
                dataSourceX.SetXMapping(x => x);
                dataSourceY.SetYMapping(y => y);
                lineGraph.DataSource = compositeDataSource;
                dataSourceY.RaiseDataChanged();
            }
            else
            {
                ShowMaxMin(min, max, lineGraph, data, xRate, yRate);
            }
        }

        /// <summary>
        /// 极大极小值抽取显示HV，如果索引差大于1万，则抽取5000点
        /// <param name="min">真实数据的最小索引</param>
        /// <param name="max">真实数据的最大索引</param>
        /// </summary>
        private static void ShowMaxMin(long minIndex, long maxIndex, LineGraph lineGraph, double[] data, double xRate, double yRate)
        {
            int pixel = DISPLAY_PIXEL * 2;
            double lastAverage = 0;

            double[] dataX = new double[pixel];
            double[] dataY = new double[pixel];

            long dataLength = maxIndex - minIndex + 1;
            double divide = (double)dataLength / DISPLAY_PIXEL;//平均这么多个点抽取一个
            for (long i = 0; i < pixel; i++)//这里用long，否则会溢出
            {
                //x从真实的索引转换到显示的时间
                if (i % 2 == 0)
                {
                    dataX[i] = (double)(minIndex * xRate) + (i / 2) * divide * xRate;
                    dataX[i + 1] = dataX[i];
                    double[] temp = new double[(int)divide];
                    //也许(int)((double)i / 2 * divide不是特别严谨
                    Array.Copy(data, minIndex + (int)((double)i / 2 * divide), temp, 0, (int)divide);
                    double max = temp.Max();
                    double min = temp.Min();
                    double average = (max + min) / 2;
                    if (lastAverage > average)
                    {
                        dataY[i] = max * yRate;
                        dataY[i + 1] = min * yRate;
                    }
                    else
                    {
                        dataY[i] = min * yRate;
                        dataY[i + 1] = max * yRate;
                    }
                    lastAverage = average;
                }
            }

            EnumerableDataSource<double> dataSourceX;
            EnumerableDataSource<double> dataSourceY;
            dataSourceX = dataX.AsDataSource();
            dataSourceY = dataY.AsDataSource();
            CompositeDataSource compositeDataSource = new CompositeDataSource(dataSourceX, dataSourceY);
            dataSourceX.SetXMapping(x => x);
            dataSourceY.SetYMapping(y => y);
            lineGraph.DataSource = compositeDataSource;
            dataSourceY.RaiseDataChanged();//不加此句，滚轮缩小时，波形会不更新完全
        }

        public static void StaticDisplay(LineGraph lineGraph, IEnumerable<double> xData,double xrate, IEnumerable<double> yData,double yrate)
        {
            EnumerableDataSource<double> hvDataSourceX = xData.AsXDataSource();
            EnumerableDataSource<double> hvDataSourceY = yData.AsYDataSource();
            hvDataSourceX.SetXMapping(x => x*xrate);
            hvDataSourceY.SetYMapping(y => y*yrate);
            CompositeDataSource hvCompositeDataSource = new CompositeDataSource(hvDataSourceX, hvDataSourceY);
            lineGraph.DataSource = hvCompositeDataSource;
            hvDataSourceY.RaiseDataChanged();
        }


        public static void DisplayMarker(MarkerPointsGraph mpg, IEnumerable<double> xData, IEnumerable<double> yData)
        {
            EnumerableDataSource<double> hvDataSourceX = xData.AsXDataSource();
            EnumerableDataSource<double> hvDataSourceY = yData.AsYDataSource();
            hvDataSourceX.SetXMapping(x => x);
            hvDataSourceY.SetYMapping(y => y);
            //hvDataSourceX.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
            //    Y => String.Format("幅值： {0}", Y));
            //hvDataSourceY.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
            //    Y => String.Format("幅值： {0}", Y));
            CompositeDataSource hvCompositeDataSource = new CompositeDataSource(hvDataSourceX, hvDataSourceY);
            mpg.DataSource = hvCompositeDataSource;
            hvDataSourceY.RaiseDataChanged();
        }

    }
}
