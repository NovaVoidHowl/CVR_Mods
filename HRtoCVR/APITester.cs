using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR
{
#pragma warning disable S101
  public static class APITester
#pragma warning restore S101
  {
    public static async Task<bool> IsApiReachable(
      string baseUri,
      List<System.Net.HttpStatusCode> expectedResponseCodes = null,
      bool noOKLog = false
    )
    {
      if (expectedResponseCodes == null)
      {
        expectedResponseCodes = new List<System.Net.HttpStatusCode> { System.Net.HttpStatusCode.OK };
      }

      using (HttpClient client = new HttpClient())
      {
        client.Timeout = TimeSpan.FromSeconds(10); // Set a timeout for the request
        try
        {
          HttpResponseMessage response = await client.GetAsync(baseUri);
          if (expectedResponseCodes.Contains(response.StatusCode))
          {
            if (!noOKLog)
            {
              MelonLogger.Msg($"API {baseUri} is reachable with status code: {response.StatusCode}");
            }
            return true;
          }
          else
          {
            MelonLogger.Error($"API {baseUri} is not reachable. Status code: {response.StatusCode}");
            return false;
          }
        }
        catch (Exception ex)
        {
          MelonLogger.Error($"Error checking API {baseUri}: {ex.Message}");
          MelonLogger.Error($"Exception Type: {ex.GetType().FullName}");
          MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
          return false;
        }
      }
    }
  }
}
