using System;
using System.Data;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Strings;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Extensions
{
	public static class Extensions
	{

		#region GetStringValue(this Money source)
		public static string GetStringValue(this Money source)
		{
			return source.GetStringValue(String.Empty);
		} 
		#endregion

		#region GetStringValue(this Money source, string format)
		public static string GetStringValue(this Money source, string format)
		{
			return source == null ? String.Empty : source.Value.ToString(format);
		} 
		#endregion

		#region GetStringValue(this DateTime? source, string format)		
		public static string GetStringValue(this DateTime? source, string format)
		{		
			return !source.HasValue ? String.Empty : source.Value.ToString(format);
		}
		#endregion

		#region GetStringValue(this int? source)
		public static string GetStringValue(this int? source)
		{
			return source.GetStringValue(String.Empty);
		} 
		#endregion

		#region GetStringValue(this int? source, string format)
		public static string GetStringValue(this int? source, string format)
		{
			return !source.HasValue ? String.Empty : source.Value.ToString(format);
		}
		#endregion

		#region GetStringValue(this int? source, string format)
		public static string GetStringValue(this double? source, string format)
		{
			return !source.HasValue ? String.Empty : source.Value.ToString(format);
		}
		#endregion

		#region HasColumn(this IDataRecord dr, string columnName)
		public static bool HasColumn(this IDataRecord dr, string columnName)
		{
			for (int i = 0; i < dr.FieldCount; i++)
			{
				if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}
			return false;
		} 
		#endregion

        #region GetSalutation(this IOrganizationService service, string salutationName)
        public static EntityReference GetSalutation(this IOrganizationService service, string salutationName)
        {
            if (String.IsNullOrEmpty(salutationName))
            {
                return null;
            }

            QueryByAttribute query = new QueryByAttribute(bmw_salutation.EntityLogicalName)
            {
                PageInfo = new PagingInfo { PageNumber = 1, Count = 1 },
                ColumnSet = new ColumnSet()
            };
            query.AddAttributeValue(CrmMetadataHelper.GetAttributeLogicalName<bmw_salutation, string>(sal => sal.bmw_name), salutationName);
            EntityCollection queryResult = service.RetrieveMultiple(query);
            if (queryResult == null || queryResult.Entities.Count == 0)
                return null;

            return new EntityReference(bmw_salutation.EntityLogicalName, queryResult.Entities.First().Id);
        }
        #endregion

        #region GetSalutation(this IOrganizationService service, string salutationName)

        #endregion
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

        #region LogDebugToCrm(this IOrganizationService organizationService, string name, string message)
        public static void LogDebugToCrm(this IOrganizationService organizationService, string name, string message)
        {
            organizationService.LogToCrm(bmw_log_bmw_reasonstate.NotConfirmed, bmw_log_bmw_logrecordtype.DEBUG, bmw_log_bmw_severity.Low, String.Empty, name, message);
        }
        #endregion

        #region LogToCrm(this IOrganizationService organizationService, bmw_log_bmw_ReasonState reasonState, bmw_log_bmw_LogRecordType? logRecordType, bmw_log_bmw_Severity severity, string entityName, string name, string message, string exceptionText = null, Guid? entityId = null, Guid? interfaceRunId = null)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizationService"></param>
        /// <param name="reasonState"></param>
        /// <param name="logRecordType"></param>
        /// <param name="severity"></param>
        /// <param name="entityName"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <param name="exceptionText"> </param>
        /// <param name="entityId"> </param>
        /// <param name="interfaceRunId"></param>
        public static void LogToCrm(this IOrganizationService organizationService, bmw_log_bmw_reasonstate reasonState, bmw_log_bmw_logrecordtype? logRecordType, bmw_log_bmw_severity severity, string entityName, string name, string message, string exceptionText = null, Guid? entityId = null, Guid? interfaceRunId = null)
        {
            bmw_log log = new bmw_log
            {
                bmw_reasonstate =
                    new OptionSetValue((int)reasonState),
                bmw_entity = entityName,
                bmw_name = name,
                bmw_reasontext = message.CutEnd(2000),
                bmw_severity = new OptionSetValue((int)severity)
            };

            if (entityId.HasValue)
            {
                log.bmw_guid = entityId.Value.ToString();
            }

            if (!String.IsNullOrEmpty(exceptionText))
            {
                log.bmw_exception = exceptionText.CutEnd(2000);
            }

            if (logRecordType.HasValue)
            {
                log.bmw_logrecordtype =
                    new OptionSetValue(
                        (int)logRecordType.Value);
            }

            if (interfaceRunId.HasValue)
            {
                log.bmw_interfacerunid = new EntityReference(bmw_interfacerun.EntityLogicalName, interfaceRunId.Value);
            }

            //	organizationService.Create(log);
            organizationService.Create(log.ToEntity<Entity>());
        }
        #endregion

        #region ToBool(this bool? nullable)
        public static bool ToBool(this bool? nullable)
        {
            return nullable.ToBool(false);
        }
        #endregion

        #region ToBool(this bool? nullable, bool nullValue)
        public static bool ToBool(this bool? nullable, bool nullValue)
        {
            return nullable.HasValue ? nullable.Value : nullValue;
        }
        #endregion
    }
}
