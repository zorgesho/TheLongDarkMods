using Common.Configuration;

namespace PlaceFromInventory
{
	class ModConfig: Config
	{
		public readonly bool showItemsQuantity = true;
		public readonly bool allowToPlaceItemsTooClose = true;
	}
}