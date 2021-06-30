using System.Reflection;
using MelonLoader;

namespace Common
{
	public abstract class Mod: MelonMod
	{
		public static readonly string id = Assembly.GetExecutingAssembly().GetName().Name;

		protected virtual void init() {}

		public override void OnApplicationStart()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			init();
		}
	}
}