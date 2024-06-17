using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
	public class ExcelWorksheet
	{
		public const string ElementName = "sheet";

		public const string SheetName = "Sheet";

		private bool isProtected;
		private string protectionPassword;

		// Constructors

		#region ExcelWorksheet(ExcelWorkbook excelWorkbook, string name)
		internal ExcelWorksheet(ExcelWorkbook excelWorkbook, string name)
		{
			if(excelWorkbook == null)
				throw new ArgumentNullException("excelWorkbook");

			if(String.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			this.isProtected = false;
			this.protectionPassword = null;

			this.Rows = new List<ExcelRow>();
			this.Name = name;

			this.ExcelWorkbook = excelWorkbook;
			this.Headers = new List<ExcelSheetHeader>();
			this.HeaderTextColor = Color.Black;
			this.HeaderBackgroundColor = Color.White;
			this.AutoSizeColumns = false;			
		}
		#endregion

		// Public properties
		bool IsProtected { get { return this.isProtected; } }

		#region Workbook
		public ExcelWorkbook ExcelWorkbook { get; private set; }
		#endregion

		#region HasHeaders
		public bool HasHeaders
		{
			get { return this.Headers.Count > 0; }
		}
		#endregion

		#region Name
		public string Name { get; private set; }
		#endregion

		#region Hidden
		public bool Hidden { get; set; }
		#endregion

		#region Rows
		public List<ExcelRow> Rows { get; private set; }
		#endregion

		#region Headers
		public List<ExcelSheetHeader> Headers { get; private set; }
		#endregion

		#region FreezeFirstColumn
		public bool FreezeFirstColumn { get; set; } 
		#endregion

		#region HeaderTextColor
		public Color HeaderTextColor { get; set; } 
		#endregion

		#region HeaderBackgroundColor
		public Color HeaderBackgroundColor { get; set; } 
		#endregion

		#region AutoSizeColumns
		public bool AutoSizeColumns { get; set; } 
		#endregion

		// Private static methods

		#region CreateHeaders(ExcelWorksheet worksheet, ISheet sheet)
		private static void CreateHeaders(ExcelWorksheet worksheet, ISheet sheet)
		{
			if (worksheet == null || sheet == null)
				return;

			IRow row = sheet.GetRow(0);

			if (row == null)
				return;

			for (int colIndex = row.FirstCellNum;
				colIndex <= row.LastCellNum - 1;
				colIndex++)
			{
				ICell cell = row.GetCell(colIndex);
				worksheet.CreateHeader(cell != null ? cell.StringCellValue : String.Empty);
			}
		} 
		#endregion

		// Private static methods

		#region GetColorFromName(string colorName, Color defaultColor)
		private static Color GetColorFromName(string colorName, Color defaultColor)
		{
			if (String.IsNullOrWhiteSpace(colorName))
				return defaultColor;

			try
			{
				return Color.FromName(colorName);
			}
			catch
			{
				return defaultColor;
			}
		} 
		#endregion

		// Internal static methods

		#region Create(ExcelWorkbook workbook, XElement sheetElement)
		internal static ExcelWorksheet Create(ExcelWorkbook workbook, XElement sheetElement)
		{
			if (workbook == null || sheetElement == null)
				return null;

			ExcelWorksheet worksheet = new ExcelWorksheet(workbook, sheetElement.GetAttributeValue("name", String.Empty));

			XElement headersElement = sheetElement.Descendants("headers").FirstOrDefault();

			if(headersElement != null)
			{
				worksheet.AutoSizeColumns = headersElement.GetAttributeValue("autoSize", false);
				worksheet.HeaderTextColor = ExcelWorksheet.GetColorFromName(headersElement.GetAttributeValue("headerTextColor", String.Empty), Color.Black);
				worksheet.HeaderBackgroundColor = ExcelWorksheet.GetColorFromName(headersElement.GetAttributeValue("headerBackgroundColor", String.Empty), Color.White);				
			}


			foreach (XElement headerElement in sheetElement.Descendants(ExcelSheetHeader.ElementName))
			{
				worksheet.Headers.Add(ExcelSheetHeader.Create(worksheet, headerElement));
			}

			foreach (XElement headerElement in sheetElement.Descendants(ExcelRow.ElementName))
			{
				worksheet.Rows.Add(ExcelRow.Create(worksheet, headerElement));
			}

			worksheet.Hidden = sheetElement.GetAttributeValue("hidden", false);
			if (sheetElement.GetAttributeValue("isProtected", false) && !String.IsNullOrEmpty(sheetElement.GetAttributeValue("protectionPassword", String.Empty)))
			{
				worksheet.ProtectSheet(sheetElement.GetAttributeValue("protectionPassword", String.Empty));
			}

			return worksheet;

		} 
		#endregion

		#region Create(ExcelWorkbook workbook, ISheet sheet, bool hasHeaders, OperationResult operationResult)
		internal static ExcelWorksheet Create(ExcelWorkbook workbook, ISheet sheet, bool hasHeaders, OperationResult operationResult)
		{
			if (workbook == null || sheet == null)
				return null;

			ExcelWorksheet worksheet = new ExcelWorksheet(workbook, sheet.SheetName);

			if(sheet.LastRowNum < 0)
				return worksheet;

			var prevCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			try
			{
				if (hasHeaders)
				{
					ExcelWorksheet.CreateHeaders(worksheet, sheet);
				}

				for (int rowIndex = hasHeaders ? sheet.FirstRowNum + 1 : sheet.FirstRowNum;
					rowIndex <= sheet.LastRowNum; rowIndex++)
				{
					IRow row = sheet.GetRow(rowIndex);

					if (row == null)
						continue;

					//toto se musi resit jinak, zpusobuje chyby
					//InvalidOperationException message: Cannot get a text value from a numeric cell
					if (row.Cells.Where(cell => cell.CellType == CellType.String).Any(cell => cell.StringCellValue.Any(p => !XmlConvert.IsXmlChar(p))))
					{
						operationResult.Error(String.Format("Row {0} contains one or more invalid characters and could not have been imported", rowIndex + 1),
							bmw_log_bmw_reasonstate.Error);
						continue;
					}

					worksheet.Rows.Add(ExcelRow.Create(worksheet, row));
				}

				worksheet.Hidden = sheet.Workbook.IsSheetHidden(sheet.Workbook.GetSheetIndex(sheet));

				if (sheet.Protect)
				{
					worksheet.ProtectSheet("123456789"); // "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! to nepude :(
				}
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = prevCulture;
			}

			return worksheet;
		}
		#endregion

		// Internal methods

		#region GetHeaderName(int index)
		internal string GetHeaderName(int index)
		{
			if (this.Headers.Count == 0 || this.Headers.Count - 1 < index)
				return String.Empty;

			return this.Headers[index].Name;
		} 
		#endregion

		#region GetHeaderIndex(string name)
		internal int GetHeaderIndex(string name)
		{
			if (this.Headers.Count == 0 || String.IsNullOrWhiteSpace(name))
				return -1;

			var header = this.Headers.FirstOrDefault(h => h.Name == name);

			if (header == null)
				return -1;

			return this.Headers.IndexOf(header);
		}
		#endregion

		// Public methods
		public void ProtectSheet(string password)
		{
			if(String.IsNullOrEmpty(password))throw  new ApplicationException("protection password cant be null or empty");

			this.isProtected = true;
			this.protectionPassword = password;
		}

		#region ContainsAllHeaders(IEnumerable<string> headers)
		public bool ContainsAllHeaders(IEnumerable<string> headers)
		{
			if (headers == null || !headers.Any())
			{
				return this.Headers.Count == 0;
			}

			return headers.All(h => this.Headers.Any(wh => wh.Name == h));

		} 
		#endregion

		#region ContainsAllHeaders(IEnumerable<string> headers)
		public bool ContainsHeader(string headerName)
		{
			if (String.IsNullOrWhiteSpace(headerName))
			{
				return this.Headers.Count == 0;
			}

			return this.Headers.Any(wh => wh.Name == headerName);

		}
		#endregion

		#region GetRowIndex(ExcelRow row)
		public int GetRowIndex(ExcelRow row)
		{
			if(row == null)
				throw new ArgumentNullException();

			return this.Rows.IndexOf(row);
		} 
		#endregion

		#region CreateHeader(string name, IEnumerable<string> allowedValues = null)
		public ExcelSheetHeader CreateHeader(string name, IEnumerable<string> allowedValues = null)
		{
			return this.CreateHeader(name, false, allowedValues);
		} 
		#endregion

		#region CreateHeader(string name, bool  freeze, IEnumerable<string> allowedValues = null)
		public ExcelSheetHeader CreateHeader(string name, bool  freeze, IEnumerable<string> allowedValues = null)
		{
			if (this.Headers.Any(h => h.Name == name))
				throw new Exception(String.Format("Header with name '{0}' exists", name));

			ExcelSheetHeader header = new ExcelSheetHeader(this, name, allowedValues) {Freeze = freeze};
			this.Headers.Add(header);

			return header;
		}
		#endregion

		#region RemoveHeader(string name)
		public void RemoveHeader(string name)
		{
			ExcelSheetHeader header = this.Headers.FirstOrDefault(h => h.Name == name);

			if (header == null)
				return;

			this.Headers.Remove(header);
		}
		#endregion

		#region CreateRow(IEnumerable<string> data = null)
		public ExcelRow CreateRow(IEnumerable<string> data = null)
		{
			ExcelRow row = new ExcelRow(this);
			this.Rows.Add(row);

			if(data != null)
			{				
				foreach (string cellValue in data)
				{
					row.CreateCell(cellValue);
				}
			}
			return row;
		} 
		#endregion

		#region RemoveRow(ExcelRow row)
		public void RemoveRow(ExcelRow row)
		{
			if (!this.Rows.Contains(row))
				return;

			this.Rows.Remove(row);
		} 
		#endregion

		#region ToString()
		public override string ToString()
		{
			return this.Name;
		}
		#endregion

		// Internal Methods

		#region AppendToXml(XElement parentElement)
		internal void AppendToXml(XElement parentElement)
		{
			if (parentElement == null)
				return;

			XElement sheetElement = parentElement.CreateElement(ExcelWorksheet.ElementName);
			sheetElement.CreateAttribute("name", this.Name);

			if (this.isProtected && !String.IsNullOrEmpty(this.protectionPassword))
			{
				sheetElement.CreateAttribute("isProtected", this.IsProtected);
				sheetElement.CreateAttribute("protectionPassword", this.protectionPassword);
			}
			
			sheetElement.CreateAttribute("hidden", this.Hidden);
			sheetElement.CreateAttribute("hasHeaders", (this.Headers.Count > 0).ToString().ToLower());

			XElement headersElement = sheetElement.CreateElement("headers");
			headersElement.AddAttribute("autoSize", this.AutoSizeColumns.ToString().ToLower());
			headersElement.AddAttribute("headerTextColor", this.HeaderTextColor.Name);
			headersElement.AddAttribute("headerBackgroundColor", this.HeaderBackgroundColor.Name);

			this.Headers.ForEach(h => h.AppendToXml(headersElement));

			XElement rowsElement = sheetElement.CreateElement("rows");
			this.Rows.ForEach(r => r.AppendToXml(rowsElement));
		}
		#endregion
	}
}