#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IRefTemplate
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.Interfaces
* 文 件 名:       IRefTemplate.cs
* 创建时间:       2022/2/16 10:31:56
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      0e31df3c-6f40-4615-bcd5-e207ad74a6b0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/16 10:31:56
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Interfaces
{
    /// <summary>
    /// 参考模板
    /// </summary>
    public interface ICoordTemplate
    {
        /// <summary>
        /// 参考模板
        /// </summary>
        LCoord Template { get; set; }

        /// <summary>
        /// 设置模板坐标方法
        /// </summary>
        /// <param name="matrix"></param>
        void SetTemplate(ITransform transform);

        /// <summary>
        /// 获取当前旋转矩阵
        /// </summary>
        /// <returns></returns>
        ITransform GetTransform();
    }
}