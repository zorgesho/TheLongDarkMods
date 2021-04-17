﻿#if DEBUG
using System.Diagnostics;

using Harmony;
using UnityEngine;
using Newtonsoft.Json;

using Common;

namespace SaveAnywhere
{
	[PatchClass]
	static class DebugPatches
	{
		static readonly bool dumpSaveSlotOnLoad = false;
		static readonly bool printTimerDetails = false;

		static void dumpSaveSlot(string slotName)
		{
			if (!(SaveGameSlots.GetSaveSlotFromName(slotName) is SlotData slotData))
				return;

			foreach (var key in slotData.m_Dict.Keys)
			{
				string slotStr = SaveGameSlots.LoadDataFromSlot(slotName, key);
				string slotStrFormatted = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(slotStr), new JsonSerializerSettings() { Formatting = Formatting.Indented });
				slotStrFormatted.saveToFile($"{PersistentDataPath.m_Path}\\saves-dump\\{slotName}-{key}.json");
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(MissionServicesManager), "Serialize")]
		static void MissionServicesManager_Serialize_Postfix(MissionServicesManager __instance)
		{
			int srzCount = MissionServicesManager.m_MissionServicesManagerSaveProxy.m_SerializedTimers.Count / 3;
			$"MissionServicesManager.Serialize: timers count {__instance.m_MissionTimers.Count} / serialized: {srzCount}".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(MissionServicesManager), "DeserializeTimers")]
		static void MissionServicesManager_DeserializeTimers_Prefix(MissionServicesManagerSaveProxy proxy)
		{
			$"MissionServicesManager.DeserializeTimers: timers count {proxy.m_SerializedTimers.Count / 3}".logDbg();

			if (!printTimerDetails)
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
			const string msg = "GameManager.InstantiateSandboxManager";

			if (GameObject.Find(GameManager.m_Instance.m_SandboxManagerPrefab.name))
				$"{msg}: SandboxManager is already exists!".logWarning();
			else
				msg.logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(GameManager), "LoadGame")]
		static void GameManager_LoadGame_Prefix()
		{
			$"GameManager.LoadGame".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(GameManager), "LoadSaveGameSlot")]
		static void GameManager_LoadSaveGameSlot_Prefix(string slotName)
		{
			if (dumpSaveSlotOnLoad)
				dumpSaveSlot(slotName);
		}

		[HarmonyPrefix, HarmonyPatch(typeof(SandboxManager), "Serialize")]
		static void SandboxManager_Serialize_Prefix()
		{
			$"SandboxManager.Serialize".logDbg();
		}

		[HarmonyPrefix, HarmonyPatch(typeof(SandboxManager), "Deserialize")]
		static void SandboxManager_Deserialize_Prefix()
		{
			$"SandboxManager.Deserialize".logDbg();
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
#endif