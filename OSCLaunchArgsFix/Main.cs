using System;
using ABI_RC.Core.Savior;
using ABI_RC.Systems.OSC;
using HarmonyLib;
using MelonLoader;

namespace uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix
{
#pragma warning disable S101
  public class OSCLaunchArgsFix : MelonMod
#pragma warning restore S101
  {
    private const string HarmonyId = "uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix";
    private const string ListenerPortPrefix = "--osc-listener-port=";
    private const string SenderPortPrefix = "--osc-sender-port=";

    private static bool _hasCheckedLaunchArgs;
    private static bool _hasLoggedMissingCheckVr;
    private static bool _hasLoggedNoArgs;
    private static bool _hasLoggedInvalidListener;
    private static bool _hasLoggedInvalidSender;
    private static HarmonyLib.Harmony _harmony;

    public override void OnInitializeMelon()
    {
      _harmony = new HarmonyLib.Harmony(HarmonyId);
      _harmony.PatchAll();
      MelonLogger.Msg("OSC launch argument fix loaded.");
    }

    public override void OnDeinitializeMelon()
    {
      _harmony?.UnpatchSelf();
      _harmony = null;
    }

    internal static void ApplyLaunchArgsBeforeOscStart()
    {
      if (CheckVR.Instance == null)
      {
        if (!_hasLoggedMissingCheckVr)
        {
          _hasLoggedMissingCheckVr = true;
          MelonLogger.Warning("CheckVR.Instance is not available yet; OSC launch argument fix could not run.");
        }

        return;
      }

      var validListener = TryReadPortArg(
        ListenerPortPrefix,
        "listener",
        ref _hasLoggedInvalidListener,
        out var foundListener,
        out var listenerPort
      );
      var validSender = TryReadPortArg(
        SenderPortPrefix,
        "sender",
        ref _hasLoggedInvalidSender,
        out var foundSender,
        out var senderPort
      );

      if (!foundListener && !foundSender)
      {
        LogNoArgsOnce();
        return;
      }

      if (validListener)
      {
        ApplyPort(
          "listener",
          listenerPort,
          CheckVR.Instance.oscListenerPort,
          value => CheckVR.Instance.oscListenerPort = value
        );
      }

      if (validSender)
      {
        ApplyPort(
          "sender",
          senderPort,
          CheckVR.Instance.oscSenderPort,
          value => CheckVR.Instance.oscSenderPort = value
        );
      }

      _hasCheckedLaunchArgs = true;
    }

    private static void ApplyPort(string portName, int parsedPort, int currentPort, Action<int> setPort)
    {
      if (currentPort == parsedPort)
      {
        if (!_hasCheckedLaunchArgs)
        {
          MelonLogger.Msg(
            $"CVR already loaded a valid OSC {portName} port from launch args; OSCLaunchArgsFix may no longer be needed."
          );
        }
        return;
      }

      setPort(parsedPort);
      MelonLogger.Msg($"Applied OSC {portName} port override: {parsedPort}");
    }

    private static bool TryReadPortArg(
      string prefix,
      string portName,
      ref bool hasLoggedInvalidValue,
      out bool foundArg,
      out int port
    )
    {
      foundArg = false;
      port = 0;

      foreach (var arg in Environment.GetCommandLineArgs())
      {
        if (!arg.StartsWith(prefix, StringComparison.Ordinal))
        {
          continue;
        }

        foundArg = true;
        var rawPort = arg.Substring(prefix.Length);
        if (!int.TryParse(rawPort, out var parsedPort) || parsedPort < 1 || parsedPort > 65535)
        {
          if (!hasLoggedInvalidValue)
          {
            hasLoggedInvalidValue = true;
            MelonLogger.Warning($"Ignoring invalid --osc-{portName}-port value: {rawPort}");
          }

          return false;
        }

        port = parsedPort;
        return true;
      }

      return false;
    }

    private static void LogNoArgsOnce()
    {
      if (_hasLoggedNoArgs)
      {
        return;
      }

      _hasLoggedNoArgs = true;
      MelonLogger.Msg("No OSC port launch arguments found; leaving CVR defaults unchanged.");
    }
  }

  [HarmonyPatch(typeof(OSCServer), nameof(OSCServer.StartServer))]
  internal static class OSCServerStartServerPatch
  {
    private static void Prefix()
    {
      OSCLaunchArgsFix.ApplyLaunchArgsBeforeOscStart();
    }
  }
}
