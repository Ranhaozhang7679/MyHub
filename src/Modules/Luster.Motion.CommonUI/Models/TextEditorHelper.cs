using ICSharpCode.AvalonEdit;
using System;
using System.Windows;

namespace Luster.Motion.CommonUI.Models
{
    public static class TextEditorHelper
    {
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached(
                "BindableText",
                typeof(string),
                typeof(TextEditorHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableTextChanged));

        public static string GetBindableText(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject obj, string value)
        {
            obj.SetValue(BindableTextProperty, value);
        }

        private static void OnBindableTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as TextEditor;
            if (editor != null)
            {
                if (editor.Text != (string)e.NewValue)
                    editor.Text = (string)e.NewValue;

                // 避免重复订阅
                editor.TextChanged -= Editor_TextChanged;
                editor.TextChanged += Editor_TextChanged;
            }
        }

        private static void Editor_TextChanged(object sender, EventArgs e)
        {
            var editor = sender as TextEditor;
            if (editor != null)
            {
                SetBindableText(editor, editor.Text);
            }
        }
    }
}