using System.Text;
using System.Collections.Generic;

using Harmony;

using Common;

namespace MovableContainers
{
	[HarmonyPatch(typeof(Container), "GetInteractiveDisplayText")]
	static class Container_GetInteractiveDisplayText_Patch
	{
		static bool Prepare() => ModConfig.instance.hoverInfo;

		static readonly StringBuilder sb = new();

		static string _coloredStr(string str, string color) => ModConfig.instance.hoverInfoUseColor? $"[{color}]{str}[-]": str;

		static string getActionStr(Container container)
		{
			string result = container.GetInteractiveActionText();

			if (MovableContainerManager.isAllowedType(container.gameObject))
				result += _coloredStr(" [pickupable]", UIManager.colorText + "99");
#if DEBUG
			result += $" [type: {container.gameObject.baseName()}, {container.m_LocalizedDisplayName.m_LocalizationID}]";
#endif
			return result;
		}

		static void Postfix(Container __instance, ref string __result)
		{
			using var _ = Debug.profiler("Container.GetInteractiveDisplayText");

			if (!__instance.m_Inspected)
				return;

			if (__instance.IsEmpty() && __instance.m_GearToInstantiate.Count == 0)
			{
				__result = $"{getActionStr(__instance)}\n{_coloredStr(Localization.Get("GAMEPLAY_EmptyPostfix"), "999999bb")}";
				return;
			}

			sb.Clear();
			sb.AppendLine(getActionStr(__instance));

			bool allItems = true;
			HashSet<string> items = new();

			foreach (var item in __instance.m_Items)
			{
				var gearItem = item.m_GearItem;
				var itemName = gearItem.m_LocalizedDisplayName.Text();

				if (ModConfig.instance.hoverInfoUseColor && gearItem.GetRoundedCondition() < 50)
					itemName = $"{gearItem.GetColorStringBasedOnCondition()}{itemName}[-]";

				if (items.Count < ModConfig.instance.hoverInfoMaxItems)
				{
					items.Add(itemName);
				}
				else if (!items.Contains(itemName))
				{
					allItems = false;
					break;
				}
			}

			items.forEach(item => sb.Append(item + ", "));
			sb.Remove(sb.Length - 2, 2);

			if (!allItems)
				sb.Append(" and other stuff");

			__result = sb.ToString();
		}
	}
}