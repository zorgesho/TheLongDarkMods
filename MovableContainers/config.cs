using Common.Configuration;

namespace MovableContainers
{
	class ModConfig: Config
	{
		public readonly bool hoverInfo = true;
		public readonly bool hoverInfoUseColor = true;
		public readonly int hoverInfoMaxItems = 6;
	}
}