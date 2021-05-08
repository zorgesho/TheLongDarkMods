using System;

#if DEBUG
using System.Diagnostics;
#endif

namespace Common
{
	static partial class Debug
	{
		public static Profiler profiler(string message = null) =>
#if DEBUG
			new (message);
#else
			null;
#endif

		public class Profiler: IDisposable
		{
#if DEBUG
			public static double lastResult { get; private set; }

			readonly string message = null;
			readonly Stopwatch stopwatch = null;
			readonly long mem = GC.GetTotalMemory(false);

			public Profiler(string message)
			{
				this.message = message;
				stopwatch = Stopwatch.StartNew();
			}

			public void Dispose()
			{
				stopwatch.Stop();
				lastResult = stopwatch.Elapsed.TotalMilliseconds;

				if (message == null)
					return;

				long m = GC.GetTotalMemory(false) - mem;
				string memChange = $"{(m > 0? "+": "")}{(Math.Abs(m) > 1024L * 1024L? (m / 1024L / 1024L + "MB"): (m / 1024L + "KB"))} ({m})";

				string result = $"{message}: {lastResult} ms; mem alloc:{memChange}";
				$"PROFILER: {result}".log();
			}

			public static void _logCompare(double prevResult)
			{
				string res = $"PROFILER: DIFF {prevResult} ms -> {lastResult} ms, delta: {(lastResult - prevResult) / prevResult * 100f:F2} %";
				res.log();
			}
#else
			public void Dispose() {}
#endif
		}
	}
}