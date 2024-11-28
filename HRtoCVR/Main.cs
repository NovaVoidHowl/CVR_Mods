using System;
using System.Net.Http;
using System.Net.WebSockets;
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
using uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR;

#pragma warning disable S101
public class HRtoCVR : MelonMod
#pragma warning restore S101
{
  public enum HRConnectionType
  {
    Pulsoid,
    Simulated,
    TextFile
  }

  public const string CoreVersion = "0.1.3";

// Melon Loader vars should stay public
#pragma warning disable S1104
  // Core Mellon Loader Vars
  public MelonPreferences_Entry<bool> meEnable;
  public MelonPreferences_Entry<bool> meVerboseLogging;
  public MelonPreferences_Entry<bool> meVerboseParametersLogging;
  public MelonPreferences_Entry<int> meMinHR;
  public MelonPreferences_Entry<int> meMaxHR;
  public MelonPreferences_Entry<HRConnectionType> meHRType;

  // Pulsoid Specific Mellon Loader Vars
  public MelonPreferences_Entry<string> mePulsoidKey;

  // Text File Specific Mellon Loader Vars
  public MelonPreferences_Entry<string> meTextFileLocation;
  public MelonPreferences_Entry<int> meTextFilePollingRate;
#pragma warning restore S1104

  // Values to be fed
  private bool HRtoCVRDisabled; // Returns whether the mod's data feed is disabled or not
  private PulsoidClient _pulsoidClient;
  private SimulatedClient _simulatedClient;
  private TextFileClient _textFileClient;

  // private internal logic variables
  private List<IDisposable> activeClients = new List<IDisposable>();

  // On Melon Load
  public override void OnInitializeMelon()
  {
#if DEBUG
    MelonLogger.Error(
      "This mod was compiled in DEBUG mode log spam possible and API keys may be visible in logs,"
        + " do not use in production environment"
    );
#endif

    // Melon Config
    var melonCategoryHRtoCVR = MelonPreferences.CreateCategory(nameof(HRtoCVR));
    // Core Mod Options
    meEnable = melonCategoryHRtoCVR.CreateEntry(
      "Enable",
      true,
      description: "Enables or Disables the data feed, note values will default to true when disabled."
    );
    meVerboseLogging = melonCategoryHRtoCVR.CreateEntry(
      "Verbose Logging",
      false,
      description: "Enables or Disables verbose logging."
    );
    meVerboseParametersLogging = melonCategoryHRtoCVR.CreateEntry(
      "Verbose Parameters Logging",
      false,
      description: "Enables or Disables verbose logging of the avatar parameter feed values."
    );
    meMinHR = melonCategoryHRtoCVR.CreateEntry("Min HR", 0, description: "Minimum Heart Rate Value.");
    meMaxHR = melonCategoryHRtoCVR.CreateEntry("Max HR", 255, description: "Maximum Heart Rate Value.");
    meHRType = melonCategoryHRtoCVR.CreateEntry(
      "HR Type",
      HRConnectionType.Pulsoid,
      description: "Type of connection type to use."
    );

    // Pulsoid Specific Options
    mePulsoidKey = melonCategoryHRtoCVR.CreateEntry(
      "Pulsoid Key",
      "XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
      description: "Pulsoid Key for the data feed.",
      is_hidden: true // Hide the key from the UI its an API key and should not be visible
    );

    // text file Specific Options
    meTextFileLocation = melonCategoryHRtoCVR.CreateEntry(
      "Text File Location",
      "C:\\path\\to\\file.txt",
      description: "Location of the text file to read from."
    );
    meTextFilePollingRate = melonCategoryHRtoCVR.CreateEntry(
      "HR File Polling Rate",
      10,
      description: "the rate in seconds at which the file should be read"
    );

    // Event Listeners MelonLoader
    meEnable.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        OnMeEnableChanged(oldValue, newValue);
        if (HRtoCVRDisabled)
        {
          DisposeCurrentClients();
          SetMainAvatarParameters(true);
        }
        else
        {
          InitializeHRClient();
        }
        UpdateHRtoCVR();
      }
    );
    meVerboseLogging.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("Verbose Logging Changed: " + newValue);
        if (meEnable.Value)
        {
          // needed here to change the logging level the client is initialized with
          InitializeHRClient();
        }
      }
    );
    meVerboseParametersLogging.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("Verbose Parameters Logging Changed: " + newValue);
      }
    );
    meMinHR.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("Min HR Changed: " + newValue);
        OnMeMinHRSetChanged(oldValue, newValue);
      }
    );
    meMaxHR.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("Max HR Changed: " + newValue);
        OnMeMaxHRSetChanged(oldValue, newValue);
      }
    );
    meHRType.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("HR Type Changed: " + newValue);
        if (meEnable.Value)
        {
          InitializeHRClient();
        }
      }
    );
    // Pulsoid Specific Event Listeners
    mePulsoidKey.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("Pulsoid Key Changed");
        if (meEnable.Value)
        {
          InitializeHRClient();
        }
      }
    );

    // Text File Specific Event Listeners
    meTextFileLocation.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("HR Text File Path Changed");
        if (meEnable.Value)
        {
          InitializeHRClient();
        }
      }
    );
    meTextFilePollingRate.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("HR Text File Polling Rate Changed");
        if (meEnable.Value)
        {
          InitializeHRClient();
        }
      }
    );

    // Event Listeners CVR,
    // Note: more of these these can be found in the CVRGameEventSystem class under the
    // ABI_RC.Systems.GameEventSystem namespace if needed

    CVRGameEventSystem.Initialization.OnPlayerSetupStart.AddListener(() =>
    {
      if (!meEnable.Value)
      {
        return;
      }
      MelonLogger.Msg("Player Setup Start");
      UpdateHRtoCVR();
    });

    CVRGameEventSystem.Instance.OnConnected.AddListener(
      (string message) =>
      {
        if (!meEnable.Value)
        {
          return;
        }
        MelonLogger.Msg("Instance `" + message + "` Connected, reloading HRtoCVR data source.");
        InitializeHRClient();
        UpdateHRtoCVR();
        SetMainAvatarParameters();
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
        UpdateHRtoCVR();
        SetMainAvatarParameters();
      }
    );

    if (meEnable.Value)
    {
      InitializeHRClient();
    }
  }

  private void InitializeHRClient()
  {
    // Dispose of any existing clients
    DisposeCurrentClients();
  
    switch (meHRType.Value)
    {
      case HRConnectionType.Pulsoid:
        _pulsoidClient = new PulsoidClient
        {
          verboseLogging = meVerboseLogging.Value // Set the verbose logging flag
        };
        _pulsoidClient.InitializeHeartBeatTimer();
        _pulsoidClient.OnHeartRateUpdated += OnHeartRateUpdatedHandler;
        _pulsoidClient.OnHeartRateRapidUpdated += OnHeartRateRapidUpdatedHandler;
        _ = _pulsoidClient.InitializeWebSocket(mePulsoidKey.Value, meMinHR.Value, meMaxHR.Value);
        activeClients.Add(_pulsoidClient); // Add to the list of active clients
        break;
      case HRConnectionType.Simulated:
        _simulatedClient = new SimulatedClient();
        _simulatedClient.InitializeHeartBeatTimer();
        _simulatedClient.InitializeClient(meMinHR.Value, meMaxHR.Value);
        _simulatedClient.OnHeartRateUpdated += OnHeartRateUpdatedHandler;
        _simulatedClient.OnHeartRateRapidUpdated += OnHeartRateRapidUpdatedHandler;
        activeClients.Add(_simulatedClient); // Add to the list of active clients
        break;
      case HRConnectionType.TextFile:
        _textFileClient = new TextFileClient(meTextFileLocation.Value, meTextFilePollingRate.Value);
        _textFileClient.InitializeHeartBeatTimer();
        _textFileClient.InitializeClient(meMinHR.Value, meMaxHR.Value);
        _textFileClient.OnHeartRateUpdated += OnHeartRateUpdatedHandler;
        _textFileClient.OnHeartRateRapidUpdated += OnHeartRateRapidUpdatedHandler;
        activeClients.Add(_textFileClient); // Add to the list of active clients
        break;
      default:
        MelonLogger.Error("Unknown HRConnectionType.");
        break;
    }
  }

  private void DisposeCurrentClients()
  {
    foreach (var client in activeClients)
    {
      client.Dispose();
    }
    activeClients.Clear();
  }

  #region melon variables update

  private void OnMeEnableChanged(bool oldValue, bool newValue)
  {
    if (oldValue == newValue)
    {
      // no change in value
#if DEBUG
      MelonLogger.Msg("HRtoCVR Enable parameter event triggered, but has not changed.");
#endif
    }
    else
    {
      MelonLogger.Msg("HRtoCVR Enabled: " + newValue);
      UpdateHRtoCVR();
      SetMainAvatarParameters();
    }
  }

  private void OnMeMinHRSetChanged(int oldValue, int newValue)
  {
    if (oldValue == newValue)
    {
      // no change in value
#if DEBUG
      MelonLogger.Msg("Min HR parameter event triggered, but has not changed.");
#endif
    }
    else
    {
      MelonLogger.Msg("Min HR Changed: " + newValue);
      UpdateHRtoCVR();
      SetMainAvatarParameters();
      InitializeHRClient();
    }
  }

  private void OnMeMaxHRSetChanged(int oldValue, int newValue)
  {
    if (oldValue == newValue)
    {
      // no change in value
#if DEBUG
      MelonLogger.Msg("Max HR parameter event triggered, but has not changed.");
#endif
    }
    else
    {
      MelonLogger.Msg("Max HR Changed: " + newValue);
      UpdateHRtoCVR();
      SetMainAvatarParameters();
      InitializeHRClient();
    }
  }

  #endregion melon variables update

  private void UpdateHRtoCVR()
  {
    // check if the mod is enabled
    if (!meEnable.Value)
    {
      // if disabled, set all values to defaults
      HRtoCVRDisabled = true;
      DisposeCurrentClients();
    }
    else
    {
      HRtoCVRDisabled = false;
      // at this point no need to send the HR values here,
      // just the fact that the mod is enabled
    }
  }

  private void VerbosePrintMainAvatarParameters()
  {
    // check if verbose logging is enabled
    if (!meVerboseParametersLogging.Value)
    {
      return;
    }
    // all this function does is batch print the parameters to the melon logger
    switch (meHRType.Value)
    {
      case HRConnectionType.Pulsoid:
        if (_pulsoidClient != null)
        {
          MelonLogger.Msg("onesHR: " + _pulsoidClient.onesHR);
          MelonLogger.Msg("tensHR: " + _pulsoidClient.tensHR);
          MelonLogger.Msg("hundredsHR: " + _pulsoidClient.hundredsHR);
          MelonLogger.Msg("isHRConnected: " + _pulsoidClient.isHRConnected);
          MelonLogger.Msg("isHRActive: " + _pulsoidClient.isHRActive);
          MelonLogger.Msg("HRPercent: " + _pulsoidClient.HRPercent);
          MelonLogger.Msg("HR: " + _pulsoidClient.HR);
        }
        break;
      case HRConnectionType.Simulated:
        if (_simulatedClient != null)
        {
          MelonLogger.Msg("onesHR: " + _simulatedClient.onesHR);
          MelonLogger.Msg("tensHR: " + _simulatedClient.tensHR);
          MelonLogger.Msg("hundredsHR: " + _simulatedClient.hundredsHR);
          MelonLogger.Msg("isHRConnected: " + _simulatedClient.isHRConnected);
          MelonLogger.Msg("isHRActive: " + _simulatedClient.isHRActive);
          MelonLogger.Msg("HRPercent: " + _simulatedClient.HRPercent);
          MelonLogger.Msg("HR: " + _simulatedClient.HR);
        }
        break;

      case HRConnectionType.TextFile:
        if (_textFileClient != null)
        {
          MelonLogger.Msg("onesHR: " + _textFileClient.onesHR);
          MelonLogger.Msg("tensHR: " + _textFileClient.tensHR);
          MelonLogger.Msg("hundredsHR: " + _textFileClient.hundredsHR);
          MelonLogger.Msg("isHRConnected: " + _textFileClient.isHRConnected);
          MelonLogger.Msg("isHRActive: " + _textFileClient.isHRActive);
          MelonLogger.Msg("HRPercent: " + _textFileClient.HRPercent);
          MelonLogger.Msg("HR: " + _textFileClient.HR);
        }
        break;
    }
  }

  private void OnHeartRateUpdatedHandler()
  {
    SetMainAvatarParameters();
  }

  private void OnHeartRateRapidUpdatedHandler()
  {
    SetRapidUpdateParameters();
  }

  private void SetRapidUpdateParameters()
  {
    AvatarParameterSetter.SetRapidUpdateParameters(
      _pulsoidClient,
      _simulatedClient,
      _textFileClient,
      meHRType.Value
    );
  }

  private void SetMainAvatarParameters(bool resetToDefault = false)
  {
    // print the parameters to the melon logger if verbose logging is enabled
    VerbosePrintMainAvatarParameters();
    // set the parameters using AvatarParameterSetter
    AvatarParameterSetter.SetMainParameters(
      HRtoCVRDisabled,
      _pulsoidClient,
      _simulatedClient,
      _textFileClient,
      meHRType.Value,
      resetToDefault
    );
  }

  public override void OnApplicationQuit()
  {
    DisposeCurrentClients();
  }
}
