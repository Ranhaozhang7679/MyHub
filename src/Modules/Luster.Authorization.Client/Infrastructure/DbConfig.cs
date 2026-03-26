using System.Security.Cryptography;
using System.Text;

namespace DC.Authorization.WPF.Infrastructure
{
    /// <summary>
    /// 数据库配置和工具方法（原 Core 中的 Common 类）
    /// </summary>
    public static class DbConfig
    {
        /// <summary>SQLite 连接字符串</summary>
        public static readonly string ConnectionString = "Data Source=../settings/authorization.db";

        private static readonly string DefaultSalt = @"Luster@1996";

        /// <summary>密码 MD5 哈希（含盐值）</summary>
        public static string CalcPwdMd5(string? pwd)
        {
            if (string.IsNullOrEmpty(pwd)) return string.Empty;
            MD5 md5 = MD5.Create();
            string calcPwd = string.Concat(DefaultSalt, pwd);
            byte[] bArr = md5.ComputeHash(Encoding.UTF8.GetBytes(calcPwd));
            var sb = new StringBuilder();
            foreach (byte ch in bArr)
            {
                sb.Append(ch.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
