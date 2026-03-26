#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SysOperationBtnEvent
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       SysOperationBtnEvent.cs
* 创建时间:       2023/1/3 10:50:01
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      705b6781-d254-411b-819c-56aee580f5dc
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/1/3 10:50:01
* 修 改 人:		  L05590
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
    public class SysOperationBtnEvent: PubSubEvent<DataStruct.Enums.SystemOperation>
    {
    }
}