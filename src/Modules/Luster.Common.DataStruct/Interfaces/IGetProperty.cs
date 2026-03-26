#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IGetPoint
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IGetPoint.cs
* 创建时间:       2022/1/11 9:23:11
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      eeca2af5-a2c7-4110-8a85-0981a66a5f8b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/11 9:23:11
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
    /// 获取属性的接口
    /// 实现该接口，可以获取类型的输出属性
    /// </summary>
    public interface IGetProperty
    {
        /// <summary>
        /// 获取点
        /// </summary>
        /// <param name="pointType"></param>
        /// <returns></returns>
        object GetProperty(string propName);
    }
}