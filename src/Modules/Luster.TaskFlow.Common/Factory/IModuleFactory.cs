#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ILoadFactory
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.TaskFlow.Engine.Factory
* 文 件 名:       ILoadFactory
* 创建时间:       2021/10/31 17:28:18
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      d1910b65-061c-4f6f-bc0c-825a751ba524
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 17:28:18
* 修 改 人:		  luster
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.TaskFlow.Common.Module;
using Luster.Common.DataStruct.DataModels;

namespace Luster.TaskFlow.Common
{
    /// <summary>
    /// 动态加载程序集
    /// </summary>
    public interface IModuleFactory
    {
        /// <summary>
        /// 加载模块列表
        /// </summary>
        void LoadModules(string dir, string dllStart = "Luster.Module");

        /// <summary>
        /// 获取Creator生成器
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>模块</returns>
        IModuleCreator GetCreator(string moduleName, string system);

        /// <summary>
        /// 获取模块节点
        /// </summary>
        /// <returns></returns>
        LNode GetModuleNode(string system);

        /// <summary>
        /// 通过模块名称获取
        /// </summary>
        /// <param name="module">模块的名称</param>
        /// <returns>对应模块和子函数</returns>
        List<LNode> GetFunctions(string module);

        /// <summary>
        /// 获取函数所有节点
        /// </summary>
        /// <returns></returns>
        List<LNode> GetAllFunctionNodes();
    }
}