namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface ICommsDataReader
  {
    /// <summary>
    /// Gets the current voice communications ping in milliseconds
    /// </summary>
    int VoiceCommsPing { get; }

    /// <summary>
    /// Gets whether the voice communications client is connected
    /// </summary>
    bool IsVoiceConnected { get; }

    /// <summary>
    /// Gets the voice communications connection state
    /// </summary>
    string VoiceConnectionState { get; }

    /// <summary>
    /// Gets whether there was an error reading from the communications system
    /// </summary>
    bool DataFeedErrorComms { get; }

    /// <summary>
    /// Updates the communications state and returns true if any values changed
    /// </summary>
    /// <returns>True if any state values changed, false otherwise</returns>
    bool UpdateCommsState();
  }
}
