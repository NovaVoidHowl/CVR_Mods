using Newtonsoft.Json;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Models
{
  public class OSCParameterArgumentInfo
  {
    public OSCParameterArgumentInfo(string valueType, object value)
    {
      ValueType = valueType;
      Value = value;
    }

    [JsonProperty("valueType")]
    public string ValueType { get; }

    [JsonProperty("value")]
    public object Value { get; }
  }
}
