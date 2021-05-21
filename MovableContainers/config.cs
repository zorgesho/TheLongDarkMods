using ModSettings;

namespace MovableContainers
{
	class ModConfig: JsonModSettings
	{
		public static ModConfig instance = new ModConfig();
		public ModConfig() => this.AddToModSettings("Movable Containers");

		[Name("Show Hover Information")]
		public bool hoverInfo = true;

		[Name("Colorful Hover Information")]
		[Description("The hover information shown may use colors for emphasis.")]
		public bool hoverInfoUseColor = true;

		[Name("Number Of Items Shown")]
		[Description("Max Number Of Items Shown In Hover Information")]
		[Slider(0,10)]
		public int hoverInfoMaxItems = 6;

#if DEBUG
		[Name("Allow To Move Any Container")]
		public bool dbgAllowToMoveAnyContainer = true;
#endif
	}
}