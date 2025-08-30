namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface IFPSDataReader
  {
    /// <summary>
    /// Gets the current frames per second
    /// </summary>
    int CurrentFPS { get; }

    /// <summary>
    /// Updates the FPS calculation and returns true if the value changed
    /// </summary>
    /// <returns>True if the FPS value changed, false otherwise</returns>
    bool UpdateFPS();
  }
}
