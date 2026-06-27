namespace Luster.PreviewHost.Fixtures
{
    using System.Collections.Generic;
    using System.Windows.Input;

    /// <summary>
    /// AddUserDialog 的设计时 mock VM(仅供 wpf-preview-review skill 截图用,勿用于运行时)。
    /// 用自包含 RoleItem 避免对 Luster.Common.DataStruct.KeyValue 的引用,保持 PreviewHost 零额外依赖。
    /// </summary>
    public class AddUserDialogDesignVm
    {
        public List<RoleItem> UserRoles { get; set; } = new List<RoleItem>
        {
            new RoleItem { Key = "Admin", Value = 0, Desc = "管理员" },
            new RoleItem { Key = "Engineer", Value = 1, Desc = "工程师" },
            new RoleItem { Key = "Operator", Value = 2, Desc = "操作员" },
        };

        public RoleItem CurrentRole { get; set; }

        public string UserName { get; set; } = "zhangsan";

        public bool IsEdit { get; set; } = false;

        public string Password { get; set; } = "";

        public string OriginalPassword { get; set; } = "";

        public string RepeatePassword { get; set; } = "";

        public string BadgeID { get; set; } = "";

        // 按钮绑定的 Command;留 null,WPF 绑定失败静默处理,不影响渲染。
        public ICommand CloseCommand { get; set; }

        public AddUserDialogDesignVm()
        {
            CurrentRole = UserRoles[0];
        }
    }

    /// <summary>角色下拉项(模拟 KeyValue 的 Desc/Value,供 ComboBox DisplayMemberPath/SelectedValuePath 绑定)</summary>
    public class RoleItem
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Desc { get; set; }
    }
}
