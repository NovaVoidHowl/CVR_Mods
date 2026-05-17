using System.Reflection;
using uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix.Properties;
using MelonLoader;

[assembly: AssemblyVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyFileVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfoParams.Version)]
[assembly: AssemblyTitle(nameof(uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix))]
[assembly: AssemblyCompany(AssemblyInfoParams.Author)]
[assembly: AssemblyProduct(nameof(uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix))]

[assembly: MelonInfo(
  typeof(uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix.OSCLaunchArgsFix),
  nameof(uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix),
  AssemblyInfoParams.Version,
  AssemblyInfoParams.Author,
  downloadLink: "https://github.com/NovaVoidHowl/CVR_Mods"
)]
[assembly: MelonGame(null, "ChilloutVR")]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
[assembly: VerifyLoaderVersion(0, 7, 0, true)]
[assembly: MelonColor(255, 255, 188, 0)]
[assembly: MelonAuthorColor(255, 95, 95, 255)]

namespace uk.novavoidhowl.dev.cvrmods.OSCLaunchArgsFix.Properties;

internal static class AssemblyInfoParams
{
  public const string Version = "0.1.0";
  public const string Author = "NovaVoidHowl";
}
