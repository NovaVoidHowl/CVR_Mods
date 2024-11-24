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

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR;

#pragma warning disable S101
public class HRtoCVR : MelonMod
#pragma warning restore S101
{
  public enum HRConnectionType
  {
    Pulsoid,
    Simulated
  }

  // Core Mellon Loader Vars
  public MelonPreferences_Entry<bool> meEnable;
  public MelonPreferences_Entry<int> meMinHR;
  public MelonPreferences_Entry<int> meMaxHR;
  public MelonPreferences_Entry<HRConnectionType> meHRType;
  public MelonPreferences_Entry<string> mePulsoidKey;

  // Values to be fed
  private bool HRtoCVRDisabled; // Returns whether the mod's data feed is disabled or not
  private PulsoidClient _pulsoidClient;
  private SimulatedClient _simulatedClient;

  // On Melon Load
  public override void OnInitializeMelon()
  {
    // Melon Config
    var melonCategoryHRtoCVR = MelonPreferences.CreateCategory(nameof(HRtoCVR));
    // Core Mod Options
    meEnable = melonCategoryHRtoCVR.CreateEntry(
      "Enable",
      true,
      description: "Enables or Disables the data feed, note values will default to true when disabled."
    );
    meMinHR = melonCategoryHRtoCVR.CreateEntry("Min HR", 0, description: "Minimum Heart Rate Value.");
    meMaxHR = melonCategoryHRtoCVR.CreateEntry("Max HR", 255, description: "Maximum Heart Rate Value.");
    meHRType = melonCategoryHRtoCVR.CreateEntry(
      "HR Type",
      HRConnectionType.Pulsoid,
      description: "Type of connection type to use."
    );
    mePulsoidKey = melonCategoryHRtoCVR.CreateEntry(
      "Pulsoid Key",
      "XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
      description: "Pulsoid Key for the data feed."
    );

    // Event Listeners MelonLoader
    meEnable.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        OnMeEnableChanged(oldValue, newValue);
        // Log the values to the melon logger
        if (HRtoCVRDisabled)
        {
          MelonLogger.Msg("HRtoCVR Disabled");
        }
        else
        {
          MelonLogger.Msg("HRtoCVR Enabled");
        }
        UpdateHRtoCVR();
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
        InitializeHRClient();
      }
    );
    mePulsoidKey.OnEntryValueChanged.Subscribe(
      (oldValue, newValue) =>
      {
        MelonLogger.Msg("Pulsoid Key Changed");
      }
    );

    // Event Listeners CVR,
    // Note: more of these these can be found in the CVRGameEventSystem class under the
    // ABI_RC.Systems.GameEventSystem namespace if needed

    CVRGameEventSystem.Initialization.OnPlayerSetupStart.AddListener(() =>
    {
      MelonLogger.Msg("Player Setup Start");
      UpdateHRtoCVR();
    });

    CVRGameEventSystem.Instance.OnConnected.AddListener(
      (string message) =>
      {
        MelonLogger.Msg("On Instance Load: " + message);
        UpdateHRtoCVR();
        SetMainAvatarParameters();
      }
    );

    CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener(
      (CVRAvatar avatar) =>
      {
        // get the game object of the avatar
        MelonLogger.Msg("On Local Avatar Load: " + avatar.gameObject.name);
        UpdateHRtoCVR();
        SetMainAvatarParameters();
      }
    );

    InitializeHRClient();
  }

  private void InitializeHRClient()
  {
    // Dispose of any existing client
    DisposeCurrentClient();

    switch (meHRType.Value)
    {
      case HRConnectionType.Pulsoid:
        _pulsoidClient = new PulsoidClient();
        _pulsoidClient.InitializeHeartBeatTimer();
        _ = _pulsoidClient.InitializeWebSocket(mePulsoidKey.Value, meMinHR.Value, meMaxHR.Value);
        break;
      case HRConnectionType.Simulated:
        _simulatedClient = new SimulatedClient();
        break;
      default:
        MelonLogger.Error("Unknown HRConnectionType.");
        break;
    }
  }

  private void DisposeCurrentClient()
  {
    switch (meHRType.Value)
    {
      case HRConnectionType.Pulsoid:
        _pulsoidClient?.Dispose();
        _pulsoidClient = null;
        break;
      case HRConnectionType.Simulated:
        _simulatedClient?.Dispose();
        _simulatedClient = null;
        break;
    }
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
      DisposeCurrentClient();
    }
    else
    {
      HRtoCVRDisabled = false;
      // at this point no need to send the HR values here,
      // just the fact that the mod is enabled
    }
  }

  private void SetMainAvatarParameters()
  {
    // all this function does is batch set the parameters to the animator
    PlayerSetup.Instance.animatorManager.SetParameter("HRtoCVRDisabled", HRtoCVRDisabled);

    switch (meHRType.Value)
    {
      case HRConnectionType.Pulsoid:
        if (_pulsoidClient != null)
        {
          PlayerSetup.Instance.animatorManager.SetParameter("onesHR", _pulsoidClient.onesHR);
          PlayerSetup.Instance.animatorManager.SetParameter("tensHR", _pulsoidClient.tensHR);
          PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", _pulsoidClient.hundredsHR);
          PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", _pulsoidClient.isHRConnected);
          PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", _pulsoidClient.isHRActive);
          PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", _pulsoidClient.HRPercent);
          PlayerSetup.Instance.animatorManager.SetParameter("HR", _pulsoidClient.HR);
        }
        break;
      case HRConnectionType.Simulated:
        if (_simulatedClient != null)
        {
          PlayerSetup.Instance.animatorManager.SetParameter("onesHR", _simulatedClient.onesHR);
          PlayerSetup.Instance.animatorManager.SetParameter("tensHR", _simulatedClient.tensHR);
          PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", _simulatedClient.hundredsHR);
          PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", _simulatedClient.isHRConnected);
          PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", _simulatedClient.isHRActive);
          PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", _simulatedClient.HRPercent);
          PlayerSetup.Instance.animatorManager.SetParameter("HR", _simulatedClient.HR);
        }
        break;
    }
  }

  public override void OnApplicationQuit()
  {
    DisposeCurrentClient();
  }
}
