using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public static class AvatarAbiApiService
  {
    public static async Task<AvatarAbiApiInfo> RequestAvatarDetails(string guid)
    {
      // Check if user is authenticated before making the request
      if (!AuthManager.IsAuthenticated)
      {
        MelonLogger.Warning("[ABI API Call] Cannot fetch avatar details - user is not authenticated");
        return null;
      }

      MelonLogger.Msg($"[ABI API Call] Fetching avatar {guid} details...");
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
        MelonLogger.Error($"[ABI API Call] Fetching avatar {guid} details has Failed!");
        MelonLogger.Error(ex);
        return null;
      }
      if (response == null)
      {
        MelonLogger.Msg($"[ABI API Call] Fetching avatar {guid} details has Failed! Response came back empty.");
        return null;
      }
      MelonLogger.Msg($"[ABI API Call] Fetched avatar {guid} details successfully!");

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
