#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel
* 文 件 名:       BaseDialogVM.cs
* 创建时间:       2022/6/16 17:16:56
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      cd2971f0-bfe9-46df-a441-819fcf2a40df
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/16 17:16:56
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets.ViewModel;
using Luster.Motion.Assests.Langs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Reflection;

namespace Luster.Motion.CommonUI.ViewModel
{
    /// <summary>
    /// 通用对话框
    /// </summary>
    public class MotionDialogVM : BaseDialogVM
    {
        /// <summary>
        /// 多语言支持
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override string L(string key)
        {
            string lang = LangProvider.GetLang(key);
            if (string.IsNullOrEmpty(lang))
            {
                return key;
            }

            return lang;
        }

        /// <summary>
        /// 信息描述
        /// </summary>
        private string _desc;
        public string Desc
        {
            get { return _desc; }
            set
            {
                SetProperty(ref _desc, value);
            }
        }

        /// <summary>
        /// 多语言属性
        /// </summary>
        private List<PropertyInfo> LangProperties { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_commonBus">参数注册</param>
        public MotionDialogVM()
        {
            LangProperties = new List<PropertyInfo>();
            Desc = "Desc";
            HandyControl.Tools.ConfigHelper.Instance.PropertyChanged -= Instance_PropertyChanged;
            HandyControl.Tools.ConfigHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HandyControl.Tools.ConfigHelper cHelper = (HandyControl.Tools.ConfigHelper)sender;
            string lang = cHelper.Lang.IetfLanguageTag;

            if (LangProperties.Count > 0)
            {
                if (lang == "en")
                {
                    Desc = "Key";
                }
                else
                {
                    Desc = nameof(Desc);
                }

                // 更新属性
                foreach (var item in LangProperties)
                {
                    var srcV = item.GetValue(this, null);
                    item.SetValue(this, srcV);
                }
            }
        }


    }
}