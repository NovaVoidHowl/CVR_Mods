using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class FPSDataReader : IFPSDataReader
  {
    private int _currentFPS;

    public int CurrentFPS => _currentFPS;

    public bool UpdateFPS()
    {
      // Calculate FPS using the same method as the game menu
      var currentFPS = (int)Mathf.Floor(1f / Time.deltaTime);

      if (_currentFPS != currentFPS)
      {
        _currentFPS = currentFPS;
        return true;
      }

      return false;
    }
  }
}
