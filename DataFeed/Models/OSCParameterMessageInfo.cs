using Newtonsoft.Json;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Models
{
  public class OSCParameterMessageInfo
  {
    public OSCParameterMessageInfo(
      string address,
      string parameterName,
      int argumentCount,
      IReadOnlyList<OSCParameterArgumentInfo> arguments,
      DateTime receivedAt,
      bool handledByCVR,
      bool appliedToAvatar,
      string reason
    )
    {
      Address = address;
      ParameterName = parameterName;
      ArgumentCount = argumentCount;
      Arguments = arguments;
      ReceivedAt = receivedAt;
      HandledByCVR = handledByCVR;
      AppliedToAvatar = appliedToAvatar;
      Reason = reason;
    }

    [JsonProperty("address")]
    public string Address { get; }

    [JsonProperty("parameterName")]
    public string ParameterName { get; }

    [JsonProperty("argumentCount")]
    public int ArgumentCount { get; }

    [JsonProperty("arguments")]
    public IReadOnlyList<OSCParameterArgumentInfo> Arguments { get; }

    [JsonProperty("receivedAt")]
    public DateTime ReceivedAt { get; }

    [JsonProperty("handledByCVR")]
    public bool HandledByCVR { get; }

    [JsonProperty("appliedToAvatar")]
    public bool AppliedToAvatar { get; }

    [JsonProperty("reason")]
    public string Reason { get; }
  }
}
