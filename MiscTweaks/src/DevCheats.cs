using UnityEngine;
using Common;

namespace MiscTweaks
{
	static class DevCheats
	{
		public static void processObject(GameObject go)
		{
			if (!go)
				return;

			if (go.GetComponentInChildren<SafeCracking>() is SafeCracking safeCracking)
				crackSafe(safeCracking);
			else if (go.GetComponentInChildren<Fire>() is Fire fire)
				litFire(fire);
			else if (go.GetComponentInChildren<IceFishingHole>() is IceFishingHole iceFishingHole)
				clearFishingHole(iceFishingHole);
			else if (go.GetComponentInChildren<Keypad>() is Keypad keypad)
				addKeypadCode(keypad);
		}

		static void crackSafe(SafeCracking safeCracking)
		{
			safeCracking.m_Cracked = true;
		}

		static void litFire(Fire fire)
		{
			var fuel = Resources.Load<GameObject>("gear_coal");

			if (fire.GetFireState() == FireState.Off)
			{
				fire.m_FuelPrefabIfLit = fuel.GetComponent<FuelSourceItem>();
				fire.StartFireLit();
			}

			fire.AddFuel(fuel.GetComponent<GearItem>(), false);
		}

		static void clearFishingHole(IceFishingHole iceFishingHole)
		{
			iceFishingHole.ClearHole();

			if (!GameUtils.Inventory.GetBestFishingTackle())
				GameUtils.addItem("gear_hookandline");
		}

		static void addKeypadCode(Keypad keypad)
		{
			GameUtils.PlayerManager.m_KnownCodes.Add(keypad.m_Code);
		}
	}
}