using System;
using System.Linq;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.Configuration;

namespace SaveAnywhere
{
	using Object = UnityEngine.Object;

	static class SaveLoad
	{
		const string qsSlotDisplayName = "quicksave";

		class SaveSlotsConfig: Config
		{
			public Dictionary<SaveSlotType, string> quicksaveSlot = new();
			public Dictionary<string, string> originalSlots = new(); // key - custom slot, value - original slot
		}
		static readonly SaveSlotsConfig slotsConfig = Config.tryLoad<SaveSlotsConfig>(PersistentDataPath.m_Path + "\\save-slots", Config.LoadOptions.ForcedLoad);

		public static string getQuicksaveSlot(SaveSlotType type) => slotsConfig.quicksaveSlot.TryGetValue(type, out string slot)? slot: null;

		public static string getCurrentSlot() => SaveGameSystem.m_CurrentSaveName;
		public static string getSlotDisplayName(string slot) => SaveGameSlots.GetSaveSlotFromName(slot)?.m_DisplayName;

		public static  bool isOriginalSlot(string slot) =>
			slotsConfig.originalSlots.Any(link => link.Value == slot) || !slotsConfig.originalSlots.Any(link => link.Key == slot);

		public static void updateSlots(SaveSlotType type)
		{
			if (type == SaveSlotType.CHALLENGE)
				InterfaceManager.m_Panel_MainMenu.m_ChallengeSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, SaveSlotType.CHALLENGE);
			else if (type == SaveSlotType.SANDBOX)
				InterfaceManager.m_Panel_MainMenu.m_SandboxSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, SaveSlotType.SANDBOX);
		}

		static bool setCurrentSlot(string slot)
		{
			if (!SaveGameSlots.HasSaveSlot(slot))
				return false;

			var type = SaveGameSlots.GetSaveSlotTypeFromName(slot);
			uint id = SaveGameSlots.GetSaveSlotId(slot);
			SaveGameSystem.SetCurrentSaveInfo(Episode.One, type, id, slot);															$"Current slot is set to {slot}".logDbg();

			return true;
		}

		static int getSlotIndex(string slot)
		{
			var mainmenu = InterfaceManager.m_Panel_MainMenu;
			var slots = SaveGameSlots.GetSaveSlotTypeFromName(slot) == SaveSlotType.CHALLENGE? mainmenu.m_ChallengeSaveSlots: mainmenu.m_SandboxSlots;

			return slots.findIndex(slotInfo => slotInfo.m_SaveSlotName == slot);
		}

		static string createNewSlot(string displayName, bool setCurrent = true)
		{																															$"Creating new save slot '{displayName}'".logDbg();
			string prevSlot = getCurrentSlot();

			SaveGameSystem.SetCurrentSaveInfo(Episode.One, Main.gameType, SaveGameSlots.GetUnusedGameId(), null);
			SaveGameSlots.SetSlotDisplayName(SaveGameSystem.m_CurrentSaveName, displayName);

			var newSlot = getCurrentSlot();

			if (!setCurrent)
				setCurrentSlot(prevSlot);
																																	$"New save slot created ({newSlot})".logDbg();
			return newSlot;
		}

		public static string getOriginalSlot(string slot) =>
			slotsConfig.originalSlots.TryGetValue(slot, out string originalSlot)? originalSlot: null;

		public static void tryRestoreOriginalSlot(string currentSlot)
		{
			if (getOriginalSlot(currentSlot) is string originalSlot)
				setCurrentSlot(originalSlot);
		}

		public static void removeSlotInfo(string slot)
		{
			slotsConfig.originalSlots.Remove(slot);

			var type = SaveGameSlots.GetSaveSlotTypeFromName(slot);
			if (slotsConfig.quicksaveSlot.TryGetValue(type, out string qsSlot) && qsSlot == slot)
				slotsConfig.quicksaveSlot.Remove(type);

			slotsConfig.save();
		}

		static string slotToRestore = null;

		public static void save(string displayName = qsSlotDisplayName)
		{
			if (slotToRestore != null || GameUtils.isMainMenu())
				return;
																																	$"Saving to the slot '{displayName}'".logDbg();
			slotToRestore = getCurrentSlot();
																																	$"Current slot: {slotToRestore}".logDbg();
			if (displayName == qsSlotDisplayName)
			{
				bool needNewSlot = true;
				var quicksaveSlot = getQuicksaveSlot(Main.gameType);

				if (!quicksaveSlot.isNullOrEmpty())
					needNewSlot = !setCurrentSlot(quicksaveSlot);

				if (needNewSlot)
					slotsConfig.quicksaveSlot[Main.gameType] = createNewSlot(qsSlotDisplayName);
			}
			else
			{
				createNewSlot(displayName);
			}

			slotsConfig.originalSlots[getCurrentSlot()] = slotToRestore; // store original slot
			slotsConfig.save();

			GameManager.SaveGameAndDisplayHUDMessage();
		}

		// load from quicksave slot
		public static void load()
		{
			if (GameUtils.isMainMenu())
				return;

			updateSlots(Main.gameType);

			int slotIndex = getSlotIndex(getQuicksaveSlot(Main.gameType));

			if (slotIndex == -1)
				return;

			Action action = new(() =>
			{
				if (Main.gameType == SaveSlotType.CHALLENGE)
					InterfaceManager.m_Panel_MainMenu.OnLoadChallengeMode(slotIndex);
				else
					InterfaceManager.m_Panel_MainMenu.OnLoadSandboxMode(slotIndex);
			});

			CameraFade.StartAlphaFade(Color.black, false, GameManager.m_SceneTransitionFadeOutTime, 0f, action);
		}

		[PatchClass]
		static class Patches
		{
			static SandboxManager sbm = null; // to destroy sandbox manager immediate

			[HarmonyPrefix, HarmonyPatch(typeof(GameManager), "DestroySandboxManager")]
			static void GameManager_DestroySandboxManager_Prefix() => sbm = GameManager.m_SandboxManager;

			[HarmonyPostfix, HarmonyPatch(typeof(GameManager), "DestroySandboxManager")]
			static void GameManager_DestroySandboxManager_Postfix()
			{																															$"GameManager.DestroySandboxManager".logDbg();
				if (sbm)
				{
					Object.DestroyImmediate(sbm.gameObject);
					sbm = null;
				}
			}

			[HarmonyPostfix, HarmonyPatch(typeof(GameManager), "ForceSaveGame")]
			static void GameManager_ForceSaveGame_Postfix()
			{
				if (slotToRestore == null)
					return;

				setCurrentSlot(slotToRestore);
				slotToRestore = null;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(SaveGameSlots), "DeleteSlot", typeof(SlotData))]
			static void SaveGameSlots_DeleteSlot_Postfix(SlotData slotData)
			{																															$"Deleting save slot {slotData.m_Name}".logDbg();
				if (!Main.config.removeCustomSavesWithOriginal)
					return;
																																		$"Trying to delete linked slots".logDbg();
				List<string> linksToRemove = new();

				foreach (var link in slotsConfig.originalSlots)
				{
					if (link.Value == slotData.m_Name)
					{
						linksToRemove.Add(link.Key);
						SaveGameSlots.DeleteSlot(link.Key);
					}
				}

				linksToRemove.ForEach(removeSlotInfo);
				updateSlots(SaveGameSlots.GetSaveSlotTypeFromName(slotData.m_Name));
			}
		}
	}
}