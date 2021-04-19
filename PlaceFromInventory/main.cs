using Common;
using Common.Configuration;

namespace PlaceFromInventory
{
	public class Main: Mod
	{
		internal const string version = "1.0.0";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		protected override void init() => HarmonyHelper.patchAll();
	}
}