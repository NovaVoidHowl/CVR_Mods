﻿using System.Reflection;
using uk.novavoidhowl.dev.cvrmods.THtoCVR.Properties;
using MelonLoader;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(uk.novavoidhowl.dev.cvrmods.THtoCVR))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(uk.novavoidhowl.dev.cvrmods.THtoCVR))]

[assembly: MelonInfo(
  typeof(uk.novavoidhowl.dev.cvrmods.THtoCVR.THtoCVR),
  nameof(uk.novavoidhowl.dev.cvrmods.THtoCVR),
  AssemblyInfoParams.Version,
  AssemblyInfoParams.Author,
  downloadLink: "https://github.com/NovaVoidHowl/CVR_Mods"
)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: VerifyLoaderVersion(0, 6, 6, true)]
[assembly: MelonColor(255, 255, 188, 0)]
[assembly: MelonAuthorColor(255, 95, 95, 255)]
[assembly: MelonIncompatibleAssemblies(AssemblyInfoParams.CVRParamLibName)]

namespace uk.novavoidhowl.dev.cvrmods.THtoCVR.Properties;

internal static class AssemblyInfoParams
{
  public const string Version = "0.0.3";
  public const string Author = "NovaVoidHowl";
  public const string CVRParamLibName = "CVRParamLib";
}
