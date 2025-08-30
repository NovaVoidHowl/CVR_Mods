using Newtonsoft.Json;
using EmbedIO.WebApi;
using EmbedIO;
using EmbedIO.Routing;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;
using uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public class DataFeedControllerV1 : WebApiController
  {
    private readonly DataFeed _dataFeed;

    public DataFeedControllerV1(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
    }

    private void AddCorsHeaders()
    {
      Response.Headers.Add("Access-Control-Allow-Origin", "*");
      Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
      Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-API-Key, Authorization");
      Response.Headers.Add("Access-Control-Max-Age", "86400");
    }

    [Route(HttpVerbs.Options, "/parameters")]
    [Route(HttpVerbs.Options, "/instance")]
    [Route(HttpVerbs.Options, "/avatar")]
    [Route(HttpVerbs.Options, "/world")]
    [Route(HttpVerbs.Options, "/realtime")]
    [Route(HttpVerbs.Options, "/")]
    public object HandlePreflight()
    {
      AddCorsHeaders();
      Response.StatusCode = 200;
      return new { };
    }

    [Route(HttpVerbs.Get, "/parameters")]
    public object GetState()
    {
      AddCorsHeaders();
      GeneralHelper.DebugLog("Processing request for parameters.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        return new { error = ApiConstants.ApiKeyInvalidError };
      }

      var state = new
      {
        flyingAllowed = _dataFeed.BBCCReader.FlyingAllowed,
        propsAllowed = _dataFeed.MetaPortReader.PropsAllowed,
        portalsAllowed = _dataFeed.MetaPortReader.PortalsAllowed,
        nameplatesEnabled = _dataFeed.MetaPortReader.NameplatesEnabled,
        dataFeedErrorBBCC = _dataFeed.BBCCReader.DataFeedErrorBBCC,
        dataFeedErrorMetaPort = _dataFeed.MetaPortReader.DataFeedErrorMetaPort,
        dataFeedDisabled = _dataFeed.DataFeedDisabled
      };

      GeneralHelper.DebugLog("Parameters retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return state;
    }

    [Route(HttpVerbs.Get, "/instance")]
    public object GetInstanceInfo()
    {
      AddCorsHeaders();
      GeneralHelper.DebugLog("Processing request for instance info.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        return new { error = ApiConstants.ApiKeyInvalidError };
      }

      var instanceInfo = new
      {
        currentInstanceId = _dataFeed.MetaPortReader.CurrentInstanceId,
        currentInstanceName = _dataFeed.MetaPortReader.CurrentInstanceName,
        currentWorldId = _dataFeed.MetaPortReader.CurrentWorldId,
        currentInstancePrivacy = _dataFeed.MetaPortReader.CurrentInstancePrivacy,
        worldDetails = _dataFeed.CurrentWorldDetails ?? new WorldAbiApiInfo(),
        detailsAvailable = _dataFeed.CurrentWorldDetails != null
      };

      GeneralHelper.DebugLog("Instance info retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return instanceInfo;
    }

    [Route(HttpVerbs.Get, "/avatar")]
    public object GetAvatarInfo()
    {
      AddCorsHeaders();
      GeneralHelper.DebugLog("Processing request for avatar info.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        return new { error = ApiConstants.ApiKeyInvalidError };
      }

      var avatarInfo = new
      {
        currentAvatarId = _dataFeed.CurrentAvatarId,
        avatarDetails = _dataFeed.CurrentAvatarDetails ?? new AvatarAbiApiInfo(),
        detailsAvailable = _dataFeed.CurrentAvatarDetails != null
      };

      GeneralHelper.DebugLog("Avatar info retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return avatarInfo;
    }

    [Route(HttpVerbs.Get, "/world")]
    public object GetWorldInfo()
    {
      AddCorsHeaders();
      GeneralHelper.DebugLog("Processing request for world info.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        return new { error = ApiConstants.ApiKeyInvalidError };
      }

      var worldInfo = new
      {
        currentWorldId = _dataFeed.CurrentWorldId,
        worldDetails = _dataFeed.CurrentWorldDetails ?? new WorldAbiApiInfo(),
        detailsAvailable = _dataFeed.CurrentWorldDetails != null
      };

      GeneralHelper.DebugLog("World info retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return worldInfo;
    }

    [Route(HttpVerbs.Get, "/realtime")]
    public object GetRealTimeData()
    {
      AddCorsHeaders();
      GeneralHelper.DebugLog("Processing request for real-time data.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        return new { error = ApiConstants.ApiKeyInvalidError };
      }

      var realTimeData = new
      {
        currentPing = _dataFeed.NetworkManagerReader.GameNetworkPing,
        isConnected = _dataFeed.NetworkManagerReader.IsConnected,
        connectionState = _dataFeed.NetworkManagerReader.ConnectionState,
        dataFeedErrorNetworkManager = _dataFeed.NetworkManagerReader.DataFeedErrorNetworkManager,
        currentFPS = _dataFeed.FPSReader.CurrentFPS,
        voiceCommsPing = _dataFeed.CommsReader.VoiceCommsPing,
        isVoiceConnected = _dataFeed.CommsReader.IsVoiceConnected,
        voiceConnectionState = _dataFeed.CommsReader.VoiceConnectionState,
        dataFeedErrorComms = _dataFeed.CommsReader.DataFeedErrorComms
      };

      GeneralHelper.DebugLog("Real-time data retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return realTimeData;
    }

    [Route(HttpVerbs.Get, "/")]
    public async Task GetApiV1()
    {
      AddCorsHeaders();
      var endpoints = new { endpoints = ApiConstants.availableRESTEndpoints };
      var json = JsonConvert.SerializeObject(endpoints);
      await HttpContext.SendStringAsync(json, "application/json", System.Text.Encoding.UTF8);
    }
  }
}
