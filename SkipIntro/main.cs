﻿using Common;

namespace SkipIntro
{
	public class Main: Mod
	{
		internal const string version = "1.0.0";
		protected override void init() => HarmonyHelper.patchAll();
	}
}