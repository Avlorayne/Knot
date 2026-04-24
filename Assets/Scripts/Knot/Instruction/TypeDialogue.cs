using System;
using System.Collections;
using Knot.Include.Attributes;
using Knot.Runtime.Core;
using TMPro;
using UnityEngine;

namespace Knot.Instruction
{
    /// <summary>
    /// 对话数据类
    /// </summary>
    [Serializable]
    public class Dialogue
    {
        /// <summary>
        /// 说话者名称
        /// </summary>
        public string Name;
        
        /// <summary>
        /// 对话内容
        /// </summary>
        public string Sentence;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">说话者名称</param>
        /// <param name="sentence">对话内容</param>
        public Dialogue(string name, string sentence)
        {
            Name = name;
            Sentence = sentence;
        }
    }

    /// <summary>
    /// 对话指令参数
    /// </summary>
    [Version("1.0.0")]
    [Serializable]
    public class TypeDialogueParam: InstrParam
    {
        /// <summary>
        /// 指令名称
        /// </summary>
        public override string Name { get; protected set; } = nameof(TypeDialogueParam);
        
        /// <summary>
        /// 执行器类型名称
        /// </summary>
        protected override string _ExecutorType { get; set; } = nameof(TypeDialogue);

        /// <summary>
        /// 指令描述
        /// </summary>
        public override string Description { get; protected set; } = "TypeDialogueParam Description";
        
        /// <summary>
        /// 是否允许共存
        /// </summary>
        public override bool IsCanCoexist { get; set; } = false;

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
        public override bool IsBeWaited { get; set; }

        /// <summary>
        /// 对话数据
        /// </summary>
        public Dialogue dialogue;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dialogue">对话数据</param>
        public TypeDialogueParam(Dialogue dialogue = null)
        {
            this.dialogue = dialogue;
        }
    }

    /// <summary>
    /// 对话指令执行器，实现逐字显示对话效果
    /// </summary>
    [Version("1.0.0")]
    public class TypeDialogue: InstrExecute
    {
        /// <summary>
        /// 说话者名称文本组件
        /// </summary>
        [Header("文本组件")]
        public TextMeshProUGUI speakerNameText;
        
        /// <summary>
        /// 对话内容文本组件
        /// </summary>
        public TextMeshProUGUI dialogueText;

        /// <summary>
        /// 文字显示速度（秒/字）
        /// </summary>
        [Header("文本速度")] public float textSpeed = 0.05f;

        private Dialogue currentDialogue;

        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void Init()
        {
            speakerNameText.text = "";
            dialogueText.text = "";
            ExState = ExState.Ready;
            currentDialogue = (Param as TypeDialogueParam)?.dialogue;
        }

        /// <summary>
        /// 执行逻辑
        /// </summary>
        protected override void Execute()
        {
        }

        /// <summary>
        /// 协程执行逻辑，实现逐字显示效果
        /// </summary>
        protected override IEnumerator CoExecute()
        {
            speakerNameText.text = currentDialogue.Name;
            foreach (char c in currentDialogue.Sentence)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        /// <summary>
        /// 中断逻辑，立即显示完整对话
        /// </summary>
        protected override void Interrupt()
        {
            dialogueText.text = currentDialogue.Sentence;
        }

        /// <summary>
        /// 结束逻辑
        /// </summary>
        protected override void End()
        {
        }
    }
}
