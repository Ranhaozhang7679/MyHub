using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Luster.Controls.Wpf.CanvasDrag
{
    [TemplatePart(Name = "PART_MainGrid", Type = typeof(Grid))]
    public abstract class DragHelperBase : ContentControl
    {
        #region Ctor

        static DragHelperBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragHelperBase),
                new FrameworkPropertyMetadata(typeof(DragHelperBase)));
        }

        public DragHelperBase()
        {
            SetResourceReference(StyleProperty, typeof(DragHelperBase));
        }

        #endregion Ctor

        #region Propertes

        #region CornerWidth

        public int CornerWidth
        {
            get { return (int)GetValue(CornerWidthProperty); }
            set { SetValue(CornerWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerWidthProperty =
            DependencyProperty.Register("CornerWidth", typeof(int), typeof(DragHelperBase), new PropertyMetadata(31));

        #endregion CornerWidth

        #region PART Object

        #region MainGrid

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Grid MainGrid;

        #endregion MainGrid

        #endregion PART Object

        #region DragHelperParent

        protected FrameworkElement DragHelperParent
        {
            get
            {
                return Parent as FrameworkElement;
            }
        }

        #endregion DragHelperParent

        #endregion Propertes

        #region Drag & Resize

        #region DragElement

        private Rect DragElement(double HorizontalChange, double VerticalChange)
        {
            Rect TargetActualBound = GetTargetActualBound();

            double TopOld = CorrectDoubleValue(TargetActualBound.Y);
            double LeftOld = CorrectDoubleValue(TargetActualBound.X);
            double TopNew = CorrectDoubleValue(TopOld + VerticalChange);
            double LeftNew = CorrectDoubleValue(LeftOld + HorizontalChange);

            TopNew = CorrectNewTop(DragHelperParent, TopNew, TargetActualBound.Height);
            LeftNew = CorrectNewLeft(DragHelperParent, LeftNew, TargetActualBound.Width);

            System.Windows.Controls.Canvas.SetTop(this, TopNew);
            System.Windows.Controls.Canvas.SetLeft(this, LeftNew);

            return new Rect
            {
                Y = TopNew,
                X = LeftNew,
                Width = TargetActualBound.Width,
                Height = TargetActualBound.Height
            };
        }

        #endregion DragElement

        #region ResizeElement

        private Rect ResizeElement(CustomThumb HitedThumb, double HorizontalChange, double VerticalChange)
        {
            #region Get Old Value

            if (HitedThumb == null) return Rect.Empty;

            Rect TargetActualBound = GetTargetActualBound();

            double TopOld = CorrectDoubleValue(TargetActualBound.Y);
            double LeftOld = CorrectDoubleValue(TargetActualBound.X);
            double WidthOld = CorrectDoubleValue(TargetActualBound.Width);
            double HeightOld = CorrectDoubleValue(TargetActualBound.Height);

            double TopNew = TopOld;
            double LeftNew = LeftOld;
            double WidthNew = WidthOld;
            double HeightNew = HeightOld;

            #endregion Get Old Value

            if (HitedThumb.DragDirection == DragDirection.TopLeft
                || HitedThumb.DragDirection == DragDirection.MiddleLeft
                || HitedThumb.DragDirection == DragDirection.BottomLeft)
            {
                ResizeFromLeft(DragHelperParent, LeftOld, WidthOld, HorizontalChange, out LeftNew, out WidthNew);
            }

            if (HitedThumb.DragDirection == DragDirection.TopLeft
                || HitedThumb.DragDirection == DragDirection.TopCenter
                || HitedThumb.DragDirection == DragDirection.TopRight)
            {
                ResizeFromTop(DragHelperParent, TopOld, HeightOld, VerticalChange, out TopNew, out HeightNew);
            }

            if (HitedThumb.DragDirection == DragDirection.TopRight
                || HitedThumb.DragDirection == DragDirection.MiddleRight
                || HitedThumb.DragDirection == DragDirection.BottomRight)
            {
                ResizeFromRight(DragHelperParent, LeftOld, WidthOld, HorizontalChange, out WidthNew);
            }

            if (HitedThumb.DragDirection == DragDirection.BottomLeft
                || HitedThumb.DragDirection == DragDirection.BottomCenter
                || HitedThumb.DragDirection == DragDirection.BottomRight)
            {
                ResizeFromBottom(DragHelperParent, TopOld, HeightOld, VerticalChange, out HeightNew);
            }

            this.Width = WidthNew;
            this.Height = HeightNew;
            System.Windows.Controls.Canvas.SetTop(this, TopNew);
            System.Windows.Controls.Canvas.SetLeft(this, LeftNew);

            return new Rect
            {
                X = LeftNew,
                Y = TopNew,
                Width = WidthNew,
                Height = HeightNew
            };
        }

        #endregion ResizeElement

        #region Resize Base Methods

        #region ResizeFromTop

        private static void ResizeFromTop(FrameworkElement Parent, double TopOld, double HeightOld, double VerticalChange, out double TopNew, out double HeightNew)
        {
            double MiniHeight = 10;

            double top = TopOld + VerticalChange;
            TopNew = ((top + MiniHeight) > (HeightOld + TopOld)) ? HeightOld + TopOld - MiniHeight : top;
            TopNew = TopNew < 0 ? 0 : TopNew;

            HeightNew = HeightOld + TopOld - TopNew;

            HeightNew = CorrectNewHeight(Parent, TopNew, HeightNew);
        }

        #endregion ResizeFromTop

        #region ResizeFromLeft

        private static void ResizeFromLeft(FrameworkElement Parent, double LeftOld, double WidthOld, double HorizontalChange, out double LeftNew, out double WidthNew)
        {
            double MiniWidth = 10;
            double left = LeftOld + HorizontalChange;

            LeftNew = ((left + MiniWidth) > (WidthOld + LeftOld)) ? WidthOld + LeftOld - MiniWidth : left;

            LeftNew = LeftNew < 0 ? 0 : LeftNew;

            WidthNew = WidthOld + LeftOld - LeftNew;

            WidthNew = CorrectNewWidth(Parent, LeftNew, WidthNew);
        }

        #endregion ResizeFromLeft

        #region ResizeFromRight

        private static void ResizeFromRight(FrameworkElement Parent, double LeftOld, double WidthOld, double HorizontalChange, out double WidthNew)
        {
            if (LeftOld + WidthOld + HorizontalChange < Parent.ActualWidth)
            {
                WidthNew = WidthOld + HorizontalChange;
            }
            else
            {
                WidthNew = Parent.ActualWidth - LeftOld;
            }

            WidthNew = WidthNew < 0 ? 0 : WidthNew;
        }

        #endregion ResizeFromRight

        #region ResizeFromBottom

        private static void ResizeFromBottom(FrameworkElement Parent, double TopOld, double HeightOld, double VerticalChange, out double HeightNew)
        {
            if (TopOld + HeightOld + VerticalChange < Parent.ActualWidth)
            {
                HeightNew = HeightOld + VerticalChange;
            }
            else
            {
                HeightNew = Parent.ActualHeight - TopOld;
            }

            HeightNew = HeightNew < 0 ? 0 : HeightNew;
        }

        #endregion ResizeFromBottom

        #region CorrectNewTop

        private static double CorrectNewTop(FrameworkElement Parent, double Top, double Height)
        {
            double NewHeight = ((Top + Height) > Parent.ActualHeight) ? (Parent.ActualHeight - Height) : Top;
            return NewHeight < 0 ? 0 : NewHeight;
        }

        #endregion CorrectNewTop

        #region CorrectNewLeft

        private static double CorrectNewLeft(FrameworkElement Parent, double Left, double Width)
        {
            double NewLeft = ((Left + Width) > Parent.ActualWidth) ? (Parent.ActualWidth - Width) : Left;

            return NewLeft < 0 ? 0 : NewLeft;
        }

        #endregion CorrectNewLeft

        #region CorrectNewWidth

        private static double CorrectNewWidth(FrameworkElement Parent, double Left, double WidthNewToCheck)
        {
            double Width = ((Left + WidthNewToCheck) > Parent.ActualWidth) ? (Parent.ActualWidth - Left) : WidthNewToCheck;

            return Width < 0 ? 0 : Width;
        }

        #endregion CorrectNewWidth

        #region CorrectNewHeight

        private static double CorrectNewHeight(FrameworkElement Parent, double Top, double HeightNewToCheck)
        {
            double Height = ((Top + HeightNewToCheck) > Parent.ActualHeight) ? (Parent.ActualHeight - Top) : HeightNewToCheck;
            return Height < 0 ? 0 : Height;
        }

        #endregion CorrectNewHeight

        #region CorrectDoubleValue

        protected static double CorrectDoubleValue(double Value)
        {
            return (double.IsNaN(Value) || (Value < 0.0)) ? 0 : Value;
        }

        #endregion CorrectDoubleValue

        #endregion Resize Base Methods

        protected abstract bool GetTargetIsEditable();

        protected abstract Rect GetTargetActualBound();

        protected abstract void SetTargetActualBound(Rect NewBound);

        protected abstract void RaisenDragChangingEvent(Rect NewBound);

        protected abstract void RaisenDragCompletedEvent(Rect NewBound);

        #endregion Drag & Resize

        #region Layout & Display

        #region OnApplyTemplate

        public override sealed void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            MainGrid = GetPartFormTemplate<Grid>("PART_MainGrid");

            AddLogicalChild(MainGrid);

            AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
            AddHandler(Thumb.DragCompletedEvent, new RoutedEventHandler(OnDragCompleted));

            Visibility = Visibility.Collapsed;
        }

        #endregion OnApplyTemplate

        #region GetPartFormTemplate

        private T GetPartFormTemplate<T>(string name)
        {
            return (T)Template.FindName(name, this);
        }

        #endregion GetPartFormTemplate

        #region SetupVisualPropertes

        protected void SetupVisualPropertes(double TargetThickness, bool IsEditable)
        {
            Visibility IsCornerVisibe = IsEditable ? Visibility.Visible : Visibility.Collapsed;

            double ActualMargin = (CornerWidth - TargetThickness) / 2.0;

            MainGrid.Margin = new Thickness(0 - ActualMargin);

            foreach (CustomThumb item in MainGrid.Children)
            {
                if (item != null)
                {
                    item.BorderThickness = new Thickness(TargetThickness);

                    if (item.DragDirection == DragDirection.MiddleCenter)
                    {
                        item.Margin = new Thickness(ActualMargin);
                    }
                    else
                    {
                        item.Visibility = IsCornerVisibe;
                    }
                }
            }
        }

        #endregion SetupVisualPropertes

        #endregion Layout & Display

        #region DragCompletedEvent

        public static readonly RoutedEvent DragChangingEvent
            = EventManager.RegisterRoutedEvent("DragChangingEvent", RoutingStrategy.Bubble, typeof(DragChangedEventHandler), typeof(DragHelperBase));

        public event DragChangedEventHandler DragChanging
        {
            add
            {
                AddHandler(DragChangingEvent, value);
            }
            remove
            {
                RemoveHandler(DragChangingEvent, value);
            }
        }

        public static readonly RoutedEvent DragCompletedEvent
                    = EventManager.RegisterRoutedEvent("DragCompletedEvent", RoutingStrategy.Bubble, typeof(DragChangedEventHandler), typeof(DragHelperBase));

        public event DragChangedEventHandler DragCompleted
        {
            add
            {
                AddHandler(DragCompletedEvent, value);
            }
            remove
            {
                RemoveHandler(DragCompletedEvent, value);
            }
        }

        #endregion DragCompletedEvent

        #region Drag Event Handler

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!GetTargetIsEditable())
            {
                e.Handled = true;
                return;
            }

            CustomThumb thumb = e.OriginalSource as CustomThumb;

            if (thumb == null)
            {
                return;
            }
            double VerticalChange = e.VerticalChange;
            double HorizontalChange = e.HorizontalChange;
            Rect NewBound = Rect.Empty;

            if (thumb.DragDirection == DragDirection.MiddleCenter)
            {
                //拖动改变位置
                NewBound = DragElement(HorizontalChange, VerticalChange);
            }
            else
            {
                //拖动改变大小，重绘问题1228
                NewBound = ResizeElement(thumb, HorizontalChange, VerticalChange);
            }

            RaisenDragChangingEvent(NewBound);
            SetTargetActualBound(NewBound);

            e.Handled = true;
        }

        private void OnDragCompleted(object sender, RoutedEventArgs e)
        {
            Rect NewBound = new Rect
            {
                Y = System.Windows.Controls.Canvas.GetTop(this),
                X = System.Windows.Controls.Canvas.GetLeft(this),
                Width = this.ActualWidth,
                Height = this.ActualHeight
            };

            RaisenDragCompletedEvent(NewBound);

            e.Handled = true;
        }

        #endregion Drag Event Handler
    }
}