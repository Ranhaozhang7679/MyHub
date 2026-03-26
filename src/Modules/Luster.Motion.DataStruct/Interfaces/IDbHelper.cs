#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IRepository
* 机器名称:       Z05592
* 命名空间:       Luster.Common.DataAccess.Repositories
* 文 件 名:       IRepository.cs
* 创建时间:       2022/10/14 10:02:46
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      8d313371-d3a3-4b6a-9920-ef08f3fb7b38
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:02:46
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 数据仓储
    /// </summary>
    public interface IDbHelper
    {
        /// <summary>
        /// 通过sn和数据列名称获取值
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        bool GetBySn<T>(string sn, string column, out T value);

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="data"></param>
        void SaveAOIData(object data);

        /// <summary>
        /// 获取AOI数据
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        object GetAOIData(string sn);

        /// <summary>
        /// 获取载具数量
        /// </summary>
        /// <param name="CarrierId"></param>
        /// <returns></returns>
        int GetCarrierCount(string CarrierId);

        /// <summary>
        /// 保存NG载具信息
        /// </summary>
        /// <param name="CarrierId"></param>
        /// <param name="Count"></param>
        void SaveNGCarrierId(string CarrierId,int Count);

        /// <summary>
        /// 批量删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void BatchDelete(string SN);
    }
}
