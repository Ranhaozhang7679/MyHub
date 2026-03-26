#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ImageModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       ImageModule.cs
* 创建时间:       2022/9/2 10:00:17
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7c063a79-b5ce-442c-8204-6770f0a8eb02
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/2 10:00:17
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.SubSystem.Models
{
    public class ImageModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush _brush;
        public Brush Brush
        {
            get { return _brush; }
            set { SetProperty(ref _brush, value); }
        }

        /// <summary>
        /// 图像
        /// </summary>
        private Uri _uri;
        public Uri ImgURI
        {
            get { return _uri; }
            set { SetProperty(ref _uri, value); }
        }

    }
}