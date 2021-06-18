using System;
using System.Reflection;
using System.Runtime.InteropServices;

using UnhollowerBaseLib;

namespace Common.Reflection
{
	static class ObjectExtensions
	{
		public static T cast<T>(this object obj)
		{
			try
			{																				$"cast<{typeof(T)}>(): object is null, default value is used".logDbg(obj == null);
				return obj == null? default: (T)obj;
			}
			catch
			{
				string msg = $"cast error: {obj}; {obj.GetType()} -> {typeof(T)}";
				Debug.assert(false, msg);
				msg.logError();

				return default;
			}
		}
	}

	static class TypeExtensions
	{
		static MethodInfo _method(this Type type, string name, BindingFlags bf, Type[] types)
		{
			try { return types == null? type.GetMethod(name, bf): type.GetMethod(name, bf, null, types, null); }
			catch (AmbiguousMatchException)
			{
				$"Ambiguous method: {type.Name}.{name}".logError();
			}
			catch (Exception e) { Log.msg(e); }

			return null;
		}

		public static FieldInfo field(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => type.GetField(name, bf);
		public static MethodInfo method(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => _method(type, name, bf, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, ReflectionHelper.bfAll, types);

		public static FieldInfo[] fields(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetFields(bf);
		public static MethodInfo[] methods(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetMethods(bf);
	}

	static class Il2CppTypeExtensions
	{
		public static Il2CppSystem.Reflection.FieldInfo[] fields(this Il2CppSystem.Type type, Il2CppSystem.Reflection.BindingFlags bf = ReflectionHelper.bfAll_Il2Cpp) => type.GetFields(bf);
		public static Il2CppSystem.Reflection.PropertyInfo[] properties(this Il2CppSystem.Type type, Il2CppSystem.Reflection.BindingFlags bf = ReflectionHelper.bfAll_Il2Cpp) => type.GetProperties(bf);
	}

	static class Il2CppHelper
	{
		[DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern void il2cpp_gc_wbarrier_set_field(IntPtr obj, IntPtr targetAddress, IntPtr value);

		static IntPtr getFieldAddress(Il2CppObjectBase obj, string fieldName) =>
			IL2CPP.Il2CppObjectBaseToPtrNotNull(obj) + (int)IL2CPP.il2cpp_field_get_offset((IntPtr)obj.GetType().field("NativeFieldInfoPtr_" + fieldName).GetValue(null));

		public static void setFieldValue(Il2CppObjectBase obj, string fieldName, Il2CppObjectBase value) =>
			il2cpp_gc_wbarrier_set_field(obj.Pointer, getFieldAddress(obj, fieldName), value.Pointer);
	}

	static class MemberInfoExtensions
	{
		public static string fullName(this MemberInfo memberInfo)
		{
			if (memberInfo == null)
				return "[null]";

			if ((memberInfo.MemberType & (MemberTypes.Method | MemberTypes.Field | MemberTypes.Property)) != 0)
				return $"{memberInfo.DeclaringType.FullName}.{memberInfo.Name}";

			if ((memberInfo.MemberType & (MemberTypes.TypeInfo | MemberTypes.NestedType)) != 0)
				return (memberInfo as Type).FullName;

			return memberInfo.Name;
		}

		public static MethodWrapper wrap(this MethodInfo method) => new (method);

		public static A getAttr<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A;
		public static A[] getAttrs<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.GetCustomAttributes(memberInfo, typeof(A)) as A[];
		public static bool checkAttr<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.IsDefined(memberInfo, typeof(A));
	}
}