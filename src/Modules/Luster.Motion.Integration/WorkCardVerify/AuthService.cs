using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SimpleTCP;

namespace Luster.Motion.Integration.WorkCardVerify
{
    public class AuthService : IAuthService
    {
        private const string AuthenticationFailure = "登录失败";
        private const string AuthenticationSuccess = "登录成功";
        private readonly ILogger _logger;//用于日志记录（客户需要刷卡日志和功能模块自带日志）
        private readonly IWebService _webService;//用于获取日志目录需要的工站名称
        private readonly PermissionMatrix _permissionMatrix;
        //只是挂载事件
        private readonly VisionAPI _visionAPI;//用于上传作业模式和信息{machineControl}{machineParameter}
        private readonly SFCHelper _sFCHelper;//用于查询权限等级
        public AuthService(IWebService webService, VisionAPI visionAPI, SFCHelper sFCHelper)
        {
            //初始化日志
            this._webService = webService;
            var sysConfig = webService.GetConfig() as WebConfig;
            var stationName = sysConfig?.StationName ?? "DefaultStation";
            string baseLogPath = Path.Combine($"D:\\{stationName}", "LUSTER", "Login Log");
            _logger = CreateLogger(baseLogPath);
            _logger?.Debug("Serilog Init OK");
            //注册事件
            if (visionAPI != null)
            {
                RoleChanged -= visionAPI.AuthService_RoleChanged;
                RoleChanged += visionAPI.AuthService_RoleChanged;
            }
            if (sFCHelper != null)
            {
                SFCCheckEvent -= sFCHelper.AuthService_SFCCheckEvent;
                SFCCheckEvent += sFCHelper.AuthService_SFCCheckEvent;
            }
            _permissionMatrix = new PermissionMatrix();
            //暂时注释界面日志
            //LogEvent += (logtype, msg, s) => LogTool.Log(logtype, msg, "AuthService");
        }

        /// <summary>
        /// Serilog初始化配置，异步，多进程共享，每日刷新文件夹
        /// </summary>
        /// <param name="baseLogPath"></param>
        /// <returns></returns>
        private ILogger CreateLogger(string baseLogPath)
        {
            // 确保 Info 和 Debug 目录存在
            var baseLogDir = /*Path.GetDirectoryName(baseLogPath);*/baseLogPath;
            var infoDir = baseLogDir;//客户需要的等级切换的日志路径
            var debugDir = Path.Combine(baseLogDir, "Debug");//权限模块的过程日志路径
            Directory.CreateDirectory(infoDir);
            Directory.CreateDirectory(debugDir);

            // 日志文件名格式为 yyyyMMdd.txt
            var infoLogPath = Path.Combine(infoDir, ".txt");
            var debugLogPath = Path.Combine(debugDir, ".txt");

            // 自定义输出模板
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss},{Message:lj}\r\n";

            // 配置日志器
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // 配置Info级别日志（写入到初始目录）
                .WriteTo.Async(a => a.File(
                    path: infoLogPath,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    outputTemplate: outputTemplate
                ))
                // 配置Debug级别日志（写入到初始\Debug目录）
                .WriteTo.Async(a => a.File(
                    path: debugLogPath,
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day,
                    shared: true,
                    outputTemplate: outputTemplate
                ))
                .CreateLogger();

            return logger;
        }
        /// <summary>
        /// 后台日志和界面显示日志输出
        /// </summary>
        /// <param name="msg"></param>
        private async Task SelfLogAsync(string msg, bool IsInfo = false)
        {
            await Task.Run(() =>
            {
                if (IsInfo) _logger?.Information(msg);//记录富士康要求的日志
                _logger?.Debug(msg);//本地日志记录
                LogEvent?.Invoke(LogType.Debug, msg, "AuthService");//界面日志显示
            });
        }

        #region Interface

        public event Action<LogType, string, string> LogEvent;
        public event Action<UserInfo> RoleChanged;
        public event Func<SFCEventArgs, SFCRecvEventArgs> SFCCheckEvent;

