#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NGGroup
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       NGGroup.cs
* 创建时间:       2022/8/19 8:54:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c4df1fad-572e-4e49-9163-d6e6e03cf791
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/19 8:54:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{

    public interface IActionNG
    {
        /// <summary>
        /// 设置NG处理
        /// </summary>
        /// <param name="ngModule"></param>
        /// <param name="isNg"></param>
        void SetNg(IMotionModule ngModule, bool isNg);
    }

    /// <summary>
    /// 动作NG的后处理
    /// </summary>
    public class NGGroup : Group, IActionNG
    {
        public NGGroup()
        {
            this.Icon = "\xe6a3";
            this.Tips = "NG处理动作";
        }

        public void SetNg(IMotionModule ngModule, bool isNg)
        {
            //ngModule.IsNgModule = isNg;
            foreach (var item in ngModule.Children)
            {
                SetNg(item, isNg);
            }
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            // 如果模块为NG才执行函数
            //if (!MyOwner.IsNgModule) return true;


            return base.DoExcute(out errMsg);
        }
    }
}