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

		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;

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
	}
}