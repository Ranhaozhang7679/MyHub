using Aspose.Cells;
using Luster.Common.DataStruct.DataModels;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Luster.Common.Tools.SharedMemory.CircularBuffer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Color = System.Drawing.Color;

namespace Luster.Common.Tools.FlowChart
{
    /// <summary>
    /// 流程图数据
    /// </summary>
    [Serializable]
    public class FlowChartData
    {
        /// <summary>
        /// 流程开始节点
        /// </summary>
        public FlowNode Start { get; set; }

        /// <summary>
        /// 流程结束节点
        /// </summary>
        public FlowNode End { get; set; }

        /// <summary>
        /// 流程图节点集合
        /// </summary>
        public List<FlowNode> FlowNodes { get; set; } = new List<FlowNode>();

        /// <summary>
        /// 流程图边集合
        /// </summary>
        public List<Edge> Edges { get; set; } = new List<Edge>();

        /// <summary>
        /// 数据源根
        /// </summary>
        [NonSerialized]
        public LNode rootLNode = null;

        public void InitFlowChartData(LNode root)
        {
            rootLNode = root;
            //FlowChartData flowChartData = new FlowChartData();
            LNode lEnd = new LNode();
            lEnd.Key = Guid.NewGuid().ToString().ToLower();
            lEnd.Text = "结束";

            //构造图结构
            Start = ChartNodes(root, lEnd);
            //Start.Name = Start.Name + "\n测试描述\n测试表示2";
            Start.FlowNodeShape = NodeShape.Circle;
            Start.FillColor = Color.FromArgb(248, 249, 250);
            Start.Color = Color.FromArgb(100, 116, 139);
            Start.LableColor = Color.FromArgb(100, 116, 139);
            //
            End = GetFlowNode(lEnd);// FlowNodes.Find(e => e.Id == lEnd.Key);
            End.FlowNodeShape = NodeShape.Circle;
            End.LableColor = Color.Black;

            //处理极端情况开始节点没有子节点，
            //那么ChartNodes方法不会把收尾相连，
            //因为ChartNodes方法流程重建靠的是父负责展开子流程
            if (Start.Childrens.Count() == 0)
            {
                Start.NextNode.Add(End);
                End.PreviousNode.Add(Start);
            }

            foreach (FlowNode node in Start.NextNode)
            {
                AddEdges(Start, node);
            }

            //再加一条结束到开始的线
            AddEdges(End, Start,"", Color.FromArgb(79, 70, 229));
        }

