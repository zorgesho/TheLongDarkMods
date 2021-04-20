using System;
using System.Linq;

using Harmony;
using UnityEngine;

using Common;

namespace MovableContainers
{
	static class UIManager
	{
		static readonly Color colorUI = Color.yellow;
		public static readonly string colorText = colorUI.toStringRGB();

		static GameObject pickupButton;
		static GameObject containerIcon;

		static bool containerIconVisible = false;

		static void init(GameObject panelHUD)
		{
			containerIcon = createContainerIcon(panelHUD);
			pickupButton = createPickupButton(panelHUD.getChild("NonEssentialHud/EquipItemPopup"));
		}

		static GameObject createPickupButton(GameObject panelPopup)
		{
			var prefab = panelPopup.getChild("EquipWidget/BottomCenter/Keys/PromptLeft");

			var btn = prefab.getParent().createChild(prefab, "PromptPickupContainer", localPos: new Vector2(-48f, -70f));
			btn.SetActive(false);

			return btn;
		}

		static GameObject createContainerIcon(GameObject panelHUD)
		{
			var prefab = panelHUD.getChild("NonEssentialHud/BottomRightAnchor/Sprite_CapacityBuff");

			var iconPos = prefab.transform.localPosition + new Vector3(3f, -30f);
			var icon = prefab.getParent().createChild(prefab, "Sprite_PickedContainer", localPos: iconPos, localScale: Vector2.one * 2f);

			var sprite = icon.GetComponent<UISprite>();
			sprite.enabled = true;
			sprite.spriteName = "icoMap_container";
			sprite.color = colorUI;

			icon.SetActive(false);
			return icon;
		}

		public static void setPickupButtonVisible(bool visible, bool enabled = true)
		{
			pickupButton.SetActive(visible);
			pickupButton.GetComponent<ButtonPrompt>().ShowPromptForKey("pick up", "OpenRadial");

			var color = enabled? colorUI: Color.gray;
			pickupButton.forEachComponentInChildren<UILabel>(label => label.color = color);
			pickupButton.forEachComponentInChildren<UISprite>(sprite => sprite.color = color);
		}

		public static void setContainerIconVisible(bool visible)
		{
			containerIconVisible = visible;
			containerIcon.SetActive(visible);
		}

		static void modifyInvWeightString(ref string str)
		{
			float addedWeight = MovableContainerManager.pickedContainerWeight;

			if (addedWeight == 0f)
				return;

			if (InterfaceManager.m_Panel_OptionsMenu.m_State.m_Units == MeasurementUnits.Imperial)
				addedWeight = Utils.KilogramsToPounds(addedWeight);

			str = str.Insert(str.IndexOf('/') - 1, $"[{colorText}] (+{addedWeight:F2})[-]");
		}

		[PatchClass]
		static class Patches
		{
			[HarmonyPostfix, HarmonyPatch(typeof(Panel_HUD), "Awake")]
			static void PanelHUD_Awake_Postfix(Panel_HUD __instance) => init(__instance.gameObject);

			[HarmonyPostfix, HarmonyPatch(typeof(Encumber), "GetPlayerCarryCapacityString")]
			static void Encumber_GetPlayerCarryCapacityString_Postfix(ref string __result) => modifyInvWeightString(ref __result);

			[HarmonyPostfix] // :(
			[HarmonyPatch(typeof(Panel_Log), "Enable")]
			[HarmonyPatch(typeof(Panel_Map), "Enable")]
			[HarmonyPatch(typeof(Panel_Crafting), "Enable")]
			[HarmonyPatch(typeof(Panel_Inventory), "Enable")]
			static void Panel_Enable_Postfix(bool enable) => containerIcon?.SetActive(containerIconVisible && !enable);

			[HarmonyPostfix, HarmonyPatch(typeof(Panel_ActionsRadial), "ShowPlaceItemRadial")]
			static void PanelActionsRadial_ShowPlaceItemRadial_Postfix(Panel_ActionsRadial __instance)
			{
				if (!containerIconVisible)
					return;

				if (__instance.m_RadialArms.FirstOrDefault(arm => arm.m_RadialInfo.m_RadialElement == Panel_ActionsRadial.RadialType.Shelter) is not RadialMenuArm arm)
					return;

				Panel_ActionsRadial.RadialInfo info = new() { m_RadialElement = Panel_ActionsRadial.RadialType.Shelter };
				info.m_SpriteName = info.m_SpriteNameHover = "icoMap_container";
				info.m_IconActiveColor = info.m_IconActiveHoverColor = colorUI;

				arm.SetRadialInfo(info, new Action(() => MovableContainerManager.dropContainer()), false, false);
				arm.m_NameWhenHoveredOver = "place " + Localization.Get(MovableContainerManager.pickedContainerName);
			}
		}
	}
}