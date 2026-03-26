#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       XmlExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Extensions
* 文 件 名:       XmlExtension
* 创建时间:       2021/11/2 9:14:08
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      a636c96f-9172-46ce-b613-9ab1d28756fe
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/2 9:14:08
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.Extensions
{
    public static class XmlExtension
    {
        /// <summary>
        /// 通过Attribute设置值
        /// </summary>
        /// <param name="xParameter">XML</param>
        /// <param name="attrName">特征名称</param>
        /// <param name="action">动作</param>
        public static void GetAttribute(this XElement xParameter, string attrName, Action<string> action)
        {
            // 别名
            XAttribute xAlias = xParameter.Attribute(attrName);
            if (xAlias != null)
            {
                action(xAlias.Value);
            }
        }

        /// <summary>
        /// 通过Element获取节点
        /// </summary>
        /// <param name="xParameter">模块</param>
        /// <param name="element">节点名称</param>
        /// <param name="action">动作</param>
        public static void GetElement(this XElement xParameter, string element, Action<string> action)
        {
            XElement xElement = xParameter.Element(element);
            if (xElement != null)
            {
                action(xElement.Value);
            }
        }

        /// <summary>
        /// 通过Xml转换为对象
        /// </summary>
        /// <typeparam name="T">反向对象</typeparam>
        /// <param name="xRoot">xml节点</param>
        /// <returns>T实例对象</returns>
        public static T ToObject<T>(this XElement xRoot) where T : class
        {
            Type type = typeof(T);

            return xRoot.ToObject(type) as T;
        }

        /// <summary>
        /// 将XmlElement转换为Object
        /// </summary>
        /// <param name="xRoot"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToObject(this XElement xRoot, Type type)
        {
            var obj = Activator.CreateInstance(type);

            obj.FromXml(xRoot);

            return obj;
        }
    }
}