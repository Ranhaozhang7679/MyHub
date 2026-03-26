using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Luster.Control.Wpf.Motion.Flow
{
    public abstract class AbsFlowItem : IFlowRender
    {
        #region 属性
        public Guid ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public object Tag { get; set; }

        /// <summary>
        /// 是否已经选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否已经选中
        /// </summary>
        public bool IsHover { get; set; }

        /// <summary>
        /// 所处位置
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 耗时
        /// </summary>
        protected float TimeConsuming { get; set; } = 0;

        /// <summary>
        /// 图标
        /// </summary>
        protected string Icon { get; set; }

        /// <summary>
        /// 外边缘
        /// </summary>
        public Rect OuterRect { get; set; }

        public virtual int Row
        {
            get; set;
        }

        public virtual int Column
        {
            get; set;
        }
        #endregion

        #region UI 默认值
        protected virtual double DefaultIconWidth { get; set; } = 32; // 图标宽度
        protected virtual double DefaultFontSize { get; set; } = 12;  // 字体大小
        protected virtual double DefaultTimeFontSize { get; set; } = 10; // 显示字体大小
        protected virtual double DefaultWidth { get; set; } = 150;    // 尺寸宽度
        protected virtual double DefaultHeight { get; set; } = 52;    // 尺寸高度
        protected virtual double DefaultTitleHeight { get; set; } = 20;
        protected virtual Brush DefaultBorderBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); //边框颜色
        protected virtual double DefaultIconFontSize { get; set; } = 24;   // 字体图标尺寸
        protected virtual double DefaultIconLeftPadding { get; set; } = 2;
        protected readonly double DefaultGroupRatio = 0.8;    // group 宽度占Icon 占比
        protected readonly double DefaultGroupTop = 6;        // group 组上边缘
        #endregion

        #region 状态颜色默认值
        public static readonly Brush DefaultBrush = new SolidColorBrush(Color.FromArgb(255, 188, 188, 188));
        public static readonly Brush RunningBrush = new SolidColorBrush(Color.FromArgb(255, 24, 144, 255));
        public static readonly Brush SuccessBrush = new SolidColorBrush(Color.FromArgb(255, 82, 196, 26));
        public static readonly Brush FailBrush    = new SolidColorBrush(Color.FromArgb(255, 219, 51, 64));
        public static readonly Brush SkipBrush    = new SolidColorBrush(Color.FromArgb(255, 128, 49, 238));
        public static readonly Brush TimeoutBrush = new SolidColorBrush(Color.FromArgb(255, 250, 173, 20));
        public static readonly Brush NonAreaBrush = new SolidColorBrush(Color.FromArgb(255, 51, 59, 67));
        public static readonly Brush WarningBrush = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0));
        protected virtual Brush IconBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 1, 1, 1));
        public static readonly Brush LinkBrush = new SolidColorBrush(Color.FromArgb(255, 61, 142, 214));
        protected virtual Brush TitleBrush { get; set; } = Brushes.Black;       // 标题颜色
        #endregion

        /// <summary>
        /// 隶属的矩形
        /// </summary>
        public Rectangle MyRectangle { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dispatcher Dispatcher { get; set; }
        #region 事件
        /// <summary>
        /// 需要主界面发生重绘
        /// </summary>
        public event Action InvalidateEvent;

        #endregion

        #region UI 相关

        protected TTFInfo ttfInfo;
        protected Brush BackgroundColor = Brushes.Black;  // 背景
        protected Brush BorderBrush;                      // 边框
        protected virtual double BorderBrushWidth { get; set; } = 1;            // 边框厚度
        protected virtual double IconWidth { get; set; } = 24;                  // 图标宽度


        protected FontFamily FontFamily { get; set; }     // 文字类型
        protected double FontSize { get; set; }           // 字体大小
        protected double TimeFontSize { get; set; }       // 时间日期大小
        protected virtual double IconFontSize { get; set; }
        protected double IconLeftPadding;
        protected virtual double TitleHeight { get; set; } = 20;

        protected Brush StatusBackground = Brushes.LightBlue;      // 状态图标背景
        protected Brush StatusForeground = Brushes.Black;          // 状态前景色

        /// <summary>
        /// 位置信息
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// 尺寸信息
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// 当前的矩形范围
        /// </summary>
        public Rect Rect => new Rect(Location, Size);

        /// <summary>
        /// 连接线的圆
        /// </summary>
        protected virtual Rect LinkRect => new Rect(Location.X + Size.Width / 2 - 5, Location.Y + Size.Height, 10, 20);

        /// <summary>
        /// 设置字体资源
        /// </summary>
        public FontStyle FontStyle { get; set; }

        #endregion

        public AbsFlowItem()
        {
            BorderBrush = DefaultBorderBrush;

            // 主程序集所对应的资源
            ttfInfo = new TTFInfo("pack://application:,,,/Resources/iconfont.ttf");
        }

        /// <summary>
        /// 动画释放
        /// </summary>
        ~AbsFlowItem()
        {
            //StopStory();
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
        public abstract void Render(DrawingContext drawContext, Rect outerRect);

        public virtual object PrevRender => null;


        protected void OnInvalidate()
        {
            if (Tag is IMotionModule m)
            {
                if (m.IsUseLog())
                {
                    InvalidateEvent?.Invoke();
                }
            }
        }
        /// <summary>
        /// 绘制连接圆
        /// </summary>
        protected virtual void DrawLink(DrawingContext drawContext)
        {
            // 绘制三角箭头
            double startX = LinkRect.X;
            double startY = LinkRect.Y + 3;

            Point left1 = new Point(startX, startY);
            Point left2 = new Point(left1.X, left1.Y + 8);
            Point left3 = new Point(left1.X - 3, left2.Y);
            Point arrow = new Point(startX + 3, left3.Y + 6);

            Point right1 = new Point(startX + 6, startY);
            Point right2 = new Point(right1.X, right1.Y + 8);
            Point right3 = new Point(right1.X + 3, right2.Y);

            var g = new StreamGeometry();
            using (StreamGeometryContext context = g.Open())
            {
                context.BeginFigure(left1, true, true);
                context.LineTo(left2, true, false);
                context.LineTo(left3, true, false);
                context.LineTo(arrow, true, false);

                context.LineTo(right3, true, false);
                context.LineTo(right2, true, false);
                context.LineTo(right1, true, false);
                context.LineTo(left1, true, false);
            }

            drawContext.DrawGeometry(Brushes.LightGreen, null, g);
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <returns></returns>
        public virtual string GetTipMessage()
        {
            return String.Empty;
        }

        private static long Second = 1000;
        public static long Minute = Second * 60;
        private static long Hour = Minute * 60;
        private static long Day = Hour * 24;

        /// <summary>
        /// 将ms转换成对的字符串时间
        /// </summary>
        /// <param name="ms">毫秒</param>
        /// <returns></returns>
        protected string GetTimeByMS(float ms, int decplace = 1)
        {
            StringBuilder sb = new StringBuilder();

            if (ms > Day)
            {
                sb.Append($"{(long)ms / Day}d ");
                ms = ms % Day;
            }

            if (ms > Hour)
            {
                sb.Append($"{(long)ms / Hour}h ");
                ms = ms % Hour;
            }
            if (ms > Minute)
            {
                sb.Append($"{(long)ms / Minute}m ");
                ms = ms % Minute;
            }
            if (ms > Second)
            {
                sb.Append($"{(long)ms / Second}s ");
                ms = ms % Second;
            }

            if (ms > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append($"{(int)ms}ms ");
                }
                else
                {
                    sb.Append($"{ms}ms ");
                }
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 更新状态
        /// </summary>
        public virtual void SetStatus()
        {
        }

        protected virtual Brush GetTitleBrush()
        {
            return Brushes.Black;
        }

        /// <summary>
        /// 获取注释内容对应的颜色
        /// </summary>
        /// <returns></returns>
        protected virtual Brush GetContentBrush()
        {
            return Brushes.Black;
        }

        /// <summary>
        /// 绘制状态
        /// </summary>
        protected virtual void DrawStatus(DrawingContext drawContextout, Rect cRect, out Rect statusRect)
        {
            statusRect = new Rect();
            // 2. 绘制状态区域
            var gStatus = new StreamGeometry();
            using (StreamGeometryContext context = gStatus.Open())
            {
                var left = new Point(cRect.X, cRect.Y);
                var right = new Point(cRect.X + IconWidth, cRect.Y);
                var leftBottom = new Point(left.X, cRect.Y + cRect.Height);
                var rightBottom = new Point(right.X, leftBottom.Y);

                context.BeginFigure(right, true, true);
                context.LineTo(left, true, false);
                context.LineTo(leftBottom, true, false);
                context.LineTo(rightBottom, true, false);

                statusRect = new Rect(left, rightBottom);
            }

            drawContextout.DrawGeometry(IconBrush, null, gStatus);
        }

        /// <summary>
        /// 文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontFamily"></param>
        /// <returns></returns>
        [Obsolete]
        protected FormattedText MeasureText(string text, Brush fontBrush, double fontSize, Rect cRect, bool isUnderLine = false)
        {
            var formattedText = new FormattedText(
               text,
               System.Globalization.CultureInfo.InvariantCulture,
               FlowDirection.LeftToRight,
               new Typeface("微软雅黑"),
               fontSize,
               fontBrush
               )
            { Trimming = TextTrimming.CharacterEllipsis, MaxTextWidth = cRect.Width, MaxTextHeight = cRect.Height };

            if (isUnderLine)
            {
                formattedText.SetTextDecorations(TextDecorations.Underline);
                formattedText.SetForegroundBrush(Brushes.Blue);
            }

            return formattedText;
        }

        /// <summary>
        /// 鼠标是否命中当前
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsHitItem(Point p)
        {
            return Rect.Contains(p);
        }

        /// <summary>
        /// 鼠标命中
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsHitItem(double x, double y)
        {
            return Rect.Contains(x, y);
        }

        /// <summary>
        /// Item 进行移动
        /// </summary>
        /// <param name="offsetX">X移动</param>
        /// <param name="offsetY">Y移动</param>
        public void Move(double offsetX, double offsetY)
        {
            Location = new Point(Location.X + offsetX, Location.Y + offsetY);
        }

        /// <summary>
        /// 对尺寸进行缩放
        /// </summary>
        /// <param name="scaleDelta"></param>
        public void ScaleBy(double scaleDelta)
        {
            IconWidth = DefaultIconWidth * scaleDelta;
            IconFontSize = DefaultIconFontSize * scaleDelta;
            IconLeftPadding = DefaultIconLeftPadding * scaleDelta;
            FontSize = DefaultFontSize * scaleDelta;
            TimeFontSize = DefaultTimeFontSize * scaleDelta;
            Size = new Size(DefaultWidth * scaleDelta, DefaultHeight * scaleDelta);
            TitleHeight = DefaultTitleHeight * scaleDelta;
        }

        /// <summary>
        /// 选择
        /// </summary>
        /// <param name="isSelected"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Select(bool isSelected = true)
        {
            IsSelected = isSelected;
            if (isSelected)
            {
                BorderBrush = Brushes.Green;
                BorderBrushWidth = 3;
            }
            else
            {
                BorderBrush = DefaultBorderBrush;
                BorderBrushWidth = 1;
            }
        }

        /// <summary>
        /// 鼠标悬浮
        /// </summary>
        /// <param name="isHover"></param>
        public virtual void Hover(Point pos)
        {
            IsHover = Rect.Contains(pos) || LinkRect.Contains(pos);
        }

        /// <summary>
        /// 和矩形有相交
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsHitItem(Rect rect)
        {
            return rect.IntersectsWith(Rect);
        }

        /// <summary>
        /// 创建link线
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool CreateNewLink(Point pos)
        {
            // 计算连接点位置
            return LinkRect.Contains(pos);
        }

        #region 动画
        /// <summary>
        /// 动画效果
        /// </summary>
        Storyboard storyboard = null;

        /// <summary>
        /// 开始动画
        /// </summary>
        /// <param name="rect">动画矩形</param>
        /// <param name="top">上边距</param>
        /// <param name="left">下边距（由于Cavas性质，所以必须减去left）</param>
        public void BeginStory()
        {
            if (MyRectangle == null) return;

            Dispatcher?.Invoke(() =>
            {
                MyRectangle.Width = OuterRect.Width - 2;
                MyRectangle.Height = OuterRect.Height - 2;
                MyRectangle.Stroke = Brushes.RoyalBlue;
                MyRectangle.StrokeDashArray = new DoubleCollection() { 4.045, 4.045 };
                MyRectangle.StrokeDashCap = PenLineCap.Round;
                MyRectangle.StrokeThickness = 3;

                Canvas.SetLeft(MyRectangle, OuterRect.Left - OffsetX + 1);
                Canvas.SetTop(MyRectangle, OuterRect.Top - OffsetY + 1);
                if (storyboard == null)
                {
                    var runningAnimation = new DoubleAnimationUsingKeyFrames();
                    var timeSpan = TimeSpan.Parse("0:2:0");
                    EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame(-500, KeyTime.FromTimeSpan(timeSpan));
                    runningAnimation.KeyFrames.Add(frame);

                    storyboard = new Storyboard();
                    storyboard.Children.Add(runningAnimation);

                    Storyboard.SetTarget(runningAnimation, MyRectangle);
                    Storyboard.SetTargetProperty(runningAnimation, new PropertyPath("StrokeDashOffset"));
                    Timeline.SetDesiredFrameRate(storyboard, 60);
                }

                storyboard.Begin();
            });
        }

        /// <summary>
        /// 停止动画
        /// </summary>
        /// <param name="rect">将矩形框位置放到0点隐藏起来</param>
        public void StopStory()
        {
            if (MyRectangle == null || storyboard == null) return;

            Dispatcher?.Invoke(() =>
            {
                MyRectangle.Width = 0;
                Canvas.SetLeft(MyRectangle, 0);
                Canvas.SetTop(MyRectangle, 0);
                storyboard?.Stop();
                //storyboard = null;
            });
        }
        #endregion

        #region 上下左右四个点位
        /// <summary>
        /// 获取连接开始点
        /// </summary>
        /// <returns></returns>
        public Point GetBottomPos()
        {
            return new Point(Location.X + Size.Width / 2, Location.Y + Size.Height);
        }

        /// <summary>
        /// 获取结束连接点
        /// </summary>
        /// <returns></returns>
        public Point GetTopPos()
        {
            return new Point(Location.X + Size.Width / 2, Location.Y);
        }

        public Point GetLeftPos()
        {
            return new Point(Location.X, Location.Y + Size.Height / 2);
        }

        public Point GetRightPos()
        {
            return new Point(Location.X + Size.Width, Location.Y + Size.Height / 2);
        }
        #endregion

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
        public virtual Tuple<int, int> GetRowColumn()
        {
            return Tuple.Create(0, 0);
        }

        /// <summary>
        /// 获取当前工站之前的数据
        /// </summary>
        /// <returns></returns>
        public virtual List<Guid> GetPrevItems()
        {
            return new List<Guid>();
        }

        /// <summary>
        /// 更新位置信息
        /// </summary>
        /// <param name="row">隶属行</param>
        /// <param name="col">隶属列</param>
        public virtual void SetRowColumn(int row, int col)
        {
        }

        /// <summary>
        /// 设置下一个连接点
        /// </summary>
        /// <param name="flowItem"></param>
        public virtual bool AddNextItem(IFlowRender flowItem)
        {
            return false;
        }
        #endregion
    }
}
