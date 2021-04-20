#if DEBUG
using System;
using Harmony;
using Common;

namespace MovableContainers
{
	static partial class MovableContainerManager
	{
		[HarmonyPatch(typeof(BootUpdate), "Start")]
		static class ConsoleCommands
		{
			static int dbgContainerTypeIndex = 0;
			static string dbgContainerType => allowedTypes[dbgContainerTypeIndex].type;

			static void Postfix()
			{
				uConsole.RegisterCommand("mc_info", new Action(() =>
				{
					"Movable containers info:".logDbg();

					"Hidden game objects:".logDbg();
					info.hiddenGameObjects.ForEach(go => $"{go.name} - guid: {go.GetComponent<ObjectGuid>()?.m_Guid}, visible: {go.activeSelf}".logDbg());
				}));

				uConsole.RegisterCommand("mc_setpickedtype", new Action(() =>
				{
					dbgContainerTypeIndex = (dbgContainerTypeIndex + 1) % allowedTypes.Count;
					$"dbg pick type is {dbgContainerType}".onScreen(true);
				}));

				uConsole.RegisterCommand("mc_pick", new Action(() =>
				{
					pickUpContainer(dbgContainerType);
				}));
			}
		}
	}
}
#endif // DEBUG