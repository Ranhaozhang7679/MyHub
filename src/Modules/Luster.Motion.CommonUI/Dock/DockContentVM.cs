#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DockWindowModel
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       DockWindowModel.cs
* 创建时间:       2023/2/17 14:27:53
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1861dae2-a72a-48fa-9be4-6ce22aef2ccc
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/17 14:27:53
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using AvalonDock;
using AvalonDock.Layout.Serialization;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.ViewModel;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Luster.Motion.CommonUI
{
    /// <summary>
    /// 基于Dock的VM
    /// </summary>
    public abstract class DockContentVM : MotionPageVM
    {
        
        /// <summary>
        /// 窗体名称
        /// </summary>
        public abstract string WindowName { get; }

        #region Properties

        #region CloseCommand
        private ICommand _closeCommand;
        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand(CloseWindow));

        /// <summary>
        /// 关闭窗体
        /// </summary>
        private void CloseWindow()
        {

        }
        #endregion

        private bool _IsClosed;
        public bool IsClosed
        {
            get { return _IsClosed; }
            set
            {
                SetProperty(ref _IsClosed, value);
            }
        }

        private bool _CanClose;
        public bool CanClose
        {
            get { return _CanClose; }
            set
            {
                SetProperty(ref _CanClose, value);
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                SetProperty(ref _Title, value);
            }
        }


        public ImageSource IconSource { get; protected set; }

        private string _contentId;
        public string ContentId
        {
            get => _contentId;
            set
            {
                SetProperty(ref _contentId, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                SetProperty(ref _isActive, value);
            }
        }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bus"></param>
        public DockContentVM(ICommonBus bus) : base(bus)
        {
            CanClose = true;
            IsClosed = false;
        }

        public void Close()
        {
            IsClosed = true;
        }

        #region 容器布局加载和保存
        /// <summary>
        /// 程序加载自动
        /// </summary>
        public static DockingManager dockingManager = null;

        /// <summary>
        /// 加载布局
        /// </summary>
        /// <param name="layoutConfig"></param>
        public static void LoadLayout(string layoutConfig)
        {
            var layoutSerializer = new XmlLayoutSerializer(DockContentVM.dockingManager);

            // Here I've implemented the LayoutSerializationCallback just to show
            //  a way to feed layout desarialization with content loaded at runtime
            // Actually I could in this case let AvalonDock to attach the contents
            // from current layout using the content ids
            // LayoutSerializationCallback should anyway be handled to attach contents
            // not currently loaded
            layoutSerializer.LayoutSerializationCallback += (s, e) =>
            {
                //if (e.Model.ContentId == FileStatsViewModel.ToolContentId)
                //    e.Content = Workspace.This.FileStats;
                //else if (!string.IsNullOrWhiteSpace(e.Model.ContentId) &&
                //    File.Exists(e.Model.ContentId))
                //    e.Content = Workspace.This.Open(e.Model.ContentId);
            };
            layoutSerializer.Deserialize(layoutConfig);
        }

        /// <summary>
        /// 保存布局
        /// </summary>
        /// <param name="saveLayoutConfig"></param>
        public static void SaveLayout(string saveLayoutConfig)
        {
            var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(DockContentVM.dockingManager);
            serializer.Serialize(saveLayoutConfig);
        } 
        #endregion
    }
}