using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml
{
	public static class XmlUtils
	{
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní proměné

		// Constructors - Konstruktory


		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti

		// Private Methods - Privátní metody

		// Protected Methods - Protected metody

		// Public static Methods - Public metody

		#region CreateElement(this XElement parent, string name)
		public static XElement CreateElement(this XElement parent, string name)
		{
			return parent.CreateElement(name, String.Empty);
		}
		#endregion

		#region CreateAttribute(this XElement parent, string name, bool value)
		public static XAttribute CreateAttribute(this XElement parent, string name, bool value)
		{
			return parent.CreateAttribute(name, value.ToString().ToLower());
		}
		#endregion

		#region CreateAttribute(this XElement parent, string name, string value)
		public static XAttribute CreateAttribute(this XElement parent, string name, string value)
		{
			if (parent == null)
				return null;

			XAttribute attribute = new XAttribute(name, value);
			parent.Add(attribute);
			return attribute;
		}
		#endregion

		#region CreateElement(this XElement parent, string name, string value)
		public static XElement CreateElement(this XElement parent, string name, string value)
		{
			return parent.CreateElement(name, value, Int32.MaxValue);
		}
		#endregion


		#region CreateElement(this XElement parent, string name, string value, int maxLenght)
		public static XElement CreateElement(this XElement parent, string name, string value, int maxLenght)
		{
			XElement element = new XElement(name);

			if (!String.IsNullOrWhiteSpace(value))
			{
				element.Value = value.Length > maxLenght ? value.Substring(0, maxLenght - 1) : value;
			}


			parent.Add(element);

			return element;
		}
		#endregion

		#region GetAttributeValue<T>(this XElement xElement, string attributeName, T defaultValue)
		public static T GetAttributeValue<T>(this XElement xElement, string attributeName, T defaultValue)
		{
			return xElement.GetAttributeValue(attributeName, defaultValue, false);
		}
		#endregion

		#region GetAttributeValue<T>(this XElement xElement, string attributeName, T defaultValue, bool returnTypeDefaultIfAttributeNotPresent)
		public static T GetAttributeValue<T>(this XElement xElement, string attributeName, T defaultValue, bool returnTypeDefaultIfAttributeNotPresent)
		{
			if (xElement == null)
			{
				return defaultValue;
			}
			if (String.IsNullOrWhiteSpace(attributeName))
			{
				return defaultValue;
			}

			XAttribute xAttribute = xElement.Attribute(attributeName);
			if (String.IsNullOrEmpty(xAttribute?.Value))
			{
				return returnTypeDefaultIfAttributeNotPresent ? default(T) : defaultValue;
			}

			try
			{
				if (typeof(T) == typeof(bool) || typeof(T) == typeof(Boolean))
				{
					string loweredValue = xAttribute.Value.ToLower();
					if (loweredValue == "true" || loweredValue == "y" || xAttribute.Value == "1") return (T)(Convert.ChangeType(true, typeof(T)));
					if (loweredValue == "false" || loweredValue == "n" || xAttribute.Value == "0") return (T)(Convert.ChangeType(false, typeof(T)));
				}

				if (typeof(T) == typeof(Guid))
				{
					return (T)(Convert.ChangeType(Guid.Parse(xAttribute.Value), typeof(T)));

				}
				if (typeof(T) == typeof(char))
				{
					char chr;
					// Parsing escape characters (i.e. '\t')  won't work using only Convert.ChangeType, must parse this way
					string val = Regex.Unescape(xAttribute.Value);
					if (Char.TryParse(val, out chr))
						return (T)(Convert.ChangeType(chr, typeof(T)));
				}

				return (T)(Convert.ChangeType(xAttribute.Value, typeof(T)));
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}
		#endregion

		#region AddAttribute(this XElement xElement, string attributeName, string attributeValue)
		public static void AddAttribute(this XElement xElement, string attributeName, string attributeValue)
		{
			if (attributeValue == null || String.IsNullOrEmpty(attributeName)) { return; }

			// if attributeName contains invalid chars, an exception will be thrown
			// TODO: Catch? or what
			XAttribute xAttribute = new XAttribute(attributeName, attributeValue);

			xElement.Add(xAttribute);
		}
		#endregion

		#region AttributeStringValue(XAttribute xAttribute, string defaultValueIfAttributeIsNull)
		public static string AttributeStringValue(XAttribute xAttribute, string defaultValueIfAttributeIsNull)
		{
			return xAttribute == null
					? defaultValueIfAttributeIsNull
					: xAttribute.Value;
		}
		#endregion

		#region PresentElement(XDocument xmlToCheck, String xPath)
		public static bool PresentElement(XDocument xmlToCheck, String xPath)
		{
			XElement xElement = xmlToCheck.XPathSelectElement(xPath);
			return xElement != null;
		}
		#endregion

		#region RequiredValue(XDocument xmlToCheck, String nodePath, string expectedValue)
		public static bool RequiredValue(XDocument xmlToCheck, String nodePath, string expectedValue)
		{
			XElement xElement = xmlToCheck.XPathSelectElement(nodePath);
			if (xElement != null && xElement.Value == expectedValue)
			{
				return true;
			}

			return false;
		}
		#endregion

		#region RequiredValues(XDocument xmlToCheck, String nodePath, string[] expectedValues)
		public static bool RequiredValues(XDocument xmlToCheck, String nodePath, string[] expectedValues)
		{
			XElement xElement = xmlToCheck.XPathSelectElement(nodePath);
			if (xElement == null)
			{
				return false;
			}

			/*
			foreach (string expectedValue in expectedValues)
			{
				if (xElement.Value == expectedValue)
				{
					return true;
				}
			}

			return false;
			
			// can be converted to Linq Expression:
			*/
			return expectedValues.Any(expectedValue => xElement.Value == expectedValue);
		}
		#endregion


		#region GetNodeValue<T>(this XElement parent, string path)
		public static T GetNodeValue<T>(this XElement parent, string path)
		{
			return parent.GetNodeValue(path, default(T), true);
		}
		#endregion

		#region GetNodeValue(this XElement parent, string path, string defaultValue)
		public static string GetNodeValue(this XElement parent, string path, string defaultValue)
		{
			return parent.GetNodeValue(path, defaultValue, Int32.MaxValue);
		}
		#endregion

		#region GetNodeValue(this XElement parent, string path, string defaultValue, int maxLength)
		public static string GetNodeValue(this XElement parent, string path, string defaultValue, int maxLength)
		{
			string val = parent.GetNodeValue<string>(path, defaultValue);

			if (String.IsNullOrWhiteSpace(val))
				return val;

			return val.Length > maxLength ? val.Substring(0, maxLength - 1) : val;
		}
		#endregion

		#region TransformNodeValueToBool(this XElement parent, string path, bool? defaultValue, string trueValue)
		public static bool? TransformNodeValueToBool(this XElement parent, string path, bool? defaultValue, string trueValue)
		{
			string strval = parent.GetNodeValue(path, null, trueValue == null ? 1 : trueValue.Length + 1);
			if (String.IsNullOrWhiteSpace(strval))
				return defaultValue;

			return strval.Equals(trueValue, StringComparison.OrdinalIgnoreCase);
		}
		#endregion

		#region GetNodeValue<T>(this XElement parent, string path, T defaultValue)
		public static T GetNodeValue<T>(this XElement parent, string path, T defaultValue)
		{
			return parent.GetNodeValue(path, defaultValue, false);
		}
		#endregion

		#region GetNodeValue(this XElement parent, string path, DateTime defaultValue)
		public static DateTime GetNodeValue(this XElement parent, string path, DateTime defaultValue)
		{
			return parent.GetNodeValue(path, defaultValue, false);
		}
		#endregion

		#region GetNodeValue(this XElement parent, string path)
		public static DateTime? GetNodeValue(this XElement parent, string path)
		{
			XElement node = parent.Element(path);

			if (node == null || String.IsNullOrEmpty(node.Value))
			{
				return null;
			}

			DateTime dateTime = parent.GetNodeValue(path, DateTime.MinValue);

			return dateTime > DateTime.MinValue ? dateTime : (DateTime?)null;
		}
		#endregion

		#region GetNodeValue(this XElement parent, string path, DateTime defaultValue)
		public static DateTime GetNodeValue(this XElement parent, string path, DateTime defaultValue, bool required)
		{
			XElement node = parent.Element(path);

			if (node == null)
			{

				if (required)
				{
					throw new Exception(String.Format("Node '{0}' does not exist.", path));
				}

				return defaultValue;
			}

			if (String.IsNullOrWhiteSpace(node.Value))
				return defaultValue;

			DateTime t;

			if (DateTime.TryParse(node.Value, out t))
				return t;

			double doubleValue;

			if (!Double.TryParse(node.Value, out doubleValue))
			{
				return defaultValue;
			}

			try
			{
				DateTime dateTime = DateTime.FromOADate(doubleValue);
				return dateTime > DateTime.MinValue && dateTime < DateTime.MaxValue ? dateTime : defaultValue;
			}
			catch (Exception)
			{
				return defaultValue;
			}


		}
		#endregion

		#region FindElement(this XElement parent, string name)
		public static XElement FindElement(this XElement parent, string name)
		{
			XElement element = parent.Descendants().FirstOrDefault(n => n.Name == name);
			return element;
		}
		#endregion

		#region FindElementAndGetValue<T>(this XElement parent, string name, string defaultValue)
		public static string FindElementAndGetValue(this XElement parent, string name, string defaultValue)
		{
			return parent.FindElementAndGetValue(name, defaultValue, Int32.MaxValue);
		}
		#endregion

		#region FindElementAndGetValue(this XElement parent, string name, string defaultValue, int maxLength)
		public static string FindElementAndGetValue(this XElement parent, string name, string defaultValue, int maxLength)
		{
			XElement element = parent.FindElement(name);

			if (element == null)
			{
				return defaultValue;
			}

			if (String.IsNullOrWhiteSpace(element.Value))
				return String.Empty;

			return element.Value.Length > maxLength ? element.Value.Substring(0, maxLength - 1) : element.Value;
		}
		#endregion

		#region FindElementAndGetValue<T>(this XElement parent, string name, T defaultValue)
		public static T FindElementAndGetValue<T>(this XElement parent, string name, T defaultValue)
		{
			XElement element = parent.FindElement(name);

			if (element == null)
			{
				return defaultValue;
			}

			try
			{
				Type type = typeof(T);

				return (T)Convert.ChangeType(element.Value, type.IsNullable() ? Nullable.GetUnderlyingType(type) : type, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}
		#endregion

		#region GetXPathNodeValue<T>(this XDocument source, string xpath, T defaultValue)
		public static T GetXPathNodeValue<T>(this XDocument source, string xpath, T defaultValue)
		{
			if (source == null)
				return defaultValue;

			if (String.IsNullOrWhiteSpace(xpath))
				return defaultValue;

			XElement element = source.XPathSelectElement(xpath);

			if (element == null)
				return defaultValue;

			try
			{
				Type type = typeof(T);

				return (T)Convert.ChangeType(element.Value, type.IsNullable() ? Nullable.GetUnderlyingType(type) : type, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}
		#endregion

		#region XPathSelectElementValue<T>(this XElement container, string xPath, T defaultValue)
		public static T XPathSelectElementValue<T>(this XElement container, string xPath, T defaultValue)
		{
			XElement element = container.XPathSelectElement(xPath);

			if (element == null)
			{
				return defaultValue;
			}

			try
			{
				Type type = typeof(T);
				return (T)Convert.ChangeType(element.Value, type.IsNullable() ? Nullable.GetUnderlyingType(type) : type, CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}
		#endregion

		// Private static methods

		#region IsNullable(this Type type)
		private static bool IsNullable(this Type type)
		{
			return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
		}
		#endregion

		#region GetNodeValue<T>(this XContainer parent, string xpath, T defaultValue)
		public static T GetNodeValue<T>(this XContainer parent, string xpath, T defaultValue)
		{
			return parent.GetNodeValue<T>(xpath, defaultValue, false);
		}
		#endregion

		#region GetXDocumentElementStringValue(this XDocument xDocument, string xPath, string defaultValue)
		public static string GetXDocumentElementStringValue(this XDocument xDocument, string xPath, string defaultValue)
		{
			XElement xElement = xDocument.XPathSelectElement(xPath);
			if (xElement == null) return defaultValue;

			return xElement.Value;
		}
		#endregion

		#region GetNodeStringValue(this XContainer parent, string xpath, string defaultValue)
		public static string GetNodeStringValue(this XContainer parent, string xpath, string defaultValue)
		{
			return parent.GetNodeValue<string>(xpath, defaultValue, false);
		}
		#endregion

		#region GetNodeValue (this XContainer parent, string xpath, T defaultValue, bool required)
		private static T GetNodeValue<T>(this XContainer parent, string xpath, T defaultValue, bool required)
		{
			return GetNodeValue<T>(parent, xpath, defaultValue, required, CultureInfo.InvariantCulture);
		}

		#endregion

		#region GetNodeValue<T>(this XContainer parent, string xpath, T defaultValue, bool required, CultureInfo cultureInfo)
		public static T GetNodeValue<T>(this XContainer parent, string xpath, T defaultValue, bool required, CultureInfo cultureInfo)
		{
			XElement node = parent.Element(xpath);

			if (String.IsNullOrEmpty(node?.Value))
			{
				if (required)
				{
					throw new Exception($"Node '{xpath}' does not exist.");
				}

				return defaultValue;
			}

			try
			{
				Type type = typeof(T);

				return (T)Convert.ChangeType(node.Value, type.IsNullable() ? Nullable.GetUnderlyingType(type) : type, cultureInfo);
			}
			catch (FormatException)
			{
				return defaultValue;
			}
		}
		#endregion

		#region OuterXml(this XElement thiz)
		public static string OuterXml(this XElement thiz)
		{
			var xReader = thiz.CreateReader();
			xReader.MoveToContent();
			return xReader.ReadOuterXml();
		}
		#endregion

		#region InnerXml(this XElement thiz)
		public static string InnerXml(this XElement thiz)
		{
			var xReader = thiz.CreateReader();
			xReader.MoveToContent();
			return xReader.ReadInnerXml();
		}
		#endregion
		// Event Handlers - Události

	}
}
