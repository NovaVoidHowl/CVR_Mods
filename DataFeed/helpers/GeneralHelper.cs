using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.helpers
{
  public static class GeneralHelper
  {
    public static void DebugLog(string message)
    {
#if DEBUG
      MelonLogger.Msg("[DEBUG] " + message);
#endif
    }
  }
}
