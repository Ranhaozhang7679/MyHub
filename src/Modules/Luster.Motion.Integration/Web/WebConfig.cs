using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.Integration.Web
{
    /// <summary>
    /// Web相关配置
    /// </summary>
    public class WebConfig : IXMLParser
    {
        #region 驾驶舱
        public event Action<string, bool> IsEnabledEvent;
        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _HyperTrainEnabled;
        public bool HyperTrainEnabled
        {
            get => _HyperTrainEnabled;

            set
            {
                bool src = _HyperTrainEnabled;
                _HyperTrainEnabled = value;
                if (src != value)
                {
                    IsEnabledEvent?.Invoke("HyperTrain", value);
                }
            }
        }

        //public string MainVersion => Common.Tools.SoftVersionTool.GetVersion("LusterMotion.exe");

        //public string SoftVersionDate => GetSoftVersion();

        //private string GetSoftVersion()
        //{
        //    string baseDir = "";
        //    if (string.IsNullOrEmpty(baseDir))
        //    {
        //        baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //    }

        //    string fileName = System.IO.Path.Combine(baseDir, "LusterMotion.exe");
        //    FileInfo file = new FileInfo(fileName);
        //    return file.LastWriteTime.ToString("yyMMdd");
        //}

        /// <summary>
        /// SFC URL地址
        /// </summary>
        public string SFCUrl { get; set; }

        /// <summary>
        /// subcmd
        /// </summary>
        public string subcmd { get; set; }

        /// <summary>
        /// 设备SN
        /// </summary>
        public string MachineSn { get; set; }

        /// <summary>
        /// ALink CSV文件路径
        /// </summary>
        public string FileDirectory { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string MachineType { get; set; }

        /// <summary>
        /// 机种
        /// </summary>
        public string Product { get; set; }


        /// <summary>
        /// 机种_Vision
        /// </summary>
        public string Product_Vision { get; set; }

        /// <summary>
        /// 厂区
        /// </summary>
        public string SiteCode { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 楼层
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// 线体
        /// </summary>
        public string LineCode { get; set; }

        /// <summary>
        /// 站号
        /// </summary>
        public string UniteCode { get; set; }

        /// <summary>
        /// 工站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 工站类型
        /// </summary>
        public string StationType { get; set; }

        /// <summary>
        /// 工站ID
        /// </summary>
        public string StationId { get; set; }

        /// <summary>
        /// 设备厂商
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string DeviceIP { get; set; }

        /// <summary>
        /// Mac地址
        /// </summary>
        public string DeviceMac { get; set; }

        /// <summary>
        /// Insight工站
        /// </summary>
        public string InsightType { get; set; }


        /// <summary>
        /// 视觉版本
        /// </summary>
        public string VisionVersion { get; set; }

        /// <summary>
        /// 机器人版本
        /// </summary>
        public string RobotVersion { get; set; }

        public string PlcVersion { get; set; }

        public string ReciepeVersion { get; set; }

        /// <summary>
        /// 激光版本
        /// </summary>
        public string LaserVersion { get; set; }


        /// <summary>
        /// AElimits版本
        /// </summary>
        public string LimitsVersion { get; set; }

        /// <summary>
        /// Bali版本
        /// </summary>
        public string BaliVersion { get; set; }

        /// <summary>
        /// 最终软件版本
        /// </summary>
        public string SoftVersion { get; set; }

        /// <summary>
        /// Hive AppId
        /// </summary>
        public string HiveAppId { get; set; }

        /// <summary>
        /// Vision Api版本
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// ImagePath
        /// </summary>
        public string ImagePath { get; set; }


        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string ServerIP { get; set; }

        /// <summary>
        /// 服务器Port
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Vision系统地址
        /// </summary>
        public string VisionIP { get; set; }

        /// <summary>
        /// Vision StationId
        /// </summary>
        public string VisionStationId { get; set; }


        /// <summary>
        /// Vision系统对应的端口号
        /// </summary>
        public int VisionPort { get; set; }

        /// <summary>
        /// Vision系统的API
        /// </summary>
        private bool _VisionEnabled;
        public bool VisionEnabled
        {
            get => _VisionEnabled;

            set
            {
                bool src = _VisionEnabled;
                _VisionEnabled = value;
                if (src != value)
                {
                    IsEnabledEvent?.Invoke("Vision", value);
                }
            }
        }

        /// <summary>
        /// Vision日志
        /// </summary>
        public bool VisionLog { get; set; }

        /// <summary>
        /// 安全门
        /// </summary>
        private bool _IsShieldDoor;
        public bool IsShieldDoor
        {
            get => _IsShieldDoor;
            set
            {
                bool src = _IsShieldDoor;
                _IsShieldDoor = value;
                if (src != value)
                {
                    IsEnabledEvent?.Invoke("Hive", value);
                }
            }
        }

        /// <summary>
        /// Vision补充
        /// </summary>
        public string VisionExtra { get; set; }

        /// <summary>
        /// Vision系统地址
        /// </summary>
        public string HiveIP { get; set; }

        /// <summary>
        /// 是否忽略Hive反馈
        /// </summary>
        public bool IsHiveIgnoreFeedBack { get; set; }

        /// <summary>
        /// Vision系统对应的端口号
        /// </summary>
        public int HivePort { get; set; }

        /// <summary>
        /// Hive Simulator 系统地址，并赋初值
        /// </summary>
        public string HiveSimulatorURL { get; set; } = "http://127.0.0.1:5000/";

        /// <summary>
        /// 记录是否在维修状态，在维修状态不能上报Hive报警
        /// </summary>
        public bool IsRepairing { get; set; }


        /// <summary>
        /// 记录是否在维修状态，在维修状态不能上报Hive报警
        /// </summary>
        public RepairState HiveRepairState { get; set; }

        /// <summary>
        /// 记录是否在维修状态，在维修状态不能上报Hive报警
        /// </summary>
        public int HiveMachineState { get; set; }

        /// <summary>
        /// 记录是否在维修状态，在维修状态不能上报Hive报警
        /// </summary>
        public string HiveCurrentCode { get; set; }

        /// <summary>
        /// Vision系统的API
        /// </summary>
        private bool _HiveEnabled;
        public bool HiveEnabled
        {
            get => _HiveEnabled;

            set
            {
                bool src = _HiveEnabled;
                _HiveEnabled = value;
                if (src != value)
                {
                    IsEnabledEvent?.Invoke("Hive", value);
                }
            }
        }

        //hive屏蔽按钮是否可见
        public bool HiveIsVisibility { get; set; }

        /// <summary>
        /// ftpuser
        /// </summary>
        public string FtpUser { get; set; }

        /// <summary>
        /// ftpwd
        /// </summary>
        public string FtpPwd { get; set; }

        /// <summary>
        /// Vision管理部门
        /// </summary>
        public string ManageDept_Vision { get; set; }

        /// <summary>
        /// 功能部门
        /// </summary>
        public string FunctionId { get; set; }

        /// <summary>
        /// 注册类型
        /// </summary>
        public string RegisterType { get; set; }

        /// <summary>
        /// Vision工站名称
        /// </summary>
        public string StationName_Vision { get; set; }

        /// <summary>
        /// 卷料批次名称
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// 机种
        /// </summary>
        public string category_key { get; set; }

        /// <summary>
        /// Hive启用阀门
        /// </summary>
        public bool IsHiveValveEnabled { get; set; } = true;

        #endregion

        #region 整线系统

        /// <summary>
        /// 整线系统的IP地址
        /// </summary>
        public string WholeIP { get; set; } = "127.0.0.1";

        /// <summary>
        /// 整线系统的端口
        /// </summary>
        public int WholePort { get; set; } = 8000;

        /// <summary>
        /// 整线系统是否启用
        /// </summary>
        private bool _wholeLineEnabled;
        public bool WholeLineEnabled
        {
            get => _wholeLineEnabled;
            set
            {
                bool src = _wholeLineEnabled;
                _wholeLineEnabled = value;
                if (src != value)
                {
                    IsEnabledEvent?.Invoke("WholeLine", value);
                }
            }
        }
        #endregion


        #region FFU相关
        /// <summary>
        /// FFU速度等级
        /// </summary>
        public string FFUSpeedLevel { get; set; }

        /// <summary>
        /// 低速度模式电流值下限
        /// </summary>
        public double LowCurrentLowLimit { get; set; }

        /// <summary>
        /// 低速度模式电流值上限
        /// </summary>
        public double LowCurrentUpperLimit { get; set; }

        /// <summary>
        /// 中速度模式电流值下限
        /// </summary>
        public double MiddleCurrentLowLimit { get; set; }

        /// <summary>
        /// 中速度模式电流值上限
        /// </summary>
        public double MiddleCurrentUpperLimit { get; set; }

        /// <summary>
        /// 高速度模式电流值下限
        /// </summary>

        public double HighCurrentLowLimit { get; set; }
        /// <summary>
        /// 高速度模式电流值上限
        /// </summary>
        public double HighCurrentUpperLimit { get; set; }
        public string SoftVersionDate { get; set; }

        #endregion



        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            var root = this.ToXml("WebConfig");

            return root;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(SFCUrl) && string.IsNullOrEmpty(StationId);
        }
    }
}