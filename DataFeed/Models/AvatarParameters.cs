namespace uk.novavoidhowl.dev.cvrmods.DataFeed.Models
{
  public class WorldRuleParameters
  {
    private bool _avatarParamSetEnabled;
    private bool _flyingAllowed;
    private bool _propsAllowed;
    private bool _portalsAllowed;
    private bool _nameplatesEnabled;

    // Constructor updated with clearer parameter names to match interface readers
    public WorldRuleParameters(
      bool avatarParamSetEnabled,
      bool bbccFlyingAllowed,
      bool metaPortPropsAllowed,
      bool metaPortPortalsAllowed,
      bool metaPortNameplatesEnabled
    )
    {
      _avatarParamSetEnabled = avatarParamSetEnabled;
      _flyingAllowed = bbccFlyingAllowed;
      _propsAllowed = metaPortPropsAllowed;
      _portalsAllowed = metaPortPortalsAllowed;
      _nameplatesEnabled = metaPortNameplatesEnabled;
    }

#pragma warning disable S2292
    public bool AvatarParamSetEnabled
    {
      get => _avatarParamSetEnabled;
      set => _avatarParamSetEnabled = value;
    }

    public bool FlyingAllowed
    {
      get => _flyingAllowed;
      set => _flyingAllowed = value;
    }

    public bool PropsAllowed
    {
      get => _propsAllowed;
      set => _propsAllowed = value;
    }

    public bool PortalsAllowed
    {
      get => _portalsAllowed;
      set => _portalsAllowed = value;
    }

    public bool NameplatesEnabled
    {
      get => _nameplatesEnabled;
      set => _nameplatesEnabled = value;
    }
#pragma warning restore S2292
  }

  public class ModStatusParameters
  {
    private bool _isEnabled;
    private bool _dataFeedDisabled;
    private bool _dataFeedAPIDisabled;

    public ModStatusParameters(bool isEnabled, bool dataFeedDisabled, bool dataFeedAPIDisabled)
    {
      _isEnabled = isEnabled;
      _dataFeedDisabled = dataFeedDisabled;
      _dataFeedAPIDisabled = dataFeedAPIDisabled;
    }

#pragma warning disable S2292
    public bool IsEnabled
    {
      get => _isEnabled;
      set => _isEnabled = value;
    }

    public bool DataFeedDisabled
    {
      get => _dataFeedDisabled;
      set => _dataFeedDisabled = value;
    }

    public bool DataFeedAPIDisabled
    {
      get => _dataFeedAPIDisabled;
      set => _dataFeedAPIDisabled = value;
    }
#pragma warning restore S2292
  }

  public class PlatformStateParameters
  {
    private bool _dataFeedErrorBBCC;
    private bool _dataFeedErrorMetaPort;

    public PlatformStateParameters(bool dataFeedErrorBBCC, bool dataFeedErrorMetaPort)
    {
      _dataFeedErrorBBCC = dataFeedErrorBBCC;
      _dataFeedErrorMetaPort = dataFeedErrorMetaPort;
    }

#pragma warning disable S2292
    public bool DataFeedErrorBBCC
    {
      get => _dataFeedErrorBBCC;
      set => _dataFeedErrorBBCC = value;
    }

    public bool DataFeedErrorMetaPort
    {
      get => _dataFeedErrorMetaPort;
      set => _dataFeedErrorMetaPort = value;
    }
#pragma warning restore S2292
  }
}
