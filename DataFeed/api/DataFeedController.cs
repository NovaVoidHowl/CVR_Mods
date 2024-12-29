using System.Text.Json;
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

    [Route(HttpVerbs.Get, "/parameters")]
    public object GetState()
    {
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
        currentInstancePrivacy = _dataFeed.MetaPortReader.CurrentInstancePrivacy
      };

      GeneralHelper.DebugLog("Instance info retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return instanceInfo;
    }

    [Route(HttpVerbs.Get, "/avatar")]
    public object GetAvatarInfo()
    {
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

    [Route(HttpVerbs.Get, "/realtime")]
    public object GetRealTimeData()
    {
      GeneralHelper.DebugLog("Processing request for real-time data.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        return new { error = ApiConstants.ApiKeyInvalidError };
      }

      var realTimeData = new { currentPing = _dataFeed.MetaPortReader.CurrentPing };

      GeneralHelper.DebugLog("Real-time data retrieved successfully.");
      Response.ContentType = ApiConstants.jsonContentType;
      return realTimeData;
    }

    [Route(HttpVerbs.Get, "/")]
    public async Task GetApiV1()
    {
      var endpoints = new { endpoints = ApiConstants.availableRESTEndpoints };
      var json = JsonSerializer.Serialize(endpoints);
      await HttpContext.SendStringAsync(json, "application/json", System.Text.Encoding.UTF8);
    }
  }
}
