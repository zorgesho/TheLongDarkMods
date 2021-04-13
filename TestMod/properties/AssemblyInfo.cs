using System.Reflection;
using System.Runtime.InteropServices;

using MelonLoader;

[assembly: AssemblyTitle("TestMod")]
[assembly: AssemblyProduct("TestMod")]
[assembly: AssemblyCopyright("© 2021 zorgesho")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.0")]

[assembly: MelonInfo(typeof(TestMod.Main), "TestMod", "1.0.0", "zorgesho")]
[assembly: MelonGame("Hinterland", "TheLongDark")]