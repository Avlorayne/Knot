using System;
using Knot.Runtime.Attributes;
using Knot.Runtime.Core;
using Knot.Runtime.Utility.JsonConverter;
using Newtonsoft.Json;
using UnityEngine.Events;
using UnityEngine;

namespace Knot.Test.UnityEventTest
{
    /// <summary>
    /// UnityEvent 序列化测试指令参数
    /// 验证 UnityEventConverter 在 Newtonsoft.Json 序列化体系中的往返正确性
    /// </summary>
    [Version("1.0.0")]
    [Serializable]
    public class UnityEventTestParam : InstrParam
    {
        #region InstrParam Overrides

        public override string Name { get; protected set; } = nameof(UnityEventTestParam);

        protected override string _ExecutorType { get; set; } = nameof(UnityEventTestExecutor);

        public override string Description { get; protected set; } = "UnityEvent 序列化测试指令";

        public override bool IsCanCoexist { get; set; } = false;

        public override bool IsRelease { get; set; } = true;

        public override bool IsCanBeSkipped { get; set; } = true;

        public override bool IsBeWaited { get; set; } = false;

        #endregion

        #region UnityEvent Test Fields

        /// <summary>
        /// 测试用 UnityEvent —— 由 UnityEventConverter 处理序列化
        /// </summary>
        [JsonProperty]
        [JsonConverter(typeof(UnityEventConverter))]
        public UnityEvent OnTestComplete;

        /// <summary>
        /// 附带一个普通字段，验证混合序列化时互不干扰
        /// </summary>
        [JsonProperty]
        public string TestMessage;

        #endregion

        public UnityEventTestParam()
        {
            OnTestComplete = new UnityEvent();
        }

        public UnityEventTestParam(string message) : this()
        {
            TestMessage = message;
        }
    }
}
