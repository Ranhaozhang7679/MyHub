#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Station
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       Station.cs
* 创建时间:       2022/5/30 8:50:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      88c51280-b8cb-42ce-9f4e-7af2cccd305b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 8:50:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Camera.Functions
{
    public class AreaCamera : MotionFunction
    {
        [Parameter("相机配置", 0, CN = "面阵相机", EditorType = typeof(VCamera))]
        public VDevice Camera { get; set; }

        /// <summary>
        /// 输出图像
        /// </summary>
        [Parameter("相机拍照后得到的图像", 10, CN = "图像", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public LImage OutImage { get; set; }

        /// <summary>
        /// 是否已经获取到图像
        /// </summary>
        private bool isGetImage = false;

        /// <summary>
        /// 面阵相机
        /// </summary>
        public AreaCamera()
        {
            this.Icon = "\xe661";
            this.Tips = "面阵相机!";
        }

        /// <summary>
        /// 程序执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCamera>(Camera, out var vCamera);

            vCamera.OpenCamera();
            
            vCamera.FrameImageEvent += VCamera_FrameImageEvent;

            // 拍照
            vCamera.OnCamera(false);
            WaitFunc(() => isGetImage, "图像取流中");

            isGetImage = false;
            vCamera.FrameImageEvent -= VCamera_FrameImageEvent;
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 相机取图回调
        /// </summary>
        /// <param name="obj"></param>
        private void VCamera_FrameImageEvent(LImage obj)
        {
            OutImage=obj;
            //SetParameterVal(nameof(OutImage), obj);
            isGetImage = true;
        }

        /// <summary>
        /// 相机对象释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GetVDevice<VCamera>(Camera, out var vCamera);
            // 避免为空的时候删除失败
            if (vCamera != null)
            {
                vCamera.FrameImageEvent -= VCamera_FrameImageEvent;
                vCamera.CloseCamera();
            }
        }
    }
}