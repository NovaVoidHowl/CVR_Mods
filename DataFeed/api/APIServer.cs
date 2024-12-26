using WebSocketSharp.Server;
using EmbedIO;
using EmbedIO.WebApi;
using EmbedIO.Actions;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
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
      wssv = new WebSocketServer($"ws://127.0.0.1:{config.WebSocketPortInt}");
      wssv.AddWebSocketService("/DataFeed", () => new DataFeedWebSocket(dataFeed));

      // Setup REST API server with explicit route configuration
      httpServer = new WebServer(
        o => o.WithUrlPrefix($"http://127.0.0.1:{config.RestApiPortInt}").WithMode(HttpListenerMode.Microsoft)
      )
        .WithWebApi("/api", m => m.WithController<DataFeedController>(() => new DataFeedController(dataFeed)))
        .WithModule(
          new ActionModule(
            "/",
            HttpVerbs.Any,
            async ctx =>
            {
              MelonLoader.MelonLogger.Msg($"[DEBUG] Request received: {ctx.Request.Url.AbsolutePath}");
              await ctx.SendStringAsync(
                "DataFeed API Server - Use /api/state endpoint",
                "text/plain",
                System.Text.Encoding.UTF8
              );
            }
          )
        );

      httpServer.StateChanged += (s, e) =>
      {
        MelonLoader.MelonLogger.Msg($"Server state changed to: {e.NewState}");
        if (e.NewState == WebServerState.Listening)
        {
          MelonLoader.MelonLogger.Msg("API Endpoints:");
          MelonLoader.MelonLogger.Msg($"REST API: http://127.0.0.1:{config.RestApiPortInt}/api/state");
          MelonLoader.MelonLogger.Msg($"WebSocket: ws://127.0.0.1:{config.WebSocketPortInt}/DataFeed");
        }
      };
    }

    public void Start()
    {
      try
      {
        MelonLoader.MelonLogger.Msg("[DEBUG] Starting servers...");
        wssv.Start();
        MelonLoader.MelonLogger.Msg("[DEBUG] WebSocket server started");

        httpServer.Start();
        MelonLoader.MelonLogger.Msg("[DEBUG] HTTP server started");
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
