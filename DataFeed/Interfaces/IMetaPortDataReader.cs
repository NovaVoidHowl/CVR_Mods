namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface IMetaPortDataReader
  {
    bool UpdateMetaPortState();
    void UpdateInstanceInfo();
    bool UpdateMetaPortWorldSettings();

    string CurrentInstanceId { get; }
    string CurrentInstanceName { get; }
    string CurrentWorldId { get; }
    string CurrentInstancePrivacy { get; }
    int BuildId { get; }
    string HardwareId { get; }
    int CurrentPing { get; }

    bool PropsAllowed { get; }
    bool PortalsAllowed { get; }
    bool NameplatesEnabled { get; }
    bool DataFeedErrorMetaPort { get; }
  }
}
