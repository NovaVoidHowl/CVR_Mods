using ABI_RC.Systems.OSC.Modules;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class OSCParameterDataReader : IOSCParameterDataReader
  {
    private const string ParametersAddressPrefix = "/avatar/parameters/";
    private const int MinimumRecentMessageCapacity = 0;
    private const int MaximumRecentMessageCapacity = 2048;
    private const int MinimumArgumentStringLimit = 16;
    private const int MaximumArgumentStringLimit = 4096;

    private readonly object _stateLock = new object();
    private readonly Dictionary<string, OSCParameterInfo> _parameters = new Dictionary<string, OSCParameterInfo>();
    private readonly List<OSCParameterMessageInfo> _recentMessages = new List<OSCParameterMessageInfo>();

    private bool _initialized;
    private bool _stateChanged;
    private bool _dataFeedErrorOSCParameters;
    private int _recentMessageCapacity = 128;
    private bool _verboseArguments;
    private int _argumentStringLimit = 128;

    public IReadOnlyList<OSCParameterInfo> Parameters
    {
      get
      {
        lock (_stateLock)
        {
          return _parameters.Values.OrderBy(parameter => parameter.Name).ToList();
        }
      }
    }

    public IReadOnlyList<OSCParameterMessageInfo> RecentMessages
    {
      get
      {
        lock (_stateLock)
        {
          return _recentMessages.ToList();
        }
      }
    }

    public int KnownParameterCount
    {
      get
      {
        lock (_stateLock)
        {
          return _parameters.Count;
        }
      }
    }

    public int RecentMessageCount
    {
      get
      {
        lock (_stateLock)
        {
          return _recentMessages.Count;
        }
      }
    }

    public bool DataFeedErrorOSCParameters
    {
      get
      {
        lock (_stateLock)
        {
          return _dataFeedErrorOSCParameters;
        }
      }
    }

    public void Initialize()
    {
      lock (_stateLock)
      {
        if (_initialized)
          return;

        OSCAvatarModule.OnIncomingAvatarFloatOSCParameter += OnIncomingAvatarFloatOSCParameter;
        OSCAvatarModule.OnIncomingAvatarIntOSCParameter += OnIncomingAvatarIntOSCParameter;
        OSCAvatarModule.OnIncomingAvatarBoolOSCParameter += OnIncomingAvatarBoolOSCParameter;
        OSCAvatarModule.OnIncomingAvatarNullOSCParameter += OnIncomingAvatarNullOSCParameter;

        _initialized = true;
      }
    }

    public void Dispose()
    {
      lock (_stateLock)
      {
        if (!_initialized)
          return;

        OSCAvatarModule.OnIncomingAvatarFloatOSCParameter -= OnIncomingAvatarFloatOSCParameter;
        OSCAvatarModule.OnIncomingAvatarIntOSCParameter -= OnIncomingAvatarIntOSCParameter;
        OSCAvatarModule.OnIncomingAvatarBoolOSCParameter -= OnIncomingAvatarBoolOSCParameter;
        OSCAvatarModule.OnIncomingAvatarNullOSCParameter -= OnIncomingAvatarNullOSCParameter;

        _initialized = false;
      }
    }

    public void Clear()
    {
      lock (_stateLock)
      {
        if (_parameters.Count == 0 && _recentMessages.Count == 0)
          return;

        _parameters.Clear();
        _recentMessages.Clear();
        _stateChanged = true;
      }
    }

    public void Configure(int recentMessageCapacity, bool verboseArguments, int argumentStringLimit)
    {
      var sanitizedCapacity = Clamp(
        recentMessageCapacity,
        MinimumRecentMessageCapacity,
        MaximumRecentMessageCapacity
      );
      var sanitizedStringLimit = Clamp(argumentStringLimit, MinimumArgumentStringLimit, MaximumArgumentStringLimit);

      lock (_stateLock)
      {
        var changed =
          _recentMessageCapacity != sanitizedCapacity
          || _verboseArguments != verboseArguments
          || _argumentStringLimit != sanitizedStringLimit;

        _recentMessageCapacity = sanitizedCapacity;
        _verboseArguments = verboseArguments;
        _argumentStringLimit = sanitizedStringLimit;
        TrimRecentMessages();

        _stateChanged |= changed;
      }
    }

    public bool UpdateOSCParameterState()
    {
      lock (_stateLock)
      {
        var stateChanged = _stateChanged;
        _stateChanged = false;
        return stateChanged;
      }
    }

    private void OnIncomingAvatarFloatOSCParameter(string parameterName, float value)
    {
      RecordAppliedParameter(parameterName, "float", value);
    }

    private void OnIncomingAvatarIntOSCParameter(string parameterName, int value)
    {
      RecordAppliedParameter(parameterName, "int", value);
    }

    private void OnIncomingAvatarBoolOSCParameter(string parameterName, bool value)
    {
      RecordAppliedParameter(parameterName, "bool", value);
    }

    private void OnIncomingAvatarNullOSCParameter(string parameterName)
    {
      RecordAppliedParameter(parameterName, "null", null);
    }

    private void RecordAppliedParameter(string parameterName, string valueType, object value)
    {
      if (string.IsNullOrEmpty(parameterName))
        return;

      lock (_stateLock)
      {
        try
        {
          var receivedCount = 1L;
          if (_parameters.TryGetValue(parameterName, out var existingParameter))
          {
            receivedCount = existingParameter.ReceivedCount + 1;
          }

          _parameters[parameterName] = new OSCParameterInfo(
            parameterName,
            ParametersAddressPrefix + parameterName,
            valueType,
            value,
            DateTime.UtcNow,
            receivedCount,
            true
          );

          _dataFeedErrorOSCParameters = false;
          _stateChanged = true;
        }
        catch (Exception)
        {
          _dataFeedErrorOSCParameters = true;
          _stateChanged = true;
        }
      }
    }

    private void TrimRecentMessages()
    {
      if (_recentMessages.Count <= _recentMessageCapacity)
        return;

      _recentMessages.RemoveRange(0, _recentMessages.Count - _recentMessageCapacity);
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
      if (value < minimum)
        return minimum;

      return value > maximum ? maximum : value;
    }
  }
}
