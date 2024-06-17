using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.XmlXsdValidation
{
	public class XmlXsdValidator
	{
		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní proměné
		private bool success;
		private readonly bool stopOnFirstError;
		private readonly List<string> errors;

		// Constructors - Konstruktory
		#region XmlXsdValidator()
		public XmlXsdValidator()
		{
			this.success = true;
			this.stopOnFirstError = false;
			this.errors = new List<string>();
		} 
		#endregion

		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti
		#region StopOnFirstError
		public bool StopOnFirstError { get; set; } 
		#endregion

		#region Errors
		public List<string> Errors
		{
			get { return this.errors; }
		} 
		#endregion

		#region LastError
		public string LastError
		{
			get
			{
				if (this.errors.Count > 0)
				{
					return this.errors[this.errors.Count - 1];
				}
				return null;
			}
		} 
		#endregion

		// Private Methods - Privátní metody
		#region ShouldIReadFurther()
		private bool ShouldIReadFurther()
		{
			//this method evaluates _success and StopOnFirstError values and
			//indicates if the validator should read further
			if (this.success)
			{ return true; }

			if (this.stopOnFirstError)
			{ return false; }

			return true;
		} 
		#endregion

		#region ValidationCallBack(Object sender, ValidationEventArgs args)
		private void ValidationCallBack(Object sender, ValidationEventArgs args)
		{
			this.success = false; //Validation failed
			this.errors.Add(args.Message);
		} 
		#endregion

		// Protected Methods - Protected metody

		// Public Methods - Public metody
		#region ValidateXml(String xmlUri, String xsdUri)
		public bool ValidateXml(String xmlUri, String xsdUri)
		{
			this.success = true;
			this.errors.Clear();

			XmlReaderSettings xmlSettings = new XmlReaderSettings {Schemas = new System.Xml.Schema.XmlSchemaSet()};
			xmlSettings.Schemas.Add(null, xsdUri);
			xmlSettings.ValidationType = ValidationType.Schema;
			xmlSettings.ValidationEventHandler += this.ValidationCallBack;
			using (XmlReader reader = XmlReader.Create(xmlUri, xmlSettings))
			{
				while (reader.Read() && this.ShouldIReadFurther()) { }
			}

			return success;
		} 
		#endregion

		#region ValidateXml(XDocument xmlToValidate, XDocument xsd)
		public bool ValidateXml(XDocument xmlToValidate, XDocument xsd)
		{
			this.success = true;
			this.errors.Clear();

			XmlReaderSettings xmlSettings = new XmlReaderSettings { Schemas = new System.Xml.Schema.XmlSchemaSet() };

			using (XmlReader xsdReader = xsd.CreateReader())
			{ xmlSettings.Schemas.Add(XmlSchema.Read(xsdReader, null)); }

			xmlSettings.ValidationType = ValidationType.Schema;
			xmlSettings.ValidationEventHandler += this.ValidationCallBack;

			using (XmlReader reader = XmlReader.Create(xmlToValidate.CreateReader(), xmlSettings))
			{
				while (reader.Read() && this.ShouldIReadFurther()) { }
			}

			return success;
		} 
		#endregion

		#region ValidateXml(String xmlUri, String xsdUri)
		public bool ValidateXml(XDocument xmlToValidate, String xsdUri)
		{
			this.success = true;
			this.errors.Clear();

			XmlReaderSettings xmlSettings = new XmlReaderSettings { Schemas = new System.Xml.Schema.XmlSchemaSet() };
			xmlSettings.Schemas.Add(null, xsdUri);
			xmlSettings.ValidationType = ValidationType.Schema;
			xmlSettings.ValidationEventHandler += this.ValidationCallBack;
			using (XmlReader reader = XmlReader.Create(xmlToValidate.CreateReader(), xmlSettings))
			{
				while (reader.Read() && this.ShouldIReadFurther()) { }
			}

			return success;
		}
		#endregion

		#region GetFormatedErrors()
		public string GetFormatedErrors()
		{
			if (this.Errors == null || this.Errors.Count == 0)
			{
				return String.Empty;
			}

			string result = String.Empty;

			foreach (string error in this.Errors)
			{
				if (!String.IsNullOrEmpty(result))
				{
					result += Environment.NewLine;
				}

				result += error;

			}
			return result;

		} 
		#endregion

		// Event Handlers - Události
		
	}
}
