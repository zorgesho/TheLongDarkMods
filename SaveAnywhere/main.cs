using UnityEngine;

using Common;
using Common.Configuration;

namespace SaveAnywhere
{
	public class Main: Mod
	{
		internal const string version = "1.2.1";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		internal static SaveSlotType gameType
		{
			get
			{																					$"Main.gameType: {ExperienceModeManager.s_CurrentModeType}".logDbg();
				if (GameManager.IsStoryMode())
					return SaveSlotType.STORY;

				return GameManager.GetExperienceModeManagerComponent().IsChallengeActive()? SaveSlotType.CHALLENGE: SaveSlotType.SANDBOX;
			}
		}

		protected override void init() => SaveGameSlots.MAX_SAVESLOTS = config.maxSaveSlots;

		public override void OnUpdate()
		{
			if (GameUtils.isMainMenu() || GameManager.IsStoryMode())
				return;

			if (InputHelper.isKeyDown(KeyCode.F5))
				SaveLoad.save();

			if (InputHelper.isKeyDown(KeyCode.F6))
				SaveLoad.load();

			if (InputHelper.isKeyDown(config.customSaveHotkey))
				SaveNamed.save();
		}
	}
}