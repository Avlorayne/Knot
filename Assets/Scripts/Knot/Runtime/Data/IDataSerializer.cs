namespace Knot.Runtime.Data
{
    /// 统一序列化接口，屏蔽底层序列化实现
    public interface IDataSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string json);
    }
}