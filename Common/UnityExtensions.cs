using System;

using UnityEngine;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace Common
{
	using Object = UnityEngine.Object;

	static class ObjectAndComponentExtensions
	{
		public static void setParent(this GameObject go, GameObject parent) => go.transform.SetParent(parent.transform, false);
		public static GameObject getParent(this GameObject go) => go.transform.parent?.gameObject;

		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;

		public static GameObject createChild(this GameObject go, string name, Vector3? pos = null)
		{
			GameObject child = new (name);
			child.setParent(go);

			if (pos != null)
				child.transform.position = (Vector3)pos;

			return child;
		}

		public static GameObject createChild(this GameObject go, GameObject prefab, string name = null, Vector3? pos = null, Vector3? localPos = null, Vector3? localScale = null)
		{
			var child = Object.Instantiate(prefab, go.transform);

			if (name != null)		child.name = name;
			if (pos != null)		child.transform.position = (Vector3)pos;
			if (localPos != null)	child.transform.localPosition = (Vector3)localPos;
			if (localScale != null)	child.transform.localScale = (Vector3)localScale;

			return child;
		}

		// for some reason, GetComponentsInChildren<T>() is not working well
		public static void forEachComponentInChildren<T>(this GameObject go, Action<T> action) where T: Il2CppObjectBase =>
			go.GetComponentsInChildren(Il2CppType.Of<T>()).forEach(cmp => action(cmp.TryCast<T>()));

		static void _destroy(this Object obj, bool immediate)
		{
			if (immediate)
				Object.DestroyImmediate(obj);
			else
				Object.Destroy(obj);
		}

		public static void destroyComponent<T>(this GameObject go, bool immediate = true) where T: Component =>
			go.GetComponent<T>()?._destroy(immediate);

		public static string baseName(this GameObject go) => go.name.Replace("(Clone)", "");
	}

	static class StructsExtensions
	{
		public static string toStringRGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);
	}
}