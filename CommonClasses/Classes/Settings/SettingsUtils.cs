using System;
using System.Linq;
using System.Xml.Linq;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Settings
{
	public class SettingsUtils
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors
		
		// Private Properties

		// Protected Properties

		// Public Properties

		// Private static methods

		// Protected Methods

		// Public static Methods
		#region ResolveConfigValue<T>(string crmUrl, string settingAttributeValue, bmw_interfacerun interfaceRun)
		public static string ResolveConfigValue<T>(string crmUrl, string settingAttributeValue,
			bmw_interfacerun interfaceRun)
		{
			if (String.IsNullOrWhiteSpace(settingAttributeValue))
				return String.Empty;

			//TODO: Doresit vyhazovani vyjimek

			const string crmParameter = "crmparameter:";
			if (settingAttributeValue.ToLower().StartsWith(crmParameter))
			{
				T value = CrmParametersManager.Instance.GetParameterValue<T>(crmUrl,
																			 settingAttributeValue.Substring(crmParameter.Length),
																			 null, interfaceRun);
				return value == null ? String.Empty : value.ToString();
			}
			return settingAttributeValue;

		}
		#endregion

		#region GetComposableConfigValue<T>(string crmUrl, XElement xConfigElement, string attributeName, bmw_interfacerun interfaceRun)
		public static string GetComposableConfigValue<T>(string crmUrl, XElement xConfigElement, string attributeName,
			bmw_interfacerun interfaceRun)
		{
			string allValue = xConfigElement.GetAttributeValue(attributeName, String.Empty);

			if (!String.IsNullOrWhiteSpace(allValue))
			{
				return SettingsUtils.ResolveConfigValue<T>(crmUrl, allValue, interfaceRun);
			}

			if (!xConfigElement.HasElements)
			{
				return String.Empty;
			}

			XElement[] xParts = xConfigElement.Elements(attributeName + "Part").ToArray();

			return xParts.Length == 0 ? String.Empty : xParts.Aggregate(string.Empty, (current, xPart) => current + SettingsUtils.ResolveConfigValue<T>(crmUrl, xPart.Value, interfaceRun));
		} 
		#endregion

		// Event Handlers

	}
}
