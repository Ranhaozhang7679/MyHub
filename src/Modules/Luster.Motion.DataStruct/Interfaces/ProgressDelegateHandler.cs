#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProgressDelegateHandler
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.DataStruct.Interfaces
* 文 件 名:       ProgressDelegateHandler.cs
* 创建时间:       2023/2/27 14:50:24
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ef9d43e5-d1a4-46f3-9894-6b0016fe3d6d
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/27 14:50:24
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 进度条委托信息
    /// </summary>
    /// <param name="total"></param>
    /// <param name="curVal"></param>
    /// <param name="msg"></param>
    public delegate void ProgressDelegateHandler(int total, ref int curVal, string msg);
}