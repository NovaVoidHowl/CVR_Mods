using ABI_RC.Core.Networking.API.Responses;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public abstract class AbiApiInfoBase
  {
    private string _description;
    private string _authorName;
    private DateTime _uploadedAt;
    private DateTime _updatedAt;
    private string[] _categories;
    private long _fileSize;

#pragma warning disable S2292 // keeping full property implementation for future-proofing
    public string Description
    {
      get => _description;
      set => _description = value;
    }

    public string AuthorName
    {
      get => _authorName;
      set => _authorName = value;
    }

    public DateTime UploadedAt
    {
      get => _uploadedAt;
      set => _uploadedAt = value;
    }

    public DateTime UpdatedAt
    {
      get => _updatedAt;
      set => _updatedAt = value;
    }

    public string[] Categories
    {
      get => _categories;
      set => _categories = value;
    }

    public long FileSize
    {
      get => _fileSize;
      set => _fileSize = value;
    }
#pragma warning restore S2292
  }

  public class AvatarAbiApiInfo : AbiApiInfoBase
  {
    private string _avatarName;
    private bool _switchPermitted;
    private bool _isPublished;

#pragma warning disable S2292
    public string AvatarName
    {
      get => _avatarName;
      set => _avatarName = value;
    }

    public bool SwitchPermitted
    {
      get => _switchPermitted;
      set => _switchPermitted = value;
    }

    public bool IsPublished
    {
      get => _isPublished;
      set => _isPublished = value;
    }
#pragma warning restore S2292
  }

  public class WorldAbiApiInfo : AbiApiInfoBase
  {
    private string[] _tags;
    private CompatibilityVersions _compatibilityVersion;
    private Platforms _platform;

#pragma warning disable S2292
    public string[] Tags
    {
      get => _tags;
      set => _tags = value;
    }

    public CompatibilityVersions CompatibilityVersion
    {
      get => _compatibilityVersion;
      set => _compatibilityVersion = value;
    }

    public Platforms Platform
    {
      get => _platform;
      set => _platform = value;
    }
#pragma warning restore S2292
  }
}
