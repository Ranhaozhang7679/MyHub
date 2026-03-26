#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SocketActionModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       SocketActionModel.cs
* 创建时间:       2022/8/3 13:39:10
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a3bb0e62-4b03-497b-be45-a20dc0db93c3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/3 13:39:10
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Common.DataStruct.Enums;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class SocketActionModel : BindableBase
    {
        /// <summary>
        /// 当前对象
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetProperty(ref _name, value); if (Tag != null)
                {
                    Tag.Command = value;
                }
            }
        }

        /// <summary>
        /// 是否读取命令
        /// </summary>
        private bool _isRead;
        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                SetProperty(ref _isRead, value);
                if (Tag != null)
                {
                    Tag.ActionType = value ? ActionType.Read : ActionType.Write;
                }
            }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        public DataType DataType
        {
            get { return _dataType; }
            set
            {
                SetProperty(ref _dataType, value);
                if (Tag != null)
                {
                    Tag.DataType = value;
                }
            }
        }

        /// <summary>
        /// 命令
        /// </summary>
        private string _command;
        public string Command
        {
            get { return _command; }
            set
            {
                SetProperty(ref _command, value);
                if (Tag != null)
                {
                    Tag.Command = value;
                }
            }
        }

        /// <summary>
        /// 对应的值
        /// </summary>
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                SetProperty(ref _value, value);
                if (Tag != null)
                {
                    Tag.Value = value;
                }
            }
        }

        /// <summary>
        /// Tag
        /// </summary>
        public SocketAction Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketActionModel()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sAction"></param>
        public SocketActionModel(SocketAction sAction)
        {
            Tag = sAction;
            Value = sAction.Value;
            Command = sAction.Command;
            Name = sAction.Name;
            DataType = sAction.DataType;
        }
    }
}