#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalVar
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       GlobalVar.cs
* 创建时间:       2022/7/4 18:17:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ff0c8ca4-51d5-4bf5-a695-10a6cc1cba27
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 18:17:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Attributes;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Models
{
    public class GlobalVar : BindableBase
    {
        private string _key;
        public string Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }
        public bool IsSelected { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetProperty(ref _name, value);
                if (Parameter != null)
                {
                    Parameter.Alias = value;
                    Parameter.CN = value;
                }
            }
        }

        private Type _type;
        public Type Type
        {
            get { return _type; }
            set
            {
                SetProperty(ref _type, value);
            }
        }

        private string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            set { SetProperty(ref _typeName, value); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                SetProperty(ref _isChecked, value);
                if (Parameter != null)
                {
                    Parameter.Value = value;
                }
            }
        }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                SetProperty(ref _visible, value);
            }
        }

        private object _val;
        public object Value
        {
            get { return _val; }
            set
            {
                object srcV = _val;
                SetProperty(ref _val, value);
                if (srcV != _val)
                {
                    Parameter.Value = value;
                }
            }
        }

        private object _defaultV;
        public object DefaultV
        {
            get { return _defaultV; }
            set { SetProperty(ref _defaultV, value); }
        }

        private bool _isMemoric;

        public bool IsMemoric
        {
            get { return _isMemoric; }
            set { SetProperty(ref _isMemoric, value); }
        }

        private bool _isHomeDefault;

        public bool IsHomeDefault
        {
            get { return _isHomeDefault; }
            set { SetProperty(ref _isHomeDefault, value); }
        }

        public ParameterAttribute Parameter { get; set; }

        public GlobalVar(ParameterAttribute pAttr)
        {
            Parameter = pAttr;
            Key = pAttr.Name;
            Name = pAttr.CN;
            TypeName = ParameterAttribute.GetDataTypeByType(pAttr.Type);
            Value = pAttr.Value;
            DefaultV = pAttr.DefaultV;
            IsHomeDefault = pAttr.IsHomeDefault;
            IsMemoric = pAttr.IsMemoric;
            if (pAttr.Value is bool defaultvalue)
            {
                IsChecked = defaultvalue;
            }
            Type = pAttr.Type;
            IsSelected = false;
        }

        public GlobalVar()
        {
        }
    }
}