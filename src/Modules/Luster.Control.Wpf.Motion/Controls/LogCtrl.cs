#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogCtrl
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Controls
* 文 件 名:       LogCtrl.cs
* 创建时间:       2022/8/7 15:49:04
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e7010def-7613-4c68-8dd7-3d82af2f9e53
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/7 15:49:04
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace Luster.Control.Wpf.Motion.Controls
{

    public class LogData
    {
        public Run Text { get; set; }
    }

    /// <summary>
    /// log 日志组件
    /// </summary>
    public class LogCtrl : System.Windows.Controls.Control
    {
        /// <summary>
        /// ID
        /// </summary>
        private const string RichTextBoxStr = "PART_RichTextBox";

        /// <summary>
        /// 日志最大行数
        /// </summary>
        private const int MaxLine = 500;

        /// <summary>
        /// 是否暂停
        /// </summary>
        private bool isPause = false;

        /// <summary>
        /// 段落
        /// </summary>
        private Paragraph paragraph;

        /// <summary>
        /// log 信息
        /// </summary>
        public LogInfo LogInfo
        {
            get { return (LogInfo)GetValue(LogInfoProperty); }
            set { SetValue(LogInfoProperty, value); }
        }

        private static Dictionary<LogType, SolidColorBrush> LogBrushes = new Dictionary<LogType, SolidColorBrush>();

        public LogCtrl()
        {
            LogBrushes?.Clear();
            LogBrushes.Add(LogType.Debug, Brushes.Black);
            LogBrushes.Add(LogType.Info, Brushes.Blue);
            LogBrushes.Add(LogType.Warning, Brushes.Orange);
            LogBrushes.Add(LogType.Error, Brushes.Red);
        }


        // Using a DependencyProperty as the backing store for LogInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LogInfoProperty =
            DependencyProperty.Register("LogInfo", typeof(LogInfo), typeof(LogCtrl), new PropertyMetadata(default, new PropertyChangedCallback(PropertyChangedCallback)));

        /// <summary>
        /// 属性变更
        /// </summary>
        /// <param name="obj">obj</param>
        /// <param name="e">e</param>
        private static void PropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            LogCtrl ctrl = (LogCtrl)obj;
            if (ctrl != null)
            {

            }
        }


        /// <summary>
        /// 构建log
        /// </summary>
        /// <param name="logInfo"></param>
        public void AddLog(LogInfo logInfo)
        {
            if (RichTextBox == null || logInfo == null) return;

            RichTextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                var document = RichTextBox.Document;
                if (document == null) return;

                if (paragraph == null || paragraph.Inlines.Count > MaxLine)
                {
                    RichTextBox.Document.Blocks.Clear();
                    if (paragraph != null)
                    {
                        paragraph.Inlines.Clear();
                    }

                    paragraph = new Paragraph();
                    document.Blocks.Add(paragraph);
                }

                CreateParagraph(paragraph, logInfo);
                /*document.Blocks.Add(p)*/
                RichTextBox.ScrollToEnd();
            }));
        }

        public void ClearLog()
        {
            RichTextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                var document = RichTextBox.Document;
                if (document == null) return;
                if (paragraph != null)
                {
                    paragraph.Inlines.Clear();
                    paragraph = new Paragraph();
                }

                document.Blocks.Clear();

                if (paragraph != null)
                    document.Blocks.Add(paragraph);
            }));
        }

        private static void CreateParagraph(Paragraph p, LogInfo logInfo)
        {
            Run run = new Run();
            run.Text = $"{DateTime.Now.ToString("HH:mm:ss:fff")} {logInfo.LogThreadID} [{logInfo.LogType}] {logInfo.LogMessage} {Environment.NewLine}";
            run.Foreground = LogBrushes[logInfo.LogType];
            p.Inlines.Add(run);
        }

        /// <summary>
        /// 富文本框
        /// </summary>
        public RichTextBox RichTextBox;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RichTextBox = GetTemplateChild(RichTextBoxStr) as RichTextBox;
            RichTextBox.Document.Blocks.Clear();
        }
    }
}