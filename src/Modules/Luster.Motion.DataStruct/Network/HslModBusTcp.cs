#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModbusProtocol
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Network
* 文 件 名:       ModbusProtocol.cs
* 创建时间:       2022/7/22 19:40:13
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6c0faa05-a2cf-41ac-b6e8-c92c2935102a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/22 19:40:13
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HslCommunication;

namespace Luster.Motion.DataStruct.Network
{
   
    public abstract class HslModBusTcp : IProtocol
    {
        public ManualResetEventSlim PauseReset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public  event Action<LogType, string> LogEvent;

        public abstract CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout);


        public abstract CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg);


        public abstract void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable;
       
    }
    public class HslMdobus : IProtocol
    {
        public ManualResetEventSlim PauseReset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<LogType, string> LogEvent;

        public CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            throw new NotImplementedException();
        }

        public void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable
        {
            throw new NotImplementedException();
        }

        public CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg)
        {
            throw new NotImplementedException();
        }
    }
}