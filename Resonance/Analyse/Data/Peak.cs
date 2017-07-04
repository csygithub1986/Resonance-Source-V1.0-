using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Resonance
{
    /// <summary>
    /// 波峰结构，用于排序和自动分析
    /// </summary>
    public class Peak : IComparable<Peak>
    {
        /// <summary>
        /// 数组中的索引
        /// </summary>
        public int Index;

        /// <summary>
        /// 幅值
        /// </summary>
        public double Amplitude;

        /// <summary>
        /// 相位
        /// </summary>
        public double Phase;

        /// <summary>
        /// 是否已经经过筛选
        /// </summary>
        public bool IsVisited;

        /// <summary>
        /// 大的排前面
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Peak other)
        {
            if (Math.Abs(other.Amplitude) < Math.Abs(Amplitude))
            {
                return -1;
            }
            else if (other.Amplitude == Amplitude)
            {
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// 时间相等，即相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj is Peak == false)
            {
                return false;
            }
            Peak o = obj as Peak;
            if (o.Index == Index)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }
}
