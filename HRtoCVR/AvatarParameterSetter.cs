using ABI_RC.Core.Player;
using MelonLoader;
using uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR
{
  public static class AvatarParameterSetter
  {
    public static void SetMainParameters(
      bool HRtoCVRDisabled,
      PulsoidClient pulsoidClient,
      SimulatedClient simulatedClient,
      TextFileClient textFileClient,
      HRtoCVR.HRConnectionType hrType,
      bool resetToDefault = false
    )
    {
      PlayerSetup.Instance.animatorManager.SetParameter("HRtoCVRDisabled", HRtoCVRDisabled);

      if (resetToDefault)
      {
        MelonLogger.Msg("Reset triggered, restoring HR parameters to default");
        PlayerSetup.Instance.animatorManager.SetParameter("onesHR", 0);
        PlayerSetup.Instance.animatorManager.SetParameter("tensHR", 0);
        PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", 0);
        PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", false);
        PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", false);
        PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", 0);
        PlayerSetup.Instance.animatorManager.SetParameter("HR", 0);
        PlayerSetup.Instance.animatorManager.SetParameter("isHRBeat", false);
        return;
      }

      switch (hrType)
      {
        case HRtoCVR.HRConnectionType.Pulsoid:
          if (pulsoidClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter("onesHR", pulsoidClient.onesHR);
            PlayerSetup.Instance.animatorManager.SetParameter("tensHR", pulsoidClient.tensHR);
            PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", pulsoidClient.hundredsHR);
            PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", pulsoidClient.isHRConnected);
            PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", pulsoidClient.isHRActive);
            PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", pulsoidClient.HRPercent);
            PlayerSetup.Instance.animatorManager.SetParameter("HR", pulsoidClient.HR);
          }
          break;
        case HRtoCVR.HRConnectionType.Simulated:
          if (simulatedClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter("onesHR", simulatedClient.onesHR);
            PlayerSetup.Instance.animatorManager.SetParameter("tensHR", simulatedClient.tensHR);
            PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", simulatedClient.hundredsHR);
            PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", simulatedClient.isHRConnected);
            PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", simulatedClient.isHRActive);
            PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", simulatedClient.HRPercent);
            PlayerSetup.Instance.animatorManager.SetParameter("HR", simulatedClient.HR);
          }
          break;

        case HRtoCVR.HRConnectionType.TextFile:
          if (textFileClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter("onesHR", textFileClient.onesHR);
            PlayerSetup.Instance.animatorManager.SetParameter("tensHR", textFileClient.tensHR);
            PlayerSetup.Instance.animatorManager.SetParameter("hundredsHR", textFileClient.hundredsHR);
            PlayerSetup.Instance.animatorManager.SetParameter("isHRConnected", textFileClient.isHRConnected);
            PlayerSetup.Instance.animatorManager.SetParameter("isHRActive", textFileClient.isHRActive);
            PlayerSetup.Instance.animatorManager.SetParameter("HRPercent", textFileClient.HRPercent);
            PlayerSetup.Instance.animatorManager.SetParameter("HR", textFileClient.HR);
          }
          break;
      }
    }

    public static void SetRapidUpdateParameters(
      PulsoidClient pulsoidClient,
      SimulatedClient simulatedClient,
      TextFileClient textFileClient,
      HRtoCVR.HRConnectionType hrType
    )
    {
      switch (hrType)
      {
        case HRtoCVR.HRConnectionType.Pulsoid:
          if (pulsoidClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter("isHRBeat", pulsoidClient.isHRBeat);
          }
          break;
        case HRtoCVR.HRConnectionType.Simulated:
          if (simulatedClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter("isHRBeat", simulatedClient.isHRBeat);
          }
          break;

        case HRtoCVR.HRConnectionType.TextFile:
          if (textFileClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter("isHRBeat", textFileClient.isHRBeat);
          }
          break;
      }
    }
  }
}
