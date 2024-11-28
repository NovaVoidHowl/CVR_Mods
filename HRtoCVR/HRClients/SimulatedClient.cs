using System;
using System.Timers;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients
{
  public class SimulatedClient : IDisposable
  {
    public const string SimulatedClientVersion = "0.1.0";
    private readonly System.Timers.Timer _simulationTimer;
    private System.Timers.Timer _heartBeatTimer;
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
    public event Action OnHeartRateRapidUpdated;

    private int minHR;
    private int maxHR;

    public SimulatedClient()
    {
      _random = new Random();
      _simulationTimer = new System.Timers.Timer(1000); // Simulate HR data every second
      _simulationTimer.Elapsed += (sender, e) => SimulateHeartRate();
      _simulationTimer.AutoReset = true;
      _simulationTimer.Enabled = true;
    }

    public void InitializeClient(int minHR, int maxHR)
    {
      this.minHR = minHR;
      this.maxHR = maxHR;
      MelonLogger.Msg("SimulatedClient version: " + SimulatedClientVersion);
      MelonLogger.Msg("SimulatedClient initialized with minHR: " + minHR + " and maxHR: " + maxHR);
    }

    private void SimulateHeartRate()
    {
      HR = _random.Next(60, 100); // Simulate HR between 60 and 100
      onesHR = HR % 10;
      tensHR = (HR / 10) % 10;
      hundredsHR = (HR / 100) % 10;
      HRPercent = (float)(HR - minHR) / (maxHR - minHR);
      isHRConnected = true;
      isHRActive = true;

      // Update the heart beat timer interval based on HR
      if (_heartBeatTimer == null)
      {
        InitializeHeartBeatTimer();
      }
      _heartBeatTimer.Interval = 60000.0 / HR; // Interval in milliseconds for each beat

      // Notify that heart rate data has been updated
      OnHeartRateUpdated?.Invoke();
    }

    public void InitializeHeartBeatTimer()
    {
      _heartBeatTimer = new System.Timers.Timer();
      _heartBeatTimer.Elapsed += (sender, e) => ToggleHeartBeat();
      _heartBeatTimer.AutoReset = true;
      _heartBeatTimer.Enabled = true;
    }

    private void ToggleHeartBeat()
    {
      isHRBeat = !isHRBeat;
      OnHeartRateRapidUpdated?.Invoke();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          _simulationTimer?.Stop();
          _simulationTimer?.Dispose();
          _heartBeatTimer?.Stop();
          _heartBeatTimer?.Dispose();
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
