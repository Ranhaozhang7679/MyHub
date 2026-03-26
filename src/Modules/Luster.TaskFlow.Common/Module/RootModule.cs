#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RootModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Engine.Modules
* 文 件 名:       RootModule
* 创建时间:       2021/11/1 18:21:49
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      fbe8c78b-0b3e-4f64-9500-5db6e528459b
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/1 18:21:49
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Module
{
    /// <summary>
    /// 根节点的模块
    /// </summary>
    public class RootModule : AbsModule, ILogic
    {
        public RootModule() : base()
        {
            // Root节点的ID为空
            ID = Guid.Empty;
            Name = "Root";
            this.Icon = "\xe64e";
        }

        public override void InitFunctions()
        {
            AddFunction<Root>();
        }
    }

    /// <summary>
    /// 跟节点函数执行
    /// </summary>
    public class Root : Function
    {
        public Root()
        {
            this.Icon = "\xe64e";
        }

        public override bool DoExcute(out string errMsg)
        {
            foreach (var item in Owner.Children)
            {
                item.DoFunction();
            }

            return base.DoExcute(out errMsg);
        }
    }
}
