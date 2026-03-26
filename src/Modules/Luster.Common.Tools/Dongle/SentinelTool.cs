#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SentinelTool
* 机器名称:       L05123-02
* 命名空间:       Luster.Common.Tools.Dongle
* 文 件 名:       SentinelTool.cs
* 创建时间:       2023/2/24 11:20:58
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ff0b5788-12b8-4a68-ac44-3273f80c779b
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/24 11:20:58
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.Tools;
using SafeNet.Sentinel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.Dongle
{
    public class SentinelTool
    {
        private static volatile SentinelTool instance;
        private static readonly object syncRoot = new object();

        private static AdminApi adminApi;
        private readonly AdminStatus _status;
        private AdminStatus _curStatus;

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private SentinelTool()
        {
            adminApi = new AdminApi();
            _status = adminApi.connect();
        }

        /// <summary>
        /// 加密狗接口实例
        /// </summary>
        public static SentinelTool Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SentinelTool();
                        }

                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 检查是否插入加密狗
        /// </summary>
        /// <returns></returns>
        public bool IsExisted()
        {
            // 如果离线就不在检测加密狗
            if (CommonTool.ApplicationMode == ApplicationMode.Offline) return true;

            return GetData(DataKey.Hasp).Count > 0;
        }

        /// <summary>
        /// 检查是否连接加密狗服务
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _status == AdminStatus.StatusOk;
        }

        /// <summary>
        /// 获取最后一次查询数据的状态结果
        /// </summary>
        /// <param name="sCode">状态码</param>
        /// <returns>状态信息</returns>
        public string GetQueryStatus(out int sCode)
        {
            sCode = (int)_curStatus;

            return _curStatus.ToString();
        }

        /// <summary>
        /// 查询加密狗相关数据信息
        /// </summary>
        /// <param name="key">数据类型</param>
        /// <returns>数据信息</returns>
        private List<XElement> GetData(DataKey key = DataKey.Hasp)
        {
            string info = "";
            string k = key.ToString().ToLower();
            _curStatus = AdminStatus.BadParameters;

            if (_status == AdminStatus.StatusOk)
            {
                _curStatus = adminApi.adminGet(
                    "<haspscope/>",
                    "<admin>" +
                    $"  <{k}>" +
                    "   <element name=\"*\" />" +
                    $"  </{k}>" +
                    "</admin>",
                    ref info
                    );
            }
            else
            {
                _curStatus = _status;
            }

            if (!string.IsNullOrEmpty(info) && _curStatus == AdminStatus.StatusOk)
            {
                XDocument doc = XDocument.Parse(info);
                try
                {
                    return doc.Element("admin_response").Elements(k).ToList();
                }
                catch (System.NullReferenceException)
                {

                }
            }

            return new List<XElement>();
        }


        /// <summary>
        /// 信息XML关键词
        /// </summary>
        internal enum DataKey
        {
            Hasp,
            Feature,
            Config,
            License_Manager,

            Detached,
            Certificates,
            Session,
        }
    }
}