using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Engine
{
    public class ConfigManager : IConfigManager
    {
        private object sysConfig = null;

        /// <summary>
        /// 系统配置
        /// </summary>
        /// <param name="config"></param>
        public void SetWebConfig(object config)
        {
            sysConfig = config;
        }


        /// <summary>
        /// 获取配置项目
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string GetWebConfig(string Key)
        {
            string value = string.Empty;
            var propties = sysConfig.GetType().GetProperties();

            foreach (var prop in propties)
            {
                if (prop.Name.ToLower() == Key.ToLower())
                {
                    value = prop.GetValue(sysConfig)?.ToString();
                    break;
                }
            }

            return value;
        }

        /// <summary>
        /// 数据字典
        /// </summary>
        private ConcurrentDictionary<string, object> dataDict = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 新增缓存数据
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Data"></param>
        public void AddCacheData(string Key, object Data)
        {
            if (!dataDict.ContainsKey(Key))
            {
                dataDict.TryAdd(Key, Data);
            }
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public object GetCacheData(string Key)
        {
            if (dataDict.TryRemove(Key, out object cData))
            {
                return cData;
            }

            return null;
        }


        /// <summary>
        /// 配置机台模式
        /// </summary>
        /// <param name="mode"></param>
        public void SetMachineMode(string mode)
        {

        }
    }
}
