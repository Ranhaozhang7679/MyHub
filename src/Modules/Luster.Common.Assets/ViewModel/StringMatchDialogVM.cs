#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StringMatchDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.ViewModel
* 文 件 名:       StringMatchDialogVM.cs
* 创建时间:       2022/10/19 10:28:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      398e24d4-e894-42f1-8199-5af2f88a172f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/19 10:28:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Luster.Common.Assets.Views;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Luster.Common.Assets.ViewModel
{
    public class StringMatchDialogVM : BaseDialogVM
    {

        private IDialogService dialog;
        public StringMatchDialogVM(IDialogService dialog)
        {
            DocumentText = new TextDocument() { Text = "" };
            this.dialog = dialog;
            DataTypes = typeof(DataType).EnumToDataSource();
        }

        private List<KeyValue> _dataTypes;
        public List<KeyValue> DataTypes
        {
            get { return _dataTypes; }
            set { _dataTypes = value; }
        }

        /// <summary>
        /// 标签列表
        /// </summary>
        private string _testText;
        public string TestText
        {
            get => _testText;
            set { SetProperty(ref _testText, value); }
        }

        /// <summary>
        /// 标签列表
        /// </summary>
        private ObservableCollection<StringVarModel> _variables;
        public ObservableCollection<StringVarModel> Variables
        {
            get => _variables;
            set { SetProperty(ref _variables, value); }
        }


        /// <summary>
        /// 点击标签
        /// </summary>
        private DelegateCommand<object> _selectVarCommand;
        public DelegateCommand<object> SelectVarCommand => _selectVarCommand ?? (_selectVarCommand = new DelegateCommand<object>((obj) =>
        {
            MouseButtonEventArgs args = obj as MouseButtonEventArgs;
            if (args != null)
            {
                var curItem = (args.Source as DataGrid).SelectedItem as StringVarModel;

                if (textEditor == null)
                {
                    DocumentText.Text = $"{DocumentText.Text}{curItem.Name}";
                }
                else
                {
                    DocumentText.Insert(textEditor.SelectionStart, curItem.Name);
                }
            }
        }));

        /// <summary>
        /// 预览命令
        /// </summary>
        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(() =>
        {
            var newStringModel = new StringVarModel();
            newStringModel.PropertyChanged += StrVarModel_PropertyChanged;
            Variables.Add(newStringModel);
        }));

        /// <summary>
        /// 预览命令
        /// </summary>
        private DelegateCommand<StringVarModel> _removeCommand;
        public DelegateCommand<StringVarModel> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<StringVarModel>((strVar) =>
        {
            Variables.Remove(strVar);
        }));

        /// <summary>
        /// 预览命令
        /// </summary>
        private DelegateCommand _previewCommand;
        public DelegateCommand PreviewCommand => _previewCommand ?? (_previewCommand = new DelegateCommand(() =>
        {
            if (string.IsNullOrEmpty(TestText))
            {
                throw new FriendlyException("测试文本为空!");
            }

            string text = DocumentText.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var match = BuildStringMatch();
                match.Match(TestText);
                foreach (var item in Variables)
                {
                    var v = match.Variables.FirstOrDefault(u => u.Name == item.Name);
                    if (v != null)
                    {
                        item.Value = v.Value;
                    }
                }
            }
        }));

        private LStringMatch BuildStringMatch()
        {
            List<StringVar> strVals = new List<StringVar>();
            foreach (var item in Variables)
            {
                strVals.Add(item.GetStringVar());
            }

            var strMatch = new LStringMatch()
            {
                MatchString = DocumentText.Text,
                Variables = strVals
            };

            return strMatch;
        }

        // 控件对象
        private TextEditor textEditor;

        //
        private StringMatchDialog strMatchContent;

        /// <summary>
        /// 预览命令
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is RoutedEventArgs args && args.OriginalSource is StringMatchDialog content)
            {
                textEditor = content.GetEditor();
                strMatchContent = content;
            }
        }));


        /// <summary>
        /// 文档流
        /// </summary>
        private TextDocument _document;
        public TextDocument DocumentText
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        public ParameterAttribute Parameter { get; set; }

        /// <summary>
        /// 点击确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            if (Variables.Count == 0)
            {
                throw new FriendlyException("字符变量不能为空!");
            }

            if (string.IsNullOrEmpty(DocumentText.Text))
            {
                throw new FriendlyException("字符模板不能为空!");
            }

            result.Parameters.Add("StringMatch", BuildStringMatch());
        }


        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            // 对象初始化
            DocumentText = new TextDocument();

            if (parameters.TryGetValue<ParameterAttribute>("Parameter", out var parameter))
            {
                Parameter = parameter;
                var lStringMatch = Parameter.Value as LStringMatch;
                if (lStringMatch != null)
                {
                    DocumentText.Text = lStringMatch.MatchString;
                }
                else
                {
                    // 对象为空就创建一个
                    lStringMatch = new LStringMatch();
                }

                // 构建变量
                Variables = new ObservableCollection<StringVarModel>();
                foreach (var item in lStringMatch.Variables)
                {
                    var strVarModel = new StringVarModel(item);
                    strVarModel.PropertyChanged -= StrVarModel_PropertyChanged;
                    strVarModel.PropertyChanged += StrVarModel_PropertyChanged;
                    Variables.Add(strVarModel);
                }
            }
            else
            {
                throw new KeyNotFoundException("Parameter");
            }
        }

        /// <summary>
        /// 名称发生变更，要及时更新Keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StrVarModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                var names = Variables.Select(u => u.Name).ToArray();
                strMatchContent.AddVariables(names);
            }
        }
    }

    public class StringVarModel : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetProperty(ref _name, value);
                if (Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        public DataType DataType
        {
            get { return _dataType; }
            set
            {
                SetProperty(ref _dataType, value);
                if (Tag != null)
                {
                    Tag.DataType = value;
                }
            }
        }

        /// <summary>
        /// 对应的值
        /// </summary>
        private object _value;
        public object Value
        {
            get { return _value; }
            set
            {
                SetProperty(ref _value, value);
                if (Tag != null)
                {
                    Tag.Value = value;
                }
            }
        }

        /// <summary>
        /// 字符长度
        /// </summary>
        private int _length;
        public int Length
        {
            get { return _length; }
            set
            {
                SetProperty(ref _length, value);
                if (Tag != null)
                {
                    Tag.Length = value;
                }
            }
        }

        public StringVar Tag { get; set; }

        public StringVarModel(StringVar strVal)
        {
            Tag = strVal;
            Name = strVal.Name;
            DataType = strVal.DataType;
            Value = strVal.Value;
            Length = strVal.Length;
        }

        public StringVarModel()
        {
            Name = "";
            DataType = DataType.String;
            Value = "";
            Length = -1;
        }

        public StringVar GetStringVar()
        {
            return new StringVar()
            {
                Name = Name,
                DataType = DataType,
                Length = Length,
                Value = Value
            };
        }
    }
}