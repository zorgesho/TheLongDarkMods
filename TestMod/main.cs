using Common;

namespace TestMod
{
	public class Main: Mod
	{
		protected override void init() => HarmonyHelper.patchAll();
	}
}