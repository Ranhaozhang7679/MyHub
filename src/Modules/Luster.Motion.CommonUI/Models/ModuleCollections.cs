#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleCollections
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       ModuleCollections.cs
* 创建时间:       2022/12/1 15:03:18
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      c73ed9fc-76f0-4b4f-994c-fcb490539c0b
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/1 15:03:18
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
    public class ModuleCollections: BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        /// <summary>
        ///工站list
        /// </summary>
        private List<ModuleModel> _motionChildList;
        public List<ModuleModel> MotionChildList
        {
            get { return _motionChildList; }
            set { SetProperty(ref _motionChildList, value); }
        }
    }
}