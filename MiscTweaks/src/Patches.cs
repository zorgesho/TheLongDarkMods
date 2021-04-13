using Harmony;
using UnityEngine;

using Common;

namespace MiscTweaks
{
	[HarmonyPriority(Priority.Low)]
	[HarmonyPatch(typeof(BootUpdate), "Start")]
	static class BootUpdate_Start_Patch
	{
		static void Prefix()
		{
			if (uConsole.m_Instance == null)
				Object.Instantiate(Resources.Load("uConsole"));

			uConsole.m_Instance.m_Activate = Main.config.hotkeyConsole;
		}
	}

	// weightless quest items
	[HarmonyPatch(typeof(GearItem), "GetItemWeightKG")]
	static class GearItem_GetItemWeightKG_Patch
	{
		static void Postfix(GearItem __instance, ref float __result)
		{
			if (__instance.m_NarrativeCollectibleItem)
				__result = 0f;
		}
	}

	// disable voice-overs
	[HarmonyPatch(typeof(PlayerVoice), "CanPlayPlayerVoiceEvents")]
	static class PlayerVoice_CanPlayPlayerVoiceEvents_Patch
	{
		static bool Prepare() => Main.config.disableVoiceOver;
		static bool Prefix(ref bool __result) => __result = false;
	}

	[PatchClass]
	static class GunPatches
	{
		// for cancelling reloading in the process
		[HarmonyPrefix, HarmonyPatch(typeof(vp_FPSPlayer), "Reload")]
		static void FPSPlayer_Reload_Prefix(vp_FPSPlayer __instance)
		{
			if (__instance.FPSCamera.CurrentWeapon.ReloadInProgress())
				__instance.FPSCamera.CurrentWeapon.m_BulletsToReload = 0;
		}

		[HarmonyPatch(typeof(GunItem), "Start")]
		static void GunItem_Start_Prefix(GunItem __instance) => __instance.m_FireDelayAfterReload = __instance.m_FireDelayOnAim = 0f;
	}

	// click to matches item in the inventory to set its condition to 100% (so they all can be stacked)
	[HarmonyPatch(typeof(ItemDescriptionPage), "CanExamine")]
	static class ItemDescriptionPage_CanExamine_Patch
	{
		static bool Prepare() => Main.config.stackMatches;

		static void Prefix(GearItem gi)
		{
			if (gi.name == "GEAR_WoodMatches" || gi.name == "GEAR_PackMatches")
				gi.m_CurrentHP = gi.m_MaxHP;
		}
	}

	// unlimited sleep with Control pressed
	[PatchClass]
	static class UnlimitedSleepPatches
	{
		static bool prepare() => Main.config.allowUnlimitedSleep;

		static bool unlimitedSleep = false;

		static bool controlPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_Rest), "Enable")]
		static void PanelRest_Enable_Postfix(Panel_Rest __instance, bool enable)
		{
			if (!enable)
				return;

			var descriptionLabel = __instance.gameObject.getChild("ControllerOffset/RestOnly/Label_Description");
			descriptionLabel.destroyComponent<UILocalize>();
			descriptionLabel.GetComponentInChildren<UILabel>().text = Localization.Get("GAMEPLAY_RestDescription") + "\n(press Control for unlimited sleep)";
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_Rest), "UpdateButtonLegend")]
		static void PanelRest_UpdateButtonLegend_Postfix(Panel_Rest __instance)
		{
			if (__instance.m_ShowPassTime)
				return;

			string label = Localization.Get("GAMEPLAY_Sleep") + (controlPressed? " (unlimited)": "");
			__instance.m_SleepButton.GetComponentInChildren<UILabel>().text = label;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_Rest), "OnRest")]
		static void PanelRest_OnRest_Postfix() => unlimitedSleep = controlPressed;

		[HarmonyPostfix, HarmonyPatch(typeof(Rest), "AllowUnlimitedSleep")]
		static void Rest_AllowUnlimitedSleep_Postfix(ref bool __result) => __result |= unlimitedSleep;

		[HarmonyPostfix, HarmonyPatch(typeof(Rest), "EndSleeping")]
		static void Rest_EndSleeping_Postfix() => unlimitedSleep = false; // just in case
	}
}