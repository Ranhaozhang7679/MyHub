#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       ModuleContentVM.cs
* 创建时间:       2022/6/8 9:17:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      61ea2e7d-1ecd-478a-aa25-6d2ffc337a79
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/8 9:17:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
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
using System.Windows.Media;
using Luster.Motion.DataStruct;
using HandyControl.Controls;

namespace Luster.Motion.EditorUI.ViewModel
{
    public class SearchContentVM : BaseFlowVM
    {
        public SearchContentVM(FlowBus _eventBus,
            ICommonBus cBus) : base(_eventBus, cBus)
        {
            SearchItems = new List<KeyValue>();
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
        }

        #region 快捷搜索功能
        /// <summary>
        /// 支持粘贴功能
        /// </summary>
        private bool _showSearch = false;
        public bool ShowSearch
        {
            get { return _showSearch; }
            set { SetProperty(ref _showSearch, value); }
        }

        /// <summary>
        /// 列表
        /// </summary>
        private List<KeyValue> _searchItems;
        public List<KeyValue> SearchItems
        {
            get { return _searchItems; }
            set { SetProperty(ref _searchItems, value); }
        }

        /// <summary>
        /// 搜索文本
        /// </summary>
        private string _searchKey;
        public string SearchKey
        {
            get { return _searchKey; }
            set
            {
                SetProperty(ref _searchKey, value);
                //FilterItems(0, value);
            }
        }

        /// <summary>
        /// 搜索文本
        /// </summary>
        private string _searchType;
        public string SearchType
        {
            get { return _searchType; }
            set
            {
                SetProperty(ref _searchType, value);
                //FilterItems(1, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchKey"></param>
        private void FilterItems(int type, string searchKey)
        {
            if (SearchItems == null)
            {
                SearchItems = new List<KeyValue>();
            }

            SearchItems.Clear();

            // 搜索关键字
            SearchItems = eventBus.SearchFuncByKey(type, searchKey);
        }

        /// <summary>
        /// 快捷搜索命令
        /// </summary>
        private DelegateCommand<string> _searchCommand;
        public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((txt) =>
        {
            SearchKey = txt;
            FilterItems(0, txt);
        }));

        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _goToCommand;
        public DelegateCommand<object> GoToCommand => _goToCommand ?? (_goToCommand = new DelegateCommand<object>((obj) =>
        {
            var kVal = obj as KeyValue;
            if (kVal != null && kVal.Value is IMotionModule m)
            {
                // 切换到模块所在层级
                eventBus.OnLoaded(m.Parent, m);
                eventBus.OnSelected(m);
            }
        }));
        #endregion

        /// <summary>
        /// 快捷搜索命令
        /// </summary>
        private DelegateCommand<string> _searchTypeCommand;
        public DelegateCommand<string> SearchTypeCommand => _searchTypeCommand ?? (_searchTypeCommand = new DelegateCommand<string>((txt) =>
        {
            SearchType = txt;
            FilterItems(1, txt);
        }));
    }
}