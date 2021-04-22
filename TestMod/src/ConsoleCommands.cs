using System; 

using Harmony;
using UnityEngine;

using Common;

namespace TestMod
{
	using Object = UnityEngine.Object;

	[HarmonyPatch(typeof(BootUpdate), "Start")]
	static class ConsoleCommands
	{
		static void Postfix()
		{
			uConsole.RegisterCommand("test1", new Action(() =>
			{
				"test".logDbg();
			}));
		}
	}
}