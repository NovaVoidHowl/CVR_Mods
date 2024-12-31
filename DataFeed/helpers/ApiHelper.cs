using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.helpers
{
  public static class ApiHelper
  {
    public static bool IsValidApiKey(string apiKey, string configKey)
    {
      if (string.IsNullOrEmpty(apiKey) || apiKey != configKey)
      {
#if DEBUG
        MelonLogger.Error($"Invalid API Key: received '{apiKey}', expected '{configKey}'");
#endif
        return false;
      }
      return true;
    }
  }
}
