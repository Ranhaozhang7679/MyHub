using System;
using System.Collections.Generic;

namespace DC.Authorization.Models
{
    /// <summary>
    /// 认证相关设置（登录模式等）
    /// </summary>
    public class AuthSetting
    {
        /// <summary>只能扫码登录</summary>
        public bool IsOnlyScanType { get; set; } = false;
        /// <summary>是否密码登录</summary>
        public bool IsPwdLogin { get; set; } = false;
        /// <summary>是否使用 Hook（全局键盘钩子用于刷卡）</summary>
        public bool IsUseHook { get; set; } = true;

        /// <summary>从字典反序列化设置</summary>
        public void Deserialize(Dictionary<string, string> map)
        {
            foreach (var prop in typeof(AuthSetting).GetProperties())
            {
                if (!map.ContainsKey(prop.Name)) continue;
                if (prop.PropertyType == typeof(string)) { prop.SetValue(this, map[prop.Name]); }
                else if (prop.PropertyType == typeof(string[]))
                { prop.SetValue(this, map[prop.Name].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)); }
                else if (prop.PropertyType == typeof(int)) { prop.SetValue(this, Convert.ToInt32(map[prop.Name])); }
                else if (prop.PropertyType == typeof(bool)) { prop.SetValue(this, Convert.ToBoolean(map[prop.Name])); }
            }
        }

        /// <summary>序列化为字典</summary>
        public Dictionary<string, string> Serialize()
        {
            var map = new Dictionary<string, string>();
            foreach (var prop in typeof(AuthSetting).GetProperties())
            {
                if (prop.PropertyType == typeof(string[]))
                { map.Add(prop.Name, string.Join(",", prop.GetValue(this) as string[] ?? Array.Empty<string>())); }
                else { map.Add(prop.Name, Convert.ToString(prop.GetValue(this)) ?? string.Empty); }
            }
            return map;
        }
    }
}
