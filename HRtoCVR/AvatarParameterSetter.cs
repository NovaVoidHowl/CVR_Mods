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
      PlayerSetup.Instance.AnimatorManager.SetParameter("HRtoCVRDisabled", HRtoCVRDisabled);

      if (resetToDefault)
      {
        MelonLogger.Msg("Reset triggered, restoring HR parameters to default");
        PlayerSetup.Instance.AnimatorManager.SetParameter(onesHRParam, 0);
        PlayerSetup.Instance.AnimatorManager.SetParameter(tensHRParam, 0);
        PlayerSetup.Instance.AnimatorManager.SetParameter(hundredsHRParam, 0);
        PlayerSetup.Instance.AnimatorManager.SetParameter(isHRConnectedParam, false);
        PlayerSetup.Instance.AnimatorManager.SetParameter(isHRActiveParam, false);
        PlayerSetup.Instance.AnimatorManager.SetParameter(HRPercentParam, 0);
        PlayerSetup.Instance.AnimatorManager.SetParameter(HRParam, 0);
        PlayerSetup.Instance.AnimatorManager.SetParameter(isHRBeatParam, false);
        return;
      }

      switch (hrType)
      {
        case HRtoCVR.HRConnectionType.Pulsoid:
          if (pulsoidClient != null)
          {
            PlayerSetup.Instance.AnimatorManager.SetParameter(onesHRParam, pulsoidClient.onesHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(tensHRParam, pulsoidClient.tensHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(hundredsHRParam, pulsoidClient.hundredsHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRConnectedParam, pulsoidClient.isHRConnected);
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRActiveParam, pulsoidClient.isHRActive);
            PlayerSetup.Instance.AnimatorManager.SetParameter(HRPercentParam, pulsoidClient.HRPercent);
            PlayerSetup.Instance.AnimatorManager.SetParameter(HRParam, pulsoidClient.HR);
          }
          break;
        case HRtoCVR.HRConnectionType.Simulated:
          if (simulatedClient != null)
          {
            PlayerSetup.Instance.AnimatorManager.SetParameter(onesHRParam, simulatedClient.onesHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(tensHRParam, simulatedClient.tensHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(hundredsHRParam, simulatedClient.hundredsHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRConnectedParam, simulatedClient.isHRConnected);
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRActiveParam, simulatedClient.isHRActive);
            PlayerSetup.Instance.AnimatorManager.SetParameter(HRPercentParam, simulatedClient.HRPercent);
            PlayerSetup.Instance.AnimatorManager.SetParameter(HRParam, simulatedClient.HR);
          }
          break;

        case HRtoCVR.HRConnectionType.TextFile:
          if (textFileClient != null)
          {
            PlayerSetup.Instance.AnimatorManager.SetParameter(onesHRParam, textFileClient.onesHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(tensHRParam, textFileClient.tensHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(hundredsHRParam, textFileClient.hundredsHR);
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRConnectedParam, textFileClient.isHRConnected);
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRActiveParam, textFileClient.isHRActive);
            PlayerSetup.Instance.AnimatorManager.SetParameter(HRPercentParam, textFileClient.HRPercent);
            PlayerSetup.Instance.AnimatorManager.SetParameter(HRParam, textFileClient.HR);
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
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRBeatParam, pulsoidClient.isHRBeat);
          }
          break;
        case HRtoCVR.HRConnectionType.Simulated:
          if (simulatedClient != null)
          {
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRBeatParam, simulatedClient.isHRBeat);
          }
          break;

        case HRtoCVR.HRConnectionType.TextFile:
          if (textFileClient != null)
          {
            PlayerSetup.Instance.AnimatorManager.SetParameter(isHRBeatParam, textFileClient.isHRBeat);
          }
          break;
      }
    }
  }
}
