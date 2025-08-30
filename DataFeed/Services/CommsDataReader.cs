using ABI_RC.Systems.Communications;
using ABI_RC.Systems.Communications.Networking;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class CommsDataReader : ICommsDataReader
  {
    private int _voiceCommsPing;
    private bool _isVoiceConnected;
    private string _voiceConnectionState;
    private bool _dataFeedErrorComms;

    public int VoiceCommsPing => _voiceCommsPing;
    public bool IsVoiceConnected => _isVoiceConnected;
    public string VoiceConnectionState => _voiceConnectionState;
    public bool DataFeedErrorComms => _dataFeedErrorComms;

    [SuppressMessage(
      "SonarQube",
      "csharpsquid:S3011",
      Justification = "Mod requires access to internal game state for data feed functionality"
    )]
    public bool UpdateCommsState()
    {
      var stateChanged = false;
      var commsManager = Comms_Manager.Instance;

      if (commsManager == null)
      {
        // No comms manager available
        stateChanged |= !_dataFeedErrorComms;
        _dataFeedErrorComms = true;
        stateChanged |= _isVoiceConnected;
        _isVoiceConnected = false;
        stateChanged |= _voiceConnectionState != "Unavailable";
        _voiceConnectionState = "Unavailable";
        stateChanged |= _voiceCommsPing != 0;
        _voiceCommsPing = 0;
        return stateChanged;
      }

      try
      {
        // Check voice connection status
        var currentIsVoiceConnected = Comms_Manager.IsClientConnected;
        stateChanged |= _isVoiceConnected != currentIsVoiceConnected;
        _isVoiceConnected = currentIsVoiceConnected;

        // Update voice connection state
        var currentVoiceConnectionState = currentIsVoiceConnected ? "Connected" : "Disconnected";
        stateChanged |= _voiceConnectionState != currentVoiceConnectionState;
        _voiceConnectionState = currentVoiceConnectionState;

        // Update Voice Comms Ping
        var currentVoiceCommsPing = 0;
        if (currentIsVoiceConnected)
        {
          try
          {
            // Access the internal Client property using reflection
            // This is safe in a mod context - we need access to internal game state
            // to provide voice communications data to external applications
            var managerType = typeof(Comms_Manager);
            var clientProperty = managerType.GetProperty("Client", BindingFlags.NonPublic | BindingFlags.Instance);

            if (clientProperty != null)
            {
              var commsClient = clientProperty.GetValue(commsManager);
              if (commsClient is Comms_Client client)
              {
                currentVoiceCommsPing = client.Ping;
              }
            }
          }
          catch
          {
            currentVoiceCommsPing = 0;
          }
        }

        stateChanged |= _voiceCommsPing != currentVoiceCommsPing;
        _voiceCommsPing = currentVoiceCommsPing;

        // Clear error flag if we successfully read data
        stateChanged |= _dataFeedErrorComms;
        _dataFeedErrorComms = false;
      }
      catch (System.Exception)
      {
        stateChanged |= !_dataFeedErrorComms;
        _dataFeedErrorComms = true;
        _isVoiceConnected = false;
        _voiceConnectionState = "Error";
        _voiceCommsPing = 0;
      }

      return stateChanged;
    }
  }
}
