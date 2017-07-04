using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathWorks.MATLAB.NET.Arrays;


namespace Resonance
{
    /// <summary>
    /// 算法类（周期计算，拟合。小波和带宽降噪弃用）
    /// </summary>
    class Algorithm
    {
        /// <summary>
        /// 小波和拟合相关库
        /// </summary>
        private static OWProcess.OWClass OWInstance;

        /// <summary>
        /// 带宽滤波，FFT相关库
        /// </summary>
        private static BandpassFilter.FilterClass FilterClass;

        /// <summary>
        /// 应用程序需要先调用此方法，初始化matlab库
        /// </summary>
        public static void InitAlgorithm()
        {
            FilterClass = new BandpassFilter.FilterClass();
            OWInstance = new OWProcess.OWClass();
        }

        /// <summary>
        /// 简单计算正弦波周期，从最高点开始
        /// </summary>
        /// <param name="hvData">高压数据</param>
        /// <param name="fre">高压频率</param>
        /// <param name="cap">电缆电容</param>
        /// <param name="peak">高压最大幅值</param>
        /// <param name="indexs">每个周期起点的在数组中索引</param>
        public static void GetSinParam(short[] hvData, out double fre, out double cap, out double peak, out int[] indexs)
        {
            int[] temp;
            GetSinParam2(hvData, out  fre, out  cap, out  peak, out temp);
            try
            {
                indexs = new int[temp.Length];//丢掉后面四分之一周期
                for (int i = 1; i < indexs.Length; i++)
                {
                    indexs[i] = (temp[i] - temp[i - 1]) * 3 / 4 + temp[i - 1];
                }
                //平滑处理，衰减余弦
                //short max = hvData[1]; //data[0]为0，data[1]才是最大
                //short min2 = hvData[temp[0] * 2]; //相位π时的最小值
                //for (int i = 0; i < temp[0]; i++)
                //{
                //    double atten = Math.Exp(Math.Log(1.0 * -min2 / max) / temp[0] / 2 * i);//衰减
                //    double arc = (i * 1.0 / temp[0]) * (Math.Acos(hvData[temp[0]] * 1.0 / max) - Math.PI / 2);//角度偏移 
                //    hvData[i] = (short)(atten * max * Math.Cos(2 * Math.PI * i / 4 / temp[0] + arc));
                //}
            }
            catch (Exception)
            {
                indexs = new int[] { 0 };
                return;
            }

        }

        /// <summary>
        /// 简单计算正弦波周期，0相位开始
        /// </summary>
        /// <param name="hvData">高压数据</param>
        /// <param name="fre">高压频率</param>
        /// <param name="cap">电缆电容</param>
        /// <param name="peak">高压最大幅值</param>
        /// <param name="indexs">每个周期起点的在数组中索引</param>
        public static void GetSinParam2(short[] hvData, out double fre, out double cap, out double peak, out int[] indexs)
        {
            //150us前平滑预处理，去掉开关动作
            for (int i = 12; i < 20; i++)
            {
                hvData[i] = (short)(hvData[12] + (hvData[20] - hvData[12]) * ((i - 12) / 8));
            }

            //去掉偏移
            short avg = (short)hvData.Average(a => (double)a);
            for (int i = 0; i < hvData.Length; i++)
            {
                hvData[i] -= avg;
            }

            try
            {
                //double L = 0.75;//电感
                //提取出处于y=0并处下降沿的点，即相位为cos π/2点的x坐标
                int maxPeriod = 8;//采集最多9点，8个周期
                int[] indexHalfPi = new int[maxPeriod + 1];
                int k = 0;
                for (int i = 1; i < hvData.Length; i++)
                {
                    if (hvData[i] - hvData[i - 1] < 0 && hvData[i] * hvData[i - 1] <= 0)
                    {
                        indexHalfPi[k] = i;
                        //往前进3/4个周期
                        i += indexHalfPi[0] * 3 / 4;
                        k++;//k表示下一次要找的点
                        if (k > maxPeriod)
                        {
                            //k目前的值表示找到了几个点，即k-1周期
                            break;
                        }
                    }
                }
                k--;
                fre = 1000 / ((indexHalfPi[k] - indexHalfPi[0]) * 1.0 / k / Params.SamRateHv);
                //f=1/[2π √(LC)]  C=1/((2pi*f)^2*L)
                cap = 1000000000 / ((2 * Math.PI * fre) * (2 * Math.PI * fre) * Params.L);
                peak = hvData.Max() * Params.HV_Coeffi;
                indexs = new int[k + 1];
                for (int i = 0; i < k + 1; i++)
                {
                    indexs[i] = indexHalfPi[i];
                }
            }
            catch (Exception)
            {
                fre = 0;
                cap = 0;
                peak = 0;
                int hvseg = hvData.Length / 8;
                indexs = new int[] { 0, hvseg, hvseg * 2 };
            }

        }

        /// <summary>
        /// 拟合高压，计算参数（弃用）
        /// </summary>
        /// <param name="hvData"></param>
        /// <param name="frequency"></param>
        /// <param name="capacity"></param>
        /// <param name="diel"></param>
        /// <param name="damp"></param>
        /// <param name="periodCount"></param>
        /// <param name="periodIndexs"></param>
        public static void FitHVParam(double[] hvData, out double frequency, out double capacity, out double diel, out double damp, out int periodCount, out int[] periodIndexs)
        {
            //求严格π开始
            double startIndex = 0;

            //提取出处于y=0并处于上升沿的点，即相位为π/2点的x坐标
            int maxPeriod = 10;//最多11点，10个周期
            int[] indexHalfPi = new int[maxPeriod + 1];
            int k = 0;
            for (int i = (int)startIndex + 1; i < hvData.Length; i++)
            {
                if (hvData[i] - hvData[i - 1] < 0 && hvData[i] * hvData[i - 1] <= 0 && hvData[i] != 0)
                {
                    indexHalfPi[k] = i;
                    k++;
                    if (k >= maxPeriod + 1)
                    {
                        //k目前的值表示找到了几个点，即k-1周期
                        break;
                    }
                }
            }
            periodCount = k - 1;
            periodIndexs = new int[k];
            Array.Copy(indexHalfPi, periodIndexs, k);

            //用采样率除以一个周期的采样点，得到信号的频率。将此值作为信号拟合的初始频率
            double ff = Params.SamRateHv * 1000 * (k - 1) / (indexHalfPi[k - 1] - indexHalfPi[0]);

            //抽取1000点
            int pointNum = 2000;
            double[] x = new double[pointNum];
            double[] y = new double[pointNum];
            double startTime = (int)startIndex / Params.SamRateHv / 1000;
            for (int i = 0; i < pointNum; i++)
            {
                int index = (int)startIndex + (hvData.Length - (int)startIndex) / pointNum * i;
                x[i] = index / Params.SamRateHv / 1000 - startTime;
                y[i] = hvData[index];
            }

            MWNumericArray xArray = x;
            MWNumericArray yArray = y;
            //这里有问题，hvData.Max()并不是U0
            MWArray[] results = Algorithm.OWInstance.OWPE(3, Params.R, Params.L, hvData.Max(), ff, xArray, yArray);

            //振荡波频率，电容，介损
            frequency = ((double[,])((MWNumericArray)results[0]).ToArray(MWArrayComponent.Real))[0, 0];
            capacity = ((double[,])((MWNumericArray)results[1]).ToArray(MWArrayComponent.Real))[0, 0];
            diel = ((double[,])((MWNumericArray)results[2]).ToArray(MWArrayComponent.Real))[0, 0];
            damp = 0.5 * (diel * Math.PI * 2 * frequency + Params.R / Params.L);
        }

        /// <summary>
        /// 小波降噪（弃用）{'sym8','db2','db3','db4','db5','db6','db7','db8','db9','db10'};
        /// </summary>
        /// <param name="originalData"></param>
        /// <param name="layer"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static double[] Denoise(double[] originalData, int layer, int type)
        {
            //降噪后
            MWNumericArray xxArray = originalData;
            MWArray results = Algorithm.OWInstance.OWden(xxArray, layer, type);

            MWNumericArray denoisedArray = results.ToArray();
            double[,] denoised = (double[,])denoisedArray.ToArray();
            int len = originalData.Length;
            double[] denoisedData = new double[len];
            for (int i = 0; i < len; i++)
            {
                denoisedData[i] = denoised[0, i];
            }
            return denoisedData;
        }

        /// <summary>
        /// FFT
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ffty"></param>
        /// <param name="fftx"></param>
        public static void FFT(double[] data, out double[] ffty, out double[] fftx)
        {
            MWNumericArray xArray = data;
            MWArray[] results = FilterClass.DoFFT(2, xArray, Params.SamRatePd * 1000);
            ffty = Transform(((double[,])((MWNumericArray)results[0]).ToArray(MWArrayComponent.Real)), 1);
            fftx = Transform(((double[,])((MWNumericArray)results[1]).ToArray(MWArrayComponent.Real)), 1);
        }

        /// <summary>
        /// 和matlab的数据类型转换
        /// </summary>
        /// <param name="x"></param>
        /// <param name="dimen"></param>
        /// <returns></returns>
        private static double[] Transform(double[,] x, int dimen)
        {
            int dimension = x.GetUpperBound(dimen) + 1;

            double[] xOut = new double[dimension];
            for (int i = 0; i < dimension; i++)
            {
                if (dimen == 1)
                    xOut[i] = x[0, i];
                else
                    xOut[i] = x[i, 0];
            }
            return xOut;
        }
    }
}
