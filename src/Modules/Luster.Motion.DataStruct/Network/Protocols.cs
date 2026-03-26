#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StringProtocol
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Network
* 文 件 名:       StringProtocol.cs
* 创建时间:       2022/7/20 17:37:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      811f2e2d-e526-48d3-bc1c-bfa348255406
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 17:37:43
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Network
{
    /// <summary>
    /// Default
    /// </summary>
    public class StringProtocol : IProtocol
    {
        public ManualResetEventSlim PauseReset { get; set; }

        /// <summary>
        /// log事件
        /// </summary>
        public event Action<LogType, string> LogEvent;

        public CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            CommResult<T> commResult = new CommResult<T>(true, "");
            if (typeof(T) != typeof(string))
            {
                throw new NotSupportedException(typeof(T).Name);
            }

            if (communication != null)
            {
                try
                {
                    List<byte> result = communication.ReceiveToEnd(communication.tokenSource, PauseReset, timeout);
                    string strResult = Encoding.Default.GetString(result.ToArray());
                    T t = (T)Convert.ChangeType(strResult, typeof(string));
                    commResult.Datas = new List<T>() { t };
                }
                catch (Exception ex)
                {
                    commResult.ErrorMsg = ex.Message;
                    commResult.IsSuccess = false;
                }
            }

            return commResult;
        }

        public void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable
        {
            CommResult<T> commResult = new CommResult<T>(true, "");
            if (typeof(T) != typeof(string))
            {
                throw new NotSupportedException(typeof(T).Name);
            }

            if (communication != null)
            {
                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        LogEvent?.Invoke(LogType.Debug, "通信中断!");
                        break;
                    }

                    List<byte> result = communication.ReceiveToEnd(communication.tokenSource, PauseReset, timeout);
                    string strResult = Encoding.Default.GetString(result.ToArray());
                    T t = (T)Convert.ChangeType(strResult, typeof(string));
                    if (t.Equals(val))
                    {
                        break;
                    }

                    Thread.Sleep(sleep);
                }
            }
        }

        public CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg)
        {
            CommResult commResult = new CommResult(true);
            if (typeof(T) != typeof(string))
            {
                throw new NotSupportedException(typeof(T).Name);
            }

            if (data.Count > 0)
            {
                var msg = Encoding.Default.GetBytes(data[0].ToString());
                communication.Send(msg.ToList());
            }

            return commResult;
        }
    }

    /// <summary>
    /// Utf8
    /// </summary>
    public class StringUtf8Protocol : IProtocol
    {
        public ManualResetEventSlim PauseReset { get; set; }

        public event Action<LogType, string> LogEvent;

        public byte[] Parse(string msg)
        {
            return Encoding.UTF8.GetBytes(msg);
        }
        public string Parse(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            CommResult<T> commResult = new CommResult<T>(true, "");
            if (typeof(T) != typeof(string))
            {
                throw new NotSupportedException(typeof(T).Name);
            }

            if (communication != null)
            {
                try
                {
                    List<byte> result = communication.ReceiveToEnd(communication.tokenSource, PauseReset, timeout);
                    string strResult = Encoding.UTF8.GetString(result.ToArray());
                    T t = (T)Convert.ChangeType(strResult, typeof(string));
                    commResult.Datas = new List<T>() { t };
                }
                catch (Exception ex)
                {
                    commResult.ErrorMsg = ex.Message;
                    commResult.IsSuccess = false;
                }
            }

            return commResult;
        }

        public void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable
        {
            throw new NotImplementedException();
        }

        public CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg)
        {
            CommResult commResult = new CommResult(true);
            if (typeof(T) != typeof(string))
            {
                throw new NotSupportedException(typeof(T).Name);
            }

            if (data.Count > 0)
            {
                var msg = Encoding.UTF8.GetBytes(data[0].ToString());
                communication.Send(msg.ToList());
            }

            return commResult;
        }
    }

    /// <summary>
    /// 将16进制字符串转换为byte
    /// </summary>
    //public class StringHexProtocol : IProtocol
    //{
    //    public byte[] Parse(string hexString)
    //    {
    //        hexString = hexString.Replace(" ", "");
    //        if (hexString.Length % 2 != 0)
    //        {
    //            throw new ArgumentException("参数长度不正确");
    //        }

    //        byte[] returnBytes = new byte[hexString.Length / 2];
    //        for (int i = 0; i < returnBytes.Length; i++)
    //        {
    //            returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
    //        }

    //        return returnBytes;
    //    }

    //    public string Parse(byte[] data)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}