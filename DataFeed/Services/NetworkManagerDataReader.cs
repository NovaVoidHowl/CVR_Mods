using ABI_RC.Core.Networking;
using DarkRift;
using DarkRift.Client;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using UnityEngine;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class NetworkManagerDataReader : INetworkManagerDataReader
  {
    private int _gameNetworkPing;
    private bool _isConnected;
    private string _connectionState;
    private bool _dataFeedErrorNetworkManager;

    public int GameNetworkPing => _gameNetworkPing;
    public bool IsConnected => _isConnected;
    public string ConnectionState => _connectionState;
    public bool DataFeedErrorNetworkManager => _dataFeedErrorNetworkManager;

    public bool UpdateNetworkManagerState()
    {
      var stateChanged = false;
      var networkManager = NetworkManager.Instance;

      if (networkManager?.GameNetwork == null)
      {
        stateChanged |= !_dataFeedErrorNetworkManager;
        _dataFeedErrorNetworkManager = true;
        _isConnected = false;
        _connectionState = "Unknown";
        _gameNetworkPing = 0;
        return stateChanged;
      }

      try
      {
        // Check if GameNetwork.Client is available (more reliable than just ConnectionState)
        var gameNetwork = networkManager.GameNetwork;
        var gameNetworkClient = gameNetwork?.Client;
        
        if (gameNetworkClient == null)
        {
          // No client means definitely not connected
          stateChanged |= _isConnected;
          _isConnected = false;
          stateChanged |= _connectionState != "Disconnected";
          _connectionState = "Disconnected";
          stateChanged |= _gameNetworkPing != 0;
          _gameNetworkPing = 0;
        }
        else
        {
          // Update connection state
          var currentConnectionState = gameNetwork.ConnectionState;
          var currentConnectionStateString = currentConnectionState.ToString();
          var currentIsConnected = currentConnectionStateString == "Connected";
          
          stateChanged |= _connectionState != currentConnectionStateString;
          _connectionState = currentConnectionStateString;
          stateChanged |= _isConnected != currentIsConnected;
          _isConnected = currentIsConnected;

          // Update ping - only get it if we're actually connected
          var currentPing = 0;
          if (currentIsConnected)
          {
            try
            {
              currentPing = networkManager.GameNetworkPing;
            }
            catch
            {
              currentPing = 0;
            }
          }
          
          stateChanged |= _gameNetworkPing != currentPing;
          _gameNetworkPing = currentPing;
        }

        // Clear error flag if we successfully read data
        stateChanged |= _dataFeedErrorNetworkManager;
        _dataFeedErrorNetworkManager = false;
      }
      catch (System.Exception)
      {
        stateChanged |= !_dataFeedErrorNetworkManager;
        _dataFeedErrorNetworkManager = true;
        _isConnected = false;
        _connectionState = "Error";
        _gameNetworkPing = 0;
      }

      return stateChanged;
    }
  }
}
