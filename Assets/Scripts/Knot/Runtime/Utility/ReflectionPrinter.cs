using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Knot.Runtime.Attributes;

namespace Knot.Runtime.Utility
{
    /// <summary>
    /// 对象打印接口
    /// </summary>
    public interface IPrinter
    {
        /// <summary>
        /// 打印对象为字符串
        /// </summary>
        string PrintString(object obj);
        
        /// <summary>
        /// 获取字段的访问修饰符
        /// </summary>
        string GetAccessModifier(FieldInfo field);

        /// <summary>
        /// 获取对象的所有公共可读属性及其值和类型信息
        /// </summary>
        Dictionary<(string, string), Type> GetPropers(object obj, int maxDepth = 3, int currentDepth = 0);

        /// <summary>
        /// 获取对象的所有公共字段及其值和类型信息
        /// </summary>
        Dictionary<(string, string), Type> GetFields(object obj, int maxDepth = 3, int currentDepth = 0);

        /// <summary>
        /// 获取对象的方法信息
        /// </summary>
        string GetMethods(object obj);
        
        /// <summary>
        /// 获取对象的事件信息
        /// </summary>
        string GetEvents(object obj);

        /// <summary>
        /// 将对象转换为字符串表示
        /// </summary>
        string ObjectToString(object obj, int maxDepth = 3, int currentDepth = 0);
    }

    /// <summary>
    /// 反射打印器，用于将对象信息打印为字符串
    /// </summary>
    [Version("1.0.0")]
    public class ReflectionPrinter : IPrinter
    {
        private readonly HashSet<object> _visitedObjects = new HashSet<object>();

        /// <summary>
        /// 打印对象为字符串
        /// </summary>
        /// <param name="obj">要打印的对象</param>
        /// <returns>对象的字符串表示</returns>
        public string PrintString(object obj)
        {
            _visitedObjects.Clear();
            return PrintString(obj, 3, 0);
        }

        private string PrintString(object obj, int maxDepth, int currentDepth)
        {
            if (obj == null) return "[null]";

            Type type = obj.GetType();

            StringBuilder Propers = new StringBuilder();
            foreach (var proper in GetPropers(obj, maxDepth, currentDepth))
            {
                Propers.Append($"\t{proper.Value?.Name ?? "unknown"} {proper.Key.Item1}: {proper.Key.Item2} \n");
            }

            StringBuilder Fields = new StringBuilder();
            foreach (var field in GetFields(obj, maxDepth, currentDepth))
            {
                Fields.Append($"\t{field.Value?.Name ?? "unknown"} {field.Key.Item1}: {field.Key.Item2}\n");
            }

            return $@"[{type.Name}]
[Property]: 
{Propers}
[Field]:
{Fields}
[Method]:
{GetMethods(obj)}
[Event]:
{GetEvents(obj)}";
        }

        /// <summary>
        /// 获取字段的访问修饰符
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>访问修饰符字符串</returns>
        public string GetAccessModifier(FieldInfo field)
        {
            if (field.IsPublic)
                return "public";
            else if (field.IsPrivate)
                return "private";
            else if (field.IsFamily)
                return "protected";
            else if (field.IsAssembly)
                return "internal";
            else if (field.IsFamilyOrAssembly)
                return "protected internal";
            else
                return "private";
        }

        /// <summary>
        /// 获取对象的所有公共可读属性及其值和类型信息
        /// </summary>
        public Dictionary<(string, string), Type> GetPropers(object obj, int maxDepth = 3, int currentDepth = 0)
        {
            return GetPropersInternal(obj, maxDepth, currentDepth);
        }

        /// <summary>
        /// 获取对象的所有公共字段及其值和类型信息
        /// </summary>
        public Dictionary<(string, string), Type> GetFields(object obj, int maxDepth = 3, int currentDepth = 0)
        {
            return GetFieldsInternal(obj, maxDepth, currentDepth);
        }

