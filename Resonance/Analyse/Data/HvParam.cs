using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 高压波形的参数，目前频率和电容有用，阻尼系数和Tanδ弃用
    /// </summary>
    public class HvParam
    {
        /// <summary>
        /// 频率
        /// </summary>
        public Dictionary<double, double>[] Frequency { get; set; }

        /// <summary>
        /// 电容
        /// </summary>
        public Dictionary<double, double>[] Capacity { get; set; }
        public Dictionary<double, double>[] DampCoefficient { get; set; }
        public Dictionary<double, double>[] Tanδ { get; set; }

        public HvParam()
        {
            Frequency = new Dictionary<double, double>[3];
            Capacity = new Dictionary<double, double>[3];
            DampCoefficient = new Dictionary<double, double>[3];
            Tanδ = new Dictionary<double, double>[3];
            for (int i = 0; i < 3; i++)
            {
                Frequency[i] = new Dictionary<double, double>();
                Capacity[i] = new Dictionary<double, double>();
                DampCoefficient[i] = new Dictionary<double, double>();
                Tanδ[i] = new Dictionary<double, double>();
            }
        }

        // 通过获取值，求得是平均值
        public static HvParam GetHvParam()
        {
            HvParam hvParam = new HvParam();
            string phases = "ABC";
            Dictionary<double, int>[] countDic = new Dictionary<double, int>[3];
            for (int i = 0; i < 3; i++)
            {
                countDic[i] = new Dictionary<double, int>();
            }
            foreach (var file in AnalyseState.Instance.DataInfos)
            {
                int line = phases.IndexOf(file.Key.Substring(0, 1));

                double vol = file.Value.VoltageLevel * MeasureState.CableInfo.U0;

                #region 求平均，很麻烦的
                if (hvParam.Frequency[line].ContainsKey(vol) == false)
                {
                    hvParam.Frequency[line].Add(vol, file.Value.Fre);
                    hvParam.Capacity[line].Add(vol, file.Value.Cap);
                    hvParam.DampCoefficient[line].Add(vol, file.Value.Damp);
                    hvParam.Tanδ[line].Add(vol, file.Value.Tanδ);
                    countDic[line].Add(vol, 1);
                }
                else
                {
                    int count = countDic[line][vol];
                    countDic[line].Remove(vol);
                    countDic[line].Add(vol, count);

                    double fr = hvParam.Frequency[line][vol];
                    double ca = hvParam.Capacity[line][vol];
                    double da = hvParam.DampCoefficient[line][vol];
                    double ta = hvParam.Tanδ[line][vol];

                    hvParam.Frequency[line].Remove(vol);
                    hvParam.Capacity[line].Remove(vol);
                    hvParam.DampCoefficient[line].Remove(vol);
                    hvParam.Tanδ[line].Remove(vol);

                    hvParam.Frequency[line].Add(vol, fr);
                    hvParam.Capacity[line].Add(vol, ca);
                    hvParam.DampCoefficient[line].Add(vol, da);
                    hvParam.Tanδ[line].Add(vol, ta);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                foreach (var item in countDic[i])
                {
                    double temp = hvParam.Frequency[i][item.Key];
                    hvParam.Frequency[i].Remove(item.Key);
                    hvParam.Frequency[i].Add(item.Key, temp / item.Value);

                    temp = hvParam.Capacity[i][item.Key];
                    hvParam.Capacity[i].Remove(item.Key);
                    hvParam.Capacity[i].Add(item.Key, temp / item.Value);

                    temp = hvParam.DampCoefficient[i][item.Key];
                    hvParam.DampCoefficient[i].Remove(item.Key);
                    hvParam.DampCoefficient[i].Add(item.Key, temp / item.Value);

                    temp = hvParam.Tanδ[i][item.Key];
                    hvParam.Tanδ[i].Remove(item.Key);
                    hvParam.Tanδ[i].Add(item.Key, temp / item.Value);
                }
            }
                #endregion
            //排序
            for (int i = 0; i < 3; i++)
            {
                hvParam.Frequency[i] = hvParam.Frequency[i].OrderBy(c => c.Key).ToDictionary(r => r.Key, v => v.Value);
                hvParam.Capacity[i] = hvParam.Capacity[i].OrderBy(c => c.Key).ToDictionary(r => r.Key, v => v.Value);
                hvParam.DampCoefficient[i] = hvParam.DampCoefficient[i].OrderBy(c => c.Key).ToDictionary(r => r.Key, v => v.Value);
                hvParam.Tanδ[i] = hvParam.Tanδ[i].OrderBy(c => c.Key).ToDictionary(r => r.Key, v => v.Value);
            }
            return hvParam;
        }

    }
}
