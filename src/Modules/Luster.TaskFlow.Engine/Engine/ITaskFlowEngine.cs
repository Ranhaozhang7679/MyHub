#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ITaskFlowEngine
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.TaskFlow.Engine.Engine
* 文 件 名:       ITaskFlowEngine
* 创建时间:       2021/10/31 17:19:56
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      d17e36a2-453c-449b-ab72-c51bfccbead9
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 17:19:56
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Module;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.TaskFlow.Engine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.TaskFlow.Common;
using Luster.Common.DataStruct.Enums;

namespace Luster.TaskFlow.Engine
{
    /// <summary>
    /// 任务流引擎
    /// </summary>
    public interface ITaskFlowEngine : IList<IModule>, IXMLParser, ICloneable
    {
        #region 属性

        /// <summary>
        /// 解析算子的工厂
        /// </summary>
        IModuleFactory Factory { get; set; }

        /// <summary>
        /// 运行的编号
        /// </summary>
        string ExcutorNo { get; set; }

        #endregion

        #region 事件

        /// <summary>
        /// 运行前事件
        /// </summary>
        event Action<PrevRunEventArgs> PrevRunEvent;

        /// <summary>
        /// 运行后事件
        /// </summary>
        event Action<PostRunEventArgs> PostRunEvent;

        /// <summary>
        /// Log时间
        /// </summary>
        event Action<LogType, string,string> LogEvent;

        /// <summary>
        /// 多语言
        /// </summary>
        event Func<string, string> LanguageEvent;

        /// <summary>
        /// 模块更新事件
        /// </summary>
        event Action<IModule, ModuleUpdate> MouleUpdateEvent;

        /// <summary>
        /// 输入或输出参数发生了变更
        /// </summary>
        event Action<ITaskFlowEngine> ParameterUpdateEvent;
        #endregion

        #region 运行模块

        /// <summary>
        /// 执行单个模块
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool RunOne(Guid id);

        /// <summary>
        /// 执行某个模块
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Run(Guid id);

        /// <summary>
        /// 执行流程所有模型
        /// </summary>
        /// <returns>是否执行成功</returns>
        bool RunAll();

        #endregion

        #region 算子管理

        /// <summary>
        /// 添加新的算子
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="index">模块位置</param>
        void Insert(IModule module, IModule pModule = null, int index = -1);

        /// <summary>
        /// 通过模块名称和对应的函数名称来创建
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="funcName">函数名称</param>
        /// <returns>模块</returns>
        IModule CreateByName(string moduleName, string funcName);

        #endregion

        #region 通用方法

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id">guid</param>
        /// <returns></returns>
        IModule Get(Guid id);

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();

        /// <summary>
        /// 解析子节点
        /// </summary>
        /// <param name="xModule"></param>
        /// <param name="parent"></param>
        IModule Parse(XElement xModule, IModule parent, int index = -1);

        /// <summary>
        /// 加载工程文件
        /// </summary>
        /// <param name="projfile">工程文件路径，后缀是*.3dproj</param>
        void LoadProjfile(string projfile);
        #endregion

        #region 输入和输出参数配置

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="isInoput">是否是输入参数</param>
        void AddParameterInfo(Guid moduleId, string paramName);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="node"></param>
        void AddParameterInfo(LNode node);

        /// <summary>
        /// 更新参数
        /// </summary>
        /// <param name="parameterInfos"></param>
        void UpDateParameterInfo(List<ParameterInfo> parameterInfos);

        /// <summary>
        /// 移除参数
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="paramName">参数名称</param>
        void RemoveParameterInfo(Guid moduleId, string paramName);

        /// <summary>
        /// 获取参数信息
        /// </summary>
        /// <returns>获取当前任务所有参数信息</returns>
        List<ParameterInfo> GetParameterInfo();

        /// <summary>
        /// 添加额外信息,一般用来记录序号
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="val">值</param>
        void AddExternInfo(string name, object val);

        /// <summary>
        /// 设置输入参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        void SetInput(string name, object value);

        /// <summary>
        /// 获取输出参数
        /// </summary>
        /// <param name="pNames">参数名称，如果不为空，则默认获取所有参数</param>
        /// <returns>返回所有输出参数</returns>
        List<ParameterInfo> GetOutputs(params string[] pNames);
        #endregion
    }
}