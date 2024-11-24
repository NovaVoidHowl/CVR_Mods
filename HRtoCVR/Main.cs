using ABI_RC.Core.Savior;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI.CCK.Components;
using ABI_RC.Systems.Movement;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR;

public class HRtoCVR : MelonMod
{
  public enum HRConnectionType
  {
    Pulsoid = 0
  }

  private MelonPreferences_Category _MelonCategoryHRtoCVR;

  // Core Mellon Loader Vars
  public MelonPreferences_Entry<bool> meEnable;
  public MelonPreferences_Entry<int> meMinHR;
  public MelonPreferences_Entry<int> meMaxHR;
  public MelonPreferences_Entry<HRConnectionType> meHRType;
  public MelonPreferences_Entry<string> mePulsoidKey;

  // Values to be fed
  private bool HRtoCVRDisabled; // Returns whether the mod's data feed is disabled or not
  private int onesHR; //	Ones spot in the Heart Rate reading
  private int tensHR; // Tens spot in the Heart Rate reading
  private int hundredsHR;	// Hundreds spot in the Heart Rate reading
  private bool isHRConnected; // Returns whether the device's connection is valid or not
  private bool isHRActive; // Returns whether the connection is valid or not
  private bool isHRBeat; // Estimation on when the heart is beating
  private float HRPercent; // Range of HR between the MinHR and MaxHR config value on a scale of 0 to 1
  private int HR; //Returns the raw HR, ranged from 0 - 255. (required)

  // On Melon Load
  public override void OnInitializeMelon()
  {
    // Melon Config
    _MelonCategoryHRtoCVR = MelonPreferences.CreateCategory(nameof(HRtoCVR));
    // Core Mod Options
    meEnable = _MelonCategoryHRtoCVR.CreateEntry(
      "Enable",
      true,
      description: "Enables or Disables the data feed, note values will default to true when disabled ."
    );
    meMinHR = _MelonCategoryHRtoCVR.CreateEntry(
      "Min HR",
      0,
      description: "Minimum Heart Rate Value."
    );
    meMaxHR = _MelonCategoryHRtoCVR.CreateEntry(
      "Max HR",
      255,
      description: "Maximum Heart Rate Value."
    );
    meHRType = _MelonCategoryHRtoCVR.CreateEntry(
      "HR Type",
      HRConnectionType.Pulsoid,
      description: "Type of connection type to use."
    );
    mePulsoidKey = _MelonCategoryHRtoCVR.CreateEntry(
      "Pulsoid Key",
      "XXXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
      description: "Pulsoid Key for the data feed."
    );

    // Event Listeners MelonLoader
    meEnable.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
      OnMeEnableChanged(oldValue, newValue);
      // Log the values to the melon logger
      if(HRtoCVRDisabled)
      {
        MelonLogger.Msg("HRtoCVR Disabled");
      }
      else
      {
        MelonLogger.Msg("HRtoCVR Enabled");
      }
      UpdateHRtoCVR();
    });
    meMinHR.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
      MelonLogger.Msg("Min HR Changed: " + newValue);
      OnMeMinHRSetChanged(oldValue, newValue);
    });
    meMaxHR.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
      MelonLogger.Msg("Max HR Changed: " + newValue);
      OnMeMaxHRSetChanged(oldValue, newValue);
    });
    meHRType.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
      MelonLogger.Msg("HR Type Changed: " + newValue);
    });
    mePulsoidKey.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
      MelonLogger.Msg("Pulsoid Key Changed");
    });


    // Event Listeners CVR, 
    // Note: more of these these can be found in the CVRGameEventSystem class under the
    // ABI_RC.Systems.GameEventSystem namespace if needed

    CVRGameEventSystem.Initialization.OnPlayerSetupStart.AddListener(() => {
      MelonLogger.Msg("Player Setup Start");
      UpdateHRtoCVR();
    });

    CVRGameEventSystem.Instance.OnConnected.AddListener((string message) => {
      MelonLogger.Msg("On Instance Load: " + message);
      UpdateHRtoCVR();
      SetAvatarParameters();
    });

    CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener((CVRAvatar avatar) => {
      // get the game object of the avatar
      MelonLogger.Msg("On Local Avatar Load: " + avatar.gameObject.name);
      UpdateHRtoCVR();
      SetAvatarParameters();
    });


  }

  #region melon variables update

  private void OnMeEnableChanged(bool oldValue, bool newValue)
  {
    if(oldValue == newValue)
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
      SetAvatarParameters();
    } 
  }

  private void OnMeMinHRSetChanged(int oldValue, int newValue)
  {
    if(oldValue == newValue)
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
      SetAvatarParameters();
    } 
  }

  private void OnMeMaxHRSetChanged(int oldValue, int newValue)
  {
    if(oldValue == newValue)
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
      SetAvatarParameters();
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
      onesHR = 0;
      tensHR = 0;
      hundredsHR = 0;
      isHRConnected = false;
      isHRActive = false;
      isHRBeat = false;
      HRPercent = 0.0f;
      HR = 0;
    }
    else
    {
      HRtoCVRDisabled = false;
      // at this point no need to send the HR values here, 
      // just the fact that the mod is enabled
    }
  }

  private void SetAvatarParameters()
  {
    // all this function does is batch set the parameters to the animator
    PlayerSetup.Instance.animatorManager.SetParameter("HRtoCVRDisabled", HRtoCVRDisabled);
    PlayerSetup.Instance.animatorManager.SetParameter("onesHR", onesHR);
    PlayerSetup.Instance.animatorManager.SetParameter("tensHR", tensHR);
    PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", hundredsHR);
    PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", isHRConnected);
    PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", isHRActive);
    PlayerSetup.Instance.animatorManager.SetParameter("isHRBeat", isHRBeat);
    PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", HRPercent);
    PlayerSetup.Instance.animatorManager.SetParameter("HR", HR);
  }

  public override void OnApplicationQuit()
  {
    // any on quit code here, ie closing connections etc
  }
}
