#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ContentItemConverter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Models
* 文 件 名:       ContentItemConverter.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef123456789a
* 创建年份:       2026
************************************************************************************/

#endregion

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Luster.Common.Assets.FloatingInfo.Models
{
    /// <summary>
    /// ContentItem的JSON转换器，    /// 用于处理ContentItem抽象类的序列化和和反序列化
    /// </summary>
    public class ContentItemConverter : JsonConverter
    {
        /// <summary>
        /// 可以转换的类型
        /// </summary>
        private static readonly Type[] _contentItemTypes = new Type[]
        {
            typeof(TextContentItem),
            typeof(ImageContentItem)
        };

        /// <summary>
        /// 判断是否可以转换
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ContentItem);
        }

        /// <summary>
        /// 读取JSON并转换为正确的类型
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // 加载JObject
            var jsonObject = JObject.Load(reader);

            // 获取ContentType属性来确定具体类型
            var contentTypeStr = jsonObject["ContentType"]?.ToString();
            
            // 解析ContentType字符串为枚举
            if (!Enum.TryParse<ContentType>(contentTypeStr, out var contentType))
            {
                throw new JsonSerializationException($"Unknown ContentType: {contentTypeStr}");
            }

            // 根据ContentType创建正确的实例
            ContentItem contentItem;
            switch (contentType)
            {
                case ContentType.Text:
                    contentItem = new TextContentItem();
                    break;
                case ContentType.Image:
                    contentItem = new ImageContentItem();
                    break;
                default:
                    throw new JsonSerializationException($"Unknown ContentType: {contentTypeStr}");
            }

            // 使用JObject.CreateReader()创建JsonReader来填充对象属性
            serializer.Populate(jsonObject.CreateReader(), contentItem);

            return contentItem;
        }

        /// <summary>
        /// 是否可以写入JSON
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 写入JSON
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
