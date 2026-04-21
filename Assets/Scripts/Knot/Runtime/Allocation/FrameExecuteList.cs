using System;
using System.Collections.Generic;
using System.Linq;
using Knot.Runtime.Allocation;
using Knot.Runtime.Core;
using Knot.Runtime.Data;
using Knot.Runtime.Execution;
using Knot.Runtime.Utility;
using UnityEditor;
using UnityEngine;

namespace Knot.scr.Allocate
{
    public class FrameExecuteList
    {
        #region Const Char
        private const string DEVIDE_CHAR = "---------------------------------------------------------------------------";
        public static int MaxPrint { get; } = 65;
        #endregion

        private const string Tag = "Instruction";

        private readonly List<FrameExecute> _frameExList = new();
        /// <summary>
        /// InstrExecute Type - Prefab Type
        /// </summary>
        /// <key>InstrExecute Type</key>
        /// <Value>Prefab</Value>
        private readonly Dictionary<Type,GameObject> _prefabTable = new();
        private readonly List<KeyValuePair<InstrParam, InstrExecute>> _executorTable = new();
        private readonly HashSet<GameObject> _executors = new();
        private Dictionary<Type, ExecutePool> _executorPool = new();

        #region Property Override

        public FrameExecute this[int index] { get => _frameExList[index]; }
        public int Count { get => _frameExList.Count; }
        public List<FrameExecute> Content => _frameExList;
        private HashSet<FrameExecute> _hashSet = new();
        public bool Contains(FrameExecute frameExecute)
        {
            if (_hashSet.Count != _frameExList.Count)
            {
                _hashSet.Clear();
                _hashSet = new HashSet<FrameExecute>(_frameExList);
            }

            return _hashSet.Contains(frameExecute);
        }
        #endregion

        #region Construction

        public FrameExecuteList(FrameList frames)
        {
            Cleanup();

            GetAllPrefabs();

            foreach (var frame in frames.Content)
            {
                foreach (var instr in frame.Content)
                {
                    Allocate(instr);
                }
            }

            foreach (var exe in _executors)
            {
                if (exe != null)
                    exe.SetActive(false);
            }

            // 按帧划分
            ParseByFrames(frames);
        }

        // Clean uo All Data in this Class
        private void Cleanup()
        {
            foreach (var executor in _executors)
            {
                if (executor != null)
                {
                    GameObject.DestroyImmediate(executor);
                }
            }
            _executors.Clear();
            _executorTable.Clear();
            _frameExList.Clear();
        }

        private void GetAllPrefabs()
        {
            // 查找所有预制体资源的GUID
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                // Check Tag
                if (prefab != null && prefab.CompareTag(Tag))
                {
                    // could find diverse scripts
                    MonoBehaviour[] scripts = prefab.GetComponents<MonoBehaviour>();
                    if (scripts.Length == 0)
                    {
                        Debug.LogError($"[FrameExecuteList.GetAllPrefabs]{path}: {prefab} does not contain Script!");
                        continue;
                    }

                    // Check MonoBehaviour Type
                    List<Type> targetTypes = new();
                    foreach (var script in scripts)
                    {
                        Type scriptType = script.GetType();
                        if (scriptType.IsSubclassOf(typeof(InstrExecute)))
                        {
                            targetTypes.Add(scriptType);
                        }
                    }
                    // no executor attached
                    if (targetTypes.Count == 0)
                    {
                        Debug.LogError($"[FrameExecuteList.GetAllPrefabs]{path}: {prefab} does not contain InstrExecute Script!");
                        continue;
                    }

                    // more than one executor attached
                    if (targetTypes.Count != 1)
                    {
                        Debug.LogError($"[FrameExecuteList.GetAllPrefabs]{path}: {prefab} Contains more than one InstrExecute Script!");
                        continue;
                    }

                    Type type = targetTypes[0];

                    // there exists the same type, so this Executor is excess
                    if (!_prefabTable.TryAdd(type, prefab))
                    {
                        Debug.LogError($"[FrameExecuteList.GetAllPrefabs]{type} Matches multiple Executors!");
                        continue;
                    }

                    Debug.Log($"[FrameExecuteList.GetAllPrefabs]Find instr Prefab: {path}: {type.Name}");
                }
            }

