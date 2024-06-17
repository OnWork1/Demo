using System;
using System.Collections.Generic;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Errors
{
	public static class ErrorsCounts
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors
		#region ErrorsCounts()
		static ErrorsCounts()
		{ ErrorsCounts.ContinuousErrorsCount = new Dictionary<Guid, int>(); } 
		#endregion

		// Private Properties
		#region ContinuousErrorsCount
		private static Dictionary<Guid, int> ContinuousErrorsCount { get; set; } 
		#endregion

		// Protected Properties

		// Public Properties
		
		// Private Methods

		// Protected Methods
		#region IncrementErrorCount(Guid timerGuid)
		public static int IncrementErrorCount(Guid timerGuid)
		{
			if (ErrorsCounts.ContinuousErrorsCount.ContainsKey(timerGuid))
			{
				ErrorsCounts.ContinuousErrorsCount[timerGuid]++;
				return ErrorsCounts.ContinuousErrorsCount[timerGuid];
			}
			ErrorsCounts.ContinuousErrorsCount.Add(timerGuid, 1);
			return 1;
		} 
		#endregion

		#region SetErrorCountValue(Guid timerGuid, int value)
		public static void SetErrorCountValue(Guid timerGuid, int value)
		{
			if (ErrorsCounts.ContinuousErrorsCount.ContainsKey(timerGuid))
			{
				ErrorsCounts.ContinuousErrorsCount[timerGuid] = value;
			}
			else
			{
				ErrorsCounts.ContinuousErrorsCount.Add(timerGuid, value);
			}
		} 
		#endregion

		#region ResetErrorCountValue(Guid timerGuid)
		public static void ResetErrorCountValue(Guid timerGuid)
		{
			ErrorsCounts.SetErrorCountValue(timerGuid, 0);
		} 
		#endregion

		#region GetErrorCountValue(Guid timerGuid)
		public static int GetErrorCountValue(Guid timerGuid)
		{
			return ErrorsCounts.ContinuousErrorsCount[timerGuid];
		} 
		#endregion

		#region CountThisState(Guid timerGuid, bool wasTaskSuccessful)
		public static int CountThisState(Guid timerGuid, bool wasTaskSuccessful)
		{
			if (wasTaskSuccessful)
			{
				ErrorsCounts.ResetErrorCountValue(timerGuid);
				return 0;
			}

			return ErrorsCounts.IncrementErrorCount(timerGuid);
		} 
		#endregion

		// Public Methods

		// Event Handlers
	}
}
