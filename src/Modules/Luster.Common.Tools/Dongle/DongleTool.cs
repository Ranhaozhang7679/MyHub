#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SlmTool
* 机器名称:       L05123-NB
* 命名空间:       SlmRuntimeCSharp
* 文 件 名:       SlmTool.cs
* 创建时间:       2022/9/14 19:50:37
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      0479ca8d-0d6e-47d9-9900-29b302c838eb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/14 19:50:37
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SLM_HANDLE_INDEX = System.UInt32;

namespace Luster.Common.Dongle
{
    /// <summary>
    /// 加密狗驱动Demo
    /// </summary>
    public static class DongleTool
    {
        /// <summary>
        /// 开发者ID
        /// </summary>
        private const int DEVELOPER_ID_LENGTH = 8;

        /// <summary>
        /// 设备SN长度
        /// </summary>
        private const int DEVICE_SN_LENGTH = 16;

        /// <summary>
        /// 证书
        /// </summary>
        private const int LicenseID = 300;

        /// <summary>
        /// 开发者权限
        /// </summary>
        private const string APIKey = "A676E762F324205AE6AE657BB42A56F7";

        // 回调事件需要定义为类成员变量，若定义在函数中，当函数执行完毕时对象被回收，当收到服务回调通知时程序会异常终止。
        // 回调通知生效的时间周期：slm_init - slm_cleanup 期间，当执行清理操作后回调函数将无法收到任何服务通知消息。
        private static callback pfn;

        /// <summary>
        /// 错误消息
        /// </summary>
        public static Action<string> DongleError;

