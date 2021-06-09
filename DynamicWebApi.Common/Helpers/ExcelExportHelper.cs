/* Copyright Chetan N Mandhania */
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace DynamicWebApi.Common.Helpers
{
    [DebuggerStepThrough]
    public static class ExcelExportHelper
    {
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static string GetXMLFromCSVByteArray(byte[] fileData)
        {
            using (MemoryStream ms = new(fileData))
            {
                BinaryFormatter f = new();
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                var dt = f.Deserialize(ms) as DataTable;
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                using (var Writer = new StringWriter())
                {
                    dt.WriteXml(Writer);
                    return Writer.ToString();
                }
            }
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static string GetXMLFromExcelByteArray(byte[] fileData, string sheetName = "")
        {
            using (var Writer = new StringWriter())
            {
                ImportExcelToDataTable(fileData, sheetName).WriteXml(Writer);
                return Writer.ToString();
            }
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static byte[] GenerateExcel(DataTable dt)
        {
            var ds = new DataSet();
            ds.Tables.Add(dt);
            return GenerateExcel(ds);
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static byte[] GenerateExcel(DataSet ds)
        {
            using (MemoryStream ms = new())
            {
                using (var workbook = SpreadsheetDocument.Create(ms, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new();
                    workbook.WorkbookPart.Workbook.Sheets = new();
                    foreach (DataTable table in ds.Tables)
                    {
                        var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData = new();
                        sheetPart.Worksheet = new(sheetData);
                        Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                        string relationShipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);
                        uint sheetId = 1;
                        if (sheets.Elements<Sheet>().Any()) sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                        Sheet sheet = new() { Id = relationShipId, SheetId = sheetId, Name = table.TableName };
                        sheets.Append(sheet);
                        Row headerRow = new();
                        List<string> columns = new();
                        foreach (DataColumn col in table.Columns)
                        {
                            columns.Add(col.ColumnName);
                            Cell cell = new();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new(col.ColumnName);
                            headerRow.AppendChild(cell);
                        }
                        sheetData.AppendChild(headerRow);
                        foreach (DataRow dsRow in table.Rows)
                        {
                            Row newrow = new();
                            foreach (string col in columns)
                            {
                                Cell cell = new();
                                cell.DataType = CellValues.String;
                                cell.CellValue = new(dsRow[col] + ""); ;
                                newrow.AppendChild(cell);
                            }
                            sheetData.AppendChild(newrow);
                        }
                    }
                }
                return ms.ToArray();
            }
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static DataTable ImportExcelToDataTable(byte[] fileData, string sheetName = "")
        {
            try
            {
                if (null != fileData && fileData.Length > 0)
                {
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(new MemoryStream(fileData), false))
                    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                        Sheet? sheet = null;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
                        if (!string.IsNullOrEmpty(sheetName))
                        {
                            int sheetIndex = 0;
                            foreach (WorksheetPart workSheetPart in doc.WorkbookPart.WorksheetParts)
                            {
                                sheet = doc.WorkbookPart.Workbook.Descendants<Sheet>().ElementAt(sheetIndex);
                                if (sheet.Name == sheetName) break;
                                sheetIndex++;
                            }
                        }
                        else doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
                        Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
                        IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
                        DataTable dt = new("Table");
                        foreach (Cell cell in rows.ElementAt(0))
                        {
                            var value = GetCellValue(doc, cell);
                            if (cell.DataType == CellValues.Date) dt.Columns.Add(value, typeof(DateTime));
                            else if (cell.DataType == CellValues.Number) dt.Columns.Add(value, typeof(double));
                            else if (cell.DataType == CellValues.Boolean) dt.Columns.Add(value, typeof(Boolean));
                            else if (cell.DataType == CellValues.SharedString) dt.Columns.Add(value, typeof(String));

                        }
                        int i = 0;
                        foreach (var row in rows)
                        {
                            i++;
                            if (i - 1 == 0 || string.IsNullOrEmpty(row.InnerText)) continue;
                            DataRow tempRow = dt.NewRow();
                            int columnIndex = 0;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
                                cellColumnIndex--;
                                if (columnIndex < cellColumnIndex)
                                {
                                    do
                                    {
                                        tempRow[columnIndex] = "";
                                        columnIndex++;
                                    } while (columnIndex < cellColumnIndex);
                                }
                                var value = GetCellValue(doc, cell);
                                if (columnIndex < dt.Columns.Count)
                                {
                                    if (dt.Columns[columnIndex].DataType == typeof(DateTime))
                                    {
                                        try { tempRow[columnIndex] = value; }
                                        catch { tempRow[columnIndex] = DateTime.FromOADate(Convert.ToDouble(value)); }
                                    }
                                }
                                else tempRow[columnIndex] = value;
                                columnIndex++;
                            }
                            dt.Rows.Add(tempRow);
                        }
                        return dt;
                    }
                }
                else
                {
                    return default(DataTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failure reading data from excel : {Environment.NewLine} Error : {ex.Message}");
            }
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        private static int? GetColumnIndexFromName(string columnName)
        {
            int number = 0;
            int pow = 1;
            foreach (var v in columnName)
            {
                number += (v - 'A' + 1) * pow;
                pow *= 26;
            }
            return number;
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        private static string GetColumnName(DocumentFormat.OpenXml.StringValue cellReference) => new Regex("[A-Za-z]+").Match(cellReference).Value;
        [DebuggerStepThrough]
        [DebuggerHidden]
        private static string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            SharedStringTablePart stringTablePart = doc.WorkbookPart.SharedStringTablePart;
            if (null == cell.CellValue) return "";
            string value = cell.CellValue.InnerXml;
            if (null != cell.DataType && cell.DataType == CellValues.SharedString) return stringTablePart.SharedStringTable.ChildElements[Convert.ToInt32(value)].InnerText;
            else return value;
        }
    }
}
