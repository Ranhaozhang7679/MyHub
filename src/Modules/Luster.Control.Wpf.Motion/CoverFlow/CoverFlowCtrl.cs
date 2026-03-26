using HandyControl.Tools.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Luster.Control.Wpf.Motion.CoverFlow
{
    /// <summary>
    ///     封面流
    /// </summary>
    [TemplatePart(Name = ElementViewport3D, Type = typeof(Viewport3D))]
    [TemplatePart(Name = ElementCamera, Type = typeof(ProjectionCamera))]
    [TemplatePart(Name = ElementVisualParent, Type = typeof(ModelVisual3D))]
    public class CoverFlowCtrl : System.Windows.Controls.Control
    {
        private const string ElementViewport3D = "PART_Viewport3D";

        private const string ElementCamera = "PART_Camera";

        private const string ElementVisualParent = "PART_VisualParent";

        /// <summary>
        ///     最大显示数量的一半
        /// </summary>
        private const int MaxShowCountHalf = 3;

        public int Count { get; set; }

        /// <summary>
        ///     页码
        /// </summary>
        public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(
            "PageIndex", typeof(int), typeof(CoverFlowCtrl),
            new PropertyMetadata(ValueBoxes.Int0Box, OnPageIndexChanged, CoercePageIndex));

        private static object CoercePageIndex(DependencyObject d, object baseValue)
        {
            var ctl = (CoverFlowCtrl)d;
            var v = (int)baseValue;

            if (v < 0)
            {
                return 0;
            }
            if (v >= ctl.flowItems.Count)
            {
                return ctl.flowItems.Count - 1;
            }

            return v;
        }

        private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (CoverFlowCtrl)d;
            ctl.UpdateIndex((int)e.NewValue);
        }


        /// <summary>
        /// 数据源
        /// </summary>
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(CoverFlowCtrl), new PropertyMetadata(default));


        /// <summary>
        ///     是否循环
        /// </summary>
        public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(
            "Loop", typeof(bool), typeof(CoverFlowCtrl), new PropertyMetadata(ValueBoxes.FalseBox));

        /// <summary>
        ///     存储所有的内容
        /// </summary>
        private Dictionary<int, object> _contentDic = new Dictionary<int, object>();

        /// <summary>
        ///     当前在显示范围内的项
        /// </summary>
        private readonly Dictionary<int, CoverFlowItem> _itemShowDic = new Dictionary<int, CoverFlowItem>();

        /// <summary>
        /// 当前对象列表
        /// </summary>
        private readonly List<CoverFlowItem> flowItems = new List<CoverFlowItem>();

        /// <summary>
        ///     相机
        /// </summary>
        private ProjectionCamera _camera;

        private Point3DAnimation _point3DAnimation;

        /// <summary>
        ///     3d画布
        /// </summary>
        private Viewport3D _viewport3D;

        /// <summary>
        ///     项容器
        /// </summary>
        private ModelVisual3D _visualParent;

        /// <summary>
        ///     显示范围内第一个项的编号
        /// </summary>
        private int _firstShowIndex;

        /// <summary>
        ///     显示范围内最后一个项的编号
        /// </summary>
        private int _lastShowIndex;

        /// <summary>
        ///     页码
        /// </summary>
        public int PageIndex
        {
            get => (int)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }

        /// <summary>
        ///     是否循环
        /// </summary>
        public bool Loop
        {
            get => (bool)GetValue(LoopProperty);
            set => SetValue(LoopProperty, ValueBoxes.BooleanBox(value));
        }

        public override void OnApplyTemplate()
        {
            if (_viewport3D != null)
            {
                _viewport3D.Children.Clear();
                _itemShowDic.Clear();
                _viewport3D.MouseLeftButtonDown -= Viewport3D_MouseLeftButtonDown;
            }

            base.OnApplyTemplate();

            _viewport3D = GetTemplateChild(ElementViewport3D) as Viewport3D;
            if (_viewport3D != null)
            {
                _viewport3D.MouseLeftButtonDown += Viewport3D_MouseLeftButtonDown;
            }

            _camera = GetTemplateChild(ElementCamera) as ProjectionCamera;
            _visualParent = GetTemplateChild(ElementVisualParent) as ModelVisual3D;

            UpdateShowRange();

            _point3DAnimation = new Point3DAnimation(new Point3D(CoverFlowItem.Interval * PageIndex, _camera.Position.Y, _camera.Position.Z), new Duration(TimeSpan.FromMilliseconds(200)));
            _camera.BeginAnimation(ProjectionCamera.PositionProperty, _point3DAnimation);
        }

        public void Next() => PageIndex++;

        public void Prev() => PageIndex--;

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta < 0)
            {
                var index = PageIndex + 1;
                PageIndex = index >= flowItems.Count ? Loop ? 0 : flowItems.Count - 1 : index;
            }
            else
            {
                var index = PageIndex - 1;
                PageIndex = index < 0 ? Loop ? flowItems.Count - 1 : 0 : index;
            }

            e.Handled = true;
        }



        private void Viewport3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var result = (RayMeshGeometry3DHitTestResult)VisualTreeHelper.HitTest(_viewport3D, e.GetPosition(_viewport3D));
            if (result != null)
            {
                foreach (var item in flowItems)
                {
                    if (item.HitTest(result.MeshHit))
                    {
                        PageIndex = item.Index;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     更新项的位置
        /// </summary>
        /// <param name="newIndex"></param>
        private void UpdateIndex(int newIndex)
        {
            if (!IsLoaded)
            {
                return;
            }

            //UpdateShowRange();

            //List<string> rMoveNames = new List<string>();
            //flowItems.Do(item =>
            //{
            //    if (item.Index < _firstShowIndex || item.Index > _lastShowIndex)
            //    {
            //        rMoveNames.Add(item.Name);
            //    }
            //    else
            //    {
            //        item.Move(PageIndex);
            //    }
            //});

            //// 删除要进行移除的对象
            //flowItems.RemoveAll(u => rMoveNames.Contains(u.Name));

            _itemShowDic.Do(item =>
            {
                if (item.Value.Index < _firstShowIndex || item.Value.Index > _lastShowIndex)
                {
                    if (!_itemShowDic.TryGetValue(item.Value.Index, out var items)) return;
                    _itemShowDic.Remove(item.Value.Index);
                    _visualParent.Children.Remove(items);
                }
                else
                {
                    item.Value.Move(PageIndex);
                }
            });


            _point3DAnimation = new Point3DAnimation(new Point3D(CoverFlowItem.Interval * newIndex, _camera.Position.Y,
                _camera.Position.Z), new Duration(TimeSpan.FromMilliseconds(200)));

            _camera.BeginAnimation(ProjectionCamera.PositionProperty, _point3DAnimation);
        }

        /// <summary>
        /// 更新显示范围
        /// </summary>
        private void UpdateShowRange()
        {

            var newFirstShowIndex = 0;
            var newLastShowIndex = 2;
            _itemShowDic.Clear();
            _visualParent.Children.Clear();
            for (var i = newFirstShowIndex; i <= newLastShowIndex; i++)
            {
                if (!_itemShowDic.ContainsKey(i))
                {
                    if (i <= flowItems.Count - 1)
                    {
                        var cover = flowItems[i] as CoverFlowItem;
                        _itemShowDic[i] = cover;
                        _visualParent.Children.Add(cover);
                    }

                }
            }

            _firstShowIndex = newFirstShowIndex;
            _lastShowIndex = newLastShowIndex;
            PageIndex = 1;
            UpdateIndex(PageIndex);

        }

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="brush"></param>
        public int AddItem(string name, Brush brush, Uri imgURI)
        {
            var sPanel = new System.Windows.Controls.StackPanel();

            var image = new Image
            {
                Source = BitmapFrame.Create(imgURI, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand)
            };

            Border border = new Border() { BorderBrush = brush, BorderThickness = new Thickness(1), Padding = new Thickness(5) };
            border.Child = sPanel;

            sPanel.Children.Add(new TextBlock() { Text = name, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
            sPanel.Children.Add(image);

            // 更新所有Item的index
            int index = 0;
            flowItems.ForEach(u =>
            {
                u.Index = index;
                index++;
            });

            var newItem = new CoverFlowItem(index, PageIndex, border)
            {
                Name = name
            };

            flowItems.Add(newItem);
            //_visualParent.Children.Add(newItem);
            UpdateShowRange();
            Count = flowItems.Count;
            return index;
        }

        /// <summary>
        /// 更新对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="brush"></param>
        public void Update(string name, Brush brush)
        {
            var flowItem = flowItems.FirstOrDefault(u => u.Name == name);
            if (flowItem != null)
            {
                flowItem.UpdateColor(brush);
            }
        }

        /// <summary>
        ///  删除指定位置的项
        /// </summary>
        /// <param name="pos"></param>
        public Brush Remove(string name)
        {
            Brush OutStationColor=Brushes.Black;
            var rItem = flowItems.FirstOrDefault(u => u.Name == name);
            if (rItem != null)
            {
                _visualParent.Children.Remove(rItem);
                OutStationColor=rItem.GetColor();
                flowItems.Remove(rItem);
              
            }
            return OutStationColor;
        }


        public void Clear()
        {
            flowItems.Clear();
            _itemShowDic.Clear();
            _visualParent.Children.Clear();
            UpdateShowRange();
        }
    }
}
