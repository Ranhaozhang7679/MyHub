#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionPageVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel
* 文 件 名:       MotionPageVM.cs
* 创建时间:       2022/9/5 13:55:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7c2e0ad0-2e01-4279-9f50-ddf6f1b2d1e5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/5 13:55:30
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel
{
    public class MotionPageVM : MotionVM, INavigationAware
    {
        public MotionPageVM(ICommonBus _commonBus) : base(_commonBus)
        {
        }

        public MotionPageVM() 
        {
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 离开当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// 进入当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
        }
    }
}