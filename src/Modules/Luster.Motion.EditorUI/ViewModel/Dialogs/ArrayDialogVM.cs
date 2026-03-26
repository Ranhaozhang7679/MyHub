#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ArrayDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel.Dialogs
* 文 件 名:       ArrayDialogVM.cs
* 创建时间:       2022/8/10 10:51:18
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ea31c549-8ea7-4e2e-9780-7645536363c8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/10 10:51:18
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.EditorUI.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{

    public class ArrayDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 数组
        /// </summary>
        private ObservableCollection<ValueModel> _array;
        public ObservableCollection<ValueModel> Array
        {
            get => _array;
            set => SetProperty(ref _array, value);
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        public DataType DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }

        /// <summary>
        /// 参数特征
        /// </summary>
        private ParameterAttribute pAttr;

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType dType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="FriendlyException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<ParameterAttribute>("Parameter", out pAttr))
            {
                Array = new ObservableCollection<ValueModel>();

                var realType = pAttr.Type.GetGenericArguments()[0];
                DataType = DataType.Bool;
                if (realType == typeof(string))
                {
                    DataType = DataType.String;
                }
                else if (realType == typeof(int))
                {
                    DataType = DataType.Int;
                }

                switch (DataType)
                {
                    case DataType.Int:
                        var intArr = pAttr.Value as LArray<int>;
                        if (intArr != null)
                        {
                            foreach (var item in intArr)
                            {
                                Array.Add(new ValueModel(item));
                            }
                        }

                        break;
                    case DataType.String:
                        var strArr = pAttr.Value as LArray<string>;
                        if (strArr != null)
                        {
                            foreach (var item in strArr)
                            {
                                Array.Add(new ValueModel(item));
                            }
                        }
                        break;
                    case DataType.Bool:
                        var boolArr = pAttr.Value as LArray<bool>;
                        if (boolArr != null)
                        {
                            foreach (var item in boolArr)
                            {
                                Array.Add(new ValueModel(item));
                            }
                        }
                        break;

                    default:
                        throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("TypeNotSupported")}:{dType}");
                }
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            switch (DataType)
            {
                case DataType.Bool:
                    LArray<bool> boolArray = new LArray<bool>();
                    foreach (var item in Array)
                    {
                        boolArray.Add(bool.Parse(item.Value.ToString()));
                    }
                    pAttr.Value = boolArray;

                    break;
                case DataType.Int:

                    LArray<int> intArray = new LArray<int>();
                    foreach (var item in Array)
                    {
                        intArray.Add(int.Parse(item.Value.ToString()));
                    }
                    pAttr.Value = intArray;
                    break;
                case DataType.String:

                    LArray<string> stringArr = new LArray<string>();

                    foreach (var item in Array)
                    {
                        stringArr.Add(item.Value.ToString());
                    }
                    pAttr.Value = stringArr;
                    break;
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(() =>
        {
            switch (DataType)
            {
                case DataType.Bool:
                    Array.Add(new ValueModel(false));
                    break;
                case DataType.Int:
                    Array.Add(new ValueModel(0));
                    break;
                case DataType.String:
                    Array.Add(new ValueModel("None"));
                    break;
            }
        }));
    }
}