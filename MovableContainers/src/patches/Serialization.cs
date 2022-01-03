using System.Linq;

using HarmonyLib;
using Newtonsoft.Json;

using Common;

namespace MovableContainers
{
	static partial class MovableContainerManager
	{
		[HarmonyPatch]
		static class SerializationPatches
		{
			const string savename = "moved-containers-info";

			class GlobalSaveData
			{
				public string pickedContainerType;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData")]
			static void SaveGameSystem_SaveGlobalData_Postfix(SaveSlotType gameMode, string name)
			{																																	$"SaveGameSystem.SaveGlobalData {name}".logDbg();
				string text = JsonConvert.SerializeObject(new GlobalSaveData() { pickedContainerType = pickedContainerType });
				SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, savename, text);

				if (info == null) // in case of a new world we need to init info here
					initContainersInfo(null);
			}

			[HarmonyPrefix, HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData")]
			static void SaveGameSystem_RestoreGlobalData_Prefix(string name)
			{																																	$"SaveGameSystem.RestoreGlobalData {name}".logDbg();
				string text = SaveGameSlots.LoadDataFromSlot(name, savename);
				string containerType = text == null? null: JsonConvert.DeserializeObject<GlobalSaveData>(text).pickedContainerType;

				pickUpContainer(containerType);
			}


			[HarmonyPrefix, HarmonyPatch(typeof(SaveGameSystem), "LoadSceneData")]
			static void SaveGameSystem_LoadSceneData_Prefix(string name, string sceneSaveName)
			{																																	$"Loading scene {name}:{sceneSaveName}".logDbg();
				string text = SaveGameSlots.LoadDataFromSlot(name, $"{sceneSaveName}-{savename}");
				initContainersInfo(text == null? null: JsonConvert.DeserializeObject<MovedContainersInfo>(text));
			}

			[HarmonyPrefix, HarmonyPatch(typeof(SaveGameSystem), "SaveSceneData")]
			static void SaveGameSystem_SaveSceneData_Prefix(SaveSlotType gameMode, string name, string sceneSaveName)
			{																																	$"Saving scene {name}:{sceneSaveName}".logDbg();
				string text = JsonConvert.SerializeObject(info);
				SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, $"{sceneSaveName}-{savename}", text);
			}


			[HarmonyPrefix, HarmonyPatch(typeof(ContainerManager), "Deserialize", typeof(string), typeof(Il2CppSystem.Collections.Generic.List<GearItem>))]
			static void ContainerManager_Deserialize_Prefix()
			{																																	$"ContainerManager.Deserialize".logDbg();
				info.newContainers.ForEach(info => createContainerInternal(info.prefabName, info.guid));
			}
#if DEBUG
			// just to check it's not used
			[HarmonyPrefix, HarmonyPatch(typeof(ContainerManager), "Deserialize", typeof(string))]
			static void ContainerManager_Deserialize_Prefix_Debug() => Debug.assert(false, "ContainerManager.Deserialize");
#endif
			[HarmonyPostfix, HarmonyPatch(typeof(Container), "Deserialize")]
			static void Container_Deserialize_Postfix(Container __instance, string text)
			{
				string guid = __instance.GetComponent<ObjectGuid>()?.m_Guid;

				if (info.hiddenContainers.Contains(guid))
				{																																$"Container.Deserialize: hiding container {guid}".logDbg();
					hideContainer(__instance.gameObject);
				}
				else if (info.movedContainers.Contains(guid))
				{																																$"Container.Deserialize: restoring transform for container {__instance.gameObject.baseName()} ({guid})".logDbg();
					using var _ = Debug.profiler("restoring transform");

					var proxy = Utils.DeserializeObject<ContainerSaveDataProxy>(text);
					__instance.gameObject.transform.SetPositionAndRotation(proxy.m_Position, proxy.m_Rotation);
				}
			}


			[HarmonyPostfix, HarmonyPatch(typeof(Container), "OnEnable")]
			static void Container_OnEnable_Postfix(Container __instance)
			{
				var go = __instance.gameObject;

				if (info?.hiddenGameObjects.Any(hiddenGO => hiddenGO == go) == true) // can't use Contains here for some reason
				{																																$"Container.OnEnable: hiding {go.GetComponent<ObjectGuid>()?.m_Guid} again".logDbg();
					go.SetActive(false); // hey you, stay hidden!
				}
			}
		}
	}
}