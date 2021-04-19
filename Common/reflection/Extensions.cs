using System;
using System.Reflection;

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

		public static MethodInfo method(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll) => _method(type, name, bf, null);
		public static MethodInfo method(this Type type, string name, params Type[] types) => _method(type, name, ReflectionHelper.bfAll, types);

		public static FieldInfo[] fields(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetFields(bf);
		public static MethodInfo[] methods(this Type type, BindingFlags bf = ReflectionHelper.bfAll) => type.GetMethods(bf);
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

		public static MethodWrapper wrap(this MethodInfo method) => new(method);

		public static A getAttr<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A;
		public static A[] getAttrs<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.GetCustomAttributes(memberInfo, typeof(A)) as A[];
		public static bool checkAttr<A>(this MemberInfo memberInfo) where A: Attribute => Attribute.IsDefined(memberInfo, typeof(A));
	}
}