#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ValueModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       ValueModule.cs
* 创建时间:       2022/8/10 14:09:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e1439256-e423-4ed0-a7a7-a6c4b73cff1c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/10 14:09:00
* 修 改 人:		  L05123
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
    public class ValueModel : BindableBase
    {
        private object _value;
        public object Value { get => _value; set => SetProperty(ref _value, value); }

        public ValueModel(object value)
        {
            this.Value = value;
        }
    }
}