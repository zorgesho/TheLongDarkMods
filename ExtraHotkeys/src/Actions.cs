using System;
using System.Collections.Generic;

using Common;

namespace ExtraHotkeys
{
	static class Actions
	{
		public record InventoryAction(string id, string name, Action action);

		static Inventory inv => GameUtils.Inventory;
		static void use(GearItem item)
		{
			if (GameUtils.PlayerManager?.UseInventoryItem(item, -1f) != true)
				GameAudioManager.PlayGUIError();
		}

		public static readonly List<InventoryAction> actions = new()
		{
			// weapons
			new ("Revolver",	"Equip Revolver",			() => use(inv?.GetBestRevolver())),
			new ("FlareGun",	"Equip Distress Pistol",	() => use(inv?.GetBestFlareGun())),
			new ("Rifle",		"Equip Hunting Rifle",		() => use(inv?.GetBestRifle())),
			new ("Bow",			"Equip Survival Bow",		() => use(inv?.GetBestBow())),
			new ("Stone",		"Equip Stone",				() => use(inv?.GetBestStone())),

			// light sources
			new ("Flare",		"Equip Flare",				() => use(inv?.GetBestFlare(FlareType.Red))),
			new ("FlareM",		"Equip Marine Flare",		() => use(inv?.GetBestFlare(FlareType.Blue))),
			new ("Lantern",		"Equip Storm Lantern",		() => use(inv?.GetBestLamp())),
			new ("Flash",		"Equip Flashlight",			() => use(inv?.GetBestFlashlight())),
			new ("Torch",		"Equip Torch",				() => use(inv?.GetBestTorch())),

			// misc tools
			new ("Snare",		"Use Snare",				() => use(inv?.GetBestGearItemWithName("GEAR_Snare"))),
			new ("Bedroll",		"Use Bedroll",				() => use(inv?.GetBestBed())),
			new ("Pot",			"Use Cooking Pot/Can",		() => use(inv?.GetBestGearItemWithName("GEAR_CookingPot") ?? inv?.GetBestGearItemWithName("GEAR_RecycledCan"))),

			// extra actions
			new ("Stim",		"Equip Emergency Stim",		() => use(inv?.GetNonRuinedItem("GEAR_EmergencyStim"))),
			new ("SprayCan",	"Equip Spray Paint",		() => use(inv?.GetNonRuinedItem("GEAR_SprayPaintCan")))
		};
	}
}