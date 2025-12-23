using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.API.Responses.CategoriesV2;
using ABI_RC.Core.Networking.API.Responses.CategoriesV2.User;
using ABI_RC.Core.Networking.API.Responses.DetailsV2;
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
      BaseResponse<ContentAvatarResponse> response;
      try
      {
        var payload = new { avatarID = guid };
        // Try with API version "2" explicitly as Kafeijao's OSC mod does
        response = await ApiConnection.MakeRequest<ContentAvatarResponse>(
          ApiConnection.ApiOperation.AvatarDetail,
          payload,
          "2" // Explicitly use API v2
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

      // Get platform-specific data (FileSize, UpdatedAt, Tags)
      PlatformData platformData = null;
      response.Data.Platforms?.TryGetValue(Platforms.Pc_Standalone, out platformData);

      string authorName = response.Data.Author?.Name;

      return new AvatarAbiApiInfo
      {
        AvatarName = response.Data.Name,
        Description = response.Data.Description,
        AuthorName = authorName,
        UploadedAt = response.Data.UploadedAt,
        UpdatedAt = platformData?.UpdatedAt ?? response.Data.UploadedAt,
        SwitchPermitted = response.Data.Permitted,
        IsPublished = response.Data.Public,
        Categories = response.Data.Categories?.ToArray(),
        FileSize = (long)(platformData?.FileSize ?? 0)
      };
    }
  }
}
