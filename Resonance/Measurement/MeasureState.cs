using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 管理全局的测试状态，以及数据的接收事件
    /// </summary>
    public class MeasureState
    {
        /// <summary>
        /// 网络连接
        /// </summary>
        public static TcpClient Client;
        public static BinaryReader TcpBinaryReader;
        public static BinaryWriter TcpBinaryWriter;

        /// <summary>
        /// 电缆与测试状态
        /// </summary>
        public static CableInfo CableInfo;

        /// <summary>
        /// 标定的状态
        /// </summary>
        public static CalibrationInfo[] CalibInfos = new CalibrationInfo[3];

        /// <summary>
        /// 接收扫描频率
        /// </summary>
        public static event Action FreArrived;

        /// <summary>
        /// 收到开始上传数据命令，做初始化准备
        /// </summary>
        public static event Action PrepareReceiveData;

        /// <summary>
        /// 接收到上传完毕命令，开始处理数据
        /// </summary>
        public static event Action FinishData;

        /// <summary>
        /// 接收到上传高压命令，开始接收高压数据
        /// </summary>
        public static event Action HvArrived;

        /// <summary>
        /// 接收到上传局放命令，开始接收局放数据（或标定数据）
        /// </summary>
        public static event Action PdArrived;

        /// <summary>
        /// 接收到告警命令，处理告警
        /// </summary>
        public static event Action WarningArrived;

        /// <summary>
        /// 收到扫频过程数据
        /// </summary>
        public static event Action SweepDataArrived;

        /// <summary>
        /// 网络断链处理
        /// </summary>
        public static event Action DisconnectEvent;

        /// <summary>
        /// 全局的网络数据监听
        /// </summary>
        public static void ListenThread()
        {
            try
            {
                while (true)
                {
                    short cmd = TcpBinaryReader.ReadInt16();
                    switch (cmd)
                    {
                        case Cmd.ACK_FREQREADY:
                            FreArrived();
                            break;
                        case Cmd.ACK_UPLOADDATA:
                            PrepareReceiveData();
                            break;
                        case Cmd.ACK_TESTFINISH:
                            FinishData();
                            break;
                        case Cmd.ACK_HVDATA:
                            HvArrived();
                            break;
                        case Cmd.ACK_PDDATA:
                            PdArrived();
                            break;
                        case Cmd.ACK_HVRAISING://测试版本内容，正式版本保留
                            TcpBinaryReader.ReadBytes(4096 * 2);
                            break;
                        case Cmd.ACK_WARNING:
                            WarningArrived();
                            break;
                        case Cmd.ACK_SWEEPDATA:
                            SweepDataArrived();
                            break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                DisconnectEvent();
            }
            catch (ObjectDisposedException)
            {
                DisconnectEvent();
            }
            catch (IOException)
            {
                DisconnectEvent();
            }
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

    }
}
