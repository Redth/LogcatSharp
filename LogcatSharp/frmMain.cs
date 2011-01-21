using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace LogcatSharp
{
	public partial class frmMain : Form
	{
		string adbPath = string.Empty;

		public frmMain()
		{
			InitializeComponent();
		}

		Process adb = null;
		
		private void frmMain_Load(object sender, EventArgs e)
		{

			this.toolStripComboBoxFilterType.Text  = "Simple";

			this.adbPath = Settings.GetPath();

			if (string.IsNullOrEmpty(adbPath) || !System.IO.File.Exists(adbPath))
			{
				frmAdbFinder faf = new frmAdbFinder();
				if (faf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.adbPath = faf.AdbPath;
					Settings.SavePath(faf.AdbPath);
				}
				else
				{
					MessageBox.Show(this, "Could not find adb.exe.  Please install the SDK and Emulator and try again!  Alternatively you can set the path manually adb-path.txt", "Failed to find adb.exe", MessageBoxButtons.OK);

					this.Close();
					Application.Exit();
				}
			}
		}

		void clear()
		{
			if (adb == null || adb.HasExited)
			{
				try
				{
					adb = new Process();
					adb.StartInfo.UseShellExecute = false;
					adb.StartInfo.FileName = @"C:\Program Files (x86)\Android\android-sdk-windows\platform-tools\adb.exe";//
					adb.StartInfo.Arguments = "logcat -c";
					adb.StartInfo.CreateNoWindow = true;
					adb.Start();
					adb.WaitForExit(1500);
				}
				catch { }
			}
		}

		void stop()
		{
			this.toolStripButtonStop.Enabled = false;

			if (adb != null && !adb.HasExited)
			{
				adb.Kill();
			}

			this.toolStripButtonStart.Enabled = true;
		}

		void start()
		{
			this.toolStripButtonStart.Enabled = false;

			if (adb != null && !adb.HasExited)
				return;

			adb = new Process();
			adb.StartInfo.UseShellExecute = false;
			adb.StartInfo.FileName = @"C:\Program Files (x86)\Android\android-sdk-windows\platform-tools\adb.exe";//
			adb.StartInfo.Arguments = "logcat";
			adb.StartInfo.RedirectStandardOutput = true;
			adb.StartInfo.RedirectStandardError = true;
			adb.EnableRaisingEvents = true;
			adb.StartInfo.CreateNoWindow = true;
			adb.ErrorDataReceived += new DataReceivedEventHandler(adb_ErrorDataReceived);
			adb.OutputDataReceived += new DataReceivedEventHandler(adb_OutputDataReceived);


			try { var started = adb.Start(); }
			catch (Exception ex)
			{
				this.toolStripButtonStart.Enabled = true;
				this.toolStripButtonStop.Enabled = false;

				Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
			}

			adb.BeginErrorReadLine();
			adb.BeginOutputReadLine();

			this.toolStripButtonStop.Enabled = true;
		}

		void adb_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (this.InvokeRequired)
				this.Invoke(new Action<object, DataReceivedEventArgs>(adb_OutputDataReceived), sender, e);
			else
				textAdb.AppendText(filterData(e.Data));
		}

		void adb_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (this.InvokeRequired)
				this.Invoke(new Action<object, DataReceivedEventArgs>(adb_ErrorDataReceived), sender, e);
			else
				textAdb.AppendText(filterData(e.Data));
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			stop();
			clear();
			start();
		}

		string filterData(string data)
		{
			if (string.IsNullOrWhiteSpace(data))
				return string.Empty;

			if (!string.IsNullOrEmpty(this.toolStripTextBoxFilter.Text))
			{
				var pattern = !this.toolStripComboBoxFilterType.Text.Equals("Regex") ? this.toolStripTextBoxFilter.Text.Replace("*", ".*")
					: this.toolStripTextBoxFilter.Text;
				System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

				if (rx.IsMatch(data))
					return data.Trim() + Environment.NewLine;
				else
					return string.Empty;
			}

			return data.Trim() + Environment.NewLine;
		}

		private void toolStripButtonStart_Click(object sender, EventArgs e)
		{
			start();
		}

		private void toolStripButtonStop_Click(object sender, EventArgs e)
		{
			stop();
		}
	}
}
