using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.CreateFile
{
    public class CreateExcelService
    {
        public static async Task<CreateCardListResult> CreateExcelYGOCommand(string filePath, bool hasFlag = false)
        {
            CreateCardListResult result = new();

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Result = false;
                    result.Messenger = CMess.fileNotExit.ToText();
                    return result;
                }

                string excelExtension = ConstantExtension.ExcelExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string excelFilePath;
                if (ConstantExtension.ExcelExtensions.Contains(extension)) excelFilePath = filePath;
                else excelFilePath = Path.ChangeExtension(filePath, excelExtension);

                string folderPath = System.IO.Path.GetDirectoryName(excelFilePath);
                string xlsxFileName = System.IO.Path.GetFileName(excelFilePath);

                if (hasFlag) result = await Task.Run(() => CreateExcelYGONoFlagTemplate(folderPath, xlsxFileName));
                else result = await Task.Run(() => CreateExcelYGOWithFlagTemplate(folderPath, xlsxFileName));
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateCardListResult CreateExcelYGONoFlagTemplate(string folderPath, string xlsxFilename)
        {
            CreateCardListResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(xlsxFilename))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string xlsxFilePath = Path.Combine(folderPath, xlsxFilename);
            string tempXlsxFilePath = xlsxFilePath + ".tmp.xlsx";

            try
            {
                string[] headers = ConstantColumnExcel.HeaderNoFlag;
                int numberColumn = headers.Length;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    // ==========================================
                    // Format toàn bộ các cột
                    // ==========================================

                    // A - CardEditorX: Integer
                    worksheet.Column(1).Style.NumberFormat.Format = "0";

                    // B:D - name, desc, ot: Text
                    worksheet.Columns(2, 4).Style.NumberFormat.Format = "@";

                    // E - alias: Integer
                    worksheet.Column(5).Style.NumberFormat.Format = "0";

                    // F:G - setcode, type: Text
                    worksheet.Columns(6, 7).Style.NumberFormat.Format = "@";

                    // H:I - atk, def: Integer
                    worksheet.Columns(8, 9).Style.NumberFormat.Format = "0";

                    // J -> cuối: Text
                    if (numberColumn >= 10)
                    {
                        worksheet.Columns(10, numberColumn).Style.NumberFormat.Format = "@";
                    }

                    // ==========================================
                    // Row 1 trở về format mặc định
                    // ==========================================

                    worksheet.Row(1).Style.NumberFormat.Format = "";

                    // ==========================================
                    // Ghi Header vào Row 1
                    // ==========================================

                    for (int i = 0; i < numberColumn; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    workbook.SaveAs(tempXlsxFilePath);
                }

                if (File.Exists(xlsxFilePath)) File.Delete(xlsxFilePath);
                File.Move(tempXlsxFilePath, xlsxFilePath);

                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = xlsxFilePath;
                result.Format = CardListFormat.YGONoFlag;
                result.HasFlag = false;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempXlsxFilePath)) File.Delete(tempXlsxFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateCardListResult CreateExcelYGOWithFlagTemplate(string folderPath, string xlsxFilename)
        {
            CreateCardListResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(xlsxFilename))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string xlsxFilePath = Path.Combine(folderPath, xlsxFilename);
            string tempXlsxFilePath = xlsxFilePath + ".tmp.xlsx";

            try
            {
                string[] headers = ConstantColumnExcel.HeaderWithFlag;
                int numberColumn = headers.Length;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    // ==========================================
                    // Format toàn bộ các cột
                    // ==========================================

                    // A - CardEditorX: Integer
                    worksheet.Column(1).Style.NumberFormat.Format = "0";

                    // B:D - name, desc, ot: Text
                    worksheet.Columns(2, 4).Style.NumberFormat.Format = "@";

                    // E - alias: Integer
                    worksheet.Column(5).Style.NumberFormat.Format = "0";

                    // F:G - setcode, type: Text
                    worksheet.Columns(6, 7).Style.NumberFormat.Format = "@";

                    // H:I - atk, def: Integer
                    worksheet.Columns(8, 9).Style.NumberFormat.Format = "0";

                    // J -> cuối - Text
                    if (numberColumn >= 10)
                    {
                        worksheet.Columns(10, numberColumn).Style.NumberFormat.Format = "@";
                    }

                    // ==========================================
                    // Row 1 trở về format mặc định
                    // ==========================================

                    worksheet.Row(1).Style.NumberFormat.Format = "";

                    // ==========================================
                    // Ghi Header vào Row 1
                    // ==========================================

                    for (int i = 0; i < numberColumn; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    workbook.SaveAs(tempXlsxFilePath);
                }

                if (File.Exists(xlsxFilePath)) File.Delete(xlsxFilePath);

                File.Move(tempXlsxFilePath, xlsxFilePath);

                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = xlsxFilePath;
                result.Format = CardListFormat.YGOHasFlag;
                result.HasFlag = true;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempXlsxFilePath)) File.Delete(tempXlsxFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }

        public static async Task<CreateCardListResult> CreateExcelOMEGACommand(string filePath)
        {
            CreateCardListResult result = new();

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Result = false;
                    result.Messenger = CMess.fileNotExit.ToText();
                    return result;
                }

                string excelExtension = ConstantExtension.ExcelExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string excelFilePath;
                if (ConstantExtension.ExcelExtensions.Contains(extension)) excelFilePath = filePath;
                else excelFilePath = Path.ChangeExtension(filePath, excelExtension);

                string folderPath = System.IO.Path.GetDirectoryName(excelFilePath);
                string xlsxFileName = System.IO.Path.GetFileName(excelFilePath);

                result = await Task.Run(() => CreateExcelOMEGATemplate(folderPath, xlsxFileName));
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateCardListResult CreateExcelOMEGATemplate(string folderPath, string xlsxFilename)
        {
            CreateCardListResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(xlsxFilename))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string xlsxFilePath = Path.Combine(folderPath, xlsxFilename);
            string tempXlsxFilePath = xlsxFilePath + ".tmp.xlsx";

            try
            {
                string[] headers = ConstantColumnExcel.HeaderOmega;
                int numberColumn = headers.Length;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    // ==========================================
                    // Format toàn bộ các cột
                    // ==========================================

                    // A - CardEditorX: Integer
                    worksheet.Column(1).Style.NumberFormat.Format = "0";

                    // B:D - name, desc, ot: Text
                    worksheet.Columns(2, 4).Style.NumberFormat.Format = "@";

                    // E - alias: Integer
                    worksheet.Column(5).Style.NumberFormat.Format = "0";

                    // F:G - setcode, type: Text
                    worksheet.Columns(6, 7).Style.NumberFormat.Format = "@";

                    // H:I - atk, def: Integer
                    worksheet.Columns(8, 9).Style.NumberFormat.Format = "0";

                    // J -> cuối - Text
                    if (numberColumn >= 10)
                    {
                        worksheet.Columns(10, numberColumn).Style.NumberFormat.Format = "@";
                    }

                    // ==========================================
                    // Row 1 trở về format mặc định
                    // ==========================================

                    worksheet.Row(1).Style.NumberFormat.Format = "";

                    // ==========================================
                    // Ghi Header vào Row 1
                    // ==========================================

                    for (int i = 0; i < numberColumn; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }

                    workbook.SaveAs(tempXlsxFilePath);
                }

                if (File.Exists(xlsxFilePath)) File.Delete(xlsxFilePath);
                File.Move(tempXlsxFilePath, xlsxFilePath);

                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = xlsxFilePath;
                result.Format = CardListFormat.OMEGA;
                result.HasFlag = true;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempXlsxFilePath)) File.Delete(tempXlsxFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }

    }
}
