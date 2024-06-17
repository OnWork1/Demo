using System;
using Microsoft.Xrm.Sdk;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Errors;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Logging;
using BMW.IntegrationService.CommonClassesAndEnums.Enums;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations
{
	public class OperationResult
	{
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní proměné
		private bool wasSuccessfull;
		

		// Constructors - Konstruktory
		#region OperationResult(...)
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerId,
			String.Empty,
			String.Empty,
			sourcePluginId,
			sourcePluginAssemblyName,
			null,
			String.Empty,
			true,
			DateTime.Now,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName,
			bmw_interfacerun interfaceRun)
			: this(crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			String.Empty,
			sourcePluginId,
			sourcePluginAssemblyName,
			null,
			String.Empty,
			true,
			DateTime.Now,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, String operationDescription, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_interfacerun interfaceRun)
			: this(crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			operationDescription,
			sourcePluginId,
			sourcePluginAssemblyName,
			null,
			String.Empty,
			true,
			DateTime.Now,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, String operationDescription, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype,
			bmw_interfacerun interfaceRun)
			: this(crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			operationDescription,
			sourcePluginId,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			String.Empty,
			true,
			DateTime.Now,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, String operationDescription, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message,
			bmw_interfacerun interfaceRun)
			: this(crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			operationDescription,
			sourcePluginId,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			true,
			DateTime.Now,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, String operationDescription, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			operationDescription,
			sourcePluginId,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			wasSuccessfull,
			DateTime.Now,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, String operationDescription, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull, DateTime operationStartTime, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull,
			DateTime operationStartTime,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			operationDescription,
			sourcePluginId,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			wasSuccessfull,
			operationStartTime,
			null, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl,  string taskName, Guid timerId, Guid sourcePluginId, String sourcePluginAssemblyName, string operationName, String operationDescription, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull, DateTime operationStartTime, DateTime operationEndTime, bmw_interfacerun interfaceRun)
		public OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, Guid sourcePluginId,
			String sourcePluginAssemblyName, string operationName, String operationDescription,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull,
			DateTime operationStartTime, DateTime operationEndTime,
			bmw_interfacerun interfaceRun)
			: this(
			crmOrganizationUrl,
			taskName,
			timerId,
			operationName,
			operationDescription,
			sourcePluginId,
			sourcePluginAssemblyName,
			bmw_log_bmw_operationtype,
			message,
			wasSuccessfull,
			operationStartTime,
			operationEndTime, interfaceRun) { }
		#endregion
		#region OperationResult(string crmOrganizationUrl, string taskName, Guid timerId, String operationName, String operationDescription, Guid sourcePluginId, String sourcePluginAssemblyName, bmw_log_bmw_operationtype? bmw_log_bmw_operationtype, String message, bool wasSuccessfull, DateTime operationStartTime, DateTime? operationEndTime, bmw_interfacerun interfaceRun)
		protected OperationResult(string crmOrganizationUrl,
			string taskName,
			Guid timerId,
			String operationName,
			String operationDescription,
			Guid sourcePluginId,
			String sourcePluginAssemblyName,
			bmw_log_bmw_operationtype? bmw_log_bmw_operationtype,
			String message,
			bool wasSuccessfull,
			DateTime operationStartTime,
			DateTime? operationEndTime,
			bmw_interfacerun interfaceRun)
		{
			if (operationEndTime.HasValue && operationEndTime.Value.Ticks < operationStartTime.Ticks)
			{ throw new ApplicationException("Operation EndTime must be equal or higher than StartTime"); }
			this.CrmOrganizationUrl = crmOrganizationUrl;
			this.TaskName = taskName;
			this.TimerGuid = timerId;
			if (!String.IsNullOrEmpty(operationName))
				this.Name = operationName.StartsWith(".") ? operationName : "." + operationName;
			else
				this.Name = "";
			this.Description = operationDescription;
			this.SourcePluginGuid = sourcePluginId;
			this.SourcePluginAssemblyName = sourcePluginAssemblyName;
			this.bmw_log_bmw_operationtype = bmw_log_bmw_operationtype;
			this.Message = message;
			this.WasSuccessfull = wasSuccessfull;
			this.StartTime = operationStartTime;
			this.EndTime = operationEndTime.HasValue ? operationEndTime.Value : this.StartTime;

			//this.Logger = Logger.GetLoggerWithCrmLogging(this.Name, this.CrmOrganizationUrl);
			this.ExceptionDump = null;
			this.InterfaceRun = interfaceRun;
		}
		#endregion
		#endregion

		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti

		public bool Skip { get; private set; }
		public Logger Logger { get; set; }
		public string CrmOrganizationUrl { get; set; }
		public string Message { get; set; }
		public string Name { get; set; }
		public bmw_interfacerun InterfaceRun { get; set; }
		public string Description { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string SourcePluginAssemblyName { get; set; }
		public Guid SourcePluginGuid { get; set; }
		public Guid TimerGuid { get; set; }
		public string TaskName { get; set; }
		public bmw_log_bmw_operationtype? bmw_log_bmw_operationtype { get; set; }
		
		// Internal properties
		
		internal string ExceptionDump { get; set; }

		#region WasSuccessfull
		public bool WasSuccessfull
		{
			get { return this.wasSuccessfull; }
			protected set
			{
				this.EndTime = DateTime.Now;
				this.wasSuccessfull = value;

				ErrorsCounts.CountThisState(this.TimerGuid, value);
			}
		}
		#endregion

		// Public Methods - Public metody

		#region SetSkip(string message)
		public void SetSkip(string message)
		{
			this.Debug(message, bmw_log_bmw_reasonstate.NotConfirmed);

			this.Skip = true;
		}
		#endregion

		#region ResetWasSuccesfull()
		/// <summary>
		/// resets the value of field wasSuccessfull to true
		/// it does NOT necesserily mean that the value of WasSuccessfull property in inherited classes will be true! 
		/// To achieve that, overload this method in inherited class.
		/// </summary>
		public virtual void ResetWasSuccessfull()
		{
			this.Info("Resetting wasSuccessfull value to true", bmw_log_bmw_reasonstate.NotConfirmed);
			this.wasSuccessfull = true;
		}
		#endregion

		#region Debug(...)

		#region Debug(string message, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Debug(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Debug(message, null, crmReasonState, entity);
		}
		#endregion

		#endregion
		#region Debug(Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Debug(Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Debug("no message to log", ex, crmReasonState, entity);
		}
		#endregion

		#region Debug(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Debug(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity)
		{
			this.ExceptionDump = null;
			if (ex != null)
			{
				this.ExceptionDump = "exception information:" + Environment.NewLine + this.DumpException(ex);
			}
			this.Message = message;

			//this.Logger.Debug(this, crmReasonState, entity, ex);
		}
		#endregion


		#region Error(...)

		#region Error(string message, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual int Error(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			return
				   this.Error(message, null, crmReasonState, entity);
		}
		#endregion

		#region Error(Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual int Error(Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			return this.Error("no message to log", ex, crmReasonState, entity);
		}
		#endregion

		#region Error(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)

		/// <summary>
		/// sets WasSuccessfull to false, and EndtTime to DateTime.Now and set Message
		/// </summary>
		/// <param name="message">text to be set to Message</param>
		/// <param name="ex">Exception taht caused error.</param>
		/// <param name="crmReasonState">Reason state.</param>
		/// <param name="entity">Associated entity.</param>
		/// <returns>Return number of errors.</returns>
		public virtual int Error(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.WasSuccessfull = false;
			this.ExceptionDump = null;
			if (ex != null)
			{
				this.ExceptionDump = "exception information:" + Environment.NewLine + this.DumpException(ex);
			}
			this.Message = message;

			//this.Logger.Error(this, crmReasonState, entity, ex);
			return this.CheckErrorsCount(message);
		}

		#endregion
		#endregion

		#region Info(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)

		#region Info(string message, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Info(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Info(message, null, crmReasonState, entity);
		}
		#endregion

		#region Info(Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Info(Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Info("no message to log", ex, crmReasonState, entity);
		}
		#endregion

		#region Info(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Info(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.ExceptionDump = null;
			if (ex != null)
			{
				this.ExceptionDump = "exception information:" + Environment.NewLine + this.DumpException(ex);
			}
			this.Message = message;

			////this.Logger.Info(this, crmReasonState, entity, ex);
		}
		#endregion
		#endregion

		#region Warn(...)

		#region Warn(string message, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Warn(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Warn(message, null, crmReasonState, entity);
		}
		#endregion

		#region Warn(Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Warn(Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Warn("no message to log", ex, crmReasonState, entity);
		}
		#endregion

		#region Warn(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, bmw_logbmw_Severity crmSeverity, Entity entity)
		public virtual void Warn(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.ExceptionDump = null;
			if (ex != null)
			{
				this.ExceptionDump = "exception information:" + Environment.NewLine + this.DumpException(ex);
			}
			this.Message = message;

			//this.Logger.Warn(this, crmReasonState, entity, ex);
		}
		#endregion
		#endregion

		#region Fatal(...)
		#region Fatal(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		public virtual void Fatal(string message, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Fatal(message, null, crmReasonState, entity);
		}
		#endregion
		#region Fatal(Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		public virtual void Fatal(Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.Fatal("no message to log", ex, crmReasonState, entity);
		}
		#endregion
		#region Fatal(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		public virtual void Fatal(string message, Exception ex, bmw_log_bmw_reasonstate crmReasonState, Entity entity = null)
		{
			this.WasSuccessfull = false;
			this.ExceptionDump = null;
			if (ex != null)
			{
				this.ExceptionDump = "exception information:" + Environment.NewLine + this.DumpException(ex);
			}
			this.Message = message;

			//this.Logger.Fatal(this, crmReasonState, ex, entity);

		}
		#endregion
		#endregion

		// Protected Methods - Protected metody

		#region GetWasSuccessfull()
		protected virtual bool GetWasSuccessfull()
		{
			return this.wasSuccessfull;
		}
		#endregion

		// Private Methods - Privátní metody
		#region DumpException(Exception ex)
		private string DumpException(Exception ex)
		{
			string message = "exception type: " + ex.GetType().Name + Environment.NewLine +
							 "message: " + ex.Message + Environment.NewLine +
							 "stack trace:" + Environment.NewLine +
							 ex.StackTrace;
			if (ex.InnerException != null)
			{
				message += Environment.NewLine + "Inner exception:" + Environment.NewLine +
						   DumpException(ex.InnerException);
			}
			return message;
		}
		#endregion

		#region CheckErrorsCount(string message)
		private int CheckErrorsCount(string message)
		{
			// TODO:Domyslet
			int ret = ErrorsCounts.GetErrorCountValue(this.TimerGuid);
			return ret;
		}
		#endregion

		// Event Handlers - Události
	}


}