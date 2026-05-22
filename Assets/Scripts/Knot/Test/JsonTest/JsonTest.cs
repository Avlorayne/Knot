using System;
using System.Collections.Generic;
using System.IO;
using Knot.Instruction;
using Knot.Runtime.Core;
using Knot.Runtime.Data;
using Knot.Runtime.Utility;
using Knot.Test.UnityEventTest;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace Knot.Test.JsonTest
{
    /// <summary>
    /// PlotDataService 集成测试（数据层交互中心）
    /// 测试流程：创建指令 → 组装 FrameList → PlotDataService 序列化写入文件 → 读取 → 反序列化 → 比对一致性
    /// 右键组件 → Context Menu → Run JsonTest 即可执行
    /// </summary>
    public class JsonTest : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField, Tooltip("输出 JSON 文件相对于 Assets 的路径")]
        private string _outputRelativePath = "Scripts/Knot/Test/Resource/testjson.json";

        [SerializeField, Tooltip("是否打印详细日志")]
        private bool _verboseLog = true;

        private string OutputFullPath =>
            Path.Combine(Application.dataPath, _outputRelativePath);

        /// <summary>
        /// 数据层交互中心 —— 测试的唯一序列化入口
        /// </summary>
        private IPlotDataProvider _dataService;

        /// <summary>
        /// UnityEvent 持久化回调测试标记：反序列化后 Invoke 会置为 true
        /// </summary>
        private bool _unityEventPersistentCallbackInvoked;

        /// <summary>
        /// UnityEvent 持久化监听专用回调（必须是 MonoBehaviour 上的 public 方法，
        /// 才能作为 UnityEngine.Object 目标被 UnityEventTools 注册为持久化调用）
        /// </summary>
        public void OnUnityEventPersistentCallback()
        {
            _unityEventPersistentCallbackInvoked = true;
            Debug.Log("[JsonTest] UnityEvent 持久化回调被触发！");
        }

        #region Lifecycle

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_dataService == null)
            {
                // 依赖注入组装：JsonSerilizeUtility 实现 IDataSerializer，注入 PlotDataService
                IDataSerializer serializer = new JsonSerilizeUtility(JsonSerilizeUtility.Settings);
                _dataService = new PlotDataService(serializer);
            }
        }

        #endregion

        #region Test Entry

        [ContextMenu("Run JsonTest")]
        public void RunAllTests()
        {
            EnsureInitialized();

            Debug.Log("========== [JsonTest] PlotDataService 集成测试 ==========");

            bool allPassed = true;

            allPassed &= Test_FileRoundTrip();
            allPassed &= Test_IndividualInstructionRoundTrip();
            allPassed &= Test_EmptyFrameListRoundTrip();
            allPassed &= Test_NestedDialogueRoundTrip();
            allPassed &= Test_UnityEventRoundTrip();

            if (allPassed)
                Debug.Log("<color=green>========== [JsonTest] 全部测试通过！==========</color>");
            else
                Debug.LogError("<color=red>========== [JsonTest] 存在失败的测试！==========</color>");
        }

        /// <summary>
        /// 右键独立执行：从文件加载 testjson_unityevent.json 并验证持久化调用
        /// </summary>
        [ContextMenu("Load UnityEvent From File")]
        public void RunLoadUnityEventFromFile()
        {
            EnsureInitialized();
            Debug.Log("========== [JsonTest] 从文件加载 UnityEvent 测试 ==========");
            bool passed = Test_LoadUnityEventFromFile();
            if (passed)
                Debug.Log("<color=green>========== 测试通过！==========</color>");
            else
                Debug.LogError("<color=red>========== 测试失败！==========</color>");
        }

        #endregion

        #region Test Cases

        /// <summary>
        /// 核心测试：创建含多种指令的 FrameList → SaveToJson → 写文件 → 读文件 → LoadFromJson → 比对
        /// </summary>
        private bool Test_FileRoundTrip()
        {
            const string testName = "PlotDataService 文件往返测试";
            LogTestStart(testName);

            try
            {
                // 1. 构建 FrameList：每个指令独占一帧
                FrameList originalFrames = BuildFrameList_OnePerFrame();
                LogInfo($"构建 FrameList: {originalFrames.Count} 帧");

                // 2. 通过 PlotDataService 序列化
                string jsonToWrite = _dataService.SaveToJson(originalFrames);
                if (string.IsNullOrEmpty(jsonToWrite))
                    return LogTestFail(testName, "SaveToJson 返回 null/空");
                LogInfo($"SaveToJson 成功，JSON 长度: {jsonToWrite.Length} 字符");

                // 3. 写入文件
                WriteJsonToFile(jsonToWrite);
                LogInfo($"已写入: {OutputFullPath}");

                // 4. 读取文件
                string jsonRead = ReadJsonFromFile();
                if (string.IsNullOrEmpty(jsonRead))
                    return LogTestFail(testName, "文件读取返回 null/空");
                LogInfo($"文件读取成功，JSON 长度: {jsonRead.Length} 字符");

                // 5. 比对文件 IO 前后 JSON 一致性
                if (!CompareJsonStrings(jsonToWrite, jsonRead))
                    return LogTestFail(testName, "文件 IO 前后 JSON 不一致");

                // 6. 通过 PlotDataService 反序列化
                FrameList deserializedFrames = _dataService.LoadFromJson(jsonRead);
                if (deserializedFrames == null)
                    return LogTestFail(testName, "LoadFromJson 返回 null");
                if (deserializedFrames.Count != originalFrames.Count)
                    return LogTestFail(testName,
                        $"帧数量不匹配: 期望 {originalFrames.Count}, 实际 {deserializedFrames.Count}");
                LogInfo($"LoadFromJson 成功，获得 {deserializedFrames.Count} 帧");

                // 7. 深度比对：将反序列化结果再序列化，与原始 JSON 比对
                string jsonReSerialized = _dataService.SaveToJson(deserializedFrames);
                if (!CompareJsonStrings(jsonToWrite, jsonReSerialized))
                    return LogTestFail(testName, "往返序列化 JSON 不一致（数据丢失或类型错误）");

                // 8. 逐帧比对指令
                bool framesMatch = CompareFrameLists(originalFrames, deserializedFrames);
                if (!framesMatch)
                    return LogTestFail(testName, "帧内指令比对不一致");

                return LogTestPass(testName);
            }
            catch (Exception ex)
            {
                return LogTestFail(testName, $"异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 每种指令类型单独往返测试（通过 PlotDataService）
        /// </summary>
        private bool Test_IndividualInstructionRoundTrip()
        {
            const string testName = "单指令 PlotDataService 往返测试";
            LogTestStart(testName);

            try
            {
                var testCases = new List<(string label, InstrParam instr)>
                {
                    ("TypeDialogueParam", CreateTypeDialogueParam()),
                    ("AUDIParam",        CreateAUDIParam()),
                    ("IMAGParam",        CreateIMAGParam()),
                    ("JUMPParam",        CreateJUMPParam()),
                    ("SELCParam",        CreateSELCParam()),
                    ("VIDEParam",        CreateVIDEParam()),
                    ("UnityEventTestParam", CreateUnityEventTestParam()),
                };

                foreach (var (label, original) in testCases)
                {
                    // 包装为单帧 FrameList
                    FrameList originalFL = WrapInFrameList(original);

                    // SaveToJson
                    string json = _dataService.SaveToJson(originalFL);
                    if (string.IsNullOrEmpty(json))
                        return LogTestFail(testName, $"[{label}] SaveToJson 失败");

                    // LoadFromJson
                    FrameList deserializedFL = _dataService.LoadFromJson(json);
                    if (deserializedFL == null || deserializedFL.Count == 0)
                        return LogTestFail(testName, $"[{label}] LoadFromJson 返回空");

                    // 提取反序列化后的指令
                    InstrParam deserialized = deserializedFL[0][0];
                    if (deserialized == null)
                        return LogTestFail(testName, $"[{label}] 反序列化指令为 null");

                    // 类型检查
                    if (deserialized.GetType() != original.GetType())
                        return LogTestFail(testName,
                            $"[{label}] 类型不匹配: 期望 {original.GetType().Name}, 实际 {deserialized.GetType().Name}");

                    // 往返 JSON 一致性检查
                    string jsonReSerialized = _dataService.SaveToJson(WrapInFrameList(deserialized));
                    if (!CompareJsonStrings(json, jsonReSerialized))
                        return LogTestFail(testName, $"[{label}] 往返 JSON 不一致");
                    WriteJsonToFile(jsonReSerialized); // 输出 JSON 以便调试

                    LogInfo($"  [{label}] 通过");
                }

                return LogTestPass(testName);
            }
            catch (Exception ex)
            {
                return LogTestFail(testName, $"异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 空 FrameList 边界测试
        /// </summary>
        private bool Test_EmptyFrameListRoundTrip()
        {
            const string testName = "空 FrameList 往返测试";
            LogTestStart(testName);

            try
            {
                var emptyFL = new FrameList();
                string json = _dataService.SaveToJson(emptyFL);

                if (string.IsNullOrEmpty(json))
                    return LogTestFail(testName, "空 FrameList 序列化返回 null/空");

                FrameList deserialized = _dataService.LoadFromJson(json);
                if (deserialized == null)
                    return LogTestFail(testName, "空 FrameList 反序列化返回 null");
                if (deserialized.Count != 0)
                    return LogTestFail(testName, $"空 FrameList 应有 0 帧，实际 {deserialized.Count}");

                return LogTestPass(testName);
            }
            catch (Exception ex)
            {
                return LogTestFail(testName, $"异常: {ex.Message}");
            }
        }

        /// <summary>
        /// TypeDialogueParam 嵌套 Dialogue 对象专项测试
        /// </summary>
        private bool Test_NestedDialogueRoundTrip()
        {
            const string testName = "嵌套 Dialogue 对象往返测试";
            LogTestStart(testName);

            try
            {
                var original = new TypeDialogueParam(new Dialogue("测试角色", "这是一段测试对话内容。"))
                {
                    IsCanCoexist = false,
                    IsRelease = true,
                    IsCanBeSkipped = true,
                    IsBeWaited = true,
                };

                FrameList originalFL = WrapInFrameList(original);
                string json = _dataService.SaveToJson(originalFL);
                if (string.IsNullOrEmpty(json))
                    return LogTestFail(testName, "SaveToJson 失败");

                FrameList deserializedFL = _dataService.LoadFromJson(json);
                if (deserializedFL == null || deserializedFL.Count == 0)
                    return LogTestFail(testName, "LoadFromJson 返回空");

                var deserialized = deserializedFL[0][0] as TypeDialogueParam;
                if (deserialized == null)
                    return LogTestFail(testName,
                        $"类型错误: 期望 TypeDialogueParam, 实际 {deserializedFL[0][0]?.GetType().Name ?? "null"}");

                // 验证嵌套 Dialogue
                if (deserialized.dialogue == null)
                    return LogTestFail(testName, "dialogue 字段为 null");
                if (deserialized.dialogue.Name != "测试角色")
                    return LogTestFail(testName,
                        $"dialogue.Name: 期望 '测试角色', 实际 '{deserialized.dialogue.Name}'");
                if (deserialized.dialogue.Sentence != "这是一段测试对话内容。")
                    return LogTestFail(testName,
                        $"dialogue.Sentence: 期望 '这是一段测试对话内容。', 实际 '{deserialized.dialogue.Sentence}'");

                // 验证基类属性
                if (deserialized.IsBeWaited != true)
                    return LogTestFail(testName, "IsBeWaited 不匹配");
                if (deserialized.IsRelease != true)
                    return LogTestFail(testName, "IsRelease 不匹配");
                if (deserialized.IsCanCoexist != false)
                    return LogTestFail(testName, "IsCanCoexist 不匹配");

                // 验证往返 JSON 一致性
                string jsonReSerialized = _dataService.SaveToJson(WrapInFrameList(deserialized));
                if (!CompareJsonStrings(json, jsonReSerialized))
                    return LogTestFail(testName, "往返 JSON 不一致");

                return LogTestPass(testName);
            }
            catch (Exception ex)
            {
                return LogTestFail(testName, $"异常: {ex.Message}");
            }
        }

        /// <summary>
        /// UnityEvent 持久化监听往返测试：
        /// 注册持久化监听 → 序列化 → 反序列化 → Invoke → 验证回调仍存在且可触发
        /// </summary>
        private bool Test_UnityEventRoundTrip()
        {
            const string testName = "UnityEvent 持久化监听往返测试";
            LogTestStart(testName);

            try
            {
                // === 阶段 1：创建指令并注册持久化监听 ===
                var original = CreateUnityEventTestParam();

                // 使用 UnityEventTools 将本 MonoBehaviour 的公开方法注册为持久化调用
                // 持久化调用会被 JsonUtility 序列化到 m_PersistentCalls 中
#if UNITY_EDITOR
                UnityEventTools.AddPersistentListener(original.OnTestComplete, OnUnityEventPersistentCallback);
#else
                // 非 Editor 环境回退：仅做 null 检查
                if (original.OnTestComplete == null)
                    return LogTestFail(testName, "OnTestComplete 为 null");
#endif

                // 立即调用以验证持久化监听已生效
                _unityEventPersistentCallbackInvoked = false;
                original.OnTestComplete.Invoke();
                if (!_unityEventPersistentCallbackInvoked)
                    return LogTestFail(testName, "序列化前: Invoke 未触发持久化监听（注册失败）");
                LogInfo("序列化前: Invoke 成功触发持久化监听");

                FrameList originalFL = WrapInFrameList(original);

                // === 阶段 2：序列化 ===
                string json = _dataService.SaveToJson(originalFL);
                if (string.IsNullOrEmpty(json))
                    return LogTestFail(testName, "SaveToJson 返回 null/空");
                LogInfo($"SaveToJson 成功，JSON 长度: {json.Length}");

                // 验证 JSON 包含持久化调用的序列化数据
                if (!json.Contains("m_PersistentCalls"))
                    return LogTestFail(testName, "JSON 中未找到 UnityEvent 序列化标记 'm_PersistentCalls'");

                // === 阶段 3：反序列化 ===
                FrameList deserializedFL = _dataService.LoadFromJson(json);
                if (deserializedFL == null || deserializedFL.Count == 0)
                    return LogTestFail(testName, "LoadFromJson 返回空");

                var deserialized = deserializedFL[0][0] as UnityEventTestParam;
                if (deserialized == null)
                    return LogTestFail(testName,
                        $"类型错误: 期望 UnityEventTestParam, 实际 {deserializedFL[0][0]?.GetType().Name ?? "null"}");

                // 验证普通字段
                if (deserialized.TestMessage != "UnityEvent 序列化测试消息")
                    return LogTestFail(testName,
                        $"TestMessage 不匹配: 期望 'UnityEvent 序列化测试消息', 实际 '{deserialized.TestMessage}'");

                // 验证 UnityEvent 非 null
                if (deserialized.OnTestComplete == null)
                    return LogTestFail(testName, "OnTestComplete 为 null（UnityEvent 未正确反序列化）");

                // === 阶段 4：反序列化后 Invoke，验证持久化监听仍在 ===
                _unityEventPersistentCallbackInvoked = false;
                deserialized.OnTestComplete.Invoke();

                if (!_unityEventPersistentCallbackInvoked)
                    return LogTestFail(testName,
                        "反序列化后: Invoke 未触发持久化监听（持久化调用丢失或目标引用断裂）");
                LogInfo("反序列化后: Invoke 成功触发持久化监听（持久化调用已正确恢复）");

                // === 阶段 5：验证基类属性 ===
                if (deserialized.IsRelease != true)
                    return LogTestFail(testName, "IsRelease 不匹配");

                // === 阶段 6：往返 JSON 一致性 ===
                string jsonReSerialized = _dataService.SaveToJson(WrapInFrameList(deserialized));
                if (!CompareJsonStrings(json, jsonReSerialized))
                    return LogTestFail(testName, "往返 JSON 不一致（UnityEvent 数据丢失）");

                // 输出往返 JSON 到独立文件，便于查看 UnityEvent 持久化调用的序列化结构
                WriteJsonToFile(jsonReSerialized, "Scripts/Knot/Test/Resource/testjson_unityevent.json");
                LogInfo($"UnityEvent 往返 JSON 已输出到: testjson_unityevent.json");

                return LogTestPass(testName);
            }
            catch (Exception ex)
            {
                return LogTestFail(testName, $"异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载 testjson_unityevent.json → 反序列化 → Invoke → 验证持久化调用仍有效
        /// 模拟"读取之前保存的剧情数据并执行"的真实场景
        /// </summary>
        private bool Test_LoadUnityEventFromFile()
        {
            const string testName = "从文件加载 UnityEvent 并调用";
            const string filePath = "Scripts/Knot/Test/Resource/testjson_unityevent.json";
            LogTestStart(testName);

            try
            {
                // === 阶段 1：从文件读取 JSON ===
                string json = ReadJsonFromFile(filePath);
                if (string.IsNullOrEmpty(json))
                    return LogTestFail(testName, $"文件读取失败: {filePath}（请先运行 UnityEvent 往返测试生成该文件）");
                LogInfo($"读取文件成功，JSON 长度: {json.Length}");

                // === 阶段 2：反序列化 ===
                FrameList frameList = _dataService.LoadFromJson(json);
                if (frameList == null || frameList.Count == 0)
                    return LogTestFail(testName, "LoadFromJson 返回空");

                var param = frameList[0][0] as UnityEventTestParam;
                if (param == null)
                    return LogTestFail(testName,
                        $"类型错误: 期望 UnityEventTestParam, 实际 {frameList[0][0]?.GetType().Name ?? "null"}");

                if (param.OnTestComplete == null)
                    return LogTestFail(testName, "OnTestComplete 为 null");

                LogInfo($"反序列化成功: TestMessage='{param.TestMessage}'");

                // === 阶段 3：Invoke 并验证持久化回调是否触发 ===
                _unityEventPersistentCallbackInvoked = false;
                param.OnTestComplete.Invoke();

                if (!_unityEventPersistentCallbackInvoked)
                    return LogTestFail(testName,
                        "Invoke 未触发持久化回调（文件中的持久化调用丢失或目标引用断裂）");
                LogInfo("Invoke 成功触发持久化回调 —— 文件加载的 UnityEvent 功能正常");

                return LogTestPass(testName);
            }
            catch (Exception ex)
            {
                return LogTestFail(testName, $"异常: {ex.Message}");
            }
        }

        #endregion

        #region Test Data Factory

        /// <summary>
        /// 构建 FrameList：每种指令独占一帧
        /// </summary>
        private FrameList BuildFrameList_OnePerFrame()
        {
            var fl = new FrameList();
            fl.Add(new[] { CreateTypeDialogueParam() });
            fl.Add(new[] { CreateAUDIParam() });
            fl.Add(new[] { CreateIMAGParam() });
            fl.Add(new[] { CreateJUMPParam() });
            fl.Add(new[] { CreateSELCParam() });
            fl.Add(new[] { CreateVIDEParam() });
            fl.Add(new[] { CreateUnityEventTestParam() });
            return fl;
        }

        /// <summary>
        /// 将单个指令包装为单帧 FrameList
        /// </summary>
        private FrameList WrapInFrameList(InstrParam instr)
        {
            var fl = new FrameList();
            fl.Add(new[] { instr });
            return fl;
        }

        private TypeDialogueParam CreateTypeDialogueParam()
        {
            return new TypeDialogueParam(new Dialogue("张三", "你好，欢迎来到这个世界！"))
            {
                IsCanCoexist = false,
                IsRelease = true,
                IsCanBeSkipped = true,
                IsBeWaited = false,
            };
        }

        private AUDIParam CreateAUDIParam()
        {
            return new AUDIParam
            {
                IsCanCoexist = true,
                IsRelease = false,
                IsCanBeSkipped = false,
                IsBeWaited = true,
            };
        }

        private IMAGParam CreateIMAGParam()
        {
            return new IMAGParam
            {
                IsCanCoexist = false,
                IsRelease = true,
                IsCanBeSkipped = true,
                IsBeWaited = false,
            };
        }

        private JUMPParam CreateJUMPParam()
        {
            return new JUMPParam
            {
                IsCanCoexist = false,
                IsRelease = false,
                IsCanBeSkipped = false,
                IsBeWaited = false,
            };
        }

        private SELCParam CreateSELCParam()
        {
            return new SELCParam
            {
                IsCanCoexist = false,
                IsRelease = true,
                IsCanBeSkipped = true,
                IsBeWaited = true,
            };
        }

        private VIDEParam CreateVIDEParam()
        {
            return new VIDEParam
            {
                IsCanCoexist = true,
                IsRelease = true,
                IsCanBeSkipped = true,
                IsBeWaited = false,
            };
        }

        private UnityEventTestParam CreateUnityEventTestParam()
        {
            return new UnityEventTestParam("UnityEvent 序列化测试消息")
            {
                IsCanCoexist = false,
                IsRelease = true,
                IsCanBeSkipped = true,
                IsBeWaited = false,
            };
        }

        #endregion

        #region File I/O

        private void WriteJsonToFile(string json)
        {
            WriteJsonToFile(json, _outputRelativePath);
        }

        private void WriteJsonToFile(string json, string relativePath)
        {
            string fullPath = Path.Combine(Application.dataPath, relativePath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fullPath, json, System.Text.Encoding.UTF8);
        }

        private string ReadJsonFromFile()
        {
            return ReadJsonFromFile(_outputRelativePath);
        }

        private string ReadJsonFromFile(string relativePath)
        {
            string fullPath = Path.Combine(Application.dataPath, relativePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[JsonTest] 文件不存在: {fullPath}");
                return null;
            }

            return File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
        }

        #endregion

        #region Comparison Helpers

        /// <summary>
        /// 比对两个 FrameList 的帧内指令是否一致（通过再序列化逐帧比对）
        /// </summary>
        private bool CompareFrameLists(FrameList a, FrameList b)
        {
            if (a.Count != b.Count)
            {
                LogError($"帧数量不同: {a.Count} vs {b.Count}");
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                Frame frameA = a[i];
                Frame frameB = b[i];

                if (frameA.Count != frameB.Count)
                {
                    LogError($"Frame[{i}] 指令数量不同: {frameA.Count} vs {frameB.Count}");
                    return false;
                }

                for (int j = 0; j < frameA.Count; j++)
                {
                    InstrParam instrA = frameA[j];
                    InstrParam instrB = frameB[j];

                    if (instrA.GetType() != instrB.GetType())
                    {
                        LogError($"Frame[{i}][{j}] 类型不同: {instrA.GetType().Name} vs {instrB.GetType().Name}");
                        return false;
                    }

                    // 通过再序列化深度比对
                    string jsonA = _dataService.SaveToJson(WrapInFrameList(instrA));
                    string jsonB = _dataService.SaveToJson(WrapInFrameList(instrB));

                    if (!CompareJsonStrings(jsonA, jsonB))
                    {
                        LogError($"Frame[{i}][{j}] ({instrA.GetType().Name}) 数据不一致");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 比对两个 JSON 字符串（去除 BOM 和首尾空白后逐字符比较）
        /// </summary>
        private bool CompareJsonStrings(string a, string b)
        {
            string normalizedA = (a ?? "").Trim('\uFEFF', ' ', '\t', '\n', '\r');
            string normalizedB = (b ?? "").Trim('\uFEFF', ' ', '\t', '\n', '\r');

            if (normalizedA != normalizedB)
            {
                int diffPos = FindFirstDifference(normalizedA, normalizedB);
                LogError($"JSON 不一致，首个差异位置: {diffPos}");
                LogError($"  A[{diffPos}]: '{CharAtSafe(normalizedA, diffPos)}'");
                LogError($"  B[{diffPos}]: '{CharAtSafe(normalizedB, diffPos)}'");
                if (_verboseLog)
                {
                    LogError($"--- JSON A (前200字符) ---\n{Truncate(normalizedA, 200)}");
                    LogError($"--- JSON B (前200字符) ---\n{Truncate(normalizedB, 200)}");
                }
                return false;
            }
            return true;
        }

        private int FindFirstDifference(string a, string b)
        {
            int minLen = Math.Min(a.Length, b.Length);
            for (int i = 0; i < minLen; i++)
                if (a[i] != b[i])
                    return i;
            return minLen;
        }

        private string CharAtSafe(string s, int index)
        {
            if (index < 0 || index >= s.Length) return "<OUT_OF_RANGE>";
            char c = s[index];
            return c == '\n' ? "\\n" : c == '\r' ? "\\r" : c == '\t' ? "\\t" : c.ToString();
        }

        private string Truncate(string s, int maxLen)
        {
            if (s == null) return "<null>";
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "...";
        }

        #endregion

        #region Logging

        private void LogTestStart(string name) =>
            Debug.Log($"<color=cyan>--- [Test] {name} ---</color>");

        private bool LogTestPass(string name)
        {
            Debug.Log($"<color=green>[PASS] {name}</color>");
            return true;
        }

        private bool LogTestFail(string name, string reason)
        {
            LogError($"[FAIL] {name}: {reason}");
            return false;
        }

        private void LogInfo(string msg)
        {
            if (_verboseLog)
                Debug.Log($"[JsonTest] {msg}");
        }

        private void LogError(string msg) =>
            Debug.LogError($"[JsonTest] {msg}");

        #endregion
    }
}

