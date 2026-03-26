#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CompareModuleModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       CompareModuleModel.cs
* 创建时间:       2022/11/23 14:19:50
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      cebdccb3-4771-44d8-8acb-fa399618a55e
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/23 14:19:50
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Models
{
    public class CompareModuleModel: BindableBase
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
        private List<ModuleCT> _motionChildList;
        public List<ModuleCT> MotionChildList
        {
            get { return _motionChildList; }
            set { SetProperty(ref _motionChildList, value); }
        }
    }
}