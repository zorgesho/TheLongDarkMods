#if DEBUG
using System;
using HarmonyLib;
using Common;

namespace CookWhileFishing
{
	[HarmonyPatch(typeof(BootUpdate), "Start")]
	static class ConsoleCommands
	{
		static void Postfix()
		{
			static int _count(int defaultCount) => uConsole.NextParameterIsInt()? Math.Min(uConsole.GetInt(), defaultCount): defaultCount;

			uConsole.RegisterCommand("addfish", new Action(() =>
			{
				var fish = new[] { "gear_rawsmallmouthbass", "gear_rawlakewhitefish", "gear_rawcohosalmon", "gear_rawrainbowtrout" };
				int count = _count(4);

				for (int i = 0; i < count; i++)
					GameUtils.addItem(fish[i]);
			}));

			uConsole.RegisterCommand("addmeat", new Action(() =>
			{
				var meat = new[] { "gear_rawmeatbear", "gear_rawmeatdeer", "gear_rawmeatmoose", "gear_rawmeatrabbit", "gear_rawmeatwolf" };
				int count = _count(5);

				for (int i = 0; i < count; i++)
					GameUtils.addItem(meat[i]);
			}));
		}
	}
}
#endif // DEBUG