#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RollManager
* 机器名称:       L05590-02
* 命名空间:       Luster.Motion.TaskFlow.Engine.Engine
* 文 件 名:       OrderManager.cs
* 创建时间:       2023/7/25 8:47:11
* 作    者:       鲁帆
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      281957bf-0133-4973-a082-d116b92d2cb5
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/7/25 8:47:11
* 修 改 人:		  fanlu
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
    /// 卷料管理
    /// </summary>
    public class RollManager : IRollManager
    {
        /// <summary>
        /// 卷料单
        /// </summary>
        private List<TbRollInfo> sortOrders = new List<TbRollInfo>();

        /// <summary>
        /// 卷料单更新
        /// </summary>
        public event Action<TbRollInfo> OrderUpdateEvent;

        /// <summary>
        /// 数据仓库
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">数据库</param>
        public RollManager(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 扫卷料单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="orderNum"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ScanOrder(string orderNo, int orderNum = 0)
        {
            var wOrder = new TbRollInfo() { RollNo = orderNo, RollNum = orderNum,Sation="左" };
            if (sortOrders.Any(u => u.RollNo.ToLower() == orderNo.ToLower()))
            {
                throw new FriendlyException($"卷料单:{orderNo}已存在!");
            }

            var orderID = _repository.Insert(wOrder);

            var dbOrder = _repository.Get<TbRollInfo>(orderID);
            sortOrders.Add(dbOrder);
        }

        /// <summary>
        /// 完成卷料单
        /// </summary>
        /// <param name="orderNo">工单结束</param>
        public void UpdateStation(string orderNo,string station)
        {
            var entity = sortOrders.FirstOrDefault(u => u.RollNo == orderNo);
            if (station!="")
            {
                entity.Sation = station;
            }
            _repository.Update<TbRollInfo>(entity);

            // 完成
            //OrderUpdateEvent?.Invoke(entity);
        }

        /// <summary>
        /// 完成卷料单
        /// </summary>
        /// <param name="orderNo">工单结束</param>
        public void ClearUseNum(string orderNo)
        {
            var entity = sortOrders.FirstOrDefault(u => u.RollNo == orderNo);
            if (entity != null)
            {
                entity.UseNum = 0;
            }
            _repository.Update<TbRollInfo>(entity);

            // 完成
            OrderUpdateEvent?.Invoke(entity);
        }

        /// <summary>
        /// 有无卷料单
        /// </summary>
        /// <param name="firstOrder"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool HasOrder(bool isLeft,out string firstOrder)
        {
            firstOrder = string.Empty;
            foreach (var a in sortOrders)
            {
                if(isLeft&&a.Sation=="左")
                {
                    firstOrder=a.RollNo;
                    return true;
                }
                else if(!isLeft&&a.Sation=="右")
                {
                    firstOrder = a.RollNo;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取当前卷料信息
        /// </summary>
        /// <returns></returns>
        public List<TbRollInfo> GetOrders()
        {
            sortOrders = _repository.
                         GetList<TbRollInfo>(u => !u.IsEnd, u => u.Sort, false).
                         ToList();

            return sortOrders;
        }

        /// <summary>
        /// 查找已使用卷料
        /// </summary>
        /// <param name="rollNo"></param>
        /// <returns></returns>
        public int GetUse(string rollNo)
        {
            int usenum = 0;
            var listRoll = _repository.
                             GetList<TbRollInfo>(u => u.RollNo== rollNo, u => u.Sort, false).
                             ToList();
            if(listRoll!=null)
            {
                if (listRoll.Count == 1)
                {
                    usenum = listRoll[0].UseNum;
                }
            }
            return usenum;
        }

        /// <summary>
        /// 激活卷料单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ActiveOrder(string rollNo)
        {
            var order = sortOrders.FirstOrDefault(u => u.RollNo == rollNo);
            if (order == null)
            {
                throw new FriendlyException($"卷料单:{order}不存在!");
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
        /// 删除卷料工单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveOrder(string rollNo)
        {
            var order = sortOrders.FirstOrDefault(u => u.RollNo == rollNo);
            if (order == null)
            {
                throw new FriendlyException($"卷料单:{order}不存在!");
            }

            sortOrders.Remove(order);
            _repository.Delete(order);
        }


        private int _surplusPercent;
        public int SurplusPercent
        {
            get { return _surplusPercent; }
            set {
                 if(value < 0)
                {
                    _surplusPercent = 0;
                }
                 else if(value>100)
                {
                    _surplusPercent = 100;
                }
                 else
                {
                    _surplusPercent = value;
                }
            }
        }
        public void UpdateOrder(string orderNo, int surplus)
        {
            var entity = sortOrders.FirstOrDefault(u => u.RollNo == orderNo);
            if (surplus == 0)
            {
                entity.IsEnd = true;
            }
            else
            {
                entity.RollNum = surplus;
            }

            _repository.Update<TbRollInfo>(entity);

            // 完成
            OrderUpdateEvent?.Invoke(entity);
        }
        public int UpdateUseNum(string orderNo)
        {
             SurplusPercent = 0;
            var entity = sortOrders.FirstOrDefault(u => u.Sation == orderNo);
            //如果没有找到，直接输出999
            if (entity == null) return 999;
            if (entity.UseNum < entity.RollNum)
            {
                entity.UseNum= entity.UseNum+1;
            }
            else
            {
                entity.UseNum = entity.RollNum;
                //throw new FriendlyException($"卷料单:{orderNo}已使用数量已达上限!");
            }
            _repository.Update<TbRollInfo>(entity);
            OrderUpdateEvent?.Invoke(entity);
            if (entity.RollNum>0)
            {
                SurplusPercent = Convert.ToInt32((((double)(entity.RollNum-entity.UseNum) / (double)entity.RollNum))*100);
            }
            return _surplusPercent;
        }
    }
}