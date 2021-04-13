using System.Collections.Generic;
using UnityEngine;
using Common.Configuration;

namespace DayNightSpeed
{
	class ModConfig: Config
	{
		public readonly Color labelColor = Color.red;
		public readonly Vector2 labelPos = new Vector2(-620f, 330f);

		public readonly Dictionary<KeyCode, float> hotkeys = new Dictionary<KeyCode, float>()
		{
			{ KeyCode.Insert, 1f }, // default
			{ KeyCode.Delete, 12f }, // realtime
			{ KeyCode.End, 0.01f } // fast
		};
	}
}