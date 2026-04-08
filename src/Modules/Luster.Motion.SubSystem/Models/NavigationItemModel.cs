using Luster.Motion.Assests.Langs;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace Luster.Motion.SubSystem.Models
{
    public class NavigationItemModel : BindableBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        private int _index;
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, value); }
        }

        /// <summary>
        /// 名称（语言资源键）
        /// </summary>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 显示名称（本地化文本）
        /// </summary>
        public string DisplayName
        {
            get
            {
                // 通过反射从 Lang 获取本地化文本
                var property = typeof(Lang).GetProperty(Name);
                if (property != null)
                {
                    return property.GetValue(null, null)?.ToString() ?? Name;
                }
                return Name;
            }
        }

        /// <summary>
        /// 目标视图名称（Region 导航用）
        /// </summary>
        private string _region = "";
        public string Region
        {
            get { return _region; }
            set { SetProperty(ref _region, value); }
        }

        /// <summary>
        /// 目标Region名称
        /// </summary>
        public string TargetRegion { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 刷新显示名称（语言切换时调用）
        /// </summary>
        public void RefreshDisplayName()
        {
            RaisePropertyChanged(nameof(DisplayName));
        }

        /// <summary>
        /// 选中项为互斥
        /// </summary>
        /// <param name="name"></param>
        public void SetSelected(string Name)
        {
            foreach (var item in NavigationItemModel.Pages)
            {
                if (item.Name != Name)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = true;
                }
            }
        }

        private static ObservableCollection<NavigationItemModel> _pages;

        public static ObservableCollection<NavigationItemModel> Pages
        {
            get
            {
                if (_pages == null)
                {
                    _pages = new ObservableCollection<NavigationItemModel>();
                }
                return _pages;
            }
        }
    }
}
