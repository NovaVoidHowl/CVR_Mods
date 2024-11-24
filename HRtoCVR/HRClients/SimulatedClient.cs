using System;
using System.Timers;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients
{
  public class SimulatedClient : IDisposable
  {
    private readonly System.Timers.Timer _simulationTimer;
    private readonly Random _random;
    private bool _disposed = false;

    public int HR { get; private set; }
    public bool isHRConnected { get; private set; }
    public bool isHRActive { get; private set; }
    public bool isHRBeat { get; private set; }
    public float HRPercent { get; private set; }
    public int onesHR { get; private set; }
    public int tensHR { get; private set; }
    public int hundredsHR { get; private set; }
    public event Action OnHeartRateUpdated;

    public SimulatedClient()
    {
      _random = new Random();
      _simulationTimer = new System.Timers.Timer(1000); // Simulate HR data every second
      _simulationTimer.Elapsed += (sender, e) => SimulateHeartRate();
      _simulationTimer.AutoReset = true;
      _simulationTimer.Enabled = true;
    }

    private void SimulateHeartRate()
    {
      HR = _random.Next(60, 100); // Simulate HR between 60 and 100
      onesHR = HR % 10;
      tensHR = (HR / 10) % 10;
      hundredsHR = (HR / 100) % 10;
      HRPercent = (float)(HR - 60) / (100 - 60);
      isHRConnected = true;
      isHRActive = true;
      isHRBeat = !isHRBeat;

      // Notify that heart rate data has been updated
      OnHeartRateUpdated?.Invoke();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          _simulationTimer?.Stop();
          _simulationTimer?.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~SimulatedClient()
    {
      Dispose(false);
    }
  }
}
