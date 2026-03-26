#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IImage
* 机器名称:       L05123-02
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IImage.cs
* 创建时间:       2023/1/28 16:15:46
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ed535a92-0aec-4238-a80e-2c31234036e3
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/1/28 16:15:46
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 图像对象
    /// </summary>
    public interface IImage : IDisposable
    {
        /// <summary>
        /// 图像指针
        /// </summary>
        IntPtr Data { get; }

        /// <summary>
        /// 宽
        /// </summary>
        int Width { get; }

        /// <summary>
        /// 高
        /// </summary>
        int Height { get; }

        /// <summary>
        /// 通道
        /// </summary>
        int Channel { get; }

        /// <summary>
        /// 8 / 16 
        /// </summary>
        int BytesPerPixel { get; }

        /// <summary>
        /// 获取像素接口
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        void GetPixel(int x, int y, out byte red, out byte green, out byte blue, out byte alpha);

        /// <summary>
        /// 创建图像
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="channels"></param>
        /// <param name="bytesPerPixel"></param>
        /// <returns></returns>
        IImage Create(IntPtr ptr, int width, int height, int channels, int bytesPerPixel);

        /// <summary>
        /// 图片完整流，包含的图像基础信息的流
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IImage LoadStream(Stream stream);

        /// <summary>
        /// 通过文件加载
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        IImage LoadFile(string filename);

        /// <summary>
        /// 保存图像
        /// </summary>
        /// <param name="filename"></param>
        void Save(string filename);
    }
}