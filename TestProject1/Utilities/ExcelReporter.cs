using ClosedXML.Excel;
using System;
using System.IO;

namespace TestProject1.Utilities
{
    public static class ExcelReporter
    {
        // Thay bằng đường dẫn thực tế trên máy bạn
        private static readonly string ReportPath = @"D:\BDCLPM\DoanCStar_final_1\TestProject1\bin\Debug\net9.0\Reports\BDCLPM_ggsheet.xlsx";
        private const string SheetName = "duy_testcase";

        // Chỉ số cột (1-based)
        private const int ColTestcaseId = 1;
        private const int ColActualResult = 9;
        private const int ColResult = 10;
        private const int ColNote = 11;

        public static void WriteResult(string testcaseId, string actualResult, string result, string note = "")
        {
            if (!File.Exists(ReportPath))
                throw new FileNotFoundException($"Không tìm thấy file: {ReportPath}");

            using var wb = new XLWorkbook(ReportPath);

            if (!wb.TryGetWorksheet(SheetName, out var ws))
                throw new Exception($"Không tìm thấy sheet \"{SheetName}\"");

            // Tìm dòng có Testcase ID khớp
            int targetRow = -1;
            foreach (var row in ws.RowsUsed())
            {
                var cellVal = row.Cell(ColTestcaseId).GetString().Trim();
                if (cellVal.Equals(testcaseId, StringComparison.OrdinalIgnoreCase))
                {
                    targetRow = row.RowNumber();
                    break;
                }
            }

            if (targetRow == -1)
            {
                Console.WriteLine($"⚠️ Không tìm thấy Testcase ID \"{testcaseId}\" trong sheet.");
                return;
            }

            ws.Cell(targetRow, ColActualResult).Value = actualResult;
            ws.Cell(targetRow, ColResult).Value = result;
            ws.Cell(targetRow, ColNote).Value = note;

            // Style cột Result
            var resultCell = ws.Cell(targetRow, ColResult);
            resultCell.Style.Font.Bold = true;
            resultCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            if (result == "PASS")
            {
                resultCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#375623");
                resultCell.Style.Font.FontColor = XLColor.White;
            }
            else if (result == "FAIL")
            {
                resultCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#C00000");
                resultCell.Style.Font.FontColor = XLColor.White;
            }

            // Style Actual Result và Note
            ws.Cell(targetRow, ColActualResult).Style.Alignment.WrapText = true;
            ws.Cell(targetRow, ColNote).Style.Alignment.WrapText = true;

            wb.Save();
            Console.WriteLine($"✅ Đã ghi kết quả vào dòng {targetRow} ({testcaseId}) — {result}");
        }
    }
}