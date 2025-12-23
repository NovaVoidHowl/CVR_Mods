using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.API.Responses.DetailsV2;
using uk.novavoidhowl.dev.cvrmods.DataFeed.helpers;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.abi_api_connectors
{
  public static class WorldAbiApiService
  {
    public static async Task<WorldAbiApiInfo> RequestWorldDetails(string guid)
    {
      var payload = new { worldID = guid };
      var response = await AbiApiHelper.MakeApiRequest<ContentWorldResponse>(
        ApiConnection.ApiOperation.WorldDetail,
        payload,
        "world",
        guid
      );

      if (response == null)
      {
        return null;
      }

      // Get platform-specific data (Tags, FileSize, UpdatedAt, CompatibilityVersion)
      var hasPlatformData = AbiApiHelper.TryGetPlatformData(response.Data.Platforms, out var platformData);

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
