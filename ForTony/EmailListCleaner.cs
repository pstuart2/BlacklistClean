using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ForTony
{
	public class EmailListCleaner
	{
		private static EmailListCleaner __Instance = null;

		public bool BackupFiles { get; set; }
		public string BlackListFile { get; set; }
		public string DirectoryToClean { get; set; }
		public string FilterFile { get; set; }

		private EmailListCleanResults Results { get; set; }

		private string ExecuteStamp { get; set; }

		private List<string> __BlackListEmails = null;
		private List<string> BlackListEmails
		{
			get
			{
				if (__BlackListEmails == null) __BlackListEmails = new List<string>();
				return __BlackListEmails;
			}
		}

		private List<string> __Filters = null;
		private List<string> Filters
		{
			get
			{
				if (__Filters == null) __Filters = new List<string>();
				return __Filters;
			}
		}

		private List<string> __EmailsKept = null;
		private List<string> EmailsKept
		{
			get
			{
				if (__EmailsKept == null) __EmailsKept = new List<string>();
				return __EmailsKept;
			}
		}

		private List<string> __EmailsRemoved = null;
		private List<string> EmailsRemoved
		{
			get
			{
				if (__EmailsRemoved == null) __EmailsRemoved = new List<string>();
				return __EmailsRemoved;
			}
		}

		private EmailListCleaner()
		{

		}

		public static EmailListCleaner Get()
		{
			if (__Instance == null)
				__Instance = new EmailListCleaner();
			return __Instance;
		}

		/// <summary>
		/// The actual meat of the application. This will actually do the cleaning.
		/// </summary>
		/// <returns></returns>
		public EmailListCleanResults Clean()
		{
			Results = new EmailListCleanResults();

			Results.WasSuccess = true;

			ExecuteStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

			// Ensure both text boxes have values.
			if (!string.IsNullOrWhiteSpace(DirectoryToClean)
				&& !string.IsNullOrWhiteSpace(BlackListFile))
			{
				// Ensure the validity of those values.
				if (!string.IsNullOrWhiteSpace(BlackListFile) && !File.Exists(BlackListFile))
				{
					Results.Log.AppendFormat("Black list file [{0}] doesn't exist.", BlackListFile)
						.AppendLine();
					Results.WasSuccess = false;
				}

				if (!string.IsNullOrWhiteSpace(FilterFile) && !File.Exists(FilterFile))
				{
					Results.Log.AppendFormat("Filter list file [{0}] doesn't exist.", BlackListFile)
						.AppendLine();
					Results.WasSuccess = false;
				}

				if (!Directory.Exists(DirectoryToClean))
				{
					Results.Log.AppendFormat("Directory to clean [{0}] doesn't exist.", DirectoryToClean)
						.AppendLine();
					Results.WasSuccess = false;
				}
			}
			else
			{
				Results.Log.AppendFormat("Direcotry to clean [{0}] or BlackListFile [{1}] is not set.",
					DirectoryToClean, BlackListFile)
					.AppendLine();
				Results.WasSuccess = false;
			}

			// Should we continue?
			if (Results.WasSuccess)
			{
				if (!ProcessBlackListFile() || !ProcessFilterFile() || !ProcessDirectory())
				{
					// There was an error.
					Results.WasSuccess = false;
				}
			}

			Results.Log.AppendLine().Append("Results").AppendLine()
				.Append("-------------------------------------").AppendLine();
			Results.Log.AppendFormat("{0} files cleaned.", Results.FilesCleaned).AppendLine();
			Results.Log.AppendFormat("{0} total emails kept.", Results.TotalEmailsKept).AppendLine();
			Results.Log.AppendFormat("{0} total unique emails kept.", Results.TotalUniqueEmailsKept).AppendLine();
			Results.Log.AppendFormat("{0} total emails removed.", Results.TotalEmailsRemoved).AppendLine();
			Results.Log.AppendFormat("{0} total unique emails removed.", Results.TotalUniqueEmailsRemoved).AppendLine();

			return Results;
		}

		/// <summary>
		/// Load our black list file.
		/// </summary>
		/// <returns></returns>
		private bool ProcessBlackListFile()
		{
			if(string.IsNullOrWhiteSpace(BlackListFile)) { return true; }

			bool wasSuccess = false;

			try
			{
				using (StreamReader sr = new StreamReader(BlackListFile))
				{
					wasSuccess = true;
					string line = sr.ReadLine();
					while (!string.IsNullOrWhiteSpace(line))
					{
						if (!BlackListEmails.Contains(line))
							BlackListEmails.Add(line);
						else
						{
							Results.Log.AppendFormat("Blacklist has duplicate email: {0}", line).AppendLine();
						}
						line = sr.ReadLine();
					}

					sr.Close();
				}
			}
			catch (Exception e)
			{
				wasSuccess = false;
				Results.Log.AppendFormat("Exception: {0}", e.Message).AppendLine();
			}
			return wasSuccess;
		}

		/// <summary>
		/// Load our filter file.
		/// </summary>
		/// <returns></returns>
		private bool ProcessFilterFile()
		{
			if (string.IsNullOrWhiteSpace(FilterFile)) { return true; }

			bool wasSuccess = false;

			try
			{
				using (StreamReader sr = new StreamReader(FilterFile))
				{
					wasSuccess = true;
					string line = sr.ReadLine();
					while (!string.IsNullOrWhiteSpace(line))
					{
						if (!Filters.Contains(line))
							Filters.Add(line);
						line = sr.ReadLine();
					}

					sr.Close();
				}
			}
			catch (Exception e)
			{
				wasSuccess = false;
				Results.Log.AppendFormat("Exception: {0}", e.Message).AppendLine();
			}
			return wasSuccess;
		}

		/// <summary>
		/// Process our clean directory.
		/// </summary>
		/// <returns></returns>
		private bool ProcessDirectory()
		{
			bool wasSuccess = false;

			try
			{
				string[] files = Directory.GetFiles(DirectoryToClean, "*.csv");
				if (files.Length > 0)
				{
					wasSuccess = true;
					foreach (string file in files)
					{
						if (file != BlackListFile && file != FilterFile)
						{
							if (!ProcessFileInDirectory(file))
							{
								Results.Log.AppendFormat("Failed processing file: {0}", file)
									.AppendLine();
								wasSuccess = false;
								break;
							}

							Results.FilesCleaned++;
						}
					}
				}
			}
			catch (Exception e)
			{
				wasSuccess = false;
				Results.Log.AppendFormat("Exception: {0}", e.Message).AppendLine();
			}

			return wasSuccess;
		}

		/// <summary>
		/// Processes a single file in the directory.
		/// </summary>
		/// <param name="currentFile">File we want to process</param>
		/// <returns></returns>
		private bool ProcessFileInDirectory(string currentFile)
		{
			bool wasSuccess = false;

			Results.Log.AppendFormat("Processing file: {0}", currentFile)
				.AppendLine();

			try
			{
				// First move our file.
				string oldFile = string.Format("{0}.{1}.old", currentFile, ExecuteStamp);

				File.Move(currentFile, oldFile);
				using (StreamReader srRead = new StreamReader(oldFile))
				using (StreamWriter srWrite = new StreamWriter(currentFile))
				{
					string line = srRead.ReadLine();
					while (!string.IsNullOrWhiteSpace(line))
					{
						bool foundFilter = false;
						if (!BlackListEmails.Contains(line))
						{
							foreach (string filter in Filters)
							{
								if (line.Contains(filter))
								{
									Results.Log.AppendFormat("Removing email [{0}] because of filter '{1}'.", line, filter)
										.AppendLine();
									Results.TotalEmailsRemoved++;
									if (!EmailsRemoved.Contains(line))
									{
										Results.TotalUniqueEmailsRemoved++;
										EmailsRemoved.Add(line);
									}

									foundFilter = true;
									break;
								}
							}

							if (!foundFilter)
							{
								Results.TotalEmailsKept++;
								if (!EmailsKept.Contains(line))
								{
									Results.TotalUniqueEmailsKept++;
									EmailsKept.Add(line);
								}

								srWrite.WriteLine(line);
							}
						}
						else
						{
							Results.Log.AppendFormat("Email [{0}] found in blacklist. Removing...", line)
								.AppendLine();
							Results.TotalEmailsRemoved++;
							if (!EmailsRemoved.Contains(line))
							{
								Results.TotalUniqueEmailsRemoved++;
								EmailsRemoved.Add(line);
							}
						}

						line = srRead.ReadLine();
					}

					srWrite.Flush();
					srWrite.Close();
					srRead.Close();
					wasSuccess = true;
				}

				if (!BackupFiles)
				{
					File.Delete(oldFile);
				}
			}
			catch (Exception e)
			{
				wasSuccess = false;
				Results.Log.AppendFormat("Exception: {0}", e.Message).AppendLine();
			}
			return wasSuccess;
		}
	}
}
