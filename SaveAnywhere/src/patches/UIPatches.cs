﻿using System;
using System.Collections.Generic;

using HarmonyLib;
using UnityEngine;
using UnhollowerRuntimeLib;

using Common;

namespace SaveAnywhere
{
	using Object = UnityEngine.Object;

	[HarmonyPatch]
	static class UIPatches
	{
		static bool backToPauseMenu = false;
		static bool preventClearingSaveSlotsLists = false;

		[HarmonyPrefix, HarmonyPatch(typeof(SaveGameSlotHelper), "ClearSaveSlotsLists")]
		static bool SaveGameSlotHelper_ClearSaveSlotsLists_Prefix() => !preventClearingSaveSlotsLists;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "Enable")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "Enable")]
		static void PanelChoose_Enable_Postfix(bool enable)
		{
			if (!enable)
				backToPauseMenu = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "OnClickBack")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "OnClickBack")]
		static bool PanelChoose_OnClickBack_Prefix(Panel_Base __instance)
		{
			if (!backToPauseMenu)
				return true;

			GameAudioManager.PlayGUIButtonBack();
			__instance.enable(false);

			InterfaceManager.m_Panel_PauseMenu.Enable(true);
			return false;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_MainMenu), "OnLoadSaveSlot")]
		static void PanelMainMenu_OnLoadSaveSlot_Prefix(SaveSlotType saveSlotType)
		{
			if (GameUtils.isMainMenu())
				return;

			if (saveSlotType is not (SaveSlotType.CHALLENGE or SaveSlotType.SANDBOX))
				return;

			preventClearingSaveSlotsLists = true;

			if (GameManager.m_PlayerObject)
			{																										"Destroying player object".logDbg();
				Object.DestroyImmediate(GameManager.m_PlayerObject);
			}

			InterfaceManager.m_Panel_PauseMenu.DoQuitGame();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_MainMenu), "OnLoadSaveSlot")]
		static void PanelMainMenu_OnLoadSaveSlot_Postfix(SaveSlotType saveSlotType, int slotIndex)
		{																											$"Panel_MainMenu.OnLoadSaveSlot slotType: {saveSlotType} slotIndex: {slotIndex}".logDbg();
			SaveGameSlotHelper.RefreshSaveSlots(saveSlotType, true);
			string slotName = SaveGameSlotHelper.GetSaveSlotInfo(saveSlotType, slotIndex).m_SaveSlotName;			$"Panel_MainMenu.OnLoadSaveSlot slotName: {slotName}".logDbg();
			SaveLoad.tryRestoreOriginalSlot(slotName);

			preventClearingSaveSlotsLists = false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "DeleteSaveSlot")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "DeleteSaveSlot")]
		static void PanelChoose_DeleteSaveSlot_Prefix(Panel_Base __instance)
		{
			SaveLoad.removeSlotInfo(__instance.getSlotToDelete().m_SaveSlotName);
		}

		// creating UI object for original slot info
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "Initialize")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "Initialize")]
		static void PanelChoose_Initialize_Postfix(Panel_Base __instance)
		{
			var details = __instance.getDetails().m_Details;
			var prefab = details.getChild("Texts/Date");

			var originalSlot = details.getChild("Texts").createChild(prefab, "OriginalSlot", pos: prefab.transform.position + new Vector3(0.45f, 0f, 0f));
			originalSlot.getChild("DateValue").name = "SlotValue";
		}

		// updating original slot info on the load panel
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "Update")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "Update")]
		static void PanelChoose_Update_Prefix(Panel_Base __instance)
		{
			if (__instance.getBasicMenu().m_ItemModelList.Count == 0)
			{
				__instance.onClickBack();
				return;
			}

			string originalSlot = SaveLoad.getOriginalSlot(__instance.getBasicMenu().GetSelectedItemId());

			var slotInfoGO = __instance.getDetails().m_Details.getChild("Texts/OriginalSlot");
			slotInfoGO.SetActive(originalSlot != null);

			if (originalSlot != null)
			{
				slotInfoGO.getChild("Header").GetComponent<UILabel>().text = "original save"; // need to update this every time too
				slotInfoGO.getChild("SlotValue").GetComponent<UILabel>().text = SaveLoad.getSlotDisplayName(originalSlot);
			}
		}

		// don't allow to rename quicksave slot, allow to delete original saves only in the main menu, fix Escape key
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "Update")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "Update")]
		static void PanelChoose_Update_Postfix(Panel_Base __instance)
		{
			string selectedSlot = __instance.getBasicMenu().GetSelectedItemId();

			Utils.SetActive(__instance.getRenameButton(), selectedSlot != SaveLoad.getQuicksaveSlot(__instance.getSlotType()));

			if (!GameUtils.isMainMenu())
				Utils.SetActive(__instance.getDeleteButton(), !SaveLoad.isOriginalSlot(selectedSlot));

			// for some reason Escape key is not working if we show this panel not from the main menu (probably InputManager context)
			if (backToPauseMenu && InputHelper.isKeyDown(KeyCode.Escape))
				__instance.onClickBack();
		}

		// splitting original and non-original saves on the load panels
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Panel_ChooseSandbox), "AddSavesOfTypeToMenu")]
		[HarmonyPatch(typeof(Panel_ChooseChallenge), "AddSavesOfTypeToMenu")]
		static void PanelChoose_AddSavesOfTypeToMenu_Postfix(Panel_Base __instance)
		{
			int targetIndex = 0;
			var menu = __instance.getBasicMenu();

			var quickslot = SaveLoad.getQuicksaveSlot(__instance.getSlotType());
			int qsIndex = menu.m_ItemModelList.findIndex(item => item.m_Id == quickslot);

			if (qsIndex != -1)
			{
				menu.moveItemTo(0, qsIndex); // quicksave is always first
				targetIndex++;
			}

			List<BasicMenu.BasicMenuItemModel> itemsToMove = new();

			for (int i = menu.m_ItemModelList.Count - 1; i >= 0; i--)
			{
				var item = menu.m_ItemModelList[i];

				if (item.m_Id == quickslot || SaveLoad.getOriginalSlot(item.m_Id) == null)
					continue;

				itemsToMove.Add(item);
				menu.m_ItemModelList.RemoveAt(i);
			}

			// moving custom saves up
			itemsToMove.ForEach(item => menu.m_ItemModelList.Insert(targetIndex, item));
			targetIndex += itemsToMove.Count;

			if (targetIndex != 0)
			{
				menu.AddLineBreak();
				menu.moveItemTo(targetIndex);
			}
		}

		// adding "LOAD GAME" button to the pause menu
		[HarmonyPostfix, HarmonyPatch(typeof(Panel_PauseMenu), "ConfigureMenu")]
		static void PanelPauseMenu_ConfigureMenu_Postfix(Panel_PauseMenu __instance)
		{
			if (GameManager.IsStoryMode())
				return;

			Action action = new (() =>
			{
				backToPauseMenu = true;
				InterfaceManager.m_Panel_PauseMenu.Enable(false);

				SaveGameSlotHelper.RefreshSaveSlots(Main.gameType, true);

				if (Main.gameType == SaveSlotType.SANDBOX)
					InterfaceManager.GetPanel<Panel_Sandbox>().OnClickLoad();
				else
					InterfaceManager.GetPanel<Panel_Challenges>().OnClickLoad();
			});

			Color defaultColor = new (0f, 0f, 0f, 0f);

			__instance.m_BasicMenu.AddItem("LoadGame", 0, 0, Localization.Get("GAMEPLAY_LoadGame"), Localization.Get("GAMEPLAY_DescriptionLoadGame"), null, action, defaultColor, defaultColor);
			__instance.m_BasicMenu.moveItemTo(2);
		}

		[HarmonyPatch]
		static class MainMenuPanelsUnloadBlocker
		{
			static List<Il2CppSystem.Type> keepLoaded = null;
			static bool shouldKeepLoaded(Il2CppSystem.Type panelType) => keepLoaded?.FindIndex(type => type == panelType) >= 0; // can't use 'Contains' here

			[HarmonyPrefix, HarmonyPatch(typeof(InterfaceManager), "UnloadMainMenuPanels")]
			static void InterfaceManager_UnloadMainMenuPanels_Prefix()
			{
				keepLoaded = Main.gameType switch
				{
					SaveSlotType.SANDBOX => new() { Il2CppType.Of<Panel_Sandbox>(), Il2CppType.Of<Panel_ChooseSandbox>() },
					SaveSlotType.CHALLENGE => new() { Il2CppType.Of<Panel_Challenges>(), Il2CppType.Of<Panel_ChooseChallenge>() },
					_ => null
				};

				keepLoaded?.Add(Il2CppType.Of<Panel_MainMenu>());
			}

			[HarmonyPostfix, HarmonyPatch(typeof(InterfaceManager), "UnloadMainMenuPanels")]
			static void InterfaceManager_UnloadMainMenuPanels_Postfix()
			{
				keepLoaded = null;
			}

			[HarmonyPrefix, HarmonyPatch(typeof(InterfaceManager), "TryDestroyPanel_Internal")]
			static bool InterfaceManager_TryDestroyPanelInternal_Prefix(Il2CppSystem.Type panelType)
			{																										$"InterfaceManager.TryDestroyPanel: prevent destroying {panelType.Name}".logDbg(shouldKeepLoaded(panelType));
				return !shouldKeepLoaded(panelType);
			}
		}
	}
}