#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IOModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       IOModel.cs
* 创建时间:       2022/4/26 18:19:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f09e8061-b91b-46ec-9b69-422a123209fb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 18:19:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class IOModel : BindableBase
    {
        /// <summary>
        /// 是否选择
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; 
            set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
                if (src != value)
                    OnSelected(value);
            }
        }

        /// <summary>
        /// 控制卡名称
        /// </summary>
        private string _card;
        public string CardName { get => _card; set => SetProperty(ref _card, value); }

        /// <summary>
        /// 模组
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module; set
            {
                SetProperty(ref _module, value); if (Tag != null)
                {
                    Tag.Module = value;
                }
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value);
                if (Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        /// <summary>
        /// 子定义
        /// </summary>
        private string _subDefinite;
        public string SubDefinite
        {
            get => _subDefinite; set
            {
                SetProperty(ref _subDefinite, value);
                if (Tag != null)
                {
                    Tag.SubDefinite = value;
                }
            }
        }


        /// <summary>
        /// 起始编号
        /// </summary>
        private int _index = 1;
        public int Index { get => _index; set => SetProperty(ref _index, value); }

        /// <summary>
        /// 默认值
        /// </summary>
        private bool _defaultValue;
        public bool DefaultValue { get => _defaultValue; set => SetProperty(ref _defaultValue, value); }

        /// <summary>
        /// 默认值
        /// </summary>
        private bool _value;
        public bool DigitalValue
        {
            get => _value; 
            set
            {
                var src = _value;
                SetProperty(ref _value, value);
                if (Tag != null)
                {
                    if (value != src)  //值改变才写入
                    {
                        Tag.SetDigital(value);
                    }
                }
            }
        }

        /// <summary>
        /// 模拟数字
        /// </summary>
        private double _analogVal;
        public double AnalogValue
        {
            get => _analogVal; set
            {
                var src = _analogVal;
                SetProperty(ref _analogVal, value);
                if (src != value && Tag != null)
                {
                    Tag.SetAnglog(value);
                }
            }
        }

        /// <summary>
        /// 模拟数字
        /// </summary>
        private double _finalValue;
        public double FinalValue
        {
            get => _finalValue; set
            {
                var src = _finalValue;
                SetProperty(ref _finalValue, value);
            }
        }

        /// <summary>
        /// 模拟数字表达式转换器
        /// </summary>
        private string _express = "";
        public string Express
        {
            get => _express; set
            {
                var src = _express;
                SetProperty(ref _express, value);
                if (src != value && Tag != null)
                {
                    Tag.AnglogExp = value;

                    // 更新最终的值
                    FinalValue = Tag.GetAnalogExpVal();
                }
            }
        }

        /// <summary>
        /// 是否数字信息
        /// </summary>
        private bool _isDigital;
        public bool IsDigital
        {
            get { return _isDigital; }
            set { SetProperty(ref _isDigital, value); }
        }


        /// <summary>
        /// 是否数字信息
        /// </summary>
        private bool _isIn;
        public bool IsIn
        {
            get { return _isIn; }
            set { SetProperty(ref _isIn, value); }
        }

        /// 构造函数
        /// </summary>
        /// <param name="vIO"></param>
        public IOModel(VIO vIO)
        {
            IsSelected = false;
            CardName = vIO.CardName;
            Module = vIO.Module;
            Name = vIO.Name;
            Index = vIO.Index;
            if (string.IsNullOrEmpty(vIO.SubDefinite))
            {
                vIO.SubDefinite = vIO.Index.ToString();
            }

            SubDefinite = vIO.SubDefinite;
            DefaultValue = vIO.DefaultValue == 1;
            DigitalValue = vIO.Value == 1;
            AnalogValue = vIO.Value;
            Tag = vIO;
            IsDigital = vIO.IOType == IOType.Digital;
            IsIn = vIO.Behavior == IOBehavior.Input;
            Express = vIO.AnglogExp ?? "";


        }

        public VIO Tag { get; set; }

        /// <summary>
        /// 选择发生了变更
        /// </summary>
        public event Action<bool> SelectedChanged;

        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }
    }
}