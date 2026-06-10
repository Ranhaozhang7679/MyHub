
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace TaiKeCommon
{
    public class ElectricScrewDrivers : IDisposable
    {
        public event Action<string> LogEvent;  //日志事件     
        public Action<TaiKeData> DataUpdated;
        public Action<bool> StopPressSet;
        public int ComType = 1;  // 1-网口和串口 2-网络 3-串口
        public string ProductSn { get; set; } = "sn";
        public TaiKeData TorqueData;

        public string ScrewName = ""; // 螺丝编号

        private string ScrewNumber = "1";

        private string dirStationID = ""; // 站点ID
        private string dirStationType;

        #region 构造函数
        public ElectricScrewDrivers()
        {
            //rootSaveDir = "";
            Task.Factory.StartNew(Timer100Tick);
            Task.Factory.StartNew(Timer20Tick);
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="chartTorqueTime"></param>
        /// <param name="tbxs_t"></param>
        /// <param name="chartTorqueAngle"></param>
        /// <param name="tbxs_a"></param>
        /// <param name="chartTorqueTimeParent"></param>
        /// <param name="chartTorqueAngleParent"></param>
        public void SetChartControl(Dispatcher dispatcher, CartesianChart chartTorqueTime, TextBlock[] tbxs_t, CartesianChart chartTorqueAngle,
                                     TextBlock[] tbxs_a, FrameworkElement chartTorqueTimeParent, FrameworkElement chartTorqueAngleParent)
        {
            this.dispatcher = dispatcher;
            this.chartTorqueTime = chartTorqueTime;
            this.tbxs_t = tbxs_t;
            this.chartTorqueTimeParent = chartTorqueTimeParent;
            this.chartTorqueAngle = chartTorqueAngle;
            this.tbxs_a = tbxs_a;
            this.chartTorqueAngleParent = chartTorqueAngleParent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="chartToeinForce"></param>
        /// <param name="chartToeinForceParent"></param>
        public void SetToeinChartControl(Dispatcher dispatcher, CartesianChart chartToeinForce, FrameworkElement chartToeinForceParent)
        {
            this.dispatcher = dispatcher;
            this.chartToeinForce = chartToeinForce;
            this.chartToeinForceParent = chartToeinForceParent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootDir"></param>
        public void SetRootDir(string rootDir)
        {
            if (!rootDir.EndsWith("\\"))
            {
                rootSaveDir = rootDir + "\\";
            }
            else
            {
                rootSaveDir = rootDir;
            }

            string time = DateTime.Now.ToString("yyyyMMdd");
            string path = $"D:\\TaiKeScrewDatas\\{time}\\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            dirSourceData = $"{rootSaveDir}STD1\\OriginalData\\";
            dirPRMData = $"{rootSaveDir}STD1\\PRM\\";
            dirTCRData = $"{rootSaveDir}STD1\\TCR\\";
            dirCurveTorqueTimeData = $"{rootSaveDir}STD1\\Curve\\Press_Time\\";
            dirCurveTorqueAngleData = $"{rootSaveDir}STD1\\Curve\\Torque_Angle\\";
            dirPressData = $"{rootSaveDir}STD1\\Press\\";
            dirTimeTorqueData = $"{rootSaveDir}STD1\\TimeTorque\\";
            dirCurveToeinXForceData = $"{rootSaveDir}STD1\\Curve\\Toein_X_Force\\";
            dirCurveToeinYForceData = $"{rootSaveDir}STD1\\Curve\\Toein_Y_Force\\";
            dirCurveToeinZForceData = $"{rootSaveDir}STD1\\Curve\\Toein_Z_Force\\";
            dirTimeTorAngPreData = $"{rootSaveDir}STD1\\TimeTorAngPre\\";
        }

        public void SetScrewName(string screwName)
        {
            ScrewName = screwName;
            ScrewNumber = screwName.Substring(0, 1);
        }

        public void SetDirStationID(string dirStationID,string dirStationType)
        {
            this.dirStationID = dirStationID;
            this.dirStationType=dirStationType;
        }

        /// <summary>
        /// 设置使用的通讯类型
        /// </summary>
        /// <param name="useComType"></param>
        public void SetUseComType(int useComType)
        {
            ComType = useComType;
        }

        #endregion

        string data_ProcessMonitorTK = "";          // 过程监控数据
        string data_TorqueCurveTK = "";             // 扭矩曲线数据  
        string data_PM_TC_TK = "";                  // 过程监控+扭矩曲线数据
        int tkRecvCount = 1000;                     // 接收数据点个数  

        #region 网口通信-数据接收

        public string Ip = "127.0.0.1";
        public int Port = 5000;
        public int tkDataMode = 0;                  // 0:实时模式，1：阅读模式               
        public bool bConnect = true;                // 客户端允许连接标志
        bool bClientConnected = false;              // 客户端已连接标志
        byte[] bufRec = new byte[1024 * 1024];      // 客户端接收缓存数据       
        byte[] tempb = new byte[50000];             // 存放客户端的全部字节数据 
        Socket clientScoket;                        // 套接字接口
        Thread mThread;                             // 定义线程
        int netCount = 0;                           // 接收的字节总数

        /// <summary>
        /// 获取电批网络连接状态
        /// </summary>
        /// <returns></returns>
        public bool GetNetConnect()
        {
            if (clientScoket != null)
            {
                return clientScoket.Connected;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 打开电批网络
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool OpenNetController(string ip, int port)
        {
            Ip = ip;
            Port = port;
            return OpenNetController();
        }

        /// <summary>
        /// 电批网络连接
        /// </summary>
        /// <returns></returns>
        public bool OpenNetController()
        {
            bConnect = true;
            Thread_open();       //打开电批客户端

            int count = 200;
            while (!bClientConnected)
            {
                Thread.Sleep(10);
                if (count-- <= 0)
                {
                    LogEvent?.Invoke("打开电批客户端失败");
                    break;
                }
            }
            return bClientConnected;
        }

        /// <summary>
        /// 关闭电批及客户端线程
        /// </summary>
        /// <returns></returns>
        public bool CloseNetController()
        {
            if (mThread != null && mThread.IsAlive)
            {
                mThread.Abort();
            }

            if (clientScoket != null && clientScoket.Connected)
            {
                try
                {
                    clientScoket.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(100);
                    clientScoket.Close();
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke($"关闭电批及客户端线程异常：{ex.Message}");
                    Console.WriteLine(ex.Message);
                }
            }
            clientScoket = null;
            return false;
        }

        /// <summary>
        /// 打开电批客户端线程
        /// </summary>
        private void Thread_open()
        {
            if (mThread != null && mThread.IsAlive)
            {
                mThread.Abort();
                Thread.Sleep(200);
            }
            mThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (bConnect)
                    {
                        bConnect = false;
                        ConnectController();
                    }
                }
            }));
            mThread.Start();
        }

        /// <summary>
        /// 网口通信连接 开始监听
        /// </summary>
        private void ConnectController()
        {
            try
            {
                // 如果已经连接上了，先断开
                if (clientScoket != null && clientScoket.Connected)
                {
                    clientScoket.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(100);
                    clientScoket.Close();
                }
                clientScoket = null;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"电批客户端断开连接异常: {ex.Message}");
            }

            try
            {
                clientScoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint m_IPEndPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
                clientScoket.Connect(m_IPEndPoint);
                if (clientScoket.Connected)
                {
                    clientScoket.BeginReceive(bufRec, 0, bufRec.Length, SocketFlags.None, new AsyncCallback(receiveController), clientScoket);
                    bClientConnected = true;
                }
                else
                {
                    LogEvent?.Invoke($"电批客户端连接失败");
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"电批客户端连接失败: {ex.Message}");

                if (bClientConnected)  //前面已连接过情况下
                {
                    LogEvent?.Invoke($"电批客户端已离线");
                    Debug.WriteLine("电批客户端已离线");
                    bClientConnected = false;
                }
            }
        }

        /// <summary>
        /// 网口通信数据接收
        /// </summary>
        /// <param name="ia"></param>
        private void receiveController(IAsyncResult ia)
        {
            try
            {
                clientScoket = ia.AsyncState as Socket;
                if (clientScoket != null && clientScoket.Connected)
                {
                    int receiveCount = clientScoket.EndReceive(ia);   //锁付数据是分多次发过来的，这是一次缓存触发时的字节数量
                    if (receiveCount > 0)
                    {
                        if (ComType == 1)  // 网络+串口通讯
                        {
                            byte[] data = new byte[receiveCount];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = bufRec[i];
                            }
                            //阅读模式************************************************************
                            if (netCount < (tkRecvCount * 4 + 46))
                            {
                                for (int i = 0; i < receiveCount; i++)
                                    tempb[netCount + i] = data[i];         //加入新接收的数据
                                netCount += receiveCount;                   //使用+=的原因是byRead信息個數是分段進來的
                            }
                            if (netCount >= (tkRecvCount * 4 + 46))
                            {
                                torq_update(tempb, netCount);   //接收完原始数据后转到二次处理
                                TKnetDataEnter = true;
                                netCount = 0;
                            }
                            clientScoket.BeginReceive(bufRec, 0, bufRec.Length, SocketFlags.None, new AsyncCallback(receiveController), clientScoket);
                        }
                        else if (ComType == 2)  // 仅网络通讯
                        {
                            #region 忽略
                            //string receivedData = Encoding.ASCII.GetString(bufRec, 0, receiveCount);
                            //data_PM_TC_TK += receivedData;
                            //阅读模式************************************************************
                            //int index_TK = 0;          //检测关键字$60的位置
                            //if ((index_TK = data_PM_TC_TK.IndexOf("$60")) > -1 && data_PM_TC_TK.Contains("\r\n"))
                            //{
                            //    data_PM_TC_TK = data_PM_TC_TK.Substring(index_TK);     //保证开头为$60
                            //    int newlineIndex = data_PM_TC_TK.IndexOf('\n');
                            //    // 如果没有找到 \n，返回0
                            //    int recCount = newlineIndex >= 0 ? data_PM_TC_TK.Substring(newlineIndex + 1).Length : 0;
                            //    //阅读模式************************************************************
                            //    if (netCount < (tkRecvCount * 4 + 46))
                            //    {
                            //        netCount += recCount;                    //使用+=的原因是byRead信息個數是分段進來的
                            //    }
                            //    //接收完所有数据后处理
                            //    if (netCount >= (tkRecvCount * 4 + 46))
                            //    {
                            //        //2025-11-12*****************************************************************************************
                            //        try
                            //        {
                            //            string[] temp = data_PM_TC_TK.Split('\r');
                            //            //过程监控数据
                            //            data_ProcessMonitorTK = temp[0];   //去掉换行符 
                            //            arrProcessMonitorTK = data_ProcessMonitorTK.Split(',');            //以‘，’分隔成数组                                      
                            //            data_ProcessMonitorTK = "";
                            //            //扭矩曲线数据
                            //            data_TorqueCurveTK = temp[1].Trim('\n');
                            //            byte[] bTorqueCurveTK = Encoding.UTF8.GetBytes(data_TorqueCurveTK);
                            //            torq_update(bTorqueCurveTK, bTorqueCurveTK.Length);   //接收完原始数据后转到二次处理
                            //            data_TorqueCurveTK = "";
                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            LogEvent?.Invoke($"接收数据异常: {ex.ToString()},IP：{Ip}");
                            //            Debug.WriteLine("接收数据异常");
                            //        }
                            //        //****************************************************************************************************                
                            //        TKnetDataEnter = true;
                            //        data_ProcessMonitorTK = "";
                            //        data_TorqueCurveTK = "";
                            //        data_PM_TC_TK = "";
                            //        netCount = 0;
                            //    }
                            //}
                            #endregion
                            // 将接收到的字节数据添加到缓冲区
                            byte[] receivedBytes = new byte[receiveCount];
                            Array.Copy(bufRec, 0, receivedBytes, 0, receiveCount);
                            ProcessNetworkData(receivedBytes);
                            if (!TKnetDataEnter)
                            {
                                //未接收完就继续接收数据
                                clientScoket.BeginReceive(bufRec, 0, bufRec.Length, SocketFlags.None, new AsyncCallback(receiveController), clientScoket);
                            }
                        }
                    }
                    else if (receiveCount == 0)
                    {
                        bConnect = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "远程主机强迫关闭了一个现有的连接。" ||
                    ex is ObjectDisposedException ||
                    ex is SocketException)
                {
                    bConnect = true;
                    // 确保资源释放
                    clientScoket?.Close();
                    clientScoket?.Dispose();
                    clientScoket = null;
                }
                else
                {
                    Debug.WriteLine(ex.ToString());
                }
                LogEvent?.Invoke($"电批1客户端接收数据异常: {ex.Message}");
            }
        }

        // 处理网络数据的方法
        private void ProcessNetworkData(byte[] newData)
        {
            try
            {
                // 将新数据添加到累积缓冲区
                if (dataBuffer == null)
                    dataBuffer = new List<byte>();
                dataBuffer.AddRange(newData);

                // 处理缓冲区中所有完整的数据包
                while (ProcessCompletePacket()) { }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"处理网络数据异常: {ex.Message}");
            }
        }

        // 尝试处理一个完整的数据包
        private bool ProcessCompletePacket()
        {
            if (dataBuffer == null || dataBuffer.Count == 0)
                return false;

            // 1. 查找数据包起始标记 "$60"
            int startIndex = FindPattern(dataBuffer.ToArray(), "$60");
            if (startIndex < 0)
            {
                // 没有找到起始标记，清空无效数据
                if (dataBuffer.Count > 1024) // 防止缓冲区无限增长
                    dataBuffer.Clear();
                return false;
            }

            // 移除起始标记前的无效数据
            if (startIndex > 0)
                dataBuffer.RemoveRange(0, startIndex);

            // 2. 查找过程监控数据的结束标记 "\r\n"
            int pmEndIndex = FindPattern(dataBuffer.ToArray(), "\r\n");
            if (pmEndIndex < 0)
            {
                // 过程监控数据不完整，等待更多数据
                return false;
            }

            // 过程监控数据的结束位置（包含\r\n）
            int pmEndPosition = pmEndIndex + 2;

            // 3. 计算总数据长度
            int torqueDataLength = tkRecvCount * 4 + 46;
            int totalExpectedLength = pmEndPosition + torqueDataLength;

            if (dataBuffer.Count < totalExpectedLength)
            {
                // 数据不完整，等待更多数据
                return false;
            }

            // 4. 数据完整，开始处理
            ProcessPacket(pmEndPosition, torqueDataLength);

            // 5. 从缓冲区中移除已处理的数据
            dataBuffer.RemoveRange(0, totalExpectedLength);

            // 6. 如果缓冲区还有数据，继续尝试处理
            return dataBuffer.Count > 0;
        }

        // 处理完整的数据包
        private void ProcessPacket(int pmEndPosition, int torqueDataLength)
        {
            try
            {
                // 提取过程监控数据（ASCII字符串）
                byte[] pmBytes = dataBuffer.GetRange(0, pmEndPosition).ToArray();
                string processMonitorData = Encoding.ASCII.GetString(pmBytes);

                // 提取扭矩曲线数据（二进制）
                byte[] torqueBytes = dataBuffer.GetRange(pmEndPosition, torqueDataLength).ToArray();

                // 处理过程监控数据
                if (!string.IsNullOrEmpty(processMonitorData))
                {
                    // 去掉末尾的\r\n
                    if (processMonitorData.EndsWith("\r\n"))
                        processMonitorData = processMonitorData.Substring(0, processMonitorData.Length - 2);

                    data_ProcessMonitorTK = processMonitorData;
                    arrProcessMonitorTK = data_ProcessMonitorTK.Split(',');
                }

                // 处理扭矩曲线数据
                if (torqueBytes.Length == torqueDataLength)
                {
                    torq_update(torqueBytes, torqueBytes.Length);
                }
                else
                {
                    LogEvent?.Invoke($"扭矩数据长度不正确，期望:{torqueDataLength}，实际:{torqueBytes.Length}");
                }

                TKnetDataEnter = true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"处理数据包异常: {ex.Message}");
            }
        }

        // 在字节数组中查找模式字符串（辅助方法）
        private int FindPattern(byte[] data, string pattern)
        {
            if (data == null || string.IsNullOrEmpty(pattern))
                return -1;

            byte[] patternBytes = Encoding.ASCII.GetBytes(pattern);

            for (int i = 0; i <= data.Length - patternBytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < patternBytes.Length; j++)
                {
                    if (data[i + j] != patternBytes[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return i;
            }

            return -1;
        }

        // 新增类成员变量
        private List<byte> dataBuffer; // 数据累积缓冲区

        #endregion

        #region 串口通信-数据接收

        string[] arrProcessMonitorTK = new string[44];  //电批过程监控数据
        string optID_com = "999";                       //操作序号（串口）
        SerialPort serialPort = null;
        int countPinChanged1;
        int comCount = 0;

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns></returns>
        public bool OpenSerialPort()
        {
            try
            {
                serialPort.Open();   //打开1#电批com口
                return true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"打开电批com口异常：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns></returns>
        public bool CloseSerialPort()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"关闭电批com口异常：{ex.Message}");
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <returns></returns>
        public bool OpenSerialPort(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            if (serialPort == null)
            {
                serialPort = new SerialPort();
                serialPort.DataReceived += serialPort_DataReceived;
                serialPort.PinChanged += serialPort_PinChanged;
            }
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.DataBits = dataBits;
            serialPort.Parity = parity;
            serialPort.StopBits = stopBits;

            return OpenSerialPort();
        }

        /// <summary>
        /// 串口接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string rec = serialPort.ReadExisting();   //读取缓存数据

            if (ComType == 1)
            {
                data_ProcessMonitorTK += rec;
                ProcessType1Data();
            }
            else if (ComType == 3)
            {
                data_PM_TC_TK += rec;
                ProcessType3Data();
            }

            //if (ComType == 1)
            //    data_ProcessMonitorTK = data_ProcessMonitorTK + rec;
            //else if (ComType == 3)
            //    data_PM_TC_TK = data_PM_TC_TK + rec;


            //int index_TK = 0;          //检测关键字$60的位置
            ////"$60"开头，"\r\n"结束
            //if (ComType == 1 && (index_TK = data_ProcessMonitorTK.IndexOf("$60")) > -1 && data_ProcessMonitorTK.Contains("\r\n"))
            //{
            //    data_ProcessMonitorTK = data_ProcessMonitorTK.Substring(index_TK);     //保证开头为$60
            //    string[] temp = data_ProcessMonitorTK.Split('\r');
            //    data_ProcessMonitorTK = temp[0];   //去掉换行符
            //    arrProcessMonitorTK = data_ProcessMonitorTK.Split(',');            //以‘，’分隔成数组
            //    optID_com = arrProcessMonitorTK[1];
            //    data_ProcessMonitorTK = "";

            //    TKcomDataEnter = true;

            //    if (driverOptTime < 6)
            //    {
            //        LogEvent?.Invoke($"电批操作间隔时间过短: {driverOptTime}");
            //        Debug.WriteLine("电批操作间隔时间过短");
            //    }
            //    driverOptTime = 0;
            //}
            ////"$60"开头，"\r\n"过程监控数据接收结束，继续接收扭矩曲线数据要接收4046字节
            //else if (ComType == 3 && (index_TK = data_PM_TC_TK.IndexOf("$60")) > -1 && data_PM_TC_TK.Contains("\r\n"))
            //{
            //    data_PM_TC_TK = data_PM_TC_TK.Substring(index_TK);     //保证开头为$60
            //    int newlineIndex = data_PM_TC_TK.IndexOf('\n');
            //    // 如果没有找到 \n，返回0
            //    int receiveCount = newlineIndex >= 0 ? data_PM_TC_TK.Substring(newlineIndex + 1).Length : 0;
            //    //阅读模式************************************************************
            //    if (comCount < (tkRecvCount * 4 + 46))
            //    {
            //        comCount += receiveCount;                    //使用+=的原因是byRead信息個數是分段進來的
            //    }
            //    //接收完所有数据后处理
            //    if (comCount >= (tkRecvCount * 4 + 46))
            //    {
            //        //2025-11-12*****************************************************************************************
            //        try
            //        {
            //            string[] temp = data_PM_TC_TK.Split('\r');
            //            //过程监控数据
            //            data_ProcessMonitorTK = temp[0];   //去掉换行符 
            //            arrProcessMonitorTK = data_ProcessMonitorTK.Split(',');            //以‘，’分隔成数组
            //            data_ProcessMonitorTK = "";
            //            //扭矩曲线数据
            //            data_TorqueCurveTK = temp[1].Trim('\n');
            //            byte[] bTorqueCurveTK = Encoding.UTF8.GetBytes(data_TorqueCurveTK);
            //            torq_update(bTorqueCurveTK, bTorqueCurveTK.Length);   //接收完原始数据后转到二次处理
            //            data_TorqueCurveTK = "";
            //        }
            //        catch (Exception ex)
            //        {
            //            LogEvent?.Invoke($"接收数据异常: {ex.ToString()},端口号：{serialPort.PortName}");
            //            Debug.WriteLine("接收数据异常");
            //        }
            //        //****************************************************************************************************                
            //        TKcomDataEnter = true;
            //        data_ProcessMonitorTK = "";
            //        data_TorqueCurveTK = "";
            //        data_PM_TC_TK = "";
            //        comCount = 0;
            //    }
            //}
        }

        private void ProcessType1Data()
        {
            // "进程监控数据"处理逻辑
            int index_TK = data_ProcessMonitorTK.IndexOf("$60");
            if (index_TK > -1 && data_ProcessMonitorTK.Contains("\r\n"))
            {
                data_ProcessMonitorTK = data_ProcessMonitorTK.Substring(index_TK);
                string[] temp = data_ProcessMonitorTK.Split('\r');
                data_ProcessMonitorTK = temp[0];
                arrProcessMonitorTK = data_ProcessMonitorTK.Split(',');
                optID_com = arrProcessMonitorTK[1];
                data_ProcessMonitorTK = "";

                TKcomDataEnter = true;
                HandleDriverOptTime();
            }
        }

        private void ProcessType3Data()
        {
            // "扭矩曲线数据"处理逻辑
            int index_TK = data_PM_TC_TK.IndexOf("$60");
            if (index_TK <= -1 || !data_PM_TC_TK.Contains("\r\n"))
                return;

            // 找到完整的过程监控数据行
            int newlineIndex = data_PM_TC_TK.IndexOf('\n', index_TK);
            if (newlineIndex <= 0)
                return;

            // 分离过程监控数据和扭矩曲线数据
            string processData = data_PM_TC_TK.Substring(index_TK, newlineIndex - index_TK + 1);
            string torqueData = data_PM_TC_TK.Substring(newlineIndex + 1);

            // 处理过程监控数据
            ProcessProcessData(processData);

            // 处理扭矩曲线数据
            ProcessTorqueData(torqueData);
        }

        private void ProcessProcessData(string processData)
        {
            try
            {
                // 清理过程监控数据
                processData = processData.TrimEnd('\r', '\n');
                arrProcessMonitorTK = processData.Split(',');
                if (arrProcessMonitorTK.Length > 1)
                {
                    optID_com = arrProcessMonitorTK[1];
                }

                HandleDriverOptTime();
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"处理过程监控数据异常: {ex.Message}, 数据: {processData}");
                Debug.WriteLine($"处理过程监控数据异常: {ex.Message}");
            }
        }

        private void ProcessTorqueData(string torqueData)
        {
            const int TORQUE_DATA_LENGTH = 4046; // 1000*4 + 46

            // 检查是否已累积足够的扭矩数据
            if (torqueData.Length < TORQUE_DATA_LENGTH)
            {
                // 数据不足，等待下次接收
                // 可以在这里保存部分数据到缓冲区，但原代码似乎用全局变量处理
                // 这里返回，数据会继续累积到 data_PM_TC_TK 中
                return;
            }

            try
            {
                // 取前4046个字符作为扭矩曲线数据
                string completeTorqueData = torqueData.Substring(0, TORQUE_DATA_LENGTH);

                // 剩余的字节保存起来，可能是下一条消息的开始
                string remainingData = torqueData.Length > TORQUE_DATA_LENGTH ?
                    torqueData.Substring(TORQUE_DATA_LENGTH) : "";
                data_PM_TC_TK = remainingData;

                // 处理扭矩曲线数据
                byte[] bTorqueCurveTK = Encoding.UTF8.GetBytes(completeTorqueData);
                torq_update(bTorqueCurveTK, bTorqueCurveTK.Length);
                if (ComType == 2)
                    TKnetDataEnter = true;
                else if (ComType == 3)
                    TKcomDataEnter = true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"处理扭矩曲线数据异常: {ex.Message}, 端口号：{serialPort.PortName}");
                Debug.WriteLine("处理扭矩曲线数据异常");
                // 清空数据，避免错误累积
                data_PM_TC_TK = "";
            }
        }

        private void HandleDriverOptTime()
        {
            if (driverOptTime < 6)
            {
                LogEvent?.Invoke($"电批操作间隔时间过短: {driverOptTime}");
                Debug.WriteLine("电批操作间隔时间过短");
            }
            driverOptTime = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_PinChanged(object sender, SerialPinChangedEventArgs e)   //离线检测
        {
            countPinChanged1++;
            bool comOfflineCheck = serialPort.CDHolding;
            if (!comOfflineCheck && countPinChanged1 % 2 == 0)  //一次掉线会触发两次，只取一次
            {
                LogEvent?.Invoke("电批1串口已离线");
                Debug.WriteLine("电批1串口已离线！");
                countPinChanged1 = 0;
            }
        }

        #endregion

        #region 数据接收完成后数据处理
        //======================================================================================================
        byte[] originalData;                             // 截取原始数据
        double[] torq_double = new double[200];          // 电批已算好的扭力  
        double[] Angle_double = new double[200];         // 电批已算好的角度
        public double Max_Torque;                        // 电批最大扭力
        public double Max_Angle;                         // 电批最大角度       
        string titleAxisY = "Torque / kgf.cm";           // Y轴标题       
        int indexSSL = 0;                                // SSL位置
        int optID_net = -1;                              // 操作序号（网络）

        /// <summary>
        /// 接收完原始数据后转到二次处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        private void torq_update(byte[] data, int count)
        {
            originalData = new byte[count];
            for (int i = 0; i < count; i++)
            {
                originalData[i] = data[i];      //截取原始数据
            }
            //保存电批原始数据
            Update_OriginalData(originalData);
            //阅读模式数据解析
            DataConvert_TCR(data, ref torq_double, ref Angle_double, tkRecvCount, ref titleAxisY, ref indexSSL, ref optID_net);

            int length = torq_double.Length;
            double[] tempTorque = new double[length];
            double[] tempAngle = new double[length];
            for (int i = 0; i < length; i++)
            {
                tempTorque[i] = Math.Abs(torq_double[i]);  //取扭力绝对值
                tempAngle[i] = Math.Abs(Angle_double[i]);  //取角度绝对值
            }
            Max_Torque = Math.Round(tempTorque.Max(), 4);  //求最大扭力
            Max_Angle = Math.Round(tempAngle.Max(), 1);    //求最大角度           
        }

        /// <summary>
        /// 电批阅读模式数据解析
        /// </summary>
        /// <param name="data"></param>         //原始字节数据
        /// <param name="torq_double"></param>  //扭力数组
        /// <param name="Angle_double"></param> //角度数组
        /// <param name="count"></param>        //原始字节数据个数
        /// <param name="titleAxisY"></param>   //Y轴坐标名称
        /// <param name="indexSSL"></param>     //着座点位置
        /// <param name="optID"></param>        //锁付编号
        /// <returns></returns>
        private bool DataConvert_TCR(byte[] data, ref double[] torq_double, ref double[] Angle_double, int TK_dataLen, ref string titleAxisY, ref int indexSSL, ref int optID)   //阅读模式数据解析
        {
            //int TK_dataLen = (count - 46) / 4;        //扭力采集点的个数
            int[] torq_data = new int[TK_dataLen];
            int[] Angle_data = new int[TK_dataLen];
            double[] torq_double_1 = new double[TK_dataLen];
            double[] Angle_double_1 = new double[TK_dataLen];
            byte[] HeaderBlock = new byte[16];    //头块
            byte[] FooterBlock = new byte[20];    //尾块

            if ((data[0] == 0xAA) && (data[1] == 0xB1) || (data[0] == 0x3F) && (data[1] == 0x3F))
            {
                for (int i = 0; i < 16; i++)
                    HeaderBlock[i] = data[i + 9];              //头块数据
                for (int i = 0; i < FooterBlock.Length; i++)
                {
                    FooterBlock[i] = data[i + 4 * TK_dataLen + 25];           //尾块数据
                }

                int y = data[10] >> 4;     //字节右移4位取高位
                int multiple = Convert.ToInt32(Math.Pow(10, y));//数值倍率，控制小数点
                string unitAxisY = "kgf.cm";             //Y轴单位
                y = data[10] & 15;         //和00001111按位与取低位
                switch (y)                  //获取扭力单位
                {
                    case 0:
                        unitAxisY = "Unknown";
                        break;
                    case 1:
                        unitAxisY = "gf.cm";
                        break;
                    case 2:
                        unitAxisY = "kgf.cm";
                        break;
                    case 3:
                        unitAxisY = "kgf.m";
                        break;
                    case 4:
                        unitAxisY = "mN.m";
                        break;
                    case 5:
                        unitAxisY = "cN.m";
                        break;
                    case 6:
                        unitAxisY = "N.m";
                        break;
                    case 7:
                        unitAxisY = "ozf.in";
                        break;
                    case 8:
                        unitAxisY = "lbf.in";
                        break;
                    case 9:
                        unitAxisY = "lbf.ft";
                        break;
                    default:
                        break;
                }
                titleAxisY = "Torque / " + unitAxisY;       //更改Y轴标题

                int AngC1 = data[21] * 256 + data[22];    //角度计算系数1
                int AngC2 = data[23];                     //角度计算系数2
                int CA_start = 0;                         //找出扭力大于0时的位置
                double AngleNumber = 0;                   //计算角度的临时值
                string ScrewStatusStart = "";             //找到不为0的扭力标志
                for (int i = 0; i < TK_dataLen; i++)
                {
                    torq_data[i] = data[2 * i + 25] * 256 + data[2 * i + 26];

                    if (torq_data[i] > 32767)    //按负数处理
                    {
                        torq_data[i] = torq_data[i] - 65536;
                    }
                    torq_double_1[i] = (double)(torq_data[i]) / multiple;


                    Angle_data[i] = data[2 * i + 2 * TK_dataLen + 25] * 256 + data[2 * i + 2 * TK_dataLen + 26];
                    if (Angle_data[i] > 32767)    //按负数处理
                    {
                        Angle_data[i] = Angle_data[i] - 65536;
                    }
                    AngleNumber = AngleNumber + (double)(Angle_data[i]);
                    Angle_double_1[i] = AngleNumber * AngC1 / Math.Pow(10, AngC2);

                    if (torq_double_1[i] != 0 && ScrewStatusStart == "")   //去掉初段为0的部分
                    {
                        CA_start = i;
                        ScrewStatusStart = "Start";
                    }
                }

                indexSSL = data[11] * 256 + data[12];  //SSL位置
                if (indexSSL != 0)
                    indexSSL = indexSSL - CA_start + 1;
                if (indexSSL < 0)
                    indexSSL = 0;

                optID = data[15] * 256 + data[16];                   //操作编号
                int length = TK_dataLen - CA_start + 1;
                torq_double = new double[length];                     //去掉初段为0的部分后重新整理
                Angle_double = new double[length];
                for (int i = 1; i < length; i++)
                {
                    torq_double[i] = torq_double_1[i - 1 + CA_start];
                    Angle_double[i] = Angle_double_1[i - 1 + CA_start];
                    Angle_double[i] = Math.Round(Angle_double[i], 2);  //角度保留2位小数
                }
            }
            return true;
        }

        /// <summary>
        /// 网口读取的原始数据保存csv
        /// </summary>
        /// <param name="data"></param>
        private void Update_OriginalData(byte[] data)
        {
            string fileName = ProductSn.Trim() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            string logPath = dirSourceData + fileName + ".log";
            FileInfo fi = new FileInfo(logPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            StreamWriter sw = new StreamWriter(logPath, true);
            int length = data.Length;
            for (int i = 0; i < length; i++)
            {
                if ((i != 0) && (i % 100 == 0))   //每隔100换行
                {
                    sw.WriteLine("");
                }
                sw.Write(data[i].ToString("X2") + " ");    //X表示按16进制，长度2
            }
            sw.Close();
        }
        #endregion

        #region 绘制曲线

        // 绘制图表的控件
        public CartesianChart chartTorqueTime;
        public TextBlock[] tbxs_t;
        public FrameworkElement chartTorqueTimeParent;

        public CartesianChart chartTorqueAngle;
        public TextBlock[] tbxs_a;
        public FrameworkElement chartTorqueAngleParent;
        Dispatcher dispatcher = null;

        // 绘制Toein Force的控件
        public CartesianChart chartToeinForce;
        public FrameworkElement chartToeinForceParent;

        public int tkDataInterval = 2;              // 采样时间间隔(ms)      
        TorqueChart myChart_T = new TorqueChart();  // 电批扭力对时间绘图实例化       
        TorqueChart myChart_A = new TorqueChart();  // 电批扭力对角度绘图实例化
        TorqueChart myChart_P = new TorqueChart();
        /// <summary>
        /// 绘制所有曲线
        /// </summary>
        /// <param name="time"></param>
        /// <param name="press"></param>
        public void UpdateTotalCurve(double[] time, double[] press)
        {
            var minestTime = time.FirstOrDefault();
            var timeList = time.Select(x => x - minestTime).ToArray();
            press = press.Select(x => x <= 0 ? 0 : x).ToArray();

            // 筛选时间小于 5000 的项            
            var filteredTime = timeList.Where(t => t < 5000).ToArray();
            var indices = timeList.Select((value, index) => new { value, index })
                              .Where(x => x.value < 5000)
                              .Select(x => x.index)
                              .ToArray();
            // 根据筛选出的索引筛选压力值
            var filteredPress = indices.Select(i => press[i]).ToArray();

            // 对 filteredTime 做非0加300，并确保首元素为0
            var timeListAdjusted = filteredTime.ToList();
            for (int i = 0; i < timeListAdjusted.Count; i++)
            {
                if (timeListAdjusted[i] != 0)
                {
                    timeListAdjusted[i] += 300;
                }
            }
            if (timeListAdjusted.Count > 0 && timeListAdjusted[0] != 0)
            {
                timeListAdjusted.Insert(0, 0);
                var pressList = filteredPress.ToList();
                pressList.Insert(0, 0);
                filteredPress = pressList.ToArray();
            }
            filteredTime = timeListAdjusted.ToArray();

            if (dispatcher != null)
            {
                dispatcher.Invoke(new Action(() =>
                {
                    myChart_T.DrawTotalCurve(
                        chartTorqueTime,
                        tbxs_t,
                        tkDataInterval,
                        0,
                        arrProcessMonitorTK,
                        Max_Torque,
                        Max_Angle,
                        torq_double,
                        Angle_double,
                        titleAxisY,
                        indexSSL,
                        filteredTime,
                        filteredPress);   //绘图扭力对时间
                    chartTorqueTimeParent.InvalidateVisual();
                    Task.Run(() =>
                    {
                        ShotPressPicture(filteredTime, filteredPress);
                    });
                }));
            }

            // 2025-05-14 压力扭力峰值一致性校正
            Savedata_TimePress(filteredTime, filteredPress);

            //保存时间-扭力-角度-压力数据
            Savedata_TimeTorAngPre(torq_double, Angle_double, filteredTime, filteredPress, arrProcessMonitorTK);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        /// <param name="press"></param>
        private void ShotPressPicture(double[] times, double[] press)
        {
            //Thread.Sleep(1500);
            //{
            string fileName = $"{ProductSn.Trim()}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}_PRO_{ScrewName}_Press_Time";
            string logPath = dirCurveTorqueTimeData + fileName + ".png";
            FileInfo fi = new FileInfo(logPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            myChart_T.SaveTimeCurveImageWithInfo(tkDataInterval, indexSSL, Max_Torque, Max_Angle, times, torq_double, Angle_double, press, arrProcessMonitorTK, dirCurveTorqueTimeData, fileName);// 保存时间曲线图像并附加信息
            //复制图片到GVIMAGES目录
            CopyToGVImages(logPath, ProductSn, fileName);
            //if (dispatcher != null)
            //{
            //    dispatcher.Invoke(new Action(() =>
            //    {
            //        //myChart1_T.ShotPicture(chartTorqueTimeParent, dirCurveTorqueTimeData, fileName);   //截图
            //        myChart1_T.SaveTimeCurveImageWithInfo(tkDataInterval, indexSSL1, Max_Torque_1, Max_Angle_1, times, torq_double_1, Angle_double_1, press, arrResultTK1, dirCurveTorqueTimeData, fileName);// 保存时间曲线图像并附加信息

            //    }));
            //}
            //}
        }

        /// <summary>
        /// 绘制toe in 压力曲线
        /// </summary>
        /// <param name="xforce"></param>
        /// <param name="yforce"></param>
        /// <param name="zforce"></param>
        /// <param name="time"></param>
        public void UpdateToeinCurve(double[] xforce, double[] yforce, double[] zforce, double[] time, string SNCode)
        {
            if (dispatcher != null)
            {
                myChart_P = new TorqueChart();
                dispatcher.Invoke(new Action(() =>
                {
                    myChart_P.DrawToeinForce(chartToeinForce, xforce, yforce, zforce, time);
                    chartToeinForceParent.InvalidateVisual();
                    Task.Run(() =>
                    {
                        string fileName = SNCode.Trim() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + "_PRO_XYZForce";
                        string logPath = dirCurveToeinXYZForceData + fileName + ".png";

                        FileInfo fi = new FileInfo(logPath);
                        if (!fi.Directory.Exists)
                        {
                            fi.Directory.Create();
                        }
                        myChart_P.SaveToeinForceCurveImage(xforce, yforce, zforce, time,
                             dirCurveToeinXYZForceData, fileName);
                        //复制图片到GVIMAGES目录
                        CopyToGVImages(logPath, SNCode, fileName);

                    });
                }));
            }


            //Savedata_TimePress(filteredTime, filteredPress);
        }

        private void CopyToGVImages(string sourceFilePath, string SNCode, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourceFilePath))
                    return;

                if (!File.Exists(sourceFilePath))
                    return;

                string date = DateTime.Now.ToString("yyyyMMdd");
                string snFolder = SNCode?.Trim();
                if (string.IsNullOrEmpty(snFolder))
                    snFolder = "sn";

                string destDir = Path.Combine("E:\\GVIMAGES\\Insight", date, "Screw", snFolder);

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                string destPath = Path.Combine(destDir, fileName + ".png");

                // 复制并覆盖已存在的同名文件
                File.Copy(sourceFilePath, destPath, true);

                LogEvent?.Invoke($"Toein 图片已保存并复制到: {destPath}");
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"复制 Toein 图片到目标目录时出错: {ex.Message}");
                Debug.WriteLine($"CopyToGVImages 异常: {ex}");
            }
        }
        #endregion

        #region 保存过程监视数据PRM 保存TCR/RTM 算好的扭矩值和角度值

        static string rootSaveDir = $"D:\\TaiKeScrewDatas\\{DateTime.Now:yyyyMMdd}\\";  //数据保存路径
        string dirSourceData = $"{rootSaveDir}STD1\\OriginalData\\";
        string dirPRMData = $"{rootSaveDir}STD1\\PRM\\";
        string dirTCRData = $"{rootSaveDir}STD1\\TCR\\";
        string dirPressData = $"{rootSaveDir}STD1\\Press\\";
        string dirTimeTorqueData = $"{rootSaveDir}STD1\\TimeTorque\\";
        string dirCurveTorqueTimeData = $"{rootSaveDir}STD1\\Curve\\Press_Time\\";
        string dirCurveTorqueAngleData = $"{rootSaveDir}STD1\\Curve\\Torque_Angle\\";
        string dirCurveToeinXForceData = $"{rootSaveDir}STD1\\Curve\\Toein_X_Force\\";
        string dirCurveToeinYForceData = $"{rootSaveDir}STD1\\Curve\\Toein_Y_Force\\";
        string dirCurveToeinZForceData = $"{rootSaveDir}STD1\\Curve\\Toein_Z_Force\\";
        string dirCurveToeinXYZForceData = $"{rootSaveDir}Toein_XYZ_Force\\STD1\\Curve\\";
        string dirTimeTorAngPreData = $"{rootSaveDir}STD1\\TimeTorAngPre\\";

        string titlePRM = "WIP" + "," + "Time" + "," + "HD" + "," + "ID" + "," + "MC" + "," + "EC" + "," + "ALT1" + "," + "ALT2" + "," + "WARN" + ","
                      + "T1" + "," + "T2" + "," + "T3" + "," + "T4" + "," + "Tt" + "," + "A1" + "," + "A2" + "," + "A3" + "," + "Af" + ","
                      + "Abr" + "," + "A4" + "," + "At" + "," + "PRMCT" + "," + "MECCT" + "," + "Tbr" + "," + "Dra" + "," + "Drp" + ","
                      + "TU" + "," + "SSL" + "," + "SR1" + "," + "SR2" + "," + "SN1" + "," + "SN2" + "," + "OTA" + "," + "TM" + ","
                      + "Min.Seating torque" + "," + "Ave.Seating torque" + "," + "Min.Clamp torque" + "," + "Ave.Clamp torque" + "," + "Min.Seating angle" + "," + "Ave.Seating angle" + "," + "Min.Clamp angle" + "," + "Ave.Clamp angle" + ","
                      + "Prevailing angle" + "," + "Prevailing torque";

        /// <summary>
        /// 实例化对象1
        /// </summary>
        CRecordValue recordValuePRM1 = new CRecordValue();    //

        /// <summary>
        /// 记录电批1过程数据 PRM
        /// </summary>
        public void Updata_PRM()
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd");
            string logPath = dirPRMData + fileName + ".csv";

            FileInfo fi = new FileInfo(logPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            string value = ProductSn.Trim() + "," + DateTime.Now.ToString("HH:mm:ss");
            //string[] temp = arrResultTK1[31].Split('\r');
            //arrResultTK1[31] = temp[0];   //去掉换行符
            for (int i = 0; i < 42; i++)
            {
                value = value + "," + arrProcessMonitorTK[i];
            }

            recordValuePRM1.RecordValue(dirPRMData, fileName, titlePRM, value);
        }

        //============================================================================================================================
        CRecordValue recordValueTorque = new CRecordValue();
        CRecordValue recordValuePress = new CRecordValue();
        CRecordValue recordTimeTorque = new CRecordValue();

        /// <summary>
        /// 电批1算好的扭矩值和角度值
        /// </summary>
        public void Updata_AngleTorque()
        {
            string fileName = ProductSn.Trim() + "_" + DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss-fff");
            string logPath = dirTCRData + fileName + ".csv";
            //E:\工站\厂商\Type\Date\Result\Time_WIP_SN_WorkStaion\StationID_{YYYYMMDD_HHMMSS}_SN_Status_Screw_Torque_Vendor_Machine.csv
            //E:\CGSF\Luster\Torque_Screw_1\20251121\OK\235959_SN\FXGL_B06-2FT-07_5_STATION1615_20251121_233215_Valkyrie_SN_OK_Screw_W_Torque_SR_MAM2.csv
            string dirTCRData1 = $"E:\\CGSF\\Luster\\Torque_Screw_{ScrewNumber}\\{DateTime.Now.ToString("yyyyMMdd")}\\";
            //CGSF规范文件名（不含.csv后缀，RecordValue会自动追加）
            //格式：{StationID}_{yyyyMMddHHmmss}_{SN}_OK_Screw{N}_Torque_Luster_CGSF_{StationType}
            string fileName1 = dirStationID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                               ProductSn.Trim() + "_" + "OK_Screw" + ScrewNumber +
                               "_Torque_Luster_CGSF_" + dirStationType;
            FileInfo fi = new FileInfo(logPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            string title = "No" + "," + "Torque1" + "," + "Angle1";
            string title1 = "No" + "," +
                            "ScrewNumber" + "," +
                            "Vendor" + "," +
                            "Side" + "," +
                            "Machine" + "," +
                            "SN" + "," +
                            "Time" + "," +
                            "Angle" + "," +
                            "Torque(Kgf*cm)" + "," +
                            "Status";
            string value = "";
            string value1 = "";
            int len = torq_double.Length;

            for (int i = 0; i < len; i++)
            {
                value = i + 1 + "," + torq_double[i] + "," + Angle_double[i];
                if(i==0)
                {
                    value1 = i + 1 + "," +ScrewNumber + "," + "Luster" + "," +"07_" + ScrewNumber + "," + "CGSF" + "," +
                             ProductSn.Trim() + "," + " " + "," + Angle_double[i] + "," + torq_double[i] + "," + "OK";
                }
                else
                {
                    value1 = i + 1 + "," + " " + "," + " " + "," + " "+ "," + " " + "," +
                            " " + "," + " " + "," + Angle_double[i] + "," + torq_double[i] + "," + "OK";
                }
                recordValueTorque.RecordValue(dirTCRData, fileName, title, value);
                recordValueTorque.RecordValue(dirTCRData1, fileName1, title1, value1);
            }
        }

        /// <summary>
        /// 保存时间压力曲线
        /// </summary>
        /// <param name="time"></param>
        /// <param name="press"></param>
        public void Savedata_TimePress(double[] time, double[] press)
        {
            string fileName = ProductSn.Trim() + "_" + DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss-fff");
            string logPath = dirPressData + fileName + ".csv";
            string dirPressData1 = $"E:\\CGSF\\Luster\\Force_Screw_{ScrewNumber}\\{DateTime.Now.ToString("yyyyMMdd")}\\";
            //CGSF规范文件名（不含.csv后缀，RecordValue会自动追加）
            //格式：{StationID}_{yyyyMMddHHmmss}_{SN}_OK_Screw{N}_Force_Luster_CGSF_{StationType}
            string fileName1 = dirStationID + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                               ProductSn.Trim() + "_" + "OK_Screw" + ScrewNumber +
                               "_Force_Luster_CGSF_" + dirStationType;

            //double[] timeNew = time;
            //double[] pressNew = press;

            //pressNew[0] = 0;

            //int k = 1;
            //while (timeNew[k++] < 250)
            //{
            //    if (timeNew[k] > 0.05)
            //        pressNew[k] = 0.01;
            //}

            if (!Directory.Exists(dirPressData))
            {
                Directory.CreateDirectory(dirPressData);
            }

            FileInfo fi = new FileInfo(logPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            string title = "No" + "," + "Time" + "," + "Press";
            string title1 = "ScrewNumber" + "," +
                           "Vendor" + "," +
                           "Side" + "," +
                           "Machine" + "," +
                           "SN" + "," +
                           "Time(ms)" + "," +
                           "Force(N)" + "," +
                           "Z Speed(mm/s)" + "," +
                           "Z Motion(mm)" + "," +
                           "Z Real Speed(mm/s)" + "," +
                           "Status";
            string value = "";
            string value1 = "";
            int len = press.Length;

            for (int i = 0; i < len; i++)
            {
                value = i + 1 + "," + time[i] + "," + press[i];
                value1 = ScrewNumber + "," + "Luster" + "," + "A" + "," + "CGSF" + "," + ProductSn.Trim() + "," +
                        time[i] + "," + press[i] + "," + " " + "," + " " + "," +" " + "," + "OK";
                recordValuePress.RecordValue(dirPressData, fileName, title, value);
                recordValuePress.RecordValue(dirPressData1, fileName1, title1, value1);
            }
        }

        /// <summary>
        /// 保存时间扭矩数据 
        /// </summary>
        public void Savedata_TimeTorque()
        {
            string fileName = ProductSn.Trim() + "_" + DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss-fff");
            string logPath = dirTimeTorqueData + fileName + ".csv";

            FileInfo fi = new FileInfo(logPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            string title = "Time" + "," + "Torque";
            string value = "";
            int len = torq_double.Length;

            for (int i = 0; i < len; i++)
            {
                value = tkDataInterval * i + "," + torq_double[i];
                recordValueTorque.RecordValue(dirTimeTorqueData, fileName, title, value);
            }
        }


        /// <summary>
        /// 保存 时间-扭力-角度-压力 数据
        /// </summary>
        /// <param name="torq_double"></param>
        /// <param name="Angle_double"></param>
        /// <param name="filteredTime"></param>
        /// <param name="filteredPress"></param>
        /// <param name="arrProcessMonitorTK"></param>
        private void Savedata_TimeTorAngPre(double[] torq_double, double[] Angle_double, double[] filteredTime, double[] filteredPress, string[] arrProcessMonitorTK)
        {
            try
            {
                if (!Directory.Exists(dirTimeTorAngPreData))
                    Directory.CreateDirectory(dirTimeTorAngPreData);

                string sn = ProductSn?.Trim();
                if (string.IsNullOrEmpty(sn))
                {
                    LogEvent?.Invoke("Savedata_TimeTorAngPre: ProductSn 为空，跳过保存。");
                    return;
                }

                int lenTorque = torq_double?.Length ?? 0;
                int lenAngle = Angle_double?.Length ?? 0;
                int lenTime = filteredTime?.Length ?? 0;
                int lenPress = filteredPress?.Length ?? 0;

                // 使用最大行数遍历，保留任一数组存在的数据行
                int rows = Math.Max(Math.Max(lenTorque, lenAngle), Math.Max(lenTime, lenPress));
                if (rows == 0)
                {
                    LogEvent?.Invoke("Savedata_TimeTorAngPre: 无可合并的数据行，跳过保存。");
                    return;
                }

                // 解析 T1（毫秒）：优先使用传入的 arrProcessMonitorTK[7]，其次使用 TorqueData.T1，最后退回默认 300 ms
                //double t1Ms = 300.0;
                //if (arrProcessMonitorTK != null && arrProcessMonitorTK.Length > 7)
                //{
                //    if (!double.TryParse(arrProcessMonitorTK[7], out t1Ms) || t1Ms <= 0)
                //        t1Ms = 300.0;
                //}
                //else if (TorqueData != null && !string.IsNullOrWhiteSpace(TorqueData.T1))
                //{
                //    if (!double.TryParse(TorqueData.T1, out t1Ms) || t1Ms <= 0)
                //        t1Ms = 300.0;
                //}

                string fileName = sn + "_" + DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss-fff") + "_TimeTorAngPre.csv";
                string outPath = Path.Combine(dirTimeTorAngPreData, fileName);

                using (var sw = new StreamWriter(outPath, false, Encoding.UTF8))
                {
                    sw.WriteLine("No,Torque1,Angle1,Time,Press");
                    int writtenCount = 0;
                    int noValue = 0; // 从 0 开始，步长为 2

                    for (int i = 0; i < rows; i++)
                    {
                        // 判断 Time 是否存在
                        bool timePresent = (filteredTime != null && i < lenTime);
                        double originalTime = timePresent ? filteredTime[i] : 0.0;

                        // 若存在时间且小于 T1，则舍去该行
                        //if (timePresent && originalTime < t1Ms)
                        //    continue;

                        // 准备写入
                        string no = noValue.ToString();

                        // 安全取值，避免越界
                        double torque = (torq_double != null && i < lenTorque) ? torq_double[i]: 0.0;
                        double angle = (Angle_double != null && i < lenAngle) ? Angle_double[i] : 0.0;

                        // Time 列：若存在则写 adjustedTime（originalTime - T1），否则写空值
                        //string timeStr = "";
                        //if (timePresent)
                        //{
                        //    double adjustedTime = originalTime - t1Ms;
                        //    if (adjustedTime < 0) adjustedTime = 0; // 理论上已筛除，但保守处理
                        //    timeStr = adjustedTime.ToString();
                        //}

                        double press = (filteredPress != null && i < lenPress) ? filteredPress[i] : 0.0;

                        //sw.WriteLine($"{no},{torque},{angle},{timeStr},{press}");
                        sw.WriteLine($"{no},{torque},{angle},{originalTime},{press}");

                        // 更新计数与 No 值
                        writtenCount++;
                        noValue += 2;
                    }

                    if (writtenCount == 0)
                    {
                        // 如果没有任何行被写入，删除空文件并记录
                        sw.Close();
                        File.Delete(outPath);
                        //LogEvent?.Invoke($"Savedata_TimeTorAngPre: 所有时间点均小于 T1({t1Ms} ms) 或无可写入行，未生成文件。");
                        return;
                    }
                }

                LogEvent?.Invoke($"Savedata_TimeTorAngPre: 合并完成，输出文件: {outPath}"); // (使用 T1={t1Ms} ms)");
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke($"Savedata_TimeTorAngPre 合并异常: {ex.Message}");
            }
        }
        #endregion

        #region 定时器功能

        long driverOptTime = 0;  // 电批空转到锁付操作计时
        int dataEnterTime = 0;   // 电批两路数据进来的时间间隔
        bool TKnetDataEnter = false; // 网络数据进来标志
        bool TKcomDataEnter = false; // 串口数据进来标志

        /// <summary>
        /// 100ms时钟
        /// </summary>
        private async void Timer100Tick()
        {
            while (true)
            {
                await Task.Delay(100);
                driverOptTime++;
            }
        }

        /// <summary>
        /// 20ms时钟
        /// </summary>
        private async void Timer20Tick()
        {
            while (true)
            {
                await Task.Delay(20);
                CheckDataOK();
            }
        }

        private void CheckDataOK()
        {
            switch (ComType)
            {
                case 1: CheckComNetDataOK(); break;
                case 2: CheckNetDataOK(); break;
                case 3: CheckComDataOK(); break;
            }
        }

        /// <summary>
        /// 检查电批网络接收数据与串口接收数据相隔时间
        /// </summary>
        private void CheckComNetDataOK()
        {
            if (TKnetDataEnter || TKcomDataEnter)   //检查电批两路相隔时间
            {
                if (dataEnterTime > 100)
                {
                    TKnetDataEnter = false;
                    TKcomDataEnter = false;
                    dataEnterTime = 0;
                    Debug.WriteLine("电批接收两路数据相隔时间过长！");
                    LogEvent?.Invoke("电批接收两路数据相隔时间过长!");
                }
                dataEnterTime++;
            }
            if (TKnetDataEnter && TKcomDataEnter)   //电批1两路数据均已接收
            {
                dataEnterTime = 0;
                TKnetDataEnter = false;
                TKcomDataEnter = false;
                if (optID_com == optID_net.ToString())  //判断电批1两路数据编号相同，确保为同一次锁付
                {
                    Updata_AngleTorque();  //电批1算好的扭矩值和角度值
                    Savedata_TimeTorque();
                    Updata_PRM();          //记录电批1过程数据 PRM

                    if (dispatcher != null)
                    {
                        //dispatcher.Invoke(new Action(() =>
                        //{
                        //    myChart1_T.Draw(chartTorqueTime, tbxs_t, tkDataInterval, 0, arrResultTK1, Max_Torque_1, Max_Angle_1, torq_double_1, Angle_double_1, titleAxisY_1, indexSSL1);   //绘图扭力对时间
                        //    chartTorqueTimeParent.InvalidateVisual();
                        //}));

                        dispatcher.Invoke(new Action(() =>
                        {
                            myChart_A.Draw(chartTorqueAngle, tbxs_a, tkDataInterval, 1, arrProcessMonitorTK, Max_Torque, Max_Angle, torq_double, Angle_double, titleAxisY, indexSSL);   //绘图扭力对角度
                            chartTorqueAngleParent.InvalidateVisual();
                        }));

                    }

                    #region 数据是来源于串口的
                    // 电批数据整体放入数据块中
                    TorqueData = new TaiKeData();
                    TorqueData.originalData = originalData;
                    TorqueData.arrResultTK = arrProcessMonitorTK;
                    TorqueData.torq_double = torq_double;
                    TorqueData.Angle_double = Angle_double;
                    TorqueData.Max_Torque = Max_Torque;
                    TorqueData.Max_Angle = Max_Angle;
                    TorqueData.titleAxisY = titleAxisY;
                    TorqueData.indexSSL = indexSSL;

                    TorqueData.EC = arrProcessMonitorTK[3];   //EC
                    TorqueData.T1 = double.TryParse(arrProcessMonitorTK[7], out var DT1) ? arrProcessMonitorTK[7] : "-1";   //T1
                    TorqueData.T2 = double.TryParse(arrProcessMonitorTK[8], out var DT2) ? arrProcessMonitorTK[8] : "-1";   //T2
                    TorqueData.T3 = double.TryParse(arrProcessMonitorTK[9], out var DT3) ? arrProcessMonitorTK[9] : "-1";   //T3
                    TorqueData.Tt = double.TryParse(arrProcessMonitorTK[11], out var DTt) ? arrProcessMonitorTK[11] : "-1";  //Tt
                    TorqueData.A1 = double.TryParse(arrProcessMonitorTK[12], out var DA1) ? arrProcessMonitorTK[12] : "-1";  //A1
                    TorqueData.A2 = double.TryParse(arrProcessMonitorTK[13], out var DA2) ? arrProcessMonitorTK[13] : "-1";  //A2
                    TorqueData.A3 = double.TryParse(arrProcessMonitorTK[14], out var DA3) ? arrProcessMonitorTK[14] : "-1";  //A3
                    TorqueData.SSL = torq_double[indexSSL].ToString();    //SSL
                    TorqueData.MaxAngle = Max_Angle.ToString();
                    TorqueData.MaxTorque = Max_Torque.ToString();
                    TorqueData.SeatingTorque = double.TryParse(arrProcessMonitorTK[32], out var DST) ? arrProcessMonitorTK[32] : "-1";    //
                    TorqueData.ClampTorque = double.TryParse(arrProcessMonitorTK[34], out var DCT) ? arrProcessMonitorTK[34] : "-1";
                    TorqueData.PrevailingTorque = double.TryParse(arrProcessMonitorTK[41], out var DPT) ? arrProcessMonitorTK[41] : "-1";
                    TorqueData.MC = arrProcessMonitorTK[2];  //MC
                    #endregion

                    if (DataUpdated != null)
                    {
                        DataUpdated.Invoke((TaiKeData)TorqueData.Clone());
                    }

                    if (dispatcher != null)
                    {
                        Task.Run(() =>
                        {
                            ShotPicture();
                        });
                    }
                }
                else
                {
                    LogEvent?.Invoke("电批两路数据编号不一致!");
                    Debug.WriteLine("电批两路数据编号不一致！");
                }
            }
        }

        private void CheckNetDataOK()
        {
            if (TKnetDataEnter)
            {
                TKnetDataEnter = false;
                DataOK();
            }
        }

        private void CheckComDataOK()
        {
            if (TKcomDataEnter)
            {
                TKcomDataEnter = false;
                DataOK();
            }
        }

        private void DataOK()
        {
            Updata_AngleTorque();  //电批1算好的扭矩值和角度值
            Savedata_TimeTorque();
            Updata_PRM();          //记录电批1过程数据 PRM

            if (dispatcher != null)
            {
                //type=0 绘图扭力对时间 type=1 绘图扭力对角度
                dispatcher.Invoke(new Action(() =>
                {
                    myChart_A.Draw(chartTorqueAngle, tbxs_a, tkDataInterval, 1, arrProcessMonitorTK, Max_Torque, Max_Angle, torq_double, Angle_double, titleAxisY, indexSSL);   //绘图扭力对角度
                    chartTorqueAngleParent.InvalidateVisual();
                }));

            }

            #region 数据是来源
            // 电批数据整体放入数据块中
            TorqueData = new TaiKeData();
            TorqueData.originalData = originalData;
            TorqueData.arrResultTK = arrProcessMonitorTK;
            TorqueData.torq_double = torq_double;
            TorqueData.Angle_double = Angle_double;
            TorqueData.Max_Torque = Max_Torque;
            TorqueData.Max_Angle = Max_Angle;
            TorqueData.titleAxisY = titleAxisY;
            TorqueData.indexSSL = indexSSL;

            TorqueData.EC = arrProcessMonitorTK[3];   //EC
            TorqueData.T1 = double.TryParse(arrProcessMonitorTK[7], out var DT1) ? arrProcessMonitorTK[7] : "-1";   //T1
            TorqueData.T2 = double.TryParse(arrProcessMonitorTK[8], out var DT2) ? arrProcessMonitorTK[8] : "-1";   //T2
            TorqueData.T3 = double.TryParse(arrProcessMonitorTK[9], out var DT3) ? arrProcessMonitorTK[9] : "-1";   //T3
            TorqueData.Tt = double.TryParse(arrProcessMonitorTK[11], out var DTt) ? arrProcessMonitorTK[11] : "-1";  //Tt
            TorqueData.A1 = double.TryParse(arrProcessMonitorTK[12], out var DA1) ? arrProcessMonitorTK[12] : "-1";  //A1
            TorqueData.A2 = double.TryParse(arrProcessMonitorTK[13], out var DA2) ? arrProcessMonitorTK[13] : "-1";  //A2
            TorqueData.A3 = double.TryParse(arrProcessMonitorTK[14], out var DA3) ? arrProcessMonitorTK[14] : "-1";  //A3
            TorqueData.SSL = torq_double[indexSSL].ToString();    //SSL
            TorqueData.MaxAngle = Max_Angle.ToString();
            TorqueData.MaxTorque = Max_Torque.ToString();
            TorqueData.SeatingTorque = double.TryParse(arrProcessMonitorTK[32], out var DST) ? arrProcessMonitorTK[32] : "-1";    //
            TorqueData.ClampTorque = double.TryParse(arrProcessMonitorTK[34], out var DCT) ? arrProcessMonitorTK[34] : "-1";
            TorqueData.PrevailingTorque = double.TryParse(arrProcessMonitorTK[41], out var DPT) ? arrProcessMonitorTK[41] : "-1";
            TorqueData.MC = arrProcessMonitorTK[2];  //MC
            #endregion

            if (DataUpdated != null)
            {
                DataUpdated.Invoke((TaiKeData)TorqueData.Clone());
            }

            if (dispatcher != null)
            {
                Task.Run(() =>
                {
                    ShotPicture();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShotPicture()
        {
            Thread.Sleep(1500);
            {
                string fileName = $"{ProductSn.Trim()}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")}_PRO_{ScrewName}_Torque_Angle";
                string logPath = dirCurveTorqueAngleData + fileName + ".png";

                FileInfo fi = new FileInfo(logPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                if (dispatcher != null)
                {
                    dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            //myChart1_A.ShotPicture(chartTorqueAngleParent, dirCurveTorqueAngleData, fileName);   //截图
                            myChart_A.SaveTorqueCurveImageWithInfo(tkDataInterval, indexSSL, Max_Torque, Max_Angle, 1, torq_double, Angle_double, arrProcessMonitorTK, dirCurveTorqueAngleData, fileName);// 保存时间曲线图像并附加信息
                            //复制图片到GVIMAGES目录
                            CopyToGVImages(logPath, ProductSn, fileName);
                        }
                        catch (Exception ex)
                        {
                            LogEvent?.Invoke($"电批截图异常: {ex.Message}");
                            Console.WriteLine(ex.Message);
                        }
                    }));
                }
            }
        }

        #endregion

        #region 析构代码

        public void Dispose()
        {
            // disposing=true，手动释放托管资源
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ElectricScrewDrivers()
        {
            // disposing=false，不释放托管资源，交给终结器释放
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 释放托管资源

                CloseNetController();
                CloseSerialPort();
            }

            // 释放非托管资源

            _disposed = true;
        }

        private bool _disposed;

        #endregion

        #region 没用
        public void UpdatePress(double[] time, double[] press)
        {
            if (dispatcher != null)
            {
                dispatcher.Invoke(new Action(() =>
                {
                    myChart_T.DrawPress(chartTorqueTime, tbxs_t, tkDataInterval, 0, arrProcessMonitorTK, Max_Torque, Max_Angle, torq_double, Angle_double, titleAxisY, indexSSL, time, press);   //绘图扭力对时间
                    chartTorqueTimeParent.InvalidateVisual();
                    Task.Run(() =>
                    {
                        //ShotPressPicture();
                    });
                }));
            }

        }
        #endregion
    }
}
