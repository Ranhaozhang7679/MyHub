#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AdapterBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice._5.Adapter
* 文 件 名:       AdapterBase.cs
* 创建时间:       2022/4/18 15:45:13
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2389b2e2-789f-47c6-9197-225fcf38ea58
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 15:45:13
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.SimDevice.Adapter
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AdapterBase : IAdapter
    {

        /// <summary>
        /// 获取连接方式
        /// </summary>
        /// <returns></returns>
        public abstract string GetMethod();

        /// <summary>
        /// 设置连接方式
        /// </summary>
        /// <param name="method"></param>
        public abstract void SetMethod(string method);

        /// <summary>
        /// Xml对象解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }


        public XElement ExportXml()
        {
            return this.ToXml();
        }
    }
}