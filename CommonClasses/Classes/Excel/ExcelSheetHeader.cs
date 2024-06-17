using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
	public class ExcelSheetHeader
	{

		internal const string ElementName = "header";
		private bool hasAllowedValues;
		
		// Constructors

		#region ExcelSheetHeader(ExcelWorksheet sheet, string name, IEnumerable<string> allowedValues)
		internal ExcelSheetHeader(ExcelWorksheet sheet, string name, IEnumerable<string> allowedValues)
			: this(sheet, name)
		{
			if (allowedValues != null)
			{
				this.AllowedValues.AddRange(allowedValues);
				this.hasAllowedValues = true;
			}
		} 
		#endregion

		#region ExcelSheetHeader(ExcelWorksheet sheet, string name)
		internal ExcelSheetHeader(ExcelWorksheet sheet, string name)
		{
			this.AllowedValues = new List<string>();
			this.Name = name;
			this.Sheet = sheet;
			this.Freeze = false;
		}
		#endregion

		// Public properties

		#region Sheet
		public ExcelWorksheet Sheet { get; private set; } 
		#endregion

		#region Name
		public string Name { get; private set; }
		#endregion

		#region AllowedValues
		public List<string> AllowedValues { get; private set; }
		#endregion

		#region Freeze
		public bool Freeze { get; set; } 
		#endregion

		// Internal static methods

		#region Create(ExcelWorksheet worksheet, XElement headerElement)
		internal static ExcelSheetHeader Create(ExcelWorksheet worksheet, XElement headerElement)
		{
			if (worksheet == null)
				return null;

			if (headerElement == null)
				return null;

			string name = headerElement.GetAttributeValue("name", String.Empty);

			string[] allowedValues = null;
			if (headerElement.Descendants("allowedValues").Any())
			{
				allowedValues = headerElement.Descendants("allowedValue").Select(av => av.Value).ToArray();
			}

			return new ExcelSheetHeader(worksheet, name, allowedValues);

		} 
		#endregion

		// Public methods

		#region AddAllowedValue(string value)
		public void AddAllowedValue(string value)
		{
			if (this.AllowedValues.Contains(value))
				return;

			this.AllowedValues.Add(value);
			this.hasAllowedValues = true;
		} 
		#endregion

		#region RemoveAllowedValue(string value)
		public void RemoveAllowedValue(string value)
		{
			if (!this.AllowedValues.Contains(value))
				return;

			this.AllowedValues.Remove(value);
		} 
		#endregion

		#region ClearAllowedValues()
		public void ClearAllowedValues()
		{
			this.hasAllowedValues = false;
			this.AllowedValues.Clear();
		} 
		#endregion

		#region ActivateAllowedValues()
		public void ActivateAllowedValues()
		{
			this.hasAllowedValues = true;
		}
		#endregion

		// Internal methods
		
		#region AppendToXml(XElement parentNode)
		internal void AppendToXml(XElement parentNode)
		{
			if (parentNode == null)
				return;

			XElement headerElement = new XElement(ExcelSheetHeader.ElementName);
			headerElement.CreateAttribute("name", this.Name);
			headerElement.CreateAttribute("index", this.Sheet.GetHeaderIndex(this.Name).ToString());
			headerElement.AddAttribute("freeze", this.Freeze.ToString().ToLower());

			if(this.hasAllowedValues)
			{
				XElement allowedValuesElement = headerElement.CreateElement("allowedValues");

				foreach (string allowedValue in this.AllowedValues)
				{
					allowedValuesElement.CreateElement("allowedValue", allowedValue);
				}
			}			

			parentNode.Add(headerElement);
		}
		#endregion

	}
}