using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public class DataFeedWebSocket : WebSocketBehavior
  {
    private readonly DataFeed _dataFeed;

    public DataFeedWebSocket(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
      _dataFeed.StateChanged += OnStateChanged;
    }

    protected override void OnOpen()
    {
      MelonLoader.MelonLogger.Msg("[DEBUG] WebSocket connection attempt...");

      var headers = Context.Headers;
      var apiKey = headers["X-API-Key"];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (string.IsNullOrEmpty(apiKey) || apiKey != configKey)
      {
        MelonLoader.MelonLogger.Error($"Invalid WebSocket API Key: {apiKey}");
        Context.WebSocket.Close(CloseStatusCode.PolicyViolation, "Invalid API Key");
        return;
      }

      MelonLoader.MelonLogger.Msg("[DEBUG] WebSocket connection authenticated");
      SendCurrentState(); // Send initial state on connection
      base.OnOpen();
    }

    protected override void OnClose(CloseEventArgs e)
    {
      _dataFeed.StateChanged -= OnStateChanged;
      base.OnClose(e);
    }

    private void OnStateChanged(object sender, System.EventArgs e)
    {
      SendCurrentState();
    }

    private void SendCurrentState()
    {
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
      Send(JsonSerializer.Serialize(state));
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      if (e.Data == "get_state")
      {
        SendCurrentState();
      }
    }
  }
}
