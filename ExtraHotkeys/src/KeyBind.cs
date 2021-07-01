using System;

using UnityEngine;
using MelonLoader;
using UnhollowerBaseLib.Attributes;

using Common;

namespace ExtraHotkeys
{
	[RegisterTypeInIl2Cpp]
	class KeyBind: MonoBehaviour
	{
		public KeyBind(IntPtr ptr): base(ptr) {}

		UILabel nameLabel;
		UILabel valueLabel;
		UISprite selectedSprite;

		public KeyCode key
		{
			[HideFromIl2Cpp] get => _key;
			[HideFromIl2Cpp] set => setValueLabel(InputHelper.getLabelForKey(_key = value));
		}
		KeyCode _key;

		public string actionID { [HideFromIl2Cpp] get; [HideFromIl2Cpp] private set; }

		[HideFromIl2Cpp]
		public void init(string actionID, string actionName)
		{
			this.actionID = actionID;
			nameLabel.text = actionName;

			clearValueLabel();
		}

		void Awake()
		{
			try
			{
				var labelNameGO = gameObject.getChild("Label_Name");
				nameLabel = labelNameGO.GetComponent<UILabel>();
				labelNameGO.destroyComponent<UILocalize>();

				valueLabel = gameObject.getChild("Label_Value").GetComponent<UILabel>();

				selectedSprite = gameObject.getChild("SelectedSprite").GetComponent<UISprite>();
				selectedSprite.color = Color.gray;
				setSelected(false);
			}
			catch (Exception e) { Common.Log.msg(e); }
		}

		[HideFromIl2Cpp]
		public void clearValueLabel() => setValueLabel("");

		[HideFromIl2Cpp]
		void setValueLabel(string val) => valueLabel.text = val;

		[HideFromIl2Cpp]
		public void setSelected(bool val) => selectedSprite.enabled = val;
	}
}