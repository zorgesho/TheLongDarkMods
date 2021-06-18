using System;
using System.Linq;
using System.Reflection;

using HarmonyLib;

namespace Common
{
	using Reflection;

	// is class have methods that can be used as harmony patches (for more: void patch(Type typeWithPatchMethods))
	[AttributeUsage(AttributeTargets.Class)]
	public class PatchClassAttribute: Attribute {}

	static class HarmonyHelper
	{
		public static HarmonyLib.Harmony harmonyInstance { get; private set; }

		public static void init(HarmonyLib.Harmony hInstance) => harmonyInstance ??= hInstance;

		public static void patchAll()
		{
			try
			{
				ReflectionHelper.definedTypes.Where(type => type.checkAttr<PatchClassAttribute>()).forEach(type => patch(type));
			}
			catch (Exception e)
			{
				Log.msg(e, "HarmonyHelper.patchAll"); // so the exception will be in the mod's log
				throw e;
			}
		}

		public static void patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null)
		{
			Debug.assert(original != null);										$"HarmonyHelper.patch: patching '{original.fullName()}' with prefix:'{prefix}' postfix:'{postfix}'".logDbg();

			if (original == null && "HarmonyHelper.patch: target method is null".logError())
				return;

			try
			{
				static HarmonyMethod _harmonyMethod(MethodInfo method) => (method == null)? null: new HarmonyMethod(method);
				harmonyInstance.Patch(original, _harmonyMethod(prefix), _harmonyMethod(postfix), null);
			}
			catch (Exception e)
			{
				Log.msg(e, "HarmonyHelper.patch");
				throw e;
			}
		}

		// use methods from 'typeWithPatchMethods' class as harmony patches
		// valid method need to have HarmonyPatch and Harmony[Prefix/Postfix/Transpiler] attributes
		public static void patch(Type typeWithPatchMethods) => harmonyInstance.PatchAll(typeWithPatchMethods);
	}

	static class HarmonyExtensions
	{
		public static MethodBase getTargetMethod(this HarmonyMethod harmonyMethod)
		{
			if (harmonyMethod.methodName != null)
				return harmonyMethod.declaringType?.method(harmonyMethod.methodName, harmonyMethod.argumentTypes);

			if (harmonyMethod.methodType == MethodType.Constructor)
				return harmonyMethod.declaringType?.GetConstructor(ReflectionHelper.bfAll, null, harmonyMethod.argumentTypes, null);

			return null;
		}
	}
}