#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlignmentAPI
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.API
* 文 件 名:       AlignmentAPI.cs
* 创建时间:       2021/11/24 8:55:43
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      821ce91e-5a5e-46bb-a54e-75137582e2db
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/24 8:55:43
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.ThreeD.Algorithm.Enums;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Luster.ThreeD.Algorithm.Core;

namespace Luster.ThreeD.Algorithm.API
{
    public class AlignmentAPI : IDisposable
    {
        /// <summary>
        /// 网格数据
        /// </summary>
        private VMesh triMesh;

        /// <summary>
        /// 对其函数的指针
        /// </summary>
        private IntPtr alignPtr = IntPtr.Zero;

        /// <summary>
        /// 对其构造函数
        /// </summary>
        /// <param name="mesh">网格数据</param>
        public AlignmentAPI(VMesh mesh)
        {
            triMesh = mesh;
        }

        /// <summary>
        /// 模型训练
        /// </summary>
        /// <returns></returns>
        public void Train(ShapeComplexityType shapeComplexityType, SearchTimeType searchTime, bool invertNormals, bool bestFit, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.InitAlignTrain(triMesh.LPtr, (int)shapeComplexityType, (int)searchTime, invertNormals, bestFit, out alignPtr, out var msgPtr);

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
            }
        }

        /// <summary>
        /// 初始对其
        /// </summary>
        /// <param name="pCloud"></param>
        /// <returns></returns>
        public void InitExceute(VCloud pCloud, out VTransform transform, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.InitAlignExcute(alignPtr, pCloud.LPtr, out double rms, out var matrix, out var msgPtr);
            //解决位置显示错误
            NativeAPI.MatrixCross(ref pCloud.Matrix.ToArray()[0], ref matrix.ToArray()[0], out var tempMatrix);
            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
            }
            pCloud.Matrix = tempMatrix;
            // 给矩阵赋值
            transform = new VTransform(tempMatrix);
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            if (alignPtr != IntPtr.Zero)
                NativeAPI.DeleteObj(alignPtr, NativeAPI.ClassType.InitAlign);
        }
    }
}