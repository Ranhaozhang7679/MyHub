using DC.Authorization.Models;
using DC.Authorization.WPF.Models;
using System.Collections.Generic;
using System.Linq;

namespace DC.Authorization.WPF
{
    public static class RightTreeBuilder
    {
        /// <summary>
        /// 将扁平的权限列表构建成三级树结构
        /// </summary>
        /// <param name="allRights">所有权限项</param>
        /// <param name="assignedRightIds">当前角色已分配的权限 ID 列表</param>
        public static List<RightTreeNode> Build(
            List<Right> allRights,
            List<int> assignedRightIds)
        {
            var tree = new List<RightTreeNode>();

            // 按 SortOrder 排序后，一级按 ModuleName 分组
            var moduleGroups = allRights
                .OrderBy(r => r.SortOrder)
                .GroupBy(r => r.ModuleName ?? "未分类");

            foreach (var moduleGroup in moduleGroups)
            {
                var moduleNode = new RightTreeNode
                {
                    DisplayName = moduleGroup.Key
                };

                // 二级：按 ViewName 分组，组内按 SortOrder 排序
                var viewGroups = moduleGroup
                    .GroupBy(r => r.ViewName ?? "默认");

                foreach (var viewGroup in viewGroups)
                {
                    var viewNode = new RightTreeNode
                    {
                        DisplayName = viewGroup.Key,
                        Parent = moduleNode
                    };

                    // 三级：权限项（叶子节点），按 SortOrder 排序
                    foreach (var right in viewGroup.OrderBy(r => r.SortOrder))
                    {
                        var leafNode = new RightTreeNode
                        {
                            DisplayName = !string.IsNullOrEmpty(right.Description)
                                ? right.Description
                                : right.Name,
                            RightId = right.Id,
                            Parent = viewNode,
                            IsChecked = assignedRightIds.Contains(right.Id)
                        };
                        viewNode.Children.Add(leafNode);
                    }

                    moduleNode.Children.Add(viewNode);
                }

                moduleNode.UpdateCheckStateFromChildren();
                tree.Add(moduleNode);
            }

            return tree;
        }

        /// <summary>
        /// 从树中收集所有被勾选的权限项 ID（仅叶子节点）
        /// </summary>
        public static List<int> CollectCheckedRightIds(List<RightTreeNode> tree)
        {
            var result = new List<int>();
            CollectRecursive(tree, result);
            return result;
        }

        private static void CollectRecursive(
            IEnumerable<RightTreeNode> nodes, List<int> result)
        {
            foreach (var node in nodes)
            {
                if (node.IsLeaf && node.IsChecked && node.RightId.HasValue)
                    result.Add(node.RightId.Value);
                else
                    CollectRecursive(node.Children, result);
            }
        }
    }
}
