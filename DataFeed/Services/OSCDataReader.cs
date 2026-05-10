using System.Net;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.OSC;
using ABI_RC.Systems.OSC.OSCQuery;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class OSCDataReader : IOSCDataReader
  {
    private const int DefaultInboundPort = 9000;
    private const int DefaultOutboundPort = 9001;
    private const string DefaultInboundAddress = "0.0.0.0";
    private const string OSCEnabledSettingName = "ImplementationOSCServerEnabled";
    private const string OSCVerboseLoggingEnabledSettingName = "ImplementationOSCVerboseLoggingEnabled";

    private bool _oscEnabled;
    private bool _oscRunning;
    private bool _oscVerboseLogging;
    private string _inboundAddress = DefaultInboundAddress;
    private int _inboundPort = DefaultInboundPort;
    private string _outboundAddress = IPAddress.Loopback.ToString();
    private int _outboundPort = DefaultOutboundPort;
    private string _oscQueryServiceName = string.Empty;
    private int _connectedOSCClients;
    private IReadOnlyList<OSCClientInfo> _oscClients = new List<OSCClientInfo>();
    private bool _dataFeedErrorOSC;

    public bool OSCEnabled => _oscEnabled;
    public bool OSCRunning => _oscRunning;
    public bool OSCVerboseLogging => _oscVerboseLogging;
    public string InboundAddress => _inboundAddress;
    public int InboundPort => _inboundPort;
    public string OutboundAddress => _outboundAddress;
    public int OutboundPort => _outboundPort;
    public string OSCQueryServiceName => _oscQueryServiceName;
    public int ConnectedOSCClients => _connectedOSCClients;
    public IReadOnlyList<OSCClientInfo> OSCClients => _oscClients;
    public bool DataFeedErrorOSC => _dataFeedErrorOSC;

    public bool UpdateOSCState()
    {
      var stateChanged = false;

      try
      {
        var metaPortSettings = MetaPort.Instance?.settings;
        var checkVR = CheckVR.Instance;

        var currentOSCEnabled = metaPortSettings?.GetSettingsBool(OSCEnabledSettingName) ?? false;
        var currentOSCVerboseLogging = metaPortSettings?.GetSettingsBool(OSCVerboseLoggingEnabledSettingName) ?? false;

        var fallbackInboundPort =
          checkVR?.oscListenerPort == -1 ? DefaultInboundPort : checkVR?.oscListenerPort ?? DefaultInboundPort;
        var fallbackOutboundAddress = (checkVR?.OscSenderIp ?? IPAddress.Loopback).ToString();
        var fallbackOutboundPort =
          checkVR?.oscSenderPort == -1 ? DefaultOutboundPort : checkVR?.oscSenderPort ?? DefaultOutboundPort;

        var server = OSCServer._instance;
        var currentOSCRunning = OSCServer.IsRunning;
        var listenerEndpoint = server?._listenerEndpoint;
        var senderEndpoint = server?._senderEndpoint;

        var currentInboundAddress = listenerEndpoint?.Address?.ToString() ?? DefaultInboundAddress;
        var currentInboundPort = listenerEndpoint?.Port ?? fallbackInboundPort;
        var currentOutboundAddress = senderEndpoint?.Address?.ToString() ?? fallbackOutboundAddress;
        var currentOutboundPort = senderEndpoint?.Port ?? fallbackOutboundPort;
        var currentOSCQueryServiceName = OSCServer._oscQueryServerServiceName ?? string.Empty;
        var currentOSCClients = BuildOSCClientList();
        var currentConnectedOSCClients = currentOSCClients.Count;

        stateChanged |= _oscEnabled != currentOSCEnabled;
        stateChanged |= _oscRunning != currentOSCRunning;
        stateChanged |= _oscVerboseLogging != currentOSCVerboseLogging;
        stateChanged |= _inboundAddress != currentInboundAddress;
        stateChanged |= _inboundPort != currentInboundPort;
        stateChanged |= _outboundAddress != currentOutboundAddress;
        stateChanged |= _outboundPort != currentOutboundPort;
        stateChanged |= _oscQueryServiceName != currentOSCQueryServiceName;
        stateChanged |= _connectedOSCClients != currentConnectedOSCClients;
        stateChanged |= !OSCClientListsEqual(_oscClients, currentOSCClients);
        stateChanged |= _dataFeedErrorOSC;

        _oscEnabled = currentOSCEnabled;
        _oscRunning = currentOSCRunning;
        _oscVerboseLogging = currentOSCVerboseLogging;
        _inboundAddress = currentInboundAddress;
        _inboundPort = currentInboundPort;
        _outboundAddress = currentOutboundAddress;
        _outboundPort = currentOutboundPort;
        _oscQueryServiceName = currentOSCQueryServiceName;
        _connectedOSCClients = currentConnectedOSCClients;
        _oscClients = currentOSCClients;
        _dataFeedErrorOSC = false;
      }
      catch (Exception)
      {
        stateChanged |= !_dataFeedErrorOSC;
        _dataFeedErrorOSC = true;
        _oscEnabled = false;
        _oscRunning = false;
        _oscVerboseLogging = false;
        _connectedOSCClients = 0;
        _oscClients = new List<OSCClientInfo>();
      }

      return stateChanged;
    }

    private static IReadOnlyList<OSCClientInfo> BuildOSCClientList()
    {
      var clients = new List<OSCClientInfo>();
      var foundOSCClients = OSCServer.OSCQueryServer?.FoundOSCClients;

      foreach (var connectedClient in OSCServer.ConnectedClients)
      {
        var serviceId = connectedClient.Key;
        var clientInfo = connectedClient.Value;
        var senderEndpoint = clientInfo?.SenderEndpoint;

        OscQueryServer.ClientInfo foundClient = null;
        foundOSCClients?.TryGetValue(serviceId, out foundClient);

        clients.Add(
          new OSCClientInfo(
            serviceId,
            senderEndpoint?.Address?.ToString(),
            senderEndpoint?.Port ?? 0,
            foundClient?.HttpServerIpEndpoint?.Address?.ToString(),
            foundClient?.HttpServerIpEndpoint?.Port,
            clientInfo?.HasAvatarModule ?? false,
            clientInfo?.HasInputModule ?? false,
            clientInfo?.Node?.Contents?.ChatBox != null
          )
        );
      }

      return clients.OrderBy(client => client.ServiceId).ToList();
    }

    private static bool OSCClientListsEqual(IReadOnlyList<OSCClientInfo> current, IReadOnlyList<OSCClientInfo> next)
    {
      if (current.Count != next.Count)
      {
        return false;
      }

      for (var i = 0; i < current.Count; i++)
      {
        if (!OSCClientEquals(current[i], next[i]))
        {
          return false;
        }
      }

      return true;
    }

    private static bool OSCClientEquals(OSCClientInfo current, OSCClientInfo next)
    {
      return current.ServiceId == next.ServiceId
        && current.OscAddress == next.OscAddress
        && current.OscPort == next.OscPort
        && current.HttpAddress == next.HttpAddress
        && current.HttpPort == next.HttpPort
        && current.HasAvatarModule == next.HasAvatarModule
        && current.HasInputModule == next.HasInputModule
        && current.HasChatBoxModule == next.HasChatBoxModule;
    }
  }
}
