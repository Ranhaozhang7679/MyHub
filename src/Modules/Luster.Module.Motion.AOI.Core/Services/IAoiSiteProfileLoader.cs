using Luster.Module.Motion.AOI.Core.Models;

namespace Luster.Module.Motion.AOI.Core.Services
{
    /// <summary>
    /// 站点 profile 加载器。负责从 XML 反序列化为 <see cref="AoiSiteProfile"/>，不做语义校验。
    /// </summary>
    public interface IAoiSiteProfileLoader
    {
        /// <summary>
        /// 从文件路径加载站点 profile。
        /// </summary>
        /// <param name="path">profile XML 路径。</param>
        /// <returns>解析得到的 <see cref="AoiSiteProfile"/>。如果文件不存在/格式错误，应抛 <see cref="AoiSiteProfileException"/>。</returns>
        AoiSiteProfile LoadFromFile(string path);
    }
}
