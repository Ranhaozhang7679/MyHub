using Luster.Common.DataStruct;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.LightTuning.Functions;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Luster.Motion.LightTuning.ViewModel
{
    /// <summary>
    /// 光调面板 ViewModel（TES-64 P6-F）。
    /// 消费 P2-A BX 光源设备（<see cref="ILightController"/>）+ ParamGrid 数据契约 <see cref="LightTuningParam"/>。
    /// </summary>
    /// <remarks>
    /// <b>设备绑定</b>：通道亮度走 <c>ILightController.SetChannelAndVal/GetChannelIntensity</c>（接口契约，通用）；
    /// 触发模式/分组参数源端在 <c>LightControllerBX</c> 上为 <c>SetTrigMode()/SetGroupParm(int)</c>（厂家方法，未进
    /// <c>ILightController</c> 接口）。为保持非侵入（不改接口、不硬引用设备程序集，避免 Devices/ 重复加载的类型一致性风险），
    /// 此处以反射调用 BX 厂家方法；待全栈把 <c>SetTrigMode/SetGroupParm</c> 提升进 <c>ILightController</c>
    /// （或新增 <c>IBxLightController</c>）后切换为强类型调用（已升级项目经理协调，见 issue 评论）。
    /// </remarks>
    public class LightTuningContentVM : MotionVM
    {
        private readonly IDeviceEngine _deviceEngine;
        private string _status = "就绪";
        private LightChannelItem _selectedChannel;

        /// <summary>ParamGrid 绑定的光调参数数据契约</summary>
        public LightTuningParam ModuleObj { get; } = new LightTuningParam();

        /// <summary>每通道亮度表（DataGrid 编辑 + 回读）</summary>
        public ObservableCollection<LightChannelItem> Channels { get; } = new ObservableCollection<LightChannelItem>();

        /// <summary>当前选中通道</summary>
        public LightChannelItem SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty(ref _selectedChannel, value);
        }

        /// <summary>状态/日志</summary>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand RefreshFeedbackCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand SyncFromParamCommand { get; }

        public LightTuningContentVM(IDeviceEngine deviceEngine)
            : base()
        {
            _deviceEngine = deviceEngine;
            ApplyCommand = new DelegateCommand(Apply);
            RefreshFeedbackCommand = new DelegateCommand(RefreshFeedback);
            SaveCommand = new DelegateCommand(Save);
            LoadCommand = new DelegateCommand(Load);
            SyncFromParamCommand = new DelegateCommand(SyncChannelsFromParam);

            // 初始化通道表（默认 8 通道，对齐源端 mLightNum）
            RebuildChannels(ModuleObj.ChannelCount);
            SyncChannelsFromParam();
        }

        /// <summary>解析首个 ILightController 设备（P2-A BX 落 Devices/ 后由 DeviceEngine 反射发现）</summary>
        private ILightController ResolveDevice()
        {
            try
            {
                var devs = _deviceEngine?.GetRealDevices(typeof(ILightController));
                return devs?.OfType<ILightController>().FirstOrDefault();
            }
            catch (Exception ex)
            {
                Status = $"设备解析异常：{ex.Message}";
                return null;
            }
        }

        /// <summary>下发：通道亮度 + 触发模式 + 分组参数到 BX 设备</summary>
        private void Apply()
        {
            var dev = ResolveDevice();
            if (dev == null)
            {
                Status = "⚠️ 未发现 ILightController 设备（待 P2-A BX 设备接入 Devices/）";
                return;
            }

            // 1) 通道亮度：逐通道 SetChannelAndVal（ILightController 接口契约）
            foreach (var ch in Channels)
            {
                try { dev.SetChannelAndVal(ch.Channel, ch.Width); }
                catch (Exception ex) { Status = $"通道{ch.Channel}下发异常：{ex.Message}"; return; }
            }

            // 2) 触发模式：反射设置设备 TriggerMode + 调 SetTrigMode()（BX 厂家方法）
            bool trigOk = TrySetTriggerMode(dev, ModuleObj.TriggerMode);

            // 3) 分组参数：反射调 SetGroupParm(Group)（BX 厂家方法）
            bool groupOk = TryInvokeBool(dev, "SetGroupParm", new object[] { ModuleObj.LightGroup });

            Status = $"✅ 已下发 {Channels.Count} 通道亮度 | 触发模式={ModuleObj.TriggerMode}({(trigOk ? "成功" : "跳过/失败")}) | 分组={ModuleObj.LightGroup}({(groupOk ? "成功" : "跳过/失败")})";
        }

        /// <summary>实时亮度反馈：逐通道 GetChannelIntensity 回读</summary>
        private void RefreshFeedback()
        {
            var dev = ResolveDevice();
            if (dev == null)
            {
                Status = "⚠️ 未发现 ILightController 设备，无法回读";
                return;
            }

            foreach (var ch in Channels)
            {
                try
                {
                    int val = 0;
                    dev.GetChannelIntensity(ch.Channel, ref val);
                    ch.Feedback = val;
                }
                catch (Exception ex)
                {
                    Status = $"通道{ch.Channel}回读异常：{ex.Message}";
                    return;
                }
            }
            Status = $"✅ 已回读 {Channels.Count} 通道亮度";
        }

        /// <summary>保存光调配置到 XML（P8A 配方未就位前的独立落盘）</summary>
        private void Save()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "光调配置|*.xml",
                FileName = "LightTuningProfile.xml"
            };
            if (dlg.ShowDialog() != true) return;

            var dto = ToDto();
            var ser = new XmlSerializer(typeof(LightTuningProfileDto));
            using (var fs = File.Create(dlg.FileName))
            {
                ser.Serialize(fs, dto);
            }
            Status = $"✅ 已保存：{dlg.FileName}";
        }

        /// <summary>从 XML 加载光调配置</summary>
        private void Load()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "光调配置|*.xml" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var ser = new XmlSerializer(typeof(LightTuningProfileDto));
                using (var fs = File.OpenRead(dlg.FileName))
                {
                    var dto = (LightTuningProfileDto)ser.Deserialize(fs);
                    FromDto(dto);
                }
                Status = $"✅ 已加载：{dlg.FileName}";
            }
            catch (Exception ex)
            {
                Status = $"❌ 加载失败：{ex.Message}";
            }
        }

        /// <summary>通道总数变更后重建通道表</summary>
        private void RebuildChannels(int count)
        {
            Channels.Clear();
            for (int i = 0; i < count; i++)
            {
                Channels.Add(new LightChannelItem { Channel = i, Delay = ModuleObj.ChannelDelay, Width = ModuleObj.ChannelWidth });
            }
            SelectedChannel = Channels.FirstOrDefault();
        }

        /// <summary>把 ParamGrid 标量（当前通道/脉宽/延时/通道总数）同步进通道表</summary>
        private void SyncChannelsFromParam()
        {
            if (ModuleObj.ChannelCount != Channels.Count)
            {
                RebuildChannels(ModuleObj.ChannelCount);
            }
            if (SelectedChannel != null)
            {
                SelectedChannel.Width = ModuleObj.ChannelWidth;
                SelectedChannel.Delay = ModuleObj.ChannelDelay;
            }
        }

        // ---- DTO 映射 ----

        private LightTuningProfileDto ToDto()
        {
            var dto = new LightTuningProfileDto
            {
                LightGroup = ModuleObj.LightGroup,
                TriggerMode = ModuleObj.TriggerMode,
                ChannelCount = ModuleObj.ChannelCount,
                ChanelR = ModuleObj.ChanelR,
                ChanelG = ModuleObj.ChanelG,
                ChanelB = ModuleObj.ChanelB,
                ChanelMono = ModuleObj.ChanelMono,
                GrayTargetMono = ModuleObj.GrayTargetMono,
                BelongScreen = ModuleObj.BelongScreen,
                LinkEnable = ModuleObj.LinkEnable,
                LinkIntervalTime = ModuleObj.LinkIntervalTime,
            };
            foreach (var ch in Channels)
            {
                dto.Channels.Add(new LightChannelDto { Channel = ch.Channel, Delay = ch.Delay, Width = ch.Width });
            }
            return dto;
        }

        private void FromDto(LightTuningProfileDto dto)
        {
            ModuleObj.LightGroup = dto.LightGroup;
            ModuleObj.TriggerMode = dto.TriggerMode;
            ModuleObj.ChannelCount = dto.ChannelCount;
            ModuleObj.ChanelR = dto.ChanelR;
            ModuleObj.ChanelG = dto.ChanelG;
            ModuleObj.ChanelB = dto.ChanelB;
            ModuleObj.ChanelMono = dto.ChanelMono;
            ModuleObj.GrayTargetMono = dto.GrayTargetMono;
            ModuleObj.BelongScreen = dto.BelongScreen;
            ModuleObj.LinkEnable = dto.LinkEnable;
            ModuleObj.LinkIntervalTime = dto.LinkIntervalTime;

            Channels.Clear();
            foreach (var ch in dto.Channels)
            {
                Channels.Add(new LightChannelItem { Channel = ch.Channel, Delay = ch.Delay, Width = ch.Width });
            }
            SelectedChannel = Channels.FirstOrDefault();
        }

        // ---- 反射调用 BX 厂家方法（非侵入：不引用 Luster.SimDevice.Light，避免 Devices/ 重复加载类型一致性风险）----

        /// <summary>设置触发模式：写设备 TriggerMode 属性 + 调 SetTrigMode()</summary>
        private bool TrySetTriggerMode(object dev, int mode)
        {
            try
            {
                var t = dev.GetType();
                var prop = t.GetProperty("TriggerMode");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(dev, Convert.ChangeType(mode, prop.PropertyType));
                }
                return TryInvokeBool(dev, "SetTrigMode", null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>反射调用返回 bool 的无参/带参方法（如 SetTrigMode/SetGroupParm）</summary>
        private bool TryInvokeBool(object dev, string methodName, object[] args)
        {
            try
            {
                var mi = dev.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (mi == null) return false;
                var ret = mi.Invoke(dev, args ?? Array.Empty<object>());
                return ret is bool b ? b : true;
            }
            catch
            {
                return false;
            }
        }
    }
}
