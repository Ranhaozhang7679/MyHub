using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools.FlowTreeChart
{
    /// <summary>
    /// 流程树布局
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class FlowTreeHelpers<T>
        where T : class
    {
        private static int nodeSize = 1;
        private static float siblingDistance = 0.0F;
        private static float treeDistance = 0.0F;
 
        public static void CalculateNodePositions(FlowTreeNodeModel<T> rootNode)
        {
            InitializeNodes(rootNode, 0);
            CalculateInitialX(rootNode);
            CheckAllChildrenOnScreen(rootNode);
            CalculateFinalPositions(rootNode, 0);
        }

        private static void InitializeNodes(FlowTreeNodeModel<T> node, int depth)
        {
            node.X = -1;
            node.Y = depth;
            node.Mod = 0;

            foreach (var child in node.Children)
            {
                InitializeNodes(child, depth + 1);
            }
        }

        private static void CalculateFinalPositions(FlowTreeNodeModel<T> node, float modSum)
        {
            node.X += modSum;
            modSum += node.Mod;

            foreach (var child in node.Children)
                CalculateFinalPositions(child, modSum);

            if (node.Children.Count == 0)
            {
                node.Width = node.X;
                node.Height = node.Y;
            }
            else
            {
                node.Width = node.Children.OrderByDescending(p => p.Width).First().Width;
                node.Height = node.Children.OrderByDescending(p => p.Height).First().Height;
            }
        }

        private static void CalculateInitialX(FlowTreeNodeModel<T> node)
        {
            foreach (var child in node.Children)
            {
                CalculateInitialX(child);
            }
 
            if (node.IsLeaf())
            {
                if (!node.IsLeftMost())
                {
                    node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                }
                else
                {  
                    node.X = 0;
                }
            }

            else if (node.Children.Count == 1)
            {
                if (node.IsLeftMost())
                {
                    node.X = node.Children[0].X;
                }
                else
                {
                    node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                    node.Mod = node.X - node.Children[0].X;
                } 
            }
            else
            {
                var leftChild = node.GetLeftMostChild();
                var rightChild = node.GetRightMostChild();
                var mid = (leftChild.X + rightChild.X) / 2;
 
                if (node.IsLeftMost())
                {
                    node.X = mid;
                }
                else
                {
                    node.X = node.GetPreviousSibling().X + nodeSize + siblingDistance;
                    node.Mod = node.X - mid;
                }
            }
            
            if (node.Children.Count > 0 && !node.IsLeftMost())
            {
                CheckForConflicts(node);
            }
        }
 
        private static void CheckForConflicts(FlowTreeNodeModel<T> node)
        {
            var minDistance = treeDistance + nodeSize;
            var shiftValue = 0F;
 
            var nodeContour = new Dictionary<int, float>();
            GetLeftContour(node, 0, ref nodeContour);
 
            var sibling = node.GetLeftMostSibling();
            while (sibling != null && sibling != node)
            {
                var siblingContour = new Dictionary<int, float>();
                GetRightContour(sibling, 0, ref siblingContour);
 
                for (int level = node.Y + 1; level <= Math.Min(siblingContour.Keys.Max(), nodeContour.Keys.Max()); level++)
                {
                    var distance = nodeContour[level] - siblingContour[level];
                    if (distance + shiftValue < minDistance)
                    {
                        shiftValue = minDistance - distance;
                    }
                }
 
                if (shiftValue > 0)
                {
                    node.X += shiftValue;
                    node.Mod += shiftValue;

                    CenterNodesBetween(node, sibling);

                    shiftValue = 0;
                }
 
                sibling = sibling.GetNextSibling();
            }
        }

        private static void CenterNodesBetween(FlowTreeNodeModel<T> leftNode, FlowTreeNodeModel<T> rightNode)
        {
            var leftIndex = leftNode.Parent.Children.IndexOf(rightNode);
            var rightIndex = leftNode.Parent.Children.IndexOf(leftNode);
                    
            var numNodesBetween = (rightIndex - leftIndex) - 1;

            if (numNodesBetween > 0)
            {
                var distanceBetweenNodes = (leftNode.X - rightNode.X) / (numNodesBetween + 1);

                int count = 1;
                for (int i = leftIndex + 1; i < rightIndex; i++)
                {
                    var middleNode = leftNode.Parent.Children[i];

                    var desiredX = rightNode.X + (distanceBetweenNodes * count);
                    var offset = desiredX - middleNode.X;
                    middleNode.X += offset;
                    middleNode.Mod += offset;

                    count++;
                }

                CheckForConflicts(leftNode);
            }
        }
 
        private static void CheckAllChildrenOnScreen(FlowTreeNodeModel<T> node)
        {
            var nodeContour = new Dictionary<int, float>();
            GetLeftContour(node, 0, ref nodeContour);

            float shiftAmount = 0;
            foreach (var y in nodeContour.Keys)
            {
                if (nodeContour[y] + shiftAmount < 0)
                    shiftAmount = (nodeContour[y] * -1);
            }

            if (shiftAmount > 0)
            {
                node.X += shiftAmount;
                node.Mod += shiftAmount;
            }
        }
 
        private static void GetLeftContour(FlowTreeNodeModel<T> node, float modSum, ref Dictionary<int, float> values)
        {
            if (!values.ContainsKey(node.Y))
            {
                values.Add(node.Y, node.X + modSum);
            }
            else
            {
                values[node.Y] = Math.Min(values[node.Y], node.X + modSum);
            }
 
            modSum += node.Mod;
            foreach (var child in node.Children)
            {
                GetLeftContour(child, modSum, ref values);
            }
        }
 
        private static void GetRightContour(FlowTreeNodeModel<T> node, float modSum, ref Dictionary<int, float> values)
        {
            if (!values.ContainsKey(node.Y))
            {
                values.Add(node.Y, node.X + modSum);
            }
            else
            {
                values[node.Y] = Math.Max(values[node.Y], node.X + modSum);
            }
 
            modSum += node.Mod;
            foreach (var child in node.Children)
            {
                GetRightContour(child, modSum, ref values);
            }
        }
    }
}
