#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TextContentItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Models
* 文 件 名:       TextContentItem.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567892
* 创建年份:       2026
************************************************************************************/

#endregion

using System.Windows;
using System.Windows.Media;

namespace Luster.Common.Assets.FloatingInfo.Models
{
    /// <summary>
    /// 文本内容项
    /// </summary>
    public class TextContentItem : ContentItem
    {
        /// <summary>
        /// 内容类型
        /// </summary>
        public override ContentType ContentType => ContentType.Text;

        /// <summary>
        /// 文本内容
        /// </summary>
        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        /// <summary>
        /// 字体大小
        /// </summary>
        private double _fontSize = 14;
        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        /// <summary>
        /// 字体粗细
        /// </summary>
        private FontWeight _fontWeight = FontWeights.Normal;
        public FontWeight FontWeight
        {
            get => _fontWeight;
            set => SetProperty(ref _fontWeight, value);
        }

        /// <summary>
        /// 文本对齐方式
        /// </summary>
        private TextAlignment _textAlignment = TextAlignment.Left;
        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set => SetProperty(ref _textAlignment, value);
        }

        /// <summary>
        /// 前景色
        /// </summary>
        private Brush _foreground = Brushes.Black;
        public Brush Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        /// <summary>
        /// 是否支持换行
        /// </summary>
        private bool _textWrapping = true;
        public bool TextWrapping
        {
            get => _textWrapping;
            set => SetProperty(ref _textWrapping, value);
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
    }
}
