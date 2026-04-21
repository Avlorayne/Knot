using System;

namespace Knot.Runtime.Attributes
{
    /// <summary>
    /// 用于标记代码版本的特性
    /// </summary>
    [Version("1.0.0")]
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class VersionAttribute : Attribute
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Note { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="version">版本号</param>
        public VersionAttribute(string version)
        {
            Version = version;
            Note = string.Empty;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="note">备注信息</param>
        public VersionAttribute(string version, string note)
        {
            Version = version;
            Note = note;
        }
    }
}
