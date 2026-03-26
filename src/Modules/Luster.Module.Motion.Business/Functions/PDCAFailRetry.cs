using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.SFC;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.Business.Functions
{
    public class PDCAFailRetry : MotionFunction
    {
        public enum RetryType
        {
            [Description("数据发送")]
            SendData,
        }

        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        [NotEmpty]
        [Parameter("RetryMode", 2, CN = "动作类型", DefaultV = RetryType.SendData)]
        public RetryType RetryMode { get; set; }

        [Parameter("SN", 2, CN = "SNCode")]
        public string RetrySN { get; set; } = string.Empty;

        public PDCAFailRetry()
        {
            this.Tips = "PDCA失败补传";
            this.Icon = "\xe6c9";
        }

        private SFCHelper _sfcHelper = null;

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            bool result = false;

            var RetrySNList = GetFailSNList();
            string strLog = string.Join(",", RetrySNList);
            if (string.IsNullOrEmpty(RetrySN))
            {
                MyOwner.OnLog(LogType.Warning, $"请填写任意失败的SN: {strLog}");
                return true;
            }

            if (string.IsNullOrEmpty(RetrySN) || !RetrySNList.Contains(RetrySN))
            {
                throw new FriendlyException($"PDCA数据文件不存在: {RetrySN}");
            }

            //2.连接通讯设备
            GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            try
            {
                vCommPdca.Open();
            }
            catch (Exception)
            {
                // 连接失败的通讯支持重新赋值
                vCommPdca = null;
                _sfcHelper = null;
                throw new FriendlyException("通讯失败");
            }
            if (_sfcHelper == null)
            {
                _sfcHelper = new SFCHelper(vCommPdca);
            }
            switch (RetryMode)
            {
                case RetryType.SendData:
                    var sendStr = GetSendData();
                    _sfcHelper?.SendPDCAData(sendStr, "PDCA上传数据", out errMsg);
                    break;
                default:
                    break;
            }

            result = string.IsNullOrEmpty(errMsg) ? true : false;
            return result;
        }

        /// <summary>
        /// 从本地PDACError目录读取所有失败的SN（文件名去掉扩展名）
        /// </summary>
        public List<string> GetFailSNList()
        {
            var snList = new List<string>();
            string dir = Path.Combine(MyOwner.DeviceEngine.RecipeSlnPath, "PDACError");
            if (!Directory.Exists(dir))
                return snList;

            var files = Directory.GetFiles(dir, "*.json");
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (!string.IsNullOrWhiteSpace(fileName))
                    snList.Add(fileName);
            }
            return snList;
        }


        private string GetSendData()
        {
            string reslut = string.Empty;
            string path = Path.Combine(MyOwner.DeviceEngine.RecipeSlnPath, "PDACError");
            string fileName = $"{RetrySN}.json";
            string filePath = Path.Combine(path, fileName);
            if (!File.Exists(filePath))
            {
                throw new FriendlyException($"PDCA数据文件不存在: {filePath}");
            }
            try
            {
                reslut = File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new FriendlyException($"读取PDCA数据文件失败: {filePath}, 错误信息: {ex.Message}");
            }

            return reslut;
        }


    }
}
