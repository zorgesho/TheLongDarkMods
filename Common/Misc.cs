using System;
using System.Collections.Generic;

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
	}

	static class ArrayExtensions
	{
		public static bool isNullOrEmpty(this Array array) => array == null || array.Length == 0;
	}

	static partial class StringExtensions
	{
		public static bool isNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
	}
}