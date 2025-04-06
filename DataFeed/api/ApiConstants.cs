using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Properties;
using MelonLoader;
using MelonLoader.Utils;

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

    public const string Version = "0.4.4";
    public const string ApiBasePath = "http://localhost:9000/api/v1/";
    public const string DocsPath = "http://localhost:9000/docs/";
    public const string WebSocketEndpoint = "ws://localhost:9001";
    public const string WebSocketTopic = "tracking";
    public const string ModName = "DataFeed";

    // API version constants
    public static readonly Version RestApiVersion = new Version(1, 0, 0);
    public static readonly Version WebSocketApiVersion = new Version(1, 0, 0);

    // Use System.Version to parse the version string
    public static readonly Version ModVersion = new Version(Version);
  }
}
