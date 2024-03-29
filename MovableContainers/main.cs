﻿using Common;
using Common.Configuration;

#if DEBUG
using UnityEngine;
#endif

namespace MovableContainers
{
	public class Main: Mod
	{
		internal const string version = "1.2.1";

		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

#if DEBUG
		public override void OnUpdate()
		{
			if (InputHelper.isKeyDown(KeyCode.F1))
				uConsole.RunCommand("mc_setpickedtype");

			if (InputHelper.isKeyDown(KeyCode.F2))
				uConsole.RunCommand("mc_pick");

			if (InputHelper.isKeyDown(KeyCode.F3))
				MovableContainerManager.dropContainer();
		}
#endif
	}
}