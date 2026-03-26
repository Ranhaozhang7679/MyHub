#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IActive
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IActive.cs
* 创建时间:       2022/4/29 15:56:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1cd0de08-c503-4063-a3d2-95132fa17534
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/29 15:56:59
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
    public interface IActive
    {
        /// <summary>
        /// 激活的对象参数名称
        /// </summary>
        string ParamName { get; }
    }

    public interface ICloud : IActive
    {

    }

    public interface IStl : IActive
    {

    }

    public interface ICoord : IActive
    {

    }
}