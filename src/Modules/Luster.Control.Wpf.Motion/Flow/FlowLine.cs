#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowLine
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       FlowLine.cs
* 创建时间:       2022/5/24 11:41:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c6b72e0b-6c83-41a8-85e9-99c584e08546
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 11:41:30
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Luster.Control.Wpf.Motion.Flow
{
    /// <summary>
    /// 箭头方向
    /// </summary>
    public enum ArrawDirection
    {
        Down,
        Right,
        Left
    }

    /// <summary>
    /// 连线
    /// </summary>
    public class FlowLine
    {
        private readonly Brush DefaultLineColor = Brushes.Orange;
        private double ArrowHeight { get; set; } = 4; //
        private readonly Brush ArrowBrush = Brushes.Gray;

        private double LineWidth { get; set; } = 2;   // 线宽度

        /// <summary>
        /// 线的默认颜色
        /// </summary>
        private Brush LineColor;

        /// <summary>
        /// 当前鼠标坐标信息
        /// </summary>
        private Point mousePos;

        /// <summary>
        /// 连接线的起点
        /// </summary>
        public IFlowRender StartItem { get; set; }

        /// <summary>
        /// 连接线的终点
        /// </summary>
        public IFlowRender EndItem { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 线对应的矩形范围
        /// </summary>
        private List<Rect> lineRects;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="startItem"></param>
        public FlowLine(IFlowRender startItem)
        {
            StartItem = startItem;
            LineColor = DefaultLineColor;
            lineRects = new List<Rect>();
        }

        /// <summary>
        /// 设置连接线
        /// </summary>
        /// <param name="endItem"></param>
        public void SetEndItem(IFlowRender endItem)
        {
            EndItem = endItem;
        }

        /// <summary>
        /// 创建连接器
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool CreateNewLink(Point pos)
        {
            return false;
        }

        /// <summary>
        /// 如何判断线是否被点中
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsHitItem(Point point)
        {
            return lineRects.Any(u => u.Contains(point));
        }

        /// <summary>
        /// 是否和矩形框有交互
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool IsHitItem(Rect rect)
        {
            return lineRects.Any(u => u.IntersectsWith(rect));
        }

        /// <summary>
        /// 对象渲染
        /// </summary>
        /// <param name="drawingContext"></param>
        public void Render(DrawingContext drawingContext, Rect rect, Brush lineBrush, double lineThinkness = 1.5)
        {
            LineColor = lineBrush;
            LineWidth = lineThinkness;
            ArrowHeight = lineThinkness * 2;
            if (EndItem == null || StartItem == null)
            {
                return;
            }

            if (StartItem.OuterRect.X == EndItem.OuterRect.X)
            {
                // 1.同一列
                var height = Math.Abs(EndItem.Location.Y - StartItem.Location.Y);
                if (height > rect.Height + 5)
                {
                    // 出现跨行场景
                    DrawCrossRow(drawingContext, StartItem.GetLeftPos(), EndItem.GetLeftPos());
                }
                else
                {
                    DrawDown(drawingContext, StartItem.GetBottomPos(), EndItem.GetTopPos());
                }
            }
            else
            {
                // 同一行
                if (StartItem.OuterRect.Y == EndItem.OuterRect.Y)
                {
                    DrawSameRow(drawingContext);
                }
                else if (StartItem.OuterRect.Y < EndItem.OuterRect.Y)
                {
                    if (StartItem.OuterRect.X < EndItem.OuterRect.X)
                    {
                        DrawRightBottom(drawingContext);
                    }
                    else
                    {
                        DrawLeftBottom(drawingContext);
                    }
                }
                else if (StartItem.OuterRect.Y > EndItem.OuterRect.Y)
                {

                }
                else
                {
                    DrawCrossColumn(drawingContext);
                }
            }
        }

        private void DrawSameRow(DrawingContext drawingContext)
        {
            bool isLeft2Right = StartItem.OuterRect.X < EndItem.OuterRect.X;
            Point sPoint = StartItem.GetRightPos();
            Point ePoint = EndItem.GetLeftPos();
            if (!isLeft2Right)
            {
                sPoint = StartItem.GetLeftPos();
                ePoint = EndItem.GetRightPos();
            }

            // 构建连接线的ROI，用于高亮显示
            lineRects.Clear();
            var linePen = new Pen(LineColor, LineWidth);
            var oneRect = new Rect(new Point(sPoint.X, sPoint.Y - 5), new Point(ePoint.X + 5, ePoint.Y));
            lineRects.Add(oneRect);

            // 绘制连接线
            drawingContext.DrawLine(linePen, sPoint, ePoint);

            DrawArraw(drawingContext, ePoint, isLeft2Right ? ArrawDirection.Right : ArrawDirection.Left);
        }

        /// <summary>
        /// 绘制向下箭头
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="sPoint"></param>
        /// <param name="endPos"></param>
        private void DrawDown(DrawingContext drawingContext, Point sPoint, Point endPos)
        {
            var linePen = new Pen(LineColor, LineWidth);

            lineRects.Clear();

            // 记录当前线的范围
            var rect = new Rect(new Point(sPoint.X - 5, sPoint.Y), new Point(endPos.X + 5, endPos.Y));
            lineRects.Add(rect);

            // 绘制线段
            drawingContext.DrawLine(linePen, sPoint, endPos);

            // 绘制三角箭头
            var arrowPen = new Pen(ArrowBrush, 1);

            var top = new Point(endPos.X, endPos.Y + ArrowHeight);
            var left = new Point(endPos.X - ArrowHeight, endPos.Y - 3);
            var right = new Point(endPos.X + ArrowHeight, left.Y);

            drawingContext.DrawLine(arrowPen, left, top);
            drawingContext.DrawLine(arrowPen, left, right);
            drawingContext.DrawLine(arrowPen, right, top);

            DrawArraw(drawingContext, endPos, ArrawDirection.Down);
        }

        /// <summary>
        /// 同一列，跨行
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="sPoint"></param>
        /// <param name="endPos"></param>
        private void DrawCrossRow(DrawingContext drawingContext, Point sPoint, Point ePoint)
        {
            double centerX = sPoint.X - 10;

            var second = new Point(centerX, sPoint.Y);
            var three = new Point(centerX, ePoint.Y);

            var linePen = new Pen(LineColor, LineWidth);

            // 构建连接线的ROI，用于高亮显示
            lineRects.Clear();
            var oneRect = new Rect(new Point(sPoint.X, sPoint.Y - 5), new Point(second.X, second.Y + 5));
            var twoRect = new Rect(new Point(second.X - 5, second.Y), new Point(three.X + 5, three.Y));
            var threeRect = new Rect(new Point(three.X, three.Y - 5), new Point(ePoint.X, ePoint.Y + 5));
            lineRects.Add(oneRect);
            lineRects.Add(twoRect);
            lineRects.Add(threeRect);

            // 绘制连接线
            drawingContext.DrawLine(linePen, sPoint, second);
            drawingContext.DrawLine(linePen, second, three);
            drawingContext.DrawLine(linePen, three, ePoint);

            // 绘制三角箭头
            DrawArraw(drawingContext, ePoint, ArrawDirection.Right);
        }

        /// <summary>
        /// 即跨行也跨列
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="sPoint"></param>
        /// <param name="endPos"></param>
        private void DrawCrossColumn1(DrawingContext drawingContext)
        {
            Point sPoint = StartItem.GetRightPos();
            Point ePoint = EndItem.GetLeftPos();
            bool isEndLeft = StartItem.OuterRect.X > EndItem.OuterRect.X;
            if (isEndLeft)
            {
                sPoint = StartItem.GetLeftPos();
                ePoint = EndItem.GetRightPos();
            }

            // 构建连接线的ROI，用于高亮显示
            lineRects.Clear();
            double centerX = sPoint.X + (ePoint.X - sPoint.X) / 2;
            var second = new Point(centerX, sPoint.Y);
            var three = new Point(centerX, ePoint.Y);
            var linePen = new Pen(LineColor, LineWidth);
            var oneRect = new Rect(new Point(sPoint.X, sPoint.Y - 5), new Point(second.X, second.Y + 5));
            var twoRect = new Rect(new Point(second.X - 5, second.Y), new Point(three.X + 5, three.Y));
            var threeRect = new Rect(new Point(three.X, three.Y - 5), new Point(ePoint.X, ePoint.Y + 5));
            lineRects.Add(oneRect);
            lineRects.Add(twoRect);
            lineRects.Add(threeRect);

            // 绘制连接线
            drawingContext.DrawLine(linePen, sPoint, second);
            drawingContext.DrawLine(linePen, second, three);
            drawingContext.DrawLine(linePen, three, ePoint);

            if (isEndLeft)
            {
                DrawArraw(drawingContext, ePoint, ArrawDirection.Left);
            }
            else
            {
                DrawArraw(drawingContext, ePoint, ArrawDirection.Right);
            }
        }

        /// <summary>
        /// 右下
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawRightBottom(DrawingContext drawingContext)
        {
            Point sPoint = StartItem.GetRightPos();
            Point ePoint = EndItem.GetTopPos();

            // 构建连接线的ROI，用于高亮显示
            lineRects.Clear();
            var second = new Point(ePoint.X, sPoint.Y);
            var linePen = new Pen(LineColor, LineWidth);
            var oneRect = new Rect(new Point(sPoint.X, sPoint.Y - 5), new Point(second.X, second.Y + 5));
            var twoRect = new Rect(new Point(second.X - 5, second.Y), new Point(ePoint.X + 5, ePoint.Y));
            lineRects.Add(oneRect);
            lineRects.Add(twoRect);

            // 绘制连接线
            drawingContext.DrawLine(linePen, sPoint, second);
            drawingContext.DrawLine(linePen, second, ePoint);

            DrawArraw(drawingContext, ePoint, ArrawDirection.Down);
        }

        /// <summary>
        /// 左下
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawLeftBottom(DrawingContext drawingContext)
        {
            Point sPoint = StartItem.GetBottomPos();
            Point ePoint = EndItem.GetRightPos();

            // 构建连接线的ROI，用于高亮显示
            lineRects.Clear();
            var second = new Point(sPoint.X, ePoint.Y);
            var linePen = new Pen(LineColor, LineWidth);
            var oneRect = new Rect(new Point(sPoint.X, sPoint.Y - 5), new Point(second.X, second.Y + 5));
            var twoRect = new Rect(new Point(second.X - 5, second.Y), new Point(ePoint.X + 5, ePoint.Y));
            lineRects.Add(oneRect);
            lineRects.Add(twoRect);

            // 绘制连接线
            drawingContext.DrawLine(linePen, sPoint, second);
            drawingContext.DrawLine(linePen, second, ePoint);

            DrawArraw(drawingContext, ePoint, ArrawDirection.Left);
        }

        private void DrawCrossColumn(DrawingContext drawingContext)
        {
            Point sPoint = StartItem.GetRightPos();
            Point ePoint = EndItem.GetTopPos();
            bool isEndLeft = StartItem.OuterRect.X > EndItem.OuterRect.X;
            if (isEndLeft)
            {
                sPoint = StartItem.GetBottomPos();
                ePoint = EndItem.GetRightPos();
            }

            // 构建连接线的ROI，用于高亮显示
            lineRects.Clear();
            double centerX = sPoint.X + (ePoint.X - sPoint.X) / 2;
            var second = new Point(centerX, sPoint.Y);
            var three = new Point(centerX, ePoint.Y);
            var linePen = new Pen(LineColor, LineWidth);
            var oneRect = new Rect(new Point(sPoint.X, sPoint.Y - 5), new Point(second.X, second.Y + 5));
            var twoRect = new Rect(new Point(second.X - 5, second.Y), new Point(three.X + 5, three.Y));
            var threeRect = new Rect(new Point(three.X, three.Y - 5), new Point(ePoint.X, ePoint.Y + 5));
            lineRects.Add(oneRect);
            lineRects.Add(twoRect);
            lineRects.Add(threeRect);

            // 绘制连接线
            drawingContext.DrawLine(linePen, sPoint, second);
            drawingContext.DrawLine(linePen, second, three);
            drawingContext.DrawLine(linePen, three, ePoint);

            if (isEndLeft)
            {
                DrawArraw(drawingContext, ePoint, ArrawDirection.Left);
            }
            else
            {
                DrawArraw(drawingContext, ePoint, ArrawDirection.Right);
            }
        }


        /// <summary>
        /// 绘制箭头
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isDown">如果向下</param>
        private void DrawArraw(DrawingContext drawingContext, Point pos, ArrawDirection direction)
        {
            // 绘制三角箭头
            var arrowPen = new Pen(ArrowBrush, 1);

            Point left = new Point();
            Point arraw = new Point();
            Point right = new Point();
            switch (direction)
            {
                case ArrawDirection.Down:
                    left = new Point(pos.X - ArrowHeight, pos.Y - ArrowHeight);
                    arraw = new Point(pos.X, pos.Y + ArrowHeight);
                    right = new Point(pos.X + ArrowHeight, left.Y);

                    break;
                case ArrawDirection.Left:

                    left = new Point(pos.X + ArrowHeight, pos.Y - ArrowHeight);
                    arraw = new Point(pos.X - ArrowHeight, pos.Y);
                    right = new Point(left.X, arraw.Y + ArrowHeight);
                    break;
                case ArrawDirection.Right:

                    left = new Point(pos.X - ArrowHeight, pos.Y - ArrowHeight);
                    arraw = new Point(pos.X + ArrowHeight, pos.Y);
                    right = new Point(left.X, arraw.Y + ArrowHeight);

                    break;
            }


            var g = new StreamGeometry();
            using (StreamGeometryContext context = g.Open())
            {
                context.BeginFigure(left, true, true);
                context.LineTo(arraw, true, false);
                context.LineTo(right, true, false);
                context.LineTo(left, true, false);
            }

            drawingContext.DrawGeometry(LineColor, arrowPen, g);
        }

        /// <summary>
        /// 选中连接线
        /// </summary>
        /// <param name="isSelected">是否选中</param>
        public void Select(Rect rect)
        {
            if (lineRects == null) return;
            IsSelected = lineRects.Any(u => u.IntersectsWith(rect));
            if (IsSelected)
            {
                LineColor = Brushes.Blue;
            }
            else
            {
                LineColor = DefaultLineColor;
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="ePos"></param>
        public void SetLink(Point pos)
        {
            mousePos = pos;
        }

        /// <summary>
        /// 通过起点和终点来绘制连接线
        /// </summary>
        /// <param name="sPoint">开始节点</param>
        /// <param name="endPos">结束节点</param>
        public void DrawConnector(DrawingContext drawingContext, Point endPos)
        {
            var startPos = StartItem.GetBottomPos();

            var offsetY = (endPos.Y - startPos.Y) / 2;

            var second = new Point(startPos.X, startPos.Y + offsetY);
            var threed = new Point(endPos.X, endPos.Y - offsetY);

            var linePen = new Pen(LineColor, LineWidth);
            drawingContext.DrawLine(linePen, startPos, second);
            drawingContext.DrawLine(linePen, second, threed);
            drawingContext.DrawLine(linePen, threed, endPos);

            // 绘制三角箭头
            DrawArraw(drawingContext, endPos, ArrawDirection.Down);
        }
    }
}