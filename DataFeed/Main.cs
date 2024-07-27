using ABI_RC.Core.Savior;
using ABI_RC.Systems.GameEventSystem;
using ABI.CCK.Components;
using ABI_RC.Systems.Movement;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed;

public class DataFeed : MelonMod
{
  private MelonPreferences_Category _MelonCategoryDataFeed;

  // Core Mellon Loader Vars
  public MelonPreferences_Entry<bool> meEnable;


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
    _MelonCategoryDataFeed = MelonPreferences.CreateCategory(nameof(DataFeed));
    // Core Mod Options
    meEnable = _MelonCategoryDataFeed.CreateEntry(
      "Enable",
      true,
      description: "Enables or disables the data feed, note values will default to true when disabled ."
    );

    // Event Listeners MelonLoader
    meEnable.OnEntryValueChanged.Subscribe((oldValue, newValue) => {
      OnMeEnableChanged(oldValue, newValue); 
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
    });

    CVRGameEventSystem.Avatar.OnLocalAvatarLoad.AddListener((CVRAvatar avatar) => {
      // get the game object of the avatar
      MelonLogger.Msg("On Local Avatar Load: " + avatar.gameObject.name);
      UpdateDataFeed();
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

      // Log the values to the melon logger
      MelonLogger.Msg("Data Feed:");
      MelonLogger.Msg("flyingAllowed: " + flyingAllowed);
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
