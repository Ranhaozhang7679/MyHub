#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Composition
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       Composition.cs
* 创建时间:       2022/10/18 8:51:48
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c5cf1a8c-ba47-4723-9320-d329f424a811
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/18 8:51:48
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 用于管理所有组合对象
    /// </summary>
    public class Composition : IXMLParser
    {
        /// <summary>
        /// 组合列表
        /// </summary>
        public List<IMotionModule> Modules { get; set; }

        /// <summary>
        /// 运动引擎
        /// </summary>
        private IMotionEngine _mEngine;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionEngine"></param>
        public Composition(IMotionEngine motionEngine)
        {
            _mEngine = motionEngine;
            Modules = new List<IMotionModule>();
        }

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement(nameof(Composition));

            foreach (var item in Modules)
            {
                xRoot.Add(item.ExportXml());
            }

            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            Modules?.Clear();
            foreach (var item in xElement.Elements())
            {
                var gModule = _mEngine.Parse(item, null);

                // 因为是组合模块，所以要从Root中删除该对象
                gModule.Parent.Children.Remove(gModule);
                gModule.Parent = null;

                Modules.Add(gModule);
            }
        }
        #endregion
    }
}