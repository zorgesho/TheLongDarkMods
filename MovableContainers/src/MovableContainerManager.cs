using System;
using System.Linq;
using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;

namespace MovableContainers
{
	using Object = UnityEngine.Object;

	static partial class MovableContainerManager
	{
		// serializable info about changed containers in the current scene
		class MovedContainersInfo
		{
			public record ContainerInfo(string guid, string prefabName);

			public readonly HashSet<string> movedContainers = new();
			public readonly HashSet<string> hiddenContainers = new();
			public readonly List<ContainerInfo> newContainers = new();

			[NonSerialized]
			public readonly List<GameObject> hiddenGameObjects = new();
		}
		static MovedContainersInfo info = null;

		public record ContainerProps(string type, string displayName, float weight);

		static readonly List<ContainerProps> allowedTypes = new()
		{
			new ("CONTAINER_BackPack", "GAMEPLAY_BackPack", 1f),
			new ("CONTAINER_PlasticBox", "GAMEPLAY_PlasticContainer", 1f),
			new ("CONTAINER_BriefcaseA", "GAMEPLAY_Briefcase", 1.5f),
			new ("CONTAINER_LockBoxB", "GAMEPLAY_LockBox", 1.5f),
			new ("CONTAINER_MetalBox", "GAMEPLAY_MetalContainer", 2f),
			new ("CONTAINER_TrashCanister", "GAMEPLAY_TrashCan", 2.5f),
			new ("CONTAINER_Cooler", "GAMEPLAY_Cooler", 5f),
			new ("CONTAINER_SteamerTrunk", "GAMEPLAY_SteamerTrunk", 15f),
			new ("CONTAINER_FirewoodBin", "GAMEPLAY_FireWoodBin", 30f),
			new ("CONTAINER_ForestryCrate", "GAMEPLAY_SupplyBin", 30f),

			// doesn't work properly:
			//new ("CONTAINER_FirstAidKitB", "GAMEPLAY_FirstAidKit", 0.5f),
			//new ("CONTAINER_CacheStoreCommon", "STORY_HiddenCacheContainer", 0.5f),
		};

		public static string pickedContainerType { get; private set; } = null;
		public static string pickedContainerName { get; private set; } = null;
		public static float pickedContainerWeight { get; private set; } = 0f;

		static GameObject containersRoot;
		static GameObject lastCreatedContainer;

		public static ContainerProps getContainerProps(GameObject go) => allowedTypes.FirstOrDefault(type => go.name.StartsWith(type.type));
		static ContainerProps getContainerProps(string type) => allowedTypes.FirstOrDefault(t => t.type == type);

		public static bool isAllowedType(GameObject go)
		{
			return
#if DEBUG
			Main.config.dbgAllowToMoveAnyContainer ||
#endif
			getContainerProps(go) != null;
		}

		public static void addMoved(string guid) => info.movedContainers.Add(guid);

		static bool isLargeLocation() => Main.config.largeLocations.Any(name => GameManager.m_ActiveScene == name);

		static void initContainersInfo(MovedContainersInfo info)
		{																									$"MovableContainerManager.initContainersInfo".logDbg();
			Common.Debug.assert(containersRoot == null);
			containersRoot = new GameObject("MovableContainersRoot");

			MovableContainerManager.info = info ?? new MovedContainersInfo();								$"Containers info - moved: {info.movedContainers.Count}, hidden: {info.hiddenContainers.Count}, new: {info.newContainers.Count}".logDbg();
		}

		static void initContainer(Container container)
		{
			container.m_Inspected = true;
			container.m_NotPopulated = false;
			container.m_GearToInstantiate.Clear();
		}

		public static GameObject createContainer(string prefabName)
		{																									$"Creating container {prefabName}".logDbg();
			// check hidden containers first
			var obj = info.hiddenGameObjects.FirstOrDefault(go => getContainerProps(go).type == prefabName);

			// for large indoor locations we always create new containers, because otherwise they can become invisible
			if (obj && !isLargeLocation())
			{
				showContainer(obj);																			$"Using hidden container instead {obj.baseName()}".logDbg();
			}
			else
			{
				obj = createContainerInternal(prefabName);
				info.newContainers.Add(new (prefabName, obj.GetComponent<ObjectGuid>().m_Guid));
			}

			lastCreatedContainer = obj;
			return obj;
		}

