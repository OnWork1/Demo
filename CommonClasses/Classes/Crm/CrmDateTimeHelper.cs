using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Extensions;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
	public class CrmDateTimeHelper
	{
		private readonly Dictionary<Guid, int?> keys;
		private readonly IOrganizationService organizationService;
		private readonly Dictionary<int, string> timeZones;
		private readonly Guid primaryUserId;

		// Constructors
		#region CrmDateTimeHelper(IOrganizationService organizationService)
		public CrmDateTimeHelper(IOrganizationService crmService)
		{
			if(crmService == null)
				throw new ArgumentNullException(nameof(crmService));

			this.keys = new Dictionary<Guid, int?>();
			this.timeZones = new Dictionary<int, string>();
			this.organizationService = crmService;
		} 
		#endregion

		// Private methods
		#region RetrieveUserSettings(Guid userId)
		private int? RetrieveUserSettings(Guid userId)
		{
			try
			{
				EntityCollection currentUserSettings = this.organizationService.RetrieveMultiple(
					new QueryExpression(UserSettings.EntityLogicalName)
						{
							ColumnSet = new ColumnSet("timezonecode"),
							Criteria = new FilterExpression
							           	{
							           		Conditions =
							           			{
							           				new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
							           			}
							           	}
						});

				if (currentUserSettings == null || currentUserSettings.Entities == null || currentUserSettings.Entities.Count <= 0)
				{
					return null;
				}

				Entity resultEntity = currentUserSettings.Entities.FirstOrDefault();

				return resultEntity != null ? resultEntity.ToEntity<UserSettings>().TimeZoneCode : null;

			}
			catch (Exception)
			{
				return null;
			}			
		} 
		#endregion

		#region RetrieveTimezoneName(int timeZone)
		private string RetrieveTimezoneName(int timeZone)
		{
			if (this.timeZones.ContainsKey(timeZone))
			{
				return this.timeZones.FirstOrDefault(p => p.Key == timeZone).Value;
			}

			QueryByAttribute query = new QueryByAttribute(TimeZoneDefinition.EntityLogicalName) { ColumnSet = new ColumnSet("standardname") };

			query.AddAttributeValue("timezonecode", timeZone);

			query.PageInfo = new PagingInfo { Count = 1, PageNumber = 1};

			// In addition to the RetrieveMultipleRequest message,
			// you may use the IOrganizationService.RetrieveMultiple method.

			string timeZoneName = String.Empty;

			try
			{
				EntityCollection result = this.organizationService.RetrieveMultiple(query);


				if (result.Entities != null && result.Entities.Count > 0)
				{
					Entity firstOrDefault = result.Entities.FirstOrDefault();
					if (firstOrDefault != null)
					{
						timeZoneName = firstOrDefault.ToEntity<TimeZoneDefinition>().StandardName;
					}
				}

			}
			catch (Exception)
			{
				return timeZoneName;
			}

			this.timeZones.Add(timeZone, timeZoneName);
			return timeZoneName;
		} 
		#endregion

		#region GetTimeZoneForUser(Guid userId)
		private int? GetTimeZoneForUser(Guid userId)
		{
			if (this.keys.ContainsKey(userId))
				return this.keys.FirstOrDefault(p => p.Key == userId).Value;

			int? timeZoneCode = this.RetrieveUserSettings(userId);

			this.keys.Add(userId, timeZoneCode);

			return timeZoneCode;
		} 
		#endregion

		#region ProcessCrmDateConversion(int timeZoneCode, DateTime dateTime)
		private DateTime ProcessCrmDateConversion(int timeZoneCode, DateTime dateTime)
		{
			LocalTimeFromUtcTimeRequest convert = new LocalTimeFromUtcTimeRequest
			{
				UtcTime = dateTime,
				TimeZoneCode = timeZoneCode // Timezone of user
			};

			LocalTimeFromUtcTimeResponse response = (LocalTimeFromUtcTimeResponse)this.organizationService.Execute(convert);

			return response.LocalTime;
		}
		#endregion

		#region GetActualUserId()
		private Guid GetActualUserId()
		{
			WhoAmIResponse response = (WhoAmIResponse)this.organizationService.Execute(new WhoAmIRequest());

			return response.UserId;
		} 
		#endregion

		// Public methods
		#region ResolveDateTime(DateTime? dateTime, string timeFormat)
		public string ResolveDateTime(DateTime? dateTime, string timeFormat)
		{
			return this.ResolveDateTime(this.GetActualUserId(), dateTime, timeFormat);
		} 
		#endregion

		#region ResolveDateTime(Guid userId, DateTime? dateTime, string timeFormat)
		public string ResolveDateTime(Guid userId, DateTime? dateTime, string timeFormat)
		{
			DateTime? resolvedDateTime = this.ResolveDateTime(userId, dateTime);

			if (!resolvedDateTime.HasValue)
			{
				return String.Empty;
			}

			return resolvedDateTime.Value.ToString(timeFormat);
		}
		#endregion



		#region ResolveDateTime(DateTime? dateTime)
		public DateTime? ResolveDateTime(DateTime? dateTime)
		{
			return this.ResolveDateTime(this.GetActualUserId(), dateTime);
		} 
		#endregion

		#region ResolveDateTime(Guid userId, DateTime? dateTime)
		public DateTime? ResolveDateTime(Guid userId, DateTime? dateTime)
		{
			DateTime? result;
			if (!dateTime.HasValue)
			{
				return null;
			}

			int? timeZoneCode = this.GetTimeZoneForUser(userId);

			if (!timeZoneCode.HasValue)
			{
				return dateTime.Value.ToLocalTime();
			}

			string timeZoneName = this.RetrieveTimezoneName(timeZoneCode.Value);

			try
			{
				result = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime.Value, timeZoneName);

			}
			catch (TimeZoneNotFoundException)
			{
				try
				{
					DateTime resultDate = this.ProcessCrmDateConversion(timeZoneCode.Value, dateTime.Value);
					result = resultDate;

				}
				catch (Exception)
				{
					result = dateTime.Value.ToLocalTime();
				}
			}
			catch (Exception)
			{
				result = dateTime.Value.ToLocalTime();
			}
			return result;
		}
        #endregion

        #region GetStartOfTheWeek(DateTime date)
        /// <summary>
        /// Get start date of week for date specified in <see cref="date"/>.
        /// </summary>
        /// <remarks>Time component is ignored.</remarks>
        /// <param name="date">Date for whic start of the week should be resolved.</param>
        /// <returns><see cref="DateTime"/> in which week started.</returns>
        public static DateTime GetStartOfTheWeek(DateTime date)
        {
            DateTime result = date.Date;
            while (result.DayOfWeek != DayOfWeek.Monday)
            {
                result = result.AddDays(-1);
            }
            return result.Date;
        }
        #endregion
    }
}
