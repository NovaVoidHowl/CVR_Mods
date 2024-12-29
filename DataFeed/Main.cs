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
using uk.novavoidhowl.dev.cvrmods.DataFeed.Services;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;
using uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors;

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

    // Data Feed Vars
    private bool dataFeedDisabled;
    public bool DataFeedDisabled => dataFeedDisabled;

    #region Avatar Data
    private string currentAvatarId;
    public string CurrentAvatarId => currentAvatarId;
    private AvatarAbiApiInfo currentAvatarDetails;
    public AvatarAbiApiInfo CurrentAvatarDetails => currentAvatarDetails;
    #endregion

    // API Server
    private ApiConfig apiConfig;
    private ApiServer apiServer;

    public ApiConfig ApiConfig => apiConfig;

    private readonly object _stateLock = new object();
    private bool _isQuitting;

    private readonly IMetaPortDataReader _metaPortReader;
    private readonly IAvatarParameterManager _avatarParameterManager;
    private readonly IBetterBetterCharacterControllerDataReader _bbccReader;

    public DataFeed()
    {
      _metaPortReader = new MetaPortDataReader();
      _avatarParameterManager = new AvatarParameterManager();
      _bbccReader = new BetterBetterCharacterControllerDataReader();
    }

    // Expose the interface readers
    public IMetaPortDataReader MetaPortReader => _metaPortReader;
    public IBetterBetterCharacterControllerDataReader BBCCReader => _bbccReader;

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

      CVRGameEventSystem.Instance.OnConnected.AddListener(OnInstanceConnected);
      CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(OnAvatarLoaded);

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

    private void OnInstanceConnected(string message)
    {
      lock (_stateLock)
      {
        UpdateDataFeed();
        PrintCurrentDataFeedValues();
        SetAvatarParameters();
        OnStateChanged();
        OnInstanceChanged();
      }
    }

    private async void OnAvatarLoaded(CVRAvatar avatar)
    {
      if (avatar?.gameObject == null)
        return;

      GameObject currentAvatarGameObject = avatar.gameObject;
      // get the CVRAssetInfo component on the avatar
      CVRAssetInfo assetInfo = currentAvatarGameObject.GetComponent<CVRAssetInfo>();
      if (assetInfo == null) // should not be possible, but just in case
      {
        MelonLogger.Warning("CVRAvatarAssetInfo component not found on avatar.");
        return;
      }

      string avatarId = assetInfo.objectId;
      AvatarAbiApiInfo avatarDetails = null;

      // Fetch avatar details from ABI API outside of lock
      try
      {
        avatarDetails = await AvatarAbiApiService.RequestAvatarDetails(avatarId);
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"Failed to fetch avatar details for {avatarId}: {ex.Message}");
      }

      lock (_stateLock)
      {
        currentAvatarId = avatarId;
        currentAvatarDetails = avatarDetails;

        UpdateDataFeed();
        PrintCurrentDataFeedValues();
        SetAvatarParameters();
        OnAvatarChanged();
      }
    }

    private void UpdateDataFeed()
    {
      if (!meEnable.Value)
      {
        UpdateDisabledState();
        return;
      }

      var stateChanged = false;
      stateChanged |= _bbccReader.UpdateBBCCState();
      stateChanged |= _metaPortReader.UpdateMetaPortState();

      if (stateChanged)
      {
        OnStateChanged();
      }
    }

    private void UpdateDisabledState()
    {
      dataFeedDisabled = true;
      OnStateChanged();
    }

    private void SetAvatarParameters()
    {
      var worldRules = new WorldRuleParameters(
        meAvatarParamSetEnabled.Value,
        BBCCReader.FlyingAllowed,
        MetaPortReader.PropsAllowed,
        MetaPortReader.PortalsAllowed,
        MetaPortReader.NameplatesEnabled
      );

      var modStatus = new ModStatusParameters(meEnable.Value, DataFeedDisabled, !meAPIEnable.Value);

      var platformState = new PlatformStateParameters(
        BBCCReader.DataFeedErrorBBCC,
        MetaPortReader.DataFeedErrorMetaPort
      );

      _avatarParameterManager.SetParameters(worldRules, modStatus, platformState);
    }

    private void PrintCurrentDataFeedValues()
    {
      // Log the values to the melon logger
      MelonLogger.Msg("Data Feed:");
      MelonLogger.Msg("FlyingAllowed: " + BBCCReader.FlyingAllowed);
      MelonLogger.Msg("PropsAllowed: " + MetaPortReader.PropsAllowed);
      MelonLogger.Msg("PortalsAllowed: " + MetaPortReader.PortalsAllowed);
      MelonLogger.Msg("NameplatesEnabled: " + MetaPortReader.NameplatesEnabled);
      MelonLogger.Msg("Data Feed Status:");
      MelonLogger.Msg("BBCC Error: " + BBCCReader.DataFeedErrorBBCC);
      MelonLogger.Msg("MetaPort Error: " + MetaPortReader.DataFeedErrorMetaPort);
      MelonLogger.Msg("Data Feed Disabled: " + DataFeedDisabled);
    }

    public override void OnApplicationQuit()
    {
      // any on quit code here, ie closing connections etc

      _isQuitting = true;
      apiServer?.Stop();
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
      // This can be removed as ping updates are now handled by MetaPortReader
      var waitTime = new WaitForSeconds(1f);
      while (!_isQuitting)
      {
        _metaPortReader.UpdateMetaPortState();
        yield return waitTime;
      }
    }
  }
}
