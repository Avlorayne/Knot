using System.Collections;
using Knot.Runtime.Core;
using UnityEngine;

namespace Knot.Samples
{
    /// <summary>
    /// 示例指令执行器
    /// </summary>
    [Version("1.0.0")]
    public class SampleExecute : InstrExecute
    {
        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void Init()
        {
            ExState = ExState.Ready;
        }

        /// <summary>
        /// 执行逻辑
        /// </summary>
        protected override void Execute()
        {
            var param = Param as SampleParam;
            if (param != null)
            {
                Debug.Log($"[SampleExecute] {param.Message}");
            }
            ExState = ExState.Finished;
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
