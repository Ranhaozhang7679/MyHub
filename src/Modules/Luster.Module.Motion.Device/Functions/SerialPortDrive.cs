using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public enum CommMethod
    {
        [Description("仅发送")]
        Send,

        [Description("发送并获取")]
        GetResult,
    }

    public enum BaudRateOption
    {
        [Description("9600")]
        _9600 = 9600,

        [Description("115200")]
        _115200 = 115200,

    }


    public enum ParityOption
    {
        [Description("NONE")]
        ParityNone = Parity.None,

        [Description("ODD")]
        ParityOdd = Parity.Odd,

        [Description("EVEN")]
        ParityEven = Parity.Even,

    }

    //dataBits
    public enum dataBitsOption
    {
        [Description("8")]
        db8 = 8,

        [Description("7")]
        db7 = 7,

    }

    //StopBits
    public enum StopBitsOption
    {
        [Description("ONE")]
        StopBitsOne = StopBits.One,

        [Description("TWO")]
        StopBitsTwo = StopBits.Two
    }

    public class SerialPortDrive : MotionFunction
    {

        //[NotEmpty]
        //[Parameter("串口号", 0, CN = "串口号", DefaultV = "COM6")]
        public string ComPort { get; set; }

        //[NotEmpty]
        //[Parameter("波特率", 1, CN = "波特率", DefaultV = BaudRateOption._9600)]
        public BaudRateOption BaudRates { get; set; }

        //[NotEmpty]
        //[Parameter("奇偶校验", 3, CN = "奇偶校验", DefaultV = ParityOption.ParityNone)]
        public ParityOption Paritys { get; set; }

        //[NotEmpty]
        //[Parameter("数据位", 4, CN = "数据位", DefaultV = dataBitsOption.db8)]
        public dataBitsOption DataBits { get; set; }

        //[NotEmpty]
        //[Parameter("停止位", 5, CN = "停止位", DefaultV = StopBitsOption.StopBitsOne)]
        public StopBitsOption StopBits { get; set; }


        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [NotEmpty]
        [Parameter("通讯类型", 1, CN = "通讯类型", DefaultV = CommMethod.GetResult)]
        public CommMethod Method { get; set; }

        [NotEmpty]
        [Parameter("发送内容", 2, CN = "发送内容")]
        public string Msg { get; set; }

        [Parameter("是否启用通讯超时或读取失败断连", 3, CN = "通讯超时或读取失败断连", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsAutoDisConnect { get; set; }

        [Parameter("规定时间内检查信号是否满足", 8, CN = "超时时间", DefaultV = 3000)]
        public int TimeOut { get; set; }

        [Parameter("判断返回值添加补偿", 9, CN = "结果补偿", CanRef = ParamRef.NoRef)]
        public SocketAction ReturnAddOffset { get; set; }


        [Parameter("返回结果", 9, CN = "返回结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool OutRst { get; set; }


        [Parameter("测量值", 10, CN = "测量值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutValue { get; set; }


        private SerialPort _serialPort;
        private bool IsOpen => _serialPort?.IsOpen ?? false;

        private StringBuilder _dataBuffer = new StringBuilder();    // 数据缓冲区
        private readonly object _bufferLock = new object();         // 缓冲区锁
        private readonly char _delimiter = '\r';                    // 结束符
        private AutoResetEvent dataRcvEvent = new AutoResetEvent(false);
        private VCommuncation comm;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SerialPortDrive()
        {
            this.Tips = "RS232";
            this.Icon = "\xe682";
        }


        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            OutRst = false;
            OutValue = 0.0;
            GetVDevice<VCommuncation>(CommDevice, out comm);
            comm.Open();
            Msg = Msg.Replace("\\r\\n", "\r\n");
            switch (Method)
            {
                case CommMethod.GetResult:
                    comm.Write(Msg);
                    ReceiveMessage(comm, IsAutoDisConnect);
                    break;
                case CommMethod.Send:
                    comm.Write(Msg);
                    break;
            }
            return true;
        }

        //public override void Pause()
        //{
        //    DisposePort();
        //}


        #region 不使用实现

        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="parity"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        private void InitSerialPort(string portName, BaudRateOption baudRate, ParityOption parity, dataBitsOption dataBits, StopBitsOption stopBits)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort(portName, (int)baudRate, (Parity)parity, (int)dataBits, (StopBits)stopBits)
                {
                    ReadTimeout = 3000,
                    WriteTimeout = 3000,
                    Encoding = Encoding.ASCII
                };
                _serialPort.DataReceived += SerialPort_DataRecived;
            }
        }


        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        private bool TryOpen()
        {
            string msg = string.Empty;
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }
                return true;
            }
            catch (UnauthorizedAccessException) { msg = "串口被占用"; }
            catch (IOException) { msg = "串口不存在"; }
            catch (ArgumentException) { msg = "串口名错误"; }
            catch (Exception ex) { msg = $"未知错误{ex.StackTrace}"; }
            return false;
        }


        private void DisposePort()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= SerialPort_DataRecived;
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        private bool SendDatas(string datas, int timeOut = 3000)
        {
            string msg = string.Empty;
            if (!_serialPort.IsOpen)
            {
                msg = "串口未打开!";
                return false;
            }

            try
            {
                datas = datas.Replace("\\r\\n", "\r\n");
                byte[] bytes = Encoding.ASCII.GetBytes(datas);
                _serialPort.Write(bytes, 0, bytes.Length);

                switch (Method)
                {
                    case CommMethod.GetResult:
                        if (dataRcvEvent.WaitOne(timeOut))
                        {
                            return true;
                        }
                        else
                        {
                            msg = "接收数据超时!";
                            return false;
                        }
                    case CommMethod.Send:
                        return true;
                }
            }
            catch { }
            return false;
        }


        private void SerialPort_DataRecived(object sender, SerialDataReceivedEventArgs e)
        {
            string msg = string.Empty;
            if (Method == CommMethod.Send)
                return;

            try
            {
                lock (_bufferLock)
                {
                    int bytesToRead = _serialPort.BytesToRead;
                    if (bytesToRead > 0)
                    {
                        byte[] buffer = new byte[bytesToRead];
                        _serialPort.Read(buffer, 0, bytesToRead);
                        string receivedData = Encoding.ASCII.GetString(buffer);
                        _dataBuffer.Append(receivedData);

                        // 检查是否包含完整的数据包
                        string bufferContent = _dataBuffer.ToString();
                        int delimiterIndex;
                        while ((delimiterIndex = bufferContent.IndexOf(_delimiter)) >= 0)
                        {
                            // 提取一个完整的数据包
                            string completeMessage = bufferContent.Substring(0, delimiterIndex + 1);
                            bufferContent = bufferContent.Substring(delimiterIndex + 1);

                            // 处理完整数据
                            (OutRst, OutValue) = DatasFormat(completeMessage.Trim());
                        }

                        _dataBuffer.Clear();
                    }
                }
            }
            catch (TimeoutException) { msg = "读取超时!"; }
            catch (Exception ex) { msg = $"未知错误={ex.StackTrace}"; }
        }

        // 基于帧头帧尾的协议处理
        private void ProcessDataWithProtocol()
        {
            lock (_bufferLock)
            {
                string data = _dataBuffer.ToString();

                // 示例：寻找完整的数据帧（以M开始，\r结束）
                int startIndex = data.IndexOf((char)0x4D); // M
                int endIndex = data.IndexOf((char)0x0D);   // \r

                while (startIndex >= 0 && endIndex > startIndex)
                {
                    // 提取完整帧
                    string frame = data.Substring(startIndex, endIndex - startIndex + 1);

                    // 处理数据
                    (OutRst, OutValue) = DatasFormat(frame);
                    dataRcvEvent.Set();

                    // 移除已处理的数据
                    _dataBuffer.Remove(0, endIndex + 1);
                    data = _dataBuffer.ToString();

                    // 寻找下一帧
                    startIndex = data.IndexOf((char)0x4D);
                    endIndex = data.IndexOf((char)0x0D);
                }
            }
        }


        #endregion

        /// <summary>
        /// 等待服务器返回数据
        /// </summary>
        /// <param name="comm"></param>
        private void ReceiveMessage(VCommuncation comm, bool IsAutoDisConnect)
        {
            string msg = string.Empty;
            if (Method == CommMethod.Send)
                return;

            try
            {
                lock (_bufferLock)
                {
                    // 通讯超时、读取为空时，不主动断开连接
                    string receivedData = comm.ReadSingle<string>("", TimeOut, isCloseConnect: IsAutoDisConnect);
                    _dataBuffer.Append(receivedData);
                    // 检查是否包含完整的数据包
                    string bufferContent = _dataBuffer.ToString();
                    int delimiterIndex;
                    while ((delimiterIndex = bufferContent.IndexOf(_delimiter)) >= 0)
                    {
                        // 提取一个完整的数据包
                        string completeMessage = bufferContent.Substring(0, delimiterIndex + 1);
                        bufferContent = bufferContent.Substring(delimiterIndex + 1);

                        // 处理完整数据
                        (OutRst, OutValue) = DatasFormat(completeMessage.Trim());
                    }

                    _dataBuffer.Clear();
                }
            }
            catch (TimeoutException) { msg = "读取超时!"; }
            catch (Exception ex) { msg = $"未知错误={ex.StackTrace}"; }
        }
       
        /// <summary>
        /// 激光测高返回数值处理
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private (bool, double) DatasFormat(string buffer)
        {
            try
            {
                if (string.IsNullOrEmpty(buffer))
                {
                    MyOwner.OnLog(LogType.Warning, $"232串口接收数据为空");
                    return (false, -9);//NG,返回值为空
                }
                else
                {
                    var data = buffer.Replace("\r", "");
                    if (data.Split(',').Count() > 1)
                    {
                        if (data.Split(',')[1].Contains("-FFFFFFF"))
                        {
                            MyOwner.OnLog(LogType.Warning, $"232串口接收数据超出测量范围");
                            return (false, -99);//NG，超出测量范围
                        }
                        else
                        {
                            return (true, Convert.ToDouble(data.Split(',')[1]) + getReturnAddOffset(comm));
                        }
                    }
                    else
                    {
                        MyOwner.OnLog(LogType.Warning, $"232串口接收数据格式错误，数据是：{buffer}");
                        return (false, -999);//NG，返回值格式错误
                    }

                }
            }
            catch (Exception ex)
            {
                return (false, -9999);//NG,异常={ex.StackTrace}
            }
        }

        /// <summary>
        /// 获取返回值补偿
        /// </summary>
        /// <param name="comm"></param>
        /// <returns></returns>
        private double getReturnAddOffset(VCommuncation comm)
        {
            double returnAddOffset = 0.0;
            for (int i = 0; i < comm.Actions.Count; i++)
            {
                if (comm.Actions[i].Name == ReturnAddOffset.Name)
                {
                    double.TryParse(comm.Actions[i].Value, out returnAddOffset);
                    break;
                }
            }
            return returnAddOffset;
        }
    }
}
