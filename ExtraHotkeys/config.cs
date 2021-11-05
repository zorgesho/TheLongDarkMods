using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.Configuration;

namespace ExtraHotkeys
{
	class ModConfig: Config
	{
		protected override void onLoad()
		{
			void _resetBind(Actions.InventoryAction action) => binds[action.id] = KeyCode.None;

			if (binds.Count == 0)
			{
				Actions.actions.ForEach(_resetBind);
			}
			else
			{
				// updating binds in case action list is changed
				var listToRemove = binds.Where(bind => !Actions.actions.Exists(action => action.id == bind.Key)).ToList();
				listToRemove.ForEach(bind => binds.Remove(bind.Key));

				Actions.actions.Where(action => !binds.ContainsKey(action.id)).forEach(_resetBind);
			}
		}

		public readonly Dictionary<string, KeyCode> binds = new();
	}
}