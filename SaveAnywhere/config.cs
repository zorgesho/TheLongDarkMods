using UnityEngine;
using Common.Configuration;

namespace SaveAnywhere
{
	class ModConfig: Config
	{
		public readonly bool disablePermadeath = true;
		public readonly bool removeCustomSavesWithOriginal = true;

		public readonly int maxSaveSlots = 1000;
		public readonly KeyCode customSaveHotkey = KeyCode.F7;
	}
}