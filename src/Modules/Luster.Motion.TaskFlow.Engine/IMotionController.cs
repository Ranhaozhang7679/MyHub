#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IMotionController_
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine
* 文 件 名:       IMotionController_.cs
* 创建时间:       2022/5/19 20:36:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      51c1acc8-9cf3-4b3c-aab6-e5c397f0c8c9
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/19 20:36:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine
{
    public interface IMotionController
    {

        /// <summary>
        /// 依赖的流程引擎
        /// </summary>
        IMotionEngine MotionEngine { get; set; }

        /// <summary>
        /// Web服务
        /// </summary>
        IWebService WebService { get; set; }

        /// <summary>
        /// 产品统计时间
        /// ProductStat 产品数据
        /// ImpactData 当前数据
        /// int 实际CT
        /// </summary>
        event Action<ProductStat, ImpactData> ProStatEvent;

        /// <summary>
        /// ProductInfo 产品信息
        /// bool false 代表入料 true 产品出料
        /// </summary>
        event Action<ProductInfo, bool, double> ProductEvent;

        /// <summary>
        /// 小料触发事件
        /// </summary>
        event Action<StationResult> ProductTrowEvent;

        /// <summary>
        /// 机台模式切换
        /// </summary>
        event Action<string, string> ModeChangedEvent;




        /// <summary>
        /// 工站耗时统计事件
        /// WIP 
        /// 入站时间
        /// 出站时间
        /// 做料结果
        /// </summary>
        event Action<string, string, string, bool> StationTimeEvent;

        /// <summary>
        /// 弹窗提示
        /// </summary>
        event Action<bool> CanAutoRunEvent;

        /// <summary>
        /// 回零确认
        /// </summary>
        event Func<HomeType> StartHomeEvent;

        /// <summary>
        /// 回零事件
        /// </summary>
        event Action<string> HomeEndEvent;

        /// <summary>
        /// 报警清除
        /// </summary>
        event Action<AlarmInfo, bool> AlarmProcEvent;

        /// <summary>
        /// 报警信息清除/记录
        /// true:清除 false:记录
        /// </summary>
        event Action<List<AlarmInfo>, bool> AlarmInfosUpEvent;

        /// <summary>
        /// 从DB中获取报警信息
        /// </summary>
        event Action<List<AlarmInfo>> AlarmInfosGetEvent;

        /// <summary>
        /// Plc 报警
        /// </summary>
        event Action<PlcStatus> PlcStatusEvent;

        /// <summary>
        /// 报警对象
        /// </summary>
        AlarmInfo AlarmInfo { get; set; }


        /// <summary>
        /// 设备启动事件
        /// </summary>
        public event Action<string> MachineStartEvent;

        /// <summary>
        /// 设备停止事件
        /// </summary>
        public event Action<string> MachineStopEvent;

        /// <summary>
        /// 设备暂停事件
        /// </summary>
        public event Action<string> MachinePauseEvent;

        /// <summary>
        /// 设备手自动切换事件
        /// </summary>
        public event Action<string> MachineManualEvent;

        /// <summary>
        /// 设备保养事件
        /// </summary>
        public event Action MachinenMaintenanceEvent;


        /// <summary>
        /// 传递引擎状态变化 用于新首页状态更新
        /// </summary>
        event Action<object, EngineStatus, EngineStatus> EngineStatusEvent;

        /// <summary>
        /// 引擎状态
        /// </summary>
        EngineStatus MachineStatus { get; }

        /// <summary>
        /// 系统配置
        /// </summary>
        SystemConfig SysConfig { get; set; }

        /// <summary>
        /// 文件配置
        /// </summary>
        FileConfig FileConfig { get; set; }

        /// <summary>
        /// 是否能够自动运行
        /// </summary>
        /// <param name="mesage"></param>
        /// <returns></returns>
        bool CanAutoRun(out string mesage);

        /// <summary>
        /// 安全门关闭
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DoorIsClosed(out string message);

        /// <summary>
        /// 启动 | 恢复
        /// </summary>
        bool Start();

        /// <summary>
        /// 停止
        /// </summary>
        void Stop(Action action = null);

        /// <summary>
        /// 复位
        /// </summary>
        void Recovery();

        /// <summary>
        /// 暂停
        /// </summary>
        void Pause(bool isAlarm = true, Action action = null);

        /// <summary>
        /// 设备回零
        /// </summary>
        void Home();

        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="module">模块</param>
        void Retry(IMotionModule module);

        /// <summary>
        /// 空跑模式
        /// </summary>
        void EmptyRun(bool isRun = true);

        /// <summary>
        /// 保存系统配置
        /// </summary>
        /// <returns></returns>
        XElement SaveSysConfig();

        /// <summary>
        /// 加载系统配置
        /// </summary>
        void LoadSysConfig(XElement xSys);

        /// <summary>
        /// 启动Button监听
        /// </summary>
        void StartButtonMontor();

        /// <summary>
        /// 是否处于急停状态中
        /// </summary>
        /// <returns></returns>
        bool IsEmergency();

        /// <summary>
        /// 设备回零中
        /// </summary>
        bool IsHoming { get; set; }
        /// <summary>
        /// 启动导航灯
        /// </summary>
        void OnNavigation();

        /// <summary>
        /// 终止监听
        /// </summary>
        void StopButtonMontor();


        /// <summary>
        /// 关闭稼动状态IO
        /// </summary>
        /// <param name="sysOperateIOModel"></param>
        void CloseOperateIO(SystemOperation sysOperateIOModel);

        /// <summary>
        /// 更新蜂鸣器
        /// </summary>
        /// <param name="isOn"></param>
        void SetBuzzer(bool isOn, int OnTime = 200, int OffTime = 200);
        #region 主页数据变更
        // 1.产品做完
        // 2.报警修复
        // 3.暂停后再启动是idle time
        // 4.抛料统计
        // 5.理论CT配置，平均CT是计算最近20片的平均时间

        /// <summary>
        /// 产能初始化
        /// </summary>
        /// <param name="productStat"></param>
        void SetProductInfo(ProductStat productStat);

        /// <summary>
        /// 获取产品质量
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        UploadProductionQuota GetUploadProductionQuota(DateTime startTime, DateTime endTime);
        #endregion

        /// <summary>
        /// 首页数据清零
        /// </summary>
        void ClearProductInfo();

        #region 属性变更事件
        /// <summary>
        /// 属性发生变更处理方法
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="src"></param>
        /// <param name="val"></param>
        void OnPropertyChanged(string propertyName, object src, object val);

        /// <summary>
        /// 属性发生变更事件
        /// </summary>
        event Action<string, object, object> PropertyChanged;
        #endregion

        #region 主动停机
        /// <summary>
        /// 主动停机事件
        /// <para>MotionController</para>
        /// <para>SystemOperation</para>
        /// <para>停机原因</para>
        /// </summary>
        event Action<object, SystemOperation, string> ManualDownEvent;

        /// <summary>
        /// 主动停机事件
        /// </summary>
        /// <param name="msg"></param>
        void OnManualStop(SystemOperation op, string msg);
        #endregion


        #region
        /// <summary>
        /// 换料事件
        /// </summary>
        event Action<string> MaterialChangeEvent;

        /// <summary>
        /// 主动停机事件
        /// </summary>
        /// <param name="msg"></param>
        void OnMaterialChange(string reason);
        #endregion

        #region 
        /// <summary>
        /// 获取模式
        /// </summary>
        /// <returns></returns>
        List<RunModeModel> GetModeList();

        string GetCurrentMode();

        /// <summary>
        /// 切换模式
        /// </summary>
        /// <param name="mode"></param>
        bool SetCurrentMode(string mode);
        #endregion

        #region 切换配方
        /// <summary>
        /// 切换配方
        /// </summary>
        event Action<string> ChangeRecipeEvent;

        void ChangeRecipe(string Recipe);

        #endregion

        #region 工程备份
        /// <summary>
        /// 工程备份
        /// </summary>
        event Action<string> ProjBackUpEvent;

        void ProjBackUp(string projPath);

        #endregion


        #region 角色登录
        /// <summary>
        /// 系统角色
        /// </summary>
        SystemRole SysRole { get; }

        /// <summary>
        /// 切换角色
        /// </summary>
        /// <param name="role"></param>
        void OnUserLogin(SystemRole role);

        /// <summary>
        /// 角色变更事件
        /// </summary>
        event Action<SystemRole, SystemRole> RoleChangedEvent;
        #endregion

        /// <summary>
        /// 换班
        /// </summary>
        void ChangeClass(DateTime dateTime);

        void LightInit();
        Task<bool> Update_SFC_ConnectStatus();

        Task<bool> Update_PDCA_ConnectStatus();

        Task<bool> Update_FFU_ConnectStatus();
        /// <summary>
        /// 更新FFU连接状态
        /// </summary>
        /// <param name="netName"></param>
        /// <param name="isOpen"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void Update_FFUCOM_ConnectStatus(string netName, bool isOpen, string state);

        event Action<string, bool, string> NetStatusChangedEvent;

    }
}