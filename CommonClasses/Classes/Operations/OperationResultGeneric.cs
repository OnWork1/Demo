using System;
using Microsoft.Xrm.Sdk;
using BMW.IntegrationService.CommonClassesAndEnums.Enums;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations
{
	public class OperationResult<T> 
		: OperationResult
	{
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Constructors - Konstruktory

		#region Constructors
		public OperationResult(
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
			true,
			DateTime.Now,
			null,
			default(T),interfaceRun) { }

		public OperationResult(
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
			true,
			DateTime.Now,
			null,
			default(T),interfaceRun) { }

		public OperationResult(
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
			true,
			DateTime.Now,
			null,
			default(T),interfaceRun) { }

		public OperationResult(
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
			true,
			DateTime.Now,
			null,
			default(T),interfaceRun) { }

		public OperationResult(
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
			true,
			DateTime.Now,
			null,
			default(T),interfaceRun) { }

		public OperationResult(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull,
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
			wasSuccessfull,
			DateTime.Now,
			null,
			default(T),interfaceRun) { }


		public OperationResult(
			string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull,
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
			wasSuccessfull,
			operationStartTime,
			null,
			default(T),interfaceRun) { }


		public OperationResult(string crmOrganizationUrl,
			string taskName, Guid timerGuid, Guid sourcePluginGuid,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull,
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
			wasSuccessfull,
			operationStartTime,
			operationEndTime,
			default(T),interfaceRun) { }


		public OperationResult(string crmOrganizationUrl,
			string taskName,
			Guid timerGuid,
			String operationName,
			String operationDescription,
			Guid sourcePluginGuid,
			String sourceModuleAssemblyName,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype,
			String message,
			bool wasSuccessfull,
			DateTime operationStartTime,
			DateTime? operationEndTime,
			T returnValue,
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
			wasSuccessfull,
			operationStartTime,
			operationEndTime,
			interfaceRun
			)
			{
				this.ReturnValue = returnValue;
			}
		#endregion


		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti
		public T ReturnValue { get; set; } 
				
		// Protected Methods - Protected metody

		// Public Methods - Public metody

		// Event Handlers - Události
	}
}
