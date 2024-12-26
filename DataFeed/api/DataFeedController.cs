using EmbedIO.WebApi;
using EmbedIO;
using EmbedIO.Routing;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public class DataFeedController : WebApiController
  {
    private readonly DataFeed _dataFeed;

    public DataFeedController(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
    }

    [Route(HttpVerbs.Get, "/state")]
    public object GetState()
    {
      MelonLoader.MelonLogger.Msg("[DEBUG] Processing request for state.");

      // Check for API Key in the request headers
      var apiKey = Request.Headers["X-API-Key"];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (string.IsNullOrEmpty(apiKey) || apiKey != configKey)
      {
#if DEBUG
        MelonLoader.MelonLogger.Error($"Invalid API Key: received '{apiKey}', expected '{configKey}'");
#endif
        return new { error = "Invalid API Key" };
      }

      var state = new
      {
        flyingAllowed = _dataFeed.FlyingAllowed,
        propsAllowed = _dataFeed.PropsAllowed,
        portalsAllowed = _dataFeed.PortalsAllowed,
        nameplatesEnabled = _dataFeed.NameplatesEnabled,
        dataFeedErrorBBCC = _dataFeed.DataFeedErrorBBCC,
        dataFeedErrorMetaPort = _dataFeed.DataFeedErrorMetaPort,
        dataFeedDisabled = _dataFeed.DataFeedDisabled
      };

      MelonLoader.MelonLogger.Msg("[DEBUG] State retrieved successfully.");
      Response.ContentType = "application/json";
      return state;
    }
  }
}
