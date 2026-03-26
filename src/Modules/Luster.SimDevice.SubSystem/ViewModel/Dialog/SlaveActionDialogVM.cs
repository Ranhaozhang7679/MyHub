using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class SlaveActionDialogVM : DialogVM
    {
        /// <summary>
        /// 当前对象
        /// </summary>
        private VCommuncation current;

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 输入列的名称
        /// </summary>
        private string _headerName;
        public string HeaderName
        {
            get { return _headerName; }
            set { SetProperty(ref _headerName, value); }
        }

        /// <summary>
        /// 输入列的名称
        /// </summary>
        private string _headerValue;
        public string HeaderValue
        {
            get { return _headerValue; }
            set { SetProperty(ref _headerValue, value); }
        }


        private string _slaveNum="1";
        /// <summary>
        /// 从站站号
        /// </summary>
        [Required]
        public string SlaveNum { get => _slaveNum; set => SetProperty(ref _slaveNum, value); }

        protected SlaveActionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {

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
                HeaderName=c.UserParam[0];
                HeaderValue=c.UserParam[1];
            }

            // 动作模型
            if (parameters.TryGetValue<SocketActionModel>("ActionModel", out var action))
            {
                if (action != null)
                {
                    Name = action.Name;
                    SlaveNum = action.Value;
                }
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            if (current.Actions.Any(u => u.Name == Name))
            {
                throw new FriendlyException($"活动名称:{Name}已经存在!");
            }

            var newAction = new SocketAction()
            {
                Name = Name,
                Value = SlaveNum
            };
            current.Actions.Add(newAction);

            result.Parameters.Add("Action", newAction);
           
}
    }
}
