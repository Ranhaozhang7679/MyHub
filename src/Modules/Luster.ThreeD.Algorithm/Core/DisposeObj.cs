#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DisposeObj
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm
* 文 件 名:       DisposeObj.cs
* 创建时间:       2021/11/15 16:59:27
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      702ac191-b628-4a92-960b-fcbcf4f1cb7f
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/15 16:59:27
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

namespace Luster.ThreeD.Algorithm
{
    public class LDisposable : IDisposable, IGetProperty, IEmptyObj, ICloneable
    {
        /// <summary>
        /// 通过C++ 来构建
        /// </summary>
        protected bool BuildByCpp { get; set; } = false;

        /// <summary>
        /// 指针对象
        /// </summary>
        protected IntPtr lptr;

        public bool IsDisposed
        {
            get;
            protected set;
        } = false;

        public IntPtr LPtr
        {
            get
            {
                ThrowIfDisposed();
                return lptr;
            }
        }

        /// <summary>
        /// 是否引用其他对象
        /// </summary>
        public bool IsReference
        {
            get;
            set;
        }

        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 对应的Label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LDisposable()
        {
            lptr = IntPtr.Zero;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ptr"></param>
        public LDisposable(IntPtr ptr)
        {
            lptr = ptr;
            BuildByCpp = true;
        }

        public void Dispose()
        {
            if (lptr == IntPtr.Zero)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && lptr != IntPtr.Zero && !IsReference && disposing)
            {
                DisposeObj();
                lptr = IntPtr.Zero;
            }

            IsDisposed = true;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        protected virtual void DisposeObj()
        {
        }

        ~LDisposable()
        {
            Dispose(false);
        }

        public void ThrowIfDisposed()
        {
            if (lptr == IntPtr.Zero)
            {
                return;
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        #region IGetProperty接口

        /// <summary>
        /// 获取点
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <returns>属性值</returns>
        public virtual object GetProperty(string propName)
        {
            var prop = GetType().GetProperty(propName);
            if (prop != null)
            {
                return prop.GetValue(this, null);
            }

            return null;
        }

        #endregion

        #region IEmpty

        /// <summary>
        /// 是否空对象
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEmpty()
        {
            return this == null || this.lptr == IntPtr.Zero || IsDisposed;
        }

        #endregion

        #region IClone 接口

        public virtual object Clone()
        {
            return null;
        }

        #endregion

        #region IXmlParser

        /// <summary>
        /// Xml导出
        /// </summary>
        /// <returns></returns>
        public virtual XElement ExportXml()
        {
            return new XElement("Core");
        }

        /// <summary>
        /// Xml解析
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ParserXml(XElement xElement)
        {
        }

        #endregion

        #region ISave 接口

        /// <summary>
        /// 保存格式
        /// </summary>
        public virtual string[] SaveFormat { get; }

        /// <summary>
        /// 保存方法
        /// </summary>
        /// <param name="path"></param>
        public virtual void Save(string path)
        { }

        #endregion

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}