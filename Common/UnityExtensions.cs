using UnityEngine;

namespace Common
{
	static class ObjectAndComponentExtensions
	{
		public static GameObject getChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;
	}
}