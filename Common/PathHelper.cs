using System.IO;
using MelonLoader;

namespace Common
{
	static partial class Paths
	{
		public static readonly string modRootPath = Path.Combine(Path.GetDirectoryName(typeof(MelonMod).Assembly.Location), @"..\Mods\");
	}
}