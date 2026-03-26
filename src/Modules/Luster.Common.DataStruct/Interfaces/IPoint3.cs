#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IPoint3
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IPoint3.cs
* 创建时间:       2022/7/22 9:21:11
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      62b12ff2-dacc-4968-9896-c7614c8328f7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/22 9:21:11
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 点接口
    /// </summary>
    public interface IPoint3
    {
        double X { get; set; }

        double Y { get; set; }
        double Z { get; set; }

        double I { get; set; }

        double J { get; set; }

        double K { get; set; }
    }
}