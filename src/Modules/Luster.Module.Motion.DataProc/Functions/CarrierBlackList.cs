using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.SFC;
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
    public class CarrierBlackList:MotionFunction
    {

        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        [NotEmpty]
        [Parameter("动作类型", 1, CN = "动作类型", DefaultV = BlackCarrierType.Query)]
        public BlackCarrierType BlackType { get; set; }

        [NotEmpty]
        [Parameter("首站是治具码，非首站是WIP", 2, CN = "二维码", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        [NotEmpty]
        [Parameter("首站", 3, CN = "首站", DefaultV = false)]
        public bool FirstStation { get; set; }

        [NotEmpty]
        [DependOn("BlackType", BlackCarrierType.Add)]
        [Parameter("次数", 4, CN = "次数",DefaultV =3)]
        public int SetCount { get; set; }

        [NotEmpty]
        [DependOn("BlackType", BlackCarrierType.Add)]
        [Parameter("结果是否NG，NG:True;OK:False;目的是不连续，清空数据库", 5, CN = "NG", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool NG { get; set; }

        [NotEmpty]
        [DependOn("BlackType", BlackCarrierType.Add)]
        [Parameter("是否解绑", 6, CN = "解绑", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool Unbind { get; set; }


        [Parameter("是否解绑", 7, CN = "测试地址",  DefaultV ="http://172.25.3.168/fatpve?")]
        public string TestUrl { get; set; }

        [Parameter("结果，查询时True代表在黑名单；打黑时True代表正式打入系统黑名单",10, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT,DefaultV = false)]
        public bool Result { get; set; }

        [DependOn("BlackType", BlackCarrierType.Add)]
        [Parameter("当前次数", 11, CN = "当前次数", ParamType = TaskFlow.Common.Enums.ParamType.OUT,DefaultV =0)]
        public int CurrentCount { get; set; }

        [Parameter("SFCOK返回值", 7, CN = "sfc返回值", CanRef = ParamRef.Ref)]
        public string ReturnOKValue { get; set; }

        private SFCHelper _sfcHelper = null;


        /// <summary>
        /// 黑名单操作方式
        /// </summary>
        public enum BlackCarrierType
        {
            [Description("查询")]
            Query,
            [Description("打黑")]
            Add
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public CarrierBlackList()
        {
            this.Tips = "统计治具黑名单";
            this.Icon = "\xe6aa";
        }
        public override bool DoExcute(out string errMsg)
        {
            //0.获取通讯设备
            GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            try
            {
                vCommPdca.Open();
            }
            catch { }
            if(TestUrl!="")
            {
                _sfcHelper = new SFCHelper(TestUrl);
            }
            else
            {
                _sfcHelper = new SFCHelper(vCommPdca);
            }

            switch (BlackType)
            {
                case BlackCarrierType.Query:
                    //1.查询载具是否在黑名单内,如果在直接解绑
                    if (_sfcHelper.QueryCarrierBlackList(SN, ReturnOKValue, out string msg))
                    {
                        //判断是否要解绑
                        if (Unbind)
                        {
                            _sfcHelper.UnbindCarrier(SN, out msg);
                        }
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                    break;

                case BlackCarrierType.Add:
                    //3.2 如果是首站且设定次数为1，直接打出
                    if (FirstStation && SetCount == 1)
                    {
                        MyOwner.DbHelper.BatchDelete(SN);
                        //判断是否要解绑
                        if (Unbind)
                        {
                            _sfcHelper.UnbindCarrier(SN, out errMsg);
                        }
                        _sfcHelper.AddCarrierBlackList(SN, ReturnOKValue, out errMsg);
                        CurrentCount = 1;
                        Result = true;
                        return true;
                    }
                    //如果不是首站，需要通过WIP获取
                    if (!FirstStation)
                    {
                        SN = _sfcHelper.GetCarrierSN(SN, out errMsg);
                    }
                    //2.计黑&3.打黑
                    //需要连续N次，如果不连续，直接清空数据库
                    if(NG)
                    {
                        CurrentCount = MyOwner.DbHelper.GetCarrierCount(SN) + 1;
                    }
                    else
                    {
                        MyOwner.DbHelper.BatchDelete(SN);
                        CurrentCount = 0;
                    }
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"载具SN{SN},第{CurrentCount}次出现");
                    if (CurrentCount >= SetCount)
                    {
                        //删除当前载具所有数据
                        MyOwner.DbHelper.BatchDelete(SN);
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"载具SN{SN},第{CurrentCount}次出现，执行清空数据库信息");
                        _sfcHelper.AddCarrierBlackList(SN, ReturnOKValue, out msg);
                        //判断是否要解绑
                        if(Unbind)
                        {
                            _sfcHelper.UnbindCarrier(SN, out msg);
                        }
                        Result = true;
                    }
                    else
                    {
                        MyOwner.DbHelper.SaveNGCarrierId(SN, CurrentCount);
                        Result = false;
                    }
                    break;
            }
            return base.DoExcute(out errMsg);
        }
    }
}
