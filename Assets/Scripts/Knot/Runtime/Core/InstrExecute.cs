using System.Collections;
using Knot.Runtime.Attributes;
using Knot.Runtime.Execution;
using UnityEngine;

namespace Knot.Runtime.Core
{
    /// <summary>
    /// 执行状态枚举
    /// </summary>
    /// <version>1.0.0</version>
    public enum ExState
    {
        Null,
        Ready,
        Executing,
        Completed,
        End
    }

    /// <summary>
    /// 指令执行器基类
    /// </summary>
    /// <version>1.0.0</version>
    [Version("1.0.0")]
    public abstract class InstrExecute : MonoBehaviour
    {
        protected InstrParam _param = null;

        /// <summary>
        /// 指令参数
        /// </summary>
        /// <version>1.0.0</version>
        public InstrParam Param
        {
            get => _param;
            set
            {
                if (_param != null && _param != value && !_param.IsRelease)
                {
                    Debug.LogError($"InstrParam {_param.Name} shouldn't be Loaded Here!");
                }
                else
                {
                    _param = value;
                }
            }
        }

        /// <summary>
        /// 父组件
        /// </summary>
        /// <version>1.0.0</version>
        [Header("父组件")] public GameObject ParentGameObject;

        /// <summary>
        /// 当前执行状态
        /// </summary>
        /// <version>1.0.0</version>
        public ExState ExState = ExState.Null;

        /// <summary>
        /// 释放执行器
        /// </summary>
        /// <version>1.0.0</version>
        protected void ReleaseExecutor()
        {
            Param = null;
            ExState = ExState.Null;
        }

        /// <summary>
        /// 内部初始化封装
        /// </summary>
        /// <version>1.0.0</version>
        public void Init_Pack(InstrParam param)
        {
            Param ??= param;
            if (Param != null)
                ExState = ExState.Ready;

            // 指令对象属于 UI，将其层级设为 Canvas 子物体
            if (gameObject.GetComponent<RectTransform>() != null && ParentGameObject == null)
            {
                GameObject canvas = PlotPerformSys.Instance.gameObject;
                ParentGameObject = canvas;
                RectTransform parentRectTransform = canvas.GetComponent<RectTransform>();
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.SetParent(parentRectTransform, false);
            }
            // 指令对象属于一般 GameObject，则自定义父物体
            else if (ParentGameObject != null)
            {
                transform.SetParent(ParentGameObject.transform);
            }

            Init();
        }

        /// <summary>
        /// 内部执行封装
        /// </summary>
        /// <version>1.0.0</version>
        public void Execute_Pack()
        {
            MarkExecuting();
            Execute();
            StartCoroutine(CoExecute_Pack());
        }

        private IEnumerator CoExecute_Pack()
        {
            yield return StartCoroutine(CoExecute());
            MarkCompleted();
        }

        /// <summary>
        /// 内部中断封装
        /// </summary>
        /// <version>1.0.0</version>
        public void Interrupt_Pcak()
        {
            if (!Param.IsCanBeSkipped)
            {
                Debug.Log($"[TypeDialogue.Interrupt] Cannot skip this dialogue");
                return;
            }
            // 停止协程
            StopAllCoroutines();
            Interrupt();
            MarkCompleted();
        }

        /// <summary>
        /// 内部结束封装
        /// </summary>
        /// <version>1.0.0</version>
        public void End_Pack()
        {
            End();
            ExState = ExState.End;
            Debug.Log($"[TypeDialogue.End]");
            if (Param.IsRelease)  ReleaseExecutor();
        }

        #region Executor
        /// <summary>
        /// 初始化逻辑
        /// </summary>
        /// <version>1.0.0</version>
        protected abstract void Init();

        /// <summary>
        /// 执行逻辑
        /// </summary>
        /// <version>1.0.0</version>
        protected abstract void Execute();

        /// <summary>
        /// 协程执行逻辑
        /// </summary>
        /// <version>1.0.0</version>
        protected virtual IEnumerator CoExecute()
        {
            yield return null;
        }

        /// <summary>
        /// 中断逻辑
        /// </summary>
        /// <version>1.0.0</version>
        protected abstract void Interrupt();

        /// <summary>
        /// 结束逻辑
        /// </summary>
        /// <version>1.0.0</version>
        protected abstract void End();
        #endregion


        #region Delegate And Event
        /// <summary>
        /// 执行委托
        /// </summary>
        /// <version>1.0.0</version>
        public delegate void ExecuteHandler();

        /// <summary>
        /// 完成事件
        /// </summary>
        /// <version>1.0.0</version>
        public event ExecuteHandler OnCompleted;

        /// <summary>
        /// 执行中事件
        /// </summary>
        /// <version>1.0.0</version>
        public event ExecuteHandler OnExecuting;

        /// <summary>
        /// 标记为已完成
        /// </summary>
        /// <version>1.0.0</version>
        protected void MarkCompleted()
        {
            if (ExState == ExState.Executing)
            {
                ExState = ExState.Completed;
                Debug.Log($"[InstrExecute.OnCompleted] {Param.Name} {ExState}]");
                OnCompleted?.Invoke();
            }
        }

        /// <summary>
        /// 标记为执行中
        /// </summary>
        /// <version>1.0.0</version>
        protected void MarkExecuting()
        {
            if (ExState == ExState.Ready)
            {
                ExState = ExState.Executing;
                Debug.Log($"[InstrExecute.OnExecuting] {Param.Name} {ExState}");
                OnExecuting?.Invoke();
            }
        }
        #endregion
    }
}
