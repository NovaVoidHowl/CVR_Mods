using ABI_RC.Core.Player;
using uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR
{
  public static class AvatarParameterSetter
  {
    public static void SetParameters(
      bool HRtoCVRDisabled,
      PulsoidClient pulsoidClient,
      SimulatedClient simulatedClient,
      HRtoCVR.HRConnectionType hrType
    )
    {
      PlayerSetup.Instance.animatorManager.SetParameter("HRtoCVRDisabled", HRtoCVRDisabled);

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
      }
    }
  }
}
