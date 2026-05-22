using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using JetBrains.Annotations;
using Knot.Runtime.Attributes;
using Knot.Runtime.Utility;
using UnityEngine;

namespace Knot.Runtime.Core
{
    /// <summary>
    /// 指令参数基类，所有指令参数都应继承此类
    /// </summary>
    [Version("1.0.0")]
    [Serializable]
    public class InstrParam
    {
        public InstrParam()//空构造函数用于反序列化
        {

        }
        private static ISerializer _serializer = new ReflectionSerializer();
        private static IPrinter _printer = new ReflectionPrinter();

        /// <summary>
        /// 指令命名空间前缀
        /// </summary>
        public static string Namespace = "Knot.Instruction.";

        /// <summary>
        /// 指令名称
        /// </summary>
        [JsonProperty] public virtual string Name { get; protected set; } = nameof(InstrParam);

        /// <summary>
        /// 执行器类型名称
        /// </summary>
        [JsonProperty] protected virtual string _ExecutorType { get; set; } = nameof(InstrExecute);

        /// <summary>
        /// 获取执行器类型
        /// </summary>
        [JsonIgnore]
        public Type ExecutorType
        {
            get
            {
                string typeName = _ExecutorType;
                if (!typeName.Contains(Namespace))
                {
                    typeName = Namespace + typeName;
                }

                Type type = Type.GetType(typeName) ?? Assembly.GetExecutingAssembly().GetType(typeName);

                if (type == null)
                {
                    Debug.Log($"[InstrParam.ExecutorType]Can't find executor type {_ExecutorType}");
                }
                return type;
            }
        }

        /// <summary>
        /// 指令描述
        /// </summary>
        [JsonProperty] public virtual string Description { get; protected set; } = "Basic Description";

        /// <summary>
        /// 是否允许共存（同一帧中是否允许存在多个相同类型的指令）
        /// </summary>
        [JsonProperty] public virtual bool IsCanCoexist { get; set; } = false;

        /// <summary>
        /// 是否执行后释放
        /// </summary>
        [JsonProperty] public virtual bool IsRelease { get; set; } = false;

        /// <summary>
        /// 是否可以跳过
        /// </summary>
        [JsonProperty] public virtual bool IsCanBeSkipped { get; set; } = false;

        /// <summary>
        /// 是否需要等待执行完成
        /// </summary>
        [JsonProperty] public virtual bool IsBeWaited { get; set; } = false;

        /// <summary>
        /// 扩展数据
        /// </summary>
        [JsonProperty]
        [JsonExtensionData]
        [CanBeNull]
        protected Dictionary<string, object> ExtensionData { get; set; } = new();

        /// <summary>
        /// 打印指令参数信息
        /// </summary>
        /// <param name="instrParam">要打印的指令参数</param>
        /// <returns>字符串表示</returns>
        public static string PrintString(InstrParam instrParam)
        {
            return _printer.PrintString(instrParam);
        }

        /// <summary>
        /// 序列化指令参数
        /// </summary>
        /// <param name="instrParam">要序列化的指令参数</param>
        /// <returns>JSON字符串</returns>
        public static string Serialize(InstrParam instrParam)
        {
            return _serializer.Serialize(instrParam);
        }

        /// <summary>
        /// 反序列化指令参数
        /// </summary>
        /// <typeparam name="T">指令参数类型</typeparam>
        /// <param name="jsonString">JSON字符串</param>
        /// <returns>指令参数对象</returns>
        public static T Deserialize<T>(string jsonString) where T : InstrParam
        {
            T instrParam = JsonConvert.DeserializeObject<T>(jsonString);
            return instrParam;
        }

        /// <summary>
        /// 转换指令参数类型（多态反序列化）
        /// </summary>
        /// <param name="instrParam">基类指令参数</param>
        /// <returns>具体类型的指令参数</returns>
        public static InstrParam Convert(InstrParam instrParam)
        {
            Debug.Log($"[InstrParam.Convert]Json String\n{_printer.PrintString(instrParam)}");

            string jsonString = JsonConvert.SerializeObject(instrParam);
            string typeName = instrParam.Name;

            if (!typeName.Contains(Namespace))
            {
                typeName = Namespace + typeName;
            }

            Type type = Type.GetType(typeName) ?? Assembly.GetExecutingAssembly().GetType(typeName);

            if (type == null)
            {
                throw new InvalidOperationException($"Type '{typeName}' not found.");
            }

            Debug.Log($"[InstrParam.Convert]Find Type :{type.Name}");

            return JsonConvert.DeserializeObject(jsonString, type) as InstrParam;
        }
    }
}
