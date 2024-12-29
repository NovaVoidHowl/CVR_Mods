using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
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
      BaseResponse<DetailedWorld> response;
      try
      {
        var payload = new { worldID = guid };
        response = await ApiConnection.MakeRequest<DetailedWorld>(ApiConnection.ApiOperation.WorldDetail, payload);
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

      return new WorldAbiApiInfo
      {
        Description = response.Data.Description,
        Tags = response.Data.Tags,
        Categories = response.Data.Categories,
        FileSize = response.Data.FileSize,
        UploadedAt = response.Data.UploadedAt,
        UpdatedAt = response.Data.UpdatedAt,
        AuthorName = response.Data.Author.Name,
        CompatibilityVersion = response.Data.CompatibilityVersion,
        Platform = response.Data.Platform
      };
    }
  }
}
