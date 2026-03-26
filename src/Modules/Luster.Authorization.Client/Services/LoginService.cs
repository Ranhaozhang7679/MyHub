using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using Luster.Authorization.Client.Helper;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DC.Authorization.WPF.Services
{
    /// <summary>
    /// 登录服务实现（实现 ILoginService）
    /// <para>内部通过 IAuthProvider 进行认证，不再直接操作 AccountRepository</para>
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly IAuthProvider _authProvider;
        private readonly GlobalHook _hook;
        private readonly ILogger _logger;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAuthSettingRepository _authSettingRepository;

        public bool LoginAllowed { get; set; } = false;

        /// <summary>
        /// 用户在UI上选择的目标权限等级（Role.Level），刷卡验证时传给 HiveAuthProvider 做权限校验。
        /// 由 LoginViewModel 在用户切换下拉时更新。
        /// </summary>
        public int? TargetRoleLevel { get; set; } = null;

        public LoginService(IAuthProvider authProvider, ILogger logger,
            IAuditLogRepository auditLogRepository, IAuthSettingRepository authSettingRepository)
        {
            _logger = logger;
            _authSettingRepository = authSettingRepository;
            _authProvider = authProvider;
            _auditLogRepository = auditLogRepository;
            _hook = new GlobalHook();
            _hook.KeyUp += Hook_KeyUp;
            _hook.Start();
            // 初始化设置并订阅变更（原轮询方式已改为事件驱动）
            _setting = _authSettingRepository.Query();
            _authSettingRepository.SettingChanged += (_, _) => _setting = _authSettingRepository.Query();
        }

        // vkCode 常量（对齐 LoginContentFXVM.ScanCommand 使用的 WPF Key 枚举值）
        private const byte VK_0 = 0x30;  // Key.D0
        private const byte VK_9 = 0x39;  // Key.D9
        private const byte VK_ENTER = 0x0D;  // Key.Enter

        /// <summary>
        /// 临时卡号缓存（逐键拼接，仅存数字字符，对齐 ScanCommand 的 tempCardID）
        /// </summary>
        private string _tempCardNo = string.Empty;

        /// <summary>
        /// 全局键盘钩子回调，采用与 LoginContentFXVM.ScanCommand 完全相同的解析逻辑：
        /// - 数字键（D0~D9 / vkCode 0x30~0x39）：追加到临时缓存
        /// - Enter键（vkCode 0x0D）：触发卡号验证
        /// - 其他键：忽略（不污染缓存）
        /// </summary>
        private async void Hook_KeyUp(object? sender, KeyEventArgs e)
        {
            byte vk = e.Key;

            if (vk == VK_ENTER)
            {
                // Enter 触发——与 ScanCommand 中 e.Key == Key.Enter 分支对应
                var captureNo = _tempCardNo;
                _tempCardNo = string.Empty;
                await Task.Run(() => ProcessCardNoAsync(captureNo));
                return;
            }

            // 只接受数字键（对应 ScanCommand 中 e.Key.ToString().Contains('D')）
            if (vk >= VK_0 && vk <= VK_9)
            {
                _tempCardNo += (char)vk;  // ASCII '0'~'9' 与 vkCode 值相同
            }
            // 其他键（字母、功能键等）直接忽略，不清空缓存
        }

        /// <summary>
        /// Enter 后处理卡号（对齐 ScanCommand 的 else 分支逻辑）
        /// </summary>
        private async Task ProcessCardNoAsync(string raw)
        {
            // 1. 长度校验：6~10位（与 ScanCommand 完全一致）
            if (raw.Length > 10 || raw.Length < 6)
            {
                _logger.Debug("卡号长度不符（{Len}），丢弃: {Raw}", raw.Length, raw);
                return;
            }

            // 2. 去前导零（对应 ScanCommand: if (tempCardID.Substring(0,1) == "0") 去掉）
            string cardNo = raw.StartsWith("0") ? raw.Substring(1) : raw;

            _logger.Information("检测到刷卡，原始: {Raw} → 处理后: {CardNo}", raw, cardNo);

            // 记录本次处理卡号
            LastCardNo = cardNo;

            if (!LoginAllowed || !_setting.IsUseHook) return;

            // 3. 更新等待状态消息（对应 ScanCommand 中调用 OnlineVerifyCardAsync 前）
            LastAuthMessage = $"正在向 MES 验证... / Verifying with MES: {cardNo}";
            OnCardStatusUpdated?.Invoke(this, EventArgs.Empty);

            // 4. 构造凭据并调用认证
            var credential = new AuthCredential
            {
                Method = AuthMethod.CardSwipe,
                CardNo = cardNo,
                TargetRoleLevel = TargetRoleLevel
            };

            var result = await _authProvider.AuthenticateAsync(credential);

            // 5. 填充展示字段（对应 ScanCommand 中 FXCardID / FXRecv 赋值）
            LastCardUserName = result.CardUserName;
            LastCardVendor = result.CardVendor;
            LastCardDeviceLevel = result.HiveDeviceLevel;
            LastAuthMessage = result.Message;

            if (result.Success && result.Account != null)
            {
                Current = result.Account;
                _auditLogRepository.Insert(new AuditLog
                {
                    AccountId = Current.Id,
                    Operation = "Hive刷卡登录",
                    Detail = $"卡号 {cardNo} ({result.HiveDeviceLevel}) 登录成功，角色={Current.RoleName}"
                });
                OnCardLogin?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // 对应 ScanCommand 中 Level == DeviceLevel.None 时显示离线提示
                _logger.Warning("刷卡验证失败: {Message}", result.Message);
                OnCardStatusUpdated?.Invoke(this, EventArgs.Empty);
            }
        }


        public event EventHandler<EventArgs>? OnCardLogin;
        public event EventHandler<EventArgs>? OnPasswordLogin;
        public event EventHandler<EventArgs>? OnLogout;

        // ─── 刷卡后的用户展示信息（由 ProcessCardNoAsync 填充，供 ViewModel 读取）───
        /// <summary>Hive 返回的用户姓名</summary>
        public string LastCardUserName { get; private set; } = string.Empty;
        /// <summary>Hive 返回的厂商/部门</summary>
        public string LastCardVendor { get; private set; } = string.Empty;
        /// <summary>Hive 返回的设备等级字符串（如 "L8"）</summary>
        public string LastCardDeviceLevel { get; private set; } = string.Empty;
        /// <summary>最后一次验证消息（用于 MES 状态文本框）</summary>
        public string LastAuthMessage { get; private set; } = string.Empty;
        /// <summary>最后一次处理的卡号（去前导零后），供 UI 显示</summary>
        public string LastCardNo { get; private set; } = string.Empty;

        private volatile AuthSetting _setting = new();

        /// <summary>
        /// 刷卡状态更新事件（验证中/验证失败时触发，区别于成功时的 OnCardLogin）
        /// ViewModel 订阅此事件更新 MES 状态文本框和用户信息字段
        /// </summary>
        public event EventHandler<EventArgs>? OnCardStatusUpdated;

        public bool HasLogin => Current != null;
        public Account? Current { get; internal set; }

        public (bool Succeeded, string Message, string HiveLevel) Login(string cardNo, string password, int targetRoleLevel = 4)
        {
            if (string.IsNullOrEmpty(cardNo)) throw new ArgumentNullException(nameof(cardNo), "卡号不能为空!");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password), "密码不能为空!");

            var credential = new AuthCredential
            {
                Method = AuthMethod.Password,
                CardNo = cardNo,
                Password = password,
                TargetRoleLevel = targetRoleLevel
            };
            var result = _authProvider.AuthenticateAsync(credential).GetAwaiter().GetResult();
            if (result.Success && result.Account != null)
            {
                Current = result.Account;
                _auditLogRepository.Insert(new AuditLog
                {
                    AccountId = Current.Id,
                    Operation = "用户名密码登录",
                    Detail = $"账号 {cardNo} 登录成功"
                });
                OnPasswordLogin?.Invoke(this, EventArgs.Empty);
            }
            return (result.Success, result.Message, result.HiveDeviceLevel);
        }

        public void Logout()
        {
            if (Current == null) return;
            _auditLogRepository.Insert(new AuditLog
            {
                AccountId = Current.Id,
                Operation = "用户注销",
                Detail = $"账号 {Current.AccName} 注销登录"
            });
            Current = null;
            LastCardNo = string.Empty;
            LastCardUserName = string.Empty;
            LastCardVendor = string.Empty;
            LastCardDeviceLevel = string.Empty;
            LastAuthMessage = string.Empty;
            OnLogout?.Invoke(this, EventArgs.Empty);
        }

#if DEBUG
        /// <summary>
        /// 测试仓真刷卡：直接指定卡号，绕过键盘钉辒触发验证流程。
        /// 仅在 DEBUG 模式下可用。
        /// </summary>
        public Task SimulateCardSwipeAsync(string cardNo)
            => ProcessCardNoAsync(cardNo);
        //=>KeyboardHelper.SimulateCardSwipe(cardNo);
#endif
    }
}
