using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
	public class ExcelWorkbook
	{
		private static string ElementName = "workbook";

		// Constructors

		#region ExcelWorkbook()
		public ExcelWorkbook()
		{
			this.Sheets = new List<ExcelWorksheet>();
		}
		#endregion

		// Indexers

		#region ExcelWorksheet this[int index]
		public ExcelWorksheet this[int index]
		{
			get
			{
				if (index < 0 || index > this.Sheets.Count - 1)
					throw new ArgumentOutOfRangeException("index");

				return this.Sheets[index];
			}
		} 
		#endregion

		#region this[string sheetName]
		public ExcelWorksheet this[string sheetName]
		{
			get
			{
				if (String.IsNullOrWhiteSpace(sheetName))
					throw new ArgumentNullException("sheetName");

				return this.Sheets.FirstOrDefault(s => s.Name == sheetName);
			}
		} 
		#endregion

		// Public properties

		#region Sheets
		public List<ExcelWorksheet> Sheets { get; private set; }
		#endregion

		// Private methods

		#region GenerateName()
		private string GenerateName()
		{
			int index = this.Sheets.Count + 1;
			string name = String.Format("{0} {1}", ExcelWorksheet.SheetName, index);
			while (this.Sheets.Any(s => s.Name.Equals(name)))
			{
				index++;
				name = String.Format("{0} {1}", ExcelWorksheet.SheetName, index);
			}

			return name;
		} 
		#endregion

		// Public static methods

		#region Create(XDocument data)
		public static ExcelWorkbook Create(XDocument data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			if (data.Root == null)
				return null;

			ExcelWorkbook workbook = new ExcelWorkbook();

			foreach (XElement sheetElement in data.Descendants(ExcelWorksheet.ElementName))
			{
				workbook.Sheets.Add(ExcelWorksheet.Create(workbook, sheetElement));
			}

			return workbook;

		} 
		#endregion

		#region Create(IWorkbook workbook, bool hasHeaders)
		public static ExcelWorkbook Create(IWorkbook workbook, bool hasHeaders, OperationResult operationResult)
		{
			if (workbook == null)
				throw new ArgumentNullException();

			ExcelWorkbook excelWorkbook = new ExcelWorkbook();

			if (workbook.NumberOfSheets == 0)
				return excelWorkbook;

			for (int i = 0; i < workbook.NumberOfSheets; i++)
			{
				excelWorkbook.Sheets.Add(ExcelWorksheet.Create(excelWorkbook, workbook.GetSheetAt(i), hasHeaders, operationResult));
			}

			return excelWorkbook;
		} 
		#endregion

		// Public methods
		
		#region CreateWorksheet()
		public ExcelWorksheet CreateWorksheet()
		{
			return this.CreateWorksheet(this.GenerateName());
		} 
		#endregion
		
		#region CreateWorksheet(string name)
		public ExcelWorksheet CreateWorksheet(string name)
		{
			ExcelWorksheet excelWorksheet = new ExcelWorksheet(this, name);
			this.Sheets.Add(excelWorksheet);

			return excelWorksheet;
		} 
		#endregion

		#region ToXml()
		public XDocument ToXml()
		{
			XElement root = new XElement(ExcelWorkbook.ElementName);
			
			this.Sheets.ForEach(s => s.AppendToXml(root));
			return new XDocument(root);
		}
		#endregion
	}
}