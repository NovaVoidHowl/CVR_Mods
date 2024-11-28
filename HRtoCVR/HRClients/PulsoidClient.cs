using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using Valve.Newtonsoft.Json;
using System.Timers;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients
{
  public class PulsoidClient : IDisposable
  {
    // Base URI for Pulsoid API. This is hardcoded because it is unlikely to change.
#pragma warning disable S1075
    private const string PulsoidBaseUri = "https://dev.pulsoid.net/api/v1";
#pragma warning restore S1075
    private const string PulsoidKeyValidationPath = "/token/validate";
    public const string PulsoidClientVersion = "0.1.6";

    private const int HRDataTimeout = 4000; // 4 seconds with no HR data will reset values and mark as not active
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    private System.Timers.Timer _heartBeatTimer;
    private System.Timers.Timer _messageTimeoutTimer;
    private bool _disposed = false;

    public int HR { get; set; }
    public bool isHRConnected { get; set; }
    public bool isHRActive { get; set; }
    public bool isHRBeat { get; set; }
    public float HRPercent { get; set; }
    public int onesHR { get; set; }
    public int tensHR { get; set; }
    public int hundredsHR { get; set; }
    public bool verboseLogging { get; set; }
    public event Action OnHeartRateUpdated;
    public event Action OnHeartRateRapidUpdated;

    public static Task<bool> CheckValidationSupportLibs(bool noOKLog = false)
    {
      if (!noOKLog)
      {
        MelonLogger.Msg("Checking validation support libraries...");
      }

      try
      {
        // Check if the Valve.Newtonsoft.Json library is loaded/available, note test is meant to be an unused variable
#pragma warning disable S1481
        var test = Valve.Newtonsoft.Json.JsonConvert.SerializeObject(new { test = "test" });
#pragma warning restore S1481
        if (!noOKLog)
        {
          MelonLogger.Msg("Valve.Newtonsoft.Json library loaded successfully.");
        }
        return Task.FromResult(true);
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error loading Valve.Newtonsoft.Json library: {ex.Message}");
        MelonLogger.Error($"Exception Type: {ex.GetType().FullName}");
        MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
        return Task.FromResult(false);
      }
    }

    public static Task<bool> ValidatePulsoidKeyLocal(string key)
    {
      MelonLogger.Msg("Starting Pulsoid key validation...");

      if (string.IsNullOrEmpty(key))
      {
        MelonLogger.Error($"Error validating Pulsoid key: Key value empty.");
        return Task.FromResult(false);
      }
      if (key == "XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX")
      {
        MelonLogger.Error($"Error validating Pulsoid key: Key set to placeholder value.");
        return Task.FromResult(false);
      }

#if DEBUG
      MelonLogger.Msg("Local Pulsoid key validation successful.");
#endif
      return Task.FromResult(true);
    }

    public static async Task<bool> ValidatePulsoidKeyAPI(string key)
    {
#if DEBUG
      MelonLogger.Msg("Starting Pulsoid key validation with API...");
#endif
      // call the API tester to check that the validation endpoint is reachable
      // Define the expected response codes
      var expectedResponseCodes = new List<System.Net.HttpStatusCode>
      {
        System.Net.HttpStatusCode.OK,
        System.Net.HttpStatusCode.Forbidden // 403
      };

      // Call the API tester to check that the validation endpoint is reachable
      bool isKeyValidationEndpointReachable = await APITester.IsApiReachable(
        PulsoidBaseUri + PulsoidKeyValidationPath,
        expectedResponseCodes,
        true
      );

      if (!isKeyValidationEndpointReachable)
      {
        MelonLogger.Error("Pulsoid key validation failed: API endpoint not reachable.");
        return false;
      }
      else
      {
        MelonLogger.Msg("Pulsoid key validation endpoint reachable.");
      }

      // Call the API key validation endpoint
      HttpResponseMessage response = await CallAPIKeyValidateEndpoint(key);

      if (response == null)
      {
        MelonLogger.Error("Pulsoid key validation failed: API response is null.");
        return false;
      }

      return await CheckTokenAPIValidationResponse(response);
    }

    public static async Task<bool> CheckTokenAPIValidationResponse(HttpResponseMessage message)
    {
      string responseContent = await message.Content.ReadAsStringAsync();
#if DEBUG
      // debug print the response content so we can see what we're working with
      MelonLogger.Msg($"Pulsoid key validation response: {responseContent}");
#endif

      if (!message.IsSuccessStatusCode)
      {
        MelonLogger.Error($"Pulsoid key validation API response not OK,\n status code: {message.StatusCode}");
        return false;
      }

#if DEBUG
      MelonLogger.Msg($"Pulsoid key validation API response status OK");
#endif
      // If the status code is successful, log the success and return true
      try
      {
        PulsoidResponse jsonResponse = JsonConvert.DeserializeObject<PulsoidResponse>(responseContent);
        string clientId = jsonResponse.client_id;
        int expiresIn = jsonResponse.expires_in;

        // Convert expires_in to DateTime
        DateTime expiryDate = DateTime.UtcNow.AddSeconds(expiresIn);
        string formattedExpiryDate = expiryDate.ToString("yyyy-MM-dd hh:mm tt");
#if DEBUG
        MelonLogger.Msg($"Pulsoid key validation successful. Client ID: {clientId}, Expires In: {formattedExpiryDate}");
#else
        MelonLogger.Msg($"Pulsoid key validation successful. Expires In: {formattedExpiryDate}");
#endif
        return true;
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error parsing JSON response: {ex.Message}");
        MelonLogger.Error($"Exception Type: {ex.GetType().FullName}");
        MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
        return false;
      }

      // note API response json format on success:
      //
      // {"token":"XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
      // "client_id":"XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
      // "expires_in":555827113,
      // "profile_id":"XXXXXXXXXXXXXXXXXXXXXXXX",
      // "scopes":["data:heart_rate:read","data:heart_rate:write","profile:read","news:read","profile:create"]}
    }

    public static async Task<HttpResponseMessage> CallAPIKeyValidateEndpoint(string key)
    {
      using (HttpClient client = new HttpClient())
      {
        client.Timeout = TimeSpan.FromSeconds(10); // Set a timeout for the request
        try
        {
          var request = new HttpRequestMessage(HttpMethod.Get, PulsoidBaseUri + PulsoidKeyValidationPath);
          request.Headers.Add("Authorization", $"Bearer {key}");
          HttpResponseMessage response = await client.SendAsync(request);
          return response;
        }
        catch (Exception ex)
        {
          MelonLogger.Error($"Error creating request to Pulsoid API: {ex.Message}");
          MelonLogger.Error($"Exception Type: {ex.GetType().FullName}");
          MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
          return null;
        }
      }
    }

    public async Task InitializeWebSocket(string key, int minHR, int maxHR)
    {
      MelonLogger.Msg($"PulsoidClient version: {PulsoidClientVersion}");
      bool validationSupportLibsOK = false;
      try
      {
        validationSupportLibsOK = await CheckValidationSupportLibs(noOKLog: !verboseLogging);
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error checking validation support binaries: {ex.Message}");
        return;
      }
      if (verboseLogging)
      {
        MelonLogger.Msg($"Validation support libraries loaded: {validationSupportLibsOK}");
        MelonLogger.Msg("Initializing WebSocket with Pulsoid key.");
      }

      if (string.IsNullOrEmpty(key))
      {
        MelonLogger.Error("Pulsoid key is empty.");
        return;
      }

      bool isApiReachable = await APITester.IsApiReachable(PulsoidBaseUri, noOKLog: !verboseLogging);
      if (!isApiReachable)
      {
        MelonLogger.Error("Pulsoid API is not reachable.");
        return;
      }

      bool isValidKey = false;
#if DEBUG
      MelonLogger.Msg("Calling ValidatePulsoidKeyLocal...");
#endif
      if (await ValidatePulsoidKeyLocal(key))
      {
        MelonLogger.Msg("Pulsoid key validated locally. Now calling Pulsoid API to test key ...");

        isValidKey = await ValidatePulsoidKeyAPI(key);
      }
      else
      {
        MelonLogger.Error("Pulsoid key validation failed locally.");
        return;
      }

      if (!isValidKey)
      {
        MelonLogger.Error("Invalid Pulsoid key.");
        return;
      }

      // Replace https:// with wss:// in the PulsoidBaseUri
      string PulsoidBaseWebsocketUri = PulsoidBaseUri.Replace("https://", "wss://");

      string url = $"{PulsoidBaseWebsocketUri}/data/real_time?access_token={key}";
      _webSocket = new ClientWebSocket();
      _cancellationTokenSource = new CancellationTokenSource();

      try
      {
        MelonLogger.Msg("Connecting to WebSocket...");
        await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);
        MelonLogger.Msg("WebSocket connected successfully.");
        InitializeMessageTimeoutTimer(); // Initialize the message timeout timer
        await ReceiveWebSocketMessages(key, minHR, maxHR);
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"WebSocket connection error: {ex.Message}");
        isHRConnected = false;
        isHRActive = false;
        isHRBeat = false;
        OnHeartRateUpdated?.Invoke();
      }
    }

    private async Task ReceiveWebSocketMessages(string key, int minHR, int maxHR)
    {
      if (verboseLogging)
      {
        MelonLogger.Msg("Receiving WebSocket messages...");
      }

      var buffer = new byte[1024 * 4];

      while (_webSocket.State == WebSocketState.Open)
      {
        if (verboseLogging)
        {
          MelonLogger.Msg($"websocket loop active ....");
        }
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
          HR = 0;
          onesHR = 0;
          tensHR = 0;
          hundredsHR = 0;
          HRPercent = (float)0;
          OnHeartRateUpdated?.Invoke();
          MelonLogger.Msg("WebSocket connection closed. Attempting to reconnect...");
          await Task.Delay(5000);
          await InitializeWebSocket(key, minHR, maxHR); // Retry with backoff
        }
        else
        {
          if (verboseLogging)
          {
            // verbose logging only on this or it will spam the log
            MelonLogger.Msg("WebSocket message received.");
          }
          string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
          ProcessWebSocketMessage(message, minHR, maxHR);
          ResetMessageTimeoutTimer(); // Reset the message timeout timer
        }
      }
    }

    private void ProcessWebSocketMessage(string message, int minHR, int maxHR)
    {
      // Verbose logging
      if (verboseLogging)
      {
        MelonLogger.Msg($"Processing WebSocket message: {message}");
      }

      try
      {
        WebSocketMessage data = JsonConvert.DeserializeObject<WebSocketMessage>(message);
        HR = data.data.heart_rate;
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

        // Verbose logging
        if (verboseLogging)
        {
          MelonLogger.Msg($"WebSocket message processed. HR: {HR}, HRPercent: {HRPercent}");
        }
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error processing WebSocket message: {ex.Message}");
      }
    }

    private void InitializeMessageTimeoutTimer()
    {
      _messageTimeoutTimer = new System.Timers.Timer(HRDataTimeout); // Set timeout to 4 seconds
      _messageTimeoutTimer.Elapsed += OnMessageTimeout;
      _messageTimeoutTimer.AutoReset = false; // Only trigger once
      _messageTimeoutTimer.Enabled = true;
    }

    private void ResetMessageTimeoutTimer()
    {
      _messageTimeoutTimer.Stop();
      _messageTimeoutTimer.Start();
    }

    private void OnMessageTimeout(object sender, ElapsedEventArgs e)
    {
      MelonLogger.Msg("WebSocket message timeout. Resetting values.");
      ResetHRValuesToDefault();
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
      OnHeartRateRapidUpdated?.Invoke();
    }

    private async Task CloseWebSocketAsync()
    {
      if (_webSocket != null && _webSocket.State == WebSocketState.Open)
      {
        try
        {
          await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
          MelonLogger.Msg("WebSocket closed successfully.");
        }
        catch (Exception ex)
        {
          MelonLogger.Error($"Error closing WebSocket: {ex.Message}");
        }
      }
    }

    private void ResetHRValuesToDefault()
    {
      HR = 0;
      onesHR = 0;
      tensHR = 0;
      hundredsHR = 0;
      HRPercent = 0;
      isHRActive = false;
      isHRBeat = false;
      OnHeartRateUpdated?.Invoke();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          _heartBeatTimer?.Stop();
          _heartBeatTimer?.Dispose();
          _messageTimeoutTimer?.Stop();
          _messageTimeoutTimer?.Dispose();
#pragma warning disable S6966
          // there is no such thing as CancelAsync for CancellationTokenSource
          _cancellationTokenSource?.Cancel();
#pragma warning restore S6966
          _cancellationTokenSource?.Dispose();
          CloseWebSocketAsync().GetAwaiter().GetResult();
          _webSocket?.Dispose();
          ResetHRValuesToDefault();
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

  #region supporting classes

  public class PulsoidResponse
  {
    public string token { get; set; }
    public string client_id { get; set; }
    public int expires_in { get; set; }
    public string profile_id { get; set; }
    public List<string> scopes { get; set; }
  }

  public class HeartRateData
  {
    public int heart_rate { get; set; }
  }

  public class WebSocketMessage
  {
    public long measured_at { get; set; }
    public HeartRateData data { get; set; }
  }

  #endregion //supporting classes
}