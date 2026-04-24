using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Knot.Include.Attributes;

namespace Knot.Include.Utility
{
    /// <summary>
    /// 反射辅助工具类，用于获取类型成员信息
    /// </summary>
    [Version("1.0.0")]
    public static class ReflectionHelper
    {
        /// <summary>
        /// 获取需要显示在编辑器中的字段（包含DisplayInEditor特性且不隐藏的）
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <param name="includeInherited">是否包含继承的字段</param>
        /// <returns>可显示的字段列表</returns>
        public static List<FieldInfo> GetDisplayableFields(Type type, bool includeInherited = true)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            if (!includeInherited)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            var allFields = type.GetFields(flags);
            var displayableFields = new List<FieldInfo>();

            foreach (var field in allFields)
            {
                var displayAttr = field.GetCustomAttribute<DisplayInEditorAttribute>();
                if (displayAttr != null && displayAttr.Hidden)
                    continue;

                if (displayAttr != null ||
                    (field.GetCustomAttribute<JsonIncludeAttribute>() != null &&
                     field.GetCustomAttribute<JsonIgnoreAttribute>() == null))
                {
                    displayableFields.Add(field);
                }
            }

            return displayableFields.OrderBy(f =>
            {
                var attr = f.GetCustomAttribute<DisplayInEditorAttribute>();
                return attr?.Order ?? 0;
            }).ToList();
        }

        /// <summary>
        /// 获取需要显示在编辑器中的属性（包含DisplayInEditor特性且不隐藏的）
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <param name="includeInherited">是否包含继承的属性</param>
        /// <returns>可显示的属性列表</returns>
        public static List<PropertyInfo> GetDisplayableProperties(Type type, bool includeInherited = true)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            if (!includeInherited)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            var allProperties = type.GetProperties(flags);
            var displayableProperties = new List<PropertyInfo>();

            foreach (var property in allProperties)
            {
                var displayAttr = property.GetCustomAttribute<DisplayInEditorAttribute>();
                if (displayAttr != null && displayAttr.Hidden)
                    continue;

                if (property.CanWrite &&
                    property.GetIndexParameters().Length == 0 &&
                    property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                {
                    if (displayAttr != null ||
                        property.GetCustomAttribute<JsonIncludeAttribute>() != null)
                    {
                        displayableProperties.Add(property);
                    }
                }
            }

            return displayableProperties.OrderBy(p =>
            {
                var attr = p.GetCustomAttribute<DisplayInEditorAttribute>();
                return attr?.Order ?? 0;
            }).ToList();
        }

        /// <summary>
        /// 获取成员的显示名称
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>显示名称</returns>
        public static string GetMemberDisplayName(MemberInfo member)
        {
            var displayAttr = member.GetCustomAttribute<DisplayInEditorAttribute>();
            if (!string.IsNullOrEmpty(displayAttr?.DisplayName))
            {
                return displayAttr.DisplayName;
            }

            string name = member.Name;
            if (name.StartsWith("_") && name.Length > 1)
            {
                name = name.Substring(1);
            }

            return System.Text.RegularExpressions.Regex.Replace(
                name,
                "([a-z])([A-Z])",
                "$1 $2"
            );
        }

        /// <summary>
        /// 获取成员的提示文本
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>提示文本</returns>
        public static string GetMemberTooltip(MemberInfo member)
        {
            var displayAttr = member.GetCustomAttribute<DisplayInEditorAttribute>();
            return displayAttr?.Tooltip ?? string.Empty;
        }

        /// <summary>
        /// 获取成员的数值范围限制
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>最小值和最大值</returns>
        public static (float min, float max) GetMemberValueRange(MemberInfo member)
        {
            var displayAttr = member.GetCustomAttribute<DisplayInEditorAttribute>();
            if (displayAttr != null)
            {
                return (displayAttr.MinValue, displayAttr.MaxValue);
            }

            return (float.MinValue, float.MaxValue);
        }

        /// <summary>
        /// 检查成员是否应该显示为折叠面板
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>是否显示为折叠面板</returns>
        public static bool ShouldShowAsFoldout(MemberInfo member)
        {
            var displayAttr = member.GetCustomAttribute<DisplayInEditorAttribute>();
            return displayAttr?.ShowAsFoldout ?? true;
        }
    }
}
