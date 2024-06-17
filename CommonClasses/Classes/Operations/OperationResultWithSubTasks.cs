using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Errors;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Exceptions;
using BMW.IntegrationService.CommonClassesAndEnums.Enums;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations
{
	public class OperationResultWithSubTasks
		: OperationResult
	{ 
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní promìné
		//private bool operationFailured;

		// Constructors - Konstruktory

		#region Constructors
		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid, String sourcePluginAssemblyName,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			String.Empty,
			String.Empty,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			null,
			String.Empty,
			DateTime.Now,
			null,
			null, interfaceRun) { }

		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid, String sourcePluginAssemblyName, string operationName,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			String.Empty,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			null,
			String.Empty,
			DateTime.Now,
			null,
			null,interfaceRun) { }

		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			operationDescription,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			null,
			String.Empty,
			DateTime.Now,
			null,
			null, interfaceRun) { }

		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			operationDescription,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			String.Empty,
			DateTime.Now,
			null,
			null, interfaceRun) { }

		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			operationDescription,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			DateTime.Now,
			null,
			null, interfaceRun) { }


		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message,
			DateTime operationStartTime,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			operationDescription,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			operationStartTime,
			null,
			null, interfaceRun) { }


		public OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message,
			DateTime operationStartTime, DateTime operationEndTime,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			operationDescription,
			sourcePluginGuid,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			operationStartTime,
			operationEndTime,
			null, interfaceRun) { }


		protected OperationResultWithSubTasks(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, 
			String operationName,
			String operationDescription,
			Guid sourcePluginGuid,
			String sourceModuleAssemblyName,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype,
			String message,
			DateTime operationStartTime,
			DateTime? operationEndTime,
			IEnumerable<OperationSubTask> operationSubTasks,
			bmw_interfacerun interfaceRun)
			: base(
			crmOrganizationUrl,
			taskName, timerGuid, operationName, operationDescription, sourcePluginGuid, sourceModuleAssemblyName, bmw_log_bmw_operationtype, message, true, operationStartTime, operationEndTime,
			interfaceRun)
		{
			
			this.OperationSubTasks = new List<OperationSubTask>();

			if (operationSubTasks != null)
			{
				this.OperationSubTasks.AddRange(operationSubTasks);
			}

			ErrorsCounts.ResetErrorCountValue(this.TimerGuid);
		}

		#endregion

		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti
		public List<OperationSubTask> OperationSubTasks { get; set; } 

		#region SubTasksSuccessity
		public OperationResultSeverityEnum SubTasksSuccessity
		{
			get
			{
				if (!base.GetWasSuccessfull())
					return OperationResultSeverityEnum.UnSuccessful;


				if (this.OperationSubTasks == null || this.OperationSubTasks.Count == 0)
				{
					return OperationResultSeverityEnum.Successful;
				}

				var successTasksCount = (from subtask in this.OperationSubTasks
						  where subtask.WasSuccessful
						  select subtask).Count();


				if (successTasksCount == this.OperationSubTasks.Count)
				{ return OperationResultSeverityEnum.Successful; }

				if (successTasksCount == 0)
				{ return OperationResultSeverityEnum.UnSuccessful; }

				return OperationResultSeverityEnum.PartialySuccessful;
			}
		} 
		#endregion
		
		#region SuccessfulSubtasks
		public IEnumerable<OperationSubTask> SuccessfulSubtasks
		{
			get
			{
				return (from subtask in this.OperationSubTasks
					   where subtask.WasSuccessful
					   select subtask).ToArray();
			}
		} 
		#endregion

		#region UnSuccessfulSubtasks
		public IEnumerable<OperationSubTask> UnSuccessfulSubtasks
		{
			get
			{
				return (from subtask in this.OperationSubTasks
					   where !subtask.WasSuccessful
					   select subtask).ToArray();
			}
		} 
		#endregion

		
		
		
		// Private Methods - Privátní metody

		#region CheckErrorsCount(string message)
		private int CheckErrorsCount(string message)
		{
			int ret = ErrorsCounts.GetErrorCountValue(this.TimerGuid);
			if (ret > 0)
			{
				throw new FatalFailureException(message, this);
			}
			return ret;
		} 
		#endregion

		// Protected Methods - Protected metody

		#region CountSubtaskError(OperationSubTask subtaskToCheck)
		protected int CountSubtaskError(OperationSubTask subtaskToCheck)
		{
			if (subtaskToCheck.WasSuccessful)
			{
				ErrorsCounts.ResetErrorCountValue(this.TimerGuid);
				return 0;
			}

			return ErrorsCounts.IncrementErrorCount(this.TimerGuid);
		}

		#endregion

		#region GetWasSuccessfull()
		protected override bool GetWasSuccessfull()
		{
			if ( !base.GetWasSuccessfull())
				return false;

			// TODO: solve wasSuccessful issue

			//if (!base.GetWasSuccessfull())
			//    return false;

			return this.SubTasksSuccessity == OperationResultSeverityEnum.Successful;
		} 
		#endregion

		// Public Methods - Public metody

		#region AddSubtask(OperationSubTask subtaskToAdd)
		public int AddSubtask(OperationSubTask subtaskToAdd)
		{
			int continuousErrorsCount = this.CountSubtaskError(subtaskToAdd);
			this.OperationSubTasks.Add(subtaskToAdd);

			return continuousErrorsCount;
		} 
		#endregion

		#region CreateSubTask(XDocument data)
		public OperationSubTask CreateSubTask(XDocument data)
		{
			return new OperationSubTask(data);
		}
		#endregion

		#region ConvertToOperationResult()
		public OperationResult ConvertToOperationResult()
		{
			return new OperationResult(
				this.CrmOrganizationUrl,
				this.TaskName,
				this.TimerGuid,
				this.SourcePluginGuid,
				this.SourcePluginAssemblyName,
				this.Name,
				this.Description,
				this.bmw_log_bmw_operationtype,
				this.Message,
				this.WasSuccessfull,
				this.StartTime,
				this.EndTime	,
				this.InterfaceRun
				);
		} 
		#endregion

		

		// Event Handlers - Události
	}
}
