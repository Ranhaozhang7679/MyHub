#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AddInDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.ViewModel
* 文 件 名:       AddInDialogVM.cs
* 创建时间:       2022/6/17 21:38:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      541f33d3-af83-487c-ae0c-d9fbf68a00d0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 21:38:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.Common.DataStruct;
using Luster.TaskFlow.Common.Module;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Prism.Commands;
using System.Windows.Controls;
using System.IO.Packaging;

namespace Luster.Common.Assets.ViewModel
{
    public class AddInParamDialogVM : BaseDialogVM
    {
        public ParameterAttribute pAttr;

        private List<KeyValue> _typeList;
        public List<KeyValue> TypeList
        {
            get => _typeList; set => SetProperty(ref _typeList, value);
        }

        /// <summary>
        /// 参数类型
        /// </summary>
        private Type _type;
        [Required(ErrorMessage = "参数类型不能为空")]
        public Type Type
        {
            get => _type; set
            {
                SetProperty(ref _type, value);
                if (IsNumber(value))
                {
                    IsNumberic = true;
                }
                else
                {
                    IsNumberic = false;
                }

                if (pAttr != null)
                {
                    pAttr.Type = value;
                }

                SetDefault(value);
            }
        }

        private void SetDefault(Type type)
        {
            if (type == typeof(bool))
            {
                if (bool.TryParse(DefaultV, out bool boolic))
                {
                    DefaultV = boolic.ToString();
                }
                else
                {
                    DefaultV = "False";
                }
            }
            else if (type.IsNumeric())
            {
                if (double.TryParse(DefaultV, out var d))
                {
                    DefaultV = $"{d}";
                }
                else
                {
                    DefaultV = "0";
                }
            }
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        private string _name;
        [Required]
        public string Name
        {
            get => _name; set
            {
                var srcKey = pAttr?.Name;
                SetProperty(ref _name, value);

                if (pAttr != null)
                {
                    // 名称修改，那么需要更新ID
                    if (pAttr.CN != value)
                    {
                        // 如果名称进行修改，需要删除原来的Key主键
                        if (pAttr.Owner.Parameters.ContainsKey(srcKey))
                        {
                            pAttr.Owner.Parameters.Remove(srcKey);
                        }

                        pAttr.Name = $"Extend_{Name}";
                        if (!pAttr.Owner.Parameters.ContainsKey(pAttr.Name))
                        {
                            pAttr.Owner.Parameters.Add(pAttr.Name, pAttr);
                        }
                    }

                    pAttr.CN = value;
                }
            }
        }

        /// <summary>
        /// 参数描述
        /// </summary>
        private string _desc;
        public string Desc
        {
            get => _desc; set
            {
                SetProperty(ref _desc, value);

                if (pAttr != null)
                {
                    pAttr.Tips = value;
                }
            }
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        private double _min = 0;
        public double Min { get => _min; set => SetProperty(ref _min, value); }

        /// <summary>
        /// 参数名称
        /// </summary>
        private double _max = 1000;
        public double Max { get => _max; set => SetProperty(ref _max, value); }

        /// <summary>
        /// 如果是数字类型
        /// </summary>
        private bool _isNumberic = false;
        public bool IsNumberic { get => _isNumberic; set => SetProperty(ref _isNumberic, value); }

        /// <summary>
        /// 参数是否支持引用
        /// </summary>
        private bool _canRef = false;
        public bool CanRef
        {
            get => _canRef; set
            {
                SetProperty(ref _canRef, value);
                if (pAttr != null)
                {
                    if (value)
                    {
                        pAttr.CanRef = ParamRef.Ref;
                    }
                    else
                    {
                        pAttr.CanRef = ParamRef.None;
                    }
                }
            }
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        private string _defaultV = "";
        public string DefaultV
        {
            get => _defaultV; set
            {
                SetProperty(ref _defaultV, value);
            }
        }

        /// <summary>
        /// 是否支持记忆
        /// </summary>
        private bool _isMemoric;

        public bool IsMemoric
        {
            get => _isMemoric;
            set { SetProperty(ref _isMemoric, value);}
        }

        /// <summary>
        /// 是否回零恢复默认值
        /// </summary>
        private bool _isHomeDefault;

        public bool IsHomeDefault
        {
            get => _isHomeDefault;
            set { SetProperty(ref _isHomeDefault, value); }
        }

        /// <summary>
        /// 参数隶属模块
        /// </summary>
        private IModule _module = null;

        /// <summary>
        /// 点击确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            if (_module.Parameters.ContainsKey(Name))
            {
                throw new FriendlyException($"模块:{_module.Alias} 已经存在参数:{Name}");
            }

            if (pAttr == null)
            {
                // 检查参数名称是否重复
                AddParameter();
            }
            else 
            {
                SetParamValue(pAttr, Name);
            }
        }


        private ParameterAttribute AddParameter()
        {
            var inCount = _module.Parameters.Values.Count(u => u.ParamType == TaskFlow.Common.Enums.ParamType.IN);
            string key = $"Extend_{Name}";
            string cn = Name;
            string alias = Name;

            var newParam = ParameterAttribute.CreateByType(key, Type, _module, cn, inCount);
            newParam.Tips = Desc;
            SetParamValue(newParam, alias);

            if (!_module.Parameters.ContainsKey(key))
            {
                _module.Parameters.Add(key, newParam);
            }

            return newParam;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="result"></param>
        protected override void Add(IDialogResult result)
        {
            var item = AddParameter();

            ParamList.Add(new KeyValue() { Key = item.Name, Value = item.Value, Desc = item.CN,IsMemoric=item.IsMemoric,IsHomeDefault=item.IsHomeDefault });
        }

        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="paramAttr"></param>
        /// <param name="alias"></param>
        private void SetParamValue(ParameterAttribute paramAttr, string alias)
        {
            paramAttr.Alias = alias;
            // 扩展参数必填
            paramAttr.Validations.Add(new NotEmptyAttribute());

            // 参数支持引用
            if (CanRef)
            {
                paramAttr.CanRef = ParamRef.Ref;
            }
            else
            {
                paramAttr.CanRef = ParamRef.None;
            }
            paramAttr.IsMemoric =this.IsMemoric;
            paramAttr.IsHomeDefault = this.IsHomeDefault;

            bool ChangeValue = !_module.Parameters.ContainsKey($"Extend_{Name}");
            // 如果是值类型,添加参数校验，默认值校验
            SetDefaultV(paramAttr, DefaultV,ChangeValue);
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="newParam"></param>
        /// <param name="defaultV"></param>
        /// <param name="ChangeValue">默认变更当前值,如果存在该变量则不改变</param>
        private void SetDefaultV(ParameterAttribute newParam, string defaultV,bool ChangeValue=true)
        {
            if (IsNumberic)
            {
                newParam.Validations.Add(new LimitAttribute(Min, Max));
                if (!string.IsNullOrEmpty(defaultV) && double.TryParse(defaultV, out var d))
                {
                    if (ChangeValue)
                    {
                        newParam.Value = Convert.ChangeType(d, newParam.Type);
                    }
                        newParam.DefaultV = d;
                }
                else
                {
                    if (ChangeValue)
                    {
                        newParam.Value = 0;
                    }
                    newParam.DefaultV = Convert.ChangeType(0, newParam.Type);
                }
            }
            else if (Type == typeof(bool))
            {
                if (bool.TryParse(defaultV, out var v))
                {
                    if (ChangeValue)
                    {
                        newParam.Value = v;
                    }
                    newParam.DefaultV = v;
                }
                else
                {
                    if (ChangeValue)
                    {
                        newParam.Value = false;
                    }
                    newParam.DefaultV = false;
                }
            }
            else if (Type == typeof(string))
            {
                if (ChangeValue)
                {
                    newParam.Value = defaultV;
                }
                newParam.DefaultV = defaultV;
            }
        }
        /// <summary>
        /// 是否数字检查
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsNumber(Type type)
        {
            return type == typeof(double) || type == typeof(float) || type == typeof(int);
        }

        /// <summary>
        /// Dialog 打开
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<IModule>("Module", out var module))
            {
                var list = new List<KeyValue>();
                foreach (var item in ParameterAttribute.GetAllTypes())
                {
                    list.Add(new KeyValue() { Desc = item.Key, Value = item.Value });
                }

                TypeList = list;

                _module = module;

                // 初始化
                ParamList = new ObservableCollection<KeyValue>();
                foreach (var item in module.Parameters)
                {
                    if (item.Value.ParamType == TaskFlow.Common.Enums.ParamType.IN && item.Value.Property == null)
                    {
                        ParamList.Add(new KeyValue() { Key = item.Key, Value = item.Value, Desc = item.Value.CN,
                            IsMemoric=item.Value.IsMemoric,IsHomeDefault=item.Value.IsHomeDefault});
                    }
                }
            }
            else
            {
                throw new KeyNotFoundException("Module");
            }

            if (parameters.TryGetValue<ParameterAttribute>("Parameter", out var pVal))
            {
                SetEditParam(pVal);
            }
        }


        private void SetEditParam(ParameterAttribute pVal)
        {
            pAttr = pVal;
            if (pAttr != null)
            {
                Type = pAttr.Type;
                Name = pAttr.CN;
                DefaultV = pAttr.DefaultV?.ToString();
                Desc = pAttr.Tips;
                IsMemoric = pAttr.IsMemoric;
                IsHomeDefault = pAttr.IsHomeDefault;
                // 参数支持引用
                CanRef = pAttr.CanRef == ParamRef.Ref;
            }
        }

        private ObservableCollection<KeyValue> paramList;
        public ObservableCollection<KeyValue> ParamList
        {
            get => paramList;
            set => SetProperty(ref paramList, value);
        }

        /// <summary>
        /// 点击标签
        /// </summary>
        private DelegateCommand<object> _removeCondition;
        public DelegateCommand<object> RemoveCommand => _removeCondition ?? (_removeCondition = new DelegateCommand<object>((tag) =>
        {
            KeyValue kVal = tag as KeyValue;
            if (kVal != null)
            {
                ParamList.Remove(kVal);

                _module.Parameters.Remove(kVal.Key);
                _module.OnUpdate(TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
            }
        }));

        /// <summary>
        /// 选择
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((tag) =>
        {
            SelectionChangedEventArgs args = tag as SelectionChangedEventArgs;
            if (args != null && args.AddedItems.Count > 0)
            {
                var kVal = args.AddedItems[0] as KeyValue;
                if (kVal != null)
                {
                    SetEditParam(kVal.Value as ParameterAttribute);
                }
            }
        }));
    }
}