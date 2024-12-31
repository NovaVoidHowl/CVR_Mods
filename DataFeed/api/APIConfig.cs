namespace uk.novavoidhowl.dev.cvrmods.DataFeed.api
{
  public class ApiConfig
  {
    private int _webSocketPort = 8081;
    private int _restApiPort = 8080;
    public string WebSocketPort
    {
      get => _webSocketPort.ToString();
      set => _webSocketPort = int.Parse(value);
    }

    public string RestApiPort
    {
      get => _restApiPort.ToString();
      set => _restApiPort = int.Parse(value);
    }

    public int WebSocketPortInt => _webSocketPort;
    public int RestApiPortInt => _restApiPort;

    public string ApiKey { get; set; } = Guid.NewGuid().ToString();
  }
}