        /// <summary>
        /// 树结构到图结构
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        FlowNode ChartNodes(LNode lNode, LNode lNodeNext)
        {
            //当前节点
            FlowNode c = GetFlowNode(lNode);
            //下一节点
            FlowNode cNext = GetFlowNode(lNodeNext);

            #region 节点创建
            //广度优先
            if (lNode.Children.Count() > 0)
            {
                for (int i = 0; i < lNode.Children.Count; i++)
                {
                    var currnt = lNode.Children[i];
                    var last = lNode.Children.Last();

                    LNode next = null;
                    if (c.NodeType == FlowNodeType.Branch)
                    {
                        //分支并行
                        next = lNodeNext;
                    }
                    else if (c.NodeType == FlowNodeType.Parallel)
                    {
                        //并行执行
                        next = lNodeNext;
                    }
                    else if (c.NodeType == FlowNodeType.Condition)
                    {
                        //条件并行
                        next = lNodeNext;
                    }
                    else if (c.NodeType == FlowNodeType.Cycle)
                    {
                        //顺序循环
                        if (lNode.Children.Count == 1)
                        {
                            next = lNodeNext;
                        }
                        else
                        {
                            LNode l = lNode.Children.Last();
                            //for (int j = 0; j < lNode.Children.Count; j++)
                            //{
                            //    #region 正常流程
                            //if (j == 0)
                            //{
                            //    next = lNode.Children[j + 1];
                            //}
                            //else if (lNode.Children[j] == l)
                            //{
                            //    next= lNodeNext;
                            //}
                            //else
                            //{
                            //    next = lNode.Children[j + 1];
                            //}
                            if (i == 0)
                            {
                                next = lNode.Children[i + 1];
                            }
                            else if (lNode.Children[i] == l)
                            {
                                next = lNodeNext;
                            }
                            else
                            {
                                next = lNode.Children[i + 1];
                            }
                            //    #endregion
                            //}
                        }
                    }
                    else
                    {
                        //顺序循环
                        if (lNode.Children.Count == 1)
                        {
                            next = lNodeNext;
                        }
                        else
                        {
                            LNode l = lNode.Children.Last();
                            //for (int j = 0; j < lNode.Children.Count; j++)
                            //{
                            //#region 正常流程
                            if (i == 0)
                            {
                                next = lNode.Children[i + 1];
                            }
                            else if (lNode.Children[i] == l)
                            {
                                next = lNodeNext;
                            }
                            else
                            {
                                next = lNode.Children[i + 1];
                            }
                            //#endregion
                            //}
                        }
                    }

                    FlowNode s = ChartNodes(lNode.Children[i], next);
                    c.Childrens.Add(s);
                }
            }
            #endregion

            #region 重建流程和边集合

            if (c.Childrens.Count() > 0)
            {
                #region 层次流程重建
                //层次流程重建
                if (c.NodeType == FlowNodeType.Branch)
                {
                    //分支(可能含有跳转和终止(不需要处理))
                    Dictionary<string, string> conditions = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(c.FunctionInParameters))
                    {
                        try
                        {
                            string[] inParameters = c.FunctionInParameters.Split(new char[] { ';' }
                            , StringSplitOptions.RemoveEmptyEntries);

                            if (inParameters.Length >= 1)
                            {
                                string condition = inParameters[0].Split('#')[1];
                                JArray array = JArray.Parse(condition);
                                foreach (var item in array)
                                {
                                    string name = item["Name"]?.ToString();
                                    string expression = item["StringEx"]?.ToString();
                                    //int lastDotIndex = expression.LastIndexOf('.');
                                    //if (lastDotIndex != -1)
                                    //{
                                    //    // 从最后一个点之后开始截取
                                    //    expression = expression.Substring(lastDotIndex + 1);
                                    //}

                                    conditions.Add(name, expression);
                                }
                            }
                            //
                            if (inParameters.Length >= 2)
                            {
                                string[] isWait = inParameters[1].Split('#');
                                string isWaitSting = $"{isWait[0]}={isWait[1]}";
                            }
                        }
                        catch (Exception ex)
                        {
                            //忽略异常
                        }
                    }

                    foreach (FlowNode s in c.Childrens)
                    {
                        if (conditions.ContainsKey(s.Name))
                        {
                            s.LebleText = conditions[s.Name];
                        }

                        s.Color = c.Color;
                        s.LableColor = c.LableColor;
                        s.FillColor = c.FillColor;

                        c.NextNode.Add(s);
                        s.PreviousNode.Add(c);

                        if (!s.IsExpanded)
                        {
                            s.NextNode.Add(cNext);
                        }

                        #region 特殊流程
                        if (s.NodeType == FlowNodeType.Goto)
                        {
                            Goto(s);
                        }
                        #endregion
                    }
                }
                else if (c.NodeType == FlowNodeType.Parallel)
                {
                    //并行执行(可能含有跳转和终止(不需要处理))
                    foreach (FlowNode s in c.Childrens)
                    {
                        s.Color = c.Color;
                        s.LableColor = c.LableColor;
                        s.FillColor = c.FillColor;

                        c.NextNode.Add(s);
                        s.PreviousNode.Add(c);

                        if (!s.IsExpanded)
                        {
                            s.NextNode.Add(cNext);
                        }

                        #region 特殊流程

                        if (s.NodeType == FlowNodeType.Goto)
                        {
                            Goto(s);
                        }
                        #endregion
                    }
                }
                else if (c.NodeType == FlowNodeType.Condition)
                {
                    string expression = string.Empty;
                    if (!string.IsNullOrEmpty(c.FunctionInParameters))
                    {
                        try
                        {
                            string[] inParameters = c.FunctionInParameters.Split(new char[] { ';' }
                            , StringSplitOptions.RemoveEmptyEntries);

                            if (inParameters.Length >= 1)
                            {
                                string condition = inParameters[0].Split('#')[1];
                                JObject array = JObject.Parse(condition);
                                expression = array["StringEx"]?.ToString();
                            }
                            //
                            if (inParameters.Length >= 2)
                            {
                                string[] isWait = inParameters[1].Split('#');
                                string isWaitSting = $"{isWait[0]}={isWait[1]}";
                            }
                        }
                        catch (Exception ex)
                        {
                            //忽略异常
                        }
                    }

                    //条件(只会有真和假两个节点)
                    foreach (FlowNode s in c.Childrens)
                    {
                        if (!string.IsNullOrEmpty(expression))
                        {
                            s.LebleText = $"({expression})为{(bool.Parse(s.Name)?"真":"假")}";
                        }
                        s.Color = c.Color;
                        s.LableColor = c.LableColor;
                        s.FillColor = c.FillColor;
                        //
                        c.NextNode.Add(s);
                        s.PreviousNode.Add(c);

                        if (!s.IsExpanded)
                        {
                            s.NextNode.Add(cNext);
                        }
                    }
                }
                else if (c.NodeType == FlowNodeType.Cycle)
                {
                    //顺序循环
                    if (c.Childrens.Count == 1)
                    {
                        FlowNode f = c.Childrens.FirstOrDefault();
                        c.NextNode.Add(f);
                        f.PreviousNode.Add(c);

                        if (!f.IsExpanded)
                        {
                            f.NextNode.Add(cNext);
                            f.NextNode.Add(c);

                            AddCycleEdges(f, c);
                        }

                        //只有一个节点时返回不需要处理

                        #region 特殊流程
                        if (f.NodeType == FlowNodeType.Goto)
                        {
                            Goto(f);
                        }
                        #endregion
                    }
                    else
                    {
                        FlowNode l = c.Childrens.Last();
                        for (int i = 0; i < c.Childrens.Count; i++)
                        {
                            #region 正常流程
                            if (i == 0)
                            {
                                c.NextNode.Add(c.Childrens[i]);
                                c.Childrens[i].PreviousNode.Add(c);

                                if (!c.Childrens[i].IsExpanded)
                                {
                                    c.Childrens[i].NextNode.Add(c.Childrens[i + 1]);
                                }
                            }
                            else if (c.Childrens[i] == l)
                            {
                                c.Childrens[i].PreviousNode.Add(c.Childrens[i - 1]);
                                if (!c.Childrens[i].IsExpanded)
                                {
                                    c.Childrens[i].NextNode.Add(cNext);
                                    c.Childrens[i].NextNode.Add(c);

                                    AddCycleEdges(l, c);
                                }
                            }
                            else
                            {
                                c.Childrens[i].PreviousNode.Add(c.Childrens[i - 1]);

                                if (!c.Childrens[i].IsExpanded)
                                {
                                    c.Childrens[i].NextNode.Add(c.Childrens[i + 1]);
                                }
                            }
                            #endregion

                            #region 特殊流程
                            if (c.Childrens[i].NodeType == FlowNodeType.Return)
                            {
                                //最后一个节点直接忽略条件(因为不管满不满足条件都要执行下一节点)
                                if (c.Childrens[i] != l)
                                {
                                    Return(c.Childrens[i], cNext);
                                }
                            }

                            if (c.Childrens[i].NodeType == FlowNodeType.Goto)
                            {
                                Goto(c.Childrens[i]);
                            }
                            #endregion
                        }
                    }
                }
                else
                {
                    //其他顺序执行
                    if (c.Childrens.Count == 1)
                    {
                        FlowNode f = c.Childrens.First();
                        c.NextNode.Add(f);
                        f.PreviousNode.Add(c);

                        if (!f.IsExpanded)
                        {
                            f.NextNode.Add(cNext);
                        }

                        #region 特殊流程
                        if (f.NodeType == FlowNodeType.Goto)
                        {
                            Goto(f);
                        }
                        #endregion
                    }
                    else
                    {
                        FlowNode l = c.Childrens.Last();
                        for (int i = 0; i < c.Childrens.Count; i++)
                        {
                            #region 正常流程
                            if (i == 0)
                            {
                                c.NextNode.Add(c.Childrens[i]);
                                c.Childrens[i].PreviousNode.Add(c);
                                //
                                if (!c.Childrens[i].IsExpanded)
                                {
                                    c.Childrens[i].NextNode.Add(c.Childrens[i + 1]);
                                }
                            }
                            else if (c.Childrens[i] == l)
                            {
                                c.Childrens[i].PreviousNode.Add(c.Childrens[i - 1]);
                                if (!c.Childrens[i].IsExpanded)
                                {
                                    c.Childrens[i].NextNode.Add(cNext);
                                }
                            }
                            else
                            {
                                c.Childrens[i].PreviousNode.Add(c.Childrens[i - 1]);

                                if (!c.Childrens[i].IsExpanded)
                                {
                                    c.Childrens[i].NextNode.Add(c.Childrens[i + 1]);
                                }
                            }
                            #endregion

                            #region 特殊流程
                            if (c.Childrens[i].NodeType == FlowNodeType.Return)
                            {
                                //最后一个节点直接忽略条件(因为不管满不满足条件都要执行下一节点)
                                if (c.Childrens[i] != l)
                                {
                                    //直接返回到上级节点的下一节点
                                    Return(c.Childrens[i], cNext);
                                }
                            }

                            if (c.Childrens[i].NodeType == FlowNodeType.Goto)
                            {
                                Goto(c.Childrens[i]);
                            }
                            #endregion
                        }
                    }
                }
                #endregion

                #region 层边集合添加
                //下层节点
                foreach (FlowNode cnode in c.Childrens)
                {
                    foreach (FlowNode cnnode in cnode.NextNode)
                    {
                        if (c.NodeType == FlowNodeType.Cycle)
                        {
                            if (c.Childrens.Contains(cnode) && c.Childrens.Contains(cnnode))
                            {
                                cnode.Color = c.Color;
                                cnode.FillColor = c.FillColor;
                                cnnode.LableColor = c.LableColor;

                                cnnode.Color = c.Color;
                                cnnode.FillColor = c.FillColor;
                                cnnode.LableColor = c.LableColor;

                                Edge ad = AddEdges(cnode, cnnode);
                                ad.EdgeColor = c.Color;
                            }
                            else
                            {
                                AddEdges(cnode, cnnode);
                            }
                        }
                        else
                        {
                            AddEdges(cnode, cnnode);
                        }
                    }
                }

                //上层节点
                foreach (FlowNode node in c.NextNode)
                {
                    if (c.NodeType == FlowNodeType.Cycle)
                    {
                        node.Color = c.Color;
                        node.FillColor = c.FillColor;
                        node.LableColor = c.LableColor;

                        Edge ad = AddEdges(c, node);
                        ad.EdgeColor = c.Color;
                    }
                    if (c.NodeType == FlowNodeType.Branch)
                    {
                        Edge ad = AddEdges(c, node);
                        ad.EdgeColor = c.Color;
                    }
                    if (c.NodeType == FlowNodeType.Parallel)
                    {
                        Edge ad = AddEdges(c, node);
                        ad.EdgeColor = c.Color;
                    }
                    if (c.NodeType == FlowNodeType.Condition)
                    {
                        Edge ad = AddEdges(c, node);
                        ad.EdgeColor = c.Color;
                    }
                    else
                    {
                        AddEdges(c, node);
                    }
                }
                #endregion

                c.IsExpanded = true;
            }

