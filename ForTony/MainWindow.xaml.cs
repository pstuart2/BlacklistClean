using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using ForTony.Properties;

namespace ForTony
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Settings __Settings = null;
		private Settings UserSettings
		{
			get
			{
				if (__Settings == null)
				{
					__Settings = new Settings();
					__Settings.Reload();
				}

				return __Settings;
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			CheckValidityAndEnable();

			txtBlackList.Text = UserSettings.BlackListFile;
			txtClean.Text = UserSettings.CleanDirectory;
			txtFilter.Text = UserSettings.FilterFile;
			chkBackupFiles.IsChecked = UserSettings.KeepBackup;

			txtLog.Text = @"Directory to clean is required.
You can select a black list file for cleaning or a file for
removing phrases. You have to have at least one defined. You
can have both defined as well.";

			CheckValidityAndEnable();
		}

		/// <summary>
		/// Gets the black list file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnBlackList_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				CheckFileExists = true,
				Filter = "CSV|*.csv"
			};
			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				txtBlackList.Text = dialog.FileName;
			}

			CheckValidityAndEnable();
		}

		/// <summary>
		/// The clean button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnClean_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog();
			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				txtClean.Text = dialog.SelectedPath;
			}

			CheckValidityAndEnable();
		}

		/// <summary>
		/// Optional filter file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnFilter_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				CheckFileExists = true,
				Filter = "CSV|*.csv"
			};
			DialogResult result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				txtFilter.Text = dialog.FileName;
			}

			CheckValidityAndEnable();
		}

		/// <summary>
		/// Execute me!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnExecute_Click(object sender, RoutedEventArgs e)
		{
			UserSettings.KeepBackup = chkBackupFiles.IsChecked.Value;
			EmailListCleaner.Get().BackupFiles = chkBackupFiles.IsChecked.Value;

			UserSettings.BlackListFile = txtBlackList.Text;
			EmailListCleaner.Get().BlackListFile = txtBlackList.Text;

			UserSettings.CleanDirectory = txtClean.Text;
			EmailListCleaner.Get().DirectoryToClean = txtClean.Text;

			UserSettings.FilterFile = txtFilter.Text;
			EmailListCleaner.Get().FilterFile = txtFilter.Text;

			UserSettings.Save();

			EmailListCleanResults results = EmailListCleaner.Get().Clean();

			txtLog.Text = results.Log.ToString();
		}

		private bool CheckValidityAndEnable()
		{
			bool isValid = true;
			// Ensure both text boxes have values.
			if (!string.IsNullOrWhiteSpace(txtClean.Text)
				&& (!string.IsNullOrWhiteSpace(txtBlackList.Text) || 
				!string.IsNullOrWhiteSpace(txtFilter.Text)))
			{
				// Ensure the validity of those values.
				if (!System.IO.Directory.Exists(txtClean.Text)) isValid = false;
				if(!string.IsNullOrWhiteSpace(txtBlackList.Text))
					if (!System.IO.File.Exists(txtBlackList.Text)) isValid = false;
				if (!string.IsNullOrWhiteSpace(txtFilter.Text))
					if (!System.IO.File.Exists(txtFilter.Text)) isValid = false;
			}
			else
			{
				isValid = false;
			}

			btnExecute.IsEnabled = isValid;
			return isValid;
		}

		private void txtBlackList_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckValidityAndEnable();
		}

		private void txtClean_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckValidityAndEnable();
		}

		private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckValidityAndEnable();
		}
	}
}
