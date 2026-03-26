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
            using var reader = new StreamReader(fullPath);
            var head = await reader.ReadLineAsync();
            var headList = head!.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
            var mapping = new Dictionary<string, int>();
            for (int i = 0; i < _heads.Count; i++)
            {
                if (!headList.Contains(_heads[i])) return (null, "文件头仅能包含账号,密码,卡号,角色,姓名,部门!");
                if (!mapping.ContainsKey(_heads[i])) mapping.Add(_heads[i], i);
            }
            var res = new List<Account>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);
                res.Add(new Account
                {
                    AccName = parts[mapping["账号"]],
                    AccPassword = parts[mapping["密码"]],
                    TelNo = parts[mapping["卡号"]],
                    RoleName = parts[mapping["角色"]],
                    RealName = parts[mapping["姓名"]],
                    Department = parts[mapping["部门"]],
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
