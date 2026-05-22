using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Knot.Runtime.Core;
using Knot.Runtime.Data;
using Knot.Runtime.Utility.JsonConverter;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace Knot.Runtime.Utility
{
    class JsonSerilizeUtility : IDataSerializer
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            Converters = new List<Newtonsoft.Json.JsonConverter>
            {
                new UnityEventConverter()
            }
        };
        private JsonSerializer _serializer;
        /// <summary>
        /// 构造函数，允许传入自定义的 JsonSerializerSettings 来配置序列化行为，需要传入自定义转换器实现Unity类型转换
        /// </summary>
        /// <param name="settings"></param>
        public JsonSerilizeUtility(JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = Settings;
            }
            _serializer = JsonSerializer.Create(settings ?? Settings);
        }

        /// <summary>
        /// 序列化对象为 JSON 字符串。
        /// </summary>
        public string Serialize(object _obj)
        {
            if (_obj == null) return null;
            try
            {
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    _serializer.Serialize(jsonWriter, _obj);
                    return stringWriter.ToString();
                }

            }
            catch (JsonException ex)
            {
                Debug.LogError($"[JsonSerilizeUtility] Serialize error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 反序列化 JSON 字符串为指定类型。
        /// </summary>
        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);
            try
            {
                using (var stringReader = new StringReader(json))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonReader);
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError($"[JsonSerilizeUtility] Deserialize error: {ex.Message}");
                return default(T);
            }
        }
    };
}