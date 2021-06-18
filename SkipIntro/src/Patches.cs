using HarmonyLib;
using Common;

namespace SkipIntro
{
	[PatchClass]
	static class Patches
	{
		[HarmonyPostfix, HarmonyPatch(typeof(BootUpdate), "Update")]
		static void BootUpdate_Update_Postfix(BootUpdate __instance) => __instance.m_Label_Continue.gameObject.SetActive(false);

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_MainMenu), "Enable")]
		static void PanelMainMenu_Enable_Prefix(Panel_MainMenu __instance) => MoviePlayer.m_HasIntroPlayedForMainMenu = __instance.m_StartFadedOut = true;

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_MainMenu), "UpdateFading")]
		static void PanelMainMenu_UpdateFading_Prefix(Panel_MainMenu __instance) => __instance.m_InitialScreenFadeInDuration = 0f;

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_MainMenu), "SetPanelAlpha")]
		static void PanelMainMenu_SetPanelAlpha_Prefix(ref float alpha) => alpha = 1f;

		[HarmonyPrefix, HarmonyPatch(typeof(BootUpdate), "Start")]
		static void BootUpdate_Start_Prefix(BootUpdate __instance)
		{
			for (int i = 1; i <= 3; i++)
				__instance.gameObject.getChild($"Label_Disclaimer_{i}").SetActive(false);

			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}
	}
}