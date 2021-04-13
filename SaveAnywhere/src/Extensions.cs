using UnityEngine;

namespace SaveAnywhere
{
	static class BasicMenuExtension
	{
		public static void moveItemTo(this BasicMenu menu, int indexTo, int indexFrom = -1)
		{
			if (indexFrom == -1)
				indexFrom = menu.m_ItemModelList.Count - 1;

			var item = menu.m_ItemModelList[indexFrom];
			menu.m_ItemModelList.RemoveAt(indexFrom);
			menu.m_ItemModelList.Insert(indexTo, item);
		}
	}

	// :(
	static class PanelBaseExtensions
	{
		public static void enable(this Panel_Base panel, bool enabled)
		{
			panel.TryCast<Panel_ChooseSandbox>()?.Enable(enabled);
			panel.TryCast<Panel_ChooseChallenge>()?.Enable(enabled);
		}

		public static void onClickBack(this Panel_Base panel)
		{
			panel.TryCast<Panel_ChooseSandbox>()?.OnClickBack();
			panel.TryCast<Panel_ChooseChallenge>()?.OnClickBack();
		}

		public static SaveSlotType getSlotType(this Panel_Base panel) =>
			panel.TryCast<Panel_ChooseSandbox>()? SaveSlotType.SANDBOX: SaveSlotType.CHALLENGE;

		public static SaveSlotInfo getSlotToDelete(this Panel_Base panel) =>
			panel.TryCast<Panel_ChooseSandbox>()?.m_SlotToDelete ?? panel.TryCast<Panel_ChooseChallenge>()?.m_SlotToDelete;

		public static UtilsPanelChoose.DetailsObjets getDetails(this Panel_Base panel) =>
			panel.TryCast<Panel_ChooseSandbox>()?.m_DetailObjects ?? panel.TryCast<Panel_ChooseChallenge>()?.m_DetailObjects;

		public static BasicMenu getBasicMenu(this Panel_Base panel) =>
			panel.TryCast<Panel_ChooseSandbox>()?.m_BasicMenu ?? panel.TryCast<Panel_ChooseChallenge>()?.m_BasicMenu;

		public static GameObject getRenameButton(this Panel_Base panel) =>
			panel.TryCast<Panel_ChooseSandbox>()?.m_MouseButtonRename ?? panel.TryCast<Panel_ChooseChallenge>()?.m_MouseButtonRename;

		public static GameObject getDeleteButton(this Panel_Base panel) =>
			panel.TryCast<Panel_ChooseSandbox>()?.m_MouseButtonDelete ?? panel.TryCast<Panel_ChooseChallenge>()?.m_MouseButtonDelete;
	}
}