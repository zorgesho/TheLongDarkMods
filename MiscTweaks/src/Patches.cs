using System.Text;

using HarmonyLib;
using UnityEngine;
using UnhollowerBaseLib;

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


	[HarmonyPatch]
	static class GunPatches
	{
		// cancel reloading in the process
		// don't drop revolver casings
		[HarmonyPrefix, HarmonyPatch(typeof(vp_FPSPlayer), "Reload")]
		static void FPSPlayer_Reload_Prefix(vp_FPSPlayer __instance)
		{
			var weaponFPS = __instance.FPSCamera.CurrentWeapon;
			var gun = weaponFPS.m_GearItem.m_GunItem;

			if (weaponFPS.ReloadInProgress())
				weaponFPS.m_BulletsToReload = 0;

			if (gun.m_GunType == GunType.Revolver && Main.config.addRevolverCasingsToInventory)
			{
				GameUtils.PlayerManager.InstantiateItemInPlayerInventory("GEAR_RevolverAmmoCasing", gun.m_SpentCasingsInClip);
				gun.ClearSpentCasings();
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(GunItem), "Start")]
		static void GunItem_Start_Postfix(GunItem __instance)
		{
			__instance.m_FireDelayAfterReload = __instance.m_FireDelayOnAim = 0f;

			if (__instance.m_GunType == GunType.Revolver)
				__instance.m_FiringRateSeconds = Main.config.revolverFiringRate;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(GunItem), "ZoomStart")]
		static void GunItem_ZoomStart_Postfix(GunItem __instance)
		{
			if (__instance.m_GunType == GunType.Revolver && Main.config.allowWalkWhileAimingRevolver)
				GameUtils.PlayerManager.SetControlMode(__instance.m_RestoreControlMode);
		}
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
	[HarmonyPatch]
	static class UnlimitedSleepPatches
	{
		static bool Prepare() => Main.config.allowUnlimitedSleep;

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


	// shows items for empty containers which could be instantiated there
	[HarmonyPatch]
	static class MissedItems
	{
		static readonly StringBuilder sb = new();

		static Container lastContainer;
		static Il2CppArrayBase<string> gearToInstantiate;

		static bool Prepare() => Main.config.dbgShowMissedItemsForEmptyContainers;

		static void setContainer(Container container)
		{
			lastContainer = container;
			gearToInstantiate = container?.m_GearToInstantiate.ToArray();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Container), "InstantiateContents")]
		static void Container_InstantiateContents_Prefix(Container __instance)
		{
			if (__instance.m_GearToInstantiate.Count != 0)
				setContainer(__instance);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Container), "OnContainerSearchComplete")]
		static void Container_OnContainerSearchComplete_Postfix(Container __instance)
		{
			if (__instance != lastContainer)
				return;

			if (__instance.m_Items.Count < gearToInstantiate.Length)
			{
				sb.Clear();

				gearToInstantiate.forEach(item => sb.Append(item + " "));

				sb.ToString().onScreen(true);
			}

			setContainer(null);
		}
	}


	// indoor heat sources cools down slower (doesn't goes to save)
	[HarmonyPatch(typeof(HeatSource), "Update")]
	static class SlowerCoolDown
	{
		static bool Prepare() => Main.config.slowerCoolDown < 1.0f;

		static bool isIndoorAndCoolDown(this HeatSource hs) =>
			GameManager.GetWeatherComponent().IsIndoorScene() && !hs.IsTurnedOn() && hs.m_TempIncrease > 0f;

		static float prevTemp = 0f;

		static void Prefix(HeatSource __instance)
		{
			if (__instance.isIndoorAndCoolDown())
				prevTemp = __instance.m_TempIncrease;
		}

		static void Postfix(HeatSource __instance)
		{
			if (__instance.isIndoorAndCoolDown())
				__instance.m_TempIncrease = prevTemp - (prevTemp - __instance.m_TempIncrease) * Main.config.slowerCoolDown;
		}
	}
}