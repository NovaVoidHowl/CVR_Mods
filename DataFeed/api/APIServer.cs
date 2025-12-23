using WebSocketSharp.Server;
using System.Reflection;
using EmbedIO;
using EmbedIO.WebApi;
using EmbedIO.Actions;
using Newtonsoft.Json;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;
using System.Threading.Tasks;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  // Custom module to add CORS headers to all responses
  public class CorsModule : WebModuleBase
  {
    public CorsModule(string baseRoute) : base(baseRoute) { }

    public override bool IsFinalHandler => false;

    protected override async Task OnRequestAsync(IHttpContext context)
    {
      // Add CORS headers to every request
      context.Response.Headers["Access-Control-Allow-Origin"] = "*";
      context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
      context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, X-API-Key, Authorization";
      context.Response.Headers["Access-Control-Max-Age"] = "86400";

      // Handle OPTIONS preflight - mark as handled and send empty response
      if (context.Request.HttpMethod == "OPTIONS")
      {
        context.Response.StatusCode = 204;
        context.SetHandled();
        await context.SendDataAsync(new byte[0]);
      }

      // For other methods, headers are added but let next module handle the request
    }
  }

  public class ApiServer
  {
    private WebSocketServer wssv;
    private WebServer httpServer;
    private readonly DataFeed dataFeed;
    private readonly ApiConfig config;

    public ApiServer(DataFeed dataFeed, ApiConfig config)
    {
      this.dataFeed = dataFeed;
      this.config = config;
      InitializeServers();
    }

    private void InitializeServers()
    {
      // Setup WebSocket server
      wssv = new WebSocketServer($"ws://localhost:{config.WebSocketPortInt}");
      #region WebSocket API v1
      // Create factory methods for all websocket services instead of reusing instances
      wssv.AddWebSocketService("/api/v1/parameters", () => new DataFeedWebSocketParametersV1(dataFeed));
      wssv.AddWebSocketService("/api/v1/instance", () => new DataFeedInstanceWebSocketV1(dataFeed));
      wssv.AddWebSocketService("/api/v1/avatar", () => new DataFeedAvatarWebSocketV1(dataFeed));
      wssv.AddWebSocketService("/api/v1/world", () => new DataFeedWorldWebSocketV1(dataFeed));
      wssv.AddWebSocketService("/api/v1/realtime", () => new DataFeedRealTimeWebSocketV1(dataFeed));

      // Subscribe to the events using the broadcast methods of WebSocketServer
      dataFeed.InstanceChanged += (sender, args) =>
      {
        var instanceData = dataFeed.GetCurrentInstanceData();
        var jsonData = JsonConvert.SerializeObject(instanceData);
        wssv.WebSocketServices["/api/v1/instance"]?.Sessions?.Broadcast(jsonData);
      };

      dataFeed.AvatarChanged += (sender, args) =>
      {
        var avatarData = dataFeed.GetCurrentAvatarData();
        var jsonData = JsonConvert.SerializeObject(avatarData);
        wssv.WebSocketServices["/api/v1/avatar"]?.Sessions?.Broadcast(jsonData);
      };

      #endregion // WebSocket API v1

      // Setup REST API server with multiple URL prefixes to support localhost, 127.0.0.1, and ::1
      httpServer = new WebServer(o => o
        .WithUrlPrefix($"http://localhost:{config.RestApiPortInt}/")
        .WithUrlPrefix($"http://127.0.0.1:{config.RestApiPortInt}/")
        .WithUrlPrefix($"http://[::1]:{config.RestApiPortInt}/")
        .WithMode(HttpListenerMode.Microsoft)
      )
        // Add CORS module FIRST to intercept all requests and add headers
        .WithModule(new CorsModule("/"))
        .WithWebApi("/api/v1", m => m.WithController<DataFeedControllerV1>(() => new DataFeedControllerV1(dataFeed)))
        .WithModule(
          new ActionModule(
            "/api",
            HttpVerbs.Any,
            async ctx =>
            {
              GeneralHelper.DebugLog($"Request received: {ctx.Request.Url.AbsolutePath}");
              var apiVersions = new { versions = new[] { "v1" } };
              var json = JsonConvert.SerializeObject(apiVersions);
              await ctx.SendStringAsync(json, ApiConstants.jsonContentType, System.Text.Encoding.UTF8);
            }
          )
        )
        .WithModule(
          new ActionModule(
            "/",
            HttpVerbs.Any,
            async ctx =>
            {
              GeneralHelper.DebugLog($"Request received: {ctx.Request.Url.AbsolutePath}");
              var assemblyInformationalVersion = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
              var info = new
              {
                DataFeedModVersion = assemblyInformationalVersion,
                DataFeedRestApiVersion = ApiConstants.RestApiVersion.ToString(),
                DataFeedWebSocketApiVersion = ApiConstants.WebSocketApiVersion.ToString(),
                ApiPath = "/api"
              };
              var json = JsonConvert.SerializeObject(info);
              await ctx.SendStringAsync(json, ApiConstants.jsonContentType, System.Text.Encoding.UTF8);
            }
          )
        );

      // Add global error handler for unhandled routes and exceptions
      httpServer.HandleHttpException(
        async (context, exception) =>
        {
          context.Response.StatusCode = exception.StatusCode;
          var errorResponse = new
          {
            status = exception.StatusCode,
            error = "Not Found",
            message = "The requested endpoint does not exist.",
            availableEndpoints = ApiConstants.availableRESTEndpoints
          };
          var json = JsonConvert.SerializeObject(errorResponse);
          context.Response.ContentType = ApiConstants.jsonContentType;
          await context.SendStringAsync(json, ApiConstants.jsonContentType, System.Text.Encoding.UTF8);
        }
      );

      httpServer.StateChanged += (s, e) =>
      {
        MelonLoader.MelonLogger.Msg($"Server state changed to: {e.NewState}");
        if (e.NewState == WebServerState.Listening)
        {
          MelonLoader.MelonLogger.Msg("API Endpoints:");
          MelonLoader.MelonLogger.Msg($"REST API: http://127.0.0.1:{config.RestApiPortInt}/api");
          MelonLoader.MelonLogger.Msg($"WebSocket: ws://127.0.0.1:{config.WebSocketPortInt}/api");
        }
      };
    }

    public void Start()
    {
      try
      {
        GeneralHelper.DebugLog("[DEBUG] Starting servers...");
        wssv.Start();
        GeneralHelper.DebugLog("[DEBUG] WebSocket server started");

        httpServer.Start();
        GeneralHelper.DebugLog("[DEBUG] HTTP server started");
      }
      catch (Exception ex)
      {
        MelonLoader.MelonLogger.Error($"Failed to start servers: {ex.Message}");
        throw;
      }
    }

    public void Stop()
    {
      try
      {
        wssv?.Stop();
        httpServer?.Dispose();
      }
      catch (Exception ex)
      {
        MelonLoader.MelonLogger.Error($"Error stopping API Server: {ex.Message}");
      }
    }
  }
}
