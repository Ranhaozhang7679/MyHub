using Luster.Motion.DataStruct.Interfaces;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LusterMotion.Config
{
    /// <summary>
    /// 容器控制
    /// </summary>
    public class IocManager : IIocManager
    {
        /// <summary>
        /// 容器对象
        /// </summary>
        private readonly IContainerProvider _container;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="container"></param>
        public IocManager(IContainerProvider container)
        {
            _container = container;
        }
        /// <summary>
        /// 对象解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
