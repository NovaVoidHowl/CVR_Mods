namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface INetworkManagerDataReader
  {
    bool UpdateNetworkManagerState();

    int GameNetworkPing { get; }
    bool IsConnected { get; }
    string ConnectionState { get; }
    bool DataFeedErrorNetworkManager { get; }
  }
}
