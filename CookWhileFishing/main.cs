using Common;

#if DEBUG
using UnityEngine;
#endif

namespace CookWhileFishing
{
	public class Main: Mod
	{
		internal const string version = "1.0.0";

#if DEBUG
		public override void OnUpdate()
		{
			if (InputHelper.isKeyDown(KeyCode.G))
			{
				if (GameUtils.PlayerManager.m_InteractiveObjectUnderCrosshair?.GetComponentInChildren<IceFishingHole>() is IceFishingHole iceFishingHole)
				{
					var pm = GameUtils.PlayerManager.gameObject;

					if (pm.GetComponent<AutoCook>())
						pm.destroyComponent<AutoCook>(false);
					else
						pm.AddComponent<AutoCook>().init(iceFishingHole);
				}
			}
		}
#endif // DEBUG
	}
}