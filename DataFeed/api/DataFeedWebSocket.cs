using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;
using uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public abstract class DataFeedWebSocketBase : WebSocketBehavior
  {
    protected readonly DataFeed _dataFeed;

    protected DataFeedWebSocketBase(DataFeed dataFeed)
    {
      _dataFeed = dataFeed;
    }

    protected override void OnOpen()
    {
      GeneralHelper.DebugLog($"WebSocket connection attempt for {GetConnectionType()}...");

      if (!ValidateApiKey())
      {
        return;
      }

      GeneralHelper.DebugLog($"WebSocket connection authenticated for {GetConnectionType()}");
      SendInitialData();
      base.OnOpen();
    }

    private bool ValidateApiKey()
    {
      // Try to get API key from headers first (for non-browser clients)
      var apiKey = Context.Headers[ApiConstants.ApiKeyHeader];

      // If not found in headers, try query string (for browser WebSocket connections)
      if (string.IsNullOrEmpty(apiKey))
      {
        var uri = Context.RequestUri;
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        apiKey = query["api-key"] ?? query["apikey"];
      }

      var configKey = _dataFeed.ApiConfig.ApiKey;

      if (!ApiHelper.IsValidApiKey(apiKey, configKey))
      {
        MelonLoader.MelonLogger.Error($"{ApiConstants.ApiKeyInvalidError}: {apiKey}");
        Context.WebSocket.Close(CloseStatusCode.PolicyViolation, ApiConstants.ApiKeyInvalidError);
        return false;
      }
      return true;
    }

    protected abstract string GetConnectionType();
    protected abstract void SendInitialData();

    protected void SendJsonData(object data) => Send(JsonConvert.SerializeObject(data));
  }

  public class DataFeedWebSocketParametersV1 : DataFeedWebSocketBase
  {
    public DataFeedWebSocketParametersV1(DataFeed dataFeed)
      : base(dataFeed)
    {
      _dataFeed.StateChanged += OnStateChanged;
    }

    protected override string GetConnectionType() => "parameters";

    protected override void SendInitialData() => SendCurrentState();

    protected override void OnClose(CloseEventArgs e)
    {
      _dataFeed.StateChanged -= OnStateChanged;
      base.OnClose(e);
    }

    private void OnStateChanged(object sender, System.EventArgs e) => SendCurrentState();

    private void SendCurrentState()
    {
      SendJsonData(
        new
        {
          flyingAllowed = _dataFeed.BBCCReader.FlyingAllowed,
          propsAllowed = _dataFeed.MetaPortReader.PropsAllowed,
          portalsAllowed = _dataFeed.MetaPortReader.PortalsAllowed,
          nameplatesEnabled = _dataFeed.MetaPortReader.NameplatesEnabled,
          dataFeedErrorBBCC = _dataFeed.BBCCReader.DataFeedErrorBBCC,
          dataFeedErrorMetaPort = _dataFeed.MetaPortReader.DataFeedErrorMetaPort,
          dataFeedDisabled = _dataFeed.DataFeedDisabled,
          dataFeedAPIDisabled = !_dataFeed.meAPIEnable.Value
        }
      );
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      if (e.Data == "get_state")
        SendCurrentState();
    }
  }

  public class DataFeedInstanceWebSocketV1 : DataFeedWebSocketBase
  {
    public DataFeedInstanceWebSocketV1(DataFeed dataFeed)
      : base(dataFeed) { }

    protected override string GetConnectionType() => "instance data";

    protected override void SendInitialData() => SendInstanceInfo();

    private void SendInstanceInfo()
    {
      SendJsonData(
        new
        {
          currentInstanceId = _dataFeed.MetaPortReader.CurrentInstanceId,
          currentInstanceName = _dataFeed.MetaPortReader.CurrentInstanceName,
          currentWorldId = _dataFeed.MetaPortReader.CurrentWorldId,
          currentInstancePrivacy = _dataFeed.MetaPortReader.CurrentInstancePrivacy,
          worldDetails = _dataFeed.CurrentWorldDetails ?? new WorldAbiApiInfo(),
          detailsAvailable = _dataFeed.CurrentWorldDetails != null
        }
      );
    }

    public void NotifyClients()
    {
      if (Sessions != null)
        SendInstanceInfo();
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      if (e.Data == "get_instance")
        SendInstanceInfo();
    }
  }

  public class DataFeedAvatarWebSocketV1 : DataFeedWebSocketBase
  {
    public DataFeedAvatarWebSocketV1(DataFeed dataFeed)
      : base(dataFeed) { }

    protected override string GetConnectionType() => "avatar data";

    protected override void SendInitialData() => SendAvatarInfo();

    private void SendAvatarInfo()
    {
      SendJsonData(
        new
        {
          currentAvatarId = _dataFeed.CurrentAvatarId,
          avatarDetails = _dataFeed.CurrentAvatarDetails ?? new AvatarAbiApiInfo(),
          detailsAvailable = _dataFeed.CurrentAvatarDetails != null
        }
      );
    }

    public void NotifyClients()
    {
      if (Sessions != null)
        SendAvatarInfo();
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      if (e.Data == "get_avatar")
        SendAvatarInfo();
    }
  }

  public class DataFeedRealTimeWebSocketV1 : DataFeedWebSocketBase
  {
    private bool _isRunning = true;

    public DataFeedRealTimeWebSocketV1(DataFeed dataFeed)
      : base(dataFeed) { }

    protected override string GetConnectionType() => "real-time data";

    protected override void SendInitialData() => _ = SendRealTimeData();

    protected override void OnClose(CloseEventArgs e)
    {
      _isRunning = false;
      base.OnClose(e);
    }

    private async Task SendRealTimeData()
    {
      while (_isRunning)
      {
        SendJsonData(
          new
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
          }
        );
        await Task.Delay(1000);
      }
    }
  }

  public class DataFeedWorldWebSocketV1 : DataFeedWebSocketBase
  {
    public DataFeedWorldWebSocketV1(DataFeed dataFeed)
      : base(dataFeed) { }

    protected override string GetConnectionType() => "world";

    protected override void SendInitialData() => SendCurrentWorldData();

    private void SendCurrentWorldData()
    {
      SendJsonData(
        new
        {
          currentWorldId = _dataFeed.CurrentWorldId,
          worldDetails = _dataFeed.CurrentWorldDetails ?? new WorldAbiApiInfo(),
          detailsAvailable = _dataFeed.CurrentWorldDetails != null
        }
      );
    }

    protected override void OnMessage(MessageEventArgs e)
    {
      if (e.Data == "get_world")
        SendCurrentWorldData();
    }
  }
}
