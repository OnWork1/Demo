using System;
using System.Diagnostics;
using System.IO;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging
{
	public class FolderDateRollingFileAppender : log4net.Appender.RollingFileAppender
	{
		/// <summary>
		/// Open new file and also prepare new path for next scheduled file.
		/// </summary>
		/// <param name="fileName">filename</param>
		/// <param name="append"></param>
		protected override void OpenFile(string fileName, bool append)
		{
			//m_scheduledFilename is not public field, get and set values must be done by reflection
			System.Reflection.FieldInfo fi = typeof(log4net.Appender.RollingFileAppender).GetField("m_scheduledFilename", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			object val = (string)fi.GetValue(this);

			//do all preparations only if value exists!!
			//do not change path for RollOverSize method, because of chaining date folders
			if (val != null && new StackTrace(false).GetFrame(2).GetMethod().Name != "RollOverSize")
			{
				string currentScheduledFilename = (string)val;

				//Inject folder [yyyyMMdd] before the file name
				string baseDirectory = Path.GetDirectoryName(currentScheduledFilename);
				string fileNameOnly = Path.GetFileName(currentScheduledFilename);
				string newDirectory = Path.Combine(baseDirectory, DateTime.Now.ToString("yyyyMMdd"));
				string newFileName = Path.Combine(newDirectory, fileNameOnly);

				//set value with injected date
				fi.SetValue(this, (object)newFileName);

				//create directory for future rollover file, move will not be executed to folder
				//and file stays in original (base) folder without these lines
				if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
				}
			}

			//open logfile as normaly
			base.OpenFile(fileName, append);
		}

		
	}
}