#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CPKModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       CPKModel.cs
* 创建时间:       2023/2/6 17:03:23
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      e94ae6bb-d7a0-4ba5-90e7-8252f75fad0c
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/6 17:03:23
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public class PressModel : BindableBase 
    {

		private string name;
		public string Name
		{
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private object driver;
        public object Driver
        {
            get { return driver; }
            set { SetProperty(ref driver, value); }
        }
    }
}