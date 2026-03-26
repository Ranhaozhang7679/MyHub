using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleTCP;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System.Timers;
using Luster.TaskFlow.Motion.Logic;
using System.Threading;
using Luster.TaskFlow.Motion.Interfaces;

namespace Luster.Motion.Integration.Web
{
    /// <summary>
    /// 1.整线监听模式
    /// </summary>
    public class WholeLineAPI : BaseMESAPI, ISubWebSystem
    {
        /// <summary>
        /// 简单服务器
        /// </summary>
        private SimpleTcpServer server;

        // 穴位号 如果不存在就为0
        private readonly string Slot = "Slot";
        private readonly string WIP = "WIP";


        private VPlc vPlc = null;
        private System.Timers.Timer timer = null;

        private bool bStartFirstPieceProcess=false;

        private static string globalFirstPieceName = "Extend_首件启用";

        /// <summary>
        /// 整线线体
        /// </summary>
        /// <returns></returns>
        public override string GetSystem()
        {
            return "WholeLine";
        }

        /// <summary>
        /// 启用整线监听复位
        /// </summary>
        /// <param name="mController"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public override void StartService(IMotionController mController, string ip, int port)
        {
            base.mController = mController;
            if(vPlc==null)
                vPlc = mController.MotionEngine.DeviceEngine.GetVDevices<VPlc>().FirstOrDefault();
            if(timer==null) timer=new System.Timers.Timer(5000);
            timer.Enabled = false;
            timer.Elapsed -= readPlcRegister;
            timer.Enabled = true;
            timer.Elapsed += readPlcRegister;

            //try
            //{
            //    StopService();

            //    // 启用通讯服务器
            //    server = new SimpleTcpServer();
            //    server.ClientConnected -= Server_ClientConnected;
            //    server.ClientConnected += Server_ClientConnected;

            //    // 客户端连接断开
            //    server.ClientDisconnected -= Server_ClientDisconnected;
            //    server.ClientDisconnected += Server_ClientDisconnected;

            //    // 服务器接收到客户端请求
            //    server.DataReceived -= Server_DataReceived;
            //    server.DataReceived += Server_DataReceived;

            //    server.Start(port);

            //    mController.ProductEvent -= MController_ProductEvent;
            //    mController.ProductEvent += MController_ProductEvent;

            //    OnLog("Server Is Start!");
            //}
            //catch (Exception ex)
            //{
            //    OnLog($"Server Is Start Error:{ex.Message}");
            //}
        }

        private int firstPieceProcess=0;
        private void readPlcRegister(object sender, ElapsedEventArgs e)
        {
            if(GetPlcIntValue(mController.SysConfig.FirstPieceModeCommandAddr)==1)
            {
                bStartFirstPieceProcess = true;
                //不在首件模式下，才能赋值
                if(firstPieceProcess==0)
                {
                    firstPieceProcess = 1;
                    WritePlcValueInt(mController.SysConfig.FirstPieceModeCommandAddr, 0);
                }
            }
            else
            {
                

            }

            if (bStartFirstPieceProcess)
            {
                switch (firstPieceProcess)
                {
                    case 0:
                        
                        break;

                    case 1:
                        //1.需要切首件
                        mController.SetCurrentMode(SystemConsts.FirstMode);
                        //2.告知plc
                        WritePlcValueInt(mController.SysConfig.FirstPieceModeStatusAddr, 1);
                        //3.延时1s
                        Thread.Sleep(1000);
                        //4.等待首件模式被切
                        if (FirstPieceEnable()) 
                           firstPieceProcess = 2;
                        break;
                    case 2:
                        //等待 首件启动 的全局被关掉
                        if(!FirstPieceEnable())
                        {
                            WritePlcValueInt(mController.SysConfig.FirstPieceModeStatusAddr, 2);
                            Thread.Sleep(1000);
                            firstPieceProcess = 3;
                            break;
                        }
                        break;
                    case 3:
                        //切回正常模式
                        mController.SetCurrentMode(SystemConsts.ProductMode);
                        //清空寄存器
                        WritePlcValueInt(mController.SysConfig.FirstPieceModeStatusAddr, 0);
                        Thread.Sleep(1000);
                        firstPieceProcess = 4;
                        break;
                    case 4:
                        bStartFirstPieceProcess = false;
                        firstPieceProcess = 0;
                        break;

                }
            }
        }

        private string GetCacheBySN(ICacheManager cache, string sn, string name)
        {
            string result = "";
            var cacheItem = cache.GetItem(sn);
            if (cacheItem != null && cacheItem is CacheItem cItem)
            {
                var item = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().StartsWith(name.ToUpper()));
                if (item != null && item.Value != null)
                {
                    result = item.Value.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 停止复位
        /// </summary>
        public override void StopService()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }
        }

