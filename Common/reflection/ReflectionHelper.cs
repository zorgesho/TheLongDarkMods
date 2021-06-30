using System.Reflection;

namespace Common.Reflection
{
	static class ReflectionHelper
	{
		public const BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		public const Il2CppSystem.Reflection.BindingFlags bfAll_Il2Cpp =
			Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Static;
	}
}