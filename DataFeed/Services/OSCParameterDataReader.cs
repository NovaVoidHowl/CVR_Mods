using ABI_RC.Systems.OSC.Modules;
using HarmonyLib;
using LucHeart.CoreOSC;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Models;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class OSCParameterDataReader : IOSCParameterDataReader
  {
    private const string ParametersAddressPrefix = "/avatar/parameters/";
    private const string HarmonyId = "uk.novavoidhowl.dev.cvrmods.datafeed.oscparameters";
    private const int MinimumRecentMessageCapacity = 0;
    private const int MaximumRecentMessageCapacity = 2048;
    private const int MinimumArgumentStringLimit = 16;
    private const int MaximumArgumentStringLimit = 4096;

    private static readonly object ActiveReaderLock = new object();
    private static OSCParameterDataReader _activeReader;

    private readonly object _stateLock = new object();
    private readonly Dictionary<string, OSCParameterInfo> _parameters = new Dictionary<string, OSCParameterInfo>();
    private readonly List<OSCParameterMessageRecord> _recentMessages = new List<OSCParameterMessageRecord>();

    private HarmonyLib.Harmony _harmony;
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
          return _recentMessages.Select(BuildMessageInfo).ToList();
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

        lock (ActiveReaderLock)
        {
          _activeReader = this;
        }

        InstallHarmonyPatch();

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

        _harmony?.UnpatchSelf();
        _harmony = null;

        lock (ActiveReaderLock)
        {
          if (ReferenceEquals(_activeReader, this))
            _activeReader = null;
        }

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

    private void InstallHarmonyPatch()
    {
      var original = AccessTools.Method(
        typeof(OSCAvatarModule),
        nameof(OSCAvatarModule.HandleIncoming),
        new[] { typeof(OscMessage) }
      );
      var prefix = AccessTools.Method(typeof(OSCParameterDataReader), nameof(HandleIncomingPrefix));
      var postfix = AccessTools.Method(typeof(OSCParameterDataReader), nameof(HandleIncomingPostfix));

      if (original == null || prefix == null || postfix == null)
      {
        _dataFeedErrorOSCParameters = true;
        _stateChanged = true;
        return;
      }

      _harmony = new HarmonyLib.Harmony(HarmonyId);
      _harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
    }

    private static void HandleIncomingPrefix(OscMessage packet, out OSCParameterMessageCapture __state)
    {
      __state = null;

      if (packet == null || string.IsNullOrEmpty(packet.Address))
        return;

      if (!packet.Address.StartsWith(ParametersAddressPrefix, StringComparison.OrdinalIgnoreCase))
        return;

      __state = new OSCParameterMessageCapture(
        packet.Address,
        packet.Address.Substring(ParametersAddressPrefix.Length),
        packet.Arguments ?? new object[0],
        DateTime.UtcNow
      );
    }

    private static void HandleIncomingPostfix(bool __result, OSCParameterMessageCapture __state)
    {
      if (__state == null)
        return;

      OSCParameterDataReader reader;
      lock (ActiveReaderLock)
      {
        reader = _activeReader;
      }

      reader?.RecordRawMessage(__state, __result);
    }

    private void RecordRawMessage(OSCParameterMessageCapture capture, bool handledByCVR)
    {
      lock (_stateLock)
      {
        try
        {
          AddRecentMessage(
            new OSCParameterMessageRecord(
              capture.Address,
              capture.ParameterName,
              capture.Arguments,
              capture.ReceivedAt,
              handledByCVR,
              false,
              GetRawMessageReason(capture, handledByCVR)
            )
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

          MarkRecentMessageApplied(parameterName, valueType, value);

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

    private void AddRecentMessage(OSCParameterMessageRecord message)
    {
      if (_recentMessageCapacity == 0)
        return;

      _recentMessages.Add(message);
      TrimRecentMessages();
    }

    private void MarkRecentMessageApplied(string parameterName, string valueType, object value)
    {
      for (var i = _recentMessages.Count - 1; i >= 0; i--)
      {
        var message = _recentMessages[i];
        if (message.AppliedToAvatar)
          continue;

        if (!string.Equals(message.ParameterName, parameterName, StringComparison.Ordinal))
          continue;

        if (!MessageValueMatches(message, valueType, value))
          continue;

        _recentMessages[i] = message.WithApplied("Applied to current avatar");
        return;
      }
    }

    private bool MessageValueMatches(OSCParameterMessageRecord message, string valueType, object value)
    {
      if (message.Arguments.Count == 0)
        return valueType == "null";

      if (message.Arguments.Count != 1)
        return false;

      return ArgumentValueMatches(message.Arguments[0], valueType, value);
    }

    private bool ArgumentValueMatches(object argument, string valueType, object value)
    {
      switch (valueType)
      {
        case "bool":
          return argument is bool boolValue && value is bool appliedBool && boolValue == appliedBool;
        case "int":
          return argument is int intValue && value is int appliedInt && intValue == appliedInt;
        case "float":
          return argument is float floatValue && value is float appliedFloat && Math.Abs(floatValue - appliedFloat) < 0.0001f;
        case "null":
          return argument == null && value == null;
        default:
          return false;
      }
    }

    private string GetRawMessageReason(OSCParameterMessageCapture capture, bool handledByCVR)
    {
      if (handledByCVR)
        return "Accepted by CVR avatar OSC handler; avatar application not confirmed";

      if (capture.Arguments.Count > 1)
        return "Unsupported argument count";

      if (capture.Arguments.Count == 1 && !IsSupportedAvatarParameterArgument(capture.Arguments[0]))
        return "Unsupported argument type";

      return "Rejected by CVR avatar OSC handler";
    }

    private static bool IsSupportedAvatarParameterArgument(object argument)
    {
      return argument is bool || argument is int || argument is float;
    }

    private OSCParameterMessageInfo BuildMessageInfo(OSCParameterMessageRecord message)
    {
      return new OSCParameterMessageInfo(
        message.Address,
        message.ParameterName,
        message.Arguments.Count,
        message.Arguments.Select(BuildArgumentInfo).ToList(),
        message.ReceivedAt,
        message.HandledByCVR,
        message.AppliedToAvatar,
        message.Reason
      );
    }

    private OSCParameterArgumentInfo BuildArgumentInfo(object argument)
    {
      return new OSCParameterArgumentInfo(GetValueType(argument), GetSerializableValue(argument));
    }

    private object GetSerializableValue(object value)
    {
      if (value == null)
        return null;

      if (value is string stringValue)
        return _verboseArguments ? stringValue : TruncateString(stringValue, _argumentStringLimit);

      if (value is bool || value is int || value is float || value is double || value is long)
        return value;

      return _verboseArguments
        ? value.ToString()
        : TruncateString(value.ToString(), _argumentStringLimit);
    }

    private static string GetValueType(object value)
    {
      if (value == null)
        return "null";

      if (value is bool)
        return "bool";

      if (value is int)
        return "int";

      if (value is float)
        return "float";

      if (value is string)
        return "string";

      return value.GetType().Name;
    }

    private static string TruncateString(string value, int maximumLength)
    {
      if (value == null || value.Length <= maximumLength)
        return value;

      return value.Substring(0, maximumLength) + "...";
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

    private class OSCParameterMessageCapture
    {
      public OSCParameterMessageCapture(
        string address,
        string parameterName,
        IReadOnlyList<object> arguments,
        DateTime receivedAt
      )
      {
        Address = address;
        ParameterName = parameterName;
        Arguments = arguments.ToList();
        ReceivedAt = receivedAt;
      }

      public string Address { get; }
      public string ParameterName { get; }
      public IReadOnlyList<object> Arguments { get; }
      public DateTime ReceivedAt { get; }
    }

    private class OSCParameterMessageRecord
    {
      public OSCParameterMessageRecord(
        string address,
        string parameterName,
        IReadOnlyList<object> arguments,
        DateTime receivedAt,
        bool handledByCVR,
        bool appliedToAvatar,
        string reason
      )
      {
        Address = address;
        ParameterName = parameterName;
        Arguments = arguments.ToList();
        ReceivedAt = receivedAt;
        HandledByCVR = handledByCVR;
        AppliedToAvatar = appliedToAvatar;
        Reason = reason;
      }

      public string Address { get; }
      public string ParameterName { get; }
      public IReadOnlyList<object> Arguments { get; }
      public DateTime ReceivedAt { get; }
      public bool HandledByCVR { get; }
      public bool AppliedToAvatar { get; }
      public string Reason { get; }

      public OSCParameterMessageRecord WithApplied(string reason)
      {
        return new OSCParameterMessageRecord(
          Address,
          ParameterName,
          Arguments,
          ReceivedAt,
          HandledByCVR,
          true,
          reason
        );
      }
    }
  }
}
