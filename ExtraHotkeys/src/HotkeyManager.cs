using System;
using System.Collections.Generic;

using UnityEngine;

using Common;

namespace ExtraHotkeys
{
	static class HotkeyManager
	{
		class Bind
		{
			public KeyCode key;

			public readonly string id;
			public readonly Action action;

			public Bind(string id, KeyCode key, Action action)
			{
				this.id = id;
				this.key = key;
				this.action = action;
			}
		}

		static GameObject bindsParent;

		static void forEachKeyBind(Action<KeyBind> action) => bindsParent?.forEachComponentInChildren(action);

		static readonly float[] columnPos = { -1.49f, -1.54f, 0.83f }; // horizontal positions for hotkeys columns
		static readonly List<float> bindPositions = new();

		static readonly List<Bind> binds = new();

		public static void init()
		{
			Actions.actions.ForEach(action => binds.Add(new Bind(action.id, KeyCode.None, action.action)));

			Main.config.binds.forEach(key => bindKey(key.Key, key.Value));
		}

		static void bindKey(string id, KeyCode key, bool updateConfig = false)
		{																								$"HotkeyManager.bindKey {id} {key}".logDbg();
			binds.Find(b => b.id == id).key = key;

			if (updateConfig)
				Main.config.binds[id] = key;
		}

		public static void updateBinds()
		{
			forEachKeyBind(kb => bindKey(kb.actionID, kb.key, true));

			Main.config.save();
		}

		public static void resetBinds()
		{
			forEachKeyBind(kb => kb.key = KeyCode.None);
		}

		public static void updateUI()
		{
			createUI();
			forEachKeyBind(kb => kb.key = Main.config.binds[kb.actionID]);
		}

		static void createUI()
		{
			static void _setX(GameObject go, float xPos) => go.transform.position = new Vector3(xPos, go.transform.position.y, 0f);

			if (bindsParent)
				return;

			var panel = InterfaceManager.m_Panel_OptionsMenu.m_RebindingTab.getChild("GameObject");

			var leftColumn = panel.getChild("LeftSide");
			var rightColumn = panel.getChild("RightSide");

			_setX(leftColumn, columnPos[0]);
			_setX(rightColumn, columnPos[1]);

			var bindPrefab = leftColumn.getChild("Button_Rebinding");

			// add new column to the key binding panel
			bindsParent = panel.createChild("ExtraKeys", pos: new Vector3(columnPos[2], leftColumn.transform.position.y, 0f));

			// copying keys positions (for some reason they have different spacing)
			var keys = leftColumn.transform;
			int keyCount = keys.GetChildCount();

			for (int i = 0; i < keyCount; i++)
				bindPositions.Add(keys.GetChild(i).localPosition.y);

			bindPositions.Sort((a, b) => Math.Sign(b - a)); // also they are not in order

			// add new keys to the key binding panel
			for (int i = 0; i < Actions.actions.Count; i++)
			{
				var action = Actions.actions[i];

				var newkey = bindsParent.createChild(bindPrefab, localPos: new Vector3(0f, bindPositions[i], 0f));
				newkey.destroyComponent<KeyRebindingButton>();
				newkey.AddComponent<KeyBind>().init(action.id, action.name);
			}
		}

		static bool shouldDisableHotkeys()
		{
			return GameUtils.isMainMenu() || GameManager.m_IsPaused || uConsole.IsOn() ||
				   InterfaceManager.m_Panel_Confirmation?.IsEnabled() == true ||
				   InterfaceManager.m_Panel_Log?.m_NotesTextField.isActiveAndEnabled == true;
		}

		public static void update()
		{
			if (shouldDisableHotkeys())
				return;

			foreach (var bind in binds)
			{
				if (InputHelper.isKeyDown(bind.key))
					bind.action();
			}
		}
	}
}