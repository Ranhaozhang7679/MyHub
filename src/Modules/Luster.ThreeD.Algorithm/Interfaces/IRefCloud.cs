#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ICloud
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.Interfaces
* 文 件 名:       ICloud.cs
* 创建时间:       2022/1/5 11:35:08
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      9a9ddfa9-1084-4165-8cd6-47eefae1da55
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/5 11:35:08
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Interfaces
{
    /// <summary>
    /// 通过文件加载点云
    /// </summary>
    public interface IRefCloud
    {
        /// <summary>
        /// 如果拟合则需要配置对应的点云对象
        /// </summary>
        VCloud RefCloud { get; set; }
    }
}