using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.Integration.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class WebService : IWebService
    {
        /// <summary>
        /// 触发事件
        /// </summary>
        public event Action<string, string> MachineStatusComplete;

        /// <summary>
        /// 对应的配置文件
        /// </summary>
        public WebConfig WebConfig { get; set; }

        /// <summary>
        /// 驾驶舱
        /// </summary>
        private HyperTrainAPI hTrainAPI;

        /// <summary>
        /// MES系统对应的API
        /// </summary>
        private VisionAPI visionAPI;

        /// <summary>
        /// Hive系统
        /// </summary>
        private HiveAPI hiveAPI;

        /// <summary>
        /// 整线API
        /// </summary>
        private WholeLineAPI wholeAPI;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hyperTrainAPI"></param>
        /// <param name="vision"></param>
        /// <param name="hive"></param>
        public WebService(HyperTrainAPI hyperTrainAPI, VisionAPI vision, HiveAPI hive, WholeLineAPI wholeLineAPI)
        {
            hTrainAPI = hyperTrainAPI;
            visionAPI = vision;
            hiveAPI = hive;
            wholeAPI = wholeLineAPI;
            WebConfig = new WebConfig();
        }

        /// <summary>
        /// 主动触发状态
        /// </summary>
        /// <param name="mStatus"></param>
        public void OnMachineStatus(string mStatus, string reason)
        {
            MachineStatusComplete?.Invoke(mStatus, reason);
        }

        /// <summary>
        /// 获取所有的API
        /// </summary>
        /// <returns></returns>
        public List<ISubWebSystem> GetMESAPI()
        {
            return new List<ISubWebSystem>()
                {
                    hTrainAPI,
                    visionAPI,
                    hiveAPI,
                    wholeAPI
                };
        }

        /// <summary>
        /// 配置系统
        /// </summary>
        /// <param name=""></param>
        public object GetConfig()
        {
            return WebConfig;
        }

        /// <summary>
        /// 配置路径
        /// </summary>
        /// <param name="webConfigPath"></param>
        public void SaveConfig(string webConfigPath)
        {
            if (WebConfig == null)
            {
                WebConfig = new WebConfig();
            }

            var xml = WebConfig.ExportXml();
            xml.Save(webConfigPath);
        }

        public void LoadConfig(string webConfigPath)
        {
            try
            {
                WebConfig = new WebConfig();

                if (File.Exists(webConfigPath))
                {
                    var xWeb = XElement.Load(webConfigPath);
                    WebConfig.ParserXml(xWeb);

                    string versionXMLPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Version.xml");

                    // 1. 如果 exe 同级目录下没有该文件，则自动依据默认模板生成
                    if (!File.Exists(versionXMLPath))
                    {
                        GJNStruct initt = new GJNStruct();
                        initt.ExportXml().Save(versionXMLPath);
                    }

                    // 2. 加载 xml 节点
                    XDocument doc = XDocument.Load(versionXMLPath);
                    XElement softVersionElement = doc.Root.Element("SoftVersion");
                    string softVersion = softVersionElement.Value;
                    
                    // 3. 切割提取前缀(如 Luster-03.15) 和后缀(如 251212)
                    var parts = softVersion.Split('-');
                    string firstTwoParts = string.Join("-", parts.Take(2));
                    string lastPart = parts.Last();
                    
                    // 4. 动态拼接组合：中间机器模块版本强依赖 WebConfig.xml 中的实际定义
                    WebConfig.SoftVersion = $"{firstTwoParts}-{WebConfig.VisionVersion}-{WebConfig.RobotVersion}-{WebConfig.LaserVersion}-{lastPart}";
                }
            }
            catch (Exception)
            {
                /// xxxx
            }
        }

        /// <summary>
        /// 启动Web服务
        /// </summary>
        /// <param name="mController"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartService(IMotionController mController)
        {
            // 动态加载
            foreach (var item in GetMESAPI())
            {
                if (item is WholeLineAPI)
                {
                    if (WebConfig.WholeLineEnabled)
                    {
                        item.StartService(mController, WebConfig.WholeIP, WebConfig.WholePort);
                    }
                }

                else if (item is HyperTrainAPI)
                {
                    if (WebConfig.HyperTrainEnabled)
                    {
                        item.StartService(mController, WebConfig.ServerIP, WebConfig.ServerPort);
                    }
                }
                else if (item is VisionAPI)
                {
                    if (WebConfig.VisionEnabled)
                    {
                        item.StartService(mController, WebConfig.VisionIP, WebConfig.VisionPort);
                    }
                }
                else if (item is HiveAPI)
                {
                    if (WebConfig.HiveEnabled)
                    {
                        item.StartService(mController, WebConfig.HiveIP, WebConfig.HivePort);
                    }
                }
            }
        }
    }


    public class GJNStruct : IXMLParser
    {
        public string VisionVersion { get; set; }
        public string RobotVersion { get; set; }
        public string PlcVersion { get; set; }
        public string ReciepeVersion { get; set; }
        public string LaserVersion { get; set; }
        public string SoftVersion { get; set; }
        public string UpdateTimeStr { get; set; }

        public XElement ExportXml()
        {
            var root = this.ToXml("GJNStruct");

            return root;
        }

        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        public GJNStruct()
        {
            VisionVersion = "NN.NN";
            RobotVersion = "NN.NN";
            PlcVersion = "NN.NN";
            ReciepeVersion = "3.07.31";
            LaserVersion = "NN.NN";
            UpdateTimeStr = "250731";
            SoftVersion = $"Luster-03.14-{VisionVersion}-{RobotVersion}-{LaserVersion}-{UpdateTimeStr}";
        }

        public void UpdateWebConfig(WebConfig webConfig)
        {
            webConfig.VisionVersion = VisionVersion;
            webConfig.RobotVersion = RobotVersion;
            webConfig.PlcVersion = PlcVersion;
            webConfig.ReciepeVersion = ReciepeVersion;
            webConfig.LaserVersion = LaserVersion;
            webConfig.SoftVersion = SoftVersion;
            webConfig.SoftVersionDate = UpdateTimeStr;



        }



    }



}

