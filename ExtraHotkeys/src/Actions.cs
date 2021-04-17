using System;
using System.Collections.Generic;

using Common;

namespace ExtraHotkeys
{
	static class Actions
	{
		public class InventoryAction
		{
			public readonly string id;
			public readonly string name;
			public readonly Action action;

			public InventoryAction(string id, string name, Action action)
			{
				this.id = id;
				this.name = name;
				this.action = action;
			}
		};

		static Inventory inv => GameUtils.Inventory;
		static void use(GearItem item)
		{
			if (GameUtils.PlayerManager?.UseInventoryItem(item, -1f) != true)
				GameAudioManager.PlayGUIError();
		}

		public static readonly List<InventoryAction> actions = new List<InventoryAction>()
		{
			// weapons
			new InventoryAction("Revolver",	"Equip Revolver",			() => use(inv?.GetBestRevolver())),
			new InventoryAction("FlareGun",	"Equip Distress Pistol",	() => use(inv?.GetBestFlareGun())),
			new InventoryAction("Rifle",	"Equip Hunting Rifle",		() => use(inv?.GetBestRifle())),
			new InventoryAction("Bow",		"Equip Survival Bow",		() => use(inv?.GetBestBow())),
			new InventoryAction("Stone",	"Equip Stone",				() => use(inv?.GetBestStone())),

			// light sources
			new InventoryAction("Flare",	"Equip Flare",				() => use(inv?.GetBestFlare(FlareType.Red))),
			new InventoryAction("FlareM",	"Equip Marine Flare",		() => use(inv?.GetBestFlare(FlareType.Blue))),
			new InventoryAction("Lantern",	"Equip Storm Lantern",		() => use(inv?.GetBestLamp())),
			new InventoryAction("Flash",	"Equip Flashlight",			() => use(inv?.GetBestFlashlight())),
			new InventoryAction("Torch",	"Equip Torch",				() => use(inv?.GetBestTorch())),

			// misc tools
			new InventoryAction("Snare",	"Use Snare",				() => use(inv?.GetBestGearItemWithName("GEAR_Snare"))),
			new InventoryAction("Bedroll",	"Use Bedroll",				() => use(inv?.GetBestBed())),
			new InventoryAction("Pot",		"Use Cooking Pot/Can",		() => use(inv?.GetBestGearItemWithName("GEAR_CookingPot") ?? inv?.GetBestGearItemWithName("GEAR_RecycledCan"))),

			// extra actions
			new InventoryAction("Stim",		"Equip Emergency Stim",		() => use(inv?.GetNonRuinedItem("GEAR_EmergencyStim"))),
			new InventoryAction("SprayCan", "Equip Spray Paint",		() => use(inv?.GetNonRuinedItem("GEAR_SprayPaintCan")))
		};
	}
}