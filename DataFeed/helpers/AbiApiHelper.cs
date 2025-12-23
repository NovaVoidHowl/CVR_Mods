using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABI_RC.Core.Networking;
using ABI_RC.Core.Networking.API;
using ABI_RC.Core.Networking.API.Responses;
using ABI_RC.Core.Networking.API.Responses.DetailsV2;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.DataFeed.helpers
{
  public static class AbiApiHelper
  {
    /// <summary>
    /// Makes an API request with standard error handling and logging
    /// </summary>
    public static async Task<BaseResponse<T>> MakeApiRequest<T>(
      ApiConnection.ApiOperation operation,
      object payload,
      string contentType,
      string guid
    )
    {
      // Check if user is authenticated before making the request
      if (!AuthManager.IsAuthenticated)
      {
        MelonLogger.Warning($"[ABI API Call] Cannot fetch {contentType} details - user is not authenticated");
        return null;
      }

      MelonLogger.Msg($"[ABI API Call] Fetching {contentType} {guid} details...");
      BaseResponse<T> response;
      try
      {
        // Use API version "2" explicitly to get complete data (Author, Platforms, Tags, FileSize)
        response = await ApiConnection.MakeRequest<T>(operation, payload, "2");
      }
      catch (Exception ex)
      {
        MelonLogger.Error($"[ABI API Call] Fetching {contentType} {guid} details has Failed!");
        MelonLogger.Error(ex);
        return null;
      }

      if (response == null)
      {
        MelonLogger.Msg($"[ABI API Call] Fetching {contentType} {guid} details has Failed! Response came back empty.");
        return null;
      }

      MelonLogger.Msg($"[ABI API Call] Fetched {contentType} {guid} details successfully!");
      return response;
    }

    /// <summary>
    /// Extracts platform-specific data from the Platforms dictionary
    /// </summary>
    public static bool TryGetPlatformData(
      IDictionary<Platforms, PlatformData> platforms,
      out PlatformData platformData
    )
    {
      platformData = null;
      return platforms?.TryGetValue(Platforms.Pc_Standalone, out platformData) ?? false;
    }
  }
}
