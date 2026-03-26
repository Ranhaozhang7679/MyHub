using Luster.Common.DataAccess.Tables;
using Luster.Motion.DataStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataAccess.Factory;

namespace Luster.Common.DataAccess.Repositories
{
    public class DbHelper : IDbHelper
    {
        private readonly IRepository _repository;

        private readonly IDBFactory _dbFactory;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="factory"></param>
        public DbHelper(IRepository repository, IDBFactory factory)
        {
            _repository = repository;
            _dbFactory = factory;
        }

        public bool GetBySn<T>(string sn, string column, out T value)
        {
            value = default(T);
            var entity = _repository.GetList<TbProductInfo>(u => u.SNCode == sn, u => u.ID, true).FirstOrDefault();
            if (entity == null)
            {
                return false;
            }

            var prop = typeof(T).GetProperties().FirstOrDefault(u => u.Name == column);
            if (prop != null)
            {
                value = (T)prop.GetValue(entity);

                return true;
            }

            if (string.IsNullOrEmpty(entity.Data))
            {
                return false;
            }

            var datas = entity.Data.Split('|');
            foreach (var item in datas)
            {
                var items = item.Split(':');
                if (items.Length == 2 && items[0] == column)
                {
                    value = items[1].To<T>();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="data"></param>
        public void SaveAOIData(object data)
        {
            if (data is TbAOIResult aOIResult)
            {
                _repository.SaveAOIResult(aOIResult);
            }
        }


        /// <summary>
        /// 获取AOI数据
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public object GetAOIData(string sn)
        {
            return _repository.GetList<TbAOIResult>(u => u.Serial == sn, u => u.ID, true).FirstOrDefault();
        }

        /// <summary>
        /// 获取NG载具出现的次数
        /// </summary>
        /// <returns></returns>
        public int GetCarrierCount(string CarrierId)
        {
            int count= _repository.GetList<TbNGCarrirerData>(u => u.CarrierId == CarrierId, u=>u.ID).Count();
            return count;
        }

        /// <summary>
        /// 保存NG载具信息
        /// </summary>
        /// <param name="CarrierId"></param>
        /// <param name="Count"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SaveNGCarrierId(string CarrierId, int Count)
        {
            TbNGCarrirerData tbNGCarrirerData = new TbNGCarrirerData();   
            tbNGCarrirerData.CarrierId= CarrierId;
            tbNGCarrirerData.Count = Count;
            _repository.SaveNGCarrierId(tbNGCarrirerData);
        }

        /// <summary>
        /// 清空NG载具信息
        /// </summary>
        /// <param name="CarrierId"></param>
        public void BatchDelete(string CarrierId)
        {
            var tbCarrier = _repository.GetList<TbNGCarrirerData>(x => x.CarrierId==CarrierId, x => x.ID);
            _repository.BatchDelete(tbCarrier);
        }
    }
}
