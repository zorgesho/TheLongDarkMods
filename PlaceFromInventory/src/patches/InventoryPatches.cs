using HarmonyLib;
using UnityEngine;
using Common;

namespace PlaceFromInventory
{
	// allows to drop & place items with right click from inventory and clothing tabs
	[HarmonyPatch]
	static class InventoryPatches
	{
		static GearItem clothItemPlaceAfterDrop;

		const int delayAfterCancel = 10; // delay in frames after cancelling placement (to avoid right misclicks)
		static int lastFrameCancelled = 0; // last frame when placement was cancelled

		static bool shouldSkipClick => Time.frameCount - lastFrameCancelled < delayAfterCancel;

		[HarmonyPostfix, HarmonyPatch(typeof(InventoryGridItem), "OnClick")]
		static void InventoryGridItem_OnClick_Postfix(InventoryGridItem __instance)
		{																															$"InventoryGridItem.OnClick: shouldSkip = {shouldSkipClick}".logDbg();
			if (shouldSkipClick || !Input.GetMouseButtonUp(1) || InterfaceManager.m_Panel_Container.IsEnabled())
				return;

			if (!UIHelper.invPanel.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem) || __instance.m_GearItem.m_WaterSupply)
				return;

			UIHelper.startPlaceObject(__instance.m_GearItem.gameObject, PlaceMeshFlags.UpdateInventoryOnSuccess);
		}


		[HarmonyPostfix, HarmonyPatch(typeof(ClothingSlot), "DoClickAction")]
		static void ClothingSlot_DoClickAction_Postfix(ClothingSlot __instance)
		{																															$"ClothingSlot.DoClickAction: shouldSkip = {shouldSkipClick}".logDbg();
			if (shouldSkipClick || !Input.GetMouseButtonUp(1) || !__instance.m_GearItem)
				return;

			if (!UIHelper.clothPanel.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem))
				return;

			clothItemPlaceAfterDrop = __instance.m_GearItem;
			UIHelper.clothPanel.OnDropItem();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_Clothing), "OnDropItem")]
		static void PanelClothing_OnDropItem_Postfix()
		{
			if (!clothItemPlaceAfterDrop)
				return;

			UIHelper.startPlaceObject(clothItemPlaceAfterDrop.gameObject);
			clothItemPlaceAfterDrop = null;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
		static void PlayerManager_ExitMeshPlacement_Postfix(PlayerManager __instance)
		{																															$"PlayerManager.ExitMeshPlacement: cancelled = {!__instance.m_SkipCancel}".logDbg();
			if (!__instance.m_SkipCancel)
				lastFrameCancelled = Time.frameCount;

			UIHelper.restorePreviousPanel();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(PlayerManager), "DoPositionCheck")]
		static void PlayerManager_DoPositionCheck_Postfix(ref MeshLocationCategory __result)
		{
			if (Main.config.allowToPlaceItemsTooClose && __result == MeshLocationCategory.InvalidTooClose)
				__result = MeshLocationCategory.Valid;
		}
	}
}