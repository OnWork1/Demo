using System;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Exceptions
{
	public class FatalFailureException : ApplicationException
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors
		public FatalFailureException(String message)
			: base
			(message)
		{ }
		public FatalFailureException(String message, OperationResult operationResult)
			: base
			(message)
		{
			this.OperationResult = operationResult;
		}
		public FatalFailureException(String message, OperationResultWithSubTasks operationResultWithSubTasks)
			: base
			(message)
		{
			this.OperationResultWithSubTasks = operationResultWithSubTasks;
		}

		// Private Properties

		// Protected Properties

		// Public Properties
		public OperationResult OperationResult { get; set; }
		public OperationResultWithSubTasks OperationResultWithSubTasks { get; set; }

		// Private Methods

		// Protected Methods

		// Public Methods

		// Event Handlers

	}
}
