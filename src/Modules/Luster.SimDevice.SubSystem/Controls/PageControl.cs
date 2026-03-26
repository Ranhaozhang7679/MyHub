#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PageControl
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Views
* 文 件 名:       PageControl.cs
* 创建时间:       2022/4/18 11:37:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7ddee10a-02ba-4e50-812d-513014455030
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 11:37:28
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.SimDevice.SubSystem.Controls
{
    public class PageControl : System.Windows.Controls.Control
    {
        public PageControl()
        {

        }

        /// <summary>
        /// 是否显示Add按钮
        /// </summary>
        public bool IsShowAdd
        {
            get { return (bool)GetValue(IsShowAddProperty); }
            set { SetValue(IsShowAddProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShowAdd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowAddProperty =
            DependencyProperty.Register("IsShowAdd", typeof(bool), typeof(PageControl), new PropertyMetadata(0));

        /// <summary>
        /// 页面标题
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PageControl), new PropertyMetadata(0));


        /// <summary>
        /// 图标
        /// </summary>
        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(PageControl), new PropertyMetadata(0));


        /// <summary>
        /// AddCommand
        /// </summary>
        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(PageControl), new PropertyMetadata(null));


        /// <summary>
        /// Button 按钮
        /// </summary>
        protected const string PART_ButtonViewAdd = nameof(PART_ButtonViewAdd);
        private Button btnViewAdd = null;
        public Button ViewButton
        {
            get
            {
                return btnViewAdd;
            }
            set
            {
                // 先反注册事件
                if (btnViewAdd != null)
                {
                    btnViewAdd.Click -= BtnViewAdd_Click;
                }

                btnViewAdd = value;

                if (btnViewAdd != null)
                {
                    btnViewAdd.Click += BtnViewAdd_Click;
                }
            }
        }

        private void BtnViewAdd_Click(object sender, RoutedEventArgs e)
        {
            AddCommand.Execute(this);
        }

        /// <summary>
        /// 解析对象
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ViewButton = GetTemplateChild(PART_ButtonViewAdd) as Button;
        }
    }
}