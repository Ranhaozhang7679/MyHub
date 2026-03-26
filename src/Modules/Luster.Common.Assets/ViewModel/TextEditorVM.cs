using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Luster.Common.Assets.Tools;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Common.Assets.ViewModel
{
    public class TextEditorVM : BaseDialogVM
    {
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

        public TextEditorVM()
        {
            DocumentText = new TextDocument() { Text = "" };
        }


        /// <summary>
        /// 标签列表
        /// </summary>
        private string _previewText;
        public string PreviewText
        {
            get => _previewText;
            set { SetProperty(ref _previewText, value); }
        }

        /// <summary>
        /// 标签列表
        /// </summary>
        private List<LNode> _variables;
        public List<LNode> Variables
        {
            get => _variables;
            set { SetProperty(ref _variables, value); }
        }

        /// <summary>
        /// 文档编辑器
        /// </summary>
        protected TextEditor textEditor;

        /// <summary>
        /// 点击标签
        /// </summary>
        private DelegateCommand<object> _selectVarCommand;
        public DelegateCommand<object> SelectVarCommand => _selectVarCommand ?? (_selectVarCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is LNode lNode && lNode.Tag is ParameterAttribute p)
            {
                if (textEditor == null)
                {
                    DocumentText.Text = $"{DocumentText.Text}{p.Owner.Alias}.{p.CN}";
                }
                else
                {
                    DocumentText.Insert(textEditor.SelectionStart, $"{p.Owner.Alias}.{p.CN}");
                }
            }
        }));


        /// <summary>
        /// 针对变量是否运行进行检测
        /// </summary>
        protected void CheckVarValidate(LArray<LVariable> variables)
        {
            foreach (var item in variables)
            {
                if (item.Tag is ParameterAttribute p)
                {
                    if (p.Owner is IGlobal) continue;

                    // 如果值类型，输出默认值
                    if (p.Type.IsValueType || p.Type == typeof(LStatus)) continue;

                    // 如果引用类型，要检测模块是否有无正常运行
                    if (p.Owner.Status != TaskFlow.Common.Enums.RunStatus.Success)
                    {
                        throw new FriendlyException($"模块:{p.Owner.Alias}未运行,无法预览!");
                    }
                }
            }
        }

        /// <summary>
        /// 预览命令
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            textEditor = obj as TextEditor;
        }));

        private List<LNode> originNodes = new List<LNode>();
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

            // 加载变量
            if (parameters.TryGetValue<List<LNode>>("Variables", out var variables))
            {
                if (variables != null)
                {
                    CommonHelper.BuildTreeNodes(variables);
                    originNodes = variables;
                    Variables = variables;
                }
            }

            // 自定义样式
            if (parameters.TryGetValue<string>("IconStyle", out var style))
            {
                IconStyle = style;
            }
        }

        /// <summary>
        /// 变量对应的图标样式
        /// </summary>
        private string _iconStyle = "IconSmall";
        public string IconStyle
        {
            get { return _iconStyle; }
            set { SetProperty(ref _iconStyle, value); }
        }

        protected LArray<LVariable> GetVariables(string strEx)
        {
            var variables = CommonHelper.ParseKeywords(Variables, strEx);

            foreach (var item in variables)
            {
                CommonHelper.CheckVar(item.Alias);
            }

            return variables;
        }

        #region 搜索
        /// <summary>
        /// 标签列表
        /// </summary>
        private string _searchKey;
        public string SearchKey
        {
            get => _searchKey;
            set
            {
                SetProperty(ref _searchKey, value);
                Search();
            }
        }

        /// <summary>
        /// 快捷搜索
        /// </summary>
        /// <param name="text"></param>
        /// <summary>
        /// 快捷搜索
        /// </summary>
        /// <param name="text"></param>
        private void Search()
        {
            if (string.IsNullOrEmpty(SearchKey))
            {
                Variables = new List<LNode>(originNodes);
                return;
            }

            // 找到集合
            List<LNode> lNodes = new List<LNode>();
            SearchNodes(SearchKey, originNodes, lNodes, node =>
            {
                return node.Tag is not ParameterAttribute;
            });

            // 递归
            Variables = lNodes;
        }

        private void SearchNodes(string key, List<LNode> allNodes, List<LNode> searchNodes, Func<LNode, bool> func = null)
        {
            foreach (var item in allNodes)
            {
                if (func != null && func(item))
                {
                    if (item.Text.Contains(key))
                    {
                        if (!searchNodes.Contains(item))
                        {
                            searchNodes.Add(item);
                        }
                    }
                }

                SearchNodes(key, item.Children, searchNodes, func);
            }
        }
        #endregion
    }

    public class ParamDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 数字模板
        /// </summary>
        public DataTemplate NormalTemplate { get; set; }

        /// <summary>
        /// 模拟量模板
        /// </summary>
        public DataTemplate ParamTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is LNode lNode && lNode.Tag is ParameterAttribute p)
            {
                return ParamTemplate;
            }
            else
            {
                return NormalTemplate;
            }
        }
    }
}