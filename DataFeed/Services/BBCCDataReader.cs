using ABI_RC.Systems.Movement;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class BetterBetterCharacterControllerDataReader : IBetterBetterCharacterControllerDataReader
  {
    private bool _flyingAllowed;
    private bool _dataFeedErrorBBCC;

    public bool FlyingAllowed => _flyingAllowed;
    public bool DataFeedErrorBBCC => _dataFeedErrorBBCC;

    public bool UpdateBBCCState()
    {
      var stateChanged = false;
      var bbcc = BetterBetterCharacterController.Instance;

      if (bbcc == null)
      {
        stateChanged |= !_dataFeedErrorBBCC;
        _dataFeedErrorBBCC = true;
      }
      else
      {
        stateChanged |= _flyingAllowed != bbcc.FlightAllowedInWorld;
        stateChanged |= _dataFeedErrorBBCC;
        _flyingAllowed = bbcc.FlightAllowedInWorld;
        _dataFeedErrorBBCC = false;
      }

      return stateChanged;
    }
  }
}
