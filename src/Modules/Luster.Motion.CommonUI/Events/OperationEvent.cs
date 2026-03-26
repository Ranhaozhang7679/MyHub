#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OperationEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       OperationEvent.cs
* 创建时间:       2022/9/23 10:43:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2a0b3685-042c-4833-be3a-859bb9e04615
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/23 10:43:22
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct;


namespace Luster.Motion.CommonUI.Events
{
    public class OperationEvent : PubSubEvent<StatusChanged>
    {
    }

    public class StatusChanged
    {
        public EngineStatus Src { get; set; }

        public EngineStatus Dst { get; set; }

        public StatusChanged(EngineStatus src, EngineStatus dst)
        {
            Src = src;
            Dst = dst;
        }
    }
}