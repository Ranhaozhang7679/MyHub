using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.HyperTrain
{
    /// <summary>
    /// Web服务
    /// </summary>
    public interface IWebService
    {
        /// <summary>
        /// 获取对应的服务
        /// </summary>
        /// <param name="motionController"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void StartService(IMotionController motionController);

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        object GetConfig();

        /// <summary>
        /// 主动触发状态
        /// </summary>
        /// <param name="mStatus"></param>
        void OnMachineStatus(string mStatus, string reason);

        /// <summary>
        /// 触发事件
        /// </summary>
        event Action<string, string> MachineStatusComplete;

        /// <summary>
        /// 获取系统
        /// </summary>
        /// <returns></returns>
        List<ISubWebSystem> GetMESAPI();

        /// <summary>
        /// 配置路径
        /// </summary>
        /// <param name="webConfigPath"></param>
        void SaveConfig(string webConfigPath);

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="webConfigPath"></param>
        void LoadConfig(string webConfigPath);
    }

    /// <summary>
    /// 子系统
    /// </summary>
    public interface ISubWebSystem
    {
        /// <summary>
        /// 获取系统
        /// </summary>
        /// <returns></returns>
        string GetSystem();

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="mController"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void StartService(IMotionController mController, string ip, int port);
    }
}
