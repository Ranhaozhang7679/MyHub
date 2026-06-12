#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.FlowEditor
* 文 件 名:       FlowEditor.cs
* 创建时间:       2022/5/24 11:38:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      11181dc9-53e5-40c0-8819-ca8c2f170b56
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 11:38:43
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Tools.Extension;
using Luster.Control.Wpf.Motion.Flow;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Luster.Control.Wpf.Motion.Flow
{

    /// <summary>
    /// 
    /// </summary>
    public class FlowEditor : ItemsControl
    {
        /// <summary>
        /// 缩放比例
        /// </summary>
        private double GlobalScaleDelta = 1.0;

        #region 矩形框

        /// <summary>
        /// 选择矩形
        /// </summary>
        private Rect SelectRect { get; set; } = new Rect(0, 0, 0, 0);

        /// <summary>
        /// 矩形边框颜色
        /// </summary>
        private Brush RectBorderBrush = Brushes.Blue;

        #endregion

        #region 连接线
        private Rect ConnectingRect { get; set; } = new Rect(0, 0, 0, 0);

        #endregion

        #region 全局变量
        /// <summary>
        /// 交互状态
        /// </summary>
        private MouseState MouseState { get; set; } = MouseState.None;

        /// <summary>
        /// 网格的尺寸
        /// </summary>
        public int gridSize = 40;

        /// <summary>
        /// 最后一次点击位置
        /// </summary>
        private Point lastPos = new Point(0, 0);

        /// <summary>
        /// 背景色
        /// </summary>
        private SolidColorBrush BackgroudColor = new SolidColorBrush(Color.FromArgb(240, (byte)255, (byte)255, (byte)255));

        /// <summary>
        /// 网格线颜色
        /// </summary>
        private SolidColorBrush GridLineColor = new SolidColorBrush(Color.FromArgb(64, (byte)128, (byte)128, (byte)128));

        /// <summary>
        /// 当前选择的对象
        /// </summary>
        private IFlowRender CurrentItem { get; set; }

        // 要移动的位置
        private int moveIndex = -1;

        /// <summary>
        /// 数据对象
        /// </summary>
        private List<IFlowRender> flowItems;

        /// <summary>
        /// 移动的时候，显示的矩形
        /// </summary>
        private Rect moveRect = new Rect(0, 0, 0, 0);

        /// <summary>
        /// 消息框
        /// </summary>
        private ToolTip messageTip;

        /// <summary>
        /// 连接线
        /// </summary>
        private FlowLine _currentLink;
        #endregion

        #region 计算渲染格
        /// <summary>
        /// 距离左边框的值
        /// </summary>
        private int Left = 0;

        /// <summary>
        /// 距离上边框的值
        /// </summary>
        private int Top = 0;
        #endregion

        #region 连接线
        /// <summary>
        /// 基于数据流向的连接线
        /// </summary>
        private List<FlowLine> dataLines;

        /// <summary>
        /// 边界框
        /// </summary>
        private List<RowColumn> outRects;
        #endregion

        #region 运行动画
        Rectangle rect = new Rectangle();
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public FlowEditor()
        {
            // 将布局设置为Canvas
            FrameworkElementFactory factoryPanel = new FrameworkElementFactory(typeof(Canvas));
            factoryPanel.SetValue(Canvas.IsItemsHostProperty, true);
            factoryPanel.Name = "canvas";
            ItemsPanelTemplate template = new ItemsPanelTemplate();
            template.VisualTree = factoryPanel;

            this.ItemsPanel = template;
            this.MinWidth = ColumnWidth + 20;


            dataLines = new List<FlowLine>();
            flowItems = new List<IFlowRender>();
            outRects = new List<RowColumn>();

            // 消息
            messageTip = new ToolTip();
            messageTip.PlacementTarget = this;
            ToolTipService.SetShowDuration(messageTip, 1000); // 显示1s

            this.AllowDrop = true;
        }


        #region 通用方法
        /// <summary>
        /// 将ItemSource转换为List
        /// </summary>
        /// <returns></returns>
        private List<IFlowRender> GetFlowItems()
        {
            List<IFlowRender> items = new List<IFlowRender>();
            if (ItemsSource == null) return items;

            items?.Clear();

            foreach (var item in ItemsSource)
            {
                if (item is IFlowRender flowItem)
                {
                    flowItem.Dispatcher = this.Dispatcher;
                    flowItem.InvalidateEvent -= FlowItem_InvalidateEvent;
                    flowItem.InvalidateEvent += FlowItem_InvalidateEvent;

                    items.Add(flowItem);
                }
            }

            return items;
        }

        private static object lockInvalidate = new object();
        private static int lockRener = 0;

        /// <summary>
        /// 注册子模块重绘事件
        /// </summary>
        private void FlowItem_InvalidateEvent()
        {
            Dispatcher.InvokeAsync(() =>
            {
                this.InvalidateVisual();
            }, DispatcherPriority.Render);



            //System.Threading.Monitor.Enter(lockInvalidate);
            //if (lockRener > 0) return;

            //lock (lockInvalidate)
            //{
            //    lockRener++;

            //    Thread.Sleep(150);
            //    lockRener--;
            //}

            //System.Threading.Monitor.Exit(lockInvalidate);
        }

        /// <summary>
        /// 绘制网格线
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawGridLine(DrawingContext drawingContext)
        {
            outRects.Clear();

            int left = (int)Padding.Left;
            int top = (int)Padding.Top;

            double columnW = ColumnWidth * GlobalScaleDelta;
            double rowH = RowHeight * GlobalScaleDelta;
            double lineWidth = ColumnNum * columnW;
            double lineHeight = RowNum * rowH;

            int thickness = 1;

            if (HAlignment == HorizontalAlignment.Center)
            {
                left = (int)(RenderSize.Width - lineWidth) / 2;
            }

            for (int x = 0; x <= ColumnNum; x++)
            {
                var startX = x * columnW + left;
                if (IsShowGridLine)
                {
                    drawingContext.DrawLine(new Pen(GridLineColor, thickness), new Point(startX, Padding.Top), new Point(startX, lineHeight + Padding.Top));
                }
            }

            for (int y = 0; y <= RowNum; y++)
            {
                var startY = y * rowH + top;
                if (IsShowGridLine)
                {
                    drawingContext.DrawLine(new Pen(GridLineColor, thickness), new Point(left, startY), new Point(lineWidth + left, startY));
                }
            }

            // 构造目前的矩形框
            for (int i = 0; i < RowNum; i++)
            {
                for (int j = 0; j < ColumnNum; j++)
                {
                    var startX = j * columnW + left;
                    var startY = i * rowH + top;
                    var rect = new Rect() { X = startX, Y = startY, Width = columnW, Height = rowH };
                    outRects.Add(new RowColumn() { Row = i, Column = j, Rect = rect });
                }
            }

            // 设置第一个矩形框距离左上角的值
            Left = left;
            Top = top;
        }

        /// <summary>
        /// 绘制FlowItem
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawItems(DrawingContext drawingContext)
        {
            double startX = Left;
            double startY = Top;
            int count = flowItems.Count;
            IFlowRender startItem = null;
            double columnW = ColumnWidth * GlobalScaleDelta;
            double rowH = RowHeight * GlobalScaleDelta;

            if (count == 0) return;

            // 行列遍历
            for (int col = 0; col < ColumnNum; col++)
            {
                for (int row = 0; row < RowNum; row++)
                {
                    int index = RowNum * col + row;
                    if (count <= index) continue;

                    var item = flowItems[index];

                    var outerRect = new Rect(startX, startY, columnW, rowH);

                    // 设置尺寸
                    item.ScaleBy(GlobalScaleDelta);

                    item.Render(drawingContext, outerRect);

                    // 如果有起始绘制连接示意线
                    if (item.PrevRender != null)
                    {
                        FlowLine line = new FlowLine(startItem);
                        line.SetEndItem(item);
                        line.Render(drawingContext, outerRect, LineBrush, LineThinkness);
                    }

                    // 默认要更新一下
                    startItem = item;

                    startY += rowH;
                }

                startX += columnW;
                startY = Top;
            }
        }

        /// <summary>
        /// 自动计算行
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void AutoRowColumn(ref int row, ref int col)
        {
            if (row >= RowNum)
            {
                col++;

                // 超过显示范围了
                if (col >= ColumnNum)
                {
                    return;
                }

                // 在当前列找最后一行数据
                int tmpColumn = col;
                var lastItem = flowItems.LastOrDefault(u => u.Column == tmpColumn);

                if (lastItem != null)
                {
                    var last = lastItem.GetRowColumn();
                    row = last.Item1 + 1;
                    if (row >= RowNum)
                    {
                        AutoRowColumn(ref row, ref col);
                    }
                }
                else
                {
                    row = 0;
                }
            }
            else
            {
                col = 0;
            }
        }

        /// <summary>
        /// 绘制工站
        /// </summary>
        private void DrawStation(DrawingContext drawingContext)
        {
            double startX = Left;
            double startY = Top;

            double columnW = ColumnWidth * GlobalScaleDelta;
            double rowH = RowHeight * GlobalScaleDelta;

            foreach (var item in flowItems)
            {
                var rowCol = item.GetRowColumn();
                int row = rowCol.Item1;
                int col = rowCol.Item2;
                if (row >= RowNum && col == 0)
                {
                    col = row / RowNum;
                    row = row % RowNum;
                }
                else if (row == -1)
                {
                    // 未赋值的场景
                    var maxRow = flowItems.Max(u => u.GetRowColumn().Item1);
                    row = maxRow + 1;
                    col = 0;
                    AutoRowColumn(ref row, ref col);

                    item.SetRowColumn(row, col);
                }


                startX = col * columnW + Left;
                startY = row * rowH + Top;
                var outerRect = new Rect(startX, startY, columnW, rowH);

                // 设置缩放比例
                item.ScaleBy(GlobalScaleDelta);

                // 渲染Item
                item.Render(drawingContext, outerRect);

            }
        }

        /// <summary>
        /// 绘制数据线
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawDataline(DrawingContext drawingContext)
        {
            if (dataLines == null || dataLines.Count == 0) return;
            foreach (var item in dataLines)
            {
                item.Render(drawingContext, new Rect(0, 0, ColumnWidth, RowHeight), LineBrush, LineThinkness);
            }
        }

        /// <summary>
        /// 绘制比例尺
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawScaleRuler(DrawingContext drawingContext)
        {
            var width = 80;
            var height = 20;
            var rect = new Rect(RenderSize.Width - width, RenderSize.Height - height - 10, width, height);

            // 构建几何图形
            // 1.构建左边的三个点
            var left1 = new Point(rect.X, rect.Y + 5);
            var left2 = new Point(left1.X, left1.Y + 5);
            var left3 = new Point(left1.X, left2.Y + 5);

            // 2.构建右边的三个点
            var right1 = new Point(rect.Right - 10, left1.Y);
            var right2 = new Point(right1.X, left2.Y);
            var right3 = new Point(right1.X, left3.Y);

            StreamGeometry g = new StreamGeometry();
            using (StreamGeometryContext gContext = g.Open())
            {
                gContext.BeginFigure(left1, false, false);
                gContext.LineTo(left3, true, true);

                gContext.BeginFigure(left2, true, false);
                gContext.LineTo(right2, true, true);

                gContext.BeginFigure(right1, false, false);
                gContext.LineTo(right3, true, false);
            }

            drawingContext.DrawGeometry(null, new Pen(Brushes.Black, 3), g);
            var textFormat = MeasureText($"{GlobalScaleDelta * 100} %", 15);
            var rulerWidth = right2.X - left2.X;
            drawingContext.DrawText(textFormat, new Point(left1.X + (rulerWidth - textFormat.Width) / 2, rect.Y - 10));


            var ft = new FormattedText("Luster", CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight, new Typeface("Tahoma"), 18, Brushes.Red);
            var point = new Point(rect.X + (rect.Width - ft.Width) / 2 - 5, rect.Bottom - 10);    // 注意这里的计算 很容易迷糊
            drawingContext.DrawText(ft, point);
        }

        /// <summary>
        /// 绘制比例尺
        /// </summary>
        /// <param name="drawingContext"></param>
        private void DrawStatusDesc(DrawingContext drawingContext)
        {
            // 默认 运行中 完成 报警 错误 忽略
            var width = 50;
            var height = 25;

            var startX = (RenderSize.Width - width * 5) / 2;
            var startY = (RenderSize.Height - height);

            BuildStatusCircle(drawingContext, AbsFlowItem.DefaultBrush, Luster.Motion.Assests.Langs.LangProvider.GetLang("Default"), new Point(startX, startY));
            BuildStatusCircle(drawingContext, AbsFlowItem.RunningBrush, Luster.Motion.Assests.Langs.LangProvider.GetLang("Running"), new Point(startX + width, startY));
            BuildStatusCircle(drawingContext, AbsFlowItem.SuccessBrush, Luster.Motion.Assests.Langs.LangProvider.GetLang("Done"), new Point(startX + width * 2, startY));
            BuildStatusCircle(drawingContext, AbsFlowItem.TimeoutBrush, Luster.Motion.Assests.Langs.LangProvider.GetLang("Alarm"), new Point(startX + width * 3, startY));
            BuildStatusCircle(drawingContext, AbsFlowItem.FailBrush, Luster.Motion.Assests.Langs.LangProvider.GetLang("Wrong"), new Point(startX + width * 4, startY));
            BuildStatusCircle(drawingContext, AbsFlowItem.SkipBrush, Luster.Motion.Assests.Langs.LangProvider.GetLang("Ignore"), new Point(startX + width * 5, startY));
        }

        private void BuildStatusCircle(DrawingContext drawingContext, Brush brush, string status, Point p)
        {
            drawingContext.DrawEllipse(brush, null, p, 8, 8);
            var textFormat = MeasureText(status, 11);
            var fontX = p.X - (textFormat.Width - 16) / 2 - 8;
            drawingContext.DrawText(textFormat, new Point(fontX, p.Y + 10));
        }
        /// <summary>
        /// 文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontFamily"></param>
        /// <returns></returns>
        [Obsolete]
        private FormattedText MeasureText(string text, double emsize = 20)
        {
            var formattedText = new FormattedText(
               text,
               System.Globalization.CultureInfo.InvariantCulture,
               FlowDirection.LeftToRight,
               new Typeface("微软雅黑"),
               emsize,
               Brushes.Black
               )
            { Trimming = TextTrimming.CharacterEllipsis };

            return formattedText;
        }
        #endregion

        #region 数据源变换事件
        /// <summary>
        /// 数据源发生了变换
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            // 对原来对象释放
            if (oldValue != null)
            {
                foreach (var item in oldValue)
                {
                    if (item is IFlowRender flowItem)
                    {
                        flowItem.InvalidateEvent -= FlowItem_InvalidateEvent;
                        flowItem.StopStory();
                    }
                }
            }

            flowItems = GetFlowItems();

            // 初始化数据连接线
            InitDataLines();

            this.InvalidateVisual();
            this.InvalidateMeasure();

            moveRect = new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// 构建连接线
        /// </summary>
        private void InitDataLines()
        {
            dataLines.Clear();
            foreach (var item in flowItems)
            {
                foreach (var prevID in item.GetPrevItems())
                {
                    var startItem = flowItems.FirstOrDefault(u => u.ID == prevID);
                    if (startItem != null)
                    {
                        FlowLine line = new FlowLine(startItem);
                        line.SetEndItem(item);
                        dataLines.Add(line);
                    }
                }
            }
        }

        /// <summary>
        /// 返回矩形框对象
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Rectangle();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (item is IFlowRender flowItem && element is Rectangle rect)
            {
                flowItem.MyRectangle = rect;
                flowItem.OffsetX = (int)Padding.Left;
                flowItem.OffsetY = (int)Padding.Top;
            }
        }
        #endregion

        #region 鼠标交互
        /// <summary>
        /// 按比例缩放
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control) return;

            int value = e.Delta;

            double scaleDelta = 0;

            if (value > 0)
            {
                scaleDelta = 0.1;
            }
            else
            {
                scaleDelta = -0.1;
            }

            double v = GlobalScaleDelta + scaleDelta;

            if (v < 0.1 || v > 2)
            {
                return;
            }

            GlobalScaleDelta = Math.Round(v, 2);


            CalcRowAndColNums(RenderSize.Width, RenderSize.Height);

            gridSize += (int)(scaleDelta * 50);

            if (gridSize < 5)
                gridSize = 5;

            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    if (item is IFlowRender render)
                    {
                        render.ScaleBy(GlobalScaleDelta);
                    }
                }
            }

            // 需要全部重绘
            this.InvalidateVisual();
        }

        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="mouseButtonEventArgs"></param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // 初始化矩形
            if (e.ChangedButton == MouseButton.Left)
                moveRect = new Rect();
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                var mousePos = e.GetPosition(this);

                var isHitItem = flowItems.FirstOrDefault(u => u.IsHitItem(mousePos));
                if (isHitItem != null)
                {
                    // 支持ctrl多选功能
                    var isCtrlKey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                    if (e.ChangedButton == MouseButton.Left && !isCtrlKey)
                    {
                        flowItems.ForEach(u => u.Select(false));
                    }

                    if (e.ChangedButton == MouseButton.Right)
                    {
                        // 是否已经选择
                        var existSelected = flowItems.Any(u => u.IsSelected && u == isHitItem);
                        if (!existSelected)
                        {
                            flowItems.ForEach(u => u.Select(false));
                        }
                    }

                    MouseState = MouseState.Selected;

                    // 选中了Item
                    isHitItem.Select();
                    CurrentItem = isHitItem;

                    // 实现更新选中状态
                    this.InvalidateVisual();

                    OnItemSelected(e, CurrentItem);
                }
                else
                {
                    // 选中连线
                    if (IsDataFlow)
                    {
                        dataLines.ForEach(u => u.Select(new Rect(mousePos.X, mousePos.Y, 1, 1)));
                        var selectLine = dataLines.FirstOrDefault(u => u.IsSelected);
                        if (selectLine != null)
                        {
                            OnLineSelected(e, selectLine);
                        }
                    }

                    if (e.ChangedButton == MouseButton.Left)
                    {
                        flowItems.ForEach(u => u.Select(false));

                        // 检查是否选中line
                        var createLink = flowItems.FirstOrDefault(u => u.CreateNewLink(mousePos));
                        if (createLink != null && IsDataFlow)
                        {
                            MouseState = MouseState.CreateLink;
                            _currentLink = new FlowLine(createLink);
                        }
                        else
                        {
                            // 选中了场景
                            MouseState = MouseState.DragRect;
                            flowItems.ForEach(u => (u.Tag as IMotionModule).IsSelected = false);
                            // 防止鼠标移除控件外，无法进行MouseUp事件响应
                            ((UIElement)e.Source).CaptureMouse();
                        }
                    }
                }

                if (e.ChangedButton == MouseButton.Left)
                {
                    Cursor = Cursors.Hand;
                    lastPos.X = Mouse.GetPosition(this).X;
                    lastPos.Y = Mouse.GetPosition(this).Y;
                }
            }
        }

        /// <summary>
        /// 通过位置获取数据
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        private IFlowRender GetItemByPos(Point mousePos)
        {
            return flowItems.FirstOrDefault(u => u.IsHitItem(mousePos));
        }

        /// <summary>
        /// 消息提醒
        /// </summary>
        /// <param name="mousePos"></param>
        private void ShowMessage(Point mousePos)
        {
            var hitItem = GetItemByPos(mousePos);
            if (hitItem != null)
            {
                string sMsg = hitItem.GetTipMessage();
                if (!string.IsNullOrWhiteSpace(sMsg))
                {
                    messageTip.Content = sMsg;
                    messageTip.IsOpen = true;
                }
            }

            else
            {
                messageTip.Content = "";
                messageTip.IsOpen = false;
            }
        }

        /// <summary>
        /// 鼠标移动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            var mousePos = e.GetPosition(this);
            Cursor = Cursors.Arrow;

            double x = mousePos.X;
            double y = mousePos.Y;
            double offsetX = x - lastPos.X;
            double offsetY = y - lastPos.Y;

            // 触发鼠标移动的动作
            switch (MouseState)
            {
                case MouseState.Selected:
                    if (CurrentItem != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        // 有位置变化的时候，将选中状态调整为Move状态
                        if (moveRect.Width == 0 && (Math.Abs(offsetX) > 5 || Math.Abs(offsetY) > 5))
                        {
                            // 移动的矩形
                            moveRect = CurrentItem.Rect;
                            MouseState = MouseState.Move;
                        }
                    }

                    break;
                case MouseState.Move:
                    if (CurrentItem != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        if (!IsDataFlow)
                        {
                            // 任务流，默认只能在已有数据范围内进行移动
                            var hitItem = flowItems.FirstOrDefault(u => u != CurrentItem && !u.IsSelected && u.OuterRect.Contains(moveRect));
                            if (hitItem != null)
                            {
                                moveIndex = hitItem.Index;
                            }
                            else
                            {
                                moveIndex = -1;
                            }
                        }

                        // 更新移动位置
                        moveRect.Offset(offsetX, offsetY);

                        lastPos = new Point(x, y);
                    }
                    break;

                case MouseState.DragRect:
                    if (e.LeftButton == MouseButtonState.Pressed && lastPos.X != 0 && lastPos.Y != 0)
                    {
                        double minX = x > lastPos.X ? lastPos.X : x;
                        double minY = y > lastPos.Y ? lastPos.Y : y;
                        double maxX = x > lastPos.X ? x : lastPos.X;
                        double maxY = y > lastPos.Y ? y : lastPos.Y;
                        double width = maxX - minX;
                        double height = maxY - minY;
                        if (width <= 0) width = 1;
                        if (height <= 0) height = 1;

                        SelectRect = new Rect(minX, minY, width, height);

                        flowItems.ForEach(u =>
                        {
                            bool s = u.IsHitItem(SelectRect);
                            u.Select(s);
                            (u.Tag as IMotionModule).IsSelected = s;
                        });

                        if (IsDataFlow)
                        {
                            dataLines.ForEach(u => u.Select(SelectRect));
                        }
                    }

                    break;
                case MouseState.CreateLink:

                    if (_currentLink != null)
                    {
                        _currentLink.SetLink(mousePos);
                        lastPos = new Point(x, y);
                    }

                    break;
                default:
                    // 鼠标放到FlowItem上
                    if (lastPos.X == 0 && lastPos.Y == 0)
                    {
                        flowItems.ForEach(u => u.Hover(mousePos));

                        ShowMessage(mousePos);
                        var item = GetItemByPos(mousePos);
                        if (item != null)
                        {
                            Cursor = Cursors.Hand;
                        }
                    }

                    break;
            }

            this.InvalidateVisual();
        }

        /// <summary>
        /// 鼠标抬起
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            // 指针显示还原
            Cursor = Cursors.Arrow;

            // FlowItem 位置进行移动
            if (CurrentItem != null && MouseState == MouseState.Move)
            {
                if (IsDataFlow)
                {
                    var rect = outRects.FirstOrDefault(u => u.Contains(moveRect));
                    if (rect.Rect.Width > 0)
                    {
                        CurrentItem.SetRowColumn(rect.Row, rect.Column);
                        OnItemsMoved(new List<IFlowRender>() { CurrentItem }, -1);
                    }
                }
                else
                {
                    if (moveIndex != -1)
                    {
                        var curIndex = flowItems.IndexOf(CurrentItem);

                        // 获取当前选择的Items
                        var selectItems = flowItems.Where(u => u.IsSelected).ToList();

                        // 位置发生了变化
                        if (curIndex < moveIndex)
                        {
                            OnItemsMoved(selectItems, moveIndex);
                        }
                        else if (curIndex > moveIndex)
                        {
                            // 从后往前移动
                            OnItemsMoved(selectItems, moveIndex, false);
                        }

                        // 取消选择状态
                        flowItems.ForEach(u => u.Select(false));
                    }
                }
            }
            else if (_currentLink != null)
            {
                // 通过终点找到对应的lastItem
                var endItem = flowItems.FirstOrDefault(u => u.IsHitItem(lastPos));
                if (endItem != null)
                {
                    if (_currentLink.StartItem.AddNextItem(endItem))
                    {
                        InitDataLines();
                    }
                }
            }
            // 将所有状态还原
            if (MouseState != MouseState.None)
            {
                lastPos.X = 0;
                lastPos.Y = 0;
                if (MouseState == MouseState.DragRect)
                {
                    ((UIElement)e.Source).ReleaseMouseCapture();
                }

                MouseState = MouseState.None;
                SelectRect = new Rect(0, 0, 0, 0);
                CurrentItem = null;
                _currentLink = null;

                // 还原移动的变量
                moveRect = new Rect(0, 0, 0, 0);
                moveIndex = -1;

                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// 双击Double Click
        /// </summary>
        /// <param name="e">e</param>
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var mousePos = e.GetPosition(this);

                var hitItem = flowItems.FirstOrDefault(u => u.IsHitItem(mousePos));
                if (hitItem != null)
                {
                    OnItemDoubleClick(hitItem);
                }
            }
        }

        #endregion

        #region 重绘
        /// <summary>
        /// 绘制对象
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // 1.绘制背景色
            drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

            // 2.绘制网格线
            DrawGridLine(drawingContext);


            // 3.绘制元素
            if (IsDataFlow)
            {
                // 绘制工站
                DrawStation(drawingContext);

                // 绘制数据线
                DrawDataline(drawingContext);

                // 绘制正在连接的线路
                if (_currentLink != null)
                {
                    _currentLink.DrawConnector(drawingContext, lastPos);
                }
            }
            else
            {
                DrawItems(drawingContext);
            }

            // 5.绘制矩形框
            if (!SelectRect.IsEmpty)
            {
                drawingContext.DrawRectangle(null, new Pen(RectBorderBrush, 2), SelectRect);
            }

            // 7.绘制比例尺
            if (IsShowRuler)
            {
                DrawScaleRuler(drawingContext);
            }

            // 显示描述
            if (IsShowStatus)
            {
                DrawStatusDesc(drawingContext);
            }

            if (!moveRect.IsEmpty && moveRect.Width > 0)
            {
                drawingContext.DrawRectangle(null, new Pen(Brushes.Gray, 2), moveRect);
            }

            // 6.绘制高亮移动位置框
            if (MouseState == MouseState.Move)
            {
                if (IsDataFlow)
                {
                    var rowColumn = outRects.FirstOrDefault(u => u.Contains(moveRect));
                    drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 2), rowColumn.Rect);
                }
                else
                {
                    if (moveIndex != -1)
                    {
                        var rect = flowItems[moveIndex].OuterRect;
                        drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 2), rect);
                    }
                }
            }


        }

        /// <summary>
        /// 尺寸发生变换
        /// </summary>
        /// <param name="arrangeBounds"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // 依据分配的宽高来设置Row和Column
            CalcRowAndColNums(arrangeBounds.Width, arrangeBounds.Height);
            return base.ArrangeOverride(arrangeBounds);
        }

        /// <summary>
        /// 初始评估尺寸
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            if (ItemsSource == null) return base.MeasureOverride(constraint);

            // 原始的行列布局
            int srcNum = RowNum * ColumnNum;
            int itemNum = ItemsSource.Cast<IFlowRender>().Count();

            int rWidth = (int)(constraint.Width - (Padding.Left + Padding.Right));

            // 得到默认的列数量
            if (AutoCalcuGrid)
            {
                ColumnNum = (int)(rWidth / (ColumnWidth * GlobalScaleDelta));
                if (ColumnNum < 1)
                {
                    ColumnNum = 1;
                }
            }

            // 计算需要的行数量
            RowNum = itemNum / ColumnNum;

            // 最少三行
            if (RowNum < 1)
            {
                RowNum = 1;
            }

            double rowHeight = RowNum * RowHeight * GlobalScaleDelta + Padding.Top + Padding.Bottom;

            return new Size(constraint.Width, rowHeight);
        }

        /// <summary>
        /// 计算row和列的数量
        /// 1.控件尺寸发生了变化
        /// 2.数据源发生了变换
        /// </summary>
        private void CalcRowAndColNums(double width, double height)
        {
            int srcRowNum = RowNum;

            // 原始的行列布局
            int srcNum = RowNum * ColumnNum;

            int rWidth = (int)(width - (Padding.Left + Padding.Right));
            int rHeight = (int)(height - (Padding.Top + Padding.Bottom));

            // 自动计算行和列
            if (AutoCalcuGrid)
            {
                // 得到默认的行和列
                ColumnNum = (int)(rWidth / (ColumnWidth * GlobalScaleDelta));
                if (ColumnNum < 1)
                {
                    ColumnNum = 1;
                }
            }

            // 基于高度自动计算的行数量
            int actualHeight = (int)(RowHeight * GlobalScaleDelta);
            int rowNum = rHeight / actualHeight;

            if (ItemsSource != null)
            {
                int itemNum = ItemsSource.Cast<IFlowRender>().Count();
                // 计算最终的行和列
                if (rowNum * ColumnNum <= itemNum)
                {
                    RowNum = itemNum / ColumnNum;
                }
                else
                {
                    RowNum = rowNum;
                }
            }
            else
            {
                RowNum = rowNum;
            }

            // 判断需要高度和实际高度对比
            if (RowNum < 1)
            {
                RowNum = 1;
            }
        }
        #endregion

        #region item选中事件
        /// <summary>
        /// 选中事件
        /// </summary>
        public static readonly RoutedEvent ItemSelectedEvent =
                    EventManager.RegisterRoutedEvent("ItemSelected", RoutingStrategy.Bubble,
                        typeof(RoutedEventHandler), typeof(FlowEditor));

        public event RoutedEventHandler ItemSelected
        {
            add => AddHandler(ItemSelectedEvent, value);
            remove => RemoveHandler(ItemSelectedEvent, value);
        }

        private void OnItemSelected(MouseButtonEventArgs e, IFlowRender item)
        {
            var list = flowItems.Where(u => u.IsSelected).ToList();
            var args = new FlowItemDownArgs(ItemSelectedEvent, item, e) { SelectItems = list, Source = this };
            this.RaiseEvent(args);
        }
        #endregion

        #region item选中事件
        /// <summary>
        /// 选中事件
        /// </summary>
        public static readonly RoutedEvent LineSelectedEvent =
                    EventManager.RegisterRoutedEvent("LineSelected", RoutingStrategy.Bubble,
                        typeof(RoutedEventHandler), typeof(FlowEditor));

        public event RoutedEventHandler LineSelected
        {
            add => AddHandler(LineSelectedEvent, value);
            remove => RemoveHandler(LineSelectedEvent, value);
        }

        private void OnLineSelected(MouseButtonEventArgs e, FlowLine line)
        {
            var args = new FlowLineDownArgs(LineSelectedEvent, line, e) { Source = this };
            this.RaiseEvent(args);
        }
        #endregion

        /// <summary>
        /// 获取选择项目
        /// </summary>
        /// <returns></returns>
        public List<IFlowRender> GetSelectedItems()
        {
            return flowItems.Where(u => u.IsSelected).ToList();
        }

        #region 移动事件
        /// <summary>
        /// 选中事件
        /// </summary>
        public static readonly RoutedEvent ItemsMovedEvent =
            EventManager.RegisterRoutedEvent("ItemsMoved", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FlowEditor));

        public event RoutedEventHandler ItemsMoved
        {
            add => AddHandler(ItemsMovedEvent, value);
            remove => RemoveHandler(ItemsMovedEvent, value);
        }

        private void OnItemsMoved(List<IFlowRender> items, int dstMoveIndex, bool beforeToAfter = true)
        {
            this.RaiseEvent(new FlowItemMoveArgs(ItemsMovedEvent) { MoveItems = items, MoveIndex = dstMoveIndex, BeforeToAfter = beforeToAfter });
        }
        #endregion

        #region 双击事件
        /// <summary>
        /// 双击事件
        /// </summary>
        public static readonly RoutedEvent ItemDoubleClickEvent =
            EventManager.RegisterRoutedEvent("ItemDoubleClick", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FlowEditor));

        public event RoutedEventHandler ItemDoubleClick
        {
            add => AddHandler(ItemDoubleClickEvent, value);
            remove => RemoveHandler(ItemDoubleClickEvent, value);
        }

        private void OnItemDoubleClick(IFlowRender flowItem)
        {
            this.RaiseEvent(new FlowDoubleClickArgs(ItemDoubleClickEvent, flowItem) { Source = this });
        }
        #endregion

        #region 鼠标悬浮事件
        /// <summary>
        /// 双击事件
        /// </summary>
        public static readonly RoutedEvent ItemHoverEvent =
            EventManager.RegisterRoutedEvent("ItemHover", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FlowEditor));

        public event RoutedEventHandler ItemHover
        {
            add => AddHandler(ItemHoverEvent, value);
            remove => RemoveHandler(ItemHoverEvent, value);
        }

        private void OnItemHover(IFlowRender flowItem)
        {
            this.RaiseEvent(new FlowItemHoverArgs(ItemHoverEvent) { Source = this, HoverItem = flowItem });
        }
        #endregion

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        #region 通用属性

        /// <summary>
        /// 是否是数据流
        /// </summary>
        public bool IsDataFlow
        {
            get { return (bool)GetValue(IsDataFlowProperty); }
            set { SetValue(IsDataFlowProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsDataFlow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDataFlowProperty =
            DependencyProperty.Register("IsDataFlow", typeof(bool), typeof(FlowEditor), new PropertyMetadata(true));


        /// <summary>
        /// 每行的高度
        /// </summary>
        public int RowHeight
        {
            get { return (int)GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }
        public static readonly DependencyProperty RowHeightProperty =
            DependencyProperty.Register("RowHeight", typeof(int), typeof(FlowEditor), new PropertyMetadata(75));

        public int ColumnWidth
        {
            get { return (int)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnWidthProperty =
            DependencyProperty.Register("ColumnWidth", typeof(int), typeof(FlowEditor), new PropertyMetadata(165));

        /// <summary>
        /// 是否显示卡尺
        /// </summary>
        public bool IsShowRuler
        {
            get { return (bool)GetValue(IsShowRulerProperty); }
            set { SetValue(IsShowRulerProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsShowRuler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowRulerProperty =
            DependencyProperty.Register("IsShowRuler", typeof(bool), typeof(FlowEditor), new PropertyMetadata(true));

        /// <summary>
        /// 是否显示网格线
        /// </summary>
        public bool IsShowGridLine
        {
            get { return (bool)GetValue(IsShowGridLineProperty); }
            set { SetValue(IsShowGridLineProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsShowGridLine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowGridLineProperty =
            DependencyProperty.Register("IsShowGridLine", typeof(bool), typeof(FlowEditor), new PropertyMetadata(true));

        /// <summary>
        /// 是否显示状态标识
        /// </summary>
        public bool IsShowStatus
        {
            get { return (bool)GetValue(IsShowStatusProperty); }
            set { SetValue(IsShowStatusProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsShowStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowStatusProperty =
            DependencyProperty.Register("IsShowStatus", typeof(bool), typeof(FlowEditor), new PropertyMetadata(true));


        /// <summary>
        /// 水平对其方式
        /// </summary>
        public HorizontalAlignment HAlignment
        {
            get { return (HorizontalAlignment)GetValue(HAlignmentProperty); }
            set { SetValue(HAlignmentProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HAlignmentProperty =
            DependencyProperty.Register("HAlignment", typeof(HorizontalAlignment), typeof(FlowEditor), new PropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        /// 自动网格计算
        /// </summary>
        public bool AutoCalcuGrid
        {
            get { return (bool)GetValue(AutoCalcuGridProperty); }
            set { SetValue(AutoCalcuGridProperty, value); }
        }
        public static readonly DependencyProperty AutoCalcuGridProperty =
            DependencyProperty.Register("AutoCalcuGrid", typeof(bool), typeof(FlowEditor), new PropertyMetadata(true));


        /// <summary>
        /// 行数量
        /// </summary>
        public int RowNum
        {
            get { return (int)GetValue(RowNumProperty); }
            set { SetValue(RowNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowNumProperty =
            DependencyProperty.Register("RowNum", typeof(int), typeof(FlowEditor), new PropertyMetadata(6));

        /// <summary>
        /// 列数量
        /// </summary>
        public int ColumnNum
        {
            get { return (int)GetValue(ColumnNumProperty); }
            set { SetValue(ColumnNumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColumnNum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnNumProperty =
            DependencyProperty.Register("ColumnNum", typeof(int), typeof(FlowEditor), new PropertyMetadata(2));


        /// <summary>
        /// 连接线宽度
        /// </summary>
        public double LineThinkness
        {
            get { return (double)GetValue(LineThinknessProperty); }
            set { SetValue(LineThinknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineThinkness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineThinknessProperty =
            DependencyProperty.Register("LineThinkness", typeof(double), typeof(FlowEditor), new PropertyMetadata(2.0));

        /// <summary>
        /// 连接线颜色
        /// </summary>
        public Brush LineBrush
        {
            get { return (Brush)GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register("LineBrush", typeof(Brush), typeof(FlowEditor), new PropertyMetadata(Brushes.Black));



        public Brush BackgroundBrush
        {
            get { return (Brush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(FlowEditor), new PropertyMetadata(Brushes.White));
        #endregion

        #region 拖拽事件
        /// <summary>
        /// 支持对象拖拽
        /// </summary>
        /// <param name="e">e</param>
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            var pos = e.GetPosition(this);

            // 计算当前鼠标所处的行
            int row = 0;
            int col = 0;

            if (outRects.Count > 0)
            {
                var rect = outRects.FirstOrDefault(u => u.Rect.Contains(pos));
                row = rect.Row;
                col = rect.Column;
            }

            var index = -1;

            // 计算索引
            if (!IsDataFlow)
            {
                var exist = flowItems.FirstOrDefault(u => u.OuterRect.Contains(pos));
                if (exist != null)
                {
                    index = flowItems.IndexOf(exist);
                }
            }

            var args = new ItemDropArgs(ItemDropEvent, row, col, index, e) { Source = this };
            this.RaiseEvent(args);
        }

        /// <summary>
        /// 选中事件
        /// </summary>
        public static readonly RoutedEvent ItemDropEvent =
                    EventManager.RegisterRoutedEvent("ItemDrop", RoutingStrategy.Bubble,
                        typeof(RoutedEventHandler), typeof(FlowEditor));

        public event RoutedEventHandler ItemDrop
        {
            add => AddHandler(ItemDropEvent, value);
            remove => RemoveHandler(ItemDropEvent, value);
        }
        #endregion
    }
}