using System;
using System.Text.Json;
using Knot.Include.Attributes;

namespace Knot.Include.Utility
{
    /// <summary>
    /// 序列化接口
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// 序列化对象为字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>序列化后的字符串</returns>
        string Serialize(object obj);
    }

    /// <summary>
    /// 反射序列化器，使用JSON进行序列化
    /// </summary>
    [Version("1.0.0")]
    public class ReflectionSerializer : ISerializer
    {
        /// <summary>
        /// 序列化对象为JSON字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>JSON字符串</returns>
        public string Serialize(object obj)
        {
            if (obj == null) return null;

            Type type = obj.GetType();

            return JsonSerializer.Serialize(obj, type, new JsonSerializerOptions{IncludeFields = true, WriteIndented = true});
        }
    }
}
