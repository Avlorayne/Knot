using System.Text;
using System.Text.Json;
using Knot.Runtime.Attributes;
using UnityEngine;

namespace Knot.Runtime.Utility
{
    /// <summary>
    /// JSON字符串优质打印工具类
    /// 提供格式化和美化JSON字符串的功能
    /// </summary>
    [Version("1.0.0")]
    public static class JsonPrettyPrinter
    {
        private const string INDENT_STRING = "    ";
        private const string COLOR_OBJECT = "#FF6B6B";
        private const string COLOR_ARRAY = "#4ECDC4";
        private const string COLOR_STRING = "#45B7D1";
        private const string COLOR_NUMBER = "#96CEB4";
        private const string COLOR_BOOLEAN = "#FFEAA7";
        private const string COLOR_NULL = "#DDA0DD";
        private const string COLOR_KEY = "#FDCB6E";

        /// <summary>
        /// 格式化JSON字符串（无颜色）
        /// </summary>
        /// <param name="json">原始JSON字符串</param>
        /// <returns>格式化后的JSON字符串</returns>
        public static string Format(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            try
            {
                using var document = JsonDocument.Parse(json);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                return JsonSerializer.Serialize(document.RootElement, options);
            }
            catch (JsonException)
            {
                return FormatManually(json);
            }
        }

        /// <summary>
        /// 格式化并添加颜色高亮（适用于Unity控制台）
        /// </summary>
        /// <param name="json">原始JSON字符串</param>
        /// <returns>带颜色标签的格式化JSON字符串</returns>
        public static string FormatWithColor(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            var formatted = Format(json);
            return AddColorHighlight(formatted);
        }

        /// <summary>
        /// 打印到Unity控制台（带颜色）
        /// </summary>
        /// <param name="json">原始JSON字符串</param>
        /// <param name="title">可选的标题</param>
        public static void PrintToConsole(string json, string title = null)
        {
            if (!string.IsNullOrEmpty(title))
            {
                Debug.Log($"<b><color=white>=== {title} ===</color></b>");
            }

            var coloredJson = FormatWithColor(json);
            Debug.Log(coloredJson);
        }

        /// <summary>
        /// 验证并格式化JSON
        /// </summary>
        /// <param name="json">原始JSON字符串</param>
        /// <returns>格式化结果</returns>
        public static (bool isValid, string formatted, string error) ValidateAndFormat(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return (false, json, "JSON string is null or empty");

            try
            {
                using var document = JsonDocument.Parse(json);
                var formatted = Format(json);
                return (true, formatted, null);
            }
            catch (JsonException ex)
            {
                return (false, json, $"Invalid JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// 手动格式化JSON（备用方法）
        /// </summary>
        private static string FormatManually(string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();

            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            indent++;
                            sb.Append(INDENT_STRING.Repeat(indent));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            indent--;
                            sb.Append(INDENT_STRING.Repeat(indent));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        var escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            sb.Append(INDENT_STRING.Repeat(indent));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(' ');
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 添加颜色高亮
        /// </summary>
        private static string AddColorHighlight(string json)
        {
            var result = new StringBuilder();
            var inString = false;
            var escaped = false;
            var inKey = false;

            for (int i = 0; i < json.Length; i++)
            {
                var ch = json[i];

                if (ch == '\\' && inString)
                {
                    escaped = !escaped;
                    result.Append(ch);
                    continue;
                }

                if (ch == '"' && !escaped)
                {
                    if (inString)
                    {
                        if (inKey)
                        {
                            result.Append($"</color>");
                            inKey = false;
                        }
                        else
                        {
                            result.Append($"</color>");
                        }
                    }
                    else
                    {
                        if (i + 1 < json.Length)
                        {
                            int j = i + 1;
                            while (j < json.Length && char.IsWhiteSpace(json[j])) j++;
                            if (j < json.Length && json[j] == ':')
                            {
                                result.Append($"<color={COLOR_KEY}>");
                                inKey = true;
                            }
                            else
                            {
                                result.Append($"<color={COLOR_STRING}>");
                            }
                        }
                    }
                    inString = !inString;
                }

                result.Append(ch);
                escaped = false;
            }

            return result.ToString();
        }
    }
}
