using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface IOSCDataReader
  {
    bool UpdateOSCState();

    bool OSCEnabled { get; }
    bool OSCRunning { get; }
    bool OSCVerboseLogging { get; }
    string InboundAddress { get; }
    int InboundPort { get; }
    string OutboundAddress { get; }
    int OutboundPort { get; }
    string OSCQueryServiceName { get; }
    int ConnectedOSCClients { get; }
    IReadOnlyList<OSCClientInfo> OSCClients { get; }
    bool DataFeedErrorOSC { get; }
  }
}
