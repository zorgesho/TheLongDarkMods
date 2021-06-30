using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;

using Common;

namespace MovableContainers
{
	static class PickPlaceHelper
	{
		public static bool placingContainer { get; private set; } = false;

		static Quaternion rotation;
		static float addedWeight = 0f;
		static readonly List<Collider> colliders = new();

		public static bool tryEnterPlaceMode(GameObject go)
		{																								$"PickPlaceHelper: {go.name} {go.GetComponent<Container>()?.m_LocalizedDisplayName.m_LocalizationID}".logDbg();
			if (!MovableContainerManager.isAllowedType(go))
				return false;

			go.transform.localEulerAngles = new Vector3(0f, go.transform.localEulerAngles.y, 0f);
			addedWeight = go.GetComponent<Container>().GetTotalWeightKG() + (MovableContainerManager.getContainerProps(go)?.weight ?? 0f);

			placingContainer = true;
			rotation = go.transform.rotation;

			GameUtils.PlayerManager.StartPlaceMesh(go, 5f, PlaceMeshFlags.None);
			UIManager.setPickupButtonVisible(true, MovableContainerManager.isAllowedToPickup(go).allowed);

			if (go.GetComponent<ObjectGuid>()?.m_Guid is string guid)
				MovableContainerManager.addMoved(guid);

			go.forEachComponentInChildren<Collider>(collider =>
			{
				if (collider.enabled)
				{
					colliders.Add(collider);
					collider.enabled = false;
				}
			});

			return true;
		}

		static void exitPlaceMode()
		{
			if (GameUtils.PlayerManager.m_ObjectToPlace)
				GameUtils.PlayerManager.ExitMeshPlacement();

			colliders.ForEach(c => c.enabled = true);
			colliders.Clear();

			UIManager.setPickupButtonVisible(false);
			placingContainer = false;
			addedWeight = 0f;
		}

		static void update(EquipItemPopup popup)
		{
			if (placingContainer && InputManager.GetRadialButton(popup))
			{
				if (MovableContainerManager.tryPickUpContainer(GameUtils.PlayerManager.m_ObjectToPlace))
					exitPlaceMode();
			}
		}

		[HarmonyPatch]
		static class Patches
		{
			[HarmonyPrefix, HarmonyPatch(typeof(EquipItemPopup), "Update")]
			static void EquipItemPopup_Update_Prefix(EquipItemPopup __instance) => update(__instance);

			[HarmonyPostfix, HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
			static void PlayerManager_ExitMeshPlacement_Postfix() => exitPlaceMode();

			[HarmonyPostfix, HarmonyPatch(typeof(Inventory), "GetExtraWeightKG")]
			static void Inventory_GetExtraWeightKG_Postfix(ref float __result) => __result += addedWeight;

			[HarmonyPrefix, HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessAltFire")]
			static bool PlayerManager_InteractiveObjectsProcessAltFire_Prefix(PlayerManager __instance) =>
				!tryEnterPlaceMode(__instance.m_InteractiveObjectUnderCrosshair);

			[HarmonyPostfix, HarmonyPatch(typeof(PlayerManager), "DoPositionCheck")]
			static void PlayerManager_DoPositionCheck_Postfix(PlayerManager __instance)
			{
				if (placingContainer)
					__instance.m_ObjectToPlace.transform.rotation = rotation;
			}
		}
	}
}