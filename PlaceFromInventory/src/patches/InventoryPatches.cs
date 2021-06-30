using HarmonyLib;
using UnityEngine;

namespace PlaceFromInventory
{
	// allows to drop & place items with right click from inventory and clothing tabs
	[HarmonyPatch]
	static class InventoryPatches
	{
		static GearItem clothItemPlaceAfterDrop;

		[HarmonyPostfix, HarmonyPatch(typeof(InventoryGridItem), "OnClick")]
		static void InventoryGridItem_OnClick_Postfix(InventoryGridItem __instance)
		{
			if (!Input.GetMouseButtonUp(1) || InterfaceManager.m_Panel_Container.IsEnabled())
				return;

			if (!UIHelper.invPanel.m_ItemDescriptionPage.CanDrop(__instance.m_GearItem) || __instance.m_GearItem.m_WaterSupply)
				return;

			UIHelper.startPlaceObject(__instance.m_GearItem.gameObject, PlaceMeshFlags.UpdateInventoryOnSuccess);
		}


		[HarmonyPostfix, HarmonyPatch(typeof(ClothingSlot), "DoClickAction")]
		static void ClothingSlot_DoClickAction_Postfix(ClothingSlot __instance)
		{
			if (!Input.GetMouseButtonUp(1) || !__instance.m_GearItem)
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
		static void PlayerManager_ExitMeshPlacement_Postfix()
		{
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