        public async Task<string> GetCardId()
        {
            string cardId = "123456789";
            await SelfLogAsync($"获取到卡号: {cardId}");
            return cardId;
        }

        public async Task LogLoginAsync(UserInfo user, bool isOnline)
        {
            string msg = string.Empty;
            if (user == null)
            {
                msg = "用户信息为空无法记录登录日志";
                await SelfLogAsync(msg);
                return;
            }
            // 记录登录日志
            string loginStatus = isOnline ? "在线登录" : "离线登录";
            msg = string.Concat(
                    $"Name={user.Name}, ",
                    $"Company{user.Company}, ",
                    $"CardId={user.CardId}, ",
                    $"Level={user.Level}, ",
                    $"Role={user.Role}, ",
                    $"{user.LogionMsg},",
                    $"{loginStatus}");
            await SelfLogAsync(msg, true);
        }

        public async Task<UserInfo> OfflineVerifCardyAsync(string employeeId, string password)
        {
            return await Task.Run(async () =>
            {
                var role = SystemRole.Operator;
                _permissionMatrix.CheckOfflinePermission(password, out role);
                var user = new UserInfo()
                {
                    Name= role switch
                    {
                        SystemRole.Admin => "Admin",
                        SystemRole.Sustaining => "Sustaining",
                        _ => "OP"
                    },
                    CardId = employeeId,
                    Role = role
                };
                user.LogionMsg = AuthenticationSuccess;
                return user;
            });
        }

        public async Task<UserInfo> OnlineVerifyCardAsync(string HomeWorkUrl, string cardId)
        {
            return await Task.Run(async () =>
            {
                var args = new SFCEventArgs
                {
                    HomeWorkUrl = HomeWorkUrl,
                    CardId = cardId
                };
                UserInfo user = new UserInfo();
                try
                {
                    SFCRecvEventArgs recvArgs = SFCCheckEvent?.Invoke(args);
                    if (recvArgs.SfcException != null || string.IsNullOrEmpty(recvArgs.Recv))
                    {
                        user.LogionMsg = AuthenticationFailure + "\r\n" + "SFC通讯异常，请查看日志";
                    }
                    else if (ResloveCardId(recvArgs.Recv, out var res))
                    {
                        user = res;
                        user.LogionMsg = AuthenticationSuccess + "\r\n" + recvArgs;
                    }
                    else
                    {
                        user.LogionMsg = AuthenticationFailure + recvArgs;
                    }
                    await SelfLogAsync(recvArgs.Recv);
                    user.CardId = cardId;
                    //RoleChanged.Invoke(user);
                }
                catch (Exception ex)
                {
                    await SelfLogAsync(ex.ToString());
                }
                finally
                {
                    if (_permissionMatrix.InsertedAdmin.TryGetValue(user.CardId, out var role))
                    {
                        user.Role = role;
                    }//最后查询是否是特殊管理员账号
                }
                return user;
            });

        }

        #endregion
        /// <summary>
        /// 解析SFC接口返回
        /// </summary>
        /// <param name="strRecv"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool ResloveCardId(string strRecv, out UserInfo user)
        {
            user = new UserInfo();
            bool reslut = false;
            var skillsIdx = strRecv.IndexOf("skills_info=", StringComparison.OrdinalIgnoreCase);
            if (skillsIdx >= 0)
            {
                var skillsPart = strRecv.Substring(skillsIdx + "skills_info=".Length);
                var parts = skillsPart.Split(';');
                if (parts.Length >= 3)
                {
                    user.Name = parts[0];
                    user.Company = parts[1];
                    user.Level = UserInfo.ParseDeviceLevel(parts[2]);
                    _permissionMatrix.CheckOnlinePermission(user.Level, out var role);
                    user.Role = role;
                    reslut = true;
                }
            }
            return reslut;

        }

        public void OnRoleChanged(UserInfo user)
        {
            RoleChanged?.Invoke(user);
            LogLoginAsync(user, true);
        }
    }
}
