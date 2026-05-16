using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface IOSCParameterDataReader : IDisposable
  {
    void Initialize();

    void Clear();

    void Configure(int recentMessageCapacity, bool verboseArguments, int argumentStringLimit);

    bool UpdateOSCParameterState();

    IReadOnlyList<OSCParameterInfo> Parameters { get; }

    IReadOnlyList<OSCParameterMessageInfo> RecentMessages { get; }

    int KnownParameterCount { get; }

    int RecentMessageCount { get; }

    bool DataFeedErrorOSCParameters { get; }
  }
}
