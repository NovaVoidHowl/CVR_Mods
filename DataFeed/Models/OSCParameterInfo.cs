using Newtonsoft.Json;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Models
{
  public class OSCParameterInfo
  {
    public OSCParameterInfo(
      string name,
      string address,
      string valueType,
      object lastValue,
      DateTime lastReceivedAt,
      long receivedCount,
      bool appliedToAvatar
    )
    {
      Name = name;
      Address = address;
      ValueType = valueType;
      LastValue = lastValue;
      LastReceivedAt = lastReceivedAt;
      ReceivedCount = receivedCount;
      AppliedToAvatar = appliedToAvatar;
    }

    [JsonProperty("name")]
    public string Name { get; }

    [JsonProperty("address")]
    public string Address { get; }

    [JsonProperty("valueType")]
    public string ValueType { get; }

    [JsonProperty("lastValue")]
    public object LastValue { get; }

    [JsonProperty("lastReceivedAt")]
    public DateTime LastReceivedAt { get; }

    [JsonProperty("receivedCount")]
    public long ReceivedCount { get; }

    [JsonProperty("appliedToAvatar")]
    public bool AppliedToAvatar { get; }
  }
}
