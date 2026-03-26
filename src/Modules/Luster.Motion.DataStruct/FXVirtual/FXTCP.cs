using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using NLog;

namespace Luster.Motion.DataStruct.FXVirtual
{
    /// <summary>
    /// 由于IMotionCard接口内部会互相调用，因此外部调用时再做一次转发，不在FXVirtualMotionCardAspect写具体逻辑
    /// 注入IDeviceEngine
    /// </summary>
    public class FXTCP : IMotionCard
    {
        #region Singleton
        private FXTCP() { }
        private static class HolderClass
        {
            static HolderClass() { }
            internal static FXTCP instance = new FXTCP();
        }
        public static FXTCP Insatnce
        {
            get
            {
                return HolderClass.instance;
            }
        }
        private IDeviceEngine _deviceEngine;
        private void Log(string message)
        {
            LogEventService.Instance.PublishLog($"[FXTCP] {message}");
        }

        #endregion

        #region TCP

        #region 常量
        private const int INT_SIZE = sizeof(int);
        private const int DOUBLE_SIZE = sizeof(double);
        private const int Data_SIZE = 2048;
        #endregion

        #region 变量
        // 修改字段类型
        private ITcpClientWrapper m_TcpClient;
        /// <summary>
        /// 由OutIO与Axis构成
        /// </summary>
        public byte[] SendDataBuff { get; private set; } = new byte[Data_SIZE];
        /// <summary>
        /// 由InIO与Axis构成
        /// </summary>
        public byte[] RecvDataBuff { get; private set; } = new byte[Data_SIZE];
        #endregion

        // 动态IO列表
        //输入IO，从tcp读取解析
        private List<VIO> InIO { get; set; } = new List<VIO>();
        //输出IO，发给tcp
        private List<VIO> OutIO { get; set; } = new List<VIO>();
        private List<VAxis> RealAxes { get; set; } = new List<VAxis>();

        // 动态轴列表
        private List<AxisSignal> ReceiveAxes { get; set; } = new List<AxisSignal>();

        private List<AxisSignal> SendAxes { get; set; } = new List<AxisSignal>();


        // 动态计算长度
        private int InLength => (InIO.Count == 0) ? 0 : (int)Math.Ceiling(InIO.Count / 32f) * INT_SIZE;
        private int OutLength => (OutIO.Count == 0) ? 0 : (int)Math.Ceiling(OutIO.Count / 32f) * INT_SIZE;
        private int AxisSignalSize => Marshal.SizeOf<AxisSignal>(); // 36字节
        private int TotalSendLength => OutLength + SendAxes.Count * AxisSignalSize;
        private int TotalRecvLength => InLength + ReceiveAxes.Count * AxisSignalSize;

        public bool IsConnect => m_TcpClient?.Connected ?? false;
        public bool ReConnect(int times = 3, int retryMs = 500, string ip = "127.0.0.1", int port = 502, IDeviceEngine deviceEngine = null)
        {
            //读取外部的输入输出IO数           
            if (deviceEngine == null)
            {
                return false;
            }

            InjectDeviceEngine(deviceEngine);

            if (m_TcpClient == null)
            {
                m_TcpClient?.Close();
                for (int i = 0; i < times; i++)
                {
                    try
                    {
                        m_TcpClient = new TcpClientWrapper(); // 使用包装类
                        m_TcpClient.Connect(ip, port);
                        return true;
                    }
                    catch
                    {
                        Thread.Sleep(retryMs);
                    }
                }
            }
            return true;
        }


        public void Dispose()
        {
            if (m_TcpClient != null)
            {
                m_TcpClient.Close();
                m_TcpClient = null;
            }
            InIO.Clear();
            OutIO.Clear();
            ReceiveAxes.Clear();
            SendAxes.Clear();
            RealAxes.Clear();
        }

        #region Data Buffers Handling
        /// <summary>
        /// 更新发送缓冲区（输出IO + 所有轴状态）
        /// </summary>
        private void UpdateSendBuffer()
        {
            // 确保缓冲区足够大
            if (SendDataBuff.Length < TotalSendLength)
                SendDataBuff = new byte[TotalSendLength];
            //Array.Resize(ref SendDataBuff, TotalSendLength);

            int offset = 0;

            // 1. 打包输出IO状态
            if (OutIO.Count > 0)
            {
                byte[] outBytes = PackIOStates(OutIO);
                Buffer.BlockCopy(outBytes, 0, SendDataBuff, offset, outBytes.Length);
                offset += OutLength;
            }

            // 2. 打包所有轴状态
            foreach (AxisSignal axis in SendAxes)
            {
                byte[] axisBytes = axis.ToBytes();
                Buffer.BlockCopy(axisBytes, 0, SendDataBuff, offset, axisBytes.Length);
                offset += AxisSignalSize;
            }

            //3. 将Axes反馈给 RealAxes
            //foreach (var item in Axes)
            //{
            //    var realAxis = RealAxes.FirstOrDefault(a => a.Name == item.Name);
            //    if (realAxis != null)
            //    {
            //        realAxis.MoveSpeed = item.Speed;
            //        realAxis.Acc = (int)item.Acc;
            //        realAxis.Dec = (int)item.Dec;
            //    }
            //}


        }