        private Dictionary<(string, string), Type> GetPropersInternal(object obj, int maxDepth, int currentDepth)
        {
            if (obj == null) return new Dictionary<(string, string), Type>();

            Type type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            Dictionary<(string, string), Type> propersDict = new Dictionary<(string, string), Type>();

            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj);
                    string valueString = ObjectToString(value, maxDepth, currentDepth + 1);
                    propersDict.Add((prop.Name, valueString), prop.PropertyType);
                }
                catch (Exception ex)
                {
                    propersDict.Add((prop.Name, $"<Error: {ex.Message}>"), null);
                }
            }

            return propersDict;
        }

        private Dictionary<(string, string), Type> GetFieldsInternal(object obj, int maxDepth, int currentDepth)
        {
            if (obj == null) return new Dictionary<(string, string), Type>();

            Type type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute), false));

            Dictionary<(string, string), Type> fieldsDict = new Dictionary<(string, string), Type>();

            foreach (var field in fields)
            {
                var accessModifier = GetAccessModifier(field);
                if (accessModifier != "public")
                    continue;

                try
                {
                    var value = field.GetValue(obj);
                    string valueString = ObjectToString(value, maxDepth, currentDepth + 1);
                    fieldsDict.Add((field.Name, valueString), field.FieldType);
                }
                catch (Exception ex)
                {
                    fieldsDict.Add((field.Name, $"<Error: {ex.Message}>"), null);
                }
            }

            return fieldsDict;
        }

        /// <summary>
        /// 将对象转换为字符串表示
        /// </summary>
        public string ObjectToString(object obj, int maxDepth = 3, int currentDepth = 0)
        {
            if (obj == null)
                return "null";

            if (_visitedObjects.Contains(obj))
                return "<circular reference>";

            if (currentDepth >= maxDepth)
                return $"<max depth {maxDepth} reached>";

            Type type = obj.GetType();

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return obj.ToString();

            if (obj is UnityEngine.Object unityObj)
            {
                if (unityObj == null)
                    return "null";
                return $"{unityObj.GetType().Name}({unityObj.name})";
            }

            if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
            {
                return CollectionToString(enumerable, maxDepth, currentDepth);
            }

            try
            {
                _visitedObjects.Add(obj);
                return ComplexObjectToString(obj, maxDepth, currentDepth);
            }
            finally
            {
                _visitedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// 获取对象的方法信息
        /// </summary>
        public string GetMethods(object obj)
        {
            if (obj == null) return "[null]";

            Type type = obj.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

            StringBuilder sb = new StringBuilder();
            foreach (var method in methods)
            {
                sb.AppendLine($"\t{method.ReturnType.Name} {method.Name}()");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取对象的事件信息
        /// </summary>
        public string GetEvents(object obj)
        {
            if (obj == null) return "[null]";

            Type type = obj.GetType();
            var events = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            StringBuilder sb = new StringBuilder();
            foreach (var evt in events)
            {
                sb.AppendLine($"\tevent {evt.EventHandlerType.Name} {evt.Name}");
            }

            return sb.ToString();
        }

        private string CollectionToString(System.Collections.IEnumerable enumerable, int maxDepth, int currentDepth)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            bool first = true;
            foreach (var item in enumerable)
            {
                if (!first) sb.Append(", ");
                first = false;

                sb.Append(ObjectToString(item, maxDepth, currentDepth + 1));
            }

            sb.Append("]");
            return sb.ToString();
        }

        private string ComplexObjectToString(object obj, int maxDepth, int currentDepth)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            Type type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            bool first = true;
            foreach (var prop in properties)
            {
                if (!first) sb.Append(", ");
                first = false;

                try
                {
                    var value = prop.GetValue(obj);
                    sb.Append($"{prop.Name}={ObjectToString(value, maxDepth, currentDepth + 1)}");
                }
                catch
                {
                    sb.Append($"{prop.Name}=<error>");
                }
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
}
