#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GoPxlManager
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.LMILaser.Focal5000
* 文 件 名:       GoPxlManager.cs
* 创建时间:       2022/9/20 19:51:24
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      57c2b3d6-99e4-4581-a968-930b97b7a506
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/20 19:51:24
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoPxlSDKNet;

namespace Luster.SimDevice.Laser.Focal5000
{
    public class GoPxlManager
    {
        private static GoPxlManager manager = new GoPxlManager();
        private GoDiscoveryClient Discover = null;
        private Dictionary<string, GoSystemEx> Systems = new Dictionary<string, GoSystemEx>();


        protected GoPxlManager()
        {
            //构造Api，需在调用GoPxlSdkNet 前使用，否则会引起异常
            GoApi.Construct();
        }
        public static GoPxlManager Instance { get { return manager; } }

        /// <summary>
        /// 发现当前网段内所有的GoPxl对象
        /// </summary>
        /// <returns>1 成功，其他： 失败</returns>
        public int DiscoverGoPxLs()
        {
            if (Discover == null)
                Discover = new GoDiscoveryClient();
            Discover.BlockingDiscover(10000);
            if (Discover.Count() > 0)
                return 1;
            return 0;
        }
        /// <summary>
        /// 获得所有GoPxL 的App id
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllInstances()
        {
            List<string> list = new List<string>();
            ulong nCount = Discover.Count();
            for (ulong i = 0; i < nCount; i++)
            {
                list.Add(Discover.GetInstanceAt(i).GetAppId());
            }
            return list;
        }
        /// <summary>
        /// 构建或者获取GoSystemEx 对象，该对象也可以通过Ip地址，rest 端口号，gdp端口进行创建
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GoSystemEx GetSystem(string id)
        {
            if (Systems.ContainsKey(id))
                return Systems[id];
            else
            {
                GoSystemEx s = new GoSystemEx(Discover.GetInstance(id));
                Systems.Add(id, s);
                return s;
            }
        }
    }
}