#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IModuleCreator
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       IModuleCreator
* 创建时间:       2021/10/29 14:33:20
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      35e40da1-4e2a-4642-9fa4-1de2b3720732
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/29 14:33:20
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Module
{
    /// <summary>
    /// 模块构建
    /// </summary>
    public interface IModuleCreator
    {
        #region 1.方法
        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string L(string key);

        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        object Create();
        #endregion

        #region 2.描述属性
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 别名
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// 提示
        /// </summary>
        string Tips { get; }

        /// <summary>
        /// 模块排序
        /// </summary>
        int Sort { get; }

        /// <summary>
        /// 字体图标
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// 隶属组
        /// </summary>
        string System { get; }
        #endregion
    }
}
