#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       IModule
* 创建时间:       2021/10/29 14:32:57
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      c301875a-311a-4a59-bc82-974a7dcdd5d5
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/29 14:32:57
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Interfaces;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.DataModels;
using System.Collections.Concurrent;

namespace Luster.TaskFlow.Common.Module
{
    /// <summary>
    /// 模块接口
    /// </summary>
    public interface IModule : IXMLParser, IDisposable, ICloneable, INameInfo
    {
        #region 属性

        /// <summary>
        /// 唯一标识
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        string StatusMsg { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        int Sort { get; set; }

        ICoord3 RefCoord { get; }

        /// <summary>
        /// 运行状态
        /// </summary>
        RunStatus Status { get; set; }

        /// <summary>
        /// 是否激活状态
        /// </summary>
        bool IsActive { get; set; }



        /// <summary>
        /// 模块函数列表
        /// </summary>
        Dictionary<string, Type> FuncTypes { get; set; }

        /// <summary>
        /// 当前函数
        /// </summary>
        IFunction TaskFunction { get; set; }

        /// <summary>
        /// 通过名称设置函数
        /// </summary>
        /// <param name="funcName">函数名称</param>
        void SetFunction(string funcName);

        /// <summary>
        /// 参数列表
        /// </summary>
        Dictionary<string, ParameterAttribute> Parameters { get; set; }

        /// <summary>
        /// 任务模块集合
        /// </summary>
        IModuleCollection TaskModules { get; set; }

        /// <summary>
        /// 获取参考坐标系
        /// </summary>
        /// <returns></returns>
        ParameterAttribute GetRefCoord();

        /// <summary>
        /// 隶属父级节点
        /// </summary>
        IModule Parent { get; set; }

        /// <summary>
        /// 是否当前节点
        /// </summary>
        bool IsCurrent { get; set; }

        /// <summary>
        /// 是否被选中
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// 是否显示渲染结果
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// 交互器
        /// </summary>
        IInteractor Interactor { get; set; }

        /// <summary>
        /// 交互模式 设计时|运行时
        /// </summary>
        DesignMode Mode { get; set; }

        /// <summary>
        /// 标签注释
        /// </summary>
        bool LabelVisible { get; set; }
        #endregion

        #region 树结构相关

        /// <summary>
        /// 树层级
        /// </summary>
        int Level { get; set; }

        /// <summary>
        /// 演员的颜色属性
        /// </summary>
        string ActorColor { get; set; }

        /// <summary>
        /// 子集合
        /// </summary>
        List<IModule> Children { get; set; }

        #endregion

        #region 函数执行

        /// <summary>
        /// 执行函数
        /// </summary>
        bool DoFunction();

        #endregion

        #region 对算子进行赋值的公共方法
        /// <summary>
        /// 给参数设置引用
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="refModuleID">引用的ID</param>
        /// <param name="refParamName">引用的参数名</param>
        void SetInParameter(string name, Guid refModuleID, string refParamName = "");
        /// <summary>
        /// 对输入参数进行复制
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="val">参数名称</param>
        void SetInParameter(string name, object val);

        /// <summary>
        /// 对多个参数同时赋值
        /// </summary>
        /// <param name="parameters">参数字典</param>
        void SetInParameters(Dictionary<string, object> parameters);

        /// <summary>
        /// 获取单个参数结果值
        /// </summary>
        /// <param name="pName">参数</param>
        /// <returns>值对象</returns>
        object GetOutParameter(string pName);

        /// <summary>
        /// 同时获取多个参数值
        /// </summary>
        /// <param name="pNames">值对象</param>
        /// <returns></returns>
        Dictionary<string, object> GetOutParameters(params string[] pNames);

        /// <summary>
        /// 根据模块获取所有参数值
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        Dictionary<string, string> GetModuleParameters();

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns>参数名称和对的参数类型</returns>
        Dictionary<string, Type> GetOutParameterNames();

        /// <summary>
        /// 通过引用类型来查找当前模块引用了那些Module
        /// </summary>
        /// <param name="refType"></param>
        /// <returns></returns>
        IModule[] GetReferModules(Type refType);
        #endregion

        #region 耗时相关

        /// <summary>
        /// 好事
        /// </summary>
        float TimeConsuming { get; set; }

        /// <summary>
        /// 启动计时器
        /// </summary>
        void StartTimer();

        /// <summary>
        /// 停止计时器
        /// </summary>
        void StopTimer();

        #endregion

        #region 通用方法

        /// <summary>
        /// 事件消息
        /// </summary>
        event Action<LogType, string, string> LogEvent;

        /// <summary>
        /// 日志触发
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="message">消息</param>
        void OnLog(LogType type, string message, string logThread = "");

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string L(string key);

        /// <summary>
        /// 多语言
        /// </summary>
        event Func<string, string> LanguageEvent;

        /// <summary>
        /// 调用渲染方法
        /// </summary>
        void Render();

        /// <summary>
        /// 标签渲染
        /// </summary>
        void LabelRender();

        /// <summary>
        /// 模块变更事件
        /// </summary>
        event Action<IModule, ModuleUpdate> UpdateEvent;

        /// <summary>
        /// 模块变更通知
        /// </summary>
        void OnUpdate(ModuleUpdate mUpdate);

        /// <summary>
        /// 事件触发
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="srcVal"></param>
        /// <param name="newV"></param>
        void OnPropertyChanged(string propertyName, object srcVal, object newV);

        /// <summary>
        /// 事件变更
        /// </summary>
        event Action<IModule, string, object, object> PropertyChangedEvent;

        /// <summary>
        /// 获取模块树形结构
        /// </summary>
        /// <param name="isOutParam">是否输出参数</param>
        /// <param name="pIcon">对应参数图标</param>
        /// <param name="typeFunc">满足的类型</param>
        /// <returns>节点信息</returns>
        LNode GetTreeNode(bool isOutParam = false, string pIcon = "", Func<Type, bool> typeFunc = null);

        LNode GetTreeNodeForFlow(bool isOutParam = false, string pIcon = "", Func<Type, bool> typeFunc = null);

        #endregion

        /// <summary>
        /// 缓存对象
        /// </summary>
        ICacheManager CacheManager { get; set; }

        /// <summary>
        /// 数据ID
        /// </summary>
        string DataID { get; set; }

        /// <summary>
        /// 是否是相同的根节点
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        bool IsSameRoot(IModule module);

        /// <summary>
        /// 更新引用
        /// </summary>
        void UpdateReferences();

        /// <summary>
        /// 获取引用的模块
        /// </summary>
        /// <returns></returns>
        List<LReference> GetReferences();

        /// <summary>
        /// 获取被哪些模块引用
        /// </summary>
        /// <returns></returns>
        List<LReference> GetByReferences();

        /// <summary>
        /// 删除所有引用关系
        /// </summary>
        void RemoveAllRefs();

        /// <summary>
        /// 注册被引用
        /// </summary>
        /// <param name="module"></param>
        /// <param name="refName"></param>
        /// <param name="byRefName"></param>
        void RegisterByRef(IModule module, string refName, string byRefName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="byRefName"></param>
        void UnRegisterByRef(IModule module, string byRefName);
    }
}