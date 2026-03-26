#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SocketActionDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       SocketActionDialogVM.cs
* 创建时间:       2022/8/3 13:35:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      35bafc7f-9036-4051-aecd-a7d80df801a1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/3 13:35:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    /// <summary>
    /// 新增Action
    /// </summary>
    public class SocketActionDialogVM : DialogVM
    {
        /// <summary>
        /// 当前对象
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 是否读取命令
        /// </summary>
        private bool _isRead;
        public bool IsRead
        {
            get { return _isRead; }
            set { SetProperty(ref _isRead, value); }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        public DataType DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value); }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private List<KeyValue> _dataTypes;
        public List<KeyValue> DataTypes { get => _dataTypes; set => SetProperty(ref _dataTypes, value); }

        /// <summary>
        /// 当前对象
        /// </summary>
        private VCommuncation current;

        private SocketActionModel socketAction;
        /// <summary>
        /// 构造汉
        /// </summary>
        /// <param name="_engine"></param>
        protected SocketActionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            DataTypes = typeof(DataType).EnumToDataSource();
        }

        /// <summary>
        /// 弹出
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            // 当前
            if (parameters.TryGetValue<VCommuncation>("Current", out var c))
            {
                current = c;
            }

            // 动作模型
            if (parameters.TryGetValue<SocketActionModel>("ActionModel", out var action))
            {
                socketAction = action;
                if (action != null)
                {
                    Name = action.Name;
                    IsRead = action.IsRead;
                    DataType = action.DataType;
                }
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            if (socketAction == null)
            {
                if (current.Actions.Any(u => u.Name == Name))
                {
                    throw new FriendlyException($"活动名称:{Name}已经存在!");
                }

                var newAction = new SocketAction()
                {
                    ActionType = IsRead ? ActionType.Read : ActionType.Write,
                    DataType = DataType,
                    Name = Name
                };
                current.Actions.Add(newAction);

                result.Parameters.Add("Action", newAction);
            }
            else
            {
                socketAction.Name = Name;
                socketAction.IsRead = IsRead;
                socketAction.DataType = DataType;
            }
        }
    }
}