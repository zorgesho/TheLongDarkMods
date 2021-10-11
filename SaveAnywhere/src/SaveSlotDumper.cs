#if DEBUG
using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Common;

namespace SaveAnywhere
{
	static class SaveSlotDumper
	{
		static readonly string dumpsPath = $"{PersistentDataPath.m_Path}\\saves-dump\\";

		static JObject deserializeSafe(string str)
		{
			JObject result = null;

			try { result = JsonConvert.DeserializeObject(str) as JObject; }
			catch (Exception) {}

			return result;
		}

		static void dump(JObject jobj, string name)
		{
			string str = JsonConvert.SerializeObject(jobj, new JsonSerializerSettings() { Formatting = Formatting.Indented });
			str.saveToFile($"{dumpsPath}{name}.json");

			foreach (var prop in jobj.Properties())
			{
				if (deserializeSafe(prop.Value.ToString()) is JObject propObj)
					dump(propObj, $"{name}.{prop.Name}");
			}
		}

		public static void dump(string slotName)
		{
			if (SaveGameSlots.GetSaveSlotFromName(slotName) is not SlotData slotData)
				return;

			foreach (var key in slotData.m_Dict.Keys)
			{
				string slotStr = SaveGameSlots.LoadDataFromSlot(slotName, key);

				if (deserializeSafe(slotStr) is JObject slotObj)
					dump(slotObj, $"{slotName}.{key}");
			}
		}
	}
}
#endif // DEBUG