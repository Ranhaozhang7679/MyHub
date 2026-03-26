using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.Attributes;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.VDevice
{
    public class VFlyingPhoto : VirtualDeviceBase
    {
        public override int Sort => 15;

        /// <summary>
        /// 真实飞拍模块名称
        /// </summary>
        public string FlyingPhotoName { get; set; }

        /// <summary>
        /// 从站号
        /// </summary>
        public int SlaveNo { get; set; }

        public IFlyingPhoto flyingPhoto = null;

        //E4O4飞拍模块标志位
        public bool IsE4o4 = false;

        /// <summary>
        /// 飞拍模块配置
        /// </summary>
        /// <param name="hardDevice"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {

            //this .GetDevice ().Brand 

            base.SetDevice(hardDevice, out errMsg);

            flyingPhoto = hardDevice as IFlyingPhoto;
            if (flyingPhoto == null)
            {
                errMsg = "设备不是飞拍模块对象";
                return false;
            }

            // 硬件中Add时候进行的初始化
            ProcessAction(() =>
            {
                // 机器人初始化
                if (!hardDevice.HasInit())
                {
                    hardDevice.InitApi();

                    // 打开设备
                    hardDevice.Open();

                    // 启用
                    hardDevice.SetEnabled(true);
                }
            });

            IsE4o4 = this.GetDevice().Brand == "凌臣飞拍E4O4";

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 连接总线
        /// </summary>
        public void Connect()
        {
            ProcessAction(() =>
            {
                flyingPhoto.Connect(SlaveNo);
            }, null, true);
        }

        /// <summary>
        /// 设置当前编码器值为0
        /// </summary>
        public void ClearEncoderData()
        {
            ProcessAction(() =>
            {
                flyingPhoto.ClearEncoderData(SlaveNo);
            }, null, true);
        }

        /// <summary>
        /// 获取当前编码器的数据
        /// </summary>
        /// <param name="data"></param>
        public void GetEncoderCurData(out long data)
        {
            Random random = new Random();
            data = random.Next(100, 1000);
            return;
            long curData = 0;
            ProcessAction(() =>
            {
                flyingPhoto.GetEncoderCurData(0,out curData); ;
            }, null, true);
            data = curData;
        }

        /// <summary>
        /// 设置压入触发通道的点位
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="position"></param>
        public void SetTriggerPos(int trigNo, long position)
        {
            ProcessAction(() =>
            {
                flyingPhoto.SetTriggerPos(trigNo, position);
            }, null, true);
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="ltcNo"></param>
        /// <param name="type"></param>
        public void SetTriggerParm(int trigNo, int ltcNo, int type)
        {
            ProcessAction(() =>
            {
                flyingPhoto.SetTriggerParm(trigNo, ltcNo, type);
            }, null, true);
        }

        /// <summary>
        /// 设置脉宽
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="width"></param>
        public void SetPulseWidth(int trigNo, int width)
        {
            ProcessAction(() =>
            {
                flyingPhoto.SetPulseWidth(trigNo, width);
            }, null, true);
        }

        public void SetTriggerCameraOffset(int trigNo, long offset)
        {
            ProcessAction(() =>
            {
                flyingPhoto.SetTriggerCameraOffset(trigNo, offset);
            }, null, true);
        }

        public void GetTriggerCount(int trigNo, ref int count)
        {
            int curCount = 0;
            ProcessAction(() =>
            {
                flyingPhoto.GetTriggerCount(trigNo, ref curCount);
            }, null, false);
            count = curCount;
        }

        public void GetTriggerFifoCount(int trigNo, int Type, ref int count)
        {
            int curCount = 0;
            ProcessAction(() =>
            {
                flyingPhoto.GetTriggerFifoCount(trigNo, Type, ref curCount);
            }, null, false);
            count = curCount;
        }


        [Ignore]
        public List<FlyingPhotoPosition> Positions { get; set; } = new List<FlyingPhotoPosition>();

        public void AddPosition(string name, long value, int trigger,int encoderNo=0)
        {
            if (Positions.Any(u => u.Name == name))
            {
                throw new FriendlyException($"点位名称:{name}已存在!");
            }

            Positions.Add(new FlyingPhotoPosition
            {
                Key = Guid.NewGuid(),
                Name = name,
                Position = value,
                EncoderNo= encoderNo,
                TriggerNo = trigger,
                FlyingPhoto = this
            });
            Engine.IsNeedSave = true;
        }

        /// <summary>
        /// 移除点位
        /// </summary>
        /// <param name="name"></param>
        public void RemovePostion(string name)
        {
            int num = Positions.RemoveAll(u => u.Name == name);
            if (num > 0)
            {
                Engine.IsNeedSave = true;
            }
        }

        #region 导入和导出

        public override XElement ExportXml()
        {
            var xml = base.ExportXml();
            if (Positions.Count > 0)
            {
                XElement xPos = new XElement(nameof(Positions));
                foreach (var item in Positions)
                {
                    xPos.Add(item.ExportXml());
                }

                xml.Add(xPos);
            }


            return xml;

        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);
            Positions.Clear();

            // 解析点位
            var xPos = xElement.Element(nameof(Positions));
            if (xPos != null)
            {
                foreach (var xItem in xPos.Elements())
                {
                    FlyingPhotoPosition pos = new FlyingPhotoPosition();
                    pos.ParserXml(xItem);
                    pos.FlyingPhoto = this;
                    Positions.Add(pos);
                }
            }
        }

        #endregion

    }
}
