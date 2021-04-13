using UnityEngine;
using Common.Configuration;

namespace MiscTweaks
{
	class ModConfig: Config
	{
		public readonly KeyCode hotkeyConsole = KeyCode.BackQuote;
		public readonly KeyCode hotkeySwitchResolution = KeyCode.F12;
		public readonly KeyCode hotkeyCheatUnlockSafe = KeyCode.F11;

		public readonly bool stackMatches = false;
		public readonly bool disableVoiceOver = true;
		public readonly bool cleanSomeLocations = true; // list of locations is in the LocationCleaner.cs
		public readonly bool allowUnlimitedSleep = true;
	}
}