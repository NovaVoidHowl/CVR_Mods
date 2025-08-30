using ABI_RC.Core.Savior;
using uk.novavoidhowl.dev.cvrmods.DataFeed.Interfaces;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Services
{
  public class MetaPortDataReader : IMetaPortDataReader
  {
    private string _currentInstanceId;
    private string _currentInstanceName;
    private string _currentWorldId;
    private string _currentInstancePrivacy;
    private int _buildId;
    private string _hardwareId;
    private bool _propsAllowed;
    private bool _portalsAllowed;
    private bool _nameplatesEnabled;
    private bool _dataFeedErrorMetaPort;

    public string CurrentInstanceId => _currentInstanceId;
    public string CurrentInstanceName => _currentInstanceName;
    public string CurrentWorldId => _currentWorldId;
    public string CurrentInstancePrivacy => _currentInstancePrivacy;
    public int BuildId => _buildId;
    public string HardwareId => _hardwareId;
    public bool PropsAllowed => _propsAllowed;
    public bool PortalsAllowed => _portalsAllowed;
    public bool NameplatesEnabled => _nameplatesEnabled;
    public bool DataFeedErrorMetaPort => _dataFeedErrorMetaPort;

    public bool UpdateMetaPortState()
    {
      var stateChanged = false;
      var metaPort = MetaPort.Instance;

      if (metaPort == null)
      {
        stateChanged |= !_dataFeedErrorMetaPort;
        _dataFeedErrorMetaPort = true;
        return stateChanged;
      }

      stateChanged |= UpdateMetaPortWorldSettings();
      UpdateInstanceInfo();

      _buildId = metaPort.buildId;
      _hardwareId = metaPort.hardwareId;
      _dataFeedErrorMetaPort = false;

      return stateChanged;
    }

    public void UpdateInstanceInfo()
    {
      var metaPort = MetaPort.Instance;
      if (metaPort == null)
        return;

      _currentInstanceId = metaPort.CurrentInstanceId;
      _currentInstanceName = metaPort.CurrentInstanceName;
      _currentWorldId = metaPort.CurrentWorldId;
      _currentInstancePrivacy = metaPort.CurrentInstancePrivacy;
    }

    public bool UpdateMetaPortWorldSettings()
    {
      var metaPort = MetaPort.Instance;
      if (metaPort == null)
        return false;

      var stateChanged = false;
      stateChanged |= _propsAllowed != metaPort.worldAllowProps;
      stateChanged |= _portalsAllowed != metaPort.worldAllowPortals;
      stateChanged |= _nameplatesEnabled != metaPort.worldEnableNameplates;

      _propsAllowed = metaPort.worldAllowProps;
      _portalsAllowed = metaPort.worldAllowPortals;
      _nameplatesEnabled = metaPort.worldEnableNameplates;

      return stateChanged;
    }
  }
}
