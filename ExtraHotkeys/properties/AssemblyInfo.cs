using System.Reflection;
using System.Runtime.InteropServices;

using MelonLoader;

[assembly: AssemblyTitle("ExtraHotkeys")]
[assembly: AssemblyProduct("ExtraHotkeys")]
[assembly: AssemblyCopyright("© 2021 zorgesho")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion(ExtraHotkeys.Main.version)]

[assembly: MelonInfo(typeof(ExtraHotkeys.Main), "ExtraHotkeys", ExtraHotkeys.Main.version, "zorgesho")]
[assembly: MelonGame("Hinterland", "TheLongDark")]