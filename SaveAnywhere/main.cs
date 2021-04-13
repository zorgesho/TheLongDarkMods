using UnityEngine;

using Common;
using Common.Configuration;

namespace SaveAnywhere
{
	public class Main: Mod
	{
		internal const string version = "1.0.0";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		internal static SaveSlotType gameType => GameManager.GetExperienceModeManagerComponent().IsChallengeActive()? SaveSlotType.CHALLENGE: SaveSlotType.SANDBOX;

		protected override void init()
		{
			HarmonyHelper.patchAll();
			SaveGameSlots.MAX_SAVESLOTS = config.maxSaveSlots;
		}

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