        /// <summary>
        /// 产品出料时间
        /// </summary>
        /// <param name="productInfo"></param>
        /// <param name="isOK"></param>
        protected void MController_ProductEvent(ProductInfo productInfo, bool isOK)
        {
            if (mController.GetCurrentMode() != SystemConsts.FirstMode)
            {
                return;
            }

            string replyMsg = "";
            try
            {
                var cacheManager = mController.MotionEngine.CacheManager;

                // 穴位号
                string slot = GetCacheBySN(cacheManager, productInfo.Barcode, nameof(Slot));
                if (string.IsNullOrEmpty(slot))
                {
                    slot = "1";
                }

                // wip码
                string wip = GetCacheBySN(cacheManager, productInfo.Barcode, nameof(WIP));

                string proResult = productInfo.Result.Result ? "0" : "1";
                string deviceResult = (mController.AlarmInfo != null && mController.AlarmInfo.AlarmType != AlarmType.InfoTip) ? "1" : "0";

                replyMsg = $"Process,{proResult};Device,{deviceResult};Slot,{slot};WIP,{wip}\r\n";
                // 发生产品信息给客户端
                server?.Broadcast(replyMsg);
            }
            catch (Exception e)
            {
                OnLog($"通信异常:{e.Message}");
            }
            finally
            {
                OnLog($"Server Send Message To Client:{replyMsg}");
            }
        }

        /// <summary>
        /// 接收客户端数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_DataReceived(object sender, Message e)
        {
            string receiveMsg = Encoding.UTF8.GetString(e.Data);
            string replyMsg = "";

            // 是否能切换
            //bool canChanged = mController.MachineStatus == EngineStatus.Idle ||
            //    mController.MachineStatus == EngineStatus.Ready ||
            //    mController.MachineStatus == EngineStatus.Stop;
            bool canChanged = true;
            receiveMsg = receiveMsg.Replace("\r", "").Replace("\n", "");

            // 客户端 
            if (receiveMsg.Contains("ModeTo_FAI,0"))
            {
                replyMsg = "ModeTo_FAI,0";
                if (canChanged)
                {
                    // 切换到首件模式
                    bool isPass = mController.SetCurrentMode(SystemConsts.FirstMode);
                    replyMsg = $"ModeTo_FAI,{(isPass ? 0 : 1)}";
                }
                else
                {
                    replyMsg = $"ModeTo_FAI,1";
                }

                // 通知客户端切换首件模式成功
                replyMsg = replyMsg + "\r\n";
                server?.Broadcast(replyMsg);
            }
            else if (receiveMsg.Contains("ModeTo_PROD,0"))
            {
                // 如果上一次是首件模式，接到切换到生产模式通知，则可以进行变更切换
                var curMode = mController.GetCurrentMode();
                if (curMode == SystemConsts.FirstMode)
                {
                    canChanged = true;
                }

                replyMsg = "ModeTo_PROD,0\r\n";
                if (canChanged)
                {
                    // 切换到首件模式
                    bool isPass = mController.SetCurrentMode(SystemConsts.ProductMode);
                    replyMsg = $"ModeTo_PROD,{(isPass ? 0 : 1)}";
                }
                else
                {
                    replyMsg = $"ModeTo_PROD,1";
                }

                // 通知客户端切换生产模式成功
                replyMsg = replyMsg + "\r\n";
                server?.Broadcast(replyMsg);
            }

            OnLog($"Client:{e.TcpClient.Client.RemoteEndPoint} Send Message:{receiveMsg},CanChanged:{canChanged} Reply Message:{replyMsg}");
        }

        private void Server_ClientDisconnected(object sender, System.Net.Sockets.TcpClient e)
        {
            OnLog($"客户端:{e.Client.RemoteEndPoint} 连接断开!");
        }

        private void Server_ClientConnected(object sender, System.Net.Sockets.TcpClient e)
        {
            OnLog($"客户端:{e.Client.RemoteEndPoint} 连接成功!");
        }

        /// <summary>
        /// 写入PLC值，int
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool WritePlcValueInt(string addr, short val)
        {
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return false; }

            try
            {
                vPlc.Write(addr, val, $"01 06 {addr} 1");
            }
            catch (Exception)
            {
            }
            return true;
        }
        /// <summary>
        /// 获取Plc地址
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private int GetPlcIntValue(string addr)
        {
            int i = 0;
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return -1; }

            try
            {
                i = (int)vPlc.ReadNumber(addr);
                return (int)vPlc.ReadNumber(addr);
            }
            catch (Exception)
            {
            }

            return -1;
        }


        private bool FirstPieceEnable()
        {
            // 对SFC和PDCA进行监控
            var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);
            foreach (var item in globalModule.Parameters)
            {
                if (item.Key == globalFirstPieceName)
                {
                    if (item.Value != null)
                    {
                        return (bool)item.Value.Value;
                    }
                }
            }
            return false;
        }
    }

}
