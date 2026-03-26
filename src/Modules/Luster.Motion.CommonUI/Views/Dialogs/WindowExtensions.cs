using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Luster.Motion.CommonUI.Views.Dialogs
{
    public static class WindowExtensions
    {
        // 屏幕居中显示
        public static void CenterToOwner(this Window win)
        {
            if (win.Owner != null && win.Owner.IsLoaded)
            {
                var ox = win.Owner.Left + win.Owner.Width / 2;
                var oy = win.Owner.Top + win.Owner.Height / 2;
                win.Left = ox - win.Width / 2;
                win.Top = oy - win.Height / 2;
            }
            else
            {
                var area = SystemParameters.WorkArea;
                win.Left = (area.Width - win.Width) / 2 + area.Left;
                win.Top = (area.Height - win.Height) / 2 + area.Top;
            }
        }
    }
}
