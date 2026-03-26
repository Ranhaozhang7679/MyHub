#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PlcAddressDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       PlcAddressDialogVM.cs
* 创建时间:       2022/11/19 11:01:58
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      58c324ff-72f2-47c4-a01d-f1613c74a372
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/19 11:01:58
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Controls;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class PlcAddressDialogVM : DialogVM
    {
        private VPlc _vPlc;
        private Dictionary<DataType, int> _addressLengthDic = new Dictionary<DataType, int>();
        /// <summary>
        /// 地址
        /// </summary>
        private string _address;
        [System.ComponentModel.DataAnnotations.Required]
        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }


        /// <summary>
        /// 地址数量
        /// </summary>
        private int _addressNum = 1;
        public int AddressNum
        {
            get => _addressNum;
            set
            {
                SetProperty(ref _addressNum, value);
            }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public List<KeyValue> DataTypes { get; set; }

        /// <summary>
        /// PLC读取类型
        /// </summary>
        public List<KeyValue> PlcAddrTypes { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        [System.ComponentModel.DataAnnotations.Required]
        public DataType DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }

        /// <summary>
        /// 读取类型
        /// </summary>
        private PlcAddrType _plcAddrType;
        [System.ComponentModel.DataAnnotations.Required]
        public PlcAddrType PlcAddrType
        {
            get { return _plcAddrType; }
            set { SetProperty(ref _plcAddrType, value); }
        }

        private double _value;
        public double Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        /// <summary>
        /// 地址描述
        /// </summary>
        private string _desc;
        public string Desc
        {
            get { return _desc; }
            set { SetProperty(ref _desc, value); }
        }

        protected PlcAddressDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            DataTypes = typeof(DataType).EnumToDataSource();
            PlcAddrTypes = typeof(PlcAddrType).EnumToDataSource();
            InitLengthDic();
        }

        private void InitLengthDic()
        {
            _addressLengthDic.Add(DataType.Bool, 1);
            _addressLengthDic.Add(DataType.Short, 1);
            _addressLengthDic.Add(DataType.Int, 2);
            _addressLengthDic.Add(DataType.Long, 4);
            _addressLengthDic.Add(DataType.Float, 2);
            _addressLengthDic.Add(DataType.Double, 4);
            _addressLengthDic.Add(DataType.String, 2);//带确认
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            var plc = parameters.GetValue<VPlc>("PlcDevice");
            if (plc != null)
            {
                _vPlc = plc;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            if (_vPlc != null)
            {
                for (int i = 0; i < AddressNum; i++)
                {     
                    var address = GetAddress(Address, i, DataType);
                    var exist = _vPlc.Address.Where(x => x.Address == address).FirstOrDefault();
                    if (exist != null)
                    {
                        throw new FriendlyException($"Plc地址已设置，地址：{address}");
                    }
                    var plcAddr = new PlcAddr()
                    {
                        Address = address,
                        AddrType = PlcAddrType,
                        DataType = DataType,
                        Desc = Desc,
                        Value = Value
                    };
                    _vPlc.Address.Add(plcAddr);
                }
                // 根据数据类型分组
                _vPlc.UpdateAddr();
            }
        }

        private string GetAddress(string address, int index, DataType dataType)
        {
            int tempAddress;
            int.TryParse(address, out tempAddress);
            var finalAddress = tempAddress + _addressLengthDic[dataType] * index;
            return finalAddress.ToString();
        }
    }
}
