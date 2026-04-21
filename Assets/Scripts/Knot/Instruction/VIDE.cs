using System;
using System.Collections;
using Knot.Runtime.Attributes;
using Knot.Runtime.Core;

namespace Knot.Instruction
{
    /// <summary>
    /// 视频指令参数
    /// </summary>
    [Version("1.0.0")]
    [Serializable]
    public class VIDEParam: InstrParam
    {
        /// <summary>
        /// 指令名称
        /// </summary>
        public override string Name { get; protected set; } = nameof(VIDEParam);
        
        /// <summary>
        /// 执行器类型名称
        /// </summary>
        protected override string _ExecutorType { get; set; } = nameof(VIDE);

        /// <summary>
        /// 指令描述
        /// </summary>
        public override string Description { get; protected set; } = "VIDEParam Description";
        
        /// <summary>
        /// 是否允许共存
        /// </summary>
        public override bool IsCanCoexist { get;  set; } = false;

        /// <summary>
        /// 是否执行后释放
        /// </summary>
        public override bool IsRelease { get; set; } = false;
        
        /// <summary>
        /// 是否可以跳过
        /// </summary>
        public override bool IsCanBeSkipped { get; set; } = true;
        
        /// <summary>
        /// 是否需要等待执行完成
        /// </summary>
        public override bool IsBeWaited { get; set; } = false;
    }

    /// <summary>
    /// 视频指令执行器
    /// </summary>
    [Version("1.0.0")]
    public class VIDE: InstrExecute
    {
        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void Init()
        {
        }

        /// <summary>
        /// 执行逻辑
        /// </summary>
        protected override void Execute()
        {
        }

        /// <summary>
        /// 协程执行逻辑
        /// </summary>
        protected override IEnumerator CoExecute()
        {
            yield return null;
        }

        /// <summary>
        /// 中断逻辑
        /// </summary>
        protected override void Interrupt()
        {
        }

        /// <summary>
        /// 结束逻辑
        /// </summary>
        protected override void End()
        {
        }
    }
}
