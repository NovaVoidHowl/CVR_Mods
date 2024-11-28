using System;
using System.IO;
using System.Timers;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients
{
  public class TextFileClient : IDisposable
  {
    public const string TextFileClientVersion = "0.1.0";
    private readonly System.Timers.Timer _pollingTimer;
    private System.Timers.Timer _heartBeatTimer;
    private bool _disposed = false;
    private readonly string _filePath;

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

    public TextFileClient(string filePath, int pollingRate)
    {
      _filePath = filePath;
      _pollingTimer = new System.Timers.Timer(pollingRate * 1000); // Polling rate in seconds
      _pollingTimer.Elapsed += (sender, e) => ReadHeartRateFromFile();
      _pollingTimer.AutoReset = true;
      _pollingTimer.Enabled = true;

      MelonLogger.Msg("TextFileClient initialized with file path: " + _filePath);
    }

    public void InitializeClient(int minHR, int maxHR)
    {
      this.minHR = minHR;
      this.maxHR = maxHR;
      MelonLogger.Msg("TextFileClient version: " + TextFileClientVersion);
      MelonLogger.Msg("TextFileClient initialized with minHR: " + minHR + " and maxHR: " + maxHR);
    }

    private void ReadHeartRateFromFile()
    {
      MelonLogger.Msg("Attempting to read heart rate from file: " + _filePath);
      try
      {
        MelonLogger.Msg("Checking if file exists at path: " + _filePath);
        bool fileExists = File.Exists(_filePath);
        MelonLogger.Msg("File exists: " + fileExists);

        if (fileExists)
        {
          MelonLogger.Msg("File found at path: " + _filePath);
          var fileContent = File.ReadAllText(_filePath);
          MelonLogger.Msg("File content read: " + fileContent);
          if (int.TryParse(fileContent, out int hr))
          {
            HR = hr;
            onesHR = HR % 10;
            tensHR = (HR / 10) % 10;
            hundredsHR = (HR / 100) % 10;
            HRPercent = (float)(HR - minHR) / (maxHR - minHR);
            isHRConnected = true;
            isHRActive = true;

            // Update the heart beat timer interval based on HR
            _heartBeatTimer.Interval = 60000.0 / HR; // Interval in milliseconds for each beat

            // Notify that heart rate data has been updated
            OnHeartRateUpdated?.Invoke();
          }
          else
          {
            MelonLogger.Error("Failed to parse heart rate from file content: " + fileContent);
          }
        }
        else
        {
          // log that file not found
          MelonLogger.Error($"Input file not found at: {_filePath}");
        }
      }
      catch (Exception ex)
      {
        // Handle exceptions (e.g., log the error)
        MelonLogger.Error($"Error reading heart rate from file: {ex.Message}");
      }
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
          _heartBeatTimer?.Stop();
          _heartBeatTimer?.Dispose();
          _pollingTimer?.Stop();
          _pollingTimer?.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~TextFileClient()
    {
      Dispose(false);
    }
  }
}
