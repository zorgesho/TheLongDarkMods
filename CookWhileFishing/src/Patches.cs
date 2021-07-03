using HarmonyLib;
using Common;

namespace CookWhileFishing
{
	[HarmonyPatch(typeof(IceFishingHole), "SetFishingInProgress")]
	static class IceFishingHole_SetFishingInProgress_Patch
	{
		static void Postfix(IceFishingHole __instance, bool inProgress)
		{																										$"IceFishingHole.SetFishingInProgress: {inProgress}".logDbg();
			var pm = GameUtils.PlayerManager.gameObject;

			if (inProgress)
				pm.ensureComponent<AutoCook>().init(__instance);
			else
				pm.destroyComponent<AutoCook>(false);
		}
	}
}