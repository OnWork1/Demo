using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xrm.Sdk;
//using Webcom.IntegrationService.CommonClassesAndEnums.Classes.Logging;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
	public class CrmParametersManager
	{
		// Constants

		// Delegates

		// Events

		// Private Fields
		private static ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
		private static CrmParametersManager CurrentInstance = null;
		private static readonly object padlock = new object();
		private Dictionary<string, bool> parametersForThisUrlAreActual;
		private const int timeOut = 3600000;

		private readonly Dictionary<string, List<bmw_parameter>> organizationParameters; // string key is organization crmUrl, List<bmw_parameter> are all parameters for one organization
		Thread thread = null;

		// Constructors

		#region CrmParametersManager()
		private CrmParametersManager()
		{
			this.organizationParameters = new Dictionary<string, List<bmw_parameter>>();
			this.parametersForThisUrlAreActual = new Dictionary<string, bool>();
		} 
		#endregion

		// Private Properties

		// Protected Properties

		// Public static Properties
		
		#region Instance
		public static CrmParametersManager Instance
		{
			get
			{
				if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
				readerWriterLock.EnterUpgradeableReadLock();

				if (CrmParametersManager.CurrentInstance == null)
				{
					if (!readerWriterLock.IsWriteLockHeld)
						readerWriterLock.EnterWriteLock();
					try
					{
						if (CrmParametersManager.CurrentInstance == null)
						{
							CrmParametersManager.CurrentInstance = new CrmParametersManager();
						}
					}
					finally
					{
						if (readerWriterLock.IsWriteLockHeld)
						readerWriterLock.ExitWriteLock();
					}
				}
				if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
				return CrmParametersManager.CurrentInstance;
			}
		} 
		#endregion

		// Public Methods

		#region RefreshParameters(string crmUrl)
		public void RefreshParameters(string crmUrl)
		{
			if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
				readerWriterLock.EnterUpgradeableReadLock();

			if (this.parametersForThisUrlAreActual.ContainsKey(crmUrl))
			{
				if (!readerWriterLock.IsWriteLockHeld)
					readerWriterLock.EnterWriteLock();
				try
				{
					this.parametersForThisUrlAreActual[crmUrl] = false;
				}
				finally
				{
					if (readerWriterLock.IsWriteLockHeld)
						readerWriterLock.ExitWriteLock();
				}
			}
			if (readerWriterLock.IsUpgradeableReadLockHeld)
				readerWriterLock.ExitUpgradeableReadLock();
		}
		#endregion

		#region GetParameterValue<T>(string crmOrganizationServiceUrl, string parameterName, bmw_parameterbmw_Category? bmw_category)
		public T GetParameterValue<T>(string crmOrganizationServiceUrl, string parameterName, bmw_parameter_bmw_category? bmwCategory,
			bmw_interfacerun interfaceRun)
		{
			if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
				readerWriterLock.EnterUpgradeableReadLock();

			if (!this.ParametersForThisUrlAreActual(crmOrganizationServiceUrl))
			{
				if (!this.ParametersForThisUrlAreActual(crmOrganizationServiceUrl))
				{
					if (!readerWriterLock.IsWriteLockHeld)
						readerWriterLock.EnterWriteLock();
					try
					{
						this.CacheCrmParameters(crmOrganizationServiceUrl);
					}
					finally
					{
						if (readerWriterLock.IsWriteLockHeld)
							readerWriterLock.ExitWriteLock();
					}
				}
			}
			try
			{
				string message;
				ApplicationException ex;

				List<bmw_parameter> parameters = this.organizationParameters[crmOrganizationServiceUrl];
				if (parameters == null)
				{
					message = "CrmParameterManager: GetParameterValue<T>(), unable to load bmw_parameters for url " +
							  crmOrganizationServiceUrl;
					ex = new ApplicationException(message);

					if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
					throw ex;
				}

				bmw_parameter parameter = (
							from param in parameters
							where
								(!bmwCategory.HasValue ||
								 (param.bmw_category != null && param.bmw_category.Value == (int)bmwCategory.Value)) &&
								param.bmw_name == parameterName
							select param
					).FirstOrDefault();

				if (parameter == null)
				{
					message = "CrmParameterManager: GetParameterValue<T>(), unable to load bmw_parameter for bmw_category " +
							  bmwCategory.ToString() + ", ParameterName: " +
								 parameterName;
					ex = new ApplicationException(message);					
					if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
					throw ex;
				}


				Type type = typeof(T);

				if (type == typeof(string))
				{
					if (readerWriterLock.IsUpgradeableReadLockHeld)
						readerWriterLock.ExitUpgradeableReadLock();
					return (T)Convert.ChangeType(parameter.bmw_textvalue, typeof(string));
				}
				if (type == typeof(DateTime?))
				{
					if (!parameter.bmw_datevalue.HasValue)
					{
						object o = null;
						if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
						return (T)o;
					}
					if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
					return (T)Convert.ChangeType(parameter.bmw_datevalue.Value, typeof(DateTime));
				}
				if (type == typeof(bool))
				{
					if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
					return (T)Convert.ChangeType(parameter.bmw_booleanvalue.Value == 1, typeof(bool));
				}
				if (type == typeof(decimal?))
				{
					if (!parameter.bmw_numbervalue.HasValue)
					{
						message = "CrmParameterManager: GetParameterValue<T>(), Parameter " + parameterName + " number value is not set";
						ex = new ApplicationException(message);
						if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
						throw ex;
					}
					if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
					return (T)Convert.ChangeType(parameter.bmw_numbervalue.Value, typeof(decimal));
				}

                if (type == typeof(double))
                {
                  
                    if (parameter.bmw_methodtype == null)
                    {
                        parameter.bmw_methodtype =new OptionSetValue(174640000);
                    }
                    if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
                        return (T)Convert.ChangeType(parameter.bmw_methodtype?.Value, typeof(double));

                }

                if (type == typeof(int))
                {
                 
                    if (parameter.bmw_end == null)
                    {
                        parameter.bmw_end =false;
                    }

                    if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
                        return (T)Convert.ChangeType(parameter.bmw_end.Value, typeof(int));

                }

                message = "CrmParameterManager: GetParameterValue<T>(), Not supported type " + type.Name;
				ex = new ApplicationException(message);
				if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
				throw ex;
			}
			finally
			{
				if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
				else if (readerWriterLock.IsReadLockHeld) readerWriterLock.ExitReadLock();
			}
		}
		#endregion

		#region ExistsParameter(string crmOrganizationServiceUrl, string parameterName, bmw_parameterbmw_Category? bmwCategory)
		public bool ExistsParameter(string crmOrganizationServiceUrl, string parameterName, bmw_parameter_bmw_category? bmwCategory,
			bmw_interfacerun interfaceRun)
		{

			if (String.IsNullOrWhiteSpace(crmOrganizationServiceUrl) || String.IsNullOrWhiteSpace(parameterName))
				return false;

			if (!readerWriterLock.IsUpgradeableReadLockHeld && !readerWriterLock.IsWriteLockHeld)
				readerWriterLock.EnterUpgradeableReadLock();

			if (!this.ParametersForThisUrlAreActual(crmOrganizationServiceUrl))
			{

				if (!readerWriterLock.IsWriteLockHeld)
					readerWriterLock.EnterWriteLock();
				try
				{
					this.CacheCrmParameters(crmOrganizationServiceUrl);
				}
				finally
				{
					if (readerWriterLock.IsWriteLockHeld)
						readerWriterLock.ExitWriteLock();
				}
			}
			try
			{
				List<bmw_parameter> parameters = this.organizationParameters[crmOrganizationServiceUrl];

				if (parameters == null)
				{
					string message = "CrmParameterManager: ExistsParameter() unable to load bmw_parameters for url " +
									 crmOrganizationServiceUrl;
					Exception ex = new ApplicationException(message);
					if (readerWriterLock.IsUpgradeableReadLockHeld) readerWriterLock.ExitUpgradeableReadLock();
					throw ex;
				}
				if (readerWriterLock.IsUpgradeableReadLockHeld)
					readerWriterLock.ExitUpgradeableReadLock();
				return bmwCategory.HasValue
						? parameters.Any(b => b.bmw_name == parameterName && b.bmw_category.Value == (int)bmwCategory.Value)
						: parameters.Any(b => b.bmw_name == parameterName);
			}
			finally
			{
				if (readerWriterLock.IsUpgradeableReadLockHeld)
					readerWriterLock.ExitUpgradeableReadLock();
			}

		}

		#endregion

		// Protected Methods


		// Private Methods

		
		#region ParametersForThisUrlAreActual(string url)
		private bool ParametersForThisUrlAreActual(string url)
		{
			if (this.parametersForThisUrlAreActual.ContainsKey(url) &&
						this.parametersForThisUrlAreActual[url])
		{
				return true;
			}
			return false;
		}
		#endregion

		
		#region CacheCrmParameters(string url)
		private void CacheCrmParameters(string url)
		{
			bool itIsNeededToReleaseWriterLock = false;
			if (!readerWriterLock.IsWriteLockHeld)
			{
				readerWriterLock.EnterWriteLock();
				itIsNeededToReleaseWriterLock = true;
			}
		} 
		#endregion

		#region AddParameterSet(string organizationCrmUrlWillBeUsedAsKey, List<bmw_parameter> bmw_parameters)
		private void AddParameterSet(string organizationCrmUrlWillBeUsedAsKey, CrmDateTimeHelper dateTimeHelper, List<bmw_parameter> bmwParameters)
		{
			if (this.organizationParameters.ContainsKey(organizationCrmUrlWillBeUsedAsKey))
				return;

			foreach (bmw_parameter bmwParameter in bmwParameters)
			{
				if(!bmwParameter.bmw_datevalue.HasValue)
					continue;

				string newDateTime = dateTimeHelper.ResolveDateTime(
					bmwParameter.ModifiedBy != null ? bmwParameter.ModifiedBy.Id : bmwParameter.CreatedBy.Id,
					bmwParameter.bmw_datevalue, String.Empty);

				if(String.IsNullOrWhiteSpace(newDateTime))
					continue;

				DateTime dateTime;

				if(!DateTime.TryParse(newDateTime, out dateTime))
				{
					continue;
				}

				bmwParameter.bmw_datevalue = dateTime;
			}

			this.organizationParameters.Add(organizationCrmUrlWillBeUsedAsKey, bmwParameters);
		}
		#endregion

		
		

		// Event Handlers
	}
}
