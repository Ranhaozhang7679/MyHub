#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ISave
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       ISave.cs
* 创建时间:       2021/11/23 11:14:36
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      b7cd7bcf-e59f-4d63-a4a7-935c771b3da7
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/23 11:14:36
* 修 改 人:		  luster
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
    /// 保存接口
    /// </summary>
    public interface ISave
    {
        /// <summary>
        /// 保存格式
        /// </summary>
        string[] SaveFormat { get; }

        /// <summary>
        /// 保存方法
        /// </summary>
        /// <param name="path"></param>
        void Save(string path);
    }
}