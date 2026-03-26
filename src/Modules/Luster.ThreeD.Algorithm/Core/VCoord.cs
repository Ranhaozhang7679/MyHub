#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCoord
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VCoord.cs
* 创建时间:       2022/1/10 11:14:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      eca2a53d-adf6-4e0a-a147-43ec6e0073b1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/10 11:14:05
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Interfaces;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 坐标系
    /// </summary>
    public class VCoord : LDisposable, IXMLParser, IActor, ISave, IEmptyObj, ICoord3
    {
        /// <summary>
        /// 坐标系
        /// </summary>
        public double[] Matrix { get; set; }

        /// <summary>
        /// 轴心
        /// </summary>
        [OutProperty("轴心点")]
        public VVector Origin
        {
            get
            {
                return new VVector(Matrix[3], Matrix[7], Matrix[11]);
            }
        }

        /// <summary>
        /// X 轴坐标
        /// </summary>
        [OutProperty("X轴")]
        public VDirection XAxis
        {
            get
            {
                return new VDirection(Matrix[0], Matrix[4], Matrix[8]);
            }
        }

        /// <summary>
        /// Y 轴坐标
        /// </summary>
        [OutProperty("Y轴")]
        public VDirection YAxis
        {
            get
            {
                return new VDirection(Matrix[1], Matrix[5], Matrix[9]);
            }
        }

        /// <summary>
        /// Z 轴坐标
        /// </summary>
        [OutProperty("Z轴")]
        public VDirection ZAxis
        {
            get
            {
                return new VDirection(Matrix[2], Matrix[6], Matrix[10]);
            }
        }

        /// <summary>
        /// X面
        /// </summary>
        private VPlane _xPlane;

        [OutProperty("X面")]
        public VPlane PlaneX
        {
            get
            {
                if (_xPlane == null)
                    _xPlane = new VPlane(Origin, XAxis);

                return _xPlane;
            }
        }

        /// <summary>
        /// Y面
        /// </summary>
        private VPlane _yPlane;

        [OutProperty("Y面")]
        public VPlane PlaneY
        {
            get
            {
                if (_yPlane == null)
                    _yPlane = new VPlane(Origin, YAxis);

                return _yPlane;
            }
        }

        /// <summary>
        /// Z面
        /// </summary>
        private VPlane _zPlane;

        [OutProperty("Z面")]
        public VPlane PlaneZ
        {
            get
            {
                if (_zPlane == null)
                    _zPlane = new VPlane(Origin, ZAxis);

                return _zPlane;
            }
        }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public VCoord()
        {
            Matrix = new double[16]
            {
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                0,0,0,1
            };
        }

        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="ptr"></param>
        public VCoord(LCoord coord) : this()
        {
            var cMatrix = coord.matrix;
            Matrix = new double[16]
            {
                cMatrix[0],cMatrix[1],cMatrix[2],cMatrix[3],
                cMatrix[4],cMatrix[5],cMatrix[6],cMatrix[7],
                cMatrix[8],cMatrix[9],cMatrix[10],cMatrix[11],
                0,0,0,1
            };
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Coord");
            xRoot.SetAttributeValue("Matrix", string.Join(",", Matrix));

            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Matrix", (str) =>
            {
                Matrix = str.Split<double>(',');
            });
        }

        /// <summary>
        /// 转换为Coord
        /// </summary>
        /// <returns></returns>
        public LCoord ToCoord()
        {
            LCoord coord = new LCoord();
            coord.matrix = new double[16]
            {
                Matrix[0],Matrix[1],Matrix[2],Matrix[3],
                Matrix[4],Matrix[5],Matrix[6],Matrix[7],
                Matrix[8],Matrix[9],Matrix[10],Matrix[11],
                Matrix[12],Matrix[13],Matrix[14],Matrix[15]
            };

            return coord;
        }

        /// <summary>
        /// 交互窗口
        /// </summary>
        /// <param name="interactor"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddAxesActor(ID, ToCoord());
        }

        /// <summary>
        /// 获取逆矩阵
        /// </summary>
        /// <returns></returns>
        public double[] GetInvert()
        {
            return Matrix;
        }

        /// <summary>
        /// 获取和模板的变换关系
        /// </summary>
        /// <returns></returns>
        public LMatrix GetTransform(bool isInvert = false)
        {
            LMatrix lMatrix = new LMatrix(Matrix);

            // 如果有模板坐标系
            //if (Template != null)
            //{
            //    // 计算方式变换
            //    LMatrix src = new LMatrix(Template);

            //    lMatrix = src.Rotate(lMatrix, true);
            //}

            //// 如果是参考坐标系
            //if (RefCoord != null)
            //{
            //    LMatrix refMatrix = RefCoord.GetTransform(isInvert);
            //    lMatrix = refMatrix.Rotate(lMatrix, true);
            //}

            return lMatrix;
        }

        /// <summary>
        /// 矩阵
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Coord";
        }

        public override bool IsEmpty()
        {
            return Matrix == null || Matrix.Length == 0;
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        protected override void DisposeObj()
        {
            base.DisposeObj();
            if (_xPlane != null)
            {
                NativeAPI.DeleteObj(_xPlane.LPtr, NativeAPI.ClassType.Plane);
            }

            if (_yPlane != null)
            {
                NativeAPI.DeleteObj(_yPlane.LPtr, NativeAPI.ClassType.Plane);
            }

            if (_zPlane != null)
            {
                NativeAPI.DeleteObj(_zPlane.LPtr, NativeAPI.ClassType.Plane);
            }
        }

        public override object Clone()
        {
            return this;
        }

        #region 对象保存

        /// <summary>
        /// 旋转矩阵
        /// </summary>
        public override string[] SaveFormat => new string[] { ".calib" };

        /// <summary>
        /// 文件保存
        /// </summary>
        /// <param name="path"></param>
        public override void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < 4; i++)
                {
                    int start = i * 4;
                    sw.WriteLine($"{Matrix[start]},{Matrix[start + 1]},{Matrix[start + 2]},{Matrix[start + 3]}");
                }
            }
        }

        #endregion
    }
}