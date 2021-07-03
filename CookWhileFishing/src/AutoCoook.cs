using System;

using MelonLoader;
using UnityEngine;
using UnhollowerBaseLib.Attributes;

using Common;

namespace CookWhileFishing
{
	[RegisterTypeInIl2Cpp]
	class AutoCook: MonoBehaviour
	{
		public AutoCook(IntPtr ptr): base(ptr) {}

		Fire fire;
		IceFishingHole iceFishingHole;
		GearPlacePoint gearPlacePoint;

		bool cookMeatToo = false;

		[HideFromIl2Cpp]
		public void init(IceFishingHole iceFishingHole)
		{																											"AutoCook.init".logDbg();
			if (iceFishingHole)
			{
				this.iceFishingHole = iceFishingHole;
				fire = iceFishingHole.transform.parent.GetComponentInChildren<Fire>();
				gearPlacePoint = iceFishingHole.transform.parent.GetComponentInChildren<GearPlacePoint>();
			}

			if (!iceFishingHole || !fire || !gearPlacePoint)
			{
				$"AutoCook.init: something went wrong (iceFishingHole: '{iceFishingHole}, fire: '{fire}', gearPlacePoint: '{gearPlacePoint}'')".logError();
				Destroy(this);
			}
		}

		[HideFromIl2Cpp]
		GearItem getRawFood(bool fishOnly)
		{
			if (!GameUtils.Inventory.HasRawMeat())
				return null;

			// this loop is quite slow, so we doing it only when we sure that there is some raw food
			foreach (var item in GameUtils.Inventory.m_Items)
				if (item.m_GearItem.m_FoodItem is FoodItem foodItem && foodItem.m_IsRawMeat && (!fishOnly || foodItem.m_IsFish))
					return item;

			return null;
		}

		void Start()
		{																											"AutoCook.Start".logDbg();
			if (!iceFishingHole)
#if DEBUG
				init(FindObjectOfType<IceFishingHole>()); // for testing
#else
				Destroy(this);
#endif
			// if we already cooking meat when we start fishing, then we'll also cook meat while fishing
			var gear = gearPlacePoint?.m_PlacedGear; // can't use null-conditional operator with 'm_PlacedGear', it can be in destroyed state
			cookMeatToo = gear && gear.GetComponent<CookingPotItem>()?.m_GearItemBeingCooked?.GetComponent<FoodItem>()?.m_IsFish == false;
		}

		void Update()
		{
			if (fire.GetFireState() == FireState.Off)
			{																										"AutoCook.Update: fire isn't lit, aborting".logDbg();
				Destroy(this);
				return;
			}

			CookingPotItem pot = null;

			if (gearPlacePoint.m_PlacedGear)
			{
				if (!gearPlacePoint.m_PlacedGear.name.Contains("CookingPot")) // GEAR_CookingPot or GEAR_CookingPotDummy
				{																									"AutoCook.Update: gear in place point isn't a cooking pot, aborting".logDbg();
					Destroy(this);
					return;
				}

				pot = gearPlacePoint.m_PlacedGear.GetComponent<CookingPotItem>();

				if (pot.m_LitersSnowBeingMelted > 0f || pot.m_LitersWaterBeingBoiled > 0f) // if we started with water in pot
				{
					if (pot.m_CookingState == CookingPotItem.CookingState.Cooking || pot.m_LitersWaterBeingBoiled == 0f)
						return;
																													"AutoCook.Update: picking up boiled water".logDbg();
					pot.PickUpCookedItem();
				}

				if (pot.m_GearItemBeingCooked)
				{
					if (pot.GetCookingState() == CookingPotItem.CookingState.Cooking)
						return;
																													$"AutoCook.Update: picking up cooked item {pot.m_GearItemBeingCooked.name}".logDbg();
					pot.PickUpCookedItem();
				}
			}

			var item = getRawFood(!cookMeatToo);

			if (!item)
				return;
																													$"AutoCook.Update: start cooking {item.name}".logDbg();
			item.m_Cookable.m_DoNotCookWhenDropped = true;

			if (pot)
				pot.StartCooking(item.GetComponent<GearItem>());
			else
				gearPlacePoint.DropAndPlaceItem(item);

			item.m_Cookable.m_DoNotCookWhenDropped = false;
		}
#if DEBUG
		void OnDestroy() => "AutoCook.OnDestroy".logDbg();
#endif
	}
}