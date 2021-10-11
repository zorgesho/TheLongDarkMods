#if DEBUG
using System.Diagnostics;

using HarmonyLib;
using UnityEngine;

using Common;

namespace SaveAnywhere
{
	[HarmonyPatch]
	static class DebugPatches
	{
		static void _logMissions(this MissionServicesManager msm)
		{
			foreach (var mission in msm.m_AllMissions)
				$"MissionServicesManager.m_AllMissions: {mission.Key} {mission.Value.m_MissionId + "_" + mission.Value.m_FSMHierarchy.GetJumpString()}".logDbg();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(MissionServicesManager), "Serialize")]
		static void MissionServicesManager_Serialize_Postfix(MissionServicesManager __instance)
		{
			int srzCount = MissionServicesManager.m_MissionServicesManagerSaveProxy.m_SerializedTimers.Count / 3;
			$"MissionServicesManager.Serialize: timers count {__instance.m_MissionTimers.Count} / serialized: {srzCount}".logDbg();

			__instance._logMissions();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MissionServicesManager), "Deserialize")]
		static void MissionServicesManager_Deserialize_Prefix()
		{
			"MissionServicesManager.Deserialize: begin".logDbg();
		}

		[HarmonyPostfix, HarmonyPatch(typeof(MissionServicesManager), "Deserialize")]
		static void MissionServicesManager_Deserialize_Postfix(MissionServicesManager __instance)
		{
			__instance._logMissions();
			"MissionServicesManager.Deserialize: end".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(vp_FPSPlayer), "Awake")]
		static void VPFPSPlayer_Awake_Prefix()
		{
			"vp_FPSPlayer.Awake".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MissionServicesManager), "DeserializeTimers")]
		static void MissionServicesManager_DeserializeTimers_Prefix(MissionServicesManagerSaveProxy proxy)
		{
			$"MissionServicesManager.DeserializeTimers: timers count {proxy.m_SerializedTimers.Count / 3}".logDbg();

			if (!Main.config.dbgLogTimerDetails)
				return;

			string _info(int i) => proxy.m_SerializedTimers[i];

			for (int i = 0; i < proxy.m_SerializedTimers.Count; i += 3)
				$"\tTimer {i/3}: {_info(i)} | {_info(i + 1)} | {_info(i + 2)}".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MissionServicesManager), "StartMissionTimer")]
		static void MissionServicesManager_StartMissionTimer_Prefix(string name)
		{
			$"MissionServicesManager.StartMissionTimer: {name}".onScreen().log();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MissionServicesManager), "RemoveMissionTimer", typeof(string), typeof(bool))]
		static void MissionServicesManager_RemoveMissionTimer_Prefix(string name)
		{
			$"MissionServicesManager.RemoveMissionTimer: {name}".onScreen().log();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(GameManager), "InstantiateSandboxManager")]
		static void GameManager_InstantiateSandboxManager_Prefix()
		{
			bool sbmAlreadyExists = GameObject.Find(GameManager.m_Instance.m_SandboxManagerPrefab.name); 
			$"GameManager.InstantiateSandboxManager{(sbmAlreadyExists? ": SandboxManager is already exists!": "")}".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(GameManager), "LoadGame")]
		static void GameManager_LoadGame_Prefix()
		{
			$"GameManager.LoadGame".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(GameManager), "LoadSaveGameSlot", typeof(string), typeof(int))]
		static void GameManager_LoadSaveGameSlot_Prefix(string slotName)
		{
			if (Main.config.dbgDumpSaveSlotOnLoad)
				SaveSlotDumper.dump(slotName);
		}

		[HarmonyPostfix, HarmonyPatch(typeof(SaveGameSystem), "SaveGame")]
		static void SaveGameSystem_SaveGame_Postfix(string name)
		{
			if (Main.config.dbgDumpSaveSlotOnSave)
				SaveSlotDumper.dump(name);
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_MainMenu), "OnLoadGame")]
		static void PanelMainMenu_OnLoadGame_Prefix(SaveSlotType saveSlotType, int slotIndex)
		{
			$"Panel_MainMenu.OnLoadGame: saveSlotType {saveSlotType}, slotIndex {slotIndex}".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(ExperienceModeManager), "SetExperienceModeType")]
		static void ExperienceModeManager_SetExperienceModeType_Prefix(ExperienceModeType modeType)
		{
			$"ExperienceModeManager.SetExperienceModeType: {modeType}".logDbg();
		}

		[HarmonyPatch(typeof(SaveGameSlots), "CreateSaveSlotInfo")]
		static class SaveGameSlots_CreateSaveSlotInfo_Patch
		{
			static Stopwatch watch;

			static void Prefix()
			{
				watch = Stopwatch.StartNew();
			}

			static void Postfix(SlotData slot)
			{
				watch.Stop();
				$"SaveGameSlots.CreateSaveSlotInfo: {slot.m_Name} {watch.Elapsed.TotalMilliseconds} ms".logDbg();
			}
		}
	}
}
#endif // DEBUG