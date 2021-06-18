using System.Linq;

using HarmonyLib;
using UnityEngine;

using Common;

namespace MiscTweaks
{
	using SceneManager = UnityEngine.SceneManagement.SceneManager;

	static class LocationCleaner
	{
		static readonly string[] scenesToClean =
		{
			"GreyMothersHouseA",
			"ConvenienceStoreA",
			"CampOffice"
		};

		static readonly string[] objectsToClean =
		{
			"OBJ_Cup",
			"OBJ_DishBowl",
			"OBJ_DishPlate",
			"OBJ_WoodSpatula",
			"OBJ_PaperDeco",
			"OBJ_PaperDebris",
			"OBJ_Pitcher",
			"OBJ_PaintCan",
			"OBJ_ClipBoard",
			"OBJ_MetalFileCabinetDrawer"
		};

		static readonly string[] decalsToClean =
		{
			"FX_DebrisMail",
			"FX_DebriPaper",
			"FX_DebrisPaper",
		};

		static bool shouldRemove(GameObject go)
		{
			return objectsToClean.Any(go.name.StartsWith) ||
				   (go.GetComponent<qd_Decal>()?.texture.name is string decalName && decalsToClean.Any(decalName.StartsWith));
		}

		static void cleanUp(GameObject go)
		{
			if (!go)
				return;

			using (Common.Debug.profiler($"LocationCleaner: cleaning up object {go.name}"))
				_cleanUp(go);

			static void _cleanUp(GameObject go)
			{
				for (int i = 0; i < go.transform.GetChildCount(); i++)
				{
					var child = go.transform.GetChild(i).gameObject;

					if (shouldRemove(child))
						child.SetActive(false);
					else
						_cleanUp(child);
				}
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), "LoadSceneData")]
		static class SaveGameSystem_LoadSceneData_Patch
		{
			static bool Prepare() => Main.config.cleanSomeLocations;

			static void Postfix(string name, string sceneSaveName)
			{																																	$"Loading scene {name}:{sceneSaveName}".logDbg();
				if (!scenesToClean.Any(sceneSaveName.StartsWith))
					return;

				foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					//go.dump();
					cleanUp(go.getChild("Art"));
				}
			}
		}
	}
}