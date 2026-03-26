#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RefGroup
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       RefGroup.cs
* 创建时间:       2022/10/18 8:35:11
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      594cb04d-0542-4245-aad9-41a98dae9f1e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/18 8:35:11
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 引用模块
    /// </summary>
    public class RefGroup : MotionFunction, IRefComposition
    {
        [Parameter("引用模块", 0, CN = "引用模块")]
        public string RefModule { get; set; }

        /// <summary>
        /// 当前的模块对象
        /// </summary>
        private IMotionModule Motion { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RefGroup()
        {
            // 不显示到页面中
            this.IsVisible = false;
            this.Icon = "\xe6b1";
        }

        /// <summary>
        /// 模块执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            if (Motion != null)
            {
                // 因为涉及到引用的问题，所以需要更新此时子节点对应的父节点
                foreach (var item in Motion.Children)
                {
                    item.Parent = MyOwner;
                    item.UpdateStation();
                }

                bool isSuccess = Motion.DoFunction();

                // 如果错误
                if (!isSuccess)
                {
                    errMsg = Motion.StatusMsg;
                }
            }

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();
            if (Motion != null)
            {
                xml.SetAttributeValue("RefID", Motion.ID);
            }

            return xml;
        }

        /// <summary>
        /// 解析引用对象
        /// </summary>
        /// <param name="xFunc"></param>
        public override void ParserXml(XElement xFunc)
        {
            base.ParserXml(xFunc);

            // 添加引用模组
            xFunc.GetAttribute("RefID", rID =>
            {
                if (!string.IsNullOrEmpty(rID))
                {
                    var mID = Guid.Parse(rID);
                    if (MyOwner.TaskModules.Contains(mID))
                    {
                        Motion = MyOwner.TaskModules[mID] as IMotionModule;
                    }
                }
            });
        }

        /// <summary>
        /// 配置引用模块
        /// </summary>
        /// <param name="motionModule"></param>
        public void SetRefModule(IMotionModule motionModule)
        {
            Motion = motionModule;
            MyOwner.SetInParameter(nameof(RefModule), motionModule.Alias);
        }

        /// <summary>
        /// 获取引用的模块
        /// </summary>
        /// <returns></returns>
        public IMotionModule GetRefModule()
        {
            return Motion;
        }
    }
}