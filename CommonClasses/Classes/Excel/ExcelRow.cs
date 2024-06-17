using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
	public class ExcelRow
	{

		internal const string ElementName = "row";
		// Constructors

		#region ExcelRow(ExcelWorksheet ExcelWorksheet)
		internal ExcelRow(ExcelWorksheet excelWorksheet)
		{
			this.Cells = new List<ExcelCell>();
			this.Sheet = excelWorksheet;
		}
		#endregion

		// Public properties

		#region Sheet
		public ExcelWorksheet Sheet { get; private set; }
		#endregion

		#region Cells
		public List<ExcelCell> Cells { get; private set; }
		#endregion

		#region Index
		public int Index
		{
			get { return this.Sheet.GetRowIndex(this); }
		} 
		#endregion

		// Internal static methods

		#region Create(ExcelWorksheet worksheet, XElement rowElement)
		internal static ExcelRow Create(ExcelWorksheet worksheet, XElement rowElement)
		{
			if (rowElement == null || worksheet == null)
				return null;

			ExcelRow row = new ExcelRow(worksheet);

			foreach (XElement cellElement in rowElement.Descendants())
			{
				row.Cells.Add(ExcelCell.Create(row,  cellElement));
			}

			return row;
		} 
		#endregion

		#region Create(ExcelWorksheet worksheet, IRow row)
		internal static ExcelRow Create(ExcelWorksheet worksheet, IRow row)
		{
			if (row == null || worksheet == null)
				return null;

			ExcelRow excelRow = new ExcelRow(worksheet);

			if (row.LastCellNum < 0)
				return excelRow;

			for (int i = 0; i < row.LastCellNum; i++)
			{
				ICell cell = row.GetCell(i);

				excelRow.CreateCell(cell);
			}
			
			return excelRow;

		} 
		#endregion

		// Internal methods

		#region AppendToXml(XElement parentElement)
		internal void AppendToXml(XElement parentElement)
		{
			if (parentElement == null)
				return;

			if (this.Cells.Count == 0)
				return;

			XElement rowElement = parentElement.CreateElement(ExcelRow.ElementName);
			this.Cells.ForEach(c => c.AppentToXml(rowElement));

		}
		#endregion

		#region CreateCell(ICell cell, bool isLocked)
		internal ExcelCell CreateCell(ICell cell)
		{
			ExcelCell excelCell = ExcelCell.Create(this, cell, this.Sheet.GetHeaderName(this.Cells.Count));
			this.Cells.Add(excelCell);
			return excelCell;
		}
		#endregion

		// Public methods

		#region CreateCell(string value, bool isLocked)
		public ExcelCell CreateCell(string value, bool isLocked = true)
		{			
			return this.CreateCell(value, Int32.MaxValue, isLocked);
		} 
		#endregion

		#region CreateCell(string value, int maxLenght, bool isLocked)
		public ExcelCell CreateCell(string value, int maxLenght, bool isLocked = true)
		{
			ExcelCell cell = new ExcelCell(this, this.Sheet.GetHeaderName(this.Cells.Count), value, true, maxLenght, isLocked);
			this.Cells.Add(cell);
			return cell;
		}
		#endregion
		
		#region RemoveCell(ExcelCell cell)
		public void RemoveCell(ExcelCell cell)
		{
			if (cell == null)
				return;

			if (!this.Cells.Contains(cell))
				return;

			this.Cells.Remove(cell);
		} 
		#endregion

		#region GetCellValue(string headerName)
		public string GetCellValue(string headerName)
		{
			if (String.IsNullOrWhiteSpace(headerName))
				throw new ArgumentNullException("headerName");

			int index = this.Sheet.GetHeaderIndex(headerName);

			if (index < 0)
				throw new ApplicationException("Header not found");

			return this.GetCellValue(index);
		} 
		#endregion

		#region GetCellValue(int index)
		public string GetCellValue(int index)
		{
			if (index < 0 || index > this.Cells.Count - 1)
			{
				throw new ArgumentOutOfRangeException("index");
			}

			return this.Cells[index].Value;
		} 
		#endregion

	}
}