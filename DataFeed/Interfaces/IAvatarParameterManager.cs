using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces
{
  public interface IAvatarParameterManager
  {
    void SetParameters(
      WorldRuleParameters worldRules,
      ModStatusParameters modStatus,
      PlatformStateParameters platformState
    );
  }
}
