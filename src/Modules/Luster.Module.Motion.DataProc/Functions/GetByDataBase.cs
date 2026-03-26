using Luster.Common.DataStruct.Attributes;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum DataSourceType
    {
        [Description("数据库")]
        Database,

        [Description("数据缓存")]
        Cache
    }

    public class GetByDataBase : MotionFunction
    {
        [NotEmpty]
        [Parameter("数据类别", 0, CN = "数据类别", DefaultV = DataSourceType.Database)]
        public DataSourceType DataType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter("使用工站SN", 1, CN = "使用工站SN", DefaultV = false)]
        public bool UseCurrentSN { get; set; }

        /// <summary>
        /// 工站SN
        /// </summary>
        [DependOn("UseCurrentSN", false)]
        [Parameter("产品SN", 2, CN = "产品SN", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        [NotEmpty]
        [Parameter("要获取数据列名称", 3, CN = "数据列名")]
        public string Column { get; set; }

        [Parameter("结果获取是否成功", 4, CN = "是否成功", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [Parameter("数据库对应列的值", 5, CN = "数据值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public GetByDataBase()
        {
            this.Tips = "通过SN从数据库中/或缓存中获取指定的列";
            this.Icon = "\xe6aa";
        }

        /// <summary>
        /// 数据库
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            if (UseCurrentSN)
            {
                SN = MyOwner.DataID;
            }

            if (DataType == DataSourceType.Database)
            {
                if (!MyOwner.DbHelper.GetBySn<string>(SN, Column, out var value))
                {
                    Result = false;
                }
                else
                {
                    Result = true;
                    OutString = value;
                }
            }
            else
            {
                var value = MyOwner.CacheManager.GetCacheByAlias(SN, Column);
                if (value == null)
                {
                    Result = false;
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"缓存列:{Column}获取失败!");
                }
                else
                {
                    OutString = value.ToString();
                    Result = true;
                }
            }

            return base.DoExcute(out errMsg);
        }
    }
}
