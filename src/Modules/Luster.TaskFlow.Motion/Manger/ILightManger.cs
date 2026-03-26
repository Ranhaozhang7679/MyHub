using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    /// <summary>
    /// 配置机台灯和报警信息接口
    /// </summary>
    public interface ILightManager
    {
        void SetSysConfig(object obj);
        /// <summary>
        /// 蜂鸣器报警
        /// </summary>
        /// <param name="isOn"></param>
        /// <param name="OnTime"></param>
        /// <param name="OffTime"></param>
        void SetBuzzer(bool isOn, int OnTime = 150, int OffTime = 150);

        void StartLight();

        void ReadyLight();

        void SetDeviceEngine(IDeviceEngine deviceEngine);
        void RunningLight(Action action = null);

        void PauseLight(bool isException = true);

        void StopLight(bool bIsBuzzer);

        void UnHome();
        /// <summary>
        /// Idle 黄灯闪烁
        /// </summary>
        void IdleLight(int IdleDelayTime, Action action = null);

        void TossLight();

        void OpenLight(LightType lightType, bool isBuzzer = false, int interval = 300);

    }

    /*将设备的外部状态显示控制区分为三个独立部分
    1、ILight，纯灯光部分，带闪烁；
    2、IButtonLight,按钮灯光，带闪烁；
    3、IBuzzer,蜂鸣器，带闪烁；
    
    通过组合三个，实现外部的统一状态，每个部分具备自己的状态，
    若当前部分的状态与上次赋值的状态一致，则取消IO赋值，避免多线程竞争消耗IO资源，
    由于存在多个定制状态，三个部分的状态组合较多，
    因此提供一个总的ILightController实现三个控制的组合；
     */



    public interface ILight
    {
        void Green(bool IsOn, bool IsFlash, int OnTime, int OffTime);

        void Yellow(bool IsOn, bool IsFlash, int OnTime, int OffTime);

        void Red(bool IsOn, bool IsFlash, int OnTime, int OffTime);
    }

    public interface IButtonLight
    {
        void GreenStartBtn(bool IsOn, bool IsFlash, int OnTime, int OffTime);

        void YellowRecoveryBtn(bool IsOn, bool IsFlash, int OnTime, int OffTime);

        void RedPauseBtn(bool IsOn, bool IsFlash, int OnTime, int OffTime);
    }

    public interface IBuzzer
    {
        void Shout(bool IsOn, bool IsFlash, int OnTime, int OffTime);
    }

    public interface IHardwareController : ILight, IButtonLight, IBuzzer
    {
        /// <summary>
        /// Running,常绿
        /// </summary>
        void Run();
        /// <summary>
        /// 报警中，常红，闪鸣
        /// </summary>
        void Alarm();
        /// <summary>
        /// 报警后未回原，闪红
        /// </summary>
        void UnHome();
        /// <summary>
        /// 回原中、回原OK+未启动，常黄
        /// </summary>
        void HomingAndNotStart();
        /// <summary>
        /// Idle，无来料，堵料，10秒后闪亮
        /// </summary>
        void Idle();
        /// <summary>
        /// 缺小料，满料，抛料
        /// </summary>
        void Toss();
    }
}
