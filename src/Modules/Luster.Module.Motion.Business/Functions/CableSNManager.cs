using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;
using System.Linq;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 排线SN管理 — 保存/读取/标记使用
    /// </summary>
    public class CableSNManager : MotionFunction
    {
        /// <summary>
        /// 固定CSV文件路径
        /// </summary>
        private const string DefaultFilePath = "D:\\排线数据\\排线数据.csv";

        /// <summary>
        /// 排线SN操作模式
        /// </summary>
        public enum CableSNMode
        {
            [Description("保存SN")]
            Save,

            [Description("读取SN")]
            Read,

            [Description("标记已使用")]
            MarkUsed
        }

        /// <summary>
        /// 操作结果码
        /// </summary>
        public enum CableSNResult
        {
            [Description("成功")]
            Success = 1,

            [Description("SN码为空")]
            SNEmpty = 2,

            [Description("文件不存在")]
            FileNotExist = 3,

            [Description("文件无数据")]
            FileEmpty = 4,

            [Description("SN不存在")]
            SNNotExist = 5,

            [Description("文件写入失败")]
            FileWriteFail = 6,

            [Description("所有SN已使用")]
            AllSNUsed = 7,

            [Description("SN已存在")]
            SNExist = 8,

            [Description("未知异常")]
            UnknownError = 9
        }

        #region 输入参数

        [NotEmpty]
        [Parameter("操作模式", 0, CN = "操作模式", DefaultV = CableSNMode.Save)]
        public CableSNMode Mode { get; set; }

        [DependOn("Mode", CableSNMode.Save, CableSNMode.MarkUsed)]
        [Parameter("排线SN码", 5, CN = "排线SN码", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        #endregion

        #region 输出参数

        [Parameter("文件路径", 1, CN = "文件路径", ParamType = ParamType.OUT)]
        public string FilePath { get; set; }

        [Parameter("操作结果(1:成功 2:SN为空 3:文件不存在 4:无数据 5:SN不存在 6:写入失败 7:所有SN已使用 8:SN已存在 9:未知异常)", 10, CN = "操作结果", ParamType = ParamType.OUT)]
        public int Result { get; set; }

        [DependOn("Mode", CableSNMode.Read)]
        [Parameter("读取到的SN", 11, CN = "读取到的SN", ParamType = ParamType.OUT)]
        public string OutSN { get; set; }

        #endregion

        public CableSNManager()
        {
            Tips = "排线SN管理：保存SN到CSV、读取最早未使用的SN、标记SN为已使用";
            Icon = "\xe679";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            Result = (int)CableSNResult.Success;
            OutSN = string.Empty;
            FilePath = DefaultFilePath;

            if (IsEmptyMode)
            {
                return false;
            }

            try
            {
                switch (Mode)
                {
                    case CableSNMode.Save:
                        DoSave();
                        break;
                    case CableSNMode.Read:
                        DoRead();
                        break;
                    case CableSNMode.MarkUsed:
                        DoMarkUsed();
                        break;
                }
            }
            catch (Exception ex)
            {
                Result = (int)CableSNResult.UnknownError;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 异常：{ex.Message}");
            }

            // 所有模式：模块始终绿色，通过Result值判断业务结果
            return base.DoExcute(out errMsg);

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 保存SN到CSV文件，若已存在则返回失败
        /// </summary>
        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(SN))
            {
                Result = (int)CableSNResult.SNEmpty;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 保存失败：SN码为空");
                return;
            }

            var records = CableSNFileHelper.ReadAllRecords(DefaultFilePath);
            var existing = records?.FirstOrDefault(r => r.SN == SN);

            if (existing != null)
            {
                Result = (int)CableSNResult.SNExist;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 保存失败：SN {SN} 已存在");
                return;
            }

            // SN不存在：新增记录
            if (!CableSNFileHelper.AppendRecord(DefaultFilePath, SN))
            {
                Result = (int)CableSNResult.FileWriteFail;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 保存失败：无法写入文件 {DefaultFilePath}");
                return;
            }

            MyOwner.OnLog(LogType.Info, $"[排线SN管理] 保存成功（新增）：{SN}");
        }

        /// <summary>
        /// 从CSV读取最早未使用的SN（模块始终不变红）
        /// </summary>
        private void DoRead()
        {
            var records = CableSNFileHelper.ReadAllRecords(DefaultFilePath);

            if (records == null)
            {
                Result = (int)CableSNResult.FileNotExist;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 读取失败：文件不存在 {DefaultFilePath}");
                return;
            }

            if (records.Count == 0)
            {
                Result = (int)CableSNResult.FileEmpty;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 读取失败：文件中无数据");
                return;
            }

            var target = records
                .OrderBy(r => r.Time)
                .FirstOrDefault(r => r.IsUsed == "否");

            if (target != null)
            {
                OutSN = target.SN;
                MyOwner.OnLog(LogType.Info, $"[排线SN管理] 读取成功：{target.SN}");
            }
            else
            {
                Result = (int)CableSNResult.AllSNUsed;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 读取失败：所有SN均已使用");
            }
        }

        /// <summary>
        /// 标记指定SN为已使用（模块始终不变红）
        /// </summary>
        private void DoMarkUsed()
        {
            if (string.IsNullOrWhiteSpace(SN))
            {
                Result = (int)CableSNResult.SNEmpty;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 标记失败：SN码为空");
                return;
            }

            var records = CableSNFileHelper.ReadAllRecords(DefaultFilePath);

            if (records == null)
            {
                Result = (int)CableSNResult.FileNotExist;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：文件不存在 {DefaultFilePath}");
                return;
            }

            if (records.Count == 0)
            {
                Result = (int)CableSNResult.FileEmpty;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 标记失败：文件中无数据");
                return;
            }

            var target = records.FirstOrDefault(r => r.SN == SN);

            if (target == null)
            {
                Result = (int)CableSNResult.SNNotExist;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：SN {SN} 不存在于文件中");
                return;
            }

            if (target.IsUsed == "是")
            {
                MyOwner.OnLog(LogType.Info, $"[排线SN管理] 标记跳过：SN {SN} 已是使用状态");
                return;
            }

            target.IsUsed = "是";
            if (!CableSNFileHelper.SaveAllRecords(DefaultFilePath, records))
            {
                Result = (int)CableSNResult.FileWriteFail;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：文件回写异常 {DefaultFilePath}");
                return;
            }

            MyOwner.OnLog(LogType.Info, $"[排线SN管理] 标记成功：SN {SN}");
        }
    }
}
