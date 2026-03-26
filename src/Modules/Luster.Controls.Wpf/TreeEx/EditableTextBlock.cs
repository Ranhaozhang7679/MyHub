#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EditableTextBlock
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.TreeEx
* 文 件 名:       EditableTextBlock
* 创建时间:       2021/11/4 10:38:17
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      721c96be-62c5-4258-8dbf-0e95feb0c97e
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/4 10:38:17
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Luster.Controls.Wpf.TreeEx
{
    /// <summary>
    /// Provides an editable text block.
    /// </summary>
    public class EditableTextBlock : TextBlock
    {
        /// <summary>
        /// Identifies the <see cref="IsEditing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            nameof(IsEditing),
            typeof(bool),
            typeof(EditableTextBlock),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) => ((EditableTextBlock)s).IsEditingChanged()));

        /// <summary>
        /// Flags if it is an internal change in the IsEditing property
        /// </summary>
        private bool internalIsEditingChange;

        /// <summary>
        /// The old focus element.
        /// </summary>
        private UIElement oldfocus;

        /// <summary>
        /// The text box.
        /// </summary>
        private TextBox textBox;

        /// <summary>
        /// Initializes static members of the <see cref="EditableTextBlock" /> class.
        /// </summary>
        static EditableTextBlock()
        {
            TextProperty.OverrideMetadata(
                typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editing.
        /// </summary>
        public bool IsEditing
        {
            get
            {
                return (bool)this.GetValue(IsEditingProperty);
            }

            set
            {
                this.SetValue(IsEditingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the content when editing
        /// </summary>
        [Bindable(true)]
        public HorizontalAlignment HorizontalContentAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the content when editing
        /// </summary>
        [Bindable(true)]
        public VerticalAlignment VerticalContentAlignment { get; set; }

        /// <summary>
        /// Begins the edit.
        /// </summary>
        private void BeginEdit()
        {
            if (this.textBox != null)
            {
                throw new InvalidOperationException();
            }

            var scope = FocusManager.GetFocusScope(this);
            this.oldfocus = FocusManager.GetFocusedElement(scope) as UIElement;
            this.textBox = new TextBox();
            this.textBox.SetBinding(
                TextBox.TextProperty,
                new Binding("Text") { Source = this, UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
            Grid.SetColumn(this.textBox, Grid.GetColumn(this));
            Grid.SetColumnSpan(this.textBox, Grid.GetColumnSpan(this));
            this.Visibility = Visibility.Collapsed;
            var p = this.Parent as Panel;
            if (p == null)
            {
                throw new InvalidOperationException(string.Format("{0} must be a child of a {1}", this.GetType(), typeof(Panel)));
            }

            int index = p.Children.IndexOf(this);
            p.Children.Insert(index, this.textBox);

            this.textBox.HorizontalAlignment = this.HorizontalAlignment;
            this.textBox.VerticalAlignment = this.VerticalAlignment;
            this.textBox.HorizontalContentAlignment = this.HorizontalContentAlignment;
            this.textBox.VerticalContentAlignment = this.VerticalContentAlignment;
            this.textBox.LostFocus += this.TextBoxLostFocus;
            this.textBox.KeyDown += this.TextBoxKeyDown;
            this.textBox.CaretIndex = this.textBox.Text.Length;
            this.textBox.SelectAll();
            this.textBox.Focus();
        }

        /// <summary>
        /// Ends the edit.
        /// </summary>
        /// <param name="commit">if set to <c>true</c> [commit].</param>
        private void EndEdit(bool commit)
        {
            if (this.textBox == null)
            {
                return;
            }

            var textBoxBindingExpression = this.textBox.GetBindingExpression(TextBox.TextProperty);
            var textBlockBindingExpression = this.GetBindingExpression(TextProperty);
            if (commit)
            {
                if (textBoxBindingExpression != null)
                {
                    textBoxBindingExpression.UpdateSource();
                }

                if (textBlockBindingExpression != null)
                {
                    textBlockBindingExpression.UpdateTarget();
                }
            }

            this.internalIsEditingChange = true;
            this.IsEditing = false;
            this.internalIsEditingChange = false;
            var p = (Panel)this.Parent;
            p.Children.Remove(this.textBox);
            this.textBox = null;
            this.Visibility = Visibility.Visible;
            if (this.oldfocus != null && this.oldfocus.IsVisible)
            {
                this.oldfocus.Focus();
            }
        }

        /// <summary>
        /// Handles changes in IsEditing.
        /// </summary>
        private void IsEditingChanged()
        {
            if (this.internalIsEditingChange)
            {
                return;
            }

            if (this.IsEditing)
            {
                this.BeginEdit();
            }
            else
            {
                this.EndEdit(true);
            }
        }

        /// <summary>
        /// TextBox key down handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs" /> instance containing the event data.</param>
        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.EndEdit(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.EndEdit(false);
                e.Handled = true;
            }
        }

        /// <summary>
        /// TextBox lost focus handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            this.EndEdit(true);
        }
    }
}
