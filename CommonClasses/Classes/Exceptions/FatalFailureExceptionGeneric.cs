using System;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Exceptions
{
	public class FatalFailureException<T> : FatalFailureException
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors
		public FatalFailureException(string message)
			:base(message)
		{ }
		public FatalFailureException(String message, OperationResult<T> operationResult)
			: base
			(message)
		{
			this.OperationResult = operationResult;
		}
		public FatalFailureException(String message, OperationResultWithSubTasks<T> operationResultWithSubTasks)
			: base
			(message)
		{
			this.OperationResultWithSubTasks = operationResultWithSubTasks;
		}

		// Private Properties

		// Protected Properties

		// Public Properties
		public new OperationResult<T> OperationResult { get; set; }
		public new OperationResultWithSubTasks<T> OperationResultWithSubTasks { get; set; }

		// Private Methods

		// Protected Methods

		// Public Methods

		// Event Handlers

	}
}
