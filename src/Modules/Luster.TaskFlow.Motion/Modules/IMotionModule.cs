#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IMotionModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion
* 文 件 名:       IMotionModule.cs
* 创建时间:       2022/5/18 8:12:01
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6e096c5f-6afc-48da-b5c2-9c47f8ec0457
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 8:12:01
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.TaskFlow.Motion
{
    /// <summary>
    /// 运动模块
    /// </summary>
    public interface IMotionModule : IModule
    {
        /// <summary>
        /// 容器控制对象
        /// </summary>
        IIocManager Ioc { get; set; }

        /// <summary>
        /// 订单管理
        /// </summary>
        IOrderManager OrderManager { get; set; }

        /// <summary>
        /// 卷料管理
        /// </summary>
        IRollManager RollManager { get; set; }

        /// <summary>
        /// 错误管理器
        /// </summary>
        IErrorManager ErrorManager { get; set; }

        /// <summary>
        /// 配置管理
        /// </summary>
        IConfigManager ConfigManager { get; set; }

        /// <summary>
        /// 光源控制器
        /// </summary>
        ILightManager LightManager { get; set; }

        /// <summary>
        /// 设备模块
        /// </summary>
        IDeviceEngine DeviceEngine { get; set; }

        /// <summary>
        /// 暂停中断
        /// </summary>
        ManualResetEventSlim BrokenOff { get; set; }

        /// <summary>
        /// 数据库访问接口
        /// </summary>
        IDbHelper DbHelper { get; set; }

        /// <summary>
        /// 运行模式
        /// </summary>
        RunMode RunMode { get; set; }

        /// <summary>
        /// 报警类别
        /// </summary>
        AlarmInfo AlarmInfo { get; set; }

        /// <summary>
        /// 标准CT
        /// </summary>
        int CT { get; set; }

        /// <summary>
        /// 隶属工站
        /// </summary>
        IMotionModule Station { get; set; }

        /// <summary>
        /// 开始事件
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        DateTime EndTime { get; set; }

        /// <summary>
        /// 暂停时间
        /// </summary>
        float PauseTime { get; set; }



        /// <summary>
        /// 所属父节点
        /// </summary>
        new IMotionModule Parent { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        new List<IMotionModule> Children { get; set; }

        /// <summary>
        /// 前一步骤
        /// </summary>
        IMotionModule PrevModule { get; set; }

        /// <summary>
        /// 下一步 
        /// 目前有三种类型
        /// 正常下一步、超时跳转、失败
        /// </summary>
        IMotionModule NextModule { get; set; }

        /// <summary>
        /// 报警事件
        /// </summary>
        event Action<AlarmInfo> AlarmEvent_module;

        /// <summary>
        /// 模型类型，定义是否起点，是否终点
        /// </summary>
        ModuleType ModuleType { get; }

        /// <summary>
        /// 是否终止
        /// </summary>
        bool IsBreak { get; set; }

        /// <summary>
        /// 是否包含输出数据
        /// </summary>
        bool IsOutData { get; set; }

        /// <summary>
        /// 运行次数
        /// </summary>
        int RunNum { get; set; }

        /// <summary>
        /// 获取当前模块对应的ID
        /// </summary>
        /// <returns></returns>
        int ThreadID { get; set; }

        /// <summary>
        /// 软件版本信息
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// 报警代码
        /// </summary>
        string ErrorCode { get; set; }
        /// <summary>
        /// 报警内容
        /// </summary>
        string ErrorContent { get; set; }

        /// <summary>
        /// 是否使用Log
        /// </summary>
        /// <returns></returns>
        bool IsUseLog();
        #region 报警
        /// <summary>
        /// 发出报警信息
        /// </summary>
        /// <param name="alarmMsg">报警信息</param>
        void OnAlarm(AlarmType alarmType, string alarmMsg, string code = "", string module = "");

        /// <summary>
        /// 报警
        /// </summary>
        void OnAlarm_Module();

        /// <summary>
        /// 清除报警
        /// </summary>
        void ClearAlarm();
        #endregion

        #region 通用方法
        /// <summary>
        /// 模块校验
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        void ModuleValid();

        /// <summary>
        /// 获取参数别名
        /// </summary>
        /// <param name="motion"></param>
        /// <param name="alias"></param>
        void GetModuleAlias(IMotionModule motion, ref string alias);

        /// <summary>
        /// 更新工站对象，因为涉及到引用对象，所以需要主动更新工站
        /// </summary>
        void UpdateStation();

        /// <summary>
        /// 是否有其他对象引用
        /// </summary>
        /// <param name="otherRef"></param>
        /// <returns></returns>
        bool IsOtherRef(out Dictionary<Guid, string> otherRef);
        #endregion

        #region 产品相关事件
        /// <summary>
        /// 产品入料
        /// </summary>
        event Action<IMotionModule, StationResult, List<LColumn>> ProLoadedEvent;

        /// <summary>
        /// 入栈信息
        /// </summary>
        /// <param name="jigCode">治具码</param>
        /// <param name="barCode">产品二维码</param>
        /// <param name="isFirst">是否首站</param>
        /// <param name="prevResult">前工站结果</param>
        void OnProLoaded(string jigCode, string barCode, bool prevResult, DateTime dateTime);

        /// <summary>
        /// 产品出料（）
        /// </summary>
        event Action<IMotionModule, StationResult> ProUnloadedEvent;

        /// <summary>
        /// 产品出料
        /// </summary>
        /// <param name="sResult"></param>
        /// <param name="datas"></param>
        void OnProUnloaded(StationResult sResult);

        /// <summary>
        /// 产品抛料
        /// </summary>
        event Action<StationResult, string, string> ProThrowEvent;

        /// <summary>
        /// 产品抛料
        /// </summary>
        void OnProThrow(StationResult stationResult, string material = "");

        /// <summary>
        /// 数据上传
        /// </summary>
        /// <param name="code"></param>
        /// <param name="keyDatas">数据对象</param>
        void OnDataUpload(string code, List<LColumn> keyDatas);



        /// <summary>
        /// 工站结束事件
        /// </summary>
        /// <param name="WIP"></param>
        /// <param name="InputTime"></param>
        /// <param name="OutputTime"></param>
        /// <param name="Result"></param>
        public void OnStationTime(string WIP, string InputTime, string OutputTime, bool Result);



        /// <summary>
        /// 产品抛料
        /// </summary>
        event Action<Guid, string, List<LColumn>> DataUploadEvent;

        /// <summary>
        /// 产品阻塞事件
        /// </summary>
        event Action<IMotionModule, double, string, string> ProBlockEvent;

        /// <summary>
        /// 产品阻塞
        /// </summary>
        /// <param name="blockCt">阻塞时间</param>
        /// <param name="errCode">错误编码 block/</param>
        /// <param name="reason">阻塞原因</param>
        void OnProBlock(double blockCt, string errCode, string reason);


        event Func<string, InteractiveType> DialogBoxEvent;

        InteractiveType OnDialogBox(string content);
        #endregion


        // 太科电批注册事件
        event Action<object, string> TaiKeScrewRegisterEvent;
        void TaiKeScrewRegister(object obj, string name);


        //压力注册事件
        event Action<object, string> PressRegisterEvent;

        //Cowling 扣合压力
        event Action<object, string> ToeinForceRegisterEvent;
        void ToeinForceRegister(object obj, string name);



        void PressRegister(object obj, string name);


        //工站结束事件
        event Action<string, string, string, bool> StationTimeEvent;

    }
}