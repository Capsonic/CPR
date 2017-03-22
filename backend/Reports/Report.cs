using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using OfficeOpenXml.Drawing;

namespace Reports
{

    public interface IReport
    {
        byte[] generate();
    }

    public abstract class Report : IReport
    {
        private string owner = "";

        private int rowIndex = 1;
        private int colIndex = 1;
        private int maxColIndex = 1;

        private int documentColOffset = 0;
        private int baseRowHeight = 12;

        private bool inTable = false;

        private ExcelRange insertValue(object value, string sType, int offset, TextAlign AlignMode = TextAlign.LEFT, Precision Decimals = Precision.ZERO, NumberTypes Type = NumberTypes.NUMBER, int mergeCols = 1, int? rowsOccuppied = null, float fontSize = 9, bool fontBold = false, ExcelVerticalAlignment VerticalAlign = ExcelVerticalAlignment.Center, int? col = null, int? row = null)
        {
            int targetColumn;
            int targetRow;

            if (col != null)
            {
                targetColumn = (int)col;
            }
            else
            {
                colIndex += offset;
                targetColumn = colIndex;
            }

            if (row != null)
            {
                targetRow = (int)row;
            }
            else
            {
                targetRow = rowIndex;
            }


            if (targetColumn > maxColIndex) maxColIndex = targetColumn;

            if (mergeCols > 1)
            {
                if (targetColumn + mergeCols > maxColIndex)
                {
                    maxColIndex = targetColumn + mergeCols;
                }
            }

            if (rowsOccuppied != null)
            {
                ws.Row(rowIndex).Height = (int)rowsOccuppied * baseRowHeight;
            }

            ExcelRange cell = ws.Cells[targetRow, targetColumn];

            if (inTable)
            {
                TD(cell);
            }

            switch (sType)
            {
                case "formula":
                    FormatNumber(cell, Type: Type, Decimals: Decimals);
                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else
                    {
                        cell.Formula = (string)value;
                    }
                    break;
                case "string":
                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    cell.Style.VerticalAlignment = VerticalAlign;
                    cell.Style.Font.Size = fontSize;
                    cell.Style.Font.Bold = fontBold;
                    if (string.IsNullOrWhiteSpace((string)value))
                    {
                        cell.Value = null;
                    }
                    else
                    {
                        cell.Value = (string)value;
                    }
                    break;
                case "paragraph":
                    if (mergeCols > 1)
                    {
                        cell = ws.Cells[cell.Address + ":" + cell.Offset(0, mergeCols).Address];
                        cell.Merge = true;
                    }
                    cell.Style.WrapText = true;

                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else
                    {
                        cell.Value = (string)value;
                    }
                    break;
                case "number":
                case "double":
                    FormatNumber(cell, Type: NumberTypes.NUMBER, Decimals: Decimals);
                    if (value == null)
                    {
                        cell.Value = 0;
                    }
                    else
                    {
                        cell.Value = value;
                        try
                        {
                            if ((decimal)(double)value < 0)
                            {
                                cell.Style.Font.Color.SetColor(Color.Red);
                            }
                        }
                        catch { }
                    }
                    break;
                case "currency":
                    FormatNumber(cell, Type: NumberTypes.CURRENCY, Decimals: Decimals);
                    if (value == null)
                    {
                        cell.Value = 0;
                    }
                    else
                    {
                        cell.Value = value;
                        try
                        {
                            if ((decimal)(double)value < 0)
                            {
                                cell.Style.Font.Color.SetColor(Color.Red);
                            }
                        }
                        catch { }
                    }
                    break;
                case "percentage":
                    FormatNumber(cell, Type: NumberTypes.PERCENTAGE, Decimals: Decimals);

                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }

                    if (value == null)
                    {
                        cell.Value = 0;
                    }
                    else
                    {
                        cell.Value = (decimal)value;
                        try
                        {
                            if ((decimal)(double)value < 0)
                            {
                                cell.Style.Font.Color.SetColor(Color.Red);
                            }
                        }
                        catch { }
                    }
                    break;
                case "title":
                    cell.Style.Font.Size = 18;
                    ws.Row(rowIndex).Height = 23;

                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    cell.Value = (string)value;
                    break;
                case "subtitle":
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 14;
                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    cell.Value = (string)value;
                    break;
                case "label":
                    cell.Style.Font.Bold = true;
                    cell.Style.WrapText = true;
                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    cell.Value = (string)value;
                    break;
                case "date":
                    cell.Style.Numberformat.Format = "dd-mmm-yyyy";

                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else
                    {
                        DateTime theDate = (DateTime)value;
                        cell.Formula = "=DATE(" + theDate.Year + "," + theDate.Month + "," + theDate.Day + ")";
                    }
                    break;
                case "datetime":
                    cell.Style.Numberformat.Format = "dd-mmm-yyyy h:mm";

                    switch (AlignMode)
                    {
                        case TextAlign.LEFT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            break;
                        case TextAlign.CENTER:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case TextAlign.RIGHT:
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            break;
                        default:
                            break;
                    }
                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else
                    {
                        DateTime theDate = (DateTime)value;
                        cell.Formula = "=DATE(" + theDate.Year + "," + theDate.Month + "," + theDate.Day + ") + TIME(" + theDate.Hour + "," + theDate.Minute + "," + theDate.Second + ")";
                    }
                    break;
                case "bool":
                    cell.Value = value;
                    break;
            }
            if (col == null && row == null)
            {
                tab();
            }

            return cell;
        }

        private void tab()
        {
            colIndex++;
        }


        /****************************************
         *                  API:                *
         ****************************************/
        protected ExcelPackage p;
        protected ExcelWorksheet ws;

        protected enum TextAlign { LEFT, CENTER, RIGHT };
        protected enum Precision { ZERO, ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN };
        protected enum NumberTypes { NUMBER, CURRENCY, PERCENTAGE };

        //Constructor
        protected Report(string Owner)
        {
            owner = Owner;
        }

        //Init
        protected void InitWorkBook(string name)
        {
            //Here setting some document properties
            p.Workbook.Properties.Author = owner;
            p.Workbook.Properties.Title = name;
        }
        protected ExcelWorksheet CreateWorkSheet(string name, int baseRowHeight = 12, int documentColOffset = 0, eOrientation orientation = eOrientation.Portrait)
        {
            //Create a sheet
            p.Workbook.Worksheets.Add(name);
            ExcelWorksheet wsNewOne = p.Workbook.Worksheets[name];
            wsNewOne.Name = name; //Setting Sheet's name
            wsNewOne.Cells.Style.Font.Size = 9; //Default font size for whole sheet
            wsNewOne.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
            wsNewOne.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            wsNewOne.PrinterSettings.PrintArea = null;
            wsNewOne.PrinterSettings.TopMargin = .02M;
            wsNewOne.PrinterSettings.LeftMargin = .02M;
            wsNewOne.PrinterSettings.BottomMargin = .02M;
            wsNewOne.PrinterSettings.RightMargin = .02M;
            wsNewOne.PrinterSettings.HorizontalCentered = true;

            wsNewOne.PrinterSettings.Orientation = orientation;

            this.baseRowHeight = baseRowHeight;
            wsNewOne.DefaultRowHeight = baseRowHeight;

            this.documentColOffset = documentColOffset;
            colIndex += documentColOffset;
            //wsNewOne.Cells.Style.Locked = true;
            //wsNewOne.Protection.IsProtected = true;
            this.ws = wsNewOne;
            return wsNewOne;
        }

        //Misc
        protected void NewLine(int offset = 1)
        {
            rowIndex += offset;
            colIndex = 1 + documentColOffset;
        }
        protected ExcelRange FormatNumber(ExcelRange cell, NumberTypes Type = NumberTypes.NUMBER, Precision Decimals = Precision.ZERO)
        {
            switch (Type)
            {
                case NumberTypes.CURRENCY:
                    switch (Decimals)
                    {
                        case Precision.ZERO:
                            cell.Style.Numberformat.Format = "_($* #,##0_);_($* (#,##0);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.ONE:
                            cell.Style.Numberformat.Format = "_($* #,##0.0_);_($* (#,##0.0);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.TWO:
                            cell.Style.Numberformat.Format = "_($* #,##0.00_);_($* (#,##0.00);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.THREE:
                            cell.Style.Numberformat.Format = "_($* #,##0.000_);_($* (#,##0.000);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.FOUR:
                            cell.Style.Numberformat.Format = "_($* #,##0.0000_);_($* (#,##0.0000);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.FIVE:
                            cell.Style.Numberformat.Format = "_($* #,##0.00000_);_($* (#,##0.00000);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.SIX:
                            cell.Style.Numberformat.Format = "_($* #,##0.000000_);_($* (#,##0.000000);_($* \"-\"??_);_(@_)";
                            break;
                        case Precision.SEVEN:
                            cell.Style.Numberformat.Format = "_($* #,##0.0000000_);_($* (#,##0.0000000);_($* \"-\"??_);_(@_)";
                            break;
                        default:
                            cell.Style.Numberformat.Format = "_($* #,##0.00000000_);_($* (#,##0.00000000);_($* \"-\"??_);_(@_)";
                            break;
                    }
                    break;
                case NumberTypes.PERCENTAGE:
                    switch (Decimals)
                    {
                        case Precision.ZERO:
                            cell.Style.Numberformat.Format = "#,##0 %";
                            break;
                        case Precision.ONE:
                            cell.Style.Numberformat.Format = "#,##0.0 %";
                            break;
                        case Precision.TWO:
                            cell.Style.Numberformat.Format = "#,##0.00 %";
                            break;
                        case Precision.THREE:
                            cell.Style.Numberformat.Format = "#,##0.000 %";
                            break;
                        case Precision.FOUR:
                            cell.Style.Numberformat.Format = "#,##0.0000 %";
                            break;
                        case Precision.FIVE:
                            cell.Style.Numberformat.Format = "#,##0.00000 %";
                            break;
                        case Precision.SIX:
                            cell.Style.Numberformat.Format = "#,##0.000000 %";
                            break;
                        case Precision.SEVEN:
                            cell.Style.Numberformat.Format = "#,##0.0000000 %";
                            break;
                        default:
                            cell.Style.Numberformat.Format = "#,##0 %";
                            break;
                    }
                    break;
                case NumberTypes.NUMBER:
                default:
                    switch (Decimals)
                    {
                        case Precision.ZERO:
                            cell.Style.Numberformat.Format = "###,###,##0";
                            break;
                        case Precision.ONE:
                            cell.Style.Numberformat.Format = "###,###,##0.0";
                            break;
                        case Precision.TWO:
                            cell.Style.Numberformat.Format = "###,###,##0.00";
                            break;
                        case Precision.THREE:
                            cell.Style.Numberformat.Format = "###,###,##0.000";
                            break;
                        case Precision.FOUR:
                            cell.Style.Numberformat.Format = "###,###,##0.0000";
                            break;
                        case Precision.FIVE:
                            cell.Style.Numberformat.Format = "###,###,##0.00000";
                            break;
                        case Precision.SIX:
                            cell.Style.Numberformat.Format = "###,###,##0.000000";
                            break;
                        case Precision.SEVEN:
                            cell.Style.Numberformat.Format = "###,###,##0.0000000";
                            break;
                        default:
                            cell.Style.Numberformat.Format = "###,###,##0";
                            break;
                    }
                    break;
            }
            return cell;
        }
        protected ExcelRange TD(ExcelRange cell)
        {
            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

            cell.Style.Border.Top.Color.SetColor(Color.FromArgb(204, 204, 204));
            cell.Style.Border.Right.Color.SetColor(Color.FromArgb(204, 204, 204));
            cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(204, 204, 204));
            cell.Style.Border.Left.Color.SetColor(Color.FromArgb(204, 204, 204));

            return cell;
        }
        protected void pageBreak()
        {
            ws.Row(rowIndex).PageBreak = true;
            NewLine();
        }
        protected void deduceColPageBreak()
        {
            for (int i = 1; i < maxColIndex; i++)
            {
                ws.Column(i).PageBreak = false;
            }
            if (rowIndex > 1)
            {
                ws.PrinterSettings.PrintArea = ws.Cells["A1:" + ws.Cells[rowIndex - 1, maxColIndex].Address];
                ws.PrinterSettings.FitToPage = true;
                ws.PrinterSettings.FitToHeight = 0;
            }
        }
        protected void TableStarts()
        {
            inTable = true;
        }
        protected void TableEnds()
        {
            inTable = false;
        }
        //protected ExcelRange Format(ExcelRange cell, TextAlign AlignMode = TextAlign.LEFT, Precision Decimals = Precision.ZERO, NumberTypes Type = NumberTypes.NUMBER, int mergeCols = 1, int? rowsOccuppied = null, float fontSize = 9, bool fontBold = false, ExcelVerticalAlignment VerticalAlign = ExcelVerticalAlignment.Center)
        //{

        //    return cell;
        //}

        //Insert Values
        protected ExcelRange InsertDate(DateTime? value, int colOffset = 0, TextAlign AlignMode = TextAlign.CENTER, int? col = null, int? row = null)
        {
            return insertValue(value, "date", colOffset, AlignMode, col: col, row: row);
        }
        protected ExcelRange InsertDateTime(DateTime? value, int colOffset = 0, TextAlign AlignMode = TextAlign.CENTER, int? col = null, int? row = null)
        {
            return insertValue(value, "datetime", colOffset, AlignMode, col: col, row: row);
        }
        protected ExcelRange InsertTitle(string value, int colOffset = 0, TextAlign AlignMode = TextAlign.LEFT, int? col = null, int? row = null)
        {
            return insertValue(value, "title", colOffset, AlignMode, col: col, row: row);
        }
        protected ExcelRange InsertSubtitle(string value, int colOffset = 0, TextAlign AlignMode = TextAlign.LEFT, int? col = null, int? row = null)
        {
            return insertValue(value, "subtitle", colOffset, AlignMode, col: col, row: row);
        }
        protected ExcelRange InsertLabel(string value, int colOffset = 0, TextAlign AlignMode = TextAlign.RIGHT, int? col = null, int? row = null)
        {
            return insertValue(value, "label", colOffset, AlignMode, col: col, row: row);
        }
        protected ExcelRange InsertCurrency(decimal? value, int colOffset = 0, Precision Decimals = Precision.TWO, int? col = null, int? row = null)
        {
            return insertValue(value, "currency", colOffset, Decimals: Decimals, col: col, row: row);
        }
        protected ExcelRange InsertCurrency(double value, int colOffset = 0, Precision Decimals = Precision.TWO, int? col = null, int? row = null)
        {
            return insertValue(value, "currency", colOffset, Decimals: Decimals, col: col, row: row);
        }
        protected ExcelRange InsertString(string value, int colOffset = 0, TextAlign AlignMode = TextAlign.LEFT, float fontSize = 9, bool fontBold = false, ExcelVerticalAlignment VerticalAlign = ExcelVerticalAlignment.Center, int? col = null, int? row = null)
        {
            return insertValue(value, "string", colOffset, AlignMode, fontSize: fontSize, fontBold: fontBold, VerticalAlign: VerticalAlign, col: col, row: row);
        }
        protected ExcelRange InsertNumber(decimal? value, int colOffset = 0, Precision Decimals = Precision.ZERO, int? col = null, int? row = null)
        {
            return insertValue(value, "number", colOffset, Decimals: Decimals, col: col, row: row);
        }
        protected ExcelRange InsertNumber(double value, int colOffset = 0, Precision Decimals = Precision.ZERO, int? col = null, int? row = null)
        {
            return insertValue(value, "double", colOffset, Decimals: Decimals, col: col, row: row);
        }
        protected ExcelRange InsertPercentage(decimal? value, int colOffset = 0, Precision Decimals = Precision.ZERO, TextAlign AlignMode = TextAlign.LEFT, int? col = null, int? row = null)
        {
            return insertValue(value, "percentage", colOffset, AlignMode, Decimals: Decimals, col: col, row: row);
        }
        protected ExcelRange InsertParagraph(string value, int colOffset = 0, TextAlign AlignMode = TextAlign.LEFT, int mergeCols = 1, int? rowsOccuppied = null)
        {
            return insertValue(value, "paragraph", colOffset, AlignMode, mergeCols: mergeCols, rowsOccuppied: rowsOccuppied);
        }
        protected void InsertImage(Bitmap theImage, int colOffset = 0, int colsOccuppied = 1, int sizePercentage = 100, int rowHeight = 12, int rowOffsetPixels = 0, int colOffsetPixels = 0)
        {
            ExcelPicture pic = ws.Drawings.AddPicture("pic" + (rowIndex).ToString(), theImage);

            colIndex += colOffset;
            if (colsOccuppied + colIndex > maxColIndex) maxColIndex = colsOccuppied + colIndex;
            //pic.From.Row = rowIndex - 1;
            //pic.From.Column = colIndex - 1;

            pic.SetPosition(rowIndex - 1, rowOffsetPixels, colIndex - 1, colOffsetPixels);

            pic.SetSize(sizePercentage);
            ws.Row(rowIndex).Height = rowHeight;
            colIndex += colsOccuppied;
        }
        protected ExcelRange InsertBool(bool? value, int colOffset = 0, int? col = null, int? row = null)
        {
            return insertValue(value, "bool", colOffset, col: col, row: row);
        }


        //Exclusive Excel
        protected int CurrentRow()
        {
            return rowIndex;
        }
        protected void CurrentRow(int row)
        {
            rowIndex = row;
        }
        protected ExcelRange CurrentCell()
        {
            return ws.Cells[rowIndex, colIndex];
        }
        protected void CurrentCol(int col)
        {
            colIndex = col;
        }
        protected int CurrentCol()
        {
            return colIndex;
        }
        private string ColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
        protected void freeze()
        {
            ws.View.FreezePanes(rowIndex, 1);
        }
        protected ExcelRange UnprotectCell(ExcelRange cell)
        {
            cell.Style.Locked = false;

            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

            cell.Style.Border.Top.Color.SetColor(Color.FromArgb(255, 215, 0));
            cell.Style.Border.Right.Color.SetColor(Color.FromArgb(255, 215, 0));
            cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(255, 215, 0));
            cell.Style.Border.Left.Color.SetColor(Color.FromArgb(255, 215, 0));

            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 248, 200));

            return cell;
        }
        protected ExcelRange InsertFormula(string value, int colOffset = 0, Precision Decimals = Precision.ZERO, NumberTypes Type = NumberTypes.NUMBER, int? col = null, int? row = null)
        {
            return insertValue(value, "formula", colOffset, Decimals: Decimals, Type: Type, col: col, row: row);
        }
        protected ExcelRange InsertSUM(ExcelRange cellFrom)
        {
            ExcelRange cell = CurrentCell();
            if (cellFrom == null)
            {
                return CurrentCell();
            }
            InsertFormula("SUM(" + cellFrom.Address + ":" + cell.Offset(-1, 0).Address + ")");
            cell.Style.Numberformat.Format = cellFrom.Style.Numberformat.Format;
            cell.Style.HorizontalAlignment = cellFrom.Style.HorizontalAlignment;
            return cell;
        }

        protected ExcelRange Decorate(ExcelRange cell, Style style)
        {
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(style.BackgroundColor);
            return cell;
        }

        protected class Style
        {
            public Color BackgroundColor { get; set; }
        }

        public string DebugErrorHelper { get; set; }

        protected abstract void define();
        public byte[] generate()
        {
            try
            {
                using (p = new ExcelPackage())
                {
                    define();
                    return p.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                DebugErrorHelper = ex.Message;
                throw;
            }
        }
    }
}
