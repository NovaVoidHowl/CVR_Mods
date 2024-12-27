using UnityEngine;
using System.Collections;
using MelonLoader;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI.CCK.Components;
using ABI_RC.Systems.Movement;
using uk.novavoidhowl.dev.cvrmods.DataFeed.api;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed
{
  public class DataFeed : MelonMod
  {
    public event EventHandler StateChanged;
    public event EventHandler AvatarChanged;
    public event EventHandler InstanceChanged;

    // Melon Loader vars should stay public
#pragma warning disable S1104
    // Core Mellon Loader Vars
    public MelonPreferences_Entry<bool> meEnable;
    public MelonPreferences_Entry<bool> meAvatarParamSetEnabled;

    public MelonPreferences_Entry<bool> meAPIEnable;
    public MelonPreferences_Entry<string> meRestAPIPort;
    public MelonPreferences_Entry<string> meWebsocketAPIPort;
    public MelonPreferences_Entry<string> meAPIKey;
#pragma warning restore S1104



    // Values to be fed to APIs and Avatar Parameters
    private bool flyingAllowed;
    private bool propsAllowed;
    private bool portalsAllowed;
    private bool nameplatesEnabled;
    private bool dataFeedErrorBBCC;
    private bool dataFeedErrorMetaPort;
    private bool dataFeedDisabled;

    public bool FlyingAllowed => flyingAllowed;
    public bool PropsAllowed => propsAllowed;
    public bool PortalsAllowed => portalsAllowed;
    public bool NameplatesEnabled => nameplatesEnabled;
    public bool DataFeedErrorBBCC => dataFeedErrorBBCC;
    public bool DataFeedErrorMetaPort => dataFeedErrorMetaPort;
    public bool DataFeedDisabled => dataFeedDisabled;

    // Values to be fed to APIs only
    #region World Data
    private string currentInstanceId;
    private string currentInstanceName;
    private string currentWorldId;
    private string currentInstancePrivacy;

    public string CurrentInstanceId => currentInstanceId;
    public string CurrentInstanceName => currentInstanceName;
    public string CurrentWorldId => currentWorldId;
    public string CurrentInstancePrivacy => currentInstancePrivacy;
    #endregion // World Data

    #region Avatar Data
    private string currentAvatarId;
    public string CurrentAvatarId => currentAvatarId;
    #endregion


    #region  Game data
    private int buildId;
    private string hardwareId;

    public int BuildId => buildId;
    public string HardwareId => hardwareId;
    #endregion // Game data

    #region real-time data
    private int currentPing;
    public int CurrentPing => currentPing;
    #endregion // real-time data

    // API Server
    private ApiConfig apiConfig;
    private ApiServer apiServer;

    public ApiConfig ApiConfig => apiConfig;

    private bool isQuitting = false;

    // On Melon Load
    public override void OnInitializeMelon()
    {
      // Melon Config
      var _MelonCategoryDataFeed = MelonPreferences.CreateCategory(nameof(DataFeed));
      meEnable = _MelonCategoryDataFeed.CreateEntry(
        "Enable",
        true,
        description: "Enables or Disables the data feed, note values will default to true when disabled ."
      );
      meAvatarParamSetEnabled = _MelonCategoryDataFeed.CreateEntry(
        "Avatar Parameter Output Enabled",
        true,
        description: "Enables or Disables setting of Avatar Parameters."
      );

      meAPIEnable = _MelonCategoryDataFeed.CreateEntry(
        "API Enable",
        true,
        description: "Enables or Disables the API server"
      );

      meRestAPIPort = _MelonCategoryDataFeed.CreateEntry(
        "REST API Port",
        "8080",
        description: "Port for the API server"
      );
      meWebsocketAPIPort = _MelonCategoryDataFeed.CreateEntry(
        "Websocket API Port",
        "8081",
        description: "Port for the Websocket API server"
      );

      meAPIKey = _MelonCategoryDataFeed.CreateEntry(
        "API Key",
        _MelonCategoryDataFeed.GetEntry<string>("API_Key")?.Value ?? Guid.NewGuid().ToString(),
        description: "API Key to access the local DataFeed server",
        is_hidden: true // hide the key from the UI to prevent accidental leaks
      );

      // Event Listeners MelonLoader
      meEnable.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          OnMeEnableChanged(oldValue, newValue);
        }
      );
      meAvatarParamSetEnabled.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          OnMeAvatarParamSetEnabledChanged(oldValue, newValue);
        }
      );

      meAPIEnable.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          OnMeAPIEnableChanged(oldValue, newValue);
        }
      );

      meRestAPIPort.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          OnMeAPIPortChanged(oldValue, newValue);
        }
      );

      meWebsocketAPIPort.OnEntryValueChanged.Subscribe(
        (oldValue, newValue) =>
        {
          OnMeWebsocketAPIPortChanged(oldValue, newValue);
        }
      );

      // Event Listeners CVR,
      // Note: more of these these can be found in the CVRGameEventSystem class under the
      // ABI_RC.Systems.GameEventSystem namespace if needed

      CVRGameEventSystem.Initialization.OnPlayerSetupStart.AddListener(() =>
      {
        GeneralHelper.DebugLog("Player Setup Start");
        UpdateDataFeed();
      });

      CVRGameEventSystem.Instance.OnConnected.AddListener(
        (string message) =>
        {
          GeneralHelper.DebugLog("On Instance Load: " + message);
          UpdateDataFeed();
          PrintCurrentDataFeedValues();
          SetAvatarParameters();
          OnStateChanged();
          OnInstanceChanged(); // Add this line
        }
      );

      CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(
        (CVRAvatar avatar) =>
        {
          // get the game object of the avatar
          GeneralHelper.DebugLog("On Local Avatar Load: " + avatar.gameObject.name);
          currentAvatarId = avatar.gameObject.name;
          UpdateDataFeed();
          PrintCurrentDataFeedValues();
          SetAvatarParameters();
          OnAvatarChanged(); // Add this line
        }
      );

      #region api init
      apiConfig = new ApiConfig
      {
        WebSocketPort = meWebsocketAPIPort.Value,
        RestApiPort = meRestAPIPort.Value,
        ApiKey = meAPIKey.Value
      };
      GeneralHelper.DebugLog($"Initialized API config with key: {apiConfig.ApiKey}");
      if (meAPIEnable.Value)
      {
        apiServer = new ApiServer(this, apiConfig);
        apiServer.Start();
      }
      #endregion // api init

      MelonCoroutines.Start(UpdatePingCoroutine());
    }

    private void OnMeEnableChanged(bool oldValue, bool newValue)
    {
      if (oldValue == newValue)
      {
        // no change in value
        GeneralHelper.DebugLog("Data Feed Enable parameter event triggered, but has not changed.");
      }
      else
      {
        MelonLogger.Msg("Data Feed Enabled: " + newValue);
        UpdateDataFeed();
        SetAvatarParameters();

        if (!newValue && apiServer != null)
        {
          GeneralHelper.DebugLog("Disabling API Server due to Data Feed being disabled.");
          apiServer.Stop();
          apiServer = null;
        }
        else if (newValue && meAPIEnable.Value && apiServer == null)
        {
          GeneralHelper.DebugLog("Enabling API Server due to Data Feed being enabled.");
          apiServer = new ApiServer(this, apiConfig);
          apiServer.Start();
        }
      }
    }

    private void OnMeAvatarParamSetEnabledChanged(bool oldValue, bool newValue)
    {
      if (oldValue == newValue)
      {
        // no change in value
        GeneralHelper.DebugLog("Avatar Parameter Output Enabled parameter event triggered, but has not changed.");
      }
      else
      {
        MelonLogger.Msg("Avatar Parameter Output Enabled: " + newValue);
        UpdateDataFeed();
        SetAvatarParameters();
      }
    }

    private void OnMeAPIEnableChanged(bool oldValue, bool newValue)
    {
      if (oldValue == newValue)
        return;

      if (newValue && meEnable.Value)
      {
        MelonLogger.Msg("API Server Enabled");
        apiServer = new ApiServer(this, apiConfig);
        apiServer.Start();
      }
      else
      {
        MelonLogger.Msg("API Server Disabled");
        apiServer?.Stop();
        apiServer = null;
      }
    }

    // Update existing OnMeAPIPortChanged method
    private void OnMeAPIPortChanged(string oldValue, string newValue)
    {
      if (oldValue == newValue)
        return;

      GeneralHelper.DebugLog($"REST API Port changed from {oldValue} to {newValue}");
      apiConfig.RestApiPort = newValue;

      if (meAPIEnable.Value && apiServer != null)
      {
        apiServer.Stop();
        apiServer = new ApiServer(this, apiConfig);
        apiServer.Start();
      }
    }

    private void OnMeWebsocketAPIPortChanged(string oldValue, string newValue)
    {
      if (oldValue == newValue)
        return;

      GeneralHelper.DebugLog($"WebSocket API Port changed from {oldValue} to {newValue}");
      apiConfig.WebSocketPort = newValue;

      if (meAPIEnable.Value && apiServer != null)
      {
        apiServer.Stop();
        apiServer = new ApiServer(this, apiConfig);
        apiServer.Start();
      }
    }

    private void UpdateDataFeed()
    {
      bool stateChanged = false;

      if (!meEnable.Value)
      {
        stateChanged |= !flyingAllowed;
        stateChanged |= !propsAllowed;
        stateChanged |= !portalsAllowed;
        stateChanged |= !nameplatesEnabled;
        stateChanged |= dataFeedDisabled;

        flyingAllowed = true;
        propsAllowed = true;
        portalsAllowed = true;
        nameplatesEnabled = true;
        dataFeedDisabled = true;
      }
      else
      {
        stateChanged |= dataFeedDisabled;
        dataFeedDisabled = false;

        if (BetterBetterCharacterController.Instance == null)
        {
          stateChanged |= !dataFeedErrorBBCC;
          dataFeedErrorBBCC = true;
        }
        else
        {
          stateChanged |= flyingAllowed != BetterBetterCharacterController.Instance.FlightAllowedInWorld;
          stateChanged |= dataFeedErrorBBCC;
          flyingAllowed = BetterBetterCharacterController.Instance.FlightAllowedInWorld;
          dataFeedErrorBBCC = false;
        }

        if (MetaPort.Instance == null)
        {
          stateChanged |= dataFeedErrorMetaPort;
          dataFeedErrorMetaPort = true;
        }
        else
        {
          stateChanged |= propsAllowed != MetaPort.Instance.worldAllowProps;
          stateChanged |= portalsAllowed != MetaPort.Instance.worldAllowPortals;
          stateChanged |= nameplatesEnabled != MetaPort.Instance.worldEnableNameplates;
          stateChanged |= dataFeedErrorMetaPort;

          propsAllowed = MetaPort.Instance.worldAllowProps;
          portalsAllowed = MetaPort.Instance.worldAllowPortals;
          nameplatesEnabled = MetaPort.Instance.worldEnableNameplates;
          dataFeedErrorMetaPort = false;

          // Update instance and world information
          currentInstanceId = MetaPort.Instance.CurrentInstanceId;
          currentInstanceName = MetaPort.Instance.CurrentInstanceName;
          currentWorldId = MetaPort.Instance.CurrentWorldId;
          currentInstancePrivacy = MetaPort.Instance.CurrentInstancePrivacy;

          // Update game data
          buildId = MetaPort.Instance.buildId;
          hardwareId = MetaPort.Instance.hardwareId;
        }
      }

      if (stateChanged)
      {
        OnStateChanged();
      }
    }

    private void SetAvatarParameters()
    {
      // check if the mod is enabled
      if (!meEnable.Value || !meAvatarParamSetEnabled.Value)
      {
        // Feed disabled
        // main data feed
        PlayerSetup.Instance.animatorManager.SetParameter("flyingAllowed", true);
        PlayerSetup.Instance.animatorManager.SetParameter("propsAllowed", true);
        PlayerSetup.Instance.animatorManager.SetParameter("portalsAllowed", true);
        PlayerSetup.Instance.animatorManager.SetParameter("nameplatesEnabled", true);

        // mod status data feed
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedErrorBBCC", false);
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedErrorMetaPort", false);
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedDisabled", true);

        MelonLogger.Msg("Avatar Data Feed Disabled, Parameters set to default.");
      }
      else
      {
        // main data feed
        PlayerSetup.Instance.animatorManager.SetParameter("flyingAllowed", FlyingAllowed);
        PlayerSetup.Instance.animatorManager.SetParameter("propsAllowed", PropsAllowed);
        PlayerSetup.Instance.animatorManager.SetParameter("portalsAllowed", PortalsAllowed);
        PlayerSetup.Instance.animatorManager.SetParameter("nameplatesEnabled", NameplatesEnabled);

        // mod status data feed
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedErrorBBCC", DataFeedErrorBBCC);
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedErrorMetaPort", DataFeedErrorMetaPort);
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedDisabled", DataFeedDisabled);

        MelonLogger.Msg("Avatar Parameters Set.");
      }
    }

    private void PrintCurrentDataFeedValues()
    {
      // Log the values to the melon logger
      MelonLogger.Msg("Data Feed:");
      MelonLogger.Msg("FlyingAllowed: " + FlyingAllowed);
      MelonLogger.Msg("PropsAllowed: " + PropsAllowed);
      MelonLogger.Msg("PortalsAllowed: " + PortalsAllowed);
      MelonLogger.Msg("NameplatesEnabled: " + NameplatesEnabled);
      MelonLogger.Msg("Data Feed Status:");
      MelonLogger.Msg("BBCC Error: " + DataFeedErrorBBCC);
      MelonLogger.Msg("MetaPort Error: " + DataFeedErrorMetaPort);
      MelonLogger.Msg("Data Feed Disabled: " + DataFeedDisabled);
    }

    public override void OnApplicationQuit()
    {
      // any on quit code here, ie closing connections etc
      isQuitting = true; // Set the flag to true when the application is quitting
      if (apiServer != null && meAPIEnable.Value)
      {
        apiServer.Stop();
      }
    }

    protected virtual void OnStateChanged()
    {
      StateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnAvatarChanged()
    {
      AvatarChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnInstanceChanged()
    {
      InstanceChanged?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator UpdatePingCoroutine()
    {
      while (true)
      {
        if (MetaPort.Instance != null)
        {
          currentPing = MetaPort.Instance.currentPing;
        }
        if (isQuitting) // Check the flag to break out of the loop
        {
          yield break;
        }
        yield return new WaitForSeconds(1f); // Update every second
      }
    }
  }
}
