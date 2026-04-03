using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Motion.DataStruct;
using Luster.Motion.SubSystem.Models;
using System.Reflection;
using Luster.Motion.Integration.WorkCardVerify;

namespace Luster.Motion.CommonUI
{
    /// <summary>
    /// 通用事件总线
    /// </summary>
    public interface ICommonBus
    {
        /// <summary>
        /// 配方
        /// </summary>
        event Action<XElement> LoadRecipeEvent;

        /// <summary>
        /// 是否需要保存
        /// </summary>
        bool IsNeedSave { get; set; }

        /// <summary>
        /// 编辑计数（用于判断是否有未保存的修改）
        /// </summary>
        int EditCount { get; }

        /// <summary>
        /// 事件总线
        /// </summary>
        IEventAggregator EventBus { get; set; }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        EngineStatus GetStatus();

        /// <summary>
        /// 当前页面
        /// </summary>
        PageModel CurrentPage { get; set; }

        /// <summary>
        /// 导航
        /// </summary>
        /// <param name="regionName"></param>
        void OnNavigate(PageModel pageModel);

        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="logInfo"></param>
        void OnLog(LogInfo logInfo);

        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="logInfo"></param>
        void OnLog(LogType logType, string logInfo, string logThreadNo = "");

        /// <summary>
        /// 保存系统配置
        /// </summary>
        void OnSaveSystem(string sysConfig = "");


        /// <summary>
        /// 保存系统配置
        /// </summary>
        void OnSaveError(string sysConfig = "");

        /// <summary>
        /// 加载系统配置
        /// </summary>
        void OnLoadSystem(string sysConfig = "");

        /// <summary>
        /// 激活配方
        /// </summary>
        /// <param name="recipeName">配方名称</param>
        void OnActiveRecipe(Recipe recipe);

        /// <summary>
        /// 保存配方
        /// </summary>
        /// <param name="saveRecipe"></param>
        void OnSaveRecipe(string saveRecipe = "");


        /// <summary>
        /// 配方备份
        /// </summary>
        /// <param name="backUpRecipe"></param>
        void OnBackUpRecipe(bool IsMaual = false);

        /// <summary>
        /// 保存
        /// </summary>
        void OnSaveDevice();

        void PublishEvent<T, K>(K eventData) where T : PubSubEvent<K>, new();

        /// <summary>
        /// 当前的配方
        /// </summary>
        Recipe CurrentRecipe { get; set; }

        /// <summary>
        /// 用户配置信息
        /// </summary>
        UserConfig UserConfig { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        UserModel CurrentUser { get; set; }

        /// <summary>
        /// 工程信息
        /// </summary>
        ProjectInfo ProjInfo { get; set; }

        /// <summary>
        /// 条码配置
        /// </summary>

        BarcodeConfig BarConfig { get; set; }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="model"></param>
        void OnUserLogin(UserModel model);
        void OnUserRoleChange(UserInfo userInfo);
        void OnRemainTimeChange(int remainTime);

        /// <summary>
        /// 模块布局保存
        /// </summary>
        void OnAvalonLayoutSave();

        /// <summary>
        /// 工程列表
        /// </summary>
        List<ProjectInfo> ProjectList { get; set; }

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string L(string key);

        /// <summary>
        /// 模式切换
        /// </summary>
        /// <param name="deviceMode"></param>
        void ChangeDeviceMode(DeviceMode deviceMode);

        #region 数据映射
        /// <summary>
        /// 获取任务对应的数据
        /// </summary>
        /// <returns></returns>
        List<LNode> GetOutDataTree();

        /// <summary>
        /// 获取数据映射
        /// </summary>
        /// <returns></returns>
        List<MapData> GetMapDatas();

        ///// <summary>
        ///// 添加映射对象
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="key"></param>
        ///// <param name="alias"></param>
        //void AddMapData(MapData mapData);

        ///// <summary>
        ///// 移除映射对象
        ///// </summary>
        ///// <param name="mapData">映射对象</param>
        //void RemoveMapData(MapData mapData);

        ///// <summary>
        ///// 对象更新
        ///// </summary>
        ///// <param name="data"></param>
        //void UpdateMapData(MapData mapData);       

        /// <summary>
        /// 更新数据匹配源
        /// </summary>
        /// <param name="mapData"></param>
        void UpdayeMapDataSource(List<MapData> mapData, List<MapData> newMapDatas, List<MapData> removeMapData);
        #endregion

        /// <summary>
        /// 触发变更记录
        /// </summary>
        /// <param name="changeType">新增/修改/删除</param>
        /// <param name="module">模块</param>
        /// <param name="prop">属性</param>
        /// <param name="content">变更内容</param>
        void OnChangeRecord(OperationType changeType, string module, string prop, string content);

        #region 工程管理
        /// <summary>
        /// 加载工程
        /// </summary>
        /// <param name="proj"></param>
        void InitSolution(string solution = "");

        /// <summary>
        /// 添加工程
        /// </summary>
        /// <param name="projName"></param>
        /// <param name="slnPath"></param>
        /// <param name="recipe"></param>
        void AddProject(string projName, string slnPath, string recipe);

        /// <summary>
        /// 删除工程
        /// </summary>
        /// <param name="projName"></param>
        void RemoveProject(string projName);

        /// <summary>
        /// 打开已有工程
        /// </summary>
        /// <param name="projname"></param>
        void OpenExistProj(string projName, string slnPath);

        /// <summary>
        /// 保存Solution
        /// </summary>
        void SaveSolution(string solution = "");

        /// <summary>
        /// 巡检转储数据库
        /// </summary>
        void CheckBackUpFile();

        /// <summary>
        /// 切换语言
        /// </summary>
        void ChangeLanguage();

        void SaveChartConfig(string chartconfig, List<ChartDataModel> chartList);

        List<ChartDataModel> LoadChartList();
        #endregion

        /// <summary>
        /// 打开或者新建3D
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        void NewOrOpenHolo3D(IMotionModule holoModule, string taskName, bool isNew = true);

        /// <summary>
        /// 加载SystemUI
        /// </summary>
        void RegisterSystemDll();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Type GetUiModuleType();

        /// <summary>
        /// 获取主页类型
        /// </summary>
        /// <returns></returns>
        Type GetMainContentType();

        /// <summary>
        /// 获取工具栏类型
        /// </summary>
        /// <returns></returns>
        Type GetToolbarContentType();

        /// <summary>
        /// 对历史数据做清除处理
        /// </summary>
        void StartHistoryFileDelete();

        string PickAvailableDrive();
    }
}
