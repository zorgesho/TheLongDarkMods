using MelonLoader;

using Common;
using Common.Configuration;

namespace DayNightSpeed
{
	public class Main: MelonMod
	{
		internal const string version = "1.0.0";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public override void OnUpdate()
		{
			foreach (var pair in config.hotkeys)
			{
				if (InputHelper.isKeyDown(pair.Key))
					TimeScalePatch.timescale = pair.Value;
			}
		}
	}
}