using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Knot.Runtime.Utility.JsonConverter
{
    /// <summary>
    /// UnityEvent 的 Newtonsoft.Json 转换器
    /// 桥接 JsonUtility 与 Newtonsoft.Json，使 UnityEvent 可在 Newtonsoft 序列化体系中正常工作
    /// </summary>
    public class UnityEventConverter : JsonConverter<UnityEvent>
    {
        public override void WriteJson(JsonWriter writer, UnityEvent value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // 使用 JsonUtility 将 UnityEvent 转为 JSON 字符串
            string eventJson = JsonUtility.ToJson(value);

            // 兜底：空 UnityEvent 可能返回 "{}"
            if (string.IsNullOrEmpty(eventJson))
                eventJson = "{}";

            // 将此字符串作为 JToken 写入 Newtonsoft writer
            JToken token = JToken.Parse(eventJson);
            token.WriteTo(writer);
        }

        public override UnityEvent ReadJson(JsonReader reader, Type objectType, UnityEvent existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // 处理 null
            if (reader.TokenType == JsonToken.Null)
            {
                reader.Skip();
                return null;
            }

            // 从 Reader 加载 JToken（兼容对象/数组等）
            JToken token = JToken.Load(reader);
            string eventJson = token.ToString();

            // 使用 JsonUtility 反序列化回 UnityEvent
            UnityEvent result = existingValue ?? new UnityEvent();
            JsonUtility.FromJsonOverwrite(eventJson, result);
            return result;
        }
    }
}
