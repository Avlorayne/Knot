using Knot.Runtime.Execution;
using Knot.Runtime.Data;
using UnityEngine;

namespace Knot.Samples
{
    /// <summary>
    /// 示例场景控制器，演示如何使用Knot剧情系统
    /// </summary>
    [Version("1.0.0")]
    public class SampleSceneController : MonoBehaviour
    {
        [Header("剧情脚本")]
        [SerializeField]
        private TextAsset plotJson;

        private PlotPerformSys plotSystem;

        private void Start()
        {
            InitializePlotSystem();
        }

        private void InitializePlotSystem()
        {
            plotSystem = PlotPerformSys.Instance;
            
            if (plotJson != null)
            {
                Plot plot = Plot.FromJson(plotJson.text);
                if (plot != null)
                {
                    Debug.Log($"[SampleSceneController] 剧情加载成功，共 {plot.Frames.Count} 帧");
                }
            }
            else
            {
                Debug.LogWarning("[SampleSceneController] 未指定剧情脚本文件");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[SampleSceneController] 按下空格键 - 演示跳过功能");
            }
        }
    }
}
