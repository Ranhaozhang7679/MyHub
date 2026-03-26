using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI
{
    public class ConfigHelper
    {
        public static string GetAppKey(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return ConfigurationManager.AppSettings[key];
            }

            return string.Empty;
        }

        /// <summary>
        /// 通过是否IPhone来进行判断
        /// </summary>
        /// <param name="normalAction"></param>
        /// <param name="iphoneAction"></param>
        public static void IsIPhone(Action normalAction, Action iphoneAction)
        {
            if (GetAppKey("IsIPhone") == "False")
            {
                normalAction?.Invoke();
            }
            else
            {
                iphoneAction?.Invoke();
            }
        }
    }
}
