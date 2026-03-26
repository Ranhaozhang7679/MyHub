#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TTFInfo
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       TTFInfo.cs
* 创建时间:       2022/5/24 13:09:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      718e9d8a-b8f1-4b77-8a94-def5f4338fdb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 13:09:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Luster.Control.Wpf.Motion.Flow
{
    public class TTFInfo
    {
        public FileInfo TTFFileInfo { get; private set; }

        private GlyphTypeface GlyphTypeface { get; set; }

        public Size Bounds { get; private set; }

        public string ErrorMessage { get; private set; }

        public TTFInfo(string file)
        {
            //this.TTFFileInfo = new FileInfo(file);
            this.GlyphTypeface = new GlyphTypeface(new Uri(file, UriKind.Absolute));
        }

        public TTFInfo(FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            this.TTFFileInfo = null;
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            GlyphTypeface glyphTypeface = null;
            if (typeface.TryGetGlyphTypeface(out glyphTypeface))
            {
                this.GlyphTypeface = glyphTypeface;
            }

            if (this.GlyphTypeface == null)
            {
                var missfont = Application.Current.MainWindow.FontFamily;
                typeface = new Typeface(missfont, fontStyle, fontWeight, fontStretch);
                if (typeface.TryGetGlyphTypeface(out glyphTypeface))
                {
                    this.GlyphTypeface = glyphTypeface;
                }

                this.ErrorMessage = "当前系统无此字体";
            }
        }

        public ImageSource GetStrImage(string str, double fontsize, Brush foreBrush = null)
        {
            if (str != null)
            {
                var strs = ConverterText(str);
                if (str.Length > 0 && this.GlyphTypeface != null)
                {
                    DrawingGroup dg = new DrawingGroup();
                    double width = 0;
                    double height = 0;
                    foreach (var text in strs)
                    {
                        int iconLen = text.Length;
                        if (iconLen > 1)
                        {
                            iconLen = 1;
                        }
                        var glyphIndexes = new ushort[iconLen];
                        var advanceWidths = new double[iconLen];
                        for (int n = 0; n < text.Length; n++)
                        {
                            int key = text[n];
                            if (this.GlyphTypeface.CharacterToGlyphMap.ContainsKey(key))
                            {
                                var glyphIndex = this.GlyphTypeface.CharacterToGlyphMap[key];
                                glyphIndexes[n] = glyphIndex;
                                advanceWidths[n] = this.GlyphTypeface.AdvanceWidths[glyphIndex] * 1.0;
                            }
                        }

                        if (text.Length > 1)
                        {
                            glyphIndexes[0] = 117;
                            advanceWidths[0] = 1;
                        }

                        var gr = new GlyphRun(this.GlyphTypeface, 10, false, 1.0, glyphIndexes, new Point(0, 0), advanceWidths, null, null, null, null, null, null);
                        var glyphRunDrawing = new GlyphRunDrawing(foreBrush ?? Brushes.White, gr);
                        var w = glyphRunDrawing.Bounds.Width * fontsize;
                        var h = glyphRunDrawing.Bounds.Height * fontsize;

                        if (w > 0 && h > 0)
                        {
                            ImageDrawing dring = new ImageDrawing(new DrawingImage(glyphRunDrawing), new Rect(new Point(width, height), new Size(w, h)));
                            dg.Children.Add(dring);
                        }

                        width += w;
                        height += h;
                    }
                    Bounds = new Size(width, height);
                    return new DrawingImage(dg);
                }
            }

            return null;
        }

        public List<string> ConverterText(string text)
        {
            var t = text.Replace("&amp;", "&");
            t = t.Replace("\t", "   ");
            return t.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
        }
    }
}