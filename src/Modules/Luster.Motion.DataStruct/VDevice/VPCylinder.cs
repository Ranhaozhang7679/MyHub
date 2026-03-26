#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VMCylinder
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.VDevice
* 文 件 名:       VMCylinder.cs
* 创建时间:       2022/8/9 14:50:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b897a8f5-f6c5-4acf-a0b4-11afce7f539f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/9 14:50:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.Common.DataStruct;
using System.Threading;
using Luster.Common.DataStruct.Enums;
using System.Runtime.ConstrainedExecution;
using System.Drawing;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 力矩电缸
    /// </summary>
    public class VPCylinder : VirtualDeviceBase, IHome, IStop, IDisposable, IHeartbeat, IDeviceShield, IPosition
    {
        public override int Sort => 8;

        /// <summary>
        /// 站号
        /// </summary>
        public int AddrNo { get; set; }

        /// <summary>
        /// Socket 通信
        /// </summary>
        public ICommunication Communication { get; set; }

        /// <summary>
        /// 协议种类
        /// </summary>
        public Enums.ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// 协议
        /// </summary>
        [Ignore]
        public IProtocol Protocol { get; set; }

        /// <summary>
        /// 多个点位
        /// </summary>
        public LArray<PointPos> Positions { get; set; }

        public VPCylinder()
        {
            Positions = new LArray<PointPos>();
        }

        private string WriteMsg = string.Empty;


        private void CommonWrite<T>(T val, string writeMsg)
        {
            if (!Communication.IsConnected)
            {
                Communication.Open();
            }

            CommResult result = Protocol.Write<T>(Communication, new List<T>() { val }, writeMsg);
            if (!result.IsSuccess)
            {
                throw new DeviceException(AlarmCode, result.ErrorMsg);
            }
        }

        private T CommonRead<T>(string readMsg)
        {
            if (!Communication.IsConnected)
            {
                Communication.Open();
            }

            CommResult<T> cResult = Protocol.Read<T>(Communication, readMsg, 1);
            if (!cResult.IsSuccess)
            {
                throw new DeviceException(AlarmCode, cResult.ErrorMsg);
            }

            return cResult.Datas[0];
        }

        /// <summary>
        /// 执行点位
        /// </summary>
        /// <param name="point"></param>
        /// <returns>返回N</returns>
        public double Move(int point, bool isCheckDone = true, int timeout = 5000)
        {
            CheckComm();

            if (Engine.IsBreakAction)
            {
                return -1;
            }

            var pointPos = GetPoint(point);

            string moveMsg = $"设备:{Name} 开始Move:{pointPos.PointNo},动作类型:{pointPos.CmdType.GetDescription()},speed={pointPos.Speed},pos={pointPos.Pos},pressure={pointPos.Pressure},pressure dis={pointPos.PreDistance}";
            OnLog(Common.DataStruct.Enums.LogType.Debug, moveMsg);

            // 记录状态
            VStatus = VStatus.Running;

            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, pointPos.PointNo + 1408);//写
            CommonWrite(false, WriteMsg);
            System.Threading.Thread.Sleep(12);
            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, pointPos.PointNo + 1408);//写
            CommonWrite(true, WriteMsg);

            if (!isCheckDone)
            {
                return 0;
            }

            int lastp = pointPos.PointNo;
            GetLastPoint(lastp, ref lastp);
            return CheckPosDone(lastp, timeout);
        }

        private PointPos GetPoint(int point)
        {
            var postion = Positions.FirstOrDefault(u => u.PointNo == point);
            if (postion == null)
            {
                throw new DeviceException(AlarmCode, $"设备:{Name} 不存在点位:{point},请加载点位信息");
            }

            return postion;
        }

        private void GetLastPoint(int startP, ref int lastP)
        {
            var position = GetPoint(startP);

            if (position.Next != -1)
            {
                lastP = position.Next;
                GetLastPoint(position.Next, ref lastP);
            }
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            CheckComm();

            if (NeedHome)
            {
                //if (Communication != null && Communication.IsConnected)
                //{
                //    Communication.Close();
                //}

                // 回零时默认开启连接
                var result = Communication.Open();
                if (!result.IsSuccess)
                {
                    throw new DeviceException(AlarmCode, $"通讯设备:{Name} 连接失败!");
                }

                // 先清理错误
                Reset();

                CheckStatus();

                // 重置力
                Move(11);

                // 回零
                WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1401);//写
                CommonWrite(false, WriteMsg);
                System.Threading.Thread.Sleep(20);
                CommonWrite(true, WriteMsg);

                // 回零完成
                InitStopToken();
                string homeEnd = "01 01 1501 1";
                Protocol.Wait<bool>(Communication, homeEnd, true, OpRule.Equal, 5000, 50, stopReset);

                // 闭环推压需要回零两次，否则电缸第二次状态还会保持
                //System.Threading.Thread.Sleep(20);
                //WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1401);//写
                //CommonWrite(false, WriteMsg);
                //System.Threading.Thread.Sleep(20);
                //CommonWrite(true, WriteMsg);

                //Protocol.Wait<bool>(Communication, homeEnd, true, OpRule.Equal, 5000, 50, stopReset);
            }
        }

        private void InitStopToken()
        {
            if (stopReset == null || stopReset.IsCancellationRequested)
            {
                stopReset = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// 通讯释放
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (Communication != null && Communication.IsConnected)
            {
                Communication.Close();
            }
        }

        public void CheckStatus()
        {
            string errMsg = "01 04 6 1";
            var status = CommonRead<int>(errMsg);
            switch (status)
            {
                case 0:
                    break;
                case 1:
                    throw new DeviceException(AlarmCode, $"电缸:{Name}发生通用错误!");
                case 2:
                    throw new DeviceException(AlarmCode, $"电缸:{Name}发生电机失相!");
                case 3:
                    throw new DeviceException(AlarmCode, $"电缸:{Name}发生位置超差!");
                case 4:
                    throw new DeviceException(AlarmCode, $"电缸:{Name}发生速度超差!");
                case 5:
                    throw new DeviceException(AlarmCode, $"电缸:{Name}发生电机堵转!");
                case 6:
                    throw new DeviceException(AlarmCode, $"电缸:{Name}发生初相励磁错误!");
                default:
                    break;
            }
        }

        /// <summary>
        /// 清除+使能
        /// </summary>
        public void Reset()
        {
            CheckComm();

            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1402);//写
            CommonWrite(false, WriteMsg);
            System.Threading.Thread.Sleep(30);
            CommonWrite(true, WriteMsg);
        }

        /// <summary>
        /// 通信检查
        /// </summary>
        /// <exception cref="FriendlyException"></exception>
        private void CheckComm()
        {
            if (Communication == null)
            {
                throw new FriendlyException("通讯对象为空!");
            }

            if (Protocol == null)
            {
                throw new FriendlyException("未配置通讯协议!");
            }
        }

        /// <summary>
        /// 停止对象
        /// </summary>
        private CancellationTokenSource stopReset;

        /// <summary>
        /// 检查点位是否运动到位
        /// </summary>
        /// <param name="pos"></param>
        public double CheckPosDone(int point, int overtime = 5000)
        {
            CheckComm();
            double n = -1;
            var pos = GetPoint(point);

            // 空跑模式下，运动只是做点位检查
            if (GetIsShield())
            {
                OnLog(LogType.Debug, $"空跑模式:等待{overtime / 4}");
                Thread.Sleep(overtime / 4);
                ClearPos();
                return n;
            }

            WriteMsg = string.Format("{0} 1 {1} 1", AddrNo, 1506 + point);

            if (stopReset == null || stopReset.IsCancellationRequested)
            {
                stopReset = new CancellationTokenSource();
            }

            try
            {

                // 2.检查运动到达点位
                OnLog(LogType.Debug, $"设备:{Name} 点位:{point} 开始CheckMotionDone {WriteMsg} Wait等待值 {OpRule.Equal} {true}");
                Protocol.Wait<bool>(Communication, WriteMsg, true, OpRule.Equal, overtime, 50, stopReset);

                // 1.推压运动需要检查运动结束（首先检查运动结束）
                if (pos.CmdType == PClinderCmdType.Push)
                {
                    // 判断是否运动中，如果运动中才代表有夹住东西
                    Thread.Sleep(10);
                    string readMsg = $"01 1 1505 1";

                    OnLog(LogType.Debug, $"设备:{Name} 点位:{point} 类型:{pos.CmdType.GetDescription()} 等待力反馈");
                    Protocol.Wait<bool>(Communication, readMsg, true, OpRule.Equal, overtime, 10, stopReset);
                    OnLog(LogType.Debug, $"设备:{Name} 点位:{point} 类型:{pos.CmdType.GetDescription()} 满足力反馈");
                }

                // 3.读取电缸的力值
                n = CommonRead<int>("01 04 18 1") / Ratio;
                OnLog(LogType.Debug, $"设备：{Name} 点位:{point} 力反馈值:{Math.Round(n / Ratio, 3)} N");

                // 4.如果电缸在运行中，则更新原始状态
                if (VStatus == VStatus.Running && !stopReset.IsCancellationRequested)
                {
                    OnLog(LogType.Debug, $"设备:{Name} 点位:{point} {WriteMsg} Wait满足值 {OpRule.Equal} {true} 当前位置:{GetCurrentPos()}");
                    VStatus = VStatus.Idle;
                }
            }
            catch (TimeoutException ex)
            {
                ClearPos();

                OnLog(Common.DataStruct.Enums.LogType.Error, $"设备:{Name} {ex.Message}");

                throw new DeviceTimeoutException(DeviceError.ConnectTimeFail, this.ID, ex.Message);
            }

            return n;
        }

        // 系数
        private const double Ratio = 1000.0;

        /// <summary>
        /// 清除
        /// </summary>
        private void ClearPos()
        {
            // 超时的情况，则连续发生两次到0点位
            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1408);//写
            CommonWrite(false, WriteMsg);
            System.Threading.Thread.Sleep(12);
            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1408);//写
            CommonWrite(true, WriteMsg);

            // 超时的情况，则连续发生两次到0点位
            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1408);//写
            CommonWrite(false, WriteMsg);
            System.Threading.Thread.Sleep(12);
            WriteMsg = string.Format("{0} 05 {1} 1", AddrNo, 1408);//写
            CommonWrite(true, WriteMsg);
        }

        /// <summary>
        /// 更新参数
        /// </summary>
        public void SetParam(int point)
        {
            CheckComm();

            var position = Positions.FirstOrDefault(u => u.PointNo == point);
            if (position == null)
            {
                throw new FriendlyException($"点位:{point}不存在!");
            }

            var cmdType = position.CmdType;
            if (cmdType == PClinderCmdType.Abs || cmdType == PClinderCmdType.Rel)
            {
                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5000 + (point) * 20);//指令类型
                CommonWrite((int)cmdType, WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5002 + (point) * 20);//位置
                CommonWrite((int)(position.Pos * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5004 + (point) * 20);//速度
                CommonWrite((int)(position.Speed * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5006 + (point) * 20);//加速度
                CommonWrite((int)(position.Acc * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5008 + (point) * 20);//减速度
                CommonWrite((int)(position.Dec * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5010 + (point) * 20);//定位区间
                CommonWrite((int)(position.Range * Ratio), WriteMsg);
            }
            else if (cmdType == PClinderCmdType.ClosePush)
            {
                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5000 + (point) * 20);//指令类型
                CommonWrite((int)cmdType, WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5004 + (point) * 20);//速度
                CommonWrite((int)(position.Speed * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5010 + (point) * 20);//定位区间
                CommonWrite((int)(position.Range * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5012 + (point) * 20);//推压力
                CommonWrite((int)(position.Pressure * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5014 + (point) * 20);//推压距离
                CommonWrite((int)(position.PreDistance * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5016 + (point) * 20);//延时时间
                CommonWrite((int)position.Delay, WriteMsg);
            }
            else if (cmdType == PClinderCmdType.Push)
            {
                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5000 + (point) * 20);//指令类型
                CommonWrite((int)cmdType, WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5004 + (point) * 20);//速度
                CommonWrite((int)(position.Speed * Ratio), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5012 + (point) * 20);//推压力
                CommonWrite((int)(position.Pressure), WriteMsg);

                WriteMsg = string.Format("{0} 10 {1} 1", AddrNo, 5014 + (point) * 20);//推压距离
                CommonWrite((int)(position.PreDistance * Ratio), WriteMsg);
            }

            WriteMsg = string.Format("1 10 {0} 2", 5018 + (point) * 20);//下一步指令
            CommonWrite((int)position.Next, WriteMsg);
        }
        /// <summary>
        /// 读参数
        /// </summary>
        public void ReadParam(int point)
        {
            CheckComm();

            var position = Positions.FirstOrDefault(u => u.PointNo == point);
            if (position == null) return;

            var pNo = position.PointNo;
            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5000 + pNo * 20);//指令类型
            int cmdType = CommonRead<int>(WriteMsg);
            position.CmdType = (PClinderCmdType)cmdType;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5002 + pNo * 20);//位置
            position.Pos = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5004 + pNo * 20);//速度
            position.Speed = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5006 + pNo * 20);//加速度
            position.Acc = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5008 + pNo * 20);//减速度
            position.Dec = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5010 + pNo * 20);//定位区间
            position.Range = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5012 + pNo * 20);//推压力
            position.Pressure = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5014 + pNo * 20);//推压距离
            position.PreDistance = CommonRead<int>(WriteMsg) * 1.0 / Ratio;

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5016 + pNo * 20);//延时时间
            position.Delay = CommonRead<int>(WriteMsg);

            WriteMsg = string.Format("{0} 3 {1} 1", AddrNo, 5018 + (point) * 20);//下一步指令
            position.Next = CommonRead<int>(WriteMsg);
        }

        /// <summary>
        /// 读取点位
        /// </summary>
        /// <returns></returns>
        public int ReadPointNum()
        {
            string readMsg = "1 10 1437 2";
            return CommonRead<int>(readMsg);
        }

        /// <summary>
        /// 配置协议
        /// </summary>
        /// <param name="protocol"></param>
        public void SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType protocol)
        {
            // 如果已经存在的连接，则需要先端口连接
            if (Communication != null && Communication.IsConnected)
            {
                Communication.Close();
            }

            // 如果相等
            if (ProtocolType == protocol && Protocol != null)
            {
                return;
            }

            IProtocol retProtocol = null;
            switch (protocol)
            {
                case Enums.ProtocolType.ModbusRTU:
                    retProtocol = new ModbusRTU();
                    break;
                case Enums.ProtocolType.ModbusASCII:
                    retProtocol = new ModbusAscii();
                    break;
                case Enums.ProtocolType.ModbusTCP:
                    retProtocol = new ModbusTcp();
                    break;
                default:

                    break;
            }

            ProtocolType = protocol;

            // 真实协议
            Protocol = retProtocol;
        }

        /// <summary>
        /// 当前位置
        /// </summary>
        /// <returns></returns>
        public override double GetCurrentPos()
        {
            double pos = 0;
            pos = CommonRead<int>("01 04 16 01") / Ratio;
            return pos;
        }

        #region 导入和导出
        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            // 配置协议
            SetProtocol(ProtocolType);

        }

        #endregion

        #region 停止
        /// <summary>
        /// 停止
        /// </summary>
        public override void Stop()
        {
            CheckComm();

            // 防止未连接直接关闭软件报错
            try
            {
                if (Communication.IsConnected)
                {
                    string stopCommand = string.Format("{0} 05 {1} 1", AddrNo, 1405);//写
                    CommonWrite(false, stopCommand);
                    System.Threading.Thread.Sleep(12);
                    stopCommand = string.Format("{0} 05 {1} 1", AddrNo, 1405);//写
                    CommonWrite(true, stopCommand);
                }
            }
            catch (Exception ex)
            {
                OnLog(Common.DataStruct.Enums.LogType.Error, $"通讯失败:{ex.Message}");
            }

            // 中断已有循环
            stopReset?.Cancel();
        }

        #endregion
        /// <summary>
        /// 心跳检查
        /// </summary>
        /// <param name="commRate"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsHealth(out string connStr)
        {
            connStr = string.Empty;

            if (Communication != null)
            {
                connStr = Communication.ToString();
                return Communication.TryConnect();
            }

            return false;
        }

        public void AddPosionPosition(PosionSafeModel position)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 通信连接
        /// </summary>
        public string ConnectStr => Communication.ToString();
    }

    /// <summary>
    /// 0-无；1-回原点；2-延时；3-绝对运动；4-推压运 动；5-相对运动；6-闭环推压；7-重置力
    /// </summary>
    public enum PClinderCmdType
    {
        //[Description("无")]
        //None,

        //[Description("回原点")]
        //Home,

        //[Description("等待")]
        //Delay,

        [Description("绝对运动")]
        Abs = 3,

        [Description("推压运动")]
        Push,

        [Description("相对运动")]
        Rel = 5,

        [Description("闭环推压")]
        ClosePush = 6,

        [Description("重置力")]
        ResPre = 7
    }

    /// <summary>
    /// 点位
    /// </summary>
    public class PointPos : IXMLParser
    {
        /// <summary>
        /// 地址
        /// </summary>
        private const int ComAddr = 1408;

        /// <summary>
        /// 地址
        /// </summary>
        [Ignore]
        public int Address => ComAddr + PointNo;

        /// <summary>
        /// 点位编号
        /// </summary>
        public int PointNo { get; set; }

        /// <summary>
        /// 指令类型
        /// </summary>
        public PClinderCmdType CmdType { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public double Pos { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// 加速
        /// </summary>
        public double Acc { get; set; }

        /// <summary>
        /// 减速
        /// </summary>
        public double Dec { get; set; }

        /// <summary>
        /// 位置区间
        /// </summary>
        public double Range { get; set; }

        /// <summary>
        /// 推压力
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// 推压距离
        /// </summary>
        public double PreDistance { get; set; }

        /// <summary>
        /// 延时 ms
        /// </summary>
        public double Delay { get; set; }

        /// <summary>
        /// 下一步指令
        /// </summary>
        public int Next { get; set; }


        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml();
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }
    }
}