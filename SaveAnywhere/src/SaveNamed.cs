using System;

namespace SaveAnywhere
{
	static class SaveNamed
	{
		static bool active
		{
			get => _active;
			set => _active = GameManager.m_IsPaused = value;
		}
		static bool _active;

		public static void save()
		{
			if (active || !(active = true))
				return;

			Action okAction = new (() =>
			{
				active = false;
				SaveLoad.save(InterfaceManager.m_Panel_Confirmation.GetInputFieldText());
			});

			Action<bool> cancelAction = new (canceled =>
			{
				if (canceled)
					active = false;
			});

			string savename = SaveLoad.getSlotDisplayName(SaveLoad.getCurrentSlot());
			InterfaceManager.m_Panel_Confirmation.ShowRenamePanel("new save", savename, "GAMEPLAY_Accept", "GAMEPLAY_Cancel", okAction, cancelAction);
		}
	}
}