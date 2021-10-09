using HarmonyLib;

namespace SaveAnywhere
{
	[HarmonyPatch]
	static class PermadeathPatches
	{
		static bool Prepare() => Main.config.disablePermadeath;

		static bool disableSlotDeleting = false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Condition), "PlayerDeath")]
		[HarmonyPatch(typeof(Panel_ChallengeComplete), "ShowPanel")]
		static void disableSlotDeletingPatch()
		{
			if (!GameManager.IsStoryMode())
				disableSlotDeleting = true;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(SaveGameSystem), "DeleteSaveFilesForGameId")]
		static bool SaveGameSystem_DeleteSaveFilesForGameId_Prefix() => !disableSlotDeleting || (disableSlotDeleting = false);
	}
}