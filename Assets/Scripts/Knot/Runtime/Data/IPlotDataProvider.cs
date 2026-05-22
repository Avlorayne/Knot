using Knot.Runtime.Data;

namespace Knot.Runtime.Data
{
    /// 剧情数据提供者接口
    public interface IPlotDataProvider
    {
        FrameList LoadFromJson(string json);
        string SaveToJson(FrameList frameList);
    }
}