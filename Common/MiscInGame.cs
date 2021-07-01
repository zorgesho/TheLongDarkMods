using UnityEngine;

namespace Common
{
	static partial class StringExtensions
	{
		public static string onScreen(this string str, bool highPriority = false)
		{
			HUDMessage.AddMessage(str, highPriority);
			return str;
		}
	}

	static class InputHelper
	{
		public static bool isKeyDown(KeyCode key) => key != KeyCode.None && InputManager.GetKeyDown(InputManager.m_CurrentContext, key);

		public static string getLabelForKey(KeyCode key) => key == KeyCode.None? "": InputManager.ConvertKeycodeToLabel(key.ToString());
	}

	static class GameUtils
	{
		public static PlayerManager PlayerManager => GameManager.GetPlayerManagerComponent();
		public static Inventory Inventory => GameManager.GetInventoryComponent();

		public static bool isMainMenu() => GameManager.m_ActiveScene == "MainMenu";

		public static void showErrorMessage(string message)
		{
			message?.onScreen();
			GameAudioManager.PlayGUIError();
		}
	}
}