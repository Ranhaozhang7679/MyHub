using Luster.Motion.CommonUI.Models;
using Luster.Motion.Integration.WorkCardVerify;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    /// <summary>
    /// 用户登录事件
    /// </summary>
    public class UserLoginEvent : PubSubEvent<UserModel>
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserChangeEvent : PubSubEvent<UserModel>
    {

    }

    public class UserInfoEvent : PubSubEvent<UserInfo>
    {
    }
    public class UserLogoutEvent : PubSubEvent<int>
    {
    }
}