using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resonance
{
    /// <summary>
    /// 通信协议类，定义命令头
    /// </summary>
    public class Cmd
    {
        /// <summary>
        /// 扫频命令,无参数，上位机下发的获取电缆参数命令，138开始控制高压源控制板开始扫频
        /// </summary>
        public const short CMD_SWEEPFREQ = 0x0101;
        //开始测试命令，参数1：谐振频率, 参数2：目标电压
        public const short CMD_STARTTEST = 0x0102;
        public const short CMD_OPENDEVICE = 0x0103;
        public const short CMD_CLOSEDEVICE = 0x0104;
        public const short CMD_STOPSWEEP = 0x0105;//停止扫频
        public const short CMD_STOPTEST = 0x0106;//停止测试
        public const short CMD_SWITCHRANGE = 0x0107;//切换量程

        public const short ACK_FREQREADY = 0x0201; //扫频完成,  138完成扫频后上传的完成命令，参数1：谐振频率, 参数2：NULL
        public const short ACK_UPLOADDATA = 0x0202; //开始上传数据  
        public const short ACK_TESTFINISH = 0x0203;//数据采集完成
        public const short ACK_HVDATA = 0x0204;//高压数据
        public const short ACK_PDDATA = 0x0205; //局放数据
        public const short ACK_HVRAISING = 0x0206; //升压过程（实验版）
        public const short ACK_WARNING = 0x0207; //告警
        public const short ACK_SWEEPDATA = 0x0208;

    }
}
