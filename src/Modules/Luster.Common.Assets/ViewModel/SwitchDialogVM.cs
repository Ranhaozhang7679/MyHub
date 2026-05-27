#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ExpressDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.ViewModel
* 文 件 名:       ExpressDialogVM.cs
* 创建时间:       2022/6/16 21:07:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      838e534c-1367-41ee-8697-4aa5f6670e1f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/16 21:07:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Luster.Common.Assets.Tools;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Common.Assets.ViewModel
{

    public class ParamVar : BindableBase
    {
        private string _alias;

        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                Attr.Alias = value;
            }
        }

        public ParameterAttribute Attr { get; set; }

        private string _dataSrc;

        public string DataSrc
        {
            get { return _dataSrc; }
            set
            {
                SetProperty(ref _dataSrc, value);
            }
        }

        private string _dataType;
        public string DataType
        {
            get { return _dataType; }
            set
            {
                SetProperty(ref _dataType, value);
            }
        }

        public ParamVar(ParameterAttribute p)
        {
            Attr = p;
            DataSrc = $"{p.Owner.Alias}.{p.CN}";
            Alias = string.IsNullOrEmpty(p.Alias) ? p.CN : p.Alias;
            DataType = ParameterAttribute.GetDataTypeByType(p.Type);
        }
    }

    public class SwitchDialogVM : TextEditorVM
    {
        private static readonly string ADD = "新增";
        private static readonly string EDIT = "编辑";

        private IDialogService dialog;
        public SwitchDialogVM(IDialogService dialog)
        {
            TagList = new ObservableCollection<LExpression>();
            DocumentText = new TextDocument() { Text = "" };
            this.dialog = dialog;
        }

        /// <summary>
        /// 新增动作
        /// </summary>
        private string _action = ADD;
        public string Action
        {
            get => _action;
            set { SetProperty(ref _action, value); }
        }

        /// <summary>
        /// 标签
        /// </summary>
        private string _tagName;
        public string TagName
        {
            get => _tagName;
            set { SetProperty(ref _tagName, value); }
        }

        /// <summary>
        /// Case标签自动排序
        /// </summary>
        private bool _autoSort = false;
        public bool AutoSort
        {
            get => _autoSort;
            set
            {
                SetProperty(ref _autoSort, value);
                if (_autoSort)
                {
                    SortAllCase();
                }
            }
        }

        /// <summary>
        /// 标签列表
        /// </summary>
        private ObservableCollection<LExpression> _tagList;
        public ObservableCollection<LExpression> TagList
        {
            get => _tagList;
            set { SetProperty(ref _tagList, value); }
        }


        /// <summary>
        /// 关闭对话框
        /// </summary>
        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(() =>
        {
            if (string.IsNullOrEmpty(TagName))
            {
                throw new FriendlyException("条件名称不能为空!");
            }

            if (Action == ADD)
            {
                string exText = DocumentText.Text;

                // 1.解析表达式获取变量对象
                LExpression lExpress = new LExpression()
                {
                    StringEx = exText,
                    Name = TagName,
                    Variables = GetVariables(exText)
                };

                TagList.Add(lExpress);

                if (_autoSort)
                {
                    SortAllCase();
                }
            }
            else
            {
                UpdateExpress(cExpress);
                int index = TagList.IndexOf(cExpress);
                TagList.Remove(cExpress);
                TagList.Insert(index, cExpress);
                Action = ADD;
            }

            // 清空数据
            TagName = "";
            DocumentText = new TextDocument();
        }));

        /// <summary>
        /// 对Case进行排序
        /// </summary>
        private void SortAllCase()
        {
            var a = TagList.OrderBy(item => item.Name).ToList();
            TagList.Clear();
            foreach (var b in a)
            {
                TagList.Add(b);
            }
        }

        /// <summary>
        /// 当前表达式
        /// </summary>
        private LExpression cExpress;

        /// <summary>
        /// 点击标签
        /// </summary>
        private DelegateCommand<object> _tagSelectCommand;
        public DelegateCommand<object> TagSelectCommand => _tagSelectCommand ?? (_tagSelectCommand = new DelegateCommand<object>((obj) =>
        {
            SelectionChangedEventArgs args = obj as SelectionChangedEventArgs;

            if (args == null) return;

            if (args.AddedItems.Count > 0)
            {
                cExpress = args.AddedItems[0] as LExpression;
                TagName = cExpress.Name;
                DocumentText = new TextDocument()
                {
                    Text = cExpress.StringEx
                };

                Action = EDIT;
            }
        }));

        private void UpdateExpress(LExpression lExpress)
        {
            lExpress.StringEx = DocumentText.Text;

            lExpress.Variables = GetVariables(lExpress.StringEx);
            lExpress.Name = TagName;
        }

        /// <summary>
        /// 点击标签
        /// </summary>
        private DelegateCommand<object> _removeCondition;
        public DelegateCommand<object> RemoveCondition => _removeCondition ?? (_removeCondition = new DelegateCommand<object>((tag) =>
        {
            LExpression exp = tag as LExpression;
            if (exp != null)
            {
                dialog.ShowConfirm($"确定要删除条件:{exp.Name}", r =>
                 {
                     if (r.Result == ButtonResult.OK)
                     {
                         TagList.Remove(exp);
                     }
                 });
            }
        }));

        /// <summary>
        /// 条件上移
        /// </summary>
        private DelegateCommand<object> _moveUpCommand;
        public DelegateCommand<object> MoveUpCommand => _moveUpCommand ?? (_moveUpCommand = new DelegateCommand<object>((tag) =>
        {
            if (tag is LExpression exp)
            {
                int index = TagList.IndexOf(exp);
                if (index > 0)
                {
                    TagList.Remove(exp);
                    TagList.Insert(index - 1, exp);
                }
            }
        }));

        /// <summary>
        /// 条件下移
        /// </summary>
        private DelegateCommand<object> _moveDownCommand;
        public DelegateCommand<object> MoveDownCommand => _moveDownCommand ?? (_moveDownCommand = new DelegateCommand<object>((tag) =>
        {
            if (tag is LExpression exp)
            {
                int index = TagList.IndexOf(exp);
                if (index < TagList.Count - 1)
                {
                    TagList.Remove(exp);
                    TagList.Insert(index + 1, exp);
                }
            }
        }));

        /// <summary>
        /// 点击确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            if (TagList.Count == 0)
            {
                throw new FriendlyException("不存在条件!");
            }

            if (Action == EDIT)
            {
                AddCommand.Execute();
            }


            LCondition condition = new LCondition();
            List<string> names = new List<string>();
            foreach (var item in TagList)
            {
                item.ClearCache();

                // 检查变量是否有效
                CommonHelper.ParseKeywords(Variables, item.StringEx);

                condition.Add(item);
                names.Add(item.Name);
            }

            condition.Name = string.Join(",", names);

            result.Parameters.Add("Condition", condition);
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
                if (Parameter != null)
                {
                    TagList = new ObservableCollection<LExpression>();

                    if (parameter.Value != null)
                    {
                        // 解析
                        foreach (var item in parameter.Value as LCondition)
                        {
                            TagList.Add(item);
                        }
                    }
                }
            }
            else
            {
                throw new KeyNotFoundException("Parameter");
            }
        }
    }
}