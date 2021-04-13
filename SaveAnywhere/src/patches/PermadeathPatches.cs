using Harmony;
using Common;

namespace SaveAnywhere
{
	[PatchClass]
	static class PermadeathPatches
	{
		static bool prepare() => Main.config.disablePermadeath;

		static bool disableSlotDeleting = false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Condition), "PlayerDeath")]
		[HarmonyPatch(typeof(Panel_ChallengeComplete), "Enable")]
		static void disableSlotDeletingPatch()
		{
			if (!GameManager.IsStoryMode())
				disableSlotDeleting = true;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(SaveGameSystem), "DeleteSaveFilesForGameId")]
		static bool SaveGameSystem_DeleteSaveFilesForGameId_Prefix() => !disableSlotDeleting || (disableSlotDeleting = false);
	}
}