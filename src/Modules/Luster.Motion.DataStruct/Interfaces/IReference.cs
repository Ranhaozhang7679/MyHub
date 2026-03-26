#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IReference
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Interfaces
* 文 件 名:       IReference.cs
* 创建时间:       2022/6/22 15:27:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e9416505-f155-4430-958c-64986946a02e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/22 15:27:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 引用对象
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 对象引用了几个属性
        /// </summary>
        /// <returns></returns>
        string[] GetRefProps();

        /// <summary>
        /// 获取所有引用对象
        /// </summary>
        /// <returns></returns>
        List<IVirtualDevice> GetRefObjs();
    }
}