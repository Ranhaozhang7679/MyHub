#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmModule
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.AlarmUI
* 文 件 名:       AlarmModule.cs
* 创建时间:       2022/7/12 9:13:47
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      d919f88c-f0e5-4cc2-ac4b-54f8833e4240
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 9:13:47
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.AlarmUI.ViewModel;
using Luster.Motion.AlarmUI.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.AlarmUI
{
    public class AlarmModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
           
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AlarmContent, AlarmContentVM>();
            containerRegistry.RegisterForNavigation<AlarmTimeStatisticContent, AlarmTimeStatisticContentVM>();
        }
    }
}
