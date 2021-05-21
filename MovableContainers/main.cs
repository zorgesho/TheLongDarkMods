using Common;
using Common.Configuration;

#if DEBUG
using UnityEngine;
#endif

namespace MovableContainers
{
	public class Main: Mod
	{
		internal const string version = "1.1.0";
		
		// large locations which has multiple parts
		// for those locations we'll always create new containers instead of reusing hidden ones
		public static readonly string[] largeLocations =
		{
			"Dam",
			"DamTransitionZone",
			"FarmHouseA",
		};

		protected override void init() => HarmonyHelper.patchAll();

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