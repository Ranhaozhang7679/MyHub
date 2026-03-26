#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VLineLaserModel
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       VLineLaserModel.cs
* 创建时间:       2022/9/30 13:46:55
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      74966481-5185-47cd-8ade-2cafa4a59eaa
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/30 13:46:55
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class VLineLaserModel :  BindableBase
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 激光名称
        /// </summary>
        private string _laserName;
        public string LaserName
        {
            get => _laserName;
            set => SetProperty(ref _laserName, value);
        }

        /// <summary>
        /// 线激光序列号
        /// </summary>
        public string _laserSN;
        public string LaserSN
        {
            get => _laserSN;
            set => SetProperty(ref _laserSN, value);
        }

        /// <summary>
        /// 隶属模组
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module;
            set { SetProperty(ref _module, value); }
        }

        /// <summary>
        /// 离线点云路径
        /// </summary>
        private string _pointCloudPath;
        public string PointCloudPath
        {
            get => _pointCloudPath;
            set
            {
                SetProperty(ref _pointCloudPath, value);
                ImagePathChanged();
            }
        }

        /// <summary>
        /// 离线点云路径
        /// </summary>
        private string _fBrand;
        public string FBrand
        {
            get => _fBrand;
            set
            {
                SetProperty(ref _fBrand, value);
               
            }
        }

        private void ImagePathChanged()
        {
            if (_lineLaser != null)
            {
                _lineLaser.LineLaserPath = new LPath(PointCloudPath);
            }
        }

        private VLineLaser _lineLaser;
        public VLineLaserModel( VLineLaser vLineLaser)
        {
            _lineLaser = vLineLaser;
            FBrand = vLineLaser.GetDevice().Brand;
            Module = vLineLaser.Module;
            ID = vLineLaser.ID;
            Name = vLineLaser.Name;
            LaserSN = vLineLaser.LaserSN;
            PointCloudPath = vLineLaser.LineLaserPath.Path ?? string.Empty;
        }
    }

    public enum LaserTriggerMode
    { 
        [Description("时间模式")]
        Time=0,
        [Description("编码器模式")]
        Encode =1,
        [Description("硬件IO模式")]
        IO =2,
    }
}