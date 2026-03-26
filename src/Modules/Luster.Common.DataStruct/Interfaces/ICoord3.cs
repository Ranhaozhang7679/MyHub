#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ICoord3
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       ICoord3.cs
* 创建时间:       2022/7/27 13:48:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      165c73b7-f9be-4dee-9466-cd68bc509334
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/27 13:48:29
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
    /// 三维坐标系接口
    /// </summary>
    public interface ICoord3
    {
        double[] Matrix { get; set; }
    }

    /// <summary>
    /// 将当前对象转换到世界坐标系下
    /// </summary>
    public interface IRelativeCoord
    {
        ICoord3 Coord { get; set; }

        /// <summary>
        /// 转换到世界坐标系
        /// </summary>
        /// <returns></returns>
        object RelToWorld(ICoord3 coord = null);

        /// <summary>
        /// 从世界坐标系转换到相对坐标系中
        /// </summary>
        /// <returns></returns>
        object WorldToRel(ICoord3 coord = null);
    }
}