            #endregion

            return c;
        }

        public static string GetLastExpression(string input)
        {
            // 1. 基础防护：处理空字符串或纯空格
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // 2. 查找最后一个点的位置 (比 Split 更高效，不产生多余数组)
            int lastDotIndex = input.LastIndexOf('.');

            // 3. 如果没找到点，直接返回原字符串；找到了则截取后面部分
            return lastDotIndex == -1
                ? input
                : input.Substring(lastDotIndex + 1);
        }

        /// <summary>
        /// 添加边集合
        /// </summary>
        /// <param name="Soure"></param>
        /// <param name="Target"></param>
        public Edge AddEdges(FlowNode soure, FlowNode target, string lableText = "")
        {
            Edge edge = Edges.FirstOrDefault(e => e.SoureKey == soure.Id && e.TargetKey == target.Id);
            if (edge == null)
            {
                edge = new Edge();
                edge.SoureKey = soure.Id;
                edge.SoureText = soure.Name;
                edge.TargetKey = target.Id;
                edge.TargetText = target.Name;
                edge.LabelText = string.IsNullOrEmpty(lableText)?target.LebleText: lableText;
                Edges.Add(edge);

                return edge;
            }
            else
            {
                return edge;
            }
        }

        public Edge AddEdges(FlowNode soure, FlowNode target, string lableText, Color edgesColor)
        {
            Edge edge = Edges.FirstOrDefault(e => e.SoureKey == soure.Id && e.TargetKey == target.Id);
            if (edge == null)
            {
                edge = new Edge();
                edge.SoureKey = soure.Id;
                edge.SoureText = soure.Name;
                edge.TargetKey = target.Id;
                edge.TargetText = target.Name;
                edge.LabelText = string.IsNullOrEmpty(lableText) ? target.LebleText : lableText;
                edge.EdgeColor = edgesColor;
                Edges.Add(edge);

                return edge;
            }
            else
            {
                return edge;
            }
        }

