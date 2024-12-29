namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface IBetterBetterCharacterControllerDataReader
  {
    bool UpdateBBCCState();
    bool FlyingAllowed { get; }
    bool DataFeedErrorBBCC { get; }
  }
}
