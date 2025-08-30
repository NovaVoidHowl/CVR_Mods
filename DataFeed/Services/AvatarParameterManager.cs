using ABI_RC.Core.Player;
using MelonLoader;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class AvatarParameterManager : IAvatarParameterManager
  {
    public void SetParameters(
      WorldRuleParameters worldRules,
      ModStatusParameters modStatus,
      PlatformStateParameters platformState
    )
    {
      if (!modStatus.IsEnabled || !worldRules.AvatarParamSetEnabled)
      {
        SetDefaultParameters();
        MelonLogger.Msg("Avatar Data Feed Disabled, Parameters set to default.");
        return;
      }

      SetActiveParameters(worldRules, modStatus, platformState);
      MelonLogger.Msg("Avatar Parameters Set.");
    }

    private static void SetDefaultParameters()
    {
      var animator = PlayerSetup.Instance.AnimatorManager;
      // main data feed
      animator.SetParameter("flyingAllowed", true);
      animator.SetParameter("propsAllowed", true);
      animator.SetParameter("portalsAllowed", true);
      animator.SetParameter("nameplatesEnabled", true);

      // mod status data feed
      animator.SetParameter("dataFeedErrorBBCC", false);
      animator.SetParameter("dataFeedErrorMetaPort", false);
      animator.SetParameter("dataFeedDisabled", true);
      animator.SetParameter("dataFeedAPIDisabled", true);
    }

    private static void SetActiveParameters(
      WorldRuleParameters worldRules,
      ModStatusParameters modStatus,
      PlatformStateParameters platformState
    )
    {
      var animator = PlayerSetup.Instance.AnimatorManager;

      // World rules - these parameter names match existing avatar implementations
      animator.SetParameter("flyingAllowed", worldRules.FlyingAllowed);
      animator.SetParameter("propsAllowed", worldRules.PropsAllowed);
      animator.SetParameter("portalsAllowed", worldRules.PortalsAllowed);
      animator.SetParameter("nameplatesEnabled", worldRules.NameplatesEnabled);

      // Platform state
      animator.SetParameter("dataFeedErrorBBCC", platformState.DataFeedErrorBBCC);
      animator.SetParameter("dataFeedErrorMetaPort", platformState.DataFeedErrorMetaPort);

      // Mod status
      animator.SetParameter("dataFeedDisabled", modStatus.DataFeedDisabled);
      animator.SetParameter("dataFeedAPIDisabled", modStatus.DataFeedAPIDisabled);
    }
  }
}
