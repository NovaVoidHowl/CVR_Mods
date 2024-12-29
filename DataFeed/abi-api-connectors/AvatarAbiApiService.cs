using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public static class AvatarAbiApiService
  {
    public static async Task<AvatarAbiApiInfo> RequestAvatarDetails(string guid)
    {
      MelonLogger.Msg($"[API] Fetching avatar {guid} details...");
      BaseResponse<AvatarDetailsResponse> response;
      try
      {
        var payload = new { avatarID = guid };
        response = await ApiConnection.MakeRequest<AvatarDetailsResponse>(
          ApiConnection.ApiOperation.AvatarDetail,
          payload
        );
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"[API] Fetching avatar {guid} details has Failed!");
        MelonLogger.Error(ex);
        return null;
      }
      if (response == null)
      {
        MelonLogger.Msg($"[API] Fetching avatar {guid} details has Failed! Response came back empty.");
        return null;
      }
      MelonLogger.Msg($"[API] Fetched avatar {guid} details successfully!");

      return new AvatarAbiApiInfo
      {
        AvatarName = response.Data.Name,
        Description = response.Data.Description,
        AuthorName = response.Data.User.Name, // Changed to use Name instead of Username
        UploadedAt = response.Data.UploadedAt,
        UpdatedAt = response.Data.UpdatedAt,
        SwitchPermitted = response.Data.SwitchPermitted,
        IsPublished = response.Data.IsPublished,
        Categories = response.Data.Categories,
        FileSize = response.Data.FileSize
      };
    }
  }
}
