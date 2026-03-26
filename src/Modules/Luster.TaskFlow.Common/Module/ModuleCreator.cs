#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleCreator
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       ModuleCreator
* 创建时间:       2021/10/29 16:31:15
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d2145eb3-d525-486e-84f3-7c5d7697b0bb
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/29 16:31:15
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Interfaces;
using System;

namespace Luster.TaskFlow.Common.Module
{
    public class ModuleCreator<T> : IModuleCreator where T : IModule, new()
    {
        /// <summary>
        /// 函数构建
        /// </summary>
        /// <returns></returns>
        public object Create()
        {
            var obj = Activator.CreateInstance(typeof(T));
            if (obj is IModule m)
            {
                m.Name = typeof(T).Name;
                m.Icon = this.Icon;
            }

            return obj;
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        public virtual string Name => typeof(T).Name;

        /// <summary>
        /// 模块别名
        /// </summary>
        public virtual string Alias => L(typeof(T).Name);

        /// <summary>
        /// 提示
        /// </summary>
        public virtual string Tips => L(typeof(T).Name);

        /// <summary>
        /// 模块排序
        /// </summary>
        public virtual int Sort => 10;

        /// <summary>
        /// 字体图标
        /// </summary>
        public virtual string Icon => "\xea1a";

        public virtual string System => "Holo3D";

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string L(string key)
        {
            return key;
        }
    }
}
