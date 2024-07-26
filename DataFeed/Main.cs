using ABI_RC.Systems.InputManagement;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed;

public class DataFeed : MelonMod
{
  private MelonPreferences_Category _MelonCategoryDataFeed;

  // Core Mellon Loader Vars
  public MelonPreferences_Entry<bool> meEnable;

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
  }

  public override void OnApplicationQuit()
  {
    // any on quit code here, ie closing connections etc
  }
}
