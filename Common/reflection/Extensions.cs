using System;
using System.Reflection;
using System.Runtime.InteropServices;

using UnhollowerBaseLib;

namespace Common.Reflection
{
	static class TypeExtensions
	{
		public static FieldInfo field(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => type.GetField(name, bf);
		public static FieldInfo[] fields(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetFields(bf);
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
}