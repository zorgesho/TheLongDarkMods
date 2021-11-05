using Common;
using Common.Configuration;

namespace PlaceFromInventory
{
	public class Main: Mod
	{
		internal const string version = "1.1.1";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();
	}
}