#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Network
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Adapter
* 文 件 名:       Network.cs
* 创建时间:       2022/4/11 15:24:19
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2ffd97d7-5575-431a-b467-6425ed510870
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 15:24:19
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Adapter
{

    /// <summary>
    /// 通过网络进行连接
    /// </summary>
    public class Network : AdapterBase
    {
        /// <summary>
        /// ip地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        /// <returns></returns>
        public override string GetMethod()
        {
            return $"{Ip}:{Port}";
        }

        public override void SetMethod(string address)
        {
            var temps = address.Split(':');
            if (temps.Length == 2)
            {
                Ip = temps[0];
                Port = int.Parse(temps[1]);
            }
        }
    }
}