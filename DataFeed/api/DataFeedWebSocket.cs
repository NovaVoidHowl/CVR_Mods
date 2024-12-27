using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public class DataFeedWebSocketParametersV1 : WebSocketBehavior
  {
    private readonly DataFeed _dataFeed;

    public DataFeedWebSocketParametersV1(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
      _dataFeed.StateChanged += OnStateChanged;
    }

    protected override void OnOpen()
    {
      GeneralHelper.DebugLog("WebSocket connection attempt...");

      var headers = Context.Headers;
      var apiKey = headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        MelonLoader.MelonLogger.Error($"{ApiConstants.ApiKeyInvalidError}: {apiKey}");
        Context.WebSocket.Close(CloseStatusCode.PolicyViolation, ApiConstants.ApiKeyInvalidError);
        return;
      }

      GeneralHelper.DebugLog("WebSocket connection authenticated");
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
      switch (e.Data)
      {
        case "get_state":
          SendCurrentState();
          break;
      }
    }
  }

  public class DataFeedInstanceWebSocketV1 : WebSocketBehavior
  {
    private readonly DataFeed _dataFeed;

    public DataFeedInstanceWebSocketV1(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
    }

    protected override void OnOpen()
    {
      GeneralHelper.DebugLog("WebSocket connection attempt for instance data...");

      var headers = Context.Headers;
      var apiKey = headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        MelonLoader.MelonLogger.Error($"{ApiConstants.ApiKeyInvalidError}: {apiKey}");
        Context.WebSocket.Close(CloseStatusCode.PolicyViolation, ApiConstants.ApiKeyInvalidError);
        return;
      }

      GeneralHelper.DebugLog("WebSocket connection authenticated for instance data");
      SendInstanceInfo(); // Send initial instance info on connection
      base.OnOpen();
    }

    private void SendInstanceInfo()
    {
      var instanceInfo = new
      {
        currentInstanceId = _dataFeed.CurrentInstanceId,
        currentInstanceName = _dataFeed.CurrentInstanceName,
        currentWorldId = _dataFeed.CurrentWorldId,
        currentInstancePrivacy = _dataFeed.CurrentInstancePrivacy
      };
      Send(JsonSerializer.Serialize(instanceInfo));
    }

    public void NotifyClients()
    {
      if (Sessions != null)
      {
        SendInstanceInfo();
      }
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      switch (e.Data)
      {
        case "get_instance":
          SendInstanceInfo();
          break;
      }
    }
  }

  public class DataFeedAvatarWebSocketV1 : WebSocketBehavior
  {
    private readonly DataFeed _dataFeed;

    public DataFeedAvatarWebSocketV1(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
    }

    protected override void OnOpen()
    {
      GeneralHelper.DebugLog("WebSocket connection attempt for avatar data...");

      var headers = Context.Headers;
      var apiKey = headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        MelonLoader.MelonLogger.Error($"{ApiConstants.ApiKeyInvalidError}: {apiKey}");
        Context.WebSocket.Close(CloseStatusCode.PolicyViolation, ApiConstants.ApiKeyInvalidError);
        return;
      }

      GeneralHelper.DebugLog("WebSocket connection authenticated for avatar data");
      SendAvatarInfo(); // Send initial avatar info on connection
      base.OnOpen();
    }

    private void SendAvatarInfo()
    {
      var avatarInfo = new { currentAvatarId = _dataFeed.CurrentAvatarId };
      Send(JsonSerializer.Serialize(avatarInfo));
    }

    public void NotifyClients()
    {
      if (Sessions != null)
      {
        SendAvatarInfo();
      }
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      switch (e.Data)
      {
        case "get_avatar":
          SendAvatarInfo();
          break;
      }
    }
  }

  public class DataFeedRealTimeWebSocketV1 : WebSocketBehavior
  {
    private readonly DataFeed _dataFeed;
    private bool _isRunning = true;

    public DataFeedRealTimeWebSocketV1(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
    }

    protected override async void OnOpen()
    {
      GeneralHelper.DebugLog("WebSocket connection attempt for real-time data...");

      var headers = Context.Headers;
      var apiKey = headers[ApiConstants.ApiKeyHeader];
      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        MelonLoader.MelonLogger.Error($"{ApiConstants.ApiKeyInvalidError}: {apiKey}");
        Context.WebSocket.Close(CloseStatusCode.PolicyViolation, ApiConstants.ApiKeyInvalidError);
        return;
      }

      GeneralHelper.DebugLog("WebSocket connection authenticated for real-time data");
      base.OnOpen();
      await SendRealTimeData();
    }

    protected override void OnClose(CloseEventArgs e)
    {
      _isRunning = false;
      base.OnClose(e);
    }

    private async Task SendRealTimeData()
    {
      while (_isRunning)
      {
        var realTimeData = new { currentPing = _dataFeed.CurrentPing };
        Send(JsonSerializer.Serialize(realTimeData));
        await Task.Delay(1000); // Send update every second
      }
    }
  }
}
