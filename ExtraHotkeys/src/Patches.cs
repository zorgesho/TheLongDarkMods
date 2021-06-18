using Il2CppSystem.Collections;

using HarmonyLib;
using Rewired;
using UnityEngine;

using Common;

namespace ExtraHotkeys
{
	[PatchClass]
	static class BindPatches
	{
		static KeyBind bindingNow;

		static void startBinding(KeyBind bind)
		{
			bind.setSelected(true);
			bind.clearValueLabel();

			InputManager.PushContext(bind);
			bindingNow = bind;
		}

		static void stopBinding()
		{
			if (!bindingNow)
				return;

			bindingNow.setSelected(false);

			InputManager.PopContext(bindingNow);
			bindingNow = null;
		}

		static KeyCode getPressedKey()
		{
			var keyboard = ReInput.controllers.Keyboard;

			if (!keyboard.GetAnyButtonDown())
				return KeyCode.None;

			var keys = keyboard.PollForAllKeys().GetEnumerator().TryCast<IEnumerator>();
			keys.MoveNext();

			return keys.Current.TryCast<ControllerPollingInfo>().keyboardKey;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_OptionsMenu), "OnKeyRebindButtonPress")]
		static bool PanelOptionsMenu_OnKeyRebindButtonPress_Prefix(GameObject buttonPressed)
		{
			if (bindingNow)
				return false;

			if (buttonPressed.GetComponent<KeyBind>() is not KeyBind keyBind)
				return true;

			GameAudioManager.PlayGUIButtonClick();

			startBinding(keyBind);
			return false;
		}

		[HarmonyPrefix, HarmonyPatch(typeof(Panel_OptionsMenu), "OnGUI")]
		static bool PanelOptionsMenu_OnGUI_Prefix(Panel_OptionsMenu __instance)
		{
			if (!bindingNow)
				return true;

			var key = getPressedKey();

			if (key == KeyCode.None)
				return false;

			bindingNow.key = key == KeyCode.Escape? KeyCode.None: key;
			__instance.SettingsNeedConfirmation();

			stopBinding();
			return false;
		}

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_OptionsMenu), "OnRebindingTab")]
		static void PanelOptionsMenu_OnRebindingTab_Postfix() => HotkeyManager.updateUI();

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_OptionsMenu), "OnConfirmSettings")]
		static void PanelOptionsMenu_OnConfirmSettings_Postfix() => HotkeyManager.updateBinds();

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_OptionsMenu), "OnResetKeyBindings")]
		static void PanelOptionsMenu_OnResetKeyBindings_Postfix() => HotkeyManager.resetBinds();

		[HarmonyPostfix, HarmonyPatch(typeof(Panel_OptionsMenu), "MainMenuTabOnEnable")]
		static void PanelOptionsMenu_MainMenuTabOnEnable_Postfix() => stopBinding();
	}
}