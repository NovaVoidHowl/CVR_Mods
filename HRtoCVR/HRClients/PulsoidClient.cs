using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients
{
  public class PulsoidClient : IDisposable
  {
    // Base URI for Pulsoid API. This is hardcoded because it is unlikely to change.
#pragma warning disable S1075
    private const string PulsoidBaseUri = "https://dev.pulsoid.net/api/v1";
#pragma warning restore S1075
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    private System.Timers.Timer _heartBeatTimer;
    private bool _disposed = false;

    public int HR { get; set; }
    public bool isHRConnected { get; set; }
    public bool isHRActive { get; set; }
    public bool isHRBeat { get; set; }
    public float HRPercent { get; set; }
    public int onesHR { get; set; }
    public int tensHR { get; set; }
    public int hundredsHR { get; set; }
    public event Action OnHeartRateUpdated;

    public static async Task<bool> ValidatePulsoidKey(string key)
    {
      if (string.IsNullOrEmpty(key))
      {
        MelonLogger.Error($"Error validating Pulsoid key: Key value empty.");
        return false;
      }
      if (key == "XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX")
      {
        MelonLogger.Error($"Error validating Pulsoid key: Key set to placeholder value .");
        return false;
      }

      try
      {
        using (HttpClient client = new HttpClient())
        {
          client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
          HttpResponseMessage response = await client.GetAsync($"{PulsoidBaseUri}/token/validate");
          response.EnsureSuccessStatusCode();
          string responseBody = await response.Content.ReadAsStringAsync();

          dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
          if (data != null && data.expires_in > 0)
          {
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error validating Pulsoid key: {ex.Message}");
      }

      return false;
    }

    public async Task InitializeWebSocket(string key, int minHR, int maxHR)
    {
      if (string.IsNullOrEmpty(key) || !await ValidatePulsoidKey(key))
      {
        MelonLogger.Error("Invalid Pulsoid key.");
        return;
      }

      string url = $"{PulsoidBaseUri}/data/real_time?access_token={key}";
      _webSocket = new ClientWebSocket();
      _cancellationTokenSource = new CancellationTokenSource();

      try
      {
        await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);
        await ReceiveWebSocketMessages(key, minHR, maxHR);
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"WebSocket connection error: {ex.Message}");
        isHRConnected = false;
        isHRActive = false;
        isHRBeat = false;
      }
    }

    private async Task ReceiveWebSocketMessages(string key, int minHR, int maxHR)
    {
      var buffer = new byte[1024 * 4];

      while (_webSocket.State == WebSocketState.Open)
      {
        WebSocketReceiveResult result = await _webSocket.ReceiveAsync(
          new ArraySegment<byte>(buffer),
          _cancellationTokenSource.Token
        );
        if (result.MessageType == WebSocketMessageType.Close)
        {
          await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, _cancellationTokenSource.Token);
          isHRConnected = false;
          isHRActive = false;
          isHRBeat = false;
          MelonLogger.Msg("WebSocket connection closed. Attempting to reconnect...");
          await Task.Delay(5000);
          await InitializeWebSocket(key, minHR, maxHR); // Retry with backoff
        }
        else
        {
          string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
          ProcessWebSocketMessage(message, minHR, maxHR);
        }
      }
    }

    private void ProcessWebSocketMessage(string message, int minHR, int maxHR)
    {
      try
      {
        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(message);
        HR = (int)data.data.heart_rate;
        onesHR = HR % 10;
        tensHR = (HR / 10) % 10;
        hundredsHR = (HR / 100) % 10;
        HRPercent = (float)(HR - minHR) / (maxHR - minHR);
        isHRConnected = true;
        isHRActive = true;

        // Update the heart beat timer interval based on HR
        _heartBeatTimer.Interval = 60000.0 / HR; // Interval in milliseconds for each beat

        // Notify that heart rate data has been updated
        OnHeartRateUpdated?.Invoke();
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error processing WebSocket message: {ex.Message}");
      }
    }

    public void InitializeHeartBeatTimer()
    {
      _heartBeatTimer = new System.Timers.Timer();
      _heartBeatTimer.Elapsed += (sender, e) => ToggleHeartBeat();
      _heartBeatTimer.AutoReset = true;
      _heartBeatTimer.Enabled = true;
    }

    private void ToggleHeartBeat()
    {
      isHRBeat = !isHRBeat;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          _heartBeatTimer?.Stop();
          _heartBeatTimer?.Dispose();
          _cancellationTokenSource?.Cancel();
          _cancellationTokenSource?.Dispose();
          _webSocket?.Dispose();
        }

        _disposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~PulsoidClient()
    {
      Dispose(false);
    }
  }
}
