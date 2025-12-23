using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI.CCK.Components;
using ABI_RC.Systems.Movement;
using MelonLoader;
using Microsoft.CSharp.RuntimeBinder;
using Valve.Newtonsoft.Json;

namespace uk.novavoidhowl.dev.cvrmods.THtoCVR
{
#pragma warning disable S101
  public class THtoCVR : MelonMod
#pragma warning restore S101
  {
    public enum HRConnectionType
    {
      Pulsoid,
      Simulated,
      TextFile
    }

    public const string CoreVersion = "0.1.4";

    public const string jsonErrorMissingField = "Config file contains invalid or missing fields.";

    // Melon Loader vars should stay public
#pragma warning disable S1104
    // Core Mellon Loader Vars
    public MelonPreferences_Entry<bool> meEnable;
    public MelonPreferences_Entry<bool> meVerboseLogging;
    public MelonPreferences_Entry<bool> meVerboseParametersLogging;
    public MelonPreferences_Entry<string> meConfigFileLocation;
#pragma warning restore S1104

    // THtoCVR vars
    private bool THtoCVRDisabled; // Returns whether the mod's data feed is disabled or not
    private CancellationTokenSource cancellationTokenSource; // Token source for cancelling tasks

    // On Melon Load
    public override void OnInitializeMelon()
    {
#if DEBUG
      MelonLogger.Error(
        "This mod was compiled in DEBUG mode log spam possible," + " do not use in production environment"
      );
#endif

      // Melon Config
      var melonCategoryTHtoCVR = MelonPreferences.CreateCategory(nameof(THtoCVR));
      #region  Core Mod Options
      meEnable = melonCategoryTHtoCVR.CreateEntry(
        "Enable",
        true,
        description: "Enables or Disables the data feed, note values will default to true when disabled."
      );
      meVerboseLogging = melonCategoryTHtoCVR.CreateEntry(
        "Verbose Logging",
        false,
        description: "Enables or Disables verbose logging."
      );
      meVerboseParametersLogging = melonCategoryTHtoCVR.CreateEntry(
        "Verbose Parameters Logging",
        false,
        description: "Enables or Disables verbose logging of the avatar parameter feed values."
      );

      // text file Specific Options
      meConfigFileLocation = melonCategoryTHtoCVR.CreateEntry(
        "Config File Location",
        "C:\\path\\to\\non_existing_file.txt",
        description: "Location of the config file to read from."
      );

      #endregion // Core Mod Options

      // Force save preferences to file so they appear in MelonPreferences.cfg immediately
      MelonPreferences.Save();

      #region  Event Listeners MelonLoader
      meEnable.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          if (newValue)
          {
            MelonLogger.Msg("THtoCVR Enabled");
            InitializeTHClient();
            THtoCVRDisabled = false;
            SendTHtoCVRDisabledToAnimator();
          }
          else
          {
            MelonLogger.Msg("THtoCVR Disabled");
            Cleanup();
            THtoCVRDisabled = true;
            SendTHtoCVRDisabledToAnimator();
          }
        }
      );
      meVerboseLogging.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          MelonLogger.Msg("Verbose Logging Changed: " + newValue);
          if (meEnable.Value)
          {
            // reinitialize the client, to pick up logging changes
            Cleanup();
            InitializeTHClient();
          }
        }
      );
      meVerboseParametersLogging.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          MelonLogger.Msg("Verbose Parameters Logging Changed: " + newValue);
          if (meEnable.Value)
          {
            // reinitialize the client, to pick up logging changes
            Cleanup();
            InitializeTHClient();
          }
        }
      );
      meConfigFileLocation.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          MelonLogger.Msg("Config Text File Path Changed");
          if (meEnable.Value)
          {
            InitializeTHClient();
          }
        }
      );
      #endregion // Event Listeners MelonLoader

      #region  Event Listeners CVR,
      // Note: more of these these can be found in the CVRGameEventSystem class under the
      // ABI_RC.Systems.GameEventSystem namespace if needed

      CVRGameEventSystem.Initialization.OnPlayerSetupStart.AddListener(() =>
      {
        if (!meEnable.Value)
        {
          return;
        }
        MelonLogger.Msg("Player Setup Start");
      });

      CVRGameEventSystem.Instance.OnConnected.AddListener(
        (string message) =>
        {
          if (!meEnable.Value)
          {
            return;
          }
          MelonLogger.Msg("Instance `" + message + "` Connected, reloading THtoCVR data source.");
          InitializeTHClient();
          SendTHtoCVRDisabledToAnimator();
        }
      );

      CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(
        (CVRAvatar avatar) =>
        {
          if (!meEnable.Value)
          {
            return;
          }
          // get the game object of the avatar
          MelonLogger.Msg("On Local Avatar Load: " + avatar.gameObject.name);
          InitializeTHClient();
          SendTHtoCVRDisabledToAnimator();
        }
      );

      #endregion // Event Listeners CVR
      InitializeTHClient();
    }

    private void InitializeTHClient()
    {
      MelonLogger.Msg("Initializing TH Client...");

      if (!meEnable.Value)
      {
        MelonLogger.Msg("THtoCVR is not enabled.");
        return;
      }

      // Cancel existing tasks if any
      if (cancellationTokenSource != null)
      {
        MelonLogger.Msg("Cancelling existing tasks...");
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
      }

      // check if the config file exists
      if (!File.Exists(meConfigFileLocation.Value))
      {
        MelonLogger.Error("Config file not found at: " + meConfigFileLocation.Value);
        return;
      }

      // read the config file
      var configLines = File.ReadAllLines(meConfigFileLocation.Value);

      // check if the config file is empty
      if (configLines.Length == 0)
      {
        MelonLogger.Error("Config file is empty: " + meConfigFileLocation.Value);
        return;
      }

      // Log the content of the config file
      VerboseMelonLogger.Msg("Config file content:", meVerboseLogging.Value);
      foreach (var line in configLines)
      {
        VerboseMelonLogger.Msg(line, meVerboseLogging.Value);
      }

      // check if the config file is valid json and matches the expected structure
      try
      {
        var configJson = JsonConvert.DeserializeObject<Config>(string.Join(Environment.NewLine, configLines));
        if (configJson == null || configJson.Connections == null || configJson.Connections.Count == 0)
        {
          throw new JsonReaderException("Config file does not match the expected structure.");
        }

        bool configModified = ValidateAndFixConnections(configJson.Connections);

        // Save the corrected config if modifications were made
        if (configModified)
        {
          try
          {
            var correctedJson = JsonConvert.SerializeObject(configJson, Formatting.Indented);
            File.WriteAllText(meConfigFileLocation.Value, correctedJson);
            MelonLogger.Msg("Config file updated with corrected endpoint paths.");
          }
          catch (Exception ex)
          {
            MelonLogger.Error($"Failed to save corrected config: {ex.Message}");
          }
        }

        // Initialize the cancellation token source
        cancellationTokenSource = new CancellationTokenSource();

        // Spawn tasks for each connection
        foreach (var connection in configJson.Connections)
        {
          Task.Run(
            () =>
              HandleConnectionAsync(
                connection,
                cancellationTokenSource.Token,
                meVerboseLogging.Value,
                meVerboseParametersLogging.Value
              )
          );
        }
      }
      catch (JsonReaderException e)
      {
        MelonLogger.Error(
          "Config file is not valid or does not match the expected structure: " + meConfigFileLocation.Value
        );
        MelonLogger.Error(e.Message);
      }
    }

    private static bool ValidateAndFixConnections(List<Connection> connections)
    {
      bool configModified = false;

      foreach (var connection in connections)
      {
        ValidateConnection(connection);
        configModified |= AutoFixConnectionEndpoints(connection);
      }

      return configModified;
    }

    private static void ValidateConnection(Connection connection)
    {
      if (string.IsNullOrEmpty(connection.BaseUrl))
      {
        MelonLogger.Error("BaseUrl is missing or empty.");
        throw new JsonReaderException(jsonErrorMissingField);
      }

      if (connection.TemperatureEndpointEnable && string.IsNullOrEmpty(connection.TemperatureEndpoint))
      {
        MelonLogger.Error("TemperatureEndpoint is enabled but missing or empty.");
        throw new JsonReaderException(jsonErrorMissingField);
      }

      if (connection.HumidityEndpointEnable && string.IsNullOrEmpty(connection.HumidityEndpoint))
      {
        MelonLogger.Error("HumidityEndpoint is enabled but missing or empty.");
        throw new JsonReaderException(jsonErrorMissingField);
      }

      if (string.IsNullOrEmpty(connection.TemperatureEndpointAnimatorParameter))
      {
        MelonLogger.Error("TemperatureEndpointAnimatorParameter is missing or empty.");
        throw new JsonReaderException(jsonErrorMissingField);
      }

      if (string.IsNullOrEmpty(connection.HumidityEndpointAnimatorParameter))
      {
        MelonLogger.Error("HumidityEndpointAnimatorParameter is missing or empty.");
        throw new JsonReaderException(jsonErrorMissingField);
      }

      if (connection.PolingRate <= 0)
      {
        MelonLogger.Error("PolingRate is invalid or less than or equal to 0.");
        throw new JsonReaderException(jsonErrorMissingField);
      }
    }

    private static bool AutoFixConnectionEndpoints(Connection connection)
    {
      bool modified = false;

      // Auto-fix: Add trailing slash to BaseUrl if missing
      if (!connection.BaseUrl.EndsWith('/'))
      {
        MelonLogger.Warning($"Adding trailing slash to BaseUrl: {connection.BaseUrl}");
        connection.BaseUrl += "/";
        modified = true;
      }

      // Auto-fix: Remove leading slash from endpoints if present
      if (!string.IsNullOrEmpty(connection.TemperatureEndpoint) && connection.TemperatureEndpoint.StartsWith('/'))
      {
        MelonLogger.Warning($"Removing leading slash from TemperatureEndpoint: {connection.TemperatureEndpoint}");
        connection.TemperatureEndpoint = connection.TemperatureEndpoint.TrimStart('/');
        modified = true;
      }

      if (!string.IsNullOrEmpty(connection.HumidityEndpoint) && connection.HumidityEndpoint.StartsWith('/'))
      {
        MelonLogger.Warning($"Removing leading slash from HumidityEndpoint: {connection.HumidityEndpoint}");
        connection.HumidityEndpoint = connection.HumidityEndpoint.TrimStart('/');
        modified = true;
      }

      return modified;
    }

    private static async Task HandleConnectionAsync(
      Connection connection,
      CancellationToken cancellationToken,
      bool verboseLogging = false,
      bool verboseParametersLogging = false
    )
    {
      VerboseMelonLogger.Msg($"Handling connection to {connection.BaseUrl}...", verboseLogging);
      using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) }) // Set a timeout of 2 seconds
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          if (connection.TemperatureEndpointEnable)
          {
            await HandleTemperatureDataAsync(client, connection, verboseLogging, verboseParametersLogging);
          }

          if (connection.HumidityEndpointEnable)
          {
            await HandleHumidityDataAsync(client, connection, verboseLogging, verboseParametersLogging);
          }

          await Task.Delay(connection.PolingRate * 1000, cancellationToken);
        }
      }
    }

    private static async Task HandleTemperatureDataAsync(
      HttpClient client,
      Connection connection,
      bool verboseLogging,
      bool verboseParametersLogging
    )
    {
      try
      {
        VerboseMelonLogger.Msg(
          $"Fetching temperature from: {connection.BaseUrl}{connection.TemperatureEndpoint}",
          verboseLogging
        );
        var response = await client.GetStringAsync(connection.BaseUrl + connection.TemperatureEndpoint);
        VerboseMelonLogger.Msg($"Temperature response: {response}", verboseLogging);
        var temperatureData = JsonConvert.DeserializeObject<SensorData>(response);
        if (temperatureData != null && !string.IsNullOrEmpty(temperatureData.Value))
        {
          // Handle temperature data
          VerboseMelonLogger.Msg($"Temperature parsed: {temperatureData.Value}", verboseLogging);
          VerboseMelonLogger.Msg($"Temperature: {temperatureData.Value}", verboseParametersLogging);

          // Update the animator parameter
          if (PlayerSetup.Instance?.AnimatorManager != null)
          {
            VerboseMelonLogger.Msg(
              $"Setting parameter {connection.TemperatureEndpointAnimatorParameter} to {temperatureData.Value}",
              verboseLogging
            );
            PlayerSetup.Instance.AnimatorManager.SetParameter(
              connection.TemperatureEndpointAnimatorParameter,
              float.Parse(temperatureData.Value)
            );
          }
          else
          {
            VerboseMelonLogger.Error("PlayerSetup.Instance or AnimatorManager is null.", verboseLogging);
          }
        }
        else
        {
          VerboseMelonLogger.Error(
            $"Temperature data is null or Value is empty. Data: {temperatureData?.Value ?? "null"}",
            verboseLogging
          );
        }
      }
      catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
      {
        MelonLogger.Error($"Error fetching temperature data: Request timed out.");
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error fetching temperature data: {ex.Message}");
      }
    }

    private static async Task HandleHumidityDataAsync(
      HttpClient client,
      Connection connection,
      bool verboseLogging,
      bool verboseParametersLogging
    )
    {
      try
      {
        VerboseMelonLogger.Msg(
          $"Fetching humidity from: {connection.BaseUrl}{connection.HumidityEndpoint}",
          verboseLogging
        );
        var response = await client.GetStringAsync(connection.BaseUrl + connection.HumidityEndpoint);
        VerboseMelonLogger.Msg($"Humidity response: {response}", verboseLogging);
        var humidityData = JsonConvert.DeserializeObject<SensorData>(response);
        if (humidityData != null && !string.IsNullOrEmpty(humidityData.Value))
        {
          // Handle humidity data
          VerboseMelonLogger.Msg($"Humidity parsed: {humidityData.Value}", verboseLogging);
          VerboseMelonLogger.Msg($"Humidity: {humidityData.Value}", verboseParametersLogging);

          // Update the animator parameter
          if (PlayerSetup.Instance?.AnimatorManager != null)
          {
            VerboseMelonLogger.Msg(
              $"Setting parameter {connection.HumidityEndpointAnimatorParameter} to {humidityData.Value}",
              verboseLogging
            );
            PlayerSetup.Instance.AnimatorManager.SetParameter(
              connection.HumidityEndpointAnimatorParameter,
              float.Parse(humidityData.Value)
            );
          }
          else
          {
            VerboseMelonLogger.Error("PlayerSetup.Instance or AnimatorManager is null.", verboseLogging);
          }
        }
        else
        {
          VerboseMelonLogger.Error(
            $"Humidity data is null or Value is empty. Data: {humidityData?.Value ?? "null"}",
            verboseLogging
          );
        }
      }
      catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
      {
        MelonLogger.Error($"Error fetching humidity data: Request timed out.");
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Error fetching humidity data: {ex.Message}");
      }
    }

    // function to send THtoCVRDisabled to the animator
    private void SendTHtoCVRDisabledToAnimator()
    {
      PlayerSetup.Instance.AnimatorManager.SetParameter("THtoCVRDisabled", THtoCVRDisabled ? 1 : 0);
    }

    public void Cleanup()
    {
      if (cancellationTokenSource != null)
      {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
      }
    }

    public override void OnApplicationQuit()
    {
      Cleanup();
    }
  }

#region Supporting Classes

  public static class VerboseMelonLogger
  {
    public static void Msg(string message, bool verbose = false)
    {
      if (verbose)
      {
        MelonLogger.Msg(message);
      }
    }

    public static void Error(string message, bool verbose = false)
    {
      if (verbose)
      {
        MelonLogger.Error(message);
      }
    }

    public static void BigError(string namesection, string message, bool verbose = false)
    {
      if (verbose)
      {
        MelonLogger.BigError(namesection, message);
      }
    }
  }

  // Define the classes to represent the expected structure of the config file
  public class Config
  {
    public List<Connection> Connections { get; set; }
  }

  public class Connection
  {
    public string BaseUrl { get; set; }
    public string TemperatureEndpoint { get; set; }
    public bool TemperatureEndpointEnable { get; set; }
    public string TemperatureEndpointAnimatorParameter { get; set; }
    public string HumidityEndpoint { get; set; }
    public bool HumidityEndpointEnable { get; set; }
    public string HumidityEndpointAnimatorParameter { get; set; }
    public int PolingRate { get; set; }
  }

  // Define the class to represent the sensor data
  public class SensorData
  {
    public string App { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public string Value { get; set; }
  }
#endregion // Supporting Classes
}