        /// <summary>
        /// 解析接收缓冲区（输入IO + 所有轴状态）
        /// </summary>
        private void ParseRecvBuffer()
        {
            int offset = 0;

            // 1. 解析输入IO状态
            if (InIO.Count > 0)
            {
                byte[] inBytes = new byte[InLength];
                Buffer.BlockCopy(RecvDataBuff, offset, inBytes, 0, InLength);
                UnpackIOStates(inBytes, InIO);
                offset += InLength;
            }

            // 2. 解析所有轴状态
            for (int i = 0; i < ReceiveAxes.Count; i++)
            {
                byte[] axisBytes = new byte[AxisSignalSize];
                Buffer.BlockCopy(RecvDataBuff, offset, axisBytes, 0, AxisSignalSize);

                // 使用内存直接解析避免多次拷贝
                ReceiveAxes[i] = ParseAxisSignal(axisBytes);
                offset += AxisSignalSize;
            }

            //foreach (var item in Axes)
            //{
            //    var realAxis = RealAxes.FirstOrDefault(a => a.Name == item.Name);
            //    if (realAxis != null)
            //    {
            //        realAxis.MoveSpeed = item.Speed;
            //        realAxis.Acc = (int)item.Acc;
            //        realAxis.Dec = (int)item.Dec;
            //    }
            //}
        }

        private static readonly ReaderWriterLockSlim LockSlim = new ReaderWriterLockSlim();

        /// <summary>
        /// 发送数据到TCP设备
        /// </summary>
        public void SendData(string logTip = "")
        {
            string sendstr = string.Empty;
            try
            {
                if (m_TcpClient?.Connected != true) return;

                UpdateSendBuffer();
                var stream = m_TcpClient.GetStream();
                //stream.WriteTimeout = 500; // 500ms超时               
                stream.Write(SendDataBuff, 0, TotalSendLength);
                sendstr = BitConverter.ToString(SendDataBuff.Take(TotalSendLength).ToArray(), 0, TotalSendLength);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (!string.IsNullOrEmpty(sendstr))
                    Log("Send:" + logTip + "\r\n" + sendstr);
            }
        }

        /// <summary>
        /// 从TCP设备接收数据
        /// </summary>
        public void ReceiveData(int receiveLen = 0)
        {
            string recvStr = string.Empty;
            try
            {
                if (m_TcpClient?.Connected != true) return;
                var stream = m_TcpClient.GetStream();
                //{
                stream.ReadTimeout = 500; // 500ms超时
                int bytesRead = 0;
                int totalToRead = TotalRecvLength;
                if (receiveLen > TotalRecvLength)
                {
                    totalToRead = receiveLen;
                }
                while (bytesRead < totalToRead)
                {
                    bytesRead += stream.Read(
                        RecvDataBuff,
                        bytesRead,
                        totalToRead - bytesRead
                    );
                }
                //}
                ParseRecvBuffer();
                recvStr = BitConverter.ToString(RecvDataBuff.Take(TotalRecvLength).ToArray(), 0, TotalRecvLength);
            }
            catch (Exception ex)
            {
                //Log("ReceiveError:" + "\r\n" + ex.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(recvStr))
                    Log("Receive:" + "\r\n" + recvStr);
            }
        }
        #endregion

