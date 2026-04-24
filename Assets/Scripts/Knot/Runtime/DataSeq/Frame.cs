using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Knot.Include.Attributes;
using Knot.Include.Utility;
using Knot.Runtime.Core;
using UnityEngine;

namespace Knot.Runtime.DataSeq
{
    /// <summary>
    /// 演出帧数据
    /// </summary>
    /// <version>1.0.0</version>
    [Version("1.0.0")]
    [Serializable]
    public class Frame
    {
        [JsonInclude] private List<InstrParam> _instructions = new List<InstrParam>();

        #region Properties and Methods
        /// <summary>
        /// 获取指令列表
        /// </summary>
        /// <version>1.0.0</version>
        [JsonIgnore] public List<InstrParam> Content => _instructions;

        /// <summary>
        /// 获取指令数量
        /// </summary>
        /// <version>1.0.0</version>
        [JsonIgnore] public int Count => _instructions.Count;

        /// <summary>
        /// 索引器获取或设置指令
        /// </summary>
        /// <version>1.0.0</version>
        public InstrParam this[int index]
        {
            get => _instructions[index];
            set => _instructions[index] = value;
        }

        /// <summary>
        /// 添加指令
        /// </summary>
        /// <version>1.0.0</version>
        public void Add(InstrParam instruction) => _instructions.Add(instruction);

        /// <summary>
        /// 批量添加指令
        /// </summary>
        /// <version>1.0.0</version>
        public void AddRange(IEnumerable<InstrParam> instructions) => _instructions.AddRange(instructions);

        /// <summary>
        /// 移除指令
        /// </summary>
        /// <version>1.0.0</version>
        public void Remove(InstrParam instruction) => _instructions.Remove(instruction);

        /// <summary>
        /// 移除指定索引处的指令
        /// </summary>
        /// <version>1.0.0</version>
        public void RemoveAt(int index) => _instructions.RemoveAt(index);

        /// <summary>
        /// 清空所有指令
        /// </summary>
        /// <version>1.0.0</version>
        public void Clear() => _instructions.Clear();

        /// <summary>
        /// 是否包含指定指令
        /// </summary>
        /// <version>1.0.0</version>
        public bool Contains(InstrParam instruction) => _instructions.Contains(instruction);
        #endregion

        #region Serialization
        /// <summary>
        /// 序列化帧数据
        /// </summary>
        /// <version>1.0.0</version>
        public string Serialize()
        {
            string json = JsonSerializer.Serialize(this);
            string replaced = JsonSerializer.Serialize(_instructions);

            string replacing;

            if (_instructions == null || _instructions.Count == 0)
            {
                replacing =  "[]";
            }
            else
            {
                var instructionJsonStrings = _instructions.Select(instr => InstrParam.Serialize(instr));
                replacing =  "[" + string.Join(",", instructionJsonStrings) + "]";
            }
            Debug.Log(
                @$"[Frame.Serialize]
jsonStrng: {JsonPrettyPrinter.Format(json)}
replaced: {JsonPrettyPrinter.Format(replaced)}
replacing: {JsonPrettyPrinter.Format(replacing)}");

            string result = json.Replace(replaced, replacing);

            Debug.Log($"[Frame.Serialize]result: {JsonPrettyPrinter.Format(result)}");

            return result;
        }

        /// <summary>
        /// 反序列化帧数据
        /// </summary>
        /// <version>1.0.0</version>
        public void Deserialize(string jsonString)
        {
            _instructions.Clear();

            if (string.IsNullOrWhiteSpace(jsonString))
                return;

            string cleanJson = jsonString.Trim('\uFEFF', ' ', '\t', '\n', '\r');

            try
            {
                var instructions = System.Text.Json.JsonSerializer.Deserialize<List<InstrParam>>(cleanJson);
                if (instructions != null)
                {
                    foreach (var instr in instructions)
                    {
                        InstrParam converted = InstrParam.Convert(instr);
                        _instructions.Add(converted);
                    }
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                Debug.LogError($"[Frame.Deserialize]Frame deserialization error: {ex.Message}");
            }
        }
        #endregion

        #region Print
        /// <summary>
        /// 打印帧数据信息
        /// </summary>
        /// <version>1.0.0</version>
        public string PrintString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Frame with {_instructions.Count} instructions:");

            for (int i = 0; i < _instructions.Count; i++)
            {
                sb.AppendLine($"  Instruction {i}: {InstrParam.PrintString(_instructions[i])}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 打印帧数据到控制台
        /// </summary>
        /// <version>1.0.0</version>
        public void Print()
        {
            Debug.Log($"[Frame]{PrintString()}");
        }
        #endregion

        #region Factory Methods
        /// <summary>
        /// 创建帧数据
        /// </summary>
        /// <version>1.0.0</version>
        public static Frame Create(params InstrParam[] instructions)
        {
            Frame frame = new Frame();
            frame.AddRange(instructions);
            return frame;
        }

        /// <summary>
        /// 从数组创建帧数据
        /// </summary>
        /// <version>1.0.0</version>
        public static Frame FromArray(InstrParam[] instructions)
        {
            return Create(instructions);
        }
        #endregion
    }
}
