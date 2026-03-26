#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCircle
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VCircle.cs
* 创建时间:       2021/12/17 11:43:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7642fe08-0f22-4846-9cfa-10bce1947a7b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:43:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 几何体的父类
    /// </summary>
    public class BaseGeometry : LDisposable, IActor, IXMLParser, IRefCloud, IGeometry
    {
        /// <summary>
        /// 点云引用
        /// </summary>
        public VCloud RefCloud { get; set; }

        /// <summary>
        /// 支持对象渲染
        /// </summary>
        /// <param name="interactor"></param>
        public virtual void AddActor(IInteractor interactor)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseGeometry() : base(IntPtr.Zero)
        {
        }

        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="ptr"></param>
        public BaseGeometry(IntPtr ptr) : base(ptr)
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (RefCloud != null)
            {
                RefCloud.Dispose();
            }
        }
    }
}
