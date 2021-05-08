using Common.Configuration;

namespace MovableContainers
{
	class ModConfig: Config
	{
		public readonly bool hoverInfo = true;
		public readonly bool hoverInfoUseColor = true;
		public readonly int hoverInfoMaxItems = 6;

		// large locations which has multiple parts
		// for those locations we'll always create new containers instead of reusing hidden ones
		public readonly string[] largeLocations =
		{
			"Dam",
			"DamTransitionZone",
			"FarmHouseA",
		};

#if DEBUG
		public readonly bool dbgAllowToMoveAnyContainer = true;
#endif
	}
}