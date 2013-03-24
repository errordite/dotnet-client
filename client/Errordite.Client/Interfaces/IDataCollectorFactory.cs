namespace Errordite.Client.Interfaces
{
    public interface IDataCollectorFactory
    {
        IDataCollector Create();
        string Prefix { get; }
    }
}