        #region IO Packing Utilities
        /// <summary>
        /// 将IO状态列表打包为字节数组
        /// </summary>
        private byte[] PackIOStates(List<VIO> ioList)
        {
            int intCount = (ioList.Count + 31) / 32;
            int[] states = new int[intCount];

            for (int i = 0; i < ioList.Count; i++)
            {
                if (ioList[i].Value == 1)
                {
                    int intIndex = i / 32;
                    int bitIndex = i % 32;
                    states[intIndex] |= (1 << bitIndex);
                }
            }

            byte[] result = new byte[intCount * INT_SIZE];
            Buffer.BlockCopy(states, 0, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// 从字节数组解包IO状态到列表
        /// </summary>
        private void UnpackIOStates(byte[] data, List<VIO> ioList)
        {
            int intCount = data.Length / INT_SIZE;
            if (intCount == 0) return;

            int[] states = new int[intCount];
            Buffer.BlockCopy(data, 0, states, 0, data.Length);

            for (int i = 0; i < Math.Min(ioList.Count, intCount * 32); i++)
            {
                int intIndex = i / 32;
                int bitIndex = i % 32;
                ioList[i].Value = (states[intIndex] & (1 << bitIndex)) != 0 ? 1 : 0;
            }
        }
        #endregion

        #region Axis Signal Parsing
        /// <summary>
        /// 高性能解析轴信号（避免多次内存拷贝，处理大端序转换）
        /// </summary>
        private AxisSignal ParseAxisSignal(byte[] data)
        {
            if (data == null || data.Length < 36)
                throw new ArgumentException("数据长度不足");
            // 解析状态（4字节）
            uint stateValue = ParseBigEndianUInt32(data, 0);

            // 解析双精度值（自动处理字节序）
            double speed = ParseBigEndianDouble(data, 4);
            double acc = ParseBigEndianDouble(data, 12);
            double dec = ParseBigEndianDouble(data, 20);
            double pos = ParseBigEndianDouble(data, 28);

            return new AxisSignal(
                new AxisState(stateValue),
                speed,
                acc,
                dec,
                pos
            );
        }

        // 解析大端序的32位整数
        private static uint ParseBigEndianUInt32(byte[] data, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                // 手动反转字节顺序
                return (uint)(
                    (data[offset] << 24) |
                    (data[offset + 1] << 16) |
                    (data[offset + 2] << 8) |
                    data[offset + 3]
                );
            }

            // 大端系统直接转换
            return BitConverter.ToUInt32(data, offset);
        }
        // 解析大端序的双精度浮点数
        private static double ParseBigEndianDouble(byte[] data, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                // 创建临时缓冲区并反转字节
                byte[] buffer = new byte[8];
                Array.Copy(data, offset, buffer, 0, 8);
                Array.Reverse(buffer);
                return BitConverter.ToDouble(buffer, 0);
            }

            // 大端系统直接转换
            return BitConverter.ToDouble(data, offset);
        }
        #endregion

        #endregion

        #region Interface       

        public bool GetDigitalIn(int index)
        {
            // 1. 找到对应的VIO对象
            var vio = InIO.FirstOrDefault(io => io.Index == index);
            if (vio == null)
                throw new ArgumentException($"未找到Index={index}的输入IO");

            // 2. 返回其当前值（一般为0或1）
            return vio.Value == 1;
        }

        public bool GetDigitalOut(int index)
        {
            // 1. 找到对应的VIO对象
            var vio = OutIO.FirstOrDefault(io => io.Index == index);
            if (vio == null)
                throw new ArgumentException($"未找到Index={index}的输入IO");

            // 2. 返回其当前值（一般为0或1）
            return vio.Value == 1;
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            var vio = OutIO.FirstOrDefault(io => io.Index == index);
            if (vio == null)
                throw new ArgumentException($"未找到Index={index}的输出IO");

            vio.Value = digitalOut ? 1 : 0;

            // 更新缓冲区
            UpdateSendBuffer();
            SendData($"SetDigitalOut(int index={index}, bool digitalOut={digitalOut})");
        }

        public void ScanAxis(out uint axisNum)
        {
            axisNum = (uint)ReceiveAxes.Count;
        }

        public void ScanDigitalIO(out uint digitalIn, out ushort digitalOut)
        {
            digitalIn = (uint)InIO.Count;
            digitalOut = (ushort)OutIO.Count;
        }

        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPulse = 0)
        {
            //如果axisNo=-1，则检查所有轴是否完成运动
            if (axisNo == -1)
            {
                // 检查所有轴是否都在目标位置
                foreach (var axis in ReceiveAxes)
                {
                    if (Math.Abs(axis.Pos) > precision)
                    {
                        return false; // 只要有一个轴未到位就返回false
                    }
                }
                return true; // 所有轴都到位
            }
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= ReceiveAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");
            double targetPos = SendAxes[realAxis].Pos;
            //ReceiveData();
            double nowPos = ReceiveAxes[realAxis].Pos;
            if (Math.Abs(targetPos - nowPos) <= precision)
            {
                return true;
            }
            return false;
        }

