using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    public interface IConfigManager
    {
        /// <summary>
        /// 系统配置
        /// </summary>
        /// <param name="config"></param>
        void SetWebConfig(object config);

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        string GetWebConfig(string Key);

        /// <summary>
        /// 新增缓存数据
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Data"></param>
        void AddCacheData(string Key, object Data);

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        object GetCacheData(string Key);
    }
}
