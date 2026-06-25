#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LoadModules
* 机器名称:       L05123-NB
* 命名空间:       LusterMotion
* 文 件 名:       LoadModules.cs
* 创建时间:       2022/6/10 13:40:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      530f78f7-43d3-465a-a988-68dcd5b967d7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/10 13:40:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataAccess.Factory;
using Luster.Common.Tools;
using Luster.Module.Motion.Handover.Services;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.EditorUI;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace LusterMotion
{
    public class LoadModules
    {
        /// <summary>
        /// 解决方法配置
        /// </summary>
        private string SolutionConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "SolutionConfig.xml");

        /// <summary>
        /// 设备引擎
        /// </summary>
        private IDeviceEngine _deviceEngine;

        /// <summary>
        /// 模块加载
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// 驱动程序加载
        /// </summary>
        private IModuleFactory _moduleFactory;

        /// <summary>
        /// 流程
        /// </summary>
        private ICommonBus _commonBus;

        /// <summary>
        /// 区域管理
        /// </summary>
        private IRegionManager _regionManager;

        /// <summary>
        /// 数据库
        /// </summary>
        private IDBFactory _dbFactory;

        /// <summary>
        /// 自动信号命令派发器（TES-55 DeviceEngine 接线）。
        /// 持引用防 GC：AutoCommandDispatcher 构造时订阅了 OnAutoCommand，被回收会断链。
        /// 生命周期跟随 LoadModules（Shell 启动期构造，进程级单例）。
        /// </summary>
        private AutoCommandDispatcher _autoSignalDispatcher;

        /// <summary>
        /// 自动信号字寄存器地址（TES-55）。
        /// 生产配置来源（SystemConfig/工程约定）待 TES-37-7 Client 接线接入；
        /// 本 issue 仅接通生产路径骨架（委托指向真实 VModbusServer.ReadRegister）。
        /// </summary>
        private const int AutoSignalRegisterAddress = 0;
        /// <summary>
        /// 加载动画模块
        /// </summary>
        /// <param name="deviceEngine"></param>
        public LoadModules()
        {
        }

        /// <summary>
        /// 加载回调
        /// </summary>
        public event Action<int, string> LoadingEvent;

        /// <summary>
        /// 模块加载
        /// </summary>
        public void LoadModule()
        {
            LoadingEvent?.Invoke(0, "程序集解析中");
            var app = (Application.Current as App) as PrismApplication;
            if (app != null)
            {
                _deviceEngine = app.Container.Resolve<IDeviceEngine>();
                _moduleFactory = app.Container.Resolve<IModuleFactory>();
                _commonBus = app.Container.Resolve<ICommonBus>();
                _regionManager = app.Container.Resolve<IRegionManager>();
                _dbFactory = app.Container.Resolve<IDBFactory>();
            }

            // 1.设备加载驱动
            LoadingEvent?.Invoke(20, "驱动程序加载中...");
            _deviceEngine.LoadDrivers();

            // TES-55 DeviceEngine 接线：在 Initialize(InitSolution 内触发) 之前订阅 InitializedEvent,
            // 回调里 VModbusServer 已就绪 → 构造读取委托注入 HandoverAutoSignalService → Configure + 派发器。
            // 服务侧已用注入式委托解耦,本处仅薄适配,不改 IHandoverAutoSignalService/AutoCommandDispatcher 契约。
            WireHandoverAutoSignal();

            // 2.只加载一次工程
            LoadingEvent?.Invoke(40, "加载工程信息...");
            _commonBus.InitSolution(SolutionConfig);

            
            // 3.硬件设备初始化
            LoadingEvent?.Invoke(60, "运控模块加载中");
            string dllPath = Path.Combine(ReflectionTool.GetAssemblyPath(), "Motions");
            _moduleFactory.LoadModules(dllPath);
            _moduleFactory.GetModuleNode(AppConfig.System);


            // 4.3D模块加载
            LoadingEvent?.Invoke(80, "3D模块加载中");
            string holoDll = Path.Combine(ReflectionTool.GetAssemblyPath(), "Holo3D", "Modules");
            _moduleFactory.LoadModules(holoDll);
            _moduleFactory.GetModuleNode("Holo3D");

            //4.数据库转储检查
            _commonBus.CheckBackUpFile();

            _commonBus.ChangeLanguage();

            // 3.模块配方
            LoadingEvent?.Invoke(100, "程序启动中...");
            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// Handover 自动信号 DeviceEngine 生产接线（TES-55）。
        /// <para>订阅 <see cref="IDeviceEngine.InitializedEvent"/>:Initialize 完成、VModbusServer 已入表后,
        /// 取 VModbusServer 实例 → 构造 <c>Func&lt;ushort&gt;</c> 读取委托(<c>server.ReadRegister(address)</c>)
        /// 注入 <see cref="HandoverAutoSignalService"/> → <see cref="IHandoverAutoSignalService.Configure"/> +
        /// <see cref="AutoCommandDispatcher"/> 订阅派发 → <see cref="IHandoverAutoSignalService.Start"/>。</para>
        /// <para>容错:工程未含 VModbusServer 时跳过(不抛);Start 早于 Home 时 Server 未启动,
        /// HandoverAutoSignalService.ScanOnce 内 try/catch 容忍、Home 后自动恢复采样。</para>
        /// </summary>
        private void WireHandoverAutoSignal()
        {
            _deviceEngine.InitializedEvent += (engine, task) =>
            {
                // 工程未配置 VModbusServer 时跳过接线(不阻断其他设备初始化)
                var server = engine.GetVDevices<VModbusServer>().FirstOrDefault();
                if (server == null)
                {
                    return;
                }

                // 读取委托:指向真实 VModbusServer.ReadRegister,生产路径连通
                int address = AutoSignalRegisterAddress;
                Func<ushort> readAutoSignal = () => server.ReadRegister(address);

                var service = new HandoverAutoSignalService(readAutoSignal);
                service.Configure(server.Name, address);

                // 派发器订阅 OnAutoCommand → IMotionController 命令(仅上升沿),生命周期由本类持有防 GC
                var app = (Application.Current as App) as PrismApplication;
                var controller = app?.Container.Resolve<IMotionController>();
                if (controller != null)
                {
                    _autoSignalDispatcher = new AutoCommandDispatcher(service, controller);
                }

                service.Start();
            };
        }

        /// <summary>
        /// 模块卸载
        /// 硬件要进行释放
        /// </summary>
        public void Unload()
        {
            LoadingEvent?.Invoke(0, "程序集开始卸载...");

            var app = (Application.Current as App) as PrismApplication;
            if (app != null)
            {
                _deviceEngine = app.Container.Resolve<IDeviceEngine>();
                _moduleFactory = app.Container.Resolve<IModuleFactory>();
                _commonBus = app.Container.Resolve<ICommonBus>();
                _mController = app.Container.Resolve<IMotionController>();
            }

            // 0.关闭报警灯
            _mController.StopButtonMontor();

            // 是否需要保存
            LoadingEvent?.Invoke(10, "保存解决方案...");
            _commonBus.SaveSolution(SolutionConfig);

            // 保存任务流程
            LoadingEvent?.Invoke(20, "任务流程保存中...");
            _commonBus.OnSaveRecipe();

            // 保存系统配置
            LoadingEvent?.Invoke(50, "系统配置保存中...");
            _commonBus.OnSaveSystem();

            // 保存系统配置
            LoadingEvent?.Invoke(70, "错误配置保存中...");
            _commonBus.OnSaveError();

            LoadingEvent?.Invoke(80, "模块布局保存...");
            _commonBus.OnAvalonLayoutSave();

            // 硬件设备关闭连接
            LoadingEvent?.Invoke(90, "硬件设备关闭连接...");
            if (_deviceEngine.IsNeedSave)
            {
                _deviceEngine.Save();
            }

            _deviceEngine.Stop();
            _deviceEngine.Dispose();

            LoadingEvent?.Invoke(100, "卸载完成...");
        }
    }
}