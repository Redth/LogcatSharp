using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LogcatSharp
{
	public partial class frmAdbFinder : Form
	{
		public frmAdbFinder()
		{
			InitializeComponent();
		}

		private void frmAdbFinder_Load(object sender, EventArgs e)
		{
			var task = Task.Factory.StartNew(() =>
			{
				foreach (var drive in DriveInfo.GetDrives())
				{
					if (!drive.IsReady)
						continue;

					try
					{
						this.AdbPath = FindAdb(drive.RootDirectory);
						break;
					}
					catch (UnauthorizedAccessException)
					{
					}
				}

				done(!string.IsNullOrEmpty(this.AdbPath) && File.Exists(this.AdbPath));

			}).ContinueWith((Task t) => {
				done(false);
			}, TaskContinuationOptions.OnlyOnFaulted);
		}

		private string FindAdb(DirectoryInfo dir)
		{
			var files = dir.GetFiles("adb.exe");

			if (files != null && files.Count() > 0)
				return files[0].FullName;

			var subDirs = dir.GetDirectories();

			foreach (var subDir in subDirs)
			{
				try
				{
					var adbPath = FindAdb(subDir);

					if (!string.IsNullOrEmpty(adbPath))
						return adbPath;
				}
				catch (UnauthorizedAccessException) { }
			}

			return null;
		}

		
		public string AdbPath
		{
			get;
			private set;
		}

		private void done(bool success)
		{
			if (this.InvokeRequired)
				this.Invoke(new Action<bool>(done), success);
			else
			{
				this.DialogResult = success ? DialogResult.OK : DialogResult.Abort;
				this.Close();
			}
		}
	}
}
