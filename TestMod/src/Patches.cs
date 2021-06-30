using HarmonyLib;
using UnityEngine;

using Common;

namespace TestMod
{
	[HarmonyPatch]
	static class Patches
	{
		[HarmonyPrefix, HarmonyPatch(typeof(PlayerManager), "Awake")]
		static bool Prefix1()
		{
			return true;
		}
	}
}