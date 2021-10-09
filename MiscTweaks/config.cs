using UnityEngine;
using Common.Configuration;

namespace MiscTweaks
{
	class ModConfig: Config
	{
		public readonly KeyCode hotkeyConsole = KeyCode.BackQuote;
		public readonly KeyCode hotkeySwitchResolution = KeyCode.F12;
		public readonly KeyCode hotkeyDevCheat = KeyCode.F11;
		public readonly KeyCode hotkeyJump = KeyCode.D;

		public readonly float jumpFatigueDrain = 0.5f;

		public readonly bool stackMatches = false;
		public readonly bool disableVoiceOver = true;
		public readonly bool cleanSomeLocations = true; // list of locations is in the LocationCleaner.cs
		public readonly bool allowUnlimitedSleep = true;

		public readonly bool allowWalkWhileAimingRevolver = true;
		public readonly bool addRevolverCasingsToInventory = true;
		public readonly float revolverFiringRate = 0.25f; // vanilla 0.5f

		public readonly float slowerCoolDown = 0.01f; // 1.0f for default

		public readonly bool dbgShowMissedItemsForEmptyContainers = true;
	}
}