#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MatchAPI
* 机器名称:       L05014-NB
* 命名空间:       Luster.ThreeD.Algorithm.API
* 文 件 名:       MatchAPI.cs
* 创建时间:       2023/3/8 14:35:59
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      47f059b2-0131-4af5-a8dd-178519fe3cb7
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/3/8 14:35:59
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.API
{
    /// <summary>
    /// 点云对其匹配句柄
    /// </summary>
    public class MatchAPI : IDisposable
    {

        /// <summary>
        /// 模型点云数据
        /// </summary>
        private VCloud modCloud;

        /// <summary>
        /// 对其匹配函数的指针
        /// </summary>
        private IntPtr matchPtr = IntPtr.Zero;

        public MatchAPI(VCloud vCloud)
        {
            modCloud = vCloud;
        }

        /// <summary>
        /// 训练点云模型
        /// </summary>
        public void Train(int iMaxResNum, double dMinScore, double dOverlapRate, double dRelDiameter, int iFeaturePointMode, double dEdgeSigMulScale, int iKnnSeaNum, double dModRefDisSca, double dSenceRefDisSca, double dSenceSampleScale, out string mes)
        {
            mes = "";
            int result = NativeAPI.ModelMatchTrain(modCloud.LPtr, iMaxResNum, dMinScore, dOverlapRate, dRelDiameter, iFeaturePointMode, dEdgeSigMulScale, iKnnSeaNum, dModRefDisSca, dSenceRefDisSca, dSenceSampleScale, out matchPtr, out IntPtr errMes);
            if (result != 0)
            {
                mes = Marshal.PtrToStringUni(errMes);
            }
        }

        public void Excute(IntPtr pSenCloud, out LMatrix outMatrix, out string errMsg)
        {
            errMsg = "";
            if (matchPtr==IntPtr.Zero)
            {
                errMsg = "对其匹配训练时失败";
            }
            int result = NativeAPI.ModelMatchExcute(matchPtr, pSenCloud, out outMatrix, out IntPtr errMes);
            if (result != 0)
            {
                errMsg = Marshal.PtrToStringUni(errMes);
            }
        }

        public void Dispose()
        {
            if (matchPtr != IntPtr.Zero)
                NativeAPI.DeleteObj(matchPtr, NativeAPI.ClassType.ModelMatch);
        }
    }
}