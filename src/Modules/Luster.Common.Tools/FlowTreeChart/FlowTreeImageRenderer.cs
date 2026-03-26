using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Luster.Common.Tools.FlowTreeChart
{
    /// <summary>
    /// 流程树渲染
    /// </summary>
    public partial class FlowTreeImageRenderer
    {
        private int NODE_HEIGHT = 60;
        private int NODE_WIDTH = 200;
        private int NODE_MARGIN_X = 50;
        private int NODE_MARGIN_Y = 150;
        private int CORNER_RADIUS = 5;
        private Pen NODE_PEN = new Pen(Color.FromArgb(102, 43, 0), 1.0f);
        //private readonly Pen ARROW_PEN = Pens.Gray;
        Bitmap bitmap = null;

        public void Renderer(LNode root, string outputPath)
        {
            FlowTreeNodeModel<LNode> tree = GetTreeFromLNode(root, null);
            FlowTreeHelpers<LNode>.CalculateNodePositions(tree);
            CalculateControlSize(tree);
            Paint(tree, outputPath);
        }

        #region 流程树工具

        private FlowTreeNodeModel<LNode> GetTreeFromLNode(LNode node, FlowTreeNodeModel<LNode> treeNodeModel)
        {
            FlowTreeNodeModel<LNode> c = new FlowTreeNodeModel<LNode>(node, treeNodeModel);
            if (node.Children.Count() > 0)
            {
                //
                for (int i = 0; i < node.Children.Count; i++)
                {
                    FlowTreeNodeModel<LNode> cn = GetTreeFromLNode(node.Children[i], c);
                    c.Children.Add(cn);
                }
            }

            return c;
        }

        #endregion

        #region 流程树绘制

        private void Paint(FlowTreeNodeModel<LNode> tree, string outputPath)
        {
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);
            DrawNode(tree, graphics);
            //var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TreeLayout.png");
            bitmap.Save(outputPath, ImageFormat.Png);
        }

        private void CalculateControlSize(FlowTreeNodeModel<LNode> tree)
        {
            var treeWidth = tree.Width + 1;
            var treeHeight = tree.Height + 1;

            Size s = new Size(
                Convert.ToInt32((treeWidth * NODE_WIDTH) + ((treeWidth + 1) * NODE_MARGIN_X)),
                (treeHeight * NODE_HEIGHT) + ((treeHeight + 1) * NODE_MARGIN_Y));

            bitmap = new Bitmap(s.Width, s.Height);
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            // 左上角圆角
            path.AddArc(arc, 180, 90);

            // 右上角圆角
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // 右下角圆角
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // 左下角圆角
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void DrawNode(FlowTreeNodeModel<LNode> node, Graphics g)
        {
            // 启用高质量绘图
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 计算节点矩形位置
            var nodeRect = new Rectangle(
                Convert.ToInt32(NODE_MARGIN_X + (node.X * (NODE_WIDTH + NODE_MARGIN_X))),
                NODE_MARGIN_Y + (node.Y * (NODE_HEIGHT + NODE_MARGIN_Y)),
                NODE_WIDTH, NODE_HEIGHT);

            // 创建圆角矩形路径
            GraphicsPath roundedRect = GetRoundedRect(nodeRect, CORNER_RADIUS);

            // 绘制圆角框
            g.DrawPath(NODE_PEN, roundedRect);

            // 填充背景色
            //using (SolidBrush brush = new SolidBrush(Color.White))
            //{
            //    g.FillPath(brush, roundedRect);
            //}

            //渐变色填充
            Color startColor;
            Color endColor;
            //int gradientAngle;
            //startColor = Color.White;
            //endColor = Color.White; 
            startColor = Color.FromArgb(30, 255, 108, 0);
            endColor = Color.FromArgb(66, 255, 108, 0);
            //gradientAngle = 30;
            using (var gradientBrush = new LinearGradientBrush(
                new PointF(nodeRect.X, nodeRect.Y),
                new PointF(nodeRect.X + nodeRect.Width, nodeRect.Y + nodeRect.Height),
                startColor,
                endColor))
            {
                g.FillPath(gradientBrush, roundedRect);
            }
            // 绘制边框
            using (var borderPen = new Pen(Color.FromArgb(102, 43, 0), 1.0f))
            {
                g.DrawPath(borderPen, roundedRect);
            }

            // 绘制节点内容
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(node.Item.Text, SystemFonts.DefaultFont, Brushes.Black, nodeRect, format);
            }

            // 使用贝塞尔曲线连接子节点
            if (node.Children.Count > 0)
            {
                var nodeBottomMiddle = new PointF(nodeRect.X + (nodeRect.Width / 2f), nodeRect.Y + nodeRect.Height);

                foreach (var child in node.Children)
                {
                    var childTopMiddle = new PointF(
                        Convert.ToInt32(NODE_MARGIN_X + (child.X * (NODE_WIDTH + NODE_MARGIN_X)) + (NODE_WIDTH / 2f)),
                        NODE_MARGIN_Y + (child.Y * (NODE_HEIGHT + NODE_MARGIN_Y)));

                    // 计算控制点，使曲线自然弯曲
                    float controlOffset = NODE_MARGIN_Y / 2;
                    var controlPoint1 = new PointF(nodeBottomMiddle.X, nodeBottomMiddle.Y + controlOffset);
                    var controlPoint2 = new PointF(childTopMiddle.X, childTopMiddle.Y - controlOffset);

                    // 绘制贝塞尔曲线
                    using (var path = new GraphicsPath())
                    using (var p = new Pen(Color.FromArgb(102, 43, 0)))
                    {
                        path.AddBezier(nodeBottomMiddle, controlPoint1, controlPoint2, childTopMiddle);
                        g.DrawPath(p, path);
                    }

                    // 计算箭头方向（修正版）
                    PointF[] points = GetBezierPoints(nodeBottomMiddle, controlPoint1, controlPoint2, childTopMiddle, 10);
                    PointF arrowTip = points[points.Length - 1];
                    PointF arrowBase = points[points.Length - 2];

                    // 绘制箭头
                    float angle = (float)Math.Atan2(arrowBase.Y - arrowTip.Y, arrowBase.X - arrowTip.X);
                    PointF[] arrowPoints = new PointF[3];
                    arrowPoints[0] = arrowTip;
                    arrowPoints[1] = new PointF(arrowTip.X + 10 * (float)Math.Cos(angle - Math.PI / 6),
                                                 arrowTip.Y + 10 * (float)Math.Sin(angle - Math.PI / 6));
                    arrowPoints[2] = new PointF(arrowTip.X + 10 * (float)Math.Cos(angle + Math.PI / 6),
                                                 arrowTip.Y + 10 * (float)Math.Sin(angle + Math.PI / 6));
                    using (Brush brush = new SolidBrush(Color.FromArgb(80, 80, 96)))
                    {
                        g.FillPolygon(brush, arrowPoints);
                    }
                }
            }

            // 递归绘制子节点
            foreach (var item in node.Children)
            {
                DrawNode(item, g);
            }
        }

        private PointF[] GetBezierPoints(PointF p0, PointF p1, PointF p2, PointF p3, int steps)
        {
            PointF[] points = new PointF[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                points[i] = new PointF(
                    (float)(Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * p1.X + 3 * (1 - t) * t * t * p2.X + Math.Pow(t, 3) * p3.X),
                    (float)(Math.Pow(1 - t, 3) * p0.Y + 3 * Math.Pow(1 - t, 2) * t * p1.Y + 3 * (1 - t) * t * t * p2.Y + Math.Pow(t, 3) * p3.Y)
                );
            }
            return points;
        }
        #endregion
    }
}
