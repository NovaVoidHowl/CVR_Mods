using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.API.Responses.DetailsV2;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public static class WorldAbiApiService
  {
    public static async Task<WorldAbiApiInfo> RequestWorldDetails(string guid)
    {
      // Check if user is authenticated before making the request
      if (!AuthManager.IsAuthenticated)
      {
        MelonLogger.Warning("[ABI API Call] Cannot fetch world details - user is not authenticated");
        return null;
      }

      MelonLogger.Msg($"[ABI API Call] Fetching world {guid} details...");
      BaseResponse<ContentWorldResponse> response;
      try
      {
        var payload = new { worldID = guid };
        // Use API version "2" explicitly to get complete data (Author, Platforms, Tags, FileSize)
        response = await ApiConnection.MakeRequest<ContentWorldResponse>(
          ApiConnection.ApiOperation.WorldDetail,
          payload,
          "2" // Explicitly use API v2
        );
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"[ABI API Call] Fetching world {guid} details has Failed!");
        MelonLogger.Error(ex);
        return null;
      }
      if (response == null)
      {
        MelonLogger.Msg($"[ABI API Call] Fetching world {guid} details has Failed! Response came back empty.");
        return null;
      }
      MelonLogger.Msg($"[ABI API Call] Fetched world {guid} details successfully!");

      // Get platform-specific data (Tags, FileSize, UpdatedAt, CompatibilityVersion)
      PlatformData platformData = null;
      var hasPlatformData = response.Data.Platforms?.TryGetValue(Platforms.Pc_Standalone, out platformData) ?? false;

      return new WorldAbiApiInfo
      {
        Description = response.Data.Description,
        Tags = hasPlatformData ? platformData.Tags : new string[0],
        Categories = response.Data.Categories?.ToArray(),
        FileSize = hasPlatformData ? (long)platformData.FileSize : 0,
        UploadedAt = response.Data.UploadedAt,
        UpdatedAt = hasPlatformData ? platformData.UpdatedAt : response.Data.UploadedAt,
        AuthorName = response.Data.Author?.Name,
        CompatibilityVersion = hasPlatformData ? platformData.CompatibilityVersion : CompatibilityVersions.Invalid,
        Platform = Platforms.Pc_Standalone
      };
    }
  }
}
