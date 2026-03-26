#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IAdapter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Adapter
* 文 件 名:       IAdapter.cs
* 创建时间:       2022/4/6 17:39:13
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      32c6d8dc-b02f-4200-9da9-c796adbc2db3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 17:39:13
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Common.DataStruct.Extensions;

namespace Luster.Motion.DataStruct.Real
{
    public interface IAdapter : IXMLParser
    {
        /// <summary>
        /// 获取连接方式
        /// </summary>
        /// <returns></returns>
        string GetMethod();

        /// <summary>
        /// 连接方式
        /// </summary>
        /// <param name="method"></param>
        void SetMethod(string method);
    }
}