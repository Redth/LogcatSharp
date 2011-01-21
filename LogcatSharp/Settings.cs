using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LogcatSharp
{
	public class Settings
	{
		public static string GetPath()
		{
			var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb-path.txt");

			if (File.Exists(file))
			{
				var adbPath = File.ReadAllText(file);

				if (File.Exists(adbPath))
					return adbPath;
			}

			return null;
		}

		public static void SavePath(string adbPath)
		{
			var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "adb-path.txt");

			try
			{
				File.WriteAllText(file, adbPath);
			}
			catch { }
		}
	}
}