        /// <summary>
        /// 将FlowChartData对象保存为JSON文件
        /// </summary>
        /// <param name="path">文件保存路径</param>
        /// <param name="flowChartData">要序列化的FlowChartData对象</param>
        //public static void SaveFlowChartDataJson(string path, FlowChartData flowChartData)
        //{
        //    string json = JsonConvert.SerializeObject(flowChartData, Formatting.Indented);
        //    File.WriteAllText(path, json);
        //}

        public static void SaveBinaryLegacy(string path, FlowChartData data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(fs, data);
            }
        }

        public static FlowChartData LoadBinaryLegacy(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return (FlowChartData)formatter.Deserialize(fs);
            }
        }

        public static void SaveToMsagl(string path, FlowChartData data)
        {
            GViewer viewer = ToGViewer(data);
            viewer.Graph.Write(path);
        }
        public static void SaveToSVG(string path, FlowChartData data)
        {
            GViewer viewer = ToGViewer(data);
            SvgGraphWriter.Write(viewer.Graph, path, null, null, 4);
        }

        public static void SaveToImage(string path, FlowChartData data)
        {
            GViewer viewer =ToGViewer(data);
            SkiaGraphWriter.SaveAsImage(viewer, 1, path);
        }

        private static Microsoft.Msagl.GraphViewerGdi.GViewer ToGViewer(FlowChartData data)
        {
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            foreach (FlowNode flowNode in data.FlowNodes)
            {
                Microsoft.Msagl.Drawing.Node node = new Microsoft.Msagl.Drawing.Node(flowNode.Id);
                node.LabelText = flowNode.Name;
                node.Attr.Shape = (Shape)flowNode.FlowNodeShape;
                node.Attr.FillColor = new Microsoft.Msagl.Drawing.Color(flowNode.FillColor.A
                    , flowNode.FillColor.R
                    , flowNode.FillColor.G
                    , flowNode.FillColor.B);
                node.Attr.Color = new Microsoft.Msagl.Drawing.Color(flowNode.Color.A
                    , flowNode.Color.R
                    , flowNode.Color.G
                    , flowNode.Color.B);

                node.Label.FontColor = new Microsoft.Msagl.Drawing.Color(flowNode.LableColor.A
                    , flowNode.LableColor.R
                    , flowNode.LableColor.G
                    , flowNode.LableColor.B);

                node.Attr.XRadius = 8;
                node.Attr.YRadius = 8;

                if (flowNode.NodeType == FlowNodeType.Condition)
                {
                    node.Attr.LabelMargin = 7;
                }
                else
                {
                    node.Attr.LabelMargin = 7;
                }

                if (flowNode.FlowNodeState == NodeState.Skip)
                {
                    node.Attr.AddStyle(Microsoft.Msagl.Drawing.Style.Dashed);
                }

                graph.AddNode(node);
            }

            foreach (Luster.Common.Tools.FlowChart.Edge edge in data.Edges)
            {
                Microsoft.Msagl.Drawing.Edge ed = graph.AddEdge(edge.SoureKey, edge.TargetKey);
                ed.LabelText = FlowNode.SmartWrap(edge.LabelText, 18);
                ed.Attr.Color = new Microsoft.Msagl.Drawing.Color(edge.EdgeColor.A
                    , edge.EdgeColor.R
                    , edge.EdgeColor.G
                    , edge.EdgeColor.B);
                ed.Label.FontColor = ed.Attr.Color;
                //ed.Attr.LineWidth = 1;
            }

            var sugiyamaSettings = (SugiyamaLayoutSettings)graph.LayoutAlgorithmSettings;
            sugiyamaSettings.NodeSeparation *= 4;
            sugiyamaSettings.LayerSeparation = 40;

            #region 循环
            //Microsoft.Msagl.Drawing.Node statrt = graph.FindNode(flowChartData.Start.Id);
            //Microsoft.Msagl.Drawing.Edge selfEdge = graph.AddEdge(statrt.Id, statrt.Id);
            //可选：设置自环的一些属性
            //selfEdge.LabelText = "Self Loop";
            //selfEdge.Attr.Color = Color.Red;
            #endregion

            #region 分组
            //

            #endregion

            #region 邻居
            //

            #endregion

            Microsoft.Msagl.GraphViewerGdi.GViewer viewer 
                = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            viewer.Graph = graph;
            viewer.ResumeLayout();

            return viewer;
        }

        /// <summary>
        /// 从JSON文件加载FlowChartData对象
        /// </summary>
        /// <param name="path">JSON文件路径</param>
        /// <returns>反序列化后的FlowChartData对象</returns>
        //public FlowChartData GetFlowChartDataFromJson(string path)
        //{
        //    string json = File.ReadAllText(path);
        //    return JsonConvert.DeserializeObject<FlowChartData>(json);
        //}

        private FlowNode GetFlowNode(LNode lNode)
        {
            FlowNode c = FlowNodes.FindAll(s => s.Id == lNode.Key).FirstOrDefault();
            if (c == null)
            {
                c = new FlowNode(lNode);
                FlowNodes.Add(c);
            }

            return c;
        }

        public LNode FindLNodeByKey(string targetKey, LNode node = null)
        {
            if (node == null)
            {
                node = rootLNode;
            }

            if (node.Key == targetKey)
            {
                return node;
            }

            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    LNode foundNode = FindLNodeByKey(targetKey, child);

                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
            }

            return null;
        }

        private void Goto(FlowNode s)
        {
            if (!string.IsNullOrEmpty(s.FunctionInParameters))
            {
                string id = string.Empty;

                try
                {
                    string instring = (s.FunctionInParameters.Split(';')[1]).Split('#')[1];
                    JObject obj = JObject.Parse(instring);
                    id = obj["ID"]?.ToString() ?? "";
                    //string name = obj["Name"]?.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    //
                }

                string stringEx = string.Empty;
                try
                {

                    string expressString = (s.FunctionInParameters.Split(';')[0]).Split('#')[1];
                    JObject express = JObject.Parse(expressString);
                    stringEx = express["StringEx"]?.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    
                }

                if (!string.IsNullOrEmpty(id))
                {
                    //增加一个跳转节点
                    LNode gotoTarget = FindLNodeByKey(id);
                    if (gotoTarget != null)
                    {
                        FlowNode gotoTargetNode = GetFlowNode(gotoTarget);
                        //
                        s.NextNode.Add(gotoTargetNode);

                        if (string.IsNullOrEmpty(stringEx))
                        {
                            stringEx = "跳转";
                        }
                        else
                        {
                            stringEx = $"跳转({stringEx})";
                        }
                        //
                        AddEdges(s, gotoTargetNode, stringEx, Color.FromArgb(79, 70, 229));
                    }
                }
            }
        }

        private void AddCycleEdges(FlowNode l, FlowNode c)
        {
            string stringEx = string.Empty;
            try
            {
                string expressString = (c.FunctionInParameters.Split(';')[0]).Split('#')[1];
                if (!string.IsNullOrEmpty(expressString))
                {
                    stringEx = $"循环({expressString})";
                }
                else
                {
                    stringEx = $"循环";
                }
            }
            catch (Exception ex)
            {

            }

            Edge ad = AddEdges(l, c, stringEx);
            ad.EdgeColor = c.Color;
        }

        private void Return(FlowNode ci, FlowNode cNext)
        {
            //直接返回到上级节点的下一节点
            ci.NextNode.Add(cNext);

            string expressString = (ci.FunctionInParameters.Split(';')[0]).Split('#')[1];
            JObject express = JObject.Parse(expressString);
            string stringEx = express["StringEx"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(stringEx))
            {
                stringEx = "终止";
            }
            else
            {
                stringEx = $"终止({stringEx})";
            }

            AddEdges(ci, cNext, stringEx, Color.FromArgb(220, 38, 38));
        }
    }
}