        public double GetCurrentPos(int axisNo, double perPulse)
        {
            // 1. 刷新接收缓冲区，获取最新数据
            //ReceiveData();
            // 2. 边界检查
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= ReceiveAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            // 3. 获取当前位置（脉冲/编码器原始值）
            double pos = ReceiveAxes[realAxis].Pos;

            // 4. 按perPulse换算为物理量（如mm/deg），如不需要可直接返回pos
            return pos /** perPulse*/;
        }

        public void SetCurrentPos(int axisNo, double perPulse, double position)
        {
            throw new NotImplementedException("转盘未用到");
        }

        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= SendAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            Move(0, low, homeAcc, homeAcc, perPlus, 0, true, axisNo, 0);

            while (!CheckHomeDone(axisNo))
            {
                Thread.Sleep(100);
            }
        }

        public void HomeCancel(int axisNo)
        {
            Stop(axisNo);
        }

        public bool CheckHomeDone(int axisNo = 0)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= ReceiveAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            // 3. 获取当前位置（脉冲/编码器原始值）
            double pos = ReceiveAxes[realAxis].Pos;
            if (Math.Abs(pos) < 1e-6) // 假设小于1e-6认为是到位
            {
                // 如果到位，重置状态
                ResetState(axisNo);
                return true;
            }
            return false; // 未到位
        }

        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= SendAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            // 1. 获取当前轴状态
            var axis = SendAxes[realAxis];
            // 2. 设置新的速度、加速度、减速度（Jog模式下通常不需要设置位置）
            axis = new AxisSignal(SendAxes[realAxis].State, vel, acc, dec, 0/*, axis.Name*/);
            // 3. 更新轴状态
            SendAxes[realAxis] = axis;
            // 4. 更新发送缓冲区并发送数据
            UpdateSendBuffer();
            SendData($"Jog(double vel={vel}, double acc={acc}, double dec={dec},int axisNo={realAxis})");
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= SendAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");
            // 1. 获取当前轴状态
            var axis = SendAxes[realAxis];
            if (!isAbsMove)
            {
                pos += GetCurrentPos(axisNo, 1);
            }
            // 2. 设置新的速度、加速度、减速度、位置
            axis = new AxisSignal(SendAxes[realAxis].State, vel, acc, dec, pos/*, axis.Name*/);
            // 3. 更新轴状态
            SendAxes[realAxis] = axis;
            // 4. 更新发送缓冲区并发送数据
            UpdateSendBuffer();
            SendData($"Move(double pos={pos}, double vel={vel}, double acc={acc}, double dec={dec}, int axisNo={axisNo})");

        }

        public void Stop(int axisNo, bool isAll = false)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= SendAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");
            var oldSignal = SendAxes[realAxis];
            double pos = oldSignal.Pos;
            double vel = 0;
            double acc = oldSignal.Acc;
            double dec = oldSignal.Dec;
            var axis = new AxisSignal(SendAxes[realAxis].State, vel, acc, dec, pos/*, axis.Name*/);
            SendAxes[realAxis] = axis;
            // 4. 更新发送缓冲区并发送数据
            UpdateSendBuffer();
            SendData($"Stop(int axisNo={axisNo},double pos={pos}, double vel={vel}, double acc={acc}, double dec={dec},, bool isAll = false)");
        }

        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            throw new NotImplementedException();
        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            throw new NotImplementedException();
        }

        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            var realAxis = axisNo;
            // 2. 边界检查
            if (realAxis < 0 || realAxis >= ReceiveAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            var state = ReceiveAxes[realAxis].State;
            // 3. 获取当前位置（脉冲/编码器原始值）
            return new Dictionary<AxisStatus, bool>()
            {
                {AxisStatus.Alarm,state.Alm},
                {AxisStatus.Emg, state.Emg},
                {AxisStatus.Mel,state.Eln},
                {AxisStatus.Pel, state.Elp},
                {AxisStatus.Org, state.Org},
                {AxisStatus.Moving, state.Run},
                {AxisStatus.OnPos, CheckMotionDone(45,axisNo)},
                {AxisStatus.IsHome,CheckHomeDone(axisNo) },
                {AxisStatus.SvOn, true} // 使能状态
            };
        }

        public void ServOn(int axisNo, bool isOn)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= SendAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            // 取出原有AxisSignal
            var oldSignal = SendAxes[realAxis];
            // 修改Sevon位
            uint stateValue = oldSignal.State.RawValue;
            if (isOn)
                stateValue |= 0x10000; // Sevon位
            else
                stateValue &= ~0x10000u;

            // 构造新AxisSignal
            var newSignal = new AxisSignal(
                new AxisState(stateValue),
                oldSignal.Speed,
                oldSignal.Acc,
                oldSignal.Dec,
                oldSignal.Pos
            );
            SendAxes[realAxis] = newSignal;

            // 更新缓冲区并下发
            UpdateSendBuffer();
            SendData($"ServOn(int axisNo={realAxis}, bool isOn={isOn})");
        }

        public void ResetState(int axisNo)
        {
            var realAxis = axisNo;
            if (realAxis < 0 || realAxis >= SendAxes.Count)
                throw new ArgumentOutOfRangeException(nameof(realAxis), $"轴号{realAxis}超出范围");

            var oldSignal = SendAxes[realAxis];
            uint stateValue = oldSignal.State.RawValue;

            // 清除Alarm和Emg位（如有其他需清除的位可补充）
            stateValue &= ~0x01u; // Alarm
            stateValue &= ~0x08u; // Emg

            var newSignal = new AxisSignal(
                new AxisState(stateValue),
                oldSignal.Speed,
                oldSignal.Acc,
                oldSignal.Dec,
                oldSignal.Pos
            );
            SendAxes[realAxis] = newSignal;

            UpdateSendBuffer();
            SendData($"ResetState(int axisNo={realAxis})");
        }

        public void ClearEmg()
        {
            // 清除所有轴的Emg状态
            for (int i = 0; i < ReceiveAxes.Count; i++)
            {
                ResetState(i);
            }
        }


        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
        {
            throw new NotImplementedException();
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            throw new NotImplementedException();
        }

        public double GetAnalogIn(int index)
        {
            throw new NotImplementedException();
        }

        public double GetAnalogOut(int index)
        {
            throw new NotImplementedException();
        }
        public void SDORead(short slave, short index, short subindex, short data_size, out int value, short count)
        {
            throw new NotImplementedException();
        }

        public void SDOWrite(short slave, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            throw new NotImplementedException();
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void InjectDeviceEngine(IDeviceEngine deviceEngine)
        {
            var ios = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO).Where(x => x.IOType == IOType.Digital);
            InIO = ios.Where(x => x.Behavior == IOBehavior.Input).ToList();
            OutIO = ios.Where(x => x.Behavior == IOBehavior.Output).ToList();

            RealAxes = deviceEngine.GetDevices(typeof(VAxis)).Select(u => u as VAxis).ToList();
            if (ReceiveAxes.Count == RealAxes.Count)
            {
                return;
            }
            ReceiveAxes = RealAxes.Select(axis => new AxisSignal
            (
                 new AxisState(0), // 初始状态为0
                 0.0, // 初始速度为0
                 0.0, // 初始加速度为0
                 0.0, // 初始减速度为0
                 0.0  // 初始位置为0
            )).ToList();

            SendAxes = RealAxes.Select(axis => new AxisSignal
            (
                new AxisState(0), // 初始状态为0
                0.0, // 初始速度为0
                0.0, // 初始加速度为0
                0.0, // 初始减速度为0
                0.0  // 初始位置为0
            )).ToList();
        }

        /// <summary>
        /// 将当前的IO和轴状态导出为XML格式
        /// </summary>
        public void ExportXml(string folder = "E:\\")
        {
            var generator = new MCDGenerator();
            // 优化：预分配足够容量
            var vioList = new List<VIO>(InIO.Count + OutIO.Count);
            vioList.AddRange(InIO);
            vioList.AddRange(OutIO);
            XElement mcdXml = generator.GenerateMCDXml(vioList, RealAxes);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var filePath = System.IO.Path.Combine(folder, "MCDInfo.xml");
            // 保存到文件
            mcdXml.Save(filePath);
            Log($"MCDInfo文件已导出到目录{folder}");
        }

        protected void CalcTime(Func<bool> action, int timeout = -1, int sleep = 5, Action timeoutAction = null)
        {
            double time = 0;

            // 等待运行
            while (true)
            {
                // 超时检查
                if (timeout > 0)
                {
                    if (time > timeout)
                    {
                        if (timeoutAction != null)
                        {
                            timeoutAction?.Invoke();
                            break;
                        }
                        else
                        {
                            throw new DeviceTimeoutException("F98OOOO-08", $"CalcTime超时");
                        }
                    }
                }

                // 判断
                if (action.Invoke())
                {
                    break;
                }

                // 防止刷新太快IO检查错误
                Thread.Sleep(sleep);
                time += sleep;
            }
        }

    }

}
