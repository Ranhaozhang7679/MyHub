#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LArray
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LArray.cs
* 创建时间:       2022/2/11 10:53:48
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      f97ab154-85c4-4b54-914c-a6e49ae55418
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/11 10:53:48
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 数组对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LArray<T> : List<T>, IXMLParser, ISave
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public LArray()
        {
        }

        [OutProperty("第一个")]
        public T First
        {
            get
            {
                if (Count == 0) return default(T);
                return this[0];
            }
        }

        [OutProperty("最后一个")]
        public T Last
        {
            get
            {
                if (Count == 0) return default(T);
                return this[Count - 1];
            }
        }

        [OutProperty("数量")]
        public new int Count { get => base.Count; }

        /// <summary>
        /// 保存成txt
        /// </summary>
        public string[] SaveFormat => new string[] { ".txt" };

        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="datas">数据</param>
        public LArray(params T[] datas)
        {
            foreach (var item in datas)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual XElement ExportXml()
        {
            var xRoot = new XElement("Array");
            if (this.Count > 0)
            {
                // 去掉L
                string realType = this[0].GetType().Name.Substring(1);

                // 记录真实类型
                xRoot.SetAttributeValue("RealType", realType);

                foreach (var item in this)
                {
                    if (item is IXMLParser parser)
                    {
                        xRoot.Add(parser.ExportXml());
                    }
                    else
                    {
                        xRoot.Add(new XElement("item", item.ToString()));
                    }
                }
            }

            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void ParserXml(XElement xElement)
        {
            string realType = "";
            xElement.GetAttribute("RealType", (rType) => realType = $"{rType}");

            // 清除历史的Item
            this.Clear();
            Type type = typeof(T);
            foreach (var xItem in xElement.Elements())
            {
                if (typeof(IXMLParser).IsAssignableFrom(type))
                {
                    var obj = Activator.CreateInstance(type) as IXMLParser;
                    obj.ParserXml(xItem);
                    this.Add((T)obj);
                }
                else
                {
                    string xVal = xItem.Value;
                    T obj = (T)Convert.ChangeType(xVal, type);
                    this.Add(obj);
                }
            }
        }

        public override string ToString()
        {
            return String.Join(",", this);
        }

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            using (StreamWriter streamWriter = new StreamWriter(fs))
            {
                foreach (var item in this)
                {
                    streamWriter.WriteLine(item.ToString());
                }
            }
        }
    }
}