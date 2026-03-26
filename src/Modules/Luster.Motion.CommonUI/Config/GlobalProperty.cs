#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalProperty
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Config
* 文 件 名:       GlobalProperty.cs
* 创建时间:       2021/11/23 9:46:20
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      ba844d4a-6dc4-437a-a7ce-f8ecb71c34c8
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/23 9:46:20
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI
{
    public class GlobalProperty
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        private static bool _isEnabeld = true; //默认配方界面可编辑
        public static bool IsEnabeld
        {
            get { return _isEnabeld; }
            set
            {
                _isEnabeld = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsEnabeld)));//异步更新静态属性
            }
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;//静态事件处理属性更改

        /// <summary>
        /// 用户角色
        /// </summary>
        public static string AutoLogin { get; set; } = "Operator";
    }
}