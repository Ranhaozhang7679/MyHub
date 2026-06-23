using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 排线SN管理 — 保存/读取(预占)/标记/归还
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
            MarkUsed,

            [Description("归还SN")]
            Release
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
            UnknownError = 9,

            [Description("已使用不可归还")]
            AlreadyUsed = 10
        }

        #region 输入参数

        [NotEmpty]
        [Parameter("操作模式", 0, CN = "操作模式", DefaultV = CableSNMode.Save)]
        public CableSNMode Mode { get; set; }

        [DependOn("Mode", CableSNMode.Save, CableSNMode.MarkUsed, CableSNMode.Release)]
        [Parameter("排线SN码", 5, CN = "排线SN码", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        #endregion

        #region 输出参数

        [Parameter("文件路径", 1, CN = "文件路径", ParamType = ParamType.OUT)]
        public string FilePath { get; set; }

        [Parameter("操作结果(1:成功 2:SN为空 3:文件不存在 4:无数据 5:SN不存在 6:写入失败 7:所有SN已使用 8:SN已存在 9:未知异常 10:已使用不可归还)", 10, CN = "操作结果", ParamType = ParamType.OUT)]
        public int Result { get; set; }

        [DependOn("Mode", CableSNMode.Read)]
        [Parameter("读取到的SN", 11, CN = "读取到的SN", ParamType = ParamType.OUT)]
        public string OutSN { get; set; }

        #endregion

        public CableSNManager()
        {
            Tips = "排线SN管理：保存SN、读取(预占)最早未使用SN、标记已使用、归还预占SN";
            Icon = "\xe679";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            Result = (int)CableSNResult.Success;
            OutSN = string.Empty;
            FilePath = DefaultFilePath;

            // 设备空跑模式：直接跳过执行
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
                    case CableSNMode.Release:
                        DoRelease();
                        break;
                }
            }
            catch (Exception ex)
            {
                Result = (int)CableSNResult.UnknownError;
                MyOwner.OnLog(LogType.Error, $"[排线SN管理] 异常：{ex.Message}");
            }

            // 所有业务动作：模块始终绿色，业务结果通过 Result 值判断
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 保存SN到CSV文件，若已存在则结果为已存在(码8)
        /// </summary>
        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(SN))
            {
                Result = (int)CableSNResult.SNEmpty;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 保存失败：SN码为空");
                return;
            }

            Result = CableSNFileHelper.SaveSN(DefaultFilePath, SN);

            switch (Result)
            {
                case 1: MyOwner.OnLog(LogType.Info, $"[排线SN管理] 保存成功（新增）：{SN}"); break;
                case 3: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 保存失败：文件处理异常 {DefaultFilePath}"); break;
                case 6: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 保存失败：无法写入文件 {DefaultFilePath}"); break;
                case 8: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 保存失败：SN {SN} 已存在"); break;
                case 9: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 保存失败：未知异常 {DefaultFilePath}"); break;
            }
        }

        /// <summary>
        /// 读取最早未使用SN并预占（模块始终不变红）
        /// </summary>
        private void DoRead()
        {
            Result = CableSNFileHelper.OccupyEarliestSN(DefaultFilePath, out var sn);
            OutSN = sn;

            switch (Result)
            {
                case 1: MyOwner.OnLog(LogType.Info, $"[排线SN管理] 读取预占成功：{sn}"); break;
                case 3: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 读取失败：文件不存在 {DefaultFilePath}"); break;
                case 4: MyOwner.OnLog(LogType.Error, "[排线SN管理] 读取失败：文件中无数据"); break;
                case 6: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 读取失败：文件回写异常 {DefaultFilePath}"); break;
                case 7: MyOwner.OnLog(LogType.Error, "[排线SN管理] 读取失败：所有SN均已使用或预占"); break;
                case 9: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 读取失败：未知异常 {DefaultFilePath}"); break;
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

            Result = CableSNFileHelper.MarkUsedSN(DefaultFilePath, SN, out var alreadyUsed);

            switch (Result)
            {
                case 1:
                    MyOwner.OnLog(LogType.Info, alreadyUsed
                        ? $"[排线SN管理] 标记跳过：SN {SN} 已是使用状态"
                        : $"[排线SN管理] 标记成功：SN {SN}");
                    break;
                case 3: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：文件不存在 {DefaultFilePath}"); break;
                case 4: MyOwner.OnLog(LogType.Error, "[排线SN管理] 标记失败：文件中无数据"); break;
                case 5: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：SN {SN} 不存在于文件中"); break;
                case 6: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：文件回写异常 {DefaultFilePath}"); break;
                case 9: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 标记失败：未知异常 {DefaultFilePath}"); break;
            }
        }

        /// <summary>
        /// 归还预占的SN（预占→未使用），用于配方异常分支回收；已使用则失败(码10)
        /// </summary>
        private void DoRelease()
        {
            if (string.IsNullOrWhiteSpace(SN))
            {
                Result = (int)CableSNResult.SNEmpty;
                MyOwner.OnLog(LogType.Error, "[排线SN管理] 归还失败：SN码为空");
                return;
            }

            Result = CableSNFileHelper.ReleaseSN(DefaultFilePath, SN);

            switch (Result)
            {
                case 1: MyOwner.OnLog(LogType.Info, $"[排线SN管理] 归还成功：SN {SN} 已恢复未使用"); break;
                case 3: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 归还失败：文件不存在 {DefaultFilePath}"); break;
                case 4: MyOwner.OnLog(LogType.Error, "[排线SN管理] 归还失败：文件中无数据"); break;
                case 5: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 归还失败：SN {SN} 不存在于文件中"); break;
                case 6: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 归还失败：文件回写异常 {DefaultFilePath}"); break;
                case 9: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 归还失败：未知异常 {DefaultFilePath}"); break;
                case 10: MyOwner.OnLog(LogType.Error, $"[排线SN管理] 归还失败：SN {SN} 已使用不可归还"); break;
            }
        }
    }
}
