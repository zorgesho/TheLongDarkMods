using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Common
{
	static partial class Debug
	{
		[Conditional("DEBUG")]
		public static void assert(bool condition, string message = null, [CallerFilePath] string __filename = "", [CallerLineNumber] int __line = 0)
		{
			if (condition)
				return;

			string msg = $"Assertion failed{(message != null? ": " + message: "")} ({__filename}:{__line})";

			$"{msg}".logError();
			throw new Exception(msg);
		}
	}
}