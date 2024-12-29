using WebSocketSharp;
using WebSocketSharp.Server;
using System.Text.Json;
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
      var apiKey = Context.Headers[ApiConstants.ApiKeyHeader];
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

    protected void SendJsonData(object data) => Send(JsonSerializer.Serialize(data));
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
          dataFeedDisabled = _dataFeed.DataFeedDisabled
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
          currentInstancePrivacy = _dataFeed.MetaPortReader.CurrentInstancePrivacy
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
        SendJsonData(new { currentPing = _dataFeed.MetaPortReader.CurrentPing });
        await Task.Delay(1000);
      }
    }
  }
}
