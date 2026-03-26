#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IDirection
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Interfaces
* 文 件 名:       IDirection.cs
* 创建时间:       2022/3/9 10:13:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      087b3c92-4dbd-440a-b098-cefb82d0f63b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/9 10:13:43
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Interfaces
{
    /// <summary>
    /// 方向信息
    /// </summary>
    public interface IDirection
    {
        VDirection Direction { get; }
    }
}