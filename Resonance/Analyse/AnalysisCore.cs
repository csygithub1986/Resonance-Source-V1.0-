using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resonance
{
    /// <summary>
    /// 脉冲筛选类
    /// </summary>
    public class AnalysisCore
    {
        /// <summary>
        /// 递归查找与横坐标交叉的上升沿
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">当前index</param>
        /// <param name="thresh">阈值</param>
        /// <returns></returns>
        private static double GetBehandData(double[] data, int index, double thresh)
        {
            if (index >= data.Length)
            {
                return thresh;
            }
            if (data[index] == thresh)
            {
                return GetBehandData(data, index + 1, thresh);
            }
            return data[index];
        }

        /// <summary>
        ///  核心函数，筛选峰值，以及其坐标
        ///  正、负脉冲分别匹配。且需满足一定范围的脉宽
        /// </summary>
        /// <param name="data">波形数据</param>
        /// <param name="_locThresh">阈值</param>
        /// <param name="pulses">输出脉冲</param>
        public static void FilterPeaks(double[] data, double _locThresh, out Pulse[] pulses)
        {
            #region 正
            List<int> indexBegin = new List<int>();//前端点坐标
            List<int> indexEnd = new List<int>();//后端点坐标
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i - 1] < _locThresh && (data[i] > _locThresh || GetBehandData(data, i, _locThresh) > _locThresh))
                {
                    indexBegin.Add(i);
                }
            }
            int indstart = 1;
            if (indexBegin.Count != 0)
            {
                indstart = indexBegin[0] + 1;
            }
            for (int i = indstart; i < data.Length; i++)
            {
                if (data[i - 1] > _locThresh && (data[i] < _locThresh || GetBehandData(data, i, _locThresh) < _locThresh))
                {
                    indexEnd.Add(i);
                }
            }
            //一种特殊情况，会没有结尾点
            if (indexBegin.Count > indexEnd.Count)
            {
                indexBegin.RemoveAt(indexBegin.Count - 1);
            }
            int peakCount = indexBegin.Count;

            // 计算开始、结束及峰值点
            int[] peakPosPlus = new int[peakCount];//位置
            double[] peakValuePlus = new double[peakCount];//幅值
            double[] phasePlus = new double[peakCount];
            for (int i = 0; i < peakCount; i++)
            {
                peakPosPlus[i] = indexBegin[i];
                peakValuePlus[i] = data[indexBegin[i]];
                for (int j = indexBegin[i] + 1; j <= indexEnd[i]; j++)//找峰值
                {
                    if (data[j] > peakValuePlus[i])
                    {
                        peakPosPlus[i] = j;
                        peakValuePlus[i] = data[j];
                    }
                }

                phasePlus[i] = (360.0 * Params.RetainPeriod * ((double)peakPosPlus[i] / data.Length)) % 360;
                if (phasePlus[i]<270)
                {
                    phasePlus[i] += 90;
                }
                else
                {
                    phasePlus[i] -= 270;
                }
            }
            #endregion

            #region 负
            List<int> indexBegin2 = new List<int>();//前端点坐标
            List<int> indexEnd2 = new List<int>();//后端点坐标
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i - 1] > -_locThresh && (data[i] < -_locThresh || GetBehandData(data, i, -_locThresh) < -_locThresh))
                {
                    indexBegin2.Add(i);
                }
            }
            int indstart2 = 1;
            if (indexBegin2.Count != 0)
            {
                indstart2 = indexBegin2[0] + 0;
            }
            for (int i = indstart2; i < data.Length; i++)
            {
                if (data[i - 1] < -_locThresh && (data[i] > -_locThresh || GetBehandData(data, i, -_locThresh) > -_locThresh))
                {
                    indexEnd2.Add(i);
                }
            }
            //一种特殊情况，会没有结尾点
            if (indexBegin2.Count > indexEnd2.Count)
            {
                indexBegin2.RemoveAt(indexBegin2.Count - 1);
            }
            int peakCount2 = indexBegin2.Count;

            // 计算开始、结束及峰值点
            int[] peakPosMinus = new int[peakCount2];//位置
            double[] peakValueMinus = new double[peakCount2];//幅值
            double[] phaseMinus = new double[peakCount2];
            for (int i = 0; i < peakCount2; i++)
            {
                peakPosMinus[i] = indexBegin2[i];
                peakValueMinus[i] = data[indexBegin2[i]];
                for (int j = indexBegin2[i] + 1; j <= indexEnd2[i]; j++)//找峰值
                {
                    if (data[j] < peakValueMinus[i])
                    {
                        peakPosMinus[i] = j;
                        peakValueMinus[i] = data[j];
                    }
                }
                phaseMinus[i] = (360.0 * Params.RetainPeriod * ((double)peakPosMinus[i] / data.Length)) % 360;
                if (phaseMinus[i] < 270)
                {
                    phaseMinus[i] += 90;
                }
                else
                {
                    phaseMinus[i] -= 270;
                }
            }
            #endregion

            double microsecond = 2 / 1000 * Params.SamRatePd;//2000ns内的点数

            HashSet<int> passPlus = new HashSet<int>();
            HashSet<int> passMinus = new HashSet<int>();
            for (int i = 0; i < peakPosPlus.Length; i++)
            {
                for (int j = 0; j < peakPosMinus.Length; j++)
                {
                    double dis = peakPosPlus[i] - peakPosMinus[j];
                    if (peakPosPlus[i] - peakPosMinus[j] < microsecond && peakPosMinus[j] - peakPosPlus[i] < microsecond)
                    {
                        if (peakPosPlus[i] > -peakPosMinus[j])
                            passMinus.Add(j);
                        else
                            passPlus.Add(i);
                    }
                }
            }

            List<Pulse> pulseList = new List<Pulse>();
            for (int i = 0; i < peakPosPlus.Length; i++)
            {
                if (!passPlus.Contains(i))
                {
                    double t = T(data, peakPosPlus[i]);
                    if (t <= Params.AF_TCondition)
                    {
                        pulseList.Add(new Pulse()
                        {
                            Index = peakPosPlus[i],
                            Amplitude = peakValuePlus[i],
                            Phase = phasePlus[i],
                            T = t,
                            //F = F(data, peakPosPlus[i])
                        });
                    }
                }
            }
            for (int j = 0; j < peakPosMinus.Length; j++)
            {
                if (!passMinus.Contains(j))
                {
                    double t = T(data, peakPosMinus[j]);
                    if (t <= Params.AF_TCondition)
                    {
                        pulseList.Add(new Pulse()
                        {
                            Index = peakPosMinus[j],
                            Amplitude = peakValueMinus[j],
                            Phase = phaseMinus[j],
                            T = t,
                            //F = F(data, peakPosMinus[j])
                        });
                    }
                }
            }
            pulses = pulseList.ToArray();
        }

        /// <summary>
        /// 计算等效脉宽
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">脉冲定点所在的坐标</param>
        /// <returns>脉宽</returns>
        public static double T(double[] data, int index)
        {
            //选取长度L为1us
            double L = Params.AF_L / 1000;//ms
            int pointCount = (int)(L * Params.SamRatePd);
            int half = pointCount / 2;
            if (index - half < 0 || index + half > data.Length)
            {
                return 1;
            }
            double all = 0; //归一化系数
            for (int i = index - half; i < index + half; i++)
            {
                all += data[i] * data[i];
            }
            //时间重心  求不求能有多大区别
            //double index0 = 0;
            //for (int i = index - half; i < index + half; i++)
            //{
            //    index0 += i * data[i] * data[i];
            //}
            //index0 /= all;
            double index0 = index;         //就以index为index0


            //等效脉宽
            double T = 0;
            for (int i = index - half; i < index + half; i++)
            {
                T += (i - index0) * (i - index0) * data[i] * data[i];
            }
            T /= all;
            T = Math.Sqrt(T) * 1000 / Params.SamRatePd;//us
            return T;
        }


        /// <summary>
        /// 计算等效带宽（未用）
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="index">脉冲所在坐标</param>
        /// <returns>带宽</returns>
        public static double F(double[] data, int index)
        {
            //选取长度L为1us
            double L = Params.AF_L / 1000;//ms
            int pointCount = (int)(L * Params.SamRatePd);
            int half = pointCount / 2;
            if (index - half < 0 || index + half > data.Length)
            {
                return 0;
            }

            double[] fftdata = new double[2 * half];
            for (int i = 0; i < fftdata.Length; i++)
            {
                fftdata[i] = data[index - half + i];
            }
            double[] ffty;
            double[] fftx;
            Algorithm.FFT(fftdata, out ffty, out fftx);

            double all = 0; //归一化系数
            for (int i = 0; i < ffty.Length; i++)
            {
                all += ffty[i] * ffty[i];
            }
            //频率重心  
            double f0 = 0;
            for (int i = 0; i < ffty.Length; i++)
            {
                f0 += fftx[i] * ffty[i] * ffty[i];
            }
            f0 /= all;

            //等效带宽
            double F = 0;
            for (int i = 0; i < ffty.Length; i++)
            {
                F += (fftx[i] - f0) * (fftx[i] - f0) * ffty[i] * ffty[i];
            }
            F /= all;
            return Math.Sqrt(F);
        }

        /// <summary>
        /// 求脉冲数量，正+负
        /// </summary>
        /// <returns></returns>
        public static int PulseCount(double[] data, double thresh)
        {
            int count = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if ((data[i] > thresh && data[i - 1] <= thresh) || (data[i] < thresh && data[i - 1] >= thresh))
                {
                    count++;
                }
            }
            return count;
        }

        //前20个阈值（弃用）
        public static double AutoThresh(double[] data)
        {
            if (Params.GlobleThresh!=0)
            {
                return Params.GlobleThresh;
            }
            //认为放电信号为2us，即250点中取一个最大值
            int scale = 250;
            int index = 0;
            List<double> list = new List<double>();
            //double total = 0;
            double max = 0;
            for (int i = 0; i < data.Length; i++)
            {
                index++;

                if (max < Math.Abs(data[i]))
                {
                    max = Math.Abs(data[i]);
                }
                if (index == scale)
                {
                    //total += max;
                    index = 0;
                    list.Add(max);
                    max = 0;
                }
            }
            list.Sort();
            if (list.Count > 20)
            {
                return list[list.Count - 20];
            }
            return list[0];
        }
    }
}
