#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Motion
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Modules
* 文 件 名:       Motion.cs
* 创建时间:       2022/5/24 9:50:06
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      19940ac8-e0ee-45a5-a3e4-2c8e3e1d8f2b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 9:50:06
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 根节点的模块
    /// </summary>
    public class Root : MotionModule, ILogic
    {
        public Root() : base()
        {
            // Root节点的ID为空
            ID = Guid.Empty;
            Name = "Root";
            this.Icon = "\xe64e";
        }

        public override void InitFunctions()
        {
            AddFunction<Group>();
        }
    }
}