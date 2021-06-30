using System;
using System.IO;
using System.Collections.Generic;

// fix for C# 9.0 (pre-5.0 .NET)
namespace System.Runtime.CompilerServices { static class IsExternalInit {} }

namespace Common
{
	static class MiscExtensions
	{
		public static void forEach<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null)
				return;

			var enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext())
				action(enumerator.Current);
		}

		public static int findIndex<T>(this Il2CppSystem.Collections.Generic.List<T> list, Predicate<T> predicate)
		{
			return list.FindIndex(new Func<T, bool>(predicate));
		}
	}

	static class ArrayExtensions
	{
		public static bool isNullOrEmpty(this Array array) => array == null || array.Length == 0;
	}

	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

		static string formatFileName(string filename)
		{
			return filename.isNullOrEmpty()? filename: Paths.makeRootPath(Paths.ensureExtension(filename, "txt"));
		}

		public static bool startsWith(this string s, string str) => s.StartsWith(str, StringComparison.Ordinal);

		public static void saveToFile(this string s, string localPath)
		{
			string targetPath = formatFileName(localPath);
			Paths.ensurePath(targetPath);

			try { File.WriteAllText(targetPath, s); }
			catch (Exception e) { Log.msg(e); }
		}
	}
}