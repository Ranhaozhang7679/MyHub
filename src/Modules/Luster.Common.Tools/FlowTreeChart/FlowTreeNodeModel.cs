using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools.FlowTreeChart
{
    /// <summary>
    /// 流程树节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FlowTreeNodeModel<T>
        where T : class
    {
        public float X { get; set; }
        public int Y { get; set; }
        public float Mod { get; set; }
        public FlowTreeNodeModel<T> Parent { get; set; }
        public List<FlowTreeNodeModel<T>> Children { get; set; }

        public float Width { get; set; }
        public int Height { get; set; }

        public T Item { get; set; }

        public FlowTreeNodeModel(T item, FlowTreeNodeModel<T> parent)
        {
            this.Item = item;
            this.Parent = parent;
            this.Children = new List<FlowTreeNodeModel<T>>();
        }

        public bool IsLeaf()
        {
            return this.Children.Count == 0;
        }

        public bool IsLeftMost()
        {
            if (this.Parent == null)
                return true;

            return this.Parent.Children[0] == this;
        }

        public bool IsRightMost()
        {
            if (this.Parent == null)
                return true;

            return this.Parent.Children[this.Parent.Children.Count - 1] == this;
        }

        public FlowTreeNodeModel<T> GetPreviousSibling()
        {
            if (this.Parent == null || this.IsLeftMost())
                return null;

            return this.Parent.Children[this.Parent.Children.IndexOf(this) - 1];
        }

        public FlowTreeNodeModel<T> GetNextSibling()
        {
            if (this.Parent == null || this.IsRightMost())
                return null;

            return this.Parent.Children[this.Parent.Children.IndexOf(this) + 1];
        }

        public FlowTreeNodeModel<T> GetLeftMostSibling()
        {
            if (this.Parent == null)
                return null;

            if (this.IsLeftMost())
                return this;

            return this.Parent.Children[0];
        }

        public FlowTreeNodeModel<T> GetLeftMostChild()
        {
            if (this.Children.Count == 0)
                return null;

            return this.Children[0];
        }

        public FlowTreeNodeModel<T> GetRightMostChild()
        {
            if (this.Children.Count == 0)
                return null;

            return this.Children[Children.Count - 1];
        }

        //public override string ToString()
        //{
        //    return Item.ToString();
        //}
    }
}
