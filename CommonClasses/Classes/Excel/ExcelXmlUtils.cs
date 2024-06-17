using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
	public class ExcelXmlUtils
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

		// Public static methods - Public metody

		#region PresentValueCondition(XDocument xExcelDocument, int? row, int? col, string expectedValue)
		public static bool PresentValueCondition(XDocument xExcelDocument, int? row, int? col, string expectedValue)
		{
			return ExcelXmlUtils.PresentValueCondition(xExcelDocument, row, col, null, expectedValue);
		}
		#endregion

		#region PresentValueCondition(XDocument xExcelDocument, int? row, int? col, string sheetName, string expectedValue)
		public static bool PresentValueCondition(XDocument xExcelDocument, int? row, int? col, string sheetName, string expectedValue)
		{
			if (expectedValue == null)
			{ throw new ApplicationException("expectedValue parameter must not be null"); }

			string sheetNameXPathCondition = "";
			if (!String.IsNullOrEmpty(sheetName))
			{
				sheetNameXPathCondition = " [@SheetName=\"" + sheetName + "\"]";
			}
			string cellRowXPathCondition = "";
			if (row.HasValue)
			{
				cellRowXPathCondition = " [@Row=\"" + row.Value.ToString(CultureInfo.InvariantCulture) + "\"]";
			}
			string cellColXPathCondition = "";
			if (col.HasValue)
			{
				cellColXPathCondition = " [@Col=\"" + col.Value.ToString(CultureInfo.InvariantCulture) + "\"]";
			}

			return
				xExcelDocument.XPathSelectElements(
				"/WorkBook/WorkSheets/Sheet" + sheetNameXPathCondition +
				"/Cell" + cellRowXPathCondition + cellColXPathCondition)
				.Any(xCell => xCell.Value == expectedValue);
		} 
		#endregion

		#region PresentSheetCondition(XDocument xExcelDocument, string expectedSheetName)
		public static bool PresentSheetCondition(XDocument xExcelDocument, string expectedSheetName)
		{
			if (String.IsNullOrEmpty(expectedSheetName))
			{
				throw new ApplicationException("expectedSheetName parameter must not be null or empty");
			}

			return xExcelDocument.XPathSelectElement(
				"/WorkBook/WorkSheets/Sheet[@SheetName=\"" + expectedSheetName + "\"]"
				) != null;
		} 
		#endregion

		#region SheetsCountCondition(XDocument xExcelDocument, int? from, int? to, int? exactCount)
		public static bool SheetsCountCondition(XDocument xExcelDocument, int? from, int? to, int? exactCount)
		{
			if (exactCount.HasValue)
			{
				return xExcelDocument.XPathSelectElements("/WorkBook/WorkSheets/Sheet").Count() == exactCount;
			}

			if (from == null && to == null && exactCount == null)
			{
				throw new ApplicationException("At least one parameter must have value");
			}

			if (from.HasValue && !to.HasValue)
			{
				return xExcelDocument.XPathSelectElements("/WorkBook/WorkSheets/Sheet").Count() >= from.Value;
			}

			if (!from.HasValue)
			{
				return xExcelDocument.XPathSelectElements("/WorkBook/WorkSheets/Sheet").Count() <= to.Value;
			}

			return xExcelDocument.XPathSelectElements("/WorkBook/WorkSheets/Sheet").Count() >= from.Value && xExcelDocument.XPathSelectElements("/WorkBook/WorkSheets/Sheet").Count() <= to.Value;
			
		} 
		#endregion

		#region ResolveXmlNodeName(string name)
		public static string ResolveXmlNodeName(string name)
		{
			if (String.IsNullOrWhiteSpace(name))
				return String.Empty;
			return name.Replace(" ", "_").Replace("/", "_").Replace("(", "_").Replace(")", "_");
		} 
		#endregion

		// Event Handlers - Události
	}
}
