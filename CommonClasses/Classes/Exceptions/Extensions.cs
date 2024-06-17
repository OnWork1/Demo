using System;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Exceptions
{
	public static class Extensions
	{
		public static string DumpException(this Exception ex)
		{
			string message = "exception type: " + ex.GetType().Name + Environment.NewLine +
							 "message: " + (ex.Message ?? "null") + Environment.NewLine +
							 "stack trace:" + Environment.NewLine +
							 ex.StackTrace ?? "null";
			if (ex.InnerException != null)
			{
				message += Environment.NewLine + "Inner exception:" + Environment.NewLine +
						   DumpException(ex.InnerException);
			}
			return message;
		}

        #region SqlMinDate(this DateTime dt)
        /// <summary>
        /// Get minimal SQL Server Datetime value - 1/1/1900
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime SqlMinDate(this DateTime dt)
        {
           return new DateTime(1900, 1, 1, 0, 0, 0);
        }
        #endregion

    }
}
