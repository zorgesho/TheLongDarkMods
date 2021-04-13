using MelonLoader;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("SaveAnywhere")]
[assembly: AssemblyProduct("SaveAnywhere")]
[assembly: AssemblyCopyright("© 2021 zorgesho")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion(SaveAnywhere.Main.version)]

[assembly: MelonInfo(typeof(SaveAnywhere.Main), "SaveAnywhere", SaveAnywhere.Main.version, "zorgesho")]
[assembly: MelonGame("Hinterland", "TheLongDark")]