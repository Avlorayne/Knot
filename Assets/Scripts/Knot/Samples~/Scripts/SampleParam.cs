using Knot.Runtime.Core;
using Knot.Runtime.Attributes;

namespace Knot.Samples
{
    /// <summary>
    /// 示例指令参数
    /// </summary>
    [Version("1.0.0")]
    public class SampleParam : InstrParam
    {
        /// <summary>
        /// 指令名称
        /// </summary>
        public override string Name { get; protected set; } = nameof(SampleParam);
        
        /// <summary>
        /// 执行器类型名称
        /// </summary>
        protected override string _ExecutorType { get; set; } = nameof(SampleExecute);

        /// <summary>
        /// 指令描述
        /// </summary>
        public override string Description { get; protected set; } = "这是一个示例指令参数";
        
        /// <summary>
        /// 是否允许共存
        /// </summary>
        public override bool IsCanCoexist { get; set; } = true;

        /// <summary>
        /// 是否执行后释放
        /// </summary>
        public override bool IsRelease { get; set; } = true;
        
        /// <summary>
        /// 是否可以跳过
        /// </summary>
        public override bool IsCanBeSkipped { get; set; } = true;
        
        /// <summary>
        /// 是否需要等待执行完成
        /// </summary>
        public override bool IsBeWaited { get; set; } = false;

        /// <summary>
        /// 示例消息
        /// </summary>
        public string Message { get; set; } = "Hello, Knot!";
    }
}
