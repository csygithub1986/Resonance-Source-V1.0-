using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Resonance
{
    /// <summary>
    /// 自动分析匹配脉冲类
    /// </summary>
    class AutoAnalyse
    {
        public AutoAnalyse(CalibrationInfo calibrationInfo, CableInfo cableInfo)
        {
            _calibrationInfo = calibrationInfo;
            _cableInfo = cableInfo;
        }

        /// <summary>
        ///  标定信息
        /// </summary>
        CalibrationInfo _calibrationInfo;

        /// <summary>
        /// 电缆信息
        /// </summary>
        CableInfo _cableInfo;

        Pulse[] _pulses;

        /// <summary>
        /// 维护波峰访问顺序
        /// </summary>
        List<Peak> _peakList = new List<Peak>();

        /// <summary>
        /// 当前显示的第几个
        /// </summary>
        int _currentId = -1;

        /// <summary>
        /// 记录分析结果
        /// </summary>
        List<PulsePair> _mapResult = new List<PulsePair>();

        /// <summary>
        /// 当前的入射波
        /// </summary>
        Peak _enterPeak;

        /// <summary>
        /// 当前的反射波
        /// </summary>
        Peak _reflectPeak;

        /// <summary>
        /// 待确定的波形
        /// </summary>
        List<Peak> _potentialPeak;

        /// <summary>
        /// 记录该脉冲配对属于哪一个文件
        /// </summary>
        string _belongTo;

        /// <summary>
        /// 相序
        /// </summary>
        int ph;

       /// <summary>
       /// 开始分析
       /// </summary>
       /// <param name="pulses">所有脉冲</param>
       /// <param name="mapResult">用于记录结果的集合</param>
       /// <param name="belongTo">属于哪个文件</param>
        public void Do(Pulse[] pulses, ref List<PulsePair> mapResult, string belongTo)
        {
            _pulses = pulses;
            //_peakPos = peakPos;
            //_peakValue = peakValue;
            _mapResult = mapResult;
            //_phase = phase;
            _belongTo = belongTo;
            ph = "ABC".IndexOf(belongTo.Substring(0, 1));
            SortPeak();
            while (Match())
            {
                AddToCurrent();
            }
        }

        /// <summary>
        /// 配对脉冲
        /// </summary>
        /// <returns>返回是否找到脉冲对</returns>
        private bool Match()
        {
            Peak currentPeak;
            _potentialPeak = new List<Peak>();
            while (true)
            {
                _currentId++;
                if (_currentId == _peakList.Count)
                {
                    return false;
                }
                currentPeak = _peakList[_currentId];
                if (currentPeak.IsVisited)
                {
                    continue;
                }
                //配对（目前没有写入正式的配对方法，只是找到时间窗内可以配对的即可）
                bool hasFindOne = false;
                foreach (var item in _peakList)
                {
                    double distance = (item.Index - currentPeak.Index) * 1000 * _calibrationInfo.Velocity / Params.SamRatePd / 2;
                    if (item.IsVisited == false
                        //&& item.Index != currentPeak.Index
                        && distance < _cableInfo.Length - 1 && currentPeak.Amplitude * item.Amplitude > 0
                        && distance > 0//0米以内不算放电
                        && MatchAttenuation(currentPeak, item)
                        && Math.Abs(item.Amplitude) < Math.Abs(currentPeak.Amplitude)
                        )
                    {
                        //item.IsVisited = true;//不用再考虑用此脉冲做入射
                        //按time排序
                        if (_potentialPeak.Count == 0)
                        {
                            _potentialPeak.Add(item);
                        }
                        else
                        {
                            for (int i = 0; i < _potentialPeak.Count; i++)
                            {
                                if (item.Index < _potentialPeak[i].Index)
                                {
                                    _potentialPeak.Insert(i, item);
                                    break;
                                }
                                if (i == _potentialPeak.Count - 1)
                                {
                                    _potentialPeak.Add(item);
                                    break;
                                }
                            }
                        }

                        if (hasFindOne == false)
                        {
                            _enterPeak = currentPeak;
                            _reflectPeak = item;
                            hasFindOne = true;
                        }
                    }
                }
                if (hasFindOne)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 判断是否满足衰减条件
        /// </summary>
        /// <param name="peakBig">入射脉冲</param>
        /// <param name="peakSmall">反射脉冲</param>
        /// <returns></returns>
        private bool MatchAttenuation(Peak peakBig, Peak peakSmall)
        {
            double theory = peakBig.Amplitude * Math.Exp(-_calibrationInfo.Attenuation * (peakSmall.Index - peakBig.Index) / Params.SamRatePd * 1000);
            if (theory > peakSmall.Amplitude * 0.7 && theory < peakSmall.Amplitude * 1.3)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将Pulse对象转化为Peak对象，便于排序和自动分析
        /// </summary>
        private void SortPeak()
        {
            int length = _pulses.Length;
            for (int i = 0; i < length; i++)
            {
                Peak peak = new Peak()
                {
                    Index = _pulses[i].Index,
                    Amplitude = _pulses[i].Amplitude,
                    IsVisited = false,
                    Phase = _pulses[i].Phase,
                };
                _peakList.Add(peak);
            }
            _peakList.Sort();
        }

        /// <summary>
        /// 满足脉冲匹配条件以后，加入结果中
        /// </summary>
        private void AddToCurrent()
        {
            _enterPeak.IsVisited = true;
            _reflectPeak.IsVisited = true;
            //填入返回的键值对
            double timeSpan = (_reflectPeak.Index - _enterPeak.Index) / Params.SamRatePd;
            double distance = _cableInfo.Length - timeSpan * 1000 * _calibrationInfo.Velocity / 2;
            //距离修正（只是自动分析的有正确的反算，手动改正后的没有）
            double amplitude = _enterPeak.Amplitude / Math.Exp(-_calibrationInfo.Attenuation * distance / _calibrationInfo.Velocity);

            List<int> rtList = new List<int>();
            foreach (var item in _potentialPeak)
            {
                rtList.Add(item.Index);//所有反射脉冲时间
            }
            _mapResult.Add(new PulsePair() { Q = _enterPeak.Amplitude * _calibrationInfo.PcPerMv, BelongTo = _belongTo, Distance = distance, Amplitude = _enterPeak.Amplitude, Phase = _enterPeak.Phase, EnterIndex = _enterPeak.Index, ReflectIndex = _reflectPeak.Index, RTList = rtList });//放电量
        }

    }
}
