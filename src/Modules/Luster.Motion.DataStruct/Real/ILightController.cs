#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ILight
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct._1.Real
* 文 件 名:       ILight.cs
* 创建时间:       2022/6/10 8:57:53
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5b0b321e-c3db-4ef0-a599-2fc4df60367d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/10 8:57:53
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    public interface ILightController
    {
        /// <summary>
        /// 光源通道
        /// </summary>
        int Channel { get; set; }

        /// <summary>
        /// 设置通道的光源亮度级数
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="intensity">光源亮度级数</param>
        void SetChannelAndVal(int channel, int intensity);

        /// <summary>
        ///  获取当前通道号的光源亮度级数
        /// </summary>
        /// <param name = "channel" > 通道号 </ param >
        /// < param name="intensity">光源亮度级数</param>
        void GetChannelIntensity(int channel, ref int intensity);

    }
}