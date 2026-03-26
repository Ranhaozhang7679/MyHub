using DC.Authorization.Models;
using System;

namespace DC.Authorization
{
    /// <summary>
    /// 认证设置仓储接口
    /// </summary>
    public interface IAuthSettingRepository
    {
        /// <summary>查询当前设置</summary>
        AuthSetting Query();

        /// <summary>保存设置</summary>
        void Update(AuthSetting config);

        /// <summary>设置变更事件</summary>
        event EventHandler<EventArgs> SettingChanged;
    }
}
