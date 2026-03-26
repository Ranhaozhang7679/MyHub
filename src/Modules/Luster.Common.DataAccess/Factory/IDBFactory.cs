#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IDBFactory
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataAccess.Factory
* 文 件 名:       IDBFactory.cs
* 创建时间:       2022/7/4 8:48:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dabcc08e-6a4e-4c39-ab55-8cc4c36d33f6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 8:48:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Factory
{

    public interface IDBFactory
    {
        /// <summary>
        /// 设置连接字符串
        /// </summary>
        /// <param name="conn"></param>
        void SetConnectionStr(string conn, string conn1 = null, string csvDir = null);

        string GetCsvDir();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IFreeSql GetDbConnection();

        /// <summary>
        /// 自动架线
        /// </summary>
        /// <returns></returns>
        IFreeSql GetDbConnection_Ass();

        /// <summary>
        /// 数据库初始化
        /// 1.确认表都存在
        /// 2.创建表
        /// </summary>
        void DbInit(Type[] types);

        /// <summary>
        /// Ass 数据库初始化（自动同步 Ass 库表结构）
        /// </summary>
        void DbInit_Ass(Type[] types);

        /// <summary>
        /// 删除指定的数据表
        /// </summary>
        /// <param name="removeTypes"></param>
        void DbDrop(Type[] removeTypes);

        /// <summary>
        /// 数据库关闭
        /// </summary>
        void DbClose();

    }
}