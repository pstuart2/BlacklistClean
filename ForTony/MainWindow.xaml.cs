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

namespace ForTony
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
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
		/// Execute me!
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnExecute_Click(object sender, RoutedEventArgs e)
		{
			EmailListCleaner.Get().BackupFiles = chkBackupFiles.IsChecked.Value;
			EmailListCleaner.Get().BlackListFile = txtBlackList.Text;
			EmailListCleaner.Get().DirectoryToClean = txtClean.Text;

			EmailListCleanResults results = EmailListCleaner.Get().Clean();

			txtLog.Text = results.Log.ToString();
		}

		private bool CheckValidityAndEnable()
		{
			bool isValid = true;
			// Ensure both text boxes have values.
			if (!string.IsNullOrWhiteSpace(txtClean.Text)
				&& !string.IsNullOrWhiteSpace(txtBlackList.Text))
			{
				// Ensure the validity of those values.
				if (!System.IO.File.Exists(txtBlackList.Text)) isValid = false;
				if (!System.IO.Directory.Exists(txtClean.Text)) isValid = false;
			}
			else
			{
				isValid = false;
			}

			btnExecute.IsEnabled = isValid;
			return isValid;
		}
	}
}
