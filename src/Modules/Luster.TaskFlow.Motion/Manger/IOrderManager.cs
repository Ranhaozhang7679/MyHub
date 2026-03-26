#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WorkOrder
* 机器名称:       L05123-02
* 命名空间:       Luster.TaskFlow.Motion.Models
* 文 件 名:       WorkOrder.cs
* 创建时间:       2023/2/21 8:37:30
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a8ae823e-7c00-43dd-b583-4a533b2b31b1
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/21 8:37:30
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataAccess.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    /// <summary>
    /// 工单管理
    /// </summary>
    public interface IOrderManager
    {
        /// <summary>
        /// 工单更新
        /// </summary>
        event Action<TbWorkOrder> OrderUpdateEvent;

        /// <summary>
        /// 有无工单，如果存在返回最新的订单
        /// </summary>
        /// <param name="firstOrder">返回最上一个订单</param>
        /// <returns></returns>
        bool HasOrder(out string firstOrder);

        /// <summary>
        /// 完成工单
        /// </summary>
        /// <param name="orderNo">工单结束</param>
        void UpdateOrder(string orderNo, int surplus);

        /// <summary>
        /// 录入工单
        /// </summary>
        /// <param name="orderNo">工单编号</param>
        /// <param name="orderNum">工单数量</param>
        void ScanOrder(string orderNo, int orderNum = 0);

        /// <summary>
        /// 激活
        /// </summary>
        /// <param name="orderNo"></param>
        void ActiveOrder(string orderNo);

        /// <summary>
        /// 删除工单
        /// </summary>
        /// <param name="orderNo"></param>
        void RemoveOrder(string orderNo);


        /// <summary>
        /// 获取当前工单信息
        /// </summary>
        /// <returns></returns>
        List<TbWorkOrder> GetOrders();
    }
}