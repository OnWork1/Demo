using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.Extractor;
using NPOI.XSSF.UserModel;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Strings;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Xml;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Excel
{
    public class XmlToXlsxResolver
    {
        // Private enums

        #region ValidationMethodEnum
        private enum ValidationMethodEnum
        {
            Lite,
            HiddenSheet
        }
        #endregion

        private const string SheetName = "Sheet";
        private const string AllowedValuesSheetName = "ALLOWED_VALUES_SHEET_NAME";
        private static string[] columnAddresses = null;
        private const ValidationMethodEnum ValidationMethod = ValidationMethodEnum.Lite;


        // Private static properties

        #region ColumnAddresses
        private static string[] ColumnAddresses
        {
            get
            {
                return XmlToXlsxResolver.columnAddresses ??
                       (XmlToXlsxResolver.columnAddresses = XmlToXlsxResolver.CreateColumnAddressesArray());
            }
        }
        #endregion

        // Public static methods

        #region ConvertXmlToXlsx(ExcelWorkbook workbook)
        public static XSSFWorkbook ConvertXmlToXlsx(ExcelWorkbook workbook)
        {
            return workbook == null ? null : XmlToXlsxResolver.ConvertXmlToXlsx(workbook.ToXml());
        }
        #endregion

        #region ConvertXmlToXlsx(XDocument data)
        public static XSSFWorkbook ConvertXmlToXlsx(XDocument data)
        {
            if (data == null || data.Root == null)
                return null;


            XSSFWorkbook workbook = new XSSFWorkbook();

            IEnumerable<XElement> sheetsElement = data.Root.Descendants("sheet");

            if (!sheetsElement.Any())
            {
                return workbook;
            }

            foreach (XElement sheetElement in sheetsElement)
            {
                XmlToXlsxResolver.CreateSheet(sheetElement, workbook);
            }

            return workbook;
        }
        #endregion

        // Private static methods

        #region GetSheetName(IWorkbook workbook)
        private static string GetSheetName(IWorkbook workbook)
        {
            int index = workbook.NumberOfSheets + 1;
            string name = String.Format("{0} {1}", XmlToXlsxResolver.SheetName, index);

            while (workbook.GetSheetIndex(name) > -1)
            {
                index++;
                name = String.Format("{0} {1}", XmlToXlsxResolver.SheetName, index);
            }
            return name;
        }
        #endregion

        #region PrepareStyles(IWorkbook workBook, out ICellStyle lockedStyle, out ICellStyle wrapStyle, out ICellStyle wrapLockedStyle)
        private static void PrepareStyles(IWorkbook workBook, out ICellStyle lockedStyle, out ICellStyle wrapStyle, out ICellStyle wrapLockedStyle)
        {
            lockedStyle = workBook.CreateCellStyle();
            lockedStyle.IsLocked = true;

            wrapStyle = workBook.CreateCellStyle();
            wrapStyle.WrapText = true;
            wrapStyle.IsLocked = false;

            wrapLockedStyle = workBook.CreateCellStyle();
            wrapLockedStyle.IsLocked = true;
            wrapLockedStyle.WrapText = true;
        }
        #endregion

        #region CreateSheet(XElement sheetElement, IWorkbook workbook)
        private static void CreateSheet(XElement sheetElement, XSSFWorkbook workbook)
        {
            XElement dataRowsElement = sheetElement.Element("rows");

            int rowIndex = 0;
            string sheetName = sheetElement.GetAttributeValue("name", String.Empty);

            if (String.IsNullOrWhiteSpace(sheetName))
                sheetName = XmlToXlsxResolver.GetSheetName(workbook);

            ISheet worksheet = workbook.CreateSheet(sheetName);

            bool hasHeaders = sheetElement.GetAttributeValue("hasHeaders", "false") == "true" || sheetElement.Descendants("header").Any();

            if (sheetElement.GetAttributeValue("isProtected", false))
            { worksheet.ProtectSheet(sheetElement.GetAttributeValue("protectionPassword", "")); }

            if (hasHeaders)
            {
                XmlToXlsxResolver.CreateHeaders(sheetElement, worksheet);
                rowIndex = 1;
            }

            if (dataRowsElement == null)
                return;

            IEnumerable<XElement> rowsElement = dataRowsElement.Elements("row").ToArray();

            ICellStyle lockedStyle;
            ICellStyle wrapStyle;
            ICellStyle wrapLockedStyle;

            XmlToXlsxResolver.PrepareStyles(workbook, out lockedStyle, out wrapStyle, out wrapLockedStyle);

            if (rowsElement.Any())
            {
                foreach (XElement[] rowValueElement in rowsElement.Select(xElement => xElement.Elements().ToArray()))
                {
                    IRow row = worksheet.GetRow(rowIndex) ?? worksheet.CreateRow(rowIndex);

                    for (int i = 0; i < rowValueElement.Count(); i++)
                    {
                        ICell cell = row.GetCell(i) ?? row.CreateCell(i);

                        bool isLocked = rowValueElement[i].GetAttributeValue("isLocked", true);
                        bool wrapText = rowValueElement[i].GetAttributeValue("wrapText", true);

                        if (isLocked)
                        {
                            cell.CellStyle = wrapText ? wrapLockedStyle : lockedStyle;
                        }
                        else if (wrapText)
                        {
                            cell.CellStyle = wrapStyle;
                        }

                        cell.SetCellValue(rowValueElement[i].Value);
                    }

                    rowIndex++;
                }
            }

            XmlToXlsxResolver.CreateHeadersValidation(sheetElement, worksheet, workbook);

            if (sheetElement.GetAttributeValue("hidden", "false") == "true")
            {
                workbook.SetSheetHidden(workbook.GetSheetIndex(worksheet), SheetState.Hidden);
            }

            if (hasHeaders)
            {
                XElement headersElement = sheetElement.FindElement("headers");

                if (headersElement == null || worksheet.LastRowNum < 1)
                    return;

                // freeze headers
                worksheet.CreateFreezePane(0, 1, 0, worksheet.LastRowNum);

                if (!headersElement.GetAttributeValue("autoSize", false))
                    return;

                foreach (XElement headerElement in headersElement.Elements("header"))
                {
                    int index = headerElement.GetAttributeValue("index", -1);

                    if (index < 0)
                        continue;

                    worksheet.AutoSizeColumn(index);
                    if (headerElement.GetAttributeValue("freeze", false))
                    {
                        worksheet.CreateFreezePane(index + 1, 1, index + 1, worksheet.LastRowNum);
                    }
                }
            }
        }
        #endregion

        #region CreateHeadersValidation(XElement sheetElement, ISheet worksheet, IWorkbook workbook)
        private static void CreateHeadersValidation(XElement sheetElement, ISheet worksheet, IWorkbook workbook)
        {
            if (XmlToXlsxResolver.ValidationMethod == ValidationMethodEnum.Lite)
                XmlToXlsxResolver.CreateHeadersValidationLite(sheetElement, worksheet, workbook);
            else
                XmlToXlsxResolver.CreateHeadersValidationHiddenList(sheetElement, worksheet, workbook);
        }
        #endregion

        #region CreateHeadersValidationLite(XElement sheetElement, ISheet worksheet, IWorkbook workbook)
        private static void CreateHeadersValidationLite(XElement sheetElement, ISheet worksheet, IWorkbook workbook)
        {
            if (sheetElement == null)
                return;

            XElement headersElement = sheetElement.FindElement("headers");

            if (headersElement == null)
                return;

            foreach (XElement headerElement in headersElement.Elements("header"))
            {
                string headerName = headerElement.GetAttributeValue("name", String.Empty);

                if (String.IsNullOrWhiteSpace(headerName))
                    continue;

                var allowedValuesElement = headerElement.Element("allowedValues");
                if (allowedValuesElement == null) continue;


                var allowedValuesItems = headerElement.Descendants("allowedValue");
                if (!allowedValuesItems.Any())
                    continue;

                int columnIndex = XmlToXlsxResolver.GetColumnIndex(headerName, worksheet);

                if (columnIndex < 0)
                    continue;

                XSSFDataValidationHelper dvHelper = new XSSFDataValidationHelper((XSSFSheet)worksheet);
                XSSFDataValidationConstraint dvConstraint = (XSSFDataValidationConstraint)
                  dvHelper.CreateExplicitListConstraint(allowedValuesItems.Select(e => e.Value).ToArray());
                CellRangeAddressList addressList = new CellRangeAddressList(1, worksheet.LastRowNum, columnIndex, columnIndex);
                XSSFDataValidation validation = (XSSFDataValidation)dvHelper.CreateValidation(
                  dvConstraint, addressList);

                // Here the boolean value false is passed to the setSuppressDropDownArrow()
                // method. In the hssf.usermodel examples above, the value passed to this
                // method is true.            
                validation.SuppressDropDownArrow = true;//this works differently than named !!! Set to true if you want to have dropdown

                // Note this extra method call. If this method call is omitted, or if the
                // boolean value false is passed, then Excel will not validate the value the
                // user enters into the cell.
                validation.ShowErrorBox = true;
                worksheet.AddValidationData(validation);

            }
        }
        #endregion

        #region CreateHeadersValidationHiddenList(XElement sheetElement, ISheet worksheet, IWorkbook workbook)
        private static void CreateHeadersValidationHiddenList(XElement sheetElement, ISheet worksheet, IWorkbook workbook)
        {//nefunguje
            if (sheetElement == null)
                return;

            XElement headersElement = sheetElement.FindElement("headers");

            if (headersElement == null)
                return;

            foreach (XElement headerElement in headersElement.Elements("header"))
            {
                string headerName = headerElement.GetAttributeValue("name", String.Empty);

                if (String.IsNullOrWhiteSpace(headerName))
                    continue;

                var allowedValues = headerElement.Descendants("allowedValue");

                if (!allowedValues.Any())
                    continue;

                int columnIndex = XmlToXlsxResolver.GetColumnIndex(headerName, worksheet);

                if (columnIndex < 0)
                    continue;

                ISheet allowedValuesSheet = workbook.GetSheet(AllowedValuesSheetName) ?? workbook.CreateSheet(AllowedValuesSheetName);

                string[] allowedValuesStrings = (from av in allowedValues select av.Value).ToArray();
                for (int i = 0; i < allowedValuesStrings.Count(); i++)
                {
                    IRow row = allowedValuesSheet.GetRow(i) ?? allowedValuesSheet.CreateRow(i);

                    var cell = row.CreateCell(columnIndex);
                    cell.SetCellValue(allowedValuesStrings[i]);
                }
                IName namedCell = workbook.CreateName();
                namedCell.NameName = headerName.RemoveWhiteSpaces() + "Formula";

                string columnAddress = XmlToXlsxResolver.ColumnAddresses[columnIndex];

                namedCell.RefersToFormula = String.Format("{0}!{1}1:{1}{2}", AllowedValuesSheetName, columnAddress, allowedValuesStrings.Length);

                XSSFDataValidationHelper dvHelper = new XSSFDataValidationHelper((XSSFSheet)worksheet);
                XSSFDataValidationConstraint dvConstraint = (XSSFDataValidationConstraint)
                  dvHelper.CreateFormulaListConstraint(namedCell.NameName);
                CellRangeAddressList addressList = new CellRangeAddressList(1, 10000, columnIndex, columnIndex);
                XSSFDataValidation validation = (XSSFDataValidation)dvHelper.CreateValidation(
                  dvConstraint, addressList);

                // Here the boolean value false is passed to the setSuppressDropDownArrow()
                // method. In the hssf.usermodel examples above, the value passed to this
                // method is true.            
                validation.SuppressDropDownArrow = false;

                // Note this extra method call. If this method call is omitted, or if the
                // boolean value false is passed, then Excel will not validate the value the
                // user enters into the cell.
                validation.ShowErrorBox = true;
                workbook.SetSheetHidden(workbook.GetSheetIndex(allowedValuesSheet), SheetState.Hidden);
                worksheet.AddValidationData(validation);
            }
        }
        #endregion

        #region CreateColumnAddressesArray()
        private static string[] CreateColumnAddressesArray()
        {
            string last = "XFD";
            string first = "";
            string second = "";
            string third = "A";
            string complet = "";
            List<string> columns = new List<string>();
            for (int i = 64; i < 91; i++)
            {
                if (i > 64)
                    first = ((char)i).ToString();
                for (int j = 64; j < 91; j++)
                {
                    if (j > 64)
                        second = ((char)j).ToString();
                    for (int k = 65; k < 91; k++)
                    {
                        third = ((char)k).ToString();
                        complet = first + second + third;
                        columns.Add(complet);
                        if (complet == last) break;
                    }
                    if (complet == last) break;
                }
                if (complet == last) break;
            }
            return columns.ToArray();
        }
        #endregion

        #region GetColumnIndex(string name, ISheet worksheet)
        private static int GetColumnIndex(string name, ISheet worksheet)
        {
            if (String.IsNullOrWhiteSpace(name))
                return -1;

            if (worksheet == null)
                return -1;

            IRow headerRow = worksheet.GetRow(0);

            if (headerRow == null)
                return -1;

            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                ICell cell = headerRow.GetCell(i);

                if (cell == null || String.IsNullOrWhiteSpace(cell.StringCellValue))
                    continue;

                if (cell.StringCellValue.Equals(name))
                    return i;
            }

            return -1;
        }
        #endregion

        #region GetColor(string colorName, short defaultColor)
        private static XSSFColor GetColor(string colorName, XSSFColor defaultColor)
        {
            if (String.IsNullOrWhiteSpace(colorName))
                return defaultColor;

            try
            {

                //TODO: vyhledat barvu podle jmena ...
                Color systemColor = Color.FromName(colorName);

                XSSFColor myColor = new XSSFColor(systemColor);

                return myColor;
            }
            catch
            {
                return defaultColor;
            }
        }
        #endregion

        #region CreateHeaders(XElement sheetElement, HSSFPalette palette, ISheet worksheet)
        private static void CreateHeaders(XElement sheetElement, ISheet worksheet)
        {
            if (sheetElement == null)
                return;

            XElement headersElement = sheetElement.FindElement("headers");

            if (headersElement == null)
                return;

            XSSFCellStyle headerCellStyle = worksheet.Workbook.CreateCellStyle() as XSSFCellStyle;
            XSSFFont tFont = new XSSFFont();
            tFont.SetColor(XmlToXlsxResolver.GetColor(headersElement.GetAttributeValue("headerTextColor", String.Empty), new XSSFColor(Color.White)));

            headerCellStyle.SetFillForegroundColor(XmlToXlsxResolver.GetColor(headersElement.GetAttributeValue("headerBackgroundColor", String.Empty), new XSSFColor(Color.White)));
            headerCellStyle.FillPattern = FillPattern.SolidForeground;
            headerCellStyle.SetFont(tFont);

            IRow row = worksheet.GetRow(0) ?? worksheet.CreateRow(0);

            int col = 0;
            foreach (XElement headerElement in headersElement.Elements("header"))
            {
                ICell cell = row.GetCell(col) ?? row.CreateCell(col, CellType.String);
                cell.SetCellValue(headerElement.GetAttributeValue("name", String.Empty));
                cell.CellStyle = headerCellStyle;

                col++;
            }
        }
        #endregion
    }
}
