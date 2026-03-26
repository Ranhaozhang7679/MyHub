#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LSurface
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm
* 文 件 名:       LSurface.cs
* 创建时间:       2021/11/15 18:49:52
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      45817e14-8401-452e-8c94-da29a2293540
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/15 18:49:52
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 三角网格数据
    /// </summary>
    public class VMesh : LDisposable, IActor, IReferenceObj
    {
        public VMesh(VMesh mesh)
        {
            lptr = mesh.lptr;
            IsReference = true;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ptr">指针</param>
        public VMesh(IntPtr ptr)
        {
            if (lptr != IntPtr.Zero)
            {
                NativeAPI.DeleteObj(lptr, NativeAPI.ClassType.MeshData);
            }

            lptr = ptr;
        }

        /// <summary>
        /// 读取网格数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>三角网格对象</returns>
        public static VMesh ReadStl(string filePath, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.ReadStl(filePath, out var lptr, out var msgPtr);

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
            }

            return new VMesh(lptr);
        }

        protected override void DisposeObj()
        {
            NativeAPI.DeleteObj(lptr, NativeAPI.ClassType.MeshData);
        }

        #region IActor对象

        /// <summary>
        /// 添加活动对象
        /// </summary>
        /// <param name="interactor"></param>
        public void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddStlActor(ID, LPtr);
        }

        #endregion
    }
}