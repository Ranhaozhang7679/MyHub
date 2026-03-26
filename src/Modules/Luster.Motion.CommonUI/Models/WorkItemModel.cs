using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.CommonUI.ViewModel;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using Luster.Motion.TaskFlow.Engine.Models;
using System.Windows.Shapes;
using System.Windows.Controls;
using Luster.Common.DataStruct.Extensions;

namespace Luster.Motion.CommonUI.Models
{
    public class WorkItemModel : AbsFlowItem
    {
        /// <summary>
        /// 模块宽带
        /// </summary>
        protected override double DefaultWidth { get; set; } = 160;    // 尺寸宽度

        /// <summary>
        /// 模块高度
        /// </summary>
        protected override double DefaultHeight { get; set; } = 54;    // 尺寸高度

        protected override Brush TitleBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 16, 16, 16));
        protected override Brush IconBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));

        protected override double DefaultIconWidth { get; set; } = 32;

        protected override double DefaultIconFontSize { get; set; } = 36;

        protected override double DefaultFontSize { get; set; } = 14;

        protected override double DefaultTitleHeight { get; set; } = 14;

        protected override double BorderBrushWidth { get; set; } = 4;

        protected override Brush DefaultBorderBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 16, 16, 16));

        /// <summary>
        /// 矩形边框
        /// </summary>
        private int Radius = 8;

        /// <summary>
        /// 真实对象
        /// </summary>
        public WorkItem Item => Tag as WorkItem;

        public override int Row
        {
            get
            {
                if (Item != null)
                {
                    return Item.Row;
                }

                return -1;
            }
            set { }
        }

        public override int Column
        {
            get
            {
                if (Item != null)
                {
                    return Item.Column;
                }

                return -1;
            }
            set { }
        }


        public WorkItemModel() : base()
        {
            BorderBrush = DefaultBorderBrush;
        }

        /// <summary>
        /// 动画释放
        /// </summary>
        ~WorkItemModel()
        {
        }

        public WorkItemModel(WorkItem wItem) : this()
        {
            ID = wItem.ID;
            Tag = wItem;
            Name = wItem.Name;
            TimeConsuming = wItem.GetTimeConsuming();
            Icon = wItem.Icon;
            wItem.StatusChanged -= WItem_StatusChanged;
            wItem.StatusChanged += WItem_StatusChanged;

            // 更新当前状态
            SetStatus();

            Select(wItem.IsSelected);
        }

        private void WItem_StatusChanged(IMotionModule module, string name, bool isStart, RunStatus arg1, bool isSelf = true)
        {
            SetStatus();
            OnInvalidate();
        }

        /// <summary>
        /// 绘制元素
        /// </summary>
        /// <param name="drawContext"></param>
        public override void Render(DrawingContext drawContext, Rect outerRect)
        {
            // 设置外边缘范围
            OuterRect = outerRect;

            Size = new Size(Size.Width, Size.Height + TitleHeight);

            // 保证起始点居中
            Location = new Point(outerRect.Left + (outerRect.Width - Size.Width) / 2, outerRect.Top + (outerRect.Height - Size.Height) / 2);

            // 1.绘制背景
            drawContext.DrawRoundedRectangle(TitleBrush, null, Rect, Radius, Radius);

            // 2.绘制内容布局
            var rContent = new Rect() { X = Location.X + BorderBrushWidth, Y = Location.Y + BorderBrushWidth, Width = Rect.Width - BorderBrushWidth * 2, Height = Size.Height - TitleHeight - BorderBrushWidth * 2 };
            drawContext.DrawRoundedRectangle(Brushes.White, null, rContent, Radius * 0.3, Radius * 0.3);

            // 3.绘制图标 &#xe6b6;
            var imgSource = ttfInfo.GetStrImage(Icon, IconFontSize, IconBrush);
            double imgX = rContent.X;
            double imgY = rContent.Y + (rContent.Height - imgSource.Height) / 2;
            var imgRect = new Rect(imgX, imgY, imgSource.Width, imgSource.Height);
            drawContext.DrawImage(imgSource, imgRect);

            // 6.构造绘制内容对象
            Rect contentRect = new Rect() { X = imgRect.Right, Y = rContent.Y, Width = rContent.Width - IconWidth, Height = rContent.Height };

            // 6.1.绘制标题
            var formatText = MeasureText(Name, GetContentBrush(), FontSize, contentRect);
            formatText.SetFontWeight(FontWeights.Bold);
            var origin = new Point(contentRect.X + 5, contentRect.Y + 2);
            drawContext.DrawText(formatText, origin);

            var s = Item.Status.GetDescription();
            var statusText = MeasureText(s, GetContentBrush(), FontSize - 3, rContent);
            var statusRect = new Rect(contentRect.X + 5, origin.Y + formatText.Height + (contentRect.Height - formatText.Height - statusText.Height) / 2 - 3, statusText.Width + 8, statusText.Height + 4);
            drawContext.DrawRectangle(StatusBackground, null, statusRect);

            var statusPoint = new Point(statusRect.X + (statusRect.Width - statusText.Width) / 2, statusRect.Y + (statusRect.Height - statusText.Height) / 2 - 2);
            drawContext.DrawText(statusText, statusPoint);

            // 8.绘制连接圆
            if (IsHover)
            {
                DrawLink(drawContext);
            }

            // 绘制选中边框
            var borderPen = new Pen(BorderBrush, Radius);
            drawContext.DrawRoundedRectangle(null, borderPen, Rect, Radius, Radius);

            // 绘制SN标签
            if (!string.IsNullOrEmpty(Item.SN))
            {
                var sn = Item.SN;
                var snRect = new Rect(rContent.Left, rContent.Bottom, rContent.Width, TitleHeight);
                var typeRect = MeasureText(sn, GetTitleBrush(), FontSize - 4, snRect);
                var titlePoint = new Point(snRect.X + 4, snRect.Top + 4 + (TitleHeight - typeRect.Height) / 2);
                drawContext.DrawText(typeRect, titlePoint);
            }

            if (Item.Status == RunStatus.Running)
            {
                BeginStory();
            }
            else
            {
                StopStory();
            }
        }

        /// <summary>
        /// 前一个模块对象
        /// </summary>
        public override object PrevRender => Item.PrevItems?.FirstOrDefault();

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <returns></returns>
        public override string GetTipMessage()
        {
            return Item.Name;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public override void SetStatus()
        {
            switch (Item.Status)
            {
                case RunStatus.Default:
                    StatusForeground = Brushes.SkyBlue;
                    break;
                case RunStatus.Running:
                    StatusBackground = RunningBrush;
                    break;
                case RunStatus.Skip:
                    StatusBackground = SkipBrush;
                    break;
                case RunStatus.Success:
                    StatusBackground = SuccessBrush;
                    break;
                case RunStatus.Alarmed:
                    StatusBackground = TimeoutBrush;
                    break;
                case RunStatus.Error:
                    StatusBackground = FailBrush;
                    break;
            }
        }

        protected override Brush GetTitleBrush()
        {
            return Brushes.White;
        }

        /// <summary>
        /// 获取注释内容对应的颜色
        /// </summary>
        /// <returns></returns>
        protected override Brush GetContentBrush()
        {
            return Brushes.Black;
        }

        /// <summary>
        /// 字符串重写
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // 防止渲染的界面上
            return $"";
        }

        public override void Select(bool isSelected = true)
        {
            IsSelected = isSelected;
            if (isSelected)
            {
                BorderBrush = Brushes.Green;
            }
            else
            {
                BorderBrush = DefaultBorderBrush;
            }
        }

        #region 工站流程
        /// <summary>
        /// 获取对应的位置
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int> GetRowColumn()
        {
            if (Item != null)
            {
                return Tuple.Create(Item.Row, Item.Column);
            }

            return Tuple.Create(0, 0);
        }

        /// <summary>
        /// 获取当前工站之前的数据
        /// </summary>
        /// <returns></returns>
        public override List<Guid> GetPrevItems()
        {
            if (Item != null)
            {
                return Item.PrevIds;
            }
            else
            {
                return new List<Guid>();
            }
        }

        /// <summary>
        /// 更新位置信息
        /// </summary>
        /// <param name="row">隶属行</param>
        /// <param name="col">隶属列</param>
        public override void SetRowColumn(int row, int col)
        {
            if (Item != null)
            {
                Item.Row = row;
                Item.Column = col;
            }
        }

        /// <summary>
        /// 设置下一个连接点
        /// </summary>
        /// <param name="flowItem"></param>
        public override bool AddNextItem(IFlowRender flowItem)
        {
            bool isAdd = false;
            var nextItem = flowItem.Tag as WorkItem;
            if (nextItem != null && Item != null)
            {
                if (!nextItem.PrevItems.Contains(Item))
                {
                    isAdd = true;
                    nextItem.PrevItems.Add(Item);

                    // 更新上一个IDs
                    nextItem.PrevIds = nextItem.PrevItems.Select(u => u.ID).ToList();
                }
            }
            return isAdd;
        }
        #endregion
    }
}