            Debug.Log($"[FrameExecuteList.GetAllPrefabs]Found {_prefabTable.Count} prefabs in total");
        }

        private void Allocate(InstrParam param)
        {
            InstrExecute execute = GetReleasedExecutor(param);

            // Add null check before adding to dictionary
            if (execute != null)
            {
                _executorTable.Add(new KeyValuePair<InstrParam, InstrExecute>(param, execute));
            }
            else
            {
                throw new InvalidOperationException($"[FrameExecuteList.Allocate]Failed to create or find executor for {param.ExecutorType}");
            }
        }

        private InstrExecute GetReleasedExecutor(InstrParam param)
        {
            // Step1. Find Type-Match Pairs used to exist
            KeyValuePair<InstrParam, InstrExecute>[] matchPairs = _executorTable
                .Where(pair => param.ExecutorType == pair.Value.GetType()).ToArray();

            Debug.Log($"[FrameExecuteList.GetReleasedExecutor] Matched Pairs: {matchPairs.Length}");

            // Step2. Get Released Executor
            List<InstrExecute> releasedExecutors = new ();

            foreach (var pair in matchPairs)
            {
                if (pair.Key.IsRelease)
                {
                    releasedExecutors.Add(pair.Value);
                }
                else if (releasedExecutors.Contains(pair.Value)) // !pair.Key.IsRelease && releasedExecutors.Contains(pair.Value)
                {
                    releasedExecutors.Remove(pair.Value);
                }
            }

            // Step3. Check if there exists a usable executor.
            // if not, Create a new instance and return.
            if (releasedExecutors.Count == 0)
            {
                Type executorType = param.ExecutorType;
                if (executorType != null && _prefabTable.TryGetValue(executorType, out var prefabType))
                {
                    // 创建新对象
                    GameObject _object = GameObject.Instantiate(
                        prefabType,
                        new Vector3(),
                        Quaternion.identity);

                    _object.name =_object.name.Replace("(Clone)", "");

                    if (_object != null)
                    {
                        InstrExecute executor = _object.GetComponent<InstrExecute>();
                        if (executor != null)
                        {
                            releasedExecutors.Add(executor);
                            _executors.Add(_object);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[FrameExecuteList.GetReleasedExecutor]Failed to create instance of {param.ExecutorType}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"[FrameExecuteList.GetReleasedExecutor]Failed to Find Executor Type of {param.ExecutorType}");
                }
            }

            return releasedExecutors.FirstOrDefault();
        }

        private void ParseByFrames(FrameList frames)
        {
            _frameExList.Clear();

            foreach (var frame in frames.Content)
            {
                List<KeyValuePair<InstrParam, InstrExecute>> pairs = new();

                foreach (var instr in frame.Content)
                {
                    InstrExecute execute = _executorTable.FirstOrDefault(pair => pair.Key == instr).Value;

                    if (execute != null)
                    {
                        pairs.Add(new KeyValuePair<InstrParam, InstrExecute>(instr, execute));
                    }
                    else
                    {
                        Debug.LogError($"[FrameExecuteList.ParseByFrames]No executor found for instruction: {instr.Name}");
                    }
                }

                _frameExList.Add(new FrameExecute(pairs.ToArray()));
            }
        }

        #endregion

        #region Print
        public void Print()
        {
            Debug.Log($"[{nameof(FrameExecuteList)}] {DEVIDE_CHAR}".Truncate(MaxPrint));

            for (int i = 0; i < _frameExList.Count; i++)
            {
                Debug.Log($"Frame {i}: \n{_frameExList[i].PrintString()}");
            }

            Debug.Log($"[{nameof(FrameExecuteList)}] In total {_frameExList.Count} items {DEVIDE_CHAR}".Truncate(MaxPrint));
        }
        #endregion
    }
}
