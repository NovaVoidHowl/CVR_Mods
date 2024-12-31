using Semver;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public static class ApiConstants
  {
    public const string ApiKeyHeader = "X-API-Key";
    public const string ApiKeyInvalidError = "Invalid API Key";
    public const string jsonContentType = "application/json";
    public static readonly string[] availableRESTEndpoints = new[]
    {
      "/api",
      "/api/v1",
      "/api/v1/parameters",
      "/api/v1/instance",
      "/api/v1/avatar",
      "/api/v1/realtime"
    };

    public static readonly SemVersion RestApiVersion = new SemVersion(1, 0, 0);
    public static readonly SemVersion WebSocketApiVersion = new SemVersion(1, 0, 0);
  }
}
