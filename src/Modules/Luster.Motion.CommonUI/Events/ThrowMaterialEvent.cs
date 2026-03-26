#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ThrowMaterialEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       ThrowMaterialEvent.cs
* 创建时间:       2022/7/27 8:58:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      121828af-d417-48ab-8188-404b7bc31f0e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/27 8:58:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    public class ThrowMaterial
    {

    }

    public class ThrowMaterialEvent : PubSubEvent<ThrowMaterial>
    {
    }
}