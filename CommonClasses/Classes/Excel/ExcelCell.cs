using System;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
	public class ExcelCell
	{
		internal const string ElementName = "cell";

		// Constructors

		#region ExcelCell(ExcelRow row, string name, string value, int maxLenght)
		internal ExcelCell(ExcelRow row, string name, string value, bool wrapText = true, int maxLenght = Int32.MaxValue, bool isLocked = true)
		{
			this.IsLocked = isLocked; // in Excel by default all cells are write protected, 
			// the effect becomes active when sheet protection is activated

			this.Value = String.Empty;
			this.WrapText = wrapText;

			if (!String.IsNullOrWhiteSpace(value))
			{
				this.Value = value.Length > maxLenght ? value.Substring(0, maxLenght - 1) : value;
			}

			this.Name = String.IsNullOrWhiteSpace(name) ? ExcelCell.ElementName : name;
			this.Row = row;
		}

		#endregion

		internal ExcelCell(ExcelRow row, string value, bool wrapText, int maxLenght, bool isLocked=true)
			: this(row, String.Empty, value, wrapText, maxLenght, isLocked)
		{

		}

		#region ExcelCell(ExcelRow row,string value, int maxLenght)
		internal ExcelCell(ExcelRow row, string value, int maxLenght, bool isLocked=true)
			: this(row, value, true, maxLenght, isLocked)
		{
		} 
		#endregion
		
		#region ExcelCell(ExcelRow row, string name, string value)
		internal ExcelCell(ExcelRow row, string name, bool wrapText, string value, bool isLocked = true)
			:this(row, name, value, wrapText, Int32.MaxValue, isLocked)
		{			
		}
		#endregion


		// Public properties
		public bool IsLocked { get; set; }

		#region Row
		public ExcelRow Row { get; private set; }
		#endregion

		#region Value
		public string Value { get; set; }
		#endregion

		#region Name
		public string Name { get; private set; }
		#endregion

		#region WrapText
		public bool WrapText { get; set; }
		#endregion

		// Public methods

		#region ToString()
		public override string ToString()
		{
			return this.Value;
		}
		#endregion

		// Internal static methods

		#region Create(ExcelRow row, XElement cellElement)
		internal static ExcelCell Create(ExcelRow row,  XElement cellElement)
		{
			return cellElement == null || row == null ? null : new ExcelCell(row, cellElement.Name.LocalName, cellElement.Value, cellElement.GetAttributeValue("wrapText", true), cellElement.GetAttributeValue("maxLength", Int32.MaxValue), cellElement.GetAttributeValue("isLocked", true));
		} 
		#endregion

		#region Create(ExcelRow row, ICell cell)
		internal static ExcelCell Create(ExcelRow row, ICell cell)
		{
			return ExcelCell.Create(row, cell, String.Empty);
		} 
		#endregion

		#region Create(ExcelRow row, ICell cell, string name)
		internal static ExcelCell Create(ExcelRow row, ICell cell, string name)
		{
			return row == null ? null : new ExcelCell(row, name, cell == null ? String.Empty : ExcelCell.GetCellValue(cell), cell == null || cell.CellStyle.WrapText, Int32.MaxValue, cell == null || cell.CellStyle.IsLocked);
		}
		#endregion

		// Internal methods

		#region AppentToXml(XElement parentElement)
		internal void AppentToXml(XElement parentElement)
		{
			if (parentElement == null)
				return;
			
			XElement element = parentElement.CreateElement(ExcelXmlUtils.ResolveXmlNodeName(String.IsNullOrWhiteSpace(this.Name) ? ExcelCell.ElementName : this.Name));

			element.Add(new XCData(String.IsNullOrWhiteSpace(this.Value) ? String.Empty : this.Value));
			element.CreateAttribute("wrapText", this.WrapText);
			element.CreateAttribute("isLocked", this.IsLocked);
		}
		#endregion

		// Private static methods

		#region GetCellValue(ICell cell)
		private static string GetCellValue(ICell cell)
		{
			if (cell == null)
				return String.Empty;

			switch (cell.CellType)
			{
				case CellType.String:
					return cell.StringCellValue;
				case CellType.Numeric:
					return cell.NumericCellValue.ToString();
				case CellType.Boolean:
					return cell.BooleanCellValue.ToString();
				case CellType.Blank:
				case CellType.Formula:
					return String.Empty;
				default:
					return String.Empty;
			}
		}
		#endregion
	} 	
}
