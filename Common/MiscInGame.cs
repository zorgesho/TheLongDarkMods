using UnityEngine;

namespace Common
{
	static partial class StringExtensions
	{
		public static string onScreen(this string str) { HUDMessage.AddMessage(str, false); return str; }
	}

	static class InputHelper
	{
		public static bool isKeyDown(KeyCode key) => key != KeyCode.None && InputManager.GetKeyDown(InputManager.m_CurrentContext, key);
	}

	static class GameUtils
	{
		public static bool isMainMenu() => GameManager.m_ActiveScene == "MainMenu";
	}
}