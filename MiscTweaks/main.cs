using UnityEngine;

using Common;
using Common.Configuration;

namespace MiscTweaks
{
	public class Main: Mod
	{
		internal const string version = "1.0.0";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		static bool moveWindow = false;

		public override void OnUpdate()
		{
			if (moveWindow && !(moveWindow = false)) // next frame after switching resolution
				Utils.setWindowPos(10, 10);

			if (InputHelper.isKeyDown(config.hotkeySwitchResolution))
			{
				if (Screen.fullScreen && (moveWindow = true))
					Screen.SetResolution(1280, 720, false);
				else
					Screen.SetResolution(2560, 1440, true);
			}

			if (InputHelper.isKeyDown(config.hotkeyCheatUnlockSafe))
			{
				if (GameUtils.PlayerManager.m_InteractiveObjectUnderCrosshair?.GetComponentInChildren<SafeCracking>() is SafeCracking safeCracking)
					safeCracking.m_Cracked = true;
			}
		}
	}
}