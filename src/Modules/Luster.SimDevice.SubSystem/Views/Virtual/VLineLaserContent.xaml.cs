using Luster.SimDevice.EngineUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.SimDevice.SubSystem.Views.Virtual
{
    /// <summary>
    /// VIOContent.xaml 的交互逻辑
    /// </summary>
    public partial class VLineLaserContent
    {
        public VLineLaserContent()
        {
            InitializeComponent();
        }
        //鼠标起始位置
        private Point mouseStartPos;

        //滚动缩放
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //获取当前位置
            var position = e.GetPosition(cv_Image);
            var m = xMatrix.Value;
            //前进就放大，后退就缩小,大于20倍或者小于十分之一就不在缩放
            if (e.Delta > 0)
            {
                if (Math.Abs(m.M11) > 20)
                {
                    return;
                }
                m.ScaleAtPrepend(1.1, 1.1, position.X, position.Y);
            }
            else
            {
                if (Math.Abs(m.M11) < 0.1)
                {
                    return;
                }
                m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, position.X, position.Y);
            }
            xMatrix = new MatrixTransform(m);
            image_Pro.RenderTransform = xMatrix;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                var m = new Matrix();
                xMatrix = new MatrixTransform(m);
                image_Pro.RenderTransform = xMatrix;
            }
            var position = e.GetPosition(cv_Image);
            //按下的起始位置
            mouseStartPos = position;
            image_Pro.CaptureMouse();
        }

        //平移
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (image_Pro.IsMouseCaptured)
            {
                var curPos = e.GetPosition(cv_Image);
                var m = xMatrix.Value;
                //X方向不可以平移出视野
                if (curPos.X > cv_Image.ActualWidth - 5 || curPos.X <= 5)
                {
                    return;
                }
                //Y方向不可以平移出视野
                if (curPos.Y > cv_Image.ActualHeight - 5 || curPos.Y <= 5)
                {
                    return;
                }
                //基于左键获得的最新当前位置进行平移
                m.OffsetX += curPos.X - mouseStartPos.X;
                m.OffsetY += curPos.Y - mouseStartPos.Y;
                xMatrix = new MatrixTransform(m);
                image_Pro.RenderTransform = xMatrix;
                mouseStartPos = curPos;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image_Pro.ReleaseMouseCapture();
        }
    }
}
