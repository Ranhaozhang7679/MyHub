#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       FlowItem.cs
* 创建时间:       2022/5/24 11:45:09
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      10a187e4-c8f2-42a3-a4d7-ea2a92cf5025
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 11:45:09
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Luster.Control.Wpf.Motion.Flow
{
    /// <summary>
    /// 流程项目
    /// </summary>
    public class FlowItem : AbsFlowItem
    {
        /// <summary>
        /// 真实对象
        /// </summary>
        public IMotionModule Motion => Tag as IMotionModule;

        public override int Row
        {
            get
            {
                if (Motion != null && Motion.TaskFunction is IStation station)
                {
                    return station.Row;
                }

                return -1;
            }
            set { }
        }

        public override int Column
        {
            get
            {
                if (Motion != null && Motion.TaskFunction is IStation station)
                {
                    return station.Column;
                }

                return -1;
            }
            set { }
        }

        public FlowItem()
        {
            BorderBrush = DefaultBorderBrush;
        }

        /// <summary>
        /// 动画释放
        /// </summary>
        ~FlowItem()
        {
        }

        public FlowItem(IMotionModule motion) : this()
        {
            ID = motion.ID;
            Tag = motion;
            Name = motion.Alias ?? motion.Name;
            Index = motion.Sort;
            TimeConsuming = motion.TimeConsuming;
            Icon = String.IsNullOrEmpty(motion.TaskFunction.Icon) ? motion.Icon : motion.TaskFunction.Icon;
            Motion.PropertyChangedEvent -= Motion_PropertyChangedEvent;
            Motion.PropertyChangedEvent += Motion_PropertyChangedEvent;
            // 更新当前状态
            SetStatus();

            Select(motion.IsSelected);
        }

        /// <summary>
        /// 事件变更
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="propName"></param>
        /// <param name="arg3"></param>
        private void Motion_PropertyChangedEvent(TaskFlow.Common.Module.IModule arg1, string propName, object srcV, object newV)
        {
            if (propName == "Status")
            {
                if (arg1 is IMotionModule m)
                {
                    bool isCurrent = m.Parent.IsCurrent;
                    if (isCurrent)
                    {
                        SetStatus();
                        OnInvalidate();
                    }
                }
            }
            else if (propName == "IsSelected" && bool.TryParse(newV?.ToString(), out var isSelected))
            {
                Select(isSelected);
            }
            else if (propName == "Alias")
            {
                Name = newV.ToString();
                OnInvalidate();
            }
        }

        private void DrawFolder(DrawingContext drawContext, Rect rect)
        {
            var gStatus = new StreamGeometry();
            using (StreamGeometryContext context = gStatus.Open())
            {
                var left = new Point(rect.X, rect.Y - DefaultGroupTop);
                var right = new Point(left.X + IconWidth * DefaultGroupRatio, left.Y);
                var leftBottom = new Point(left.X, rect.Y + 1);
                var rightBottom = new Point(left.X + IconWidth, leftBottom.Y);

                context.BeginFigure(rightBottom, true, false);
                context.LineTo(right, true, false);
                context.LineTo(left, true, false);
                context.LineTo(leftBottom, true, false);
            }

            drawContext.DrawGeometry(StatusBackground, null, gStatus);
        }

        /// <summary>
        /// 绘制元素
        /// </summary>
        /// <param name="drawContext"></param>
        public override void Render(DrawingContext drawContext, Rect outerRect)
        {
            // 设置外边缘范围
            OuterRect = outerRect;

            // 保证起始点居中
            Location = new Point(outerRect.Left + (outerRect.Width - Size.Width) / 2, outerRect.Top + (outerRect.Height - DefaultHeight) / 2);

            bool isGroup = Motion.TaskFunction is IGroup;

            // 1.绘制标题
            var titleRect = new Rect(Rect.Left, Rect.Top, Size.Width, TitleHeight);
            drawContext.DrawRectangle(StatusBackground, null, titleRect);

            // 1.1 标题和内容分割线
            drawContext.DrawLine(new Pen(BorderBrush, 1), new Point(titleRect.Left, titleRect.Bottom), new Point(titleRect.Right, titleRect.Bottom));

            // 2.绘制模块类型
            var name = Luster.Motion.Assests.Langs.LangProvider.GetLang(Motion.TaskFunction.Name);
            bool isByRef = Motion.GetByReferences().Count > 0;
            var typeRect = MeasureText(name, GetTitleBrush(), FontSize, titleRect, isByRef);
            var titlePoint = new Point(titleRect.X + 2, Rect.Top + (TitleHeight - typeRect.Height) / 2);

            drawContext.DrawText(typeRect, titlePoint);

            // 3.耗时
            if (Motion.TimeConsuming > 0)
            {
                string time = GetTimeByMS(Motion.TimeConsuming);
                var timeText = MeasureText(time, GetTitleBrush(), TimeFontSize, titleRect);
                var left = Rect.Right - 4 - timeText.Width;
                var top = Rect.Y + (TitleHeight - timeText.Height) / 2;
                drawContext.DrawText(timeText, new Point(left, top));
            }

            // 4. 绘制状态区域
            var cRect = new Rect(titleRect.X, titleRect.Bottom, Rect.Width, Rect.Height - TitleHeight);
            DrawStatus(drawContext, cRect, out var statusRect);

            // 5.绘制图标 &#xe6b6;
            var imgColor = Motion.TaskFunction is IWait ? Brushes.Orange : Brushes.White;
            var imgSource = ttfInfo.GetStrImage(Icon, IconFontSize, imgColor);
            double imgX = statusRect.X + (statusRect.Width - imgSource.Width) / 2 - IconLeftPadding;
            double imgY = statusRect.Y + (statusRect.Height - imgSource.Height) / 2;
            drawContext.DrawImage(imgSource, new Rect(imgX, imgY, imgSource.Width, imgSource.Height));

            // 6.构造绘制内容对象
            Rect contentRect = new Rect() { X = statusRect.Right, Y = statusRect.Y, Width = cRect.Width - IconWidth - 10, Height = cRect.Height };

            // 6.1.绘制内容
            var formatText = MeasureText(Name, GetContentBrush(), FontSize - 1, contentRect);
            var origin = new Point(contentRect.X + (contentRect.Width - formatText.Width) / 2, contentRect.Y + (contentRect.Height - formatText.Height) / 2);
            drawContext.DrawText(formatText, origin);

            // 7.绘制箭头
            if (isGroup)
            {
                var gFolder = new StreamGeometry();
                using (StreamGeometryContext context = gFolder.Open())
                {
                    var leftTop = new Point(cRect.Right - 10, cRect.Top + (cRect.Height - 16) / 2);
                    var right = new Point(cRect.Right - 4, leftTop.Y + 6);
                    var leftBottom = new Point(leftTop.X, leftTop.Y + 12);

                    context.BeginFigure(leftTop, false, false);
                    context.LineTo(right, true, false);
                    context.LineTo(leftBottom, true, false);
                }

                drawContext.DrawGeometry(null, new Pen(Brushes.Black, 2), gFolder);
            }

            // 8.绘制连接圆
            if (IsHover)
            {
                DrawLink(drawContext);
            }

            // 画外边框
            var pen = new Pen(BorderBrush, BorderBrushWidth);
            var borderRect = new Rect() { X = Rect.X - 1, Y = Rect.Y - 1, Width = Rect.Width + 2, Height = Rect.Height + 2 };
            drawContext.DrawRectangle(null, pen, Rect);


            if (Motion.Status == RunStatus.Running)
            {
                BeginStory();
            }
            else
            {
                StopStory();
            }
        }

        public override object PrevRender => Motion.PrevModule;

        /// <summary>
        /// 绘制元素
        /// </summary>
        /// <param name="drawContext"></param>
        //public void Render(DrawingContext drawContext, Rect outerRect)
        //{
        //    // 设置外边缘范围
        //    OuterRect = outerRect;

        //    // 保证起始点居中
        //    Location = new Point(outerRect.Left + (outerRect.Width - Size.Width) / 2, outerRect.Top + (outerRect.Height - Size.Height) / 2 + 5);

        //    bool isGroup = Motion.TaskFunction is IGroup;

        //    // 1.绘制等待
        //    DrawWait(drawContext);

        //    // 2. 绘制状态区域
        //    DrawStatus(drawContext, isGroup, out var statusRect);

        //    // 3. 如果分组
        //    if (isGroup)
        //    {
        //        DrawFolder(drawContext, statusRect);
        //    }

        //    // 4. 绘制状态和文字分割线
        //    Point startP = new Point(statusRect.Right, Location.Y);
        //    Point endP = new Point(startP.X, Location.Y + Size.Height);
        //    drawContext.DrawLine(new Pen(BorderBrush, 2), startP, endP);

        //    // 5.绘制图标 &#xe6b6;
        //    var imgSource = ttfInfo.GetStrImage(Icon, IconFontSize, Brushes.White);
        //    double imgX = statusRect.X + (statusRect.Width - imgSource.Width) / 2 - IconLeftPadding;
        //    double imgY = statusRect.Y + (statusRect.Height - imgSource.Height) / 2;
        //    if (isGroup)
        //    {
        //        imgY = statusRect.Y - DefaultGroupTop + (statusRect.Height + DefaultGroupTop - imgSource.Height) / 2;
        //    }

        //    drawContext.DrawImage(imgSource, new Rect(imgX, imgY, imgSource.Width, imgSource.Height));



        //    // 6.绘制Item的名称
        //    Rect titleRect = new Rect();
        //    var gTitle = new StreamGeometry();
        //    using (StreamGeometryContext context = gTitle.Open())
        //    {
        //        var left = new Point(startP.X, startP.Y);
        //        var right = new Point(statusRect.X + Size.Width, left.Y);
        //        var rightBottom = new Point(right.X, left.Y + Size.Height);
        //        var leftBottom = new Point(left.X, rightBottom.Y);

        //        right.X += DefaultRadius * 2;
        //        rightBottom.X += DefaultRadius * 2;
        //        context.BeginFigure(right, true, true);
        //        context.LineTo(left, true, false);
        //        context.LineTo(leftBottom, true, false);
        //        context.LineTo(rightBottom, true, false);

        //        titleRect = new Rect(left, rightBottom);
        //    }
        //    drawContext.DrawGeometry(Brushes.White, null, gTitle);

        //    // 5.绘制标题
        //    var formatText = MeasureText(Name, Brushes.Black, FontSize);
        //    var origin = new Point(titleRect.X + (titleRect.Width - formatText.Width) / 2, titleRect.Y + (titleRect.Height - formatText.Height) / 2);
        //    drawContext.DrawText(formatText, origin);

        //    DrawTypeName(drawContext);

        //    // 7.绘制外围圆角边框
        //    var gBorder = new StreamGeometry();
        //    using (StreamGeometryContext context = gBorder.Open())
        //    {
        //        if (isGroup)
        //        {
        //            var fLeft = new Point(statusRect.Left, statusRect.Y - DefaultGroupTop);
        //            var fRightTop = new Point(fLeft.X + IconWidth * DefaultGroupRatio, fLeft.Y);
        //            var fRightBottom = new Point(statusRect.X + IconWidth, statusRect.Y);

        //            context.BeginFigure(fLeft, true, true);
        //            context.LineTo(fRightTop, true, false);
        //            context.LineTo(fRightBottom, true, false);
        //            context.LineTo(titleRect.TopRight, true, false);
        //            context.LineTo(titleRect.BottomRight, true, false);
        //            context.LineTo(statusRect.BottomLeft, true, false);
        //            context.LineTo(fLeft, true, false);
        //        }
        //        else
        //        {
        //            context.BeginFigure(statusRect.TopLeft, false, true);
        //            context.LineTo(titleRect.TopRight, true, false);
        //            context.LineTo(titleRect.BottomRight, true, false);
        //            context.LineTo(statusRect.BottomLeft, true, false);
        //            context.LineTo(statusRect.TopLeft, true, false);
        //        }
        //    }

        //    drawContext.DrawGeometry(null, new Pen(BorderBrush, BorderBrushWidth), gBorder);

        //    // 8.绘制耗时
        //    DrawTime(drawContext);

        //    // 9.绘制连接圆
        //    if (IsHover)
        //    {
        //        DrawLink(drawContext);
        //    }

        //    if (Motion.Status == RunStatus.Running)
        //    {
        //        BeginStory();
        //    }
        //    else
        //    {
        //        StopStory();
        //    }
        //}

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <returns></returns>
        public override string GetTipMessage()
        {
            return Motion.StatusMsg;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public override void SetStatus()
        {
            Console.WriteLine($"{Motion.Alias} {Motion.Sort}:{Motion.Status}");
            switch (Motion.Status)
            {
                case RunStatus.Default:
                    StatusBackground = DefaultBrush;
                    StatusForeground = Brushes.WhiteSmoke;
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
                case RunStatus.Pause:
                    StatusBackground = TimeoutBrush;
                    break;
                case RunStatus.Error:
                    StatusBackground = FailBrush;
                    break;
            }
        }

        protected override Brush GetTitleBrush()
        {
            if (Motion.Status == RunStatus.Running || Motion.Status == RunStatus.Success || Motion.Status == RunStatus.Error)
            {
                return Brushes.White;
            }
            else
            {
                return Brushes.Black;
            }
        }

        /// <summary>
        /// 获取注释内容对应的颜色
        /// </summary>
        /// <returns></returns>
        protected override Brush GetContentBrush()
        {
            // 如果模块被引用，则返回蓝色
            if (Motion.GetReferences().Count > 0)
            {
                return LinkBrush;
            }
            else
            {
                return Brushes.Black;
            }
        }

        /// <summary>
        /// 绘制等待逻辑
        /// </summary>
        private void DrawWait(DrawingContext drawContextout)
        {
            if (Motion.TaskFunction is IWait)
            {
                var rect = new Rect(OuterRect.X + 2, OuterRect.Y + 2, OuterRect.Width - 4, OuterRect.Height - 4);
                drawContextout.DrawRectangle(Brushes.LightYellow, null, rect);
            }
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

        #region 工站流程
        /// <summary>
        /// 获取对应的位置
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int> GetRowColumn()
        {
            if (Motion.TaskFunction is IStation station)
            {
                return Tuple.Create(station.Row, station.Column);
            }

            return Tuple.Create(0, 0);
        }

        /// <summary>
        /// 获取当前工站之前的数据
        /// </summary>
        /// <returns></returns>
        public override List<Guid> GetPrevItems()
        {
            if (Motion.TaskFunction is IStation station)
            {
                return station.PrevStations.Select(u => u.OwnerID).ToList();
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
            if (Motion.TaskFunction is IStation station)
            {
                station.Row = row;
                station.Column = col;
            }
        }

        /// <summary>
        /// 设置下一个连接点
        /// </summary>
        /// <param name="flowItem"></param>
        public override bool AddNextItem(IFlowRender flowItem)
        {
            bool isAdd = false;
            var motion = flowItem.Tag as IMotionModule;
            if (motion.TaskFunction is IStation nextMotion)
            {
                if (Motion.TaskFunction is IStation station)
                {
                    if (!nextMotion.PrevStations.Contains(station))
                    {
                        isAdd = true;
                        nextMotion.PrevStations.Add(station);
                    }
                }
            }

            return isAdd;
        }
        #endregion
    }
}