		static GameObject createContainerInternal(string prefabName, string guid = null)
		{																									$"Creating new container {prefabName}".logDbg();
			var obj = containersRoot.createChild(Resources.Load<GameObject>(prefabName));

			Common.Debug.assert(obj?.GetComponent<Container>());
			Common.Debug.assert(!obj?.GetComponent<ObjectGuid>());

			var guidCmp = obj.AddComponent<ObjectGuid>();
			if (guid == null)
				guidCmp.Generate();
			else
				guidCmp.Set(guid);

			obj.GetComponent<Container>().m_Inspected = true;

			if (obj.GetComponent<Lock>() is Lock @lock)
				@lock.m_LockState = LockState.Unlocked;

			return obj;
		}

		public static (bool allowed, string error) isAllowedToPickup(GameObject go)
		{
			if (pickedContainerType != null)
				return (false, "You can't pick up more than one container");

			var container = go?.GetComponent<Container>();

			if (!container)
				return (false, "This is not a container"); // probably shouldn't happen

			if (go?.GetComponent<Lock>()?.m_LockState == LockState.Locked)
				return (false,  "Container is locked");

			if (!container.IsInspected())
				return (false,  "Container is not inspected");

			if (!container.IsEmpty())
				return (false,  "Container is not empty");

			return (true, null);
		}

		public static bool tryPickUpContainer(GameObject go)
		{
			var (allowed, error) = isAllowedToPickup(go);

			if (!allowed)
			{
				error?.onScreen();
				GameAudioManager.PlayGUIError();

				return false;
			}

			pickUpContainer(go);
			return true;
		}

		static void showContainer(GameObject go)
		{
			info.hiddenContainers.Remove(go.GetComponent<ObjectGuid>().m_Guid);
			info.hiddenGameObjects.Remove(go);
			go.SetActive(true);
		}

		static void hideContainer(GameObject go)
		{
			using var _ = Common.Debug.profiler("hideContainer");

			string guid = go.GetComponent<ObjectGuid>().m_Guid;
			int index = info.newContainers.FindIndex(i => i.guid == guid);

			if (index != -1) // if this a new container, we'll just remove it
			{																								$"Removing container {go.baseName()} from scene".logDbg();
				info.newContainers.RemoveAt(index);
				info.movedContainers.Remove(guid);
				Object.Destroy(go);
			}
			else
			{																								$"Hiding container {go.baseName()} from scene".logDbg();
				info.hiddenContainers.Add(guid);
				info.hiddenGameObjects.Add(go);
				go.SetActive(false);
			}
		}

		static void pickUpContainer(GameObject go)
		{																									$"pickUpContainer: {go.baseName()}".logDbg();
			hideContainer(go);
			pickUpContainer(getContainerProps(go).type);
		}

		static void pickUpContainer(string type)
		{																									$"pickUpContainer: '{type ?? "[null]"}'".logDbg();
			pickedContainerType = type;

			var containerProps = getContainerProps(pickedContainerType);
			pickedContainerName = containerProps?.displayName;
			pickedContainerWeight = containerProps?.weight ?? 0f;

			UIManager.setContainerIconVisible(type != null);
		}

		public static void dropContainer()
		{
			if (pickedContainerType == null)
				return;

			var obj = createContainer(pickedContainerType);
			pickedContainerType = null;
			pickedContainerWeight = 0f;
			UIManager.setContainerIconVisible(false);
			PickPlaceHelper.tryEnterPlaceMode(obj);
		}

		[PatchClass]
		static class Patches
		{
			[HarmonyPostfix, HarmonyPatch(typeof(Inventory), "GetExtraWeightKG")]
			static void Inventory_GetExtraWeightKG_Postfix(ref float __result)
			{
				__result += pickedContainerWeight;
			}

			[HarmonyPostfix, HarmonyPatch(typeof(Container), "Start")]
			static void Container_Start_Postfix(Container __instance)
			{
				if (__instance.gameObject == lastCreatedContainer)
					initContainer(__instance);
			}

			[HarmonyPostfix, HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
			static void PlayerManager_ExitMeshPlacement_Postfix(PlayerManager __instance)
			{																								$"PlayerManager.ExitMeshPlacement (cancelled = {!__instance.m_SkipCancel})".logDbg();
				if (lastCreatedContainer && !__instance.m_SkipCancel)
					pickUpContainer(lastCreatedContainer);

				lastCreatedContainer = null; // clear after initial placing
			}
		}
	}
}