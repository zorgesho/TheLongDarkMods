using Harmony;
using UnityEngine;

using Common;

namespace TestMod
{
	[PatchClass]
	static class Patches
	{
		[HarmonyPrefix, HarmonyPatch(typeof(PlayerManager), "Awake")]
		static bool Prefix1()
		{
			return true;
		}
	}
}