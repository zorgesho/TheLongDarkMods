using Harmony;
using UnityEngine;

using Common;

namespace PlaceFromInventory
{
	// allows to drop&place items as stack from drop panel
	[PatchClass]
	static class PanelPickPatches
	{
		static bool dropAsStack = false;
		static GearItem droppedItem;

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_PickUnits), "Update")]
		static void PanelPickUnits_Update_Postfix(Panel_PickUnits __instance)
		{
			if (!__instance.IsEnabled() || __instance.m_ExecuteAction != PickUnitsExecuteAction.Drop)
				return;

			dropAsStack = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			var text = dropAsStack? "Drop as stack": Localization.Get("GAMEPLAY_Drop");
			__instance.m_Execute_Button.GetComponentInChildren<UILabel>().text = text;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_PickUnits), "Refresh")]
		static void PanelPickUnits_Refresh_Postfix(Panel_PickUnits __instance)
		{
			if (__instance.m_ExecuteAction == PickUnitsExecuteAction.Drop)
				__instance.m_Label_Description.text += "\n(hold Control to drop as stack)";
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_PickUnits), "ExitInterface")]
		static void PanelPickUnits_ExitInterface_Postfix()
		{
			if (!droppedItem)
				return;

			UIHelper.startPlaceObject(droppedItem.gameObject);
			droppedItem = null;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_PickUnits), "DropGear")]
		static bool PanelPickUnits_DropGear_Prefix(Panel_PickUnits __instance)
		{
			if (__instance.m_ExecuteAction != PickUnitsExecuteAction.Drop || !dropAsStack)
				return true;

			droppedItem = __instance.m_GearItem.Drop(__instance.m_numUnits, true, true);
			return false;
		}
	}
}