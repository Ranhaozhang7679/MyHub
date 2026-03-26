#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FileSelector
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Controls
* 文 件 名:       FileSelector.cs
* 创建时间:       2021/11/11 11:20:37
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      a1a137ef-73f4-4b1f-bdc3-4191880949a1
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/11 11:20:37
* 修 改 人:		  luster
************************************************************************************/

#endregion

using HandyControl.Interactivity;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Luster.Common.Assets.ParamGrid
{
    /// <summary>
    /// 文件选择器
    /// </summary>
    public class FileSelector : Control
    {
        public FileSelector()
        {
            CommandBindings.Add(new CommandBinding(ControlCommands.Switch, SwitchFile));
        }

        /// <summary>
        /// 选择文件的方法
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        /// <exception cref="NotImplementedException"></exception>
        private void SwitchFile(object sender, ExecutedRoutedEventArgs e)
        {
            //if (!HasValue)
            //{
                var dialog = new OpenFileDialog
                {
                    RestoreDirectory = true,
                    DefaultExt = DefaultExt,
                    Filter = Filter
                };

                if (dialog.ShowDialog() == true)
                {
                    UriPath = new Uri(dialog.FileName, UriKind.RelativeOrAbsolute);
                }
            //}
            //else
            //{
            //    SetValue(UriPathProperty, default(Uri));
            //    SetValue(HasValueProperty, false);
            //    SetValue(PreviewBrushPropertyKey, default(Brush));
            //    SetCurrentValue(ToolTipProperty, default);
            //}
        }

        /// <summary>
        /// 更新Url信息
        /// </summary>
        /// <param name="path"></param>
        internal void UpdateUrl(Uri path)
        {
            string file = Path.GetFileNameWithoutExtension(path.LocalPath);

            int width = this.ActualWidth == 0 ? 150 : (int)(this.ActualWidth);
            int height = this.ActualHeight == 0 ? 32 : (int)(this.ActualHeight);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width - 10, height - 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            {
                var font = new System.Drawing.Font("微软雅黑", 9);
                var size = g.MeasureString(file, font);
                g.DrawString(file, font, new System.Drawing.SolidBrush(System.Drawing.Color.Black), new System.Drawing.PointF((bitmap.Width - size.Width) / 2, (bitmap.Height - size.Height) / 2));
            }

            var bmpSource = ConvertToBitmapSource(bitmap);
            var imgBrush = new ImageBrush(bmpSource) { Stretch = Stretch.Uniform };
            SetValue(PreviewBrushPropertyKey, imgBrush);
            SetValue(HasValueProperty, true);
            SetCurrentValue(ToolTipProperty, path.LocalPath);
        }

        public System.Windows.Media.Imaging.BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap btmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(btmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// 选择的路径
        /// </summary>
        public Uri UriPath
        {
            get { return (Uri)GetValue(UriPathProperty); }
            set => SetValue(UriPathProperty, value);
        }

        // Using a DependencyProperty as the backing store for UriPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriPathProperty =
            DependencyProperty.Register("UriPath", typeof(Uri), typeof(FileSelector), new PropertyMetadata(default, new PropertyChangedCallback(UriPropertyChangeCallback)));

        private static void UriPropertyChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileSelector fileSelector = (FileSelector)d;
            if (fileSelector != null)
            {
                if (e.OldValue != e.NewValue && e.NewValue != null)
                    fileSelector.UpdateUrl(e.NewValue as Uri);
            }
        }

        /// <summary>
        /// 选择的文件后缀（"(.jpg)|*.jpg|(.png)|*.png"）
        /// </summary>
        public string DefaultExt
        {
            get { return (string)GetValue(DefaultExtProperty); }
            set { SetValue(DefaultExtProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultExt.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultExtProperty =
            DependencyProperty.Register("DefaultExt", typeof(string), typeof(FileSelector), new PropertyMetadata(".txt"));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
           "Stretch", typeof(Stretch), typeof(FileSelector), new PropertyMetadata(default(Stretch)));

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(FileSelector), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
            "StrokeDashArray", typeof(DoubleCollection), typeof(FileSelector), new FrameworkPropertyMetadata(default(DoubleCollection), FrameworkPropertyMetadataOptions.AffectsRender));

        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        public static readonly DependencyPropertyKey PreviewBrushPropertyKey = DependencyProperty.RegisterReadOnly(
            "PreviewBrush", typeof(Brush), typeof(FileSelector), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty PreviewBrushProperty = PreviewBrushPropertyKey.DependencyProperty;

        public Brush PreviewBrush
        {
            get => (Brush)GetValue(PreviewBrushProperty);
            set => SetValue(PreviewBrushProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(FileSelector), new PropertyMetadata("(.jpg)|*.jpg|(.png)|*.png"));

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        /// <summary>
        /// 是否已经有文件
        /// </summary>
        public bool HasValue
        {
            get { return (bool)GetValue(HasValueProperty); }
            set { SetValue(HasValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasValueProperty =
            DependencyProperty.Register("HasValue", typeof(bool), typeof(FileSelector), new PropertyMetadata(false));
    }
}