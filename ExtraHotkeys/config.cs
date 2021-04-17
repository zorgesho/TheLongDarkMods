using System.Collections.Generic;
using UnityEngine;
using Common.Configuration;

namespace ExtraHotkeys
{
	class ModConfig: Config
	{
		protected override void onLoad()
		{
			if (binds.Count == 0)
				Actions.actions.ForEach(action => binds[action.id] = KeyCode.None);
		}

		public readonly Dictionary<string, KeyCode> binds = new Dictionary<string, KeyCode>();
	}
}