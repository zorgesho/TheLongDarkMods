using UnityEngine;

namespace Common
{
	static class ObjectAndComponentExtensions
	{
		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;

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