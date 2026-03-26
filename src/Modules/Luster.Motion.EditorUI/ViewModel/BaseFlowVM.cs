#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       BaseVM.cs
* 创建时间:       2022/5/24 11:13:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      76ab77d5-a2a6-4448-aadb-71f3e7586726
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 11:13:42
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.ViewModel;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.EditorUI.ViewModel
{
    public class BaseFlowVM : MotionVM
    {

		#region panels field
		private string _title = null;
		private string _contentId = null;
		private bool _isSelected = false;
		private bool _isActive = false;
		#endregion fields

		/// <summary>
		/// 事件总线
		/// </summary>
		protected FlowBus eventBus;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_eventBus"></param>
        public BaseFlowVM(FlowBus _eventBus, ICommonBus commonBus) : base(commonBus)
        {
            eventBus = _eventBus;
        }


		#region DockPanel 属性
		/// <summary>
		/// 页面标题
		/// </summary>
		public string Title
		{
			get => _title;
			set
			{
				SetProperty(ref _title, value);
			}
		}

		public ImageSource IconSource { get; protected set; }

		/// <summary>
		/// 内容ID
		/// </summary>
		public string ContentId
		{
			get => _contentId;
			set
			{
				SetProperty(ref _contentId, value);
			}
		}

		/// <summary>
		///是否选中
		/// </summary>
		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				SetProperty(ref _isSelected, value);
			}
		}

		/// <summary>
		/// 页面激活
		/// </summary>
		public bool IsActive
		{
			get => _isActive;
			set
			{
				SetProperty(ref _isSelected, value);
			}
		}
		#endregion Properties
	}
}