using Aspose.Cells;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Windows.Forms;
using static Luster.Common.Tools.SharedMemory.CircularBuffer;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace Luster.Common.Tools.FlowChart
{
    [Serializable]
    public class FlowNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Remark { get; set; }
        public string LebleText { get; set; }
        public string FunctionName { get; set; }
        public string FunctionInParameters { get; set; }
        public FlowNodeType NodeType { get; set; }
        public NodeState FlowNodeState { get; set; }= NodeState.Normal;
        public NodeShape FlowNodeShape { get; set; }= NodeShape.Box;
        public Color FillColor { get; set; } = Color.FromArgb(248, 249, 250);
        public Color Color { get; set; } = Color.FromArgb(100, 116, 139);
        public Color LableColor { get; set; } = Color.FromArgb(100, 116, 139);
        public List<FlowNode> PreviousNode { get; set; } = new List<FlowNode>();
        public List<FlowNode> Childrens { get; set; } = new List<FlowNode>();
        public List<FlowNode> NextNode { get; set; } = new List<FlowNode>();
        public List<Edge> Edges = new List<Edge>();
        public int Level { get; set; } = -1;
        public bool IsExpanded { get; set; }

        public FlowNode(LNode node)
        {
            if (node != null)
            {
                Id = node.Key;
                Name = SmartWrap(node.Text,36);
                Description = node.Tips;
                FunctionName = node.FunctionName;
                FunctionInParameters= node.FunctionInParameters;
                FlowNodeState = node.RunStatus==6?NodeState.Skip: NodeState.Normal;

                if (string.IsNullOrEmpty(node.FunctionName))
                {
                    NodeType = FlowNodeType.Node;
                }
                else
                {
                    if (node.FunctionName.ToLower() == "judge")
                    {
                        //条件
                        NodeType = FlowNodeType.Condition;
                        FlowNodeShape = NodeShape.Diamond;
                        Name = SmartWrap(node.Text, 14);

                        FillColor = Color.FromArgb(236, 253, 245);
                        Color = Color.FromArgb(5, 150, 105);
                        LableColor = Color.FromArgb(5, 150, 105);
                    }
                    else if (node.FunctionName.ToLower() == "switch")
                    {
                        //分支
                        NodeType = FlowNodeType.Branch;
                        FillColor = Color.FromArgb(255, 251, 235);
                        Color = Color.FromArgb(180, 83, 9);
                        LableColor = Color.FromArgb(180, 83, 9);
                        //参数
                    }
                    else if (node.FunctionName.ToLower() == "parallel")
                    {
                        //并行
                        NodeType = FlowNodeType.Parallel;
                        FillColor = Color.FromArgb(255, 251, 235);
                        Color = Color.FromArgb(180, 83, 9);
                        LableColor = Color.FromArgb(180, 83, 9);
                    }
                    else if (node.FunctionName.ToLower() == "loop")
                    {
                        //循环
                        NodeType = FlowNodeType.Cycle;
                        FillColor = Color.FromArgb(238, 242, 255);
                        Color = Color.FromArgb(79, 70, 229);
                        LableColor = Color.FromArgb(79, 70, 229);
                        //参数
                    }
                    else if (node.FunctionName.ToLower() == "return")
                    {
                        NodeType = FlowNodeType.Return;
                        FillColor = Color.FromArgb(254, 242, 242);
                        Color = Color.FromArgb(220, 38, 38);
                        LableColor = Color.FromArgb(220, 38, 38);
                    }
                    else if (node.FunctionName.ToLower() == "gotomodule")
                    {
                        //跳转
                        NodeType = FlowNodeType.Goto;
                        FillColor = Color.FromArgb(238, 242, 255);
                        Color = Color.FromArgb(79, 70, 229);
                        LableColor = Color.FromArgb(79, 70, 229);
                    }
                    else if (node.FunctionName.ToLower() == "group"
                        || node.FunctionName.ToLower() == "asyncgroup"
                        || node.FunctionName.ToLower() == "freestation"
                        || node.FunctionName.ToLower() == "refgroup"
                        || node.FunctionName.ToLower().Contains("group")
                        )
                    {
                        //组件
                        NodeType = FlowNodeType.Component;
                    }
                    else
                    {
                        //普通节点
                        NodeType = FlowNodeType.Node;
                    }
                }
            }
        }

        public static string SmartWrap(string text, int maxTotalWidth = 12)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder result = new StringBuilder();
            int currentLineWidth = 0;

            // 1. 将字符串拆分为“单词单元”（连续的英文为一个单元，单个中文为一个单元）
            var units = GetUnits(text);

            foreach (var unit in units)
            {
                int unitWidth = GetVisualWidth(unit);

                // 如果当前行加上这个单元超过了最大宽度
                if (currentLineWidth + unitWidth > maxTotalWidth && currentLineWidth > 0)
                {
                    result.Append("\n");
                    currentLineWidth = 0;
                }

                // 如果单个单元（如超长英文单词）本身就超过了最大宽度，强制截断
                if (unitWidth > maxTotalWidth)
                {
                    foreach (char c in unit)
                    {
                        int cWidth = GetVisualWidth(c.ToString());
                        if (currentLineWidth + cWidth > maxTotalWidth)
                        {
                            result.Append("\n");
                            currentLineWidth = 0;
                        }
                        result.Append(c);
                        currentLineWidth += cWidth;
                    }
                }
                else
                {
                    result.Append(unit);
                    currentLineWidth += unitWidth;
                }
            }

            return result.ToString().Trim();
        }

        // 获取视觉宽度：中文2，英文1
        private static int GetVisualWidth(string s)
        {
            int width = 0;
            foreach (char c in s)
            {
                width += (c > 127) ? 2 : 1;
            }
            return width;
        }

        // 拆分单元：将英文单词保持在一起，中文拆分为单字
        private static List<string> GetUnits(string text)
        {
            List<string> units = new List<string>();
            StringBuilder currentWord = new StringBuilder();

            foreach (char c in text)
            {
                if (c > 127 || char.IsWhiteSpace(c)) // 中文或空格
                {
                    if (currentWord.Length > 0)
                    {
                        units.Add(currentWord.ToString());
                        currentWord.Clear();
                    }
                    units.Add(c.ToString());
                }
                else // 英文或数字
                {
                    currentWord.Append(c);
                }
            }
            if (currentWord.Length > 0) units.Add(currentWord.ToString());

            return units;
        }
    }
}
