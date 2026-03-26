#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ImageContentItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Models
* 文 件 名:       ImageContentItem.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567893
* 创建年份:       2026
************************************************************************************/

#endregion

using System.Windows;
using System.Windows.Media;

namespace Luster.Common.Assets.FloatingInfo.Models
{
    /// <summary>
    /// 图片内容项
    /// </summary>
    public class ImageContentItem : ContentItem
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        public override ContentType ContentType => ContentType.Image;

        /// <summary>
        /// 图片路径（支持本地路径和网络路径）
        /// </summary>
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                SetProperty(ref _imagePath, value);
                // 当路径改变时，尝试加载图片
                if (!string.IsNullOrEmpty(value))
                {
                    LoadImage();
                }
            }
        }

        /// <summary>
        /// 图片源
        /// </summary>
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        /// <summary>
        /// 最大宽度
        /// </summary>
        private double _maxWidth = 400;
        public double MaxWidth
        {
            get => _maxWidth;
            set => SetProperty(ref _maxWidth, value);
        }

        /// <summary>
        /// 最大高度
        /// </summary>
        private double _maxHeight = 300;
        public double MaxHeight
        {
            get => _maxHeight;
            set => SetProperty(ref _maxHeight, value);
        }

        /// <summary>
        /// 拉伸模式
        /// </summary>
        private Stretch _stretch = Stretch.Uniform;
        public Stretch Stretch
        {
            get => _stretch;
            set => SetProperty(ref _stretch, value);
        }

        /// <summary>
        /// 边距
        /// </summary>
        private Thickness _margin = new Thickness(5);
        public Thickness Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        private void LoadImage()
        {
            try
            {
                if (string.IsNullOrEmpty(_imagePath))
                {
                    ImageSource = null;
                    return;
                }

                // 创建位图图像
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.UriSource = new System.Uri(_imagePath, System.UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                bitmap.Freeze();
                ImageSource = bitmap;
            }
            catch
            {
                // 图片加载失败时设置为null
                ImageSource = null;
            }
        }
    }
}
