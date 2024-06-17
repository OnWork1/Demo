using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Errors;
using BMW.IntegrationService.CommonClassesAndEnums.Enums;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations
{
	public class OperationResultWithSubTasks<T> 
		: OperationResultWithSubTasks
	
	{
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní promìné

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
			null,interfaceRun) { }

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
			null,interfaceRun) { }

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
			null,interfaceRun) { }

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
			null,interfaceRun) { }


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
			null,
			interfaceRun) { }


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
			null,interfaceRun) { }


		public OperationResultWithSubTasks(
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
			List<OperationSubTask> operationSubTasks,
			bmw_interfacerun interfaceRun)
			: base(
			crmOrganizationUrl,
			taskName,
			timerGuid,
			operationName,
			operationDescription,
			sourcePluginGuid,
			sourceModuleAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			operationStartTime,
			operationEndTime,
			operationSubTasks,
			interfaceRun
			)
		{			
			
		}
        #endregion

        // Private Properties - Privátní vlastnosti

        // Protected Properties - Protected vlastnosti

        // Public Properties - Public vlastnosti

        // Private Methods - Privátní metody

        // Protected Methods - Protected metody

        public Dictionary<Guid, List<string>> InterfaceAssociatedNotes { get; set; }

        #region CountSubtaskError(OperationSubTask subtaskToCheck)
        protected new int CountSubtaskError(OperationSubTask subtaskToCheck)
		{
			if (subtaskToCheck.WasSuccessful)
			{
				ErrorsCounts.ResetErrorCountValue(this.TimerGuid);
				return 0;
			}

			return ErrorsCounts.IncrementErrorCount(this.TimerGuid);
		}
		#endregion

		// Public Methods - Public metody

		#region AddSubtask(OperationSubTask subtaskToAdd)
		public new int AddSubtask(OperationSubTask subtaskToAdd)
		{
			int continuousErrorsCount = this.CountSubtaskError(subtaskToAdd);
			this.OperationSubTasks.Add(subtaskToAdd);

			return continuousErrorsCount;
		} 
		#endregion

		#region CreateSubTask(XDocument data)
		public new OperationSubTask CreateSubTask(XDocument data)
		{
			return new OperationSubTask(data);
		} 
		#endregion

		#region ConvertToOperationResult<TT>()
		public OperationResult<TT> ConvertToOperationResult<TT>()
		{
			OperationResult<TT> ret = new OperationResult<TT>(
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
				this.EndTime,
				this.InterfaceRun
				);
			if (typeof(TT) == typeof(int))
			{
				var tmp = Convert.ChangeType(this.OperationSubTasks.Count, typeof(TT));
				ret.ReturnValue = (TT)tmp;
			}
			return ret;
		} 
		#endregion
		

		// Event Handlers - Události
	}
}
