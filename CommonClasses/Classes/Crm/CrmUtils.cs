using System;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
	public static class CrmUtils
	{
		#region GetOrganizationNameFromOrganizationServiceUrl(string serviceUrl)
		public static string GetOrganizationNameFromOrganizationServiceUrl(string serviceUrl)
		{
			string groupName = "organizationName";

			Regex regex = new Regex(@"^https?://[a-z\-_0-9]+(\.[a-zA-Z_\-0-9]+)*(:[0-9]{1,9})?/(?<" + groupName + ">[^/]*)/.*");
			var match = regex.Match(serviceUrl);
			return match.Groups[groupName].Value;
		}
		#endregion

		#region GetFormattedAttributeValue(this Entity entity, string attributeName)
		public static string GetFormattedAttributeValue(this Entity entity, string attributeName)
		{
			if (entity == null)
			{
				return null;
			}

			if (String.IsNullOrWhiteSpace(attributeName))
			{
				return null;
			}

			if (!entity.FormattedValues.Contains(attributeName))
			{
				return null;
			}

			return entity.FormattedValues[attributeName];
		} 
		#endregion

		#region ResolveOptionSet<T>(this OptionSetValue optionSetValue)
		public static string ResolveOptionSet<T>(this OptionSetValue optionSetValue)
		{
			return optionSetValue.ResolveOptionSet<T>(String.Empty);
		} 
		#endregion

		#region ResolveOptionSet<T>(this OptionSetValue optionSetValue, string defaultValue)
		public static string ResolveOptionSet<T>(this OptionSetValue optionSetValue, string defaultValue)
		{
			if (optionSetValue == null)
				return defaultValue;

			try
			{
				return Enum.ToObject(typeof(T), optionSetValue.Value).ToString();
}
			catch
			{
				return defaultValue;
			}            
		}
		#endregion
	}
}