        /// <summary>
        /// 回调函数信息提示
        /// </summary>
        /// <param name="message"></param>
        /// <param name="wparam"></param>
        /// <param name="lparam"></param>
        /// <returns></returns>
        private static uint handle_service_msg(uint message, UIntPtr wparam, UIntPtr lparam)
        {
            uint ret = SSErrCode.SS_OK;
            string StrMsg = string.Empty;
            char[] szmsg = new char[1024];
            byte[] lock_sn_bytes = new byte[DEVICE_SN_LENGTH];
            string lock_sn = "";

            switch (message)
            {
                case SSDefine.SS_ANTI_INFORMATION:   // 信息提示
                    StrMsg = string.Format("SS_ANTI_INFORMATION is:0x{0:X8} wparam is %p", message, wparam);
                    break;
                case SSDefine.SS_ANTI_WARNING:       // 警告
                    // 反调试检查。一旦发现如下消息，建议立即停止程序正常业务，防止程序被黑客调试。

                    switch ((uint)(wparam))
                    {
                        case SSDefine.SS_ANTI_PATCH_INJECT:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "注入", message, wparam);
                            break;
                        case SSDefine.SS_ANTI_MODULE_INVALID:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "非法模块DLL", message, wparam);
                            break;
                        case SSDefine.SS_ANTI_ATTACH_FOUND:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "附加调试", message, wparam);
                            break;
                        case SSDefine.SS_ANTI_THREAD_INVALID:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "线程非法", message, wparam);
                            break;
                        case SSDefine.SS_ANTI_THREAD_ERROR:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "线程错误", message, wparam);
                            break;
                        case SSDefine.SS_ANTI_CRC_ERROR:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "内存模块 CRC 校验", message, wparam);
                            break;
                        case SSDefine.SS_ANTI_DEBUGGER_FOUND:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "发现调试器", message, wparam);
                            break;
                        default:
                            StrMsg = string.Format("信息类型=:0x{0:X8} 具体错误码= 0x{0:X8}", "其他未知错误", message, wparam);
                            break;
                    }
                    break;
                case SSDefine.SS_ANTI_EXCEPTION:         // 异常
                    StrMsg = string.Format("SS_ANTI_EXCEPTION is :0x{0:X8} wparam is %p", message, wparam);
                    break;
                case SSDefine.SS_ANTI_IDLE:              // 暂保留
                    StrMsg = string.Format("SS_ANTI_IDLE is :0x{0:X8} wparam is %p", message, wparam);
                    break;
                case SSDefine.SS_MSG_SERVICE_START:      // 服务启动
                    StrMsg = string.Format("SS_MSG_SERVICE_START is :0x{0:X8} wparam is %p", message, wparam);
                    break;
                case SSDefine.SS_MSG_SERVICE_STOP:       // 服务停止
                    StrMsg = string.Format("SS_MSG_SERVICE_STOP is :0x{0:X8} wparam is %p", message, wparam);
                    break;
                case SSDefine.SS_MSG_LOCK_AVAILABLE:
                    // 锁可用（插入锁或SS启动时锁已初始化完成），wparam 代表锁号
                    // 锁插入消息，可以根据锁号查询锁内许可信息，实现自动登录软件等功能。
                    Marshal.Copy((IntPtr)(long)wparam, lock_sn_bytes, 0, DEVICE_SN_LENGTH);
                    lock_sn = BitConverter.ToString(lock_sn_bytes).Replace("-", "");
                    StrMsg = string.Format("{0},{1:x8}锁插入", DateTime.Now.ToString(), lock_sn);

                    break;
                case SSDefine.SS_MSG_LOCK_UNAVAILABLE:   // 锁无效（锁已拔出），wparam 代表锁号
                    // 锁拔出消息，对于只使用锁的应用程序，一旦加密锁拔出软件将无法继续使用，建议发现此消息提示用户保存数据，程序功能锁定等操作。
                    Marshal.Copy((IntPtr)(long)wparam, lock_sn_bytes, 0, DEVICE_SN_LENGTH);
                    lock_sn = BitConverter.ToString(lock_sn_bytes).Replace("-", "");

                    StrMsg = string.Format("{0},{1:x8}锁拔出", DateTime.Now.ToString(), lock_sn);
                    break;
            }

            if (!string.IsNullOrEmpty(StrMsg))
            {
                DongleError?.Invoke(StrMsg);
            }

            // 输出格式化后的消息内容
            return ret;
        }

        /// <summary>
        /// 将字符串转换为16进制
        /// </summary>
        /// <param name="HexString"></param>
        /// <returns></returns>
        private static byte[] StringToHex(string HexString)
        {
            byte[] returnBytes = new byte[HexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(HexString.Substring(i * 2, 2), 16);

            return returnBytes;
        }

        /// <summary>
        /// 通用函数处理
        /// </summary>
        /// <param name="func"></param>
        /// <param name="errMsg"></param>
        /// <exception cref="Exception"></exception>
        private static void Process(Func<uint> func, string errMsg)
        {
            var ret = func();
            if (ret == SSErrCode.SS_OK)
            {
                // 执行正确
            }
            else if (ret == SSErrCode.SS_ERROR_DEVELOPER_PASSWORD)
            {
                throw new Exception("API 开发者密码错误");
            }
            else
            {
                throw new Exception(errMsg);
            }
        }

        /// <summary>
        /// 初始化
        /// <para>1.加密狗检查</para>
        /// </summary>
        public static void Init()
        {
            // 1.获取开发者ID
            byte[] developer_id = new byte[DEVELOPER_ID_LENGTH];
            Process(() => SlmRuntime.slm_get_developer_id(developer_id), "slm_get_developer_id");

            // 2.定义回调函数
            ST_INIT_PARAM initPram = new ST_INIT_PARAM();
            initPram.version = SSDefine.SLM_CALLBACK_VERSION02;
            initPram.flag = SSDefine.SLM_INIT_FLAG_NOTIFY;
            pfn = new callback(handle_service_msg);     // 响应回调通知只有在 slm_init 后 slm_cleanup 之前有效。
            initPram.pfn = pfn;

            // 3.指定开发者 API 密码，示例代码指定 Demo 开发者的 API 密码。
            // 注意：正式开发者运行测试前需要修改此值，可以从 Virbox 开发者网站获取 API 密码。
            initPram.password = StringToHex(APIKey);
            Process(() => SlmRuntime.slm_init(ref initPram), "初始化失败");
        }

        /// <summary>
        /// 登录
        /// </summary>
        public static void Login()
        {
            // 5.find License
            IntPtr desc = IntPtr.Zero;
            uint liceneseID = 300;

            uint ret = SlmRuntime.slm_find_license(liceneseID, INFO_FORMAT_TYPE.JSON, ref desc);
            if (ret != SSErrCode.SS_OK)
            {
                ret = SlmRuntime.slm_find_license(1, INFO_FORMAT_TYPE.JSON, ref desc);
                if (ret != SSErrCode.SS_OK)
                {
                    throw new Exception($"授权不支持,{ret.ToString()}");
                }

                liceneseID = 1;
            }

            SlmRuntime.slm_free(desc);

            SLM_HANDLE_INDEX Handle = 0;
            //03. LOGIN
            ST_LOGIN_PARAM stLogin = new ST_LOGIN_PARAM();
            stLogin.size = (UInt32)Marshal.SizeOf(stLogin);

            // 指定登录的许可ID，Demo 设置登录0号许可，开发者正式使用时可根据需要调整此参数。
            stLogin.license_id = liceneseID;

            // 指定登录许可ID的容器，Demo 设置为本地加密锁，开发者可根据需要调整此参数。
            stLogin.login_mode = SSDefine.SLM_LOGIN_MODE_LOCAL;

            IntPtr a = IntPtr.Zero;

            Process(() => SlmRuntime.slm_login(ref stLogin, INFO_FORMAT_TYPE.STRUCT, ref Handle, a), "Slm_Login");
        }
    }
}