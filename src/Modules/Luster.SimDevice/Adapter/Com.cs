#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Com
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Adapter
* 文 件 名:       Com.cs
* 创建时间:       2022/4/11 15:23:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      aa708605-398e-41f8-b33c-a9d0fa6aca75
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 15:23:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Adapter
{

    /// <summary>
    /// Com口
    /// </summary>
    public class Com : AdapterBase
    {
        /// <summary>
        /// COM口名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 数据位
        /// </summary>
        public int Databits { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// 奇偶校验
        /// </summary>
        public Parity Parity { get; set; }

        public static List<string> GetComs()
        {
            List<string> coms = new List<string>();
            //获取本地配置文件(注册表文件)        
            Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
            //读取HARDWARE节点的键值
            Microsoft.Win32.RegistryKey software11 = hklm.OpenSubKey("HARDWARE");
            //打开"HARDWARE"子键
            Microsoft.Win32.RegistryKey software = software11.OpenSubKey("DEVICEMAP");
            Microsoft.Win32.RegistryKey sitekey = software.OpenSubKey("SERIALCOMM");

            if (sitekey == null)
            {
                return coms;
            }

            //获取当前子键
            string[] Str2 = sitekey.GetValueNames();

            //获得当前子键存在的键值
            int i;
            for (i = 0; i < Str2.Count(); i++)
            {
                coms.Add(sitekey.GetValue(Str2[i]).ToString());
            }

            return coms;
        }

        public override string GetMethod()
        {
            return Name;
        }

        public override void SetMethod(string method)
        {
            Name = method;
        }
    }
}