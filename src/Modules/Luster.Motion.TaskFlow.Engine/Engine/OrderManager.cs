#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OrderManager
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.TaskFlow.Engine.Engine
* 文 件 名:       OrderManager.cs
* 创建时间:       2023/2/21 8:47:11
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      281957bf-0133-4973-a082-d116b92d2cb5
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/21 8:47:11
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct;
using Luster.Common.Tools;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine
{
    /// <summary>
    /// 工单管理
    /// </summary>
    public class OrderManager : IOrderManager
    {
        /// <summary>
        /// 工单
        /// </summary>
        private List<TbWorkOrder> sortOrders = new List<TbWorkOrder>();

        /// <summary>
        /// 工单更新
        /// </summary>
        public event Action<TbWorkOrder> OrderUpdateEvent;

        /// <summary>
        /// 数据仓库
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 工单构造函数
        /// </summary>
        /// <param name="repository">数据库</param>
        public OrderManager(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 扫工单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="orderNum"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ScanOrder(string orderNo, int orderNum = 0)
        {
            var wOrder = new TbWorkOrder() { OrderNo = orderNo, OrderNum = orderNum };
            if (sortOrders.Any(u => u.OrderNo.ToLower() == orderNo.ToLower()))
            {
                throw new FriendlyException($"工单:{orderNo}已存在!");
            }

            var orderID = _repository.Insert(wOrder);

            var dbOrder = _repository.Get<TbWorkOrder>(orderID);
            sortOrders.Add(dbOrder);
        }

        /// <summary>
        /// 完成工单
        /// </summary>
        /// <param name="orderNo">工单结束</param>
        public void UpdateOrder(string orderNo, int surplus)
        {
            var entity = sortOrders.FirstOrDefault(u => u.OrderNo == orderNo);
            if (surplus == 0)
            {
                entity.IsEnd = true;
            }
            else
            {
                entity.Surplus = surplus;
            }

            _repository.Update<TbWorkOrder>(entity);

            // 完成
            OrderUpdateEvent?.Invoke(entity);
        }

        /// <summary>
        /// 有无工单
        /// </summary>
        /// <param name="firstOrder"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool HasOrder(out string firstOrder)
        {
            firstOrder = string.Empty;
            var first = sortOrders.FirstOrDefault();
            if (first != null)
            {
                firstOrder = first.OrderNo;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前工单信息
        /// </summary>
        /// <returns></returns>
        public List<TbWorkOrder> GetOrders()
        {
            sortOrders = _repository.
                         GetList<TbWorkOrder>(u => !u.IsEnd, u => u.Sort, false).
                         ToList();

            return sortOrders;
        }

        /// <summary>
        /// 激活工单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ActiveOrder(string orderNo)
        {
            var order = sortOrders.FirstOrDefault(u => u.OrderNo == orderNo);
            if (order == null)
            {
                throw new FriendlyException($"工单:{order}不存在!");
            }

            sortOrders.Remove(order);
            sortOrders.Insert(0, order);

            // 更新Sort
            int sort = 0;
            foreach (var item in sortOrders)
            {
                item.Sort = sort;
                sort += 1;
            }

            // 批量更新
            _repository.BatchUpdate(sortOrders.ToArray());
        }

        /// <summary>
        /// 删除工单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveOrder(string orderNo)
        {
            var order = sortOrders.FirstOrDefault(u => u.OrderNo == orderNo);
            if (order == null)
            {
                throw new FriendlyException($"工单:{order}不存在!");
            }

            sortOrders.Remove(order);
            _repository.Delete(order);
        }
    }
}