using Newtonsoft.Json;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Models
{
  public class OSCClientInfo
  {
    public OSCClientInfo(
      string serviceId,
      string oscAddress,
      int oscPort,
      string httpAddress,
      int? httpPort,
      bool hasAvatarModule,
      bool hasInputModule,
      bool hasChatBoxModule
    )
    {
      ServiceId = serviceId;
      OscAddress = oscAddress;
      OscPort = oscPort;
      HttpAddress = httpAddress;
      HttpPort = httpPort;
      HasAvatarModule = hasAvatarModule;
      HasInputModule = hasInputModule;
      HasChatBoxModule = hasChatBoxModule;
    }

    [JsonProperty("serviceId")]
    public string ServiceId { get; }

    [JsonProperty("oscAddress")]
    public string OscAddress { get; }

    [JsonProperty("oscPort")]
    public int OscPort { get; }

    [JsonProperty("httpAddress")]
    public string HttpAddress { get; }

    [JsonProperty("httpPort")]
    public int? HttpPort { get; }

    [JsonProperty("hasAvatarModule")]
    public bool HasAvatarModule { get; }

    [JsonProperty("hasInputModule")]
    public bool HasInputModule { get; }

    [JsonProperty("hasChatBoxModule")]
    public bool HasChatBoxModule { get; }
  }
}
