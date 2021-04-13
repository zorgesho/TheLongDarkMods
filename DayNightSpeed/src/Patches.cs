using Harmony;
using UnityEngine;

using Common;

namespace DayNightSpeed
{
	[HarmonyPatch(typeof(ExperienceModeManager), "GetTimeOfDayScale")]
	static class TimeScalePatch
	{
		public static float timescale
		{
			get => _timescale;

			set
			{
				_timescale = value;
				label.text = _timescale == 1f? "": "timescale: " + (_timescale == 12f? "realtime": $"{_timescale}");
			}
		}
		static float _timescale = 1.0f;

		static UILabel label
		{
			get
			{
				if (!_label)
				{
					var labelPrefab = InterfaceManager.m_Panel_HUD.m_Label_Message.gameObject;

					var labelGO = Object.Instantiate(labelPrefab, labelPrefab.transform.parent);
					labelGO.destroyComponent<UIAnchor>();

					_label = labelGO.GetComponent<UILabel>();
					_label.pivot = UIWidget.Pivot.Left;
					_label.color = Main.config.labelColor;
					_label.transform.localPosition = Main.config.labelPos;

					labelGO.SetActive(true);
				}

				return _label;
			}
		}
		static UILabel _label;

		static void Postfix(ref float __result) => __result = _timescale;
	}
}