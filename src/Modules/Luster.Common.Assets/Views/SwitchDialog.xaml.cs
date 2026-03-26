using HandyControl.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using Luster.Common.Assets.Tools;
using Luster.Common.Assets.ViewModel;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Common.Assets.Views
{
    /// <summary>
    /// ExpressDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SwitchDialog
    {
        /// <summary>
        /// 变量类型
        /// </summary>
        private List<ParamVar> paramVars = new List<ParamVar>();

        /// <summary>
        /// 变量
        /// </summary>
        private List<string> variables = new List<string>();

        private readonly char[] Seperators = new char[] { '+', '-', '*', '/', '>', '<', '=', '.', };

        // 原始 xml 信息
        private const string LUSTERXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
				<SyntaxDefinition name=""Luster"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
				<Color name=""Comment"" foreground= ""Green"" />
				<Color name=""String"" foreground=""Blue"" />
				<RuleSet>
					<Keywords fontWeight=""bold"" foreground=""Blue"" >
						<Word>Abs</Word>
						<Word>Sqrt</Word>
					</Keywords>
					<Keywords fontWeight=""bold"" fontStyle=""italic"" foreground=""Red"" >
						<Word>AvalonEdit</Word>
					</Keywords>
					<MyKeywords />
					<Rule foreground=""DarkBlue"" >
						\b0[xX][0-9a-fA-F]+
						|	 \b
						(    \d+(\.[0-9]+)?
						|    \.[0-9]+
						)
						([eE][+-]?[0-9]+)?
					</Rule>
				</RuleSet>
				</SyntaxDefinition>";

        /// <summary>
        /// 完成窗口
        /// </summary>
        private CompletionWindow completionWindow;


        public SwitchDialog()
        {
            InitializeComponent();

            // 智能提示
            richText.TextArea.TextEntered += TextArea_TextEntered;
            richText.TextArea.TextEntering += TextArea_TextEntering;
            richText.Document = new TextDocument();
            richText.IsReadOnly = false;
            this.Loaded += ExpressDialog_Loaded;
        }


        /// <summary>
        /// Text 渲染
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        /// <summary>
        /// 控件加载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void ExpressDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取所有变量对象
            var nodes = treeVar.HierarchySource as List<LNode>;
            if (nodes != null)
            {
                variables = await Task.Run(() => CommonHelper.GetNodeKeys(nodes));
                await BuildHighlightingAsync(variables.ToArray());
            }

            richText.Focus();

            // 更新样式
            if (this.DataContext is TextEditorVM vm)
            {
                var style = HandyControl.Tools.ResourceHelper.GetResource<Style>(vm.IconStyle);
                if (style != null)
                {
                    this.Resources["IconSmall"] = style;
                }
            }
        }

        /// <summary>
        /// 渲染成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (variables == null || variables.Count == 0) return;

            var existKeywords = variables.Where(u => u.IndexOf(e.Text) == 0).ToList();
            if (existKeywords.Count > 0)
            {
                // Open code completion after the user has pressed dot:
                ShowWindow(existKeywords);
            }
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        /// <param name="keyWords"></param>
        private void ShowWindow(List<string> keyWords)
        {
            completionWindow = new CompletionWindow(richText.TextArea);
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            foreach (var item in keyWords)
            {
                data.Add(new MyCompletionData(item));
            }

            completionWindow.Show();
            completionWindow.Closed += delegate
            {
                completionWindow = null;
            };
        }

        /// <summary>
        /// 添加按钮
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string content = btn.Tag.ToString();
            switch (content)
            {
                case "CE": // 清除当前
                case "C":  // 清除所有
                    richText.Clear();
                    break;

                default:
                    richText.AppendText(content);
                    break;
            }

            richText.Focus();
        }

        /// <summary>
        /// 构建关键字
        /// </summary>
        /// <param name="varibales"></param>
        public async Task BuildHighlightingAsync(params string[] varibales)
        {
            var xKeyWords = await Task.Run(() => 
            {
                var x = new XElement("Keywords");
                x.Add(new XElement("Word", "Abs"));
                foreach (var item in varibales)
                {
                    x.Add(new XElement("Word", item));
                }
                x.SetAttributeValue("fontWeight", "bold");
                x.SetAttributeValue("foreground", "blue");
                return x;
            });

            string curXML = LUSTERXML.Replace("<MyKeywords />", xKeyWords.ToString());
            StringReader strRdr = new StringReader(curXML);
            using (XmlReader reader = new XmlTextReader(strRdr))
            {
                var customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);

                HighlightingManager.Instance.RegisterHighlighting("Luster", new string[] { ".luster" }, customHighlighting);

                // 设置默认值
                richText.SyntaxHighlighting = customHighlighting;
            }
        }

        private void TagSelected(object sender, EventArgs e)
        {
            var vm = this.DataContext as SwitchDialogVM;
            if (vm != null)
            {
                Tag tag = sender as Tag;
                vm.TagSelectCommand.Execute(tag.DataContext);
            }
        }

        /// <summary>
        /// 单击进入选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstVars_Selected(object sender, RoutedEventArgs e)
        {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);
            }
        }
    }


    internal class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            this.Text = text;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return "Description for " + this.Text; }
        }

        public double Priority => 1.0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text.Substring(1));
        }
    }
}


