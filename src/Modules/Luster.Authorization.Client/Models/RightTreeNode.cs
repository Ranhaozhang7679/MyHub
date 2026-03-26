using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace DC.Authorization.WPF.Models
{
    /// <summary>
    /// 权限树节点，支持三级：模块 → 界面 → 权限项
    /// </summary>
    public class RightTreeNode : BindableBase
    {
        /// <summary>
        /// 节点显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 权限项 ID（仅叶子节点有值）
        /// </summary>
        public int? RightId { get; set; }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public ObservableCollection<RightTreeNode> Children { get; set; } = new();

        /// <summary>
        /// 父节点引用（用于子→父状态同步）
        /// </summary>
        public RightTreeNode Parent { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    // 向下传播：选中父节点 → 全选子节点
                    if (Children.Any())
                    {
                        foreach (var child in Children)
                            child.IsChecked = value;
                    }

                    // 向上传播：更新父节点状态
                    Parent?.UpdateCheckStateFromChildren();
                }
            }
        }

        /// <summary>
        /// 是否为叶子节点（权限项）
        /// </summary>
        public bool IsLeaf => RightId.HasValue;

        /// <summary>
        /// 根据子节点状态更新自身（不触发向下传播）
        /// </summary>
        internal void UpdateCheckStateFromChildren()
        {
            if (!Children.Any()) return;

            var allChecked = Children.All(c => c.IsChecked);

            // 直接设置字段避免触发循环
            _isChecked = allChecked;
            RaisePropertyChanged(nameof(IsChecked));

            Parent?.UpdateCheckStateFromChildren();
        }
    }
}
