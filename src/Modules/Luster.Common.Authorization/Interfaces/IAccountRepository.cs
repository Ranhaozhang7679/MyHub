using DC.Authorization.Models;
using System.Collections.Generic;

namespace DC.Authorization
{
    /// <summary>
    /// 账号仓储接口
    /// </summary>
    public interface IAccountRepository
    {
        /// <summary>加载所有账号</summary>
        /// <param name="skipDefaultAdmin">是否跳过默认管理员</param>
        List<Account> Load(bool skipDefaultAdmin = true);

        /// <summary>用户名+密码登录</summary>
        (Account? Account, bool Succeeded, string Message) Login(string username, string password);

        /// <summary>卡号登录</summary>
        (Account? Account, bool Succeeded, string Message) Login(string cardNum);

        /// <summary>创建账号，返回新账号ID</summary>
        int Create(Account account);

        /// <summary>更新账号信息</summary>
        void Update(Account account);

        /// <summary>重置密码</summary>
        void ResetPassword(int id, string password);

        /// <summary>更新账号状态（1=正常, 0=停止, -1=删除）</summary>
        void UpdateStatus(int id, int status);

        /// <summary>删除账号</summary>
        void Delete(Account account);

        /// <summary>批量导入账号</summary>
        void Import(List<Account> accounts);

        /// <summary>检查用户名是否已存在</summary>
        bool AccountNameExists(string accName, int? id = null);
    }
}
