#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ImageTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       ImageTool.cs
* 创建时间:       2022/6/10 10:44:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3f4950fe-5b8f-4216-b200-358cf8b7cc6a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/10 10:44:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    /// <summary>
    /// 图像帮助类
    /// </summary>
    public class ImageTool
    {
        /// <summary>
        /// 将图像数据转换到Byte数组
        /// </summary>
        /// <param name="image">目标图像</param>
        /// <returns>图像转化后的数组</returns>
        public static byte[] ImageToBytes(Image image)
        {
            var format = image.RawFormat;
            using (var ms = new MemoryStream())
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (format.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (format.Equals(ImageFormat.Bmp))
                {
                    image.Save(ms, ImageFormat.Bmp);
                }
                else if (format.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                else if (format.Equals(ImageFormat.Icon))
                {
                    image.Save(ms, ImageFormat.Icon);
                }

                var buffer = new byte[ms.Length];

                // Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// 将Byte数组数据转换为图像
        /// </summary>
        /// <param name="buffer">图像的字节流</param>
        /// <returns>数组转化后的图像</returns>
        public static Image BytesToImage(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                return Image.FromStream(ms);
            }
        }

        /// <summary>
        /// 图像深度拷贝，如果传入图片是Format8bppIndexed
        /// <para>格式，则将克隆返回灰度图</para>
        /// </summary>
        /// <param name="srcImage">原图片</param>
        /// <returns>深度拷贝后的图片</returns>
        public static Bitmap DeepCopy(Bitmap srcImage)
        {
            var dstImage = new Bitmap(srcImage.Width, srcImage.Height, srcImage.PixelFormat);
            var rect = new Rectangle(0, 0, srcImage.Width, srcImage.Height);
            var srcData = srcImage.LockBits(rect, ImageLockMode.ReadOnly, srcImage.PixelFormat);
            var dstData = dstImage.LockBits(rect, ImageLockMode.ReadOnly, dstImage.PixelFormat);
            var dataLength = srcData.Stride * srcImage.Height;
            var temp = new byte[dataLength];
            var srcPtr = srcData.Scan0;
            var dstPtr = dstData.Scan0;
            Marshal.Copy(srcPtr, temp, 0, dataLength);
            Marshal.Copy(temp, 0, dstPtr, dataLength);
            srcImage.UnlockBits(srcData);
            dstImage.UnlockBits(dstData);
            if (dstImage.PixelFormat != PixelFormat.Format8bppIndexed)
                return dstImage;

            SetBitmapGray(dstImage);
            return dstImage;
        }

        /// <summary>
        /// Load bitmap from file.
        /// </summary>
        /// <param name="fileName">图片全路径</param>
        /// <returns>从文件读取到的图像</returns>
        public static Bitmap FromFile(string fileName)
        {
            Bitmap loadedImage = null;
            FileStream stream = null;

            try
            {
                // read image to temporary memory stream
                stream = File.OpenRead(fileName);
                var memoryStream = new MemoryStream();
                var buffer = new byte[10000];
                while (true)
                {
                    var read = stream.Read(buffer, 0, 10000);
                    if (read == 0)
                        break;
                    memoryStream.Write(buffer, 0, read);
                }

                loadedImage = (Bitmap)Image.FromStream(memoryStream);
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }

            return loadedImage;
        }

        /// <summary>
        /// 图片灰度化
        /// </summary>
        /// <param name="image"></param>
        /// <exception cref="Exception"></exception>
        public static void SetBitmapGray(Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new Exception("Source image is not 8 bpp image.");

            // get palette
            var cp = image.Palette;

            // init palette
            for (var i = 0; i < 256; i++)
            {
                cp.Entries[i] = Color.FromArgb(i, i, i);
            }

            // set palette back
            image.Palette = cp;
        }

        #region 将树节点转换为图片
        private static int nHeight = 32;
        private static int nWidth = 200;
        private static int levelPadding = 20;
        // 字体
        private static Font nFont = new Font("微软雅黑", 12);
        private static Pen linePen = new Pen(Color.Blue);

        /// <summary>
        /// 基于通过树节点，来构建图片
        /// </summary>
        /// <param name="nodes"></param>
        public static Bitmap SaveNodesToImage(string title, int imgPadding, params LNode[] nodes)
        {
            linePen.DashPattern = new float[] { 2, 2 };

            // 1.计算图片需要多宽、的高度
            int width = nWidth;

            // 2.图片最大宽和最大高
            int maxW = 100;
            int maxH = 0;
            List<int> nodeWidths = new List<int>();
            List<int> nodeHeights = new List<int>();

            Image tmpImage = new Bitmap(100, 100);

            string titleText = $"LusterMotion {title}";
            SizeF titleSize = new SizeF();
            var titleFont = new Font("微软雅黑", 20);
            using (Graphics g = Graphics.FromImage(tmpImage))
            {
                foreach (var item in nodes)
                {
                    int height = 0;
                    item.Level = 0;
                    CalcuNodeSize(g, item, ref maxW, ref height);

                    // 记录节点的宽带
                    nodeWidths.Add(maxW);
                    nodeHeights.Add(height);
                }

                // 图片标题
                titleSize = g.MeasureString(titleText, titleFont);
            }

            // 添加间距信息
            maxW = nodeWidths.Sum() + imgPadding * (nodes.Length - 1) + imgPadding * 2;
            maxH = nodeHeights.Max() + imgPadding * 2 + (int)titleSize.Height + 20;

            // 创建图片，和画刷
            Bitmap bitmap = new Bitmap(maxW, maxH);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(255, 255, 255, 255));
                int left = imgPadding;
                g.DrawString(titleText, titleFont, Brushes.Red, (maxW - titleSize.Width) / 2, imgPadding);

                int startTop = imgPadding + (int)titleSize.Height + 10;

                for (int i = 0; i < nodes.Length; i++)
                {
                    int top = startTop;
                    var node = nodes[i];
                    int lineWidth = nodeWidths[i];
                    DrawNode(g, left, lineWidth, node, ref top);
                    left = left + lineWidth + imgPadding;
                }
            }

            return bitmap;
        }

        /// <summary>
        /// 计算图片的宽高
        /// </summary>
        /// <param name="node"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private static void CalcuNodeSize(Graphics g, LNode node, ref int capWidth, ref int h)
        {
            // 计算文字宽度
            var sizef = g.MeasureString(node.Text, nFont);
            int w = (int)sizef.Width + node.Level * levelPadding;
            if (capWidth < w)
            {
                capWidth = w + 1;
            }

            h += nHeight;

            if (node.Children.Count > 0)
            {

                foreach (var item in node.Children)
                {
                    if (item.Parent == null)
                    {
                        item.Parent = node;
                    }

                    item.Level = node.Level + 1;

                    CalcuNodeSize(g, item, ref capWidth, ref h);
                }
            }
        }

        /// <summary>
        /// 绘制节点信息
        /// </summary>
        /// <param name="g"></param>
        /// <param name="n"></param>
        /// <param name="startLeft"></param>
        /// <param name="top"></param>
        private static void DrawNode(Graphics g, int lineStart, int lineWidth, LNode n, ref int top)
        {
            // 1.绘制边框
            var rect = new Rectangle(lineStart, top, nWidth, nHeight);
            int nLeft = rect.Left + n.Level * levelPadding;
            var size = g.MeasureString(n.Text, nFont);

            // 2.绘制文字
            g.DrawString(n.Text, nFont, Brushes.Black, nLeft, rect.Top + (rect.Height - size.Height) / 2);
            n.Rect = new Rectangle(nLeft, top, nWidth, nHeight);

            // 2.绘制连接线
            if (n.Parent != null && !n.Parent.Rect.IsEmpty)
            {
                int sLeft = n.Parent.Rect.Left + 5;
                int sTop = n.Parent.Rect.Bottom;
                var p1 = new Point(sLeft, sTop);

                int cLeft = sLeft;
                int cTop = rect.Top + nHeight / 2;
                var p2 = new Point(cLeft, cTop);

                int eLeft = nLeft;
                int eTop = cTop;
                var p3 = new Point(eLeft, eTop);
                g.DrawLine(linePen, p1, p2);
                g.DrawLine(linePen, p2, p3);
            }

            // 3.绘制子节点
            if (n.Children.Count > 0)
            {
                for (int i = 0; i < n.Children.Count; i++)
                {
                    top += nHeight;
                    var subNode = n.Children[i];
                    DrawNode(g, lineStart, lineWidth, subNode, ref top);
                }
            }
        }
        #endregion
    }
}