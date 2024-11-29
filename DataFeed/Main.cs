using ABI_RC.Core.Savior;
using ABI_RC.Core.Player;
using ABI_RC.Systems.GameEventSystem;
using ABI.CCK.Components;
using ABI_RC.Systems.Movement;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed
{
  public class DataFeed : MelonMod
  {

    // Core Mellon Loader Vars
    public MelonPreferences_Entry<bool> meEnable;
    public MelonPreferences_Entry<bool> meAvatarParamSetEnabled;

    // Values to be fed
    private bool flyingAllowed;
    private bool propsAllowed;
    private bool portalsAllowed;
    private bool nameplatesEnabled;

    private bool dataFeedErrorBBCC;
    private bool dataFeedErrorMetaPort;
    private bool dataFeedDisabled;

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


      // Event Listeners MelonLoader
      meEnable.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
        OnMeEnableChanged(oldValue, newValue); 
      });
      meAvatarParamSetEnabled.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
        OnMeAvatarParamSetEnabledChanged(oldValue, newValue); 
      });

      // Event Listeners CVR, 
      // Note: more of these these can be found in the CVRGameEventSystem class under the
      // ABI_RC.Systems.GameEventSystem namespace if needed

      CVRGameEventSystem.Initialization.OnPlayerSetupStart.AddListener(() => {
        MelonLogger.Msg("Player Setup Start");
        UpdateDataFeed();
      });

      CVRGameEventSystem.Instance.OnConnected.AddListener((string message) => {
        MelonLogger.Msg("On Instance Load: " + message);
        UpdateDataFeed();
        PrintCurrentDataFeedValues();
        SetAvatarParameters();
      });

      CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener((CVRAvatar avatar) => {
        // get the game object of the avatar
        MelonLogger.Msg("On Local Avatar Load: " + avatar.gameObject.name);
        UpdateDataFeed();
        PrintCurrentDataFeedValues();
        SetAvatarParameters();
      });


    }

    private void OnMeEnableChanged(bool oldValue, bool newValue)
    {
      if(oldValue == newValue)
      {
        // no change in value
        #if DEBUG
        MelonLogger.Msg("Data Feed Enable parameter event triggered, but has not changed.");
        #endif
      }
      else
      {
        MelonLogger.Msg("Data Feed Enabled: " + newValue);
        UpdateDataFeed();
        SetAvatarParameters();
      } 
    }

    private void OnMeAvatarParamSetEnabledChanged(bool oldValue, bool newValue)
    {
      if(oldValue == newValue)
      {
        // no change in value
        #if DEBUG
        MelonLogger.Msg("Avatar Parameter Output Enabled parameter event triggered, but has not changed.");
        #endif
      }
      else
      {
        MelonLogger.Msg("Avatar Parameter Output Enabled: " + newValue);
        UpdateDataFeed();
        SetAvatarParameters();
      } 
    }

    private void UpdateDataFeed()
    {
        // check if the mod is enabled
        if (!meEnable.Value)
        {
          // if disabled, set all values to true
          flyingAllowed = true;
          propsAllowed = true;
          portalsAllowed = true;
          nameplatesEnabled = true;
          dataFeedDisabled = true;
        }
        else
        {
          dataFeedDisabled = false;
          // check if BetterBetterCharacterController.Instance is null
          if (BetterBetterCharacterController.Instance == null)
          {
            MelonLogger.Error("BetterBetterCharacterController.Instance is null, cannot access data.");
            dataFeedErrorBBCC = true;
          }
          else
          {
            // if enabled, set values to the current state from the BBCC
            flyingAllowed = BetterBetterCharacterController.Instance.FlightAllowedInWorld;
            dataFeedErrorBBCC = false;
          }

          // check if MetaPort.Instance is null
          if (MetaPort.Instance == null)
          {
            MelonLogger.Error("MetaPort.Instance is null, cannot access data.");
            dataFeedErrorMetaPort = true;
          }
          else
          {
            // if enabled, set values to the current state from the MetaPort
            propsAllowed = MetaPort.Instance.worldAllowProps;
            portalsAllowed = MetaPort.Instance.worldAllowPortals;
            nameplatesEnabled = MetaPort.Instance.worldEnableNameplates;
            dataFeedErrorMetaPort = false;
          }
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
        PlayerSetup.Instance.animatorManager.SetParameter("flyingAllowed", flyingAllowed);
        PlayerSetup.Instance.animatorManager.SetParameter("propsAllowed", propsAllowed);
        PlayerSetup.Instance.animatorManager.SetParameter("portalsAllowed", portalsAllowed);
        PlayerSetup.Instance.animatorManager.SetParameter("nameplatesEnabled", nameplatesEnabled);

        // mod status data feed
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedErrorBBCC", dataFeedErrorBBCC);
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedErrorMetaPort", dataFeedErrorMetaPort);
        PlayerSetup.Instance.animatorManager.SetParameter("dataFeedDisabled", dataFeedDisabled);

        MelonLogger.Msg("Avatar Parameters Set.");
      }
    }


    private void PrintCurrentDataFeedValues()
    {

        // Log the values to the melon logger
        MelonLogger.Msg("Data Feed:");
        MelonLogger.Msg("FlyingAllowed: " + flyingAllowed);
        MelonLogger.Msg("PropsAllowed: " + propsAllowed);
        MelonLogger.Msg("PortalsAllowed: " + portalsAllowed);
        MelonLogger.Msg("NameplatesEnabled: " + nameplatesEnabled);
        MelonLogger.Msg("Data Feed Status:");
        MelonLogger.Msg("BBCC Error: " + dataFeedErrorBBCC);
        MelonLogger.Msg("MetaPort Error: " + dataFeedErrorMetaPort);
        MelonLogger.Msg("Data Feed Disabled: " + dataFeedDisabled);
    }

    public override void OnApplicationQuit()
    {
      // any on quit code here, ie closing connections etc
    }
  }
}