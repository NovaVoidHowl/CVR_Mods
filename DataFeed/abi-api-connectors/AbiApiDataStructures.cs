namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public class AvatarAbiApiInfo
  {
    private string _avatarName;
    private string _description;
    private string _authorName;
    private DateTime _uploadedAt;
    private DateTime _updatedAt;
    private bool _switchPermitted;
    private bool _isPublished;
    private string[] _categories;
    private long _fileSize;

#pragma warning disable S2292 // keeping full property implementation for future-proofing
    public string AvatarName
    {
      get => _avatarName;
      set => _avatarName = value;
    }

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
}
