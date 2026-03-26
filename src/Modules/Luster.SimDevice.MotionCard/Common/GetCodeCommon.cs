using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.MotionCard.Common
{
    public class GetCodeCommon
    {
        /// <summary>
        /// 根据ret数值查找轴卡错误码描述
        /// </summary>
        /// <param name="ret">错误码</param>
        /// <returns>错误描述</returns>
        public static string GetCardErrorMsg(int ret,int cardType) //1:LC; 2:LS
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string jsonFileName = cardType switch
                {
                    1 => "CardErrCode_LC.json",
                    2 => "CardErrCode_LS.json",
                    _ => null
                };
                if (jsonFileName == null)
                    return $"未知卡类型:{cardType}";

                string jsonPath = Path.Combine(exeDir, "CardErrorCode", jsonFileName);
                if (!File.Exists(jsonPath))
                    return $"ReadError";

                var json = File.ReadAllText(jsonPath);
                var jObj = JObject.Parse(json);

                var value = jObj[ret.ToString()];
                if (value != null)
                    return value.ToString();

                return $"未知错误码:{ret}";
            }
            catch
            {
                return $"ReadError";
            }
        }

        /// <summary>
        /// 根据errCode查找ASDA_B3伺服错误名称
        /// </summary>
        /// <param name="pBufAxis"></param>
        /// <param name="pBuf_Cad"></param>
        /// <returns></returns>
        public static string GetASDAB3ErrorNameOrByCardCode(uint pBufAxis, uint pBuf_Cad)
        {
            try
            {
                string errCode = $"AL{pBufAxis:X3}";
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string jsonPath = Path.Combine(exeDir, "CardErrorCode", "Serve_err_ASDA_B3.json");
                if (!File.Exists(jsonPath))
                    return $"{errCode}";

                var json = File.ReadAllText(jsonPath);
                var jObj = JObject.Parse(json);

                // 先按errCode查找
                var errObj = jObj[errCode];
                if (errObj != null)
                {
                    var name = errObj["故障名称"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        return $"{errCode}:{name}";
                }
                else
                {
                    return $"{errCode}";
                }

                // 如果未找到，则遍历所有项，按"错误码"字段查找
                //string pBufStr = $"0x{pBuf_Cad:X}";
                //foreach (var prop in jObj.Properties())
                //{
                //    var item = prop.Value as JObject;
                //    if (item != null && item["错误码"]?.ToString() == pBufStr)
                //    {
                //        var name = item["故障名称"]?.ToString();
                //        if (!string.IsNullOrEmpty(name))
                //            return name;
                //    }
                //}

                return $"NotFound";
            }
            catch (Exception ex)
            {
                return $"ReadError";
            }
        }

        /// <summary>
        /// 高创LDHD伺服错误名称
        /// </summary>
        /// <param name="pBufAxis"></param>
        /// <param name="pBuf_Cad"></param>
        /// <returns></returns>
        public static string GetLDHDErrorNameOrByCardCode(uint pBufAxis, uint pBuf_Cad)
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string jsonPath = Path.Combine(exeDir, "CardErrorCode", "Serve_err_LDHD2.json");
                if (!File.Exists(jsonPath))
                    return $"ReadError";
                var json = File.ReadAllText(jsonPath);
                var jObj = JObject.Parse(json);

                //string errCode = $"AL{pBufAxis:X}";

                //// 先按errCode查找
                //var errObj = jObj[errCode];
                //if (errObj != null)
                //{
                //    var name = errObj["故障名称"]?.ToString();
                //    if (!string.IsNullOrEmpty(name))
                //        return name;
                //}

                // 如果未找到，则遍历所有项，按"错误码"字段查找
                string pBufStr = $"0x{pBuf_Cad:X4}";
                foreach (var prop in jObj.Properties())
                {
                    var item = prop.Value as JObject;
                    if (item != null && item["错误码"]?.ToString() == pBufStr)
                    {
                        var name = item["故障名称"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                            return pBufStr + ":" + name;
                    }
                    else
                    {
                        return pBufStr;
                    }
                }

                return $"NotFound";
            }
            catch (Exception ex)
            {
                return $"ReadError";
            }
        }


        /// <summary>
        /// 根据errCode查找SV630N伺服错误名称
        /// </summary>
        /// <param name="errCode">如"E101.0"</param>
        /// <returns>故障名称字符串</returns>
        public static string GetSV630NErrorNameOrByCardCode(uint pBufAxis, uint pBuf_Cad)
        {
            try
            {
                ushort high16 = (ushort)(pBufAxis >> 16 & 0xFFFF);
                ushort low16 = (ushort)(pBufAxis & 0xFFFF);
                int lastThree = high16 & 0xFFF; //提取后三位（低12位）
                int fourth = high16 >> 12 & 0xF; //提取第四位（高4位）

                // 组合成最终错误码
                string errCode = $"E{lastThree:X3}.{fourth:X}";
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string jsonPath = Path.Combine(exeDir, "CardErrorCode", "Serve_err_SV630N.json");
                if (!File.Exists(jsonPath))
                    return $"{errCode}";

                var json = File.ReadAllText(jsonPath);
                var jObj = JObject.Parse(json);

                // 先按errCode查找
                var errObj = jObj[errCode];
                if (errObj != null)
                {
                    var name = errObj["故障名称"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        return $"{errCode}:{name}";
                }
                else
                {
                    return $"{errCode}";
                }

                // 如果未找到，则遍历所有项，按"错误码"字段查找
                string pBufStr = $"0x{pBuf_Cad:X4}";
                foreach (var prop in jObj.Properties())
                {
                    var item = prop.Value as JObject;
                    if (item != null && item["错误码"]?.ToString() == pBufStr)
                    {
                        var name = item["故障名称"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                            return $"{errCode}:{name}";
                    }
                    else
                    {
                        return errCode;
                    }
                }

                return $"NotFound";
            }
            catch (Exception ex)
            {
                return $"ReadError";
            }
        }

    }
}
