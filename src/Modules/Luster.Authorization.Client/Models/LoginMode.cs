using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Authorization.Client.Models
{
    public enum LoginMode
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("通用离线登录模式")]
        Offline,

        /// <summary>
        /// 真实设备
        /// </summary>
        [Description("FX在线刷卡模式")]
        FXCard,

    }

}
