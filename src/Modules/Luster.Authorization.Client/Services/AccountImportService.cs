using DC.Authorization;
using DC.Authorization.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace DC.Authorization.WPF.Services
{
    /// <summary>
    /// 账号批量导入/导出服务
    /// </summary>
    public class AccountImportService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IRoleRepository _roleRepository;
        private static readonly List<string> _heads = new() { "账号", "密码", "卡号", "角色", "姓名", "部门" };

        public AccountImportService(IAccountRepository accountRepository, IRoleRepository roleRepository)
        {
            _accountRepository = accountRepository;
            _roleRepository = roleRepository;
        }

        public async Task<IList<string>> Import(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentNullException(nameof(fullPath));
            if (!File.Exists(fullPath)) throw new ArgumentException("指定的文件不存在!");
            var (accList, message) = await ParseFile(fullPath);
            if (!string.IsNullOrEmpty(message)) return new[] { message };
            var valRes = Validate(accList!);
            if (valRes.Any()) return valRes;
            _accountRepository.Import(accList!);
            return Array.Empty<string>();
        }

        private async Task<(List<Account>?, string?)> ParseFile(string fullPath)
        {
            // 自动检测编码：先尝试UTF-8，失败则用GBK
            var encoding = System.Text.Encoding.UTF8;
            var bytes = File.ReadAllBytes(fullPath);
            try
            {
                // 简单判断：如果UTF-8解码包含替换字符，则尝试GBK
                var utf8Text = System.Text.Encoding.UTF8.GetString(bytes);
                if (utf8Text.Contains('�')) encoding = System.Text.Encoding.GetEncoding("GBK");
            }
            catch { encoding = System.Text.Encoding.GetEncoding("GBK"); }

            using var reader = new StreamReader(fullPath, encoding);
            var head = await reader.ReadLineAsync();
            var headList = head!.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(h => h.Trim()).ToArray();
            // 必需列
            var requiredHeads = new[] { "账号", "角色" };
            var missing = requiredHeads.Where(h => !headList.Contains(h)).ToList();
            if (missing.Count > 0)
            {
                return (null, $"文件头缺少必需列: {string.Join(",", missing)}!\r\n文件头可包含: 账号,密码,卡号,角色,姓名,部门,过期时间");
            }
            // 建立列名→实际列索引的映射
            var mapping = new Dictionary<string, int>();
            for (int i = 0; i < headList.Length; i++)
            {
                if (!mapping.ContainsKey(headList[i])) mapping.Add(headList[i], i);
            }
            var res = new List<Account>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                res.Add(new Account
                {
                    AccName = parts[mapping["账号"]].Trim(),
                    AccPassword = mapping.ContainsKey("密码") ? parts[mapping["密码"]].Trim() : "",
                    TelNo = mapping.ContainsKey("卡号") ? parts[mapping["卡号"]].Trim() : "",
                    RoleName = parts[mapping["角色"]].Trim(),
                    RealName = mapping.ContainsKey("姓名") ? parts[mapping["姓名"]].Trim() : "",
                    Department = mapping.ContainsKey("部门") ? parts[mapping["部门"]].Trim() : "",
                    SessionExpireMin = mapping.ContainsKey("过期时间") && int.TryParse(parts[mapping["过期时间"]].Trim(), out int min) ? min : 10,
                });
            }
            return (res, null);
        }

        private List<string> Validate(List<Account> accountList)
        {
            var valRes = new List<string>();
            if (accountList.Any(acc => string.IsNullOrEmpty(acc.AccName))) valRes.Add("账号不能为空!");
            if (accountList.Any(acc => string.IsNullOrEmpty(acc.AccPassword) && string.IsNullOrEmpty(acc.TelNo)))
                valRes.Add("密码和卡号不能同时为空!");
            if (accountList.Any(acc => acc.SessionExpireMin < 0))
                valRes.Add("登录时长不能为负数!");

            // 检查用户名与已有账户重复
            var duplicateNames = accountList.Where(acc => _accountRepository.AccountNameExists(acc.AccName)).Select(acc => acc.AccName).ToList();
            if (duplicateNames.Count > 0)
                valRes.Add($"用户名已存在: {string.Join(",", duplicateNames)}!");

            // 检查卡号与已有账户重复
            var existingCardNos = _accountRepository.Load(false).Select(a => a.TelNo).ToHashSet();
            var duplicateCards = accountList.Where(acc => !string.IsNullOrEmpty(acc.TelNo) && existingCardNos.Contains(acc.TelNo)).Select(acc => acc.TelNo).ToList();
            if (duplicateCards.Count > 0)
                valRes.Add($"卡号已存在: {string.Join(",", duplicateCards)}!");

            // 检查导入列表内部重复
            var dupNames = accountList.GroupBy(a => a.AccName).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dupNames.Count > 0)
                valRes.Add($"导入列表内用户名重复: {string.Join(",", dupNames)}!");
            var dupCards = accountList.Where(a => !string.IsNullOrEmpty(a.TelNo)).GroupBy(a => a.TelNo).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dupCards.Count > 0)
                valRes.Add($"导入列表内卡号重复: {string.Join(",", dupCards)}!");

            var roles = _roleRepository.Load().ToDictionary(rl => rl.Name, rl => rl.Id);
            foreach (var acc in accountList)
            {
                if (!roles.ContainsKey(acc.RoleName))
                {
                    valRes.Add($"角色名不存在,仅能指定{string.Join(",", roles.Keys)}!");
                    continue;
                }
                acc.RoleId = roles[acc.RoleName];
            }
            return valRes;
        }

        public void Export(string fullPath)
        {
            var accounts = _accountRepository.Load();
            using var writer = new StreamWriter(fullPath);
            writer.WriteLine("账号,卡号,角色,姓名,部门,过期时间");
            foreach (var acc in accounts)
            {
                writer.WriteLine($"{acc.AccName},{acc.TelNo},{acc.RoleName},{acc.RealName},{acc.Department},{acc.SessionExpireMin}");
            }
        }
    }
}
