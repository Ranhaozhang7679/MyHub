using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;

namespace Luster.Motion.Integration.WorkCardVerify
{
    /// <summary>
    /// 权限管理接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 异步实时读取卡号
        /// </summary>
        /// <returns></returns>
        Task<string> GetCardId();
        /// <summary>
        /// 异步验证卡号
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        Task<UserInfo> OnlineVerifyCardAsync(string HomeWorkUrl, string cardId);
        /// <summary>
        /// 手动登录，使用密码进行验证
        /// </summary>
        /// <param name="employeeId">员工ID，用于记录</param>
        /// <param name="password">本地密码</param>
        /// <returns></returns>
        Task<UserInfo> OfflineVerifCardyAsync(string employeeId, string password);
        /// <summary>
        /// 记录登录日志
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isManual"></param>
        Task LogLoginAsync(UserInfo user, bool isOnline);
        /// <summary>
        /// 订阅系统原有的NLog日志事件，使用其界面显示功能，实际日志记录应用Serilog
        /// </summary>
        event Action<LogType, string, string> LogEvent;
        /// <summary>
        /// 角色变更事件，通知外部状态切换和信息上传
        /// </summary>
        event Action<UserInfo> RoleChanged;
        /// <summary>
        /// SFC接口查询，返回是否通过检查
        /// </summary>
        event Func<SFCEventArgs, SFCRecvEventArgs> SFCCheckEvent;
        /// <summary>
        /// 外部调用RoleChanged.Invoke
        /// </summary>
        /// <param name="user"></param>
        void OnRoleChanged(UserInfo user);
    }

    public class SFCEventArgs : EventArgs
    {
        public string HomeWorkUrl { get; set; }
        public string CardId { get; set; }
    }

    public class SFCRecvEventArgs : EventArgs
    {
        public string Recv { get; set; }
        public Exception SfcException { get; set; }
    }

    /// <summary>
    /// 用户表单
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 提供默认构造函数，初始化默认值
        /// </summary>
        public UserInfo()
        {
            Name = "None";
            Company = "None";
            CardId = "None";
            Role = SystemRole.Operator;
            Level = DeviceLevel.None; // 默认设置为最低权限
            LogionMsg = string.Empty;
        }
        public string Name { get; set; }
        public string Company { get; set; }
        public string CardId { get; set; }
        /// <summary>
        /// 实际对于平台的等级
        /// </summary>
        public SystemRole Role { get; set; }
        /// <summary>
        /// 接口返回的等级
        /// </summary>
        public DeviceLevel Level { get; set; }
        /// <summary>
        /// 登录相关信息
        /// </summary>
        public string LogionMsg { get; set; }
        public static DeviceLevel ParseDeviceLevel(string levelStr)
        {
            if (Enum.TryParse<DeviceLevel>(levelStr, ignoreCase: true, out var level))
            {
                return level;
            }
            return DeviceLevel.None;
        }
    }

    /// <summary>
    /// 富士康接口返回角色
    /// </summary>
    public enum DeviceLevel
    {
        None,
        L1,
        L2,
        L3,
        L6,
        L7,
        L8,
        L9,
    }
}
