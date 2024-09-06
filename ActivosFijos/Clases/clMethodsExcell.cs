using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;

namespace ActivosFijos.Clases
{
    public static class clMethodsExcell
    {
        public static DataTable GetDataTableFromSpreadsheet(Stream MyExcelStream, bool ReadOnly)
        {
            DataTable dt = new DataTable();
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
            {
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(2))
                {
                    string getCellV = GetCellValue(sDoc, cell);

                    if (getCellV != null)
                        dt.Columns.Add(getCellV);
                }

                rows = rows.Skip(2);

                foreach (Row row in rows) //this will also include your header row...
                {
                    DataRow tempRow = dt.NewRow();
                    int j = 0;
                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        string getCellV = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                        if (getCellV != null)
                        {
                            tempRow[j] = getCellV;
                            j++;
                        }
                    }

                    dt.Rows.Add(tempRow);
                }
            }
            dt.Rows.RemoveAt(0);
            return dt;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue != null ? cell.CellValue.InnerXml : null;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        public static string ConvertDataTableToHTMLTable(DataTable dt)
        {
            string ret = "<p>AR####</p> ";
            ret += "<table id=" + (char)34 + "tblExcel" + (char)34 + ">";
            ret += "<tr>";
            foreach (DataColumn col in dt.Columns)
            {
                ret += "<td class=" + (char)34 + "tdColumnHeader" + (char)34 + ">" + col.ColumnName + "</td>";
            }
            ret += "</tr>";

            int countRows = 0;
            string valueGroupedRow = String.Empty;

            foreach (DataRow row in dt.Rows)
            {
                if (row[0].ToString().Contains("-  Área de responsabilidad: "))
                {
                    if (countRows > 0)
                    {
                        ret = ret.Replace("AR####", valueGroupedRow + " " + countRows.ToString());

                        break;
                    }

                    valueGroupedRow = row[0].ToString() + row[1].ToString();
                    //ret += "<tr col style=" + (char)34 + "background-color: #f7e8e8;" + (char)34 + ">";
                    //ret += "<td colspan=" + (char)34 + "13" + (char)34 + " class=" + (char)34 + "tdCellData" + (char)34 + ">" + valueGroupedRow + " (####)</td>";

                    countRows = 0;
                }
                else
                {
                    ret += "<tr>";

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        ret += "<td class=" + (char)34 + "tdCellData" + (char)34 + ">" + row[i].ToString() + "</td>";
                    }

                    countRows++;
                }

                ret += "</tr>";
            }
            ret += "</table>";
            return ret;
        }
    }
}
