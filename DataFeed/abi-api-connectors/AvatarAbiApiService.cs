using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.API.Responses.DetailsV2;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public static class AvatarAbiApiService
  {
    public static async Task<AvatarAbiApiInfo> RequestAvatarDetails(string guid)
    {
      var payload = new { avatarID = guid };
      var response = await AbiApiHelper.MakeApiRequest<ContentAvatarResponse>(
        ApiConnection.ApiOperation.AvatarDetail,
        payload,
        "avatar",
        guid
      );

      if (response == null)
      {
        return null;
      }

      // Get platform-specific data (FileSize, UpdatedAt, Tags)
      var hasPlatformData = AbiApiHelper.TryGetPlatformData(response.Data.Platforms, out var platformData);

      string authorName = response.Data.Author?.Name;

      return new AvatarAbiApiInfo
      {
        AvatarName = response.Data.Name,
        Description = response.Data.Description,
        AuthorName = authorName,
        UploadedAt = response.Data.UploadedAt,
        UpdatedAt = hasPlatformData ? platformData.UpdatedAt : response.Data.UploadedAt,
        SwitchPermitted = response.Data.Permitted,
        IsPublished = response.Data.Public,
        Categories = response.Data.Categories?.ToArray(),
        FileSize = hasPlatformData ? (long)platformData.FileSize : 0
      };
    }
  }
}
