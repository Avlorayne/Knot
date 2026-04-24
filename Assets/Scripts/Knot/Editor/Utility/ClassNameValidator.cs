using System.Collections.Generic;
using System.Text.RegularExpressions;
using Knot.Include.Attributes;

namespace Knot.Editor.Utility
{
    /// <summary>
    /// 验证结果类
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// 错误列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// 建议列表
        /// </summary>
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    /// <summary>
    /// 类名验证器，用于验证类名是否符合C#和Unity规范
    /// </summary>
    [Version("1.0.0")]
    public static class ClassNameValidator
    {
        private static readonly HashSet<string> ReservedKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal",
            "is", "lock", "long", "namespace", "new", "null", "object",
            "operator", "out", "override", "params", "private", "protected",
            "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch",
            "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void",
            "volatile", "while"
        };

        private static readonly HashSet<string> UnitySpecialPrefixes = new HashSet<string>
        {
            "MonoBehaviour", "ScriptableObject", "PlotEditorWindow", "Editor",
            "PropertyDrawer", "Attribute", "NetworkBehaviour"
        };

        /// <summary>
        /// 验证类名是否符合C#和Unity规范
        /// </summary>
        /// <param name="className">要验证的类名</param>
        /// <returns>验证结果</returns>
        public static ValidationResult ValidateClassName(string className)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            if (string.IsNullOrWhiteSpace(className))
            {
                result.IsValid = false;
                result.Errors.Add("类名不能为空");
                return result;
            }

            if (className.Length > 50)
            {
                result.IsValid = false;
                result.Errors.Add($"类名过长 ({className.Length} 字符)，建议不超过 50 字符");
            }

            if (char.IsDigit(className[0]))
            {
                result.IsValid = false;
                result.Errors.Add("类名不能以数字开头");
            }

            if (!Regex.IsMatch(className, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                result.IsValid = false;
                result.Errors.Add("类名只能包含字母、数字和下划线，且不能以数字开头");
            }

            if (ReservedKeywords.Contains(className.ToLower()))
            {
                result.IsValid = false;
                result.Errors.Add($"'{className}' 是 C# 保留关键字");
            }

            CheckUnityConventions(className, result);

            return result;
        }

        private static void CheckUnityConventions(string className, ValidationResult result)
        {
            bool isUnityComponent = className.EndsWith("Behaviour") ||
                                    className.EndsWith("Component") ||
                                    className.EndsWith("Manager") ||
                                    className.EndsWith("System") ||
                                    className.EndsWith("Controller");

            if (isUnityComponent && !className.EndsWith("Behaviour") &&
                !className.EndsWith("Component"))
            {
                result.Suggestions.Add($"Unity 组件建议使用 'Behaviour' 或 'Component' 后缀，例如: {className}Behaviour");
            }

            if (className.Contains("Editor") && !className.EndsWith("Editor"))
            {
                result.Suggestions.Add($"编辑器类建议以 'Editor' 结尾，例如: {className}Editor");
            }

            if (className.Contains("Drawer") && !className.EndsWith("Drawer"))
            {
                result.Suggestions.Add($"属性绘制器建议以 'Drawer' 结尾，例如: {className}Drawer");
            }
        }

        /// <summary>
        /// 生成建议的类名
        /// </summary>
        /// <param name="inputName">输入名称</param>
        /// <returns>建议的类名</returns>
        public static string GenerateSuggestedName(string inputName)
        {
            if (string.IsNullOrWhiteSpace(inputName))
                return "MyClass";

            string cleaned = Regex.Replace(inputName, @"[^a-zA-Z0-9_]", "");

            if (cleaned.Length > 0)
            {
                cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1);
            }
            else
            {
                cleaned = "MyClass";
            }

            if (ReservedKeywords.Contains(cleaned.ToLower()))
            {
                cleaned += "Class";
            }

            return cleaned;
        }
    }
}
