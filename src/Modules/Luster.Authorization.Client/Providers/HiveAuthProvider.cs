using DC.Authorization;
using DC.Authorization.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DC.Authorization.WPF.Providers
{
    /// <summary>
    /// HiveApi 认证提供者：通过 HTTP/SFC 调用 Hive 接口验证刷卡
    /// <para>
    /// Hive SFC 接口返回格式：skills_info=姓名;公司;等级 （例：skills_info=王小明;ABC;L8）
    /// </para>
    /// <para>
    /// 四级权限对应 DeviceLevel 映射矩阵（依据开发文档）：
    /// <code>
    /// DeviceLevel │ OP  │ Maintenance │ Integrator │ Administrator
    ///    L1       │  ✓  │      ✓      │     ✓      │      ✗
    ///    L2       │  ✓  │      ✓      │     ✓      │      ✗
    ///    L3       │  ✓  │      ✓      │     ✓      │      ✓
    ///    L6       │  ✓  │      ✓      │     ✗      │      ✗
    ///    L7       │  ✓  │      ✗      │     ✗      │      ✗
    ///    L8       │  ✓  │      ✗      │     ✗      │      ✗
    ///    L9       │  ✓  │      ✓      │     ✗      │      ✗
    /// </code>
    /// Role.Level 约定：数值越小权限越高（1=Administrator, 2=Integrator, 3=Maintenance, 4=OP）
    /// </para>
    /// </summary>
    public class HiveAuthProvider : IAuthProvider
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger _logger;

        // TODO: 替换为实际的 HiveApi/SFC 配置（通过属性注入或配置文件）
        public string HiveApiBaseUrl { get; set; } = "http://172.25.3.168/fatpal";

        /// <summary>
        /// DeviceLevel 字符串 → 该等级可授权的最高 Role.Level
        /// Role.Level 约定：数值越小权限越高。
        ///   1 = Administrator
        ///   2 = Integrator
        ///   3 = Maintenance
        ///   4 = OP Read only
        /// 因此此处存储"最低的 Level 数值"即"最高可授权限"。
        /// </summary>
        private static readonly Dictionary<string, int> DeviceLevelToMaxRoleLevel =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "L1", 2 },  // OP / Maintenance / Integrator（不含 Administrator）
                { "L2", 2 },  // OP / Maintenance / Integrator
                { "L3", 1 },  // 全部四级均可（含 Administrator）
                { "L6", 3 },  // OP / Maintenance
                { "L7", 4 },  // 仅 OP Read only
                { "L8", 4 },  // 仅 OP Read only（Line leader）
                { "L9", 3 },  // OP / Maintenance（MFG Technician）
            };

        public HiveAuthProvider(IRoleRepository roleRepository, IAccountRepository accountRepository,
            ILogger logger)
        {
            _roleRepository = roleRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        /// <summary>HiveApi 可用性检查（有配置 URL 时视为可用）</summary>
        public bool IsAvailable => !string.IsNullOrEmpty(HiveApiBaseUrl);

        public async Task<AuthResult> AuthenticateAsync(AuthCredential credential)
        {
            if (credential.Method != AuthMethod.CardSwipe)
                return new AuthResult { Success = false, Message = "HiveApi 仅支持刷卡认证" };

            try
            {
                _logger.Information("Hive 刷卡认证，卡号: {CardNo}，目标权限等级: {TargetLevel}",
                    credential.CardNo, credential.TargetRoleLevel);
                //#if DEBUG
                //                string deviceLevel = "L3";
                //                string name = "Debug测试用";
                //                string company = "DebugVendor";
                //#elif DEBUG
                // ─── Step 1: 调用 SFC 接口（对照 AuthService.OnlineVerifyCardAsync）────
                var (recv, sfcEx) = await CallHiveSfcAsync(credential.CardNo!);

                // SFC 通讯异常或无响应 → 提示离线登录（对照 ScanCommand 中 Level==None 时 IsOffline=Visible）
                if (sfcEx != null || string.IsNullOrEmpty(recv))
                {
                    string sfcErrMsg = sfcEx?.Message ?? "MES 无响应";
                    _logger.Warning("Hive SFC 通讯失败: {Err}", sfcErrMsg);
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"MES 通讯异常，无法在线验证 / MES unavailable\n{sfcErrMsg}\n\n→ 请勾选\"离线登录\"使用密码验证"
                    };
                }

                _logger.Debug("Hive SFC 原始响应: {Recv}", recv);

                // ─── Step 2: 解析响应（对照 AuthService.ResloveCardId）────────────
                if (!ResolveCardInfo(recv, out string name, out string company,
                    out string deviceLevel, out string parseErr))
                {
                    _logger.Warning("Hive 响应解析失败: {Err}", parseErr);
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"MES 响应解析失败 / Parse error: {parseErr}\nRaw: {recv}"
                    };
                }

                _logger.Information("Hive 解析结果: Name={N}, Company={C}, Level={L}",
                    name, company, deviceLevel);

                //#endif
                // ─── Step 3: 权限矩阵校验（四级权限表）──────────────────────────
                int targetRoleLevel = credential.TargetRoleLevel ?? 4;
                if (!CheckPermission(deviceLevel, targetRoleLevel, out string permMsg))
                {
                    _logger.Warning("权限不足: DeviceLevel={DL}, 目标={TL}", deviceLevel, targetRoleLevel);
                    return new AuthResult
                    {
                        Success = false,
                        Message = permMsg,
                        CardUserName = name,
                        CardVendor = company,
                        HiveDeviceLevel = deviceLevel
                    };
                }

                // ─── Step 4: 查找本地角色，找到或创建对应账号────────────────────
                var roles = _roleRepository.Load();
                var targetRole = roles.Find(r => r.Level == targetRoleLevel);
                if (targetRole == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"本地未找到 Level={targetRoleLevel} 的角色，请检查角色配置",
                        CardUserName = name,
                        CardVendor = company,
                        HiveDeviceLevel = deviceLevel
                    };
                }

                // ─── Step 5: 取该角色的默认账号（Hive 已完成身份认证，本地只需角色对应的占位账号）
                var allAccounts = _accountRepository.Load(false);
                var account = allAccounts.FirstOrDefault(a => a.RoleId == targetRole.Id);

                if (account == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"本地未配置 [{targetRole.Name}] 角色的账号，请联系管理员",
                        CardUserName = name,
                        CardVendor = company,
                        HiveDeviceLevel = deviceLevel
                    };
                }

                // 用 Hive 返回的 Name/Company 覆盖展示字段（账号存储不受影响）
                account.RealName = name;
                account.Department = company;
                account.HiveLevel = deviceLevel;

                _logger.Information("Hive 认证成功: {Name}({Company}) {Level} → {Role}",
                    name, company, deviceLevel, targetRole.Name);

                return new AuthResult
                {
                    Success = true,
                    Message = $"验证成功 / Verification OK  {name} [{deviceLevel} → {targetRole.Name}]",
                    Account = account,
                    RoleLevel = targetRoleLevel,
                    HiveDeviceLevel = deviceLevel,
                    CardUserName = name,
                    CardVendor = company
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Hive 认证异常");
                return new AuthResult { Success = false, Message = $"HiveApi 调用失败: {ex.Message}" };
            }
        }

        /// <summary>
        /// 调用 Hive SFC 接口查询刷卡权限信息。
        /// 对照 SFCHelper.AuthService_SFCCheckEvent 实现，不引入 SFCHelper 依赖。
        /// <para>
        /// 请求：POST {HiveApiBaseUrl}?c=QUERY_4_SFC&amp;subcmd=query_skills_info&amp;isAutomation=1&amp;cardtagid={cardNo}
        /// </para>
        /// <para>
        /// 成功返回原始响应字符串（如 "0 SFC_OK\nskills_info=王小明;ABC;L8"），
        /// 失败返回 null（此时 exception 有值）。
        /// </para>
        /// </summary>
        private async Task<(string? recv, Exception? exception)> CallHiveSfcAsync(string cardNo)
        {
            // URL 格式完全对照 SFCHelper.AuthService_SFCCheckEvent（第 2012-2017 行）
            string query = $"c=QUERY_4_SFC" +
                           $"&subcmd=query_skills_info" +
                           $"&isAutomation=1" +
                           $"&cardtagid={cardNo}";
            string url = $"{HiveApiBaseUrl}?{query}";

            string? strRecv = null;
            Exception? lastException = null;

            // 3 次重试，对照 SFCHelper 第 2021-2037 行
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var request = System.Net.WebRequest.CreateHttp(url);
                    request.Method = "POST";
                    request.Timeout = 1000;          // 1000ms，与 WebTool 默认值相同
                    request.ContentLength = 0;       // Body 空，对照 new byte[0]
                    request.ContentType = "application/x-www-form-urlencoded";

                    using var response = (System.Net.HttpWebResponse)
                        await request.GetResponseAsync().ConfigureAwait(false);
                    using var reader = new System.IO.StreamReader(
                        response.GetResponseStream()!,
                        System.Text.Encoding.UTF8);
                    strRecv = await reader.ReadToEndAsync().ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(strRecv)) break;  // 取到内容即退出
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.Warning("Hive SFC 第 {Attempt} 次请求失败: {Msg}", i + 1, ex.Message);
                }
                finally
                {
                    // 重试间隔 300ms，对照 SFCHelper 第 2035 行
                    if (i < 2) await Task.Delay(300).ConfigureAwait(false);
                }
            }

            if (strRecv == null)
                lastException ??= new Exception("SFC Connected Fail - 3 次重试均无响应");

            return (strRecv, lastException);
        }

        /// <summary>
        /// 解析 Hive SFC 接口的原始响应字符串，提取用户信息。
        /// 对照 AuthService.ResloveCardId + SFCHelper.GetHomework 的解析逻辑实现。
        /// <para>
        /// 响应格式（两行，\n 分隔）：
        ///   第 0 行：状态行，例 "0 SFC_OK" 或 "0 SFC_FATAL_ERR"
        ///   第 1 行：数据行，例 "skills_info=王小明;ABC;L8"
        /// </para>
        /// </summary>
        /// <param name="rawResponse">CallHiveSfcAsync 返回的原始字符串</param>
        /// <param name="name">对应界面 Name 字段（UserInfo.Name）</param>
        /// <param name="company">对应界面 Vendor 字段（UserInfo.Company）</param>
        /// <param name="deviceLevel">对应界面 Level 字段，如 "L8"（UserInfo.Level）</param>
        /// <param name="failReason">解析失败时的原因描述</param>
        /// <returns>解析成功返回 true</returns>
        private bool ResolveCardInfo(string rawResponse,
            out string name, out string company, out string deviceLevel, out string failReason)
        {
            name = company = deviceLevel = failReason = string.Empty;

            // Step A：按 \n 分行（对照 SFCHelper.GetHomework 第 637 行：result.Split('\n')）
            var lines = rawResponse.Split('\n');
            if (lines.Length < 2)
            {
                failReason = $"SFC 响应行数不足: {rawResponse}";
                return false;
            }

            // Step B：检查状态行（对照 GetHomework 第 641-654 行）
            var statusParts = lines[0].Trim().Split(' ');
            if (statusParts.Length < 2)
            {
                failReason = $"SFC 状态行格式异常: {lines[0]}";
                return false;
            }
            string statusCode = statusParts[1]; // "SFC_OK" 或 "SFC_FATAL_ERR"

            if (statusCode == "SFC_FATAL_ERR" || statusCode == "SFC_FATAL_ERROR")
            {
                failReason = $"SFC 接口错误: {lines[1]}";
                return false;
            }
            if (statusCode != "SFC_OK")
            {
                failReason = $"SFC 未知状态: {statusCode}";
                return false;
            }

            // Step C：解析 skills_info（对照 AuthService.ResloveCardId 第 230-243 行）
            //   AuthService 用 IndexOf("skills_info=") 搜索整个字符串
            //   此处从第 1 行开始搜（行结构更明确），逻辑等价
            string dataLine = lines[1];
            const string key = "skills_info=";
            var idx = dataLine.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                failReason = $"响应中未找到 skills_info: {dataLine}";
                return false;
            }

            var skillsPart = dataLine.Substring(idx + key.Length);
            var parts = skillsPart.Split(';');
            if (parts.Length < 3)
            {
                failReason = $"skills_info 字段不足（期望 Name;Company;Level）: {skillsPart}";
                return false;
            }

            // 对应 UserInfo.Name / UserInfo.Company / UserInfo.Level（DeviceLevel 字符串）
            name = parts[0].Trim();
            company = parts[1].Trim();
            deviceLevel = parts[2].Trim();   // e.g. "L8"
            return true;
        }

        /// <summary>
        /// 校验 Hive DeviceLevel 是否允许所选 Role.Level（四级权限矩阵）
        /// </summary>
        /// <param name="hiveDeviceLevel">Hive 返回的等级字符串（如 "L8"）</param>
        /// <param name="targetRoleLevel">本地 Role.Level（1=Admin, 2=Integrator, 3=Maintenance, 4=OP）</param>
        /// <param name="message">校验结果说明</param>
        /// <returns>是否有权限</returns>
        private bool CheckPermission(string hiveDeviceLevel, int targetRoleLevel, out string message)
        {
            if (!DeviceLevelToMaxRoleLevel.TryGetValue(hiveDeviceLevel, out int maxGrantableLevel))
            {
                message = $"未知的 Hive 等级: {hiveDeviceLevel}，拒绝访问";
                return false;
            }

            // Role.Level 越小权限越高，因此：
            //   targetRoleLevel >= maxGrantableLevel 表示目标权限不高于可授权上限 → 允许
            //   targetRoleLevel <  maxGrantableLevel 表示目标权限超出可授范围 → 拒绝
            if (targetRoleLevel >= maxGrantableLevel)
            {
                message = $"权限校验通过 ({hiveDeviceLevel} 可授予 Level {targetRoleLevel})";
                return true;
            }
            else
            {
                message = $"权限不足 / Insufficient permission\n" +
                          $"您的卡片等级 {hiveDeviceLevel} 最高可授予 Level {maxGrantableLevel}，" +
                          $"无法获得 Level {targetRoleLevel} 权限";
                return false;
            }
        }
    }
}
