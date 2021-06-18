using HarmonyLib;
using UnityEngine;

namespace PlaceFromInventory
{
	// show number of stacked items on mouse hover
	[HarmonyPatch(typeof(PlayerManager), "GetInteractiveObjectDisplayText")]
	static class PlayerManager_GetInteractiveObjectDisplayText_Patch
	{
		static bool Prepare() => Main.config.showItemsQuantity;

		static void Postfix(GameObject interactiveObject, ref string __result)
		{
			if (interactiveObject.GetComponent<GearItem>() is GearItem gearItem && gearItem.m_StackableItem?.m_Units > 1)
			{
				var countStr = $" [x{gearItem.m_StackableItem.m_Units}]";

				int nextLine = __result.IndexOf('\n');
				if (nextLine != -1)
					__result = __result.Insert(nextLine, countStr);
				else
					__result += countStr;
			}
		}
	}
}