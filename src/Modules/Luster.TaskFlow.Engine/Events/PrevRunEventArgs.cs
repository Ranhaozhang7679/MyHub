#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PrevRunEventArgs
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.TaskFlow.Engine.Events
* 文 件 名:       PrevRunEventArgs
* 创建时间:       2021/10/31 17:26:22
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2ec96bf0-68ba-4926-aa2d-874f7c48c9b1
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 17:26:22
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.TaskFlow.Engine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Engine.Events
{
    public class PrevRunEventArgs
    {
        public RunType RunType { get; set; }
    }
}
