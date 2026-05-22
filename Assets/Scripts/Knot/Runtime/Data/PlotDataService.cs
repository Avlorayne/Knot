using Knot.Runtime.Data;



namespace Knot.Runtime.Data
{
    /// 数据层交互中心，负责剧情数据的加载和保存
    public class PlotDataService : IPlotDataProvider
    {
        private readonly IDataSerializer _serializer;

        // 构造函数注入 JsonSerilizeUtility
        public PlotDataService(IDataSerializer serializer)
        {
            _serializer = serializer;
        }

        public FrameList LoadFromJson(string json)
        {
            return _serializer.Deserialize<FrameList>(json);
        }
        public string SaveToJson(FrameList frameList)
        {
            return _serializer.Serialize(frameList);
        }
    }
}