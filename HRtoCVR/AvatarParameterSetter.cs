using ABI_RC.Core.Player;
using MelonLoader;
using uk.novavoidhowl.dev.cvrmods.HRtoCVR.HRClients;

namespace uk.novavoidhowl.dev.cvrmods.HRtoCVR
{
  public static class AvatarParameterSetter
  {
    private const string onesHRParam = "onesHR";
    private const string tensHRParam = "tensHR";
    private const string hundredsHRParam = "hundredsHR";
    private const string isHRConnectedParam = "isHRConnected";
    private const string isHRActiveParam = "isHRActive";
    private const string HRPercentParam = "HRPercent";
    private const string HRParam = "HR";
    private const string isHRBeatParam = "isHRBeat";

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
        PlayerSetup.Instance.animatorManager.SetParameter(onesHRParam, 0);
        PlayerSetup.Instance.animatorManager.SetParameter(tensHRParam, 0);
        PlayerSetup.Instance.animatorManager.SetParameter(hundredsHRParam, 0);
        PlayerSetup.Instance.animatorManager.SetParameter(isHRConnectedParam, false);
        PlayerSetup.Instance.animatorManager.SetParameter(isHRActiveParam, false);
        PlayerSetup.Instance.animatorManager.SetParameter(HRPercentParam, 0);
        PlayerSetup.Instance.animatorManager.SetParameter(HRParam, 0);
        PlayerSetup.Instance.animatorManager.SetParameter(isHRBeatParam, false);
        return;
      }

      switch (hrType)
      {
        case HRtoCVR.HRConnectionType.Pulsoid:
          if (pulsoidClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter(onesHRParam, pulsoidClient.onesHR);
            PlayerSetup.Instance.animatorManager.SetParameter(tensHRParam, pulsoidClient.tensHR);
            PlayerSetup.Instance.animatorManager.SetParameter(hundredsHRParam, pulsoidClient.hundredsHR);
            PlayerSetup.Instance.animatorManager.SetParameter(isHRConnectedParam, pulsoidClient.isHRConnected);
            PlayerSetup.Instance.animatorManager.SetParameter(isHRActiveParam, pulsoidClient.isHRActive);
            PlayerSetup.Instance.animatorManager.SetParameter(HRPercentParam, pulsoidClient.HRPercent);
            PlayerSetup.Instance.animatorManager.SetParameter(HRParam, pulsoidClient.HR);
          }
          break;
        case HRtoCVR.HRConnectionType.Simulated:
          if (simulatedClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter(onesHRParam, simulatedClient.onesHR);
            PlayerSetup.Instance.animatorManager.SetParameter(tensHRParam, simulatedClient.tensHR);
            PlayerSetup.Instance.animatorManager.SetParameter(hundredsHRParam, simulatedClient.hundredsHR);
            PlayerSetup.Instance.animatorManager.SetParameter(isHRConnectedParam, simulatedClient.isHRConnected);
            PlayerSetup.Instance.animatorManager.SetParameter(isHRActiveParam, simulatedClient.isHRActive);
            PlayerSetup.Instance.animatorManager.SetParameter(HRPercentParam, simulatedClient.HRPercent);
            PlayerSetup.Instance.animatorManager.SetParameter(HRParam, simulatedClient.HR);
          }
          break;

        case HRtoCVR.HRConnectionType.TextFile:
          if (textFileClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter(onesHRParam, textFileClient.onesHR);
            PlayerSetup.Instance.animatorManager.SetParameter(tensHRParam, textFileClient.tensHR);
            PlayerSetup.Instance.animatorManager.SetParameter(hundredsHRParam, textFileClient.hundredsHR);
            PlayerSetup.Instance.animatorManager.SetParameter(isHRConnectedParam, textFileClient.isHRConnected);
            PlayerSetup.Instance.animatorManager.SetParameter(isHRActiveParam, textFileClient.isHRActive);
            PlayerSetup.Instance.animatorManager.SetParameter(HRPercentParam, textFileClient.HRPercent);
            PlayerSetup.Instance.animatorManager.SetParameter(HRParam, textFileClient.HR);
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
            PlayerSetup.Instance.animatorManager.SetParameter(isHRBeatParam, pulsoidClient.isHRBeat);
          }
          break;
        case HRtoCVR.HRConnectionType.Simulated:
          if (simulatedClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter(isHRBeatParam, simulatedClient.isHRBeat);
          }
          break;

        case HRtoCVR.HRConnectionType.TextFile:
          if (textFileClient != null)
          {
            PlayerSetup.Instance.animatorManager.SetParameter(isHRBeatParam, textFileClient.isHRBeat);
          }
          break;
      }
    }
  }
}
