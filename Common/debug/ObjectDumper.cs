using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using UnityEngine;
using UnhollowerRuntimeLib;

namespace Common
{
	using Reflection;

	static partial class Debug
	{
#if DEBUG
		const string pathForDumps = "c:/projects/tld/dumps/";
#endif
		public static string dumpGameObject(GameObject go, bool dumpProperties = true, bool dumpFields = false) =>
			ObjectDumper.dump(go, dumpProperties, dumpFields);

		public static void dump(this GameObject go, string filename = null, int dumpParent = 0)
		{
			while (dumpParent-- > 0 && go.getParent())
				go = go.getParent();

			filename ??= go.name.Replace("(Clone)", "").ToLower();
#if DEBUG
			Paths.ensurePath(pathForDumps);
			filename = pathForDumps + filename;
#endif
			ObjectDumper.dump(go, true, true).saveToFile(filename + ".yml");
		}


		static class ObjectDumper
		{
			const string indentStep = "    ";

			static readonly StringBuilder output = new();
			static readonly Regex sanitizer = new ("[\\r\\n\\t\0]", RegexOptions.Compiled);

			static bool dumpProperties;
			static bool dumpFields;

			public static string dump(GameObject go, bool dumpProperties, bool dumpFields)
			{
				output.Clear();
				ObjectDumper.dumpProperties = dumpProperties;
				ObjectDumper.dumpFields = dumpFields;

				dump(go, "");

				return output.ToString();
			}

			static void dump(GameObject go, string indent)
			{
				output.AppendLine($"{indent}gameobject: {go.name} activeS/activeH:{go.activeSelf}/{go.activeInHierarchy}");

				foreach (var cmp in go.GetComponents<Component>())
					dump(cmp, indent + indentStep, "component");

				for (int i = 0; i < go.transform.GetChildCount(); i++)
					dump(go.transform.GetChild(i).gameObject, indent + indentStep);
			}

			static void dump(Il2CppSystem.Object obj, string indent, string title = null)
			{
				if (obj == null) // it happens sometimes for some reason
				{
					output.AppendLine($"{indent}{title ?? ""}: NULL");
					return;
				}

				Il2CppSystem.Type objType = obj.GetIl2CppType();

				if (title != null)
					output.AppendLine($"{indent}{title}: {objType.Name}");

				try
				{
					var bf = ReflectionHelper.bfAll_Il2Cpp ^ Il2CppSystem.Reflection.BindingFlags.Static;

					if (dumpProperties)
					{
						var properties = objType.properties(bf).ToList();
						if (properties.Count > 0)
						{
							_sort(properties);
							output.AppendLine($"{indent}{indentStep}PROPERTIES:");

							foreach (var prop in properties)
								if (prop.GetGetMethod() != null)
									_dumpValue(prop.Name, prop.PropertyType, prop.GetValue(obj, null), indent);
						}
					}

					if (dumpFields)
					{
						var fields = objType.fields(bf).ToList();
						if (fields.Count > 0)
						{
							_sort(fields);
							output.AppendLine($"{indent}{indentStep}FIELDS:");

							foreach (var field in fields)
								_dumpValue(field.Name, field.FieldType, field.GetValue(obj), indent);
						}
					}
				}
				catch (Exception e) { Log.msg(e); }

				static void _sort<T>(List<T> list) where T: Il2CppSystem.Reflection.MemberInfo => list.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

				static void _dumpValue(string name, Il2CppSystem.Type type, Il2CppSystem.Object value, string indent)
				{
					string sanitized = value == null? "[null]": sanitizer.Replace(_toString(type, value), " ").Trim();
					output.AppendLine($"{indent}{indentStep}{name} [{type.Name}]: \"{sanitized}\"");
				}

				// best way I found for value types :( generics also won't work with that
				static string _toString(Il2CppSystem.Type type, Il2CppSystem.Object value)
				{
					if (type == Il2CppType.Of<int>())			return value.Unbox<int>().ToString();
					if (type == Il2CppType.Of<float>())			return value.Unbox<float>().ToString();
					if (type == Il2CppType.Of<Color>())			return value.Unbox<Color>().ToString();
					if (type == Il2CppType.Of<Vector2>())		return value.Unbox<Vector2>().ToString("F5");
					if (type == Il2CppType.Of<Vector3>())		return value.Unbox<Vector3>().ToString("F5");
					if (type == Il2CppType.Of<Vector4>())		return value.Unbox<Vector4>().ToString();
					if (type == Il2CppType.Of<Quaternion>())	return value.Unbox<Quaternion>().ToString();

					return value.ToString();
				}
			}
		}
	}
}