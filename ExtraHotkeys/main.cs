using Common;
using Common.Configuration;

namespace ExtraHotkeys
{
	public class Main: Mod
	{
		internal const string version = "1.2.0";

		internal static ModConfig config = Config.tryLoad<ModConfig>(Config.LoadOptions.ForcedLoad);

		protected override void init() => HotkeyManager.init();
		public override void OnUpdate() => HotkeyManager.update();
	}
}