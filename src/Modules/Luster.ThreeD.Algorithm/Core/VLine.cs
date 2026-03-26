#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VLine
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Models
* 文 件 名:       VLine.cs
* 创建时间:       2021/12/17 11:38:26
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      e8e45df8-7cfb-4da7-8034-c54a72dc6026
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:38:26
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 线
    /// </summary>
    public class VLine : BaseGeometry, IDirection
    {
        /// <summary>
        /// 起始点
        /// </summary>
        [OutProperty("起点")]
        public VVector Start
        {
            get; set;
        }

        /// <summary>
        /// 结束点
        /// </summary>
        [OutProperty("终点")]
        public VVector End
        {
            get; set;
        }

        /// <summary>
        /// 线段中心点
        /// </summary>
        [OutProperty("中心点")]
        public VVector CenPoint
        {
            get
            {
                return new VVector((Start.X + End.X) / 2, (Start.Y + End.Y) / 2, (Start.Z + End.Z) / 2);
            }
        }

        /// <summary>
        /// 长度
        /// </summary>
        [OutProperty("长度")]
        public double Length { get; set; }

        /// <summary>
        /// 线的方向
        /// </summary>
        [OutProperty("方向")]
        public VDirection Direction { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VLine() : base(IntPtr.Zero) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="intPtr"></param>
        public VLine(IntPtr intPtr) : base(intPtr)
        {
            // 如果是通过ptr来构建的那么就要自动
            InitPropByPTR();
        }

        /// <summary>
        /// 通过数据构建，需要计算出完成的项目属性
        /// </summary>
        private void InitPropByPTR()
        {
            NativeAPI.GetLineStartAndEnd(LPtr, out var startP, out var endP, out var direction, out var len);
            Start = new VVector(startP);
            End = new VVector(endP);
            Direction = new VDirection(direction);
            Length = len;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public VLine(VVector p1, VVector p2)
        {
            NetAPI.GetLineByStartEndPoints(p1, p2, out string errMsg);
            if (!string.IsNullOrEmpty(errMsg))
            {
                throw new FriendlyException(errMsg);
            }
            InitPropByPTR();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public VLine(VVector p1, VDirection direct, double len = 1.0)
        {
            NativeAPI.GetLineSegmentByPosDir(p1.ToPoint(), direct.ToVector(), len, out lptr, out var errMsg);
            InitPropByPTR();
        }

        /// <summary>
        /// 对象渲染
        /// </summary>
        /// <param name="interactor"></param>
        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddLineActor(ID, LPtr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Line";
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        protected override void DisposeObj()
        {
            NativeAPI.DeleteObj(LPtr, NativeAPI.ClassType.Line);
        }

        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Line");
            if (Start != null && Start != End)
            {
                xRoot.Add(Start.ExportXml());
                xRoot.Add(End.ExportXml());
                xRoot.Add(Direction.ExportXml());
                xRoot.SetAttributeValue("Length", Length);
            }
            return xRoot;
        }

        public override void ParserXml(XElement xElement)
        {
            int n = 0;
            foreach (var item in xElement.Elements())
            {
                if (n == 0)
                {
                    Start = new VVector();
                    Start.ParserXml(item);
                }
                else if (n == 1)
                {
                    End = new VVector();
                    End.ParserXml(item);
                }
                else if(n == 2)
                {
                    Direction = new VDirection();
                    Direction.ParserXml(item);
                }
                n++;
            }
            xElement.GetAttribute("Length", (r) => Length = double.Parse(r));
            //if (n > 0)
               //NativeAPI.GetLineSegmentByStartEndPoints(Start.ToPoint(), End.ToPoint(), out lptr, out var msg);
        }
    }
}