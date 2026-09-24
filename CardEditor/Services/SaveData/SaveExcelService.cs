using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Services.CheckData;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.SaveData
{
    public class SaveExcelYGOService
    {
        private static string[] GetHeaders(bool hasFlag) => hasFlag ? ConstantColumnExcel.HeaderWithFlag.ToArray() : ConstantColumnExcel.HeaderNoFlag.ToArray();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new(StringComparer.OrdinalIgnoreCase);
        private static SemaphoreSlim GetLock(string filePath) => _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));

        private static void WriteCardToRow(IXLWorksheet ws, int row, Card card, bool hasFlag)
        {
            ws.Cell(row, 1).SetValue(card.id.ToString());
            ws.Cell(row, 2).SetValue(card.name);
            ws.Cell(row, 3).SetValue(card.desc);
            ws.Cell(row, 4).SetValue(card.ot.ToString());
            ws.Cell(row, 5).SetValue(card.alias.ToString());
            ws.Cell(row, 6).SetValue(card.setcode.ToString());
            ws.Cell(row, 7).SetValue(card.type.ToString());
            ws.Cell(row, 8).SetValue(card.atk.ToString());
            ws.Cell(row, 9).SetValue(card.def.ToString());
            ws.Cell(row, 10).SetValue(card.level.ToString());
            ws.Cell(row, 11).SetValue(card.race.ToString());
            ws.Cell(row, 12).SetValue(card.attribute.ToString());
            ws.Cell(row, 13).SetValue(card.category.ToString());

            int strOffset = 13;
            if (hasFlag)
            {
                ws.Cell(row, 14).SetValue(card.flag.ToString());
                strOffset = 14;
            }

            ws.Cell(row, strOffset + 1).SetValue(card.str1);
            ws.Cell(row, strOffset + 2).SetValue(card.str2);
            ws.Cell(row, strOffset + 3).SetValue(card.str3);
            ws.Cell(row, strOffset + 4).SetValue(card.str4);
            ws.Cell(row, strOffset + 5).SetValue(card.str5);
            ws.Cell(row, strOffset + 6).SetValue(card.str6);
            ws.Cell(row, strOffset + 7).SetValue(card.str7);
            ws.Cell(row, strOffset + 8).SetValue(card.str8);
            ws.Cell(row, strOffset + 9).SetValue(card.str9);
            ws.Cell(row, strOffset + 10).SetValue(card.str10);
            ws.Cell(row, strOffset + 11).SetValue(card.str11);
            ws.Cell(row, strOffset + 12).SetValue(card.str12);
            ws.Cell(row, strOffset + 13).SetValue(card.str13);
            ws.Cell(row, strOffset + 14).SetValue(card.str14);
            ws.Cell(row, strOffset + 15).SetValue(card.str15);
            ws.Cell(row, strOffset + 16).SetValue(card.str16);
        }

        private static int FindRowById(IXLWorksheet ws, ulong id, int lastRow)
        {
            for (int row = 2; row <= lastRow; row++)
            {
                var cell = ws.Cell(row, 1);
                if (!cell.IsEmpty() && ulong.TryParse(cell.GetString(), out ulong rowId) && rowId == id)
                    return row;
            }
            return -1;
        }

        // Mở file có sẵn, hoặc tạo mới nếu chưa tồn tại, kèm ghi header đúng theo hasFlag
        private static (XLWorkbook workbook, IXLWorksheet worksheet, bool hasFlag) OpenOrCreate(string filePath, bool? hasFlagHint)
        {
            if (File.Exists(filePath))
            {
                var workbook = new XLWorkbook(filePath);
                var ws = workbook.Worksheets.FirstOrDefault();
                CheckCardListResult checkResult = CheckExcel.CheckExcelCardListValidity(ws);
                if (!checkResult.Result || checkResult.Format == CardListFormat.OMEGA)
                {
                    workbook.Dispose();
                    throw new InvalidOperationException(string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }
                return (workbook, ws!, checkResult.HasFlag);
            }
            else
            {
                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Cards");
                bool hasFlag = hasFlagHint ?? false;
                var headers = GetHeaders(hasFlag);
                for (int col = 0; col < headers.Length; col++)
                    ws.Cell(1, col + 1).SetValue(headers[col]);
                return (workbook, ws, hasFlag);
            }
        }

        public static async Task<WriteResult> AddNewCard(Card newCard, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws, hasFlag) = OpenOrCreate(filePath, hasFlagHint);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                    if (lastRow >= 2 && FindRowById(ws, newCard.id, lastRow) != -1)
                    {
                        result.Result = false;
                        result.Messenger = CMess.cardIDExist.ToText();
                        return result;
                    }

                    WriteCardToRow(ws, lastRow + 1, newCard, hasFlag);
                    await Task.Run(() => workbook.SaveAs(filePath));

                    result.Result = true;
                    result.Messenger = string.Empty;
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }
        public static async Task<WriteResult> ModifyCurrentCard(Card currentCard, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws, hasFlag) = OpenOrCreate(filePath, hasFlagHint);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                    int row = FindRowById(ws, currentCard.id, lastRow);
                    if (row == -1)
                    {
                        result.Result = false;
                        result.Messenger = CMess.cardIDNotExist.ToText();
                        return result;
                    }

                    WriteCardToRow(ws, row, currentCard, hasFlag);
                    await Task.Run(() => workbook.SaveAs(filePath));

                    result.Result = true;
                    result.Messenger = string.Empty;
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();

            if (!File.Exists(filePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws, hasFlag) = OpenOrCreate(filePath, hasFlagHint);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                    int row = FindRowById(ws, cardId, lastRow);

                    if (row == -1)
                    {
                        result.Result = false;
                        result.Messenger = CMess.cardIDNotExist.ToText();
                        return result;
                    }

                    ws.Row(row).Delete();

                    await Task.Run(() => workbook.SaveAs(filePath));

                    result.Result = true;
                    result.Messenger = string.Empty;
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }

        public static async Task<WriteResult> SaveCardList(List<Card> cardList, string filePath, bool hasFlag = false)
        {
            WriteResult result = new();

            if (cardList == null)
            {
                result.Result = false;
                result.Messenger = "Card list is null";
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Cards");

                var headers = GetHeaders(hasFlag);
                for (int col = 0; col < headers.Length; col++)
                    ws.Cell(1, col + 1).SetValue(headers[col]);

                for (int i = 0; i < cardList.Count; i++)
                    WriteCardToRow(ws, i + 2, cardList[i], hasFlag);

                await Task.Run(() => workbook.SaveAs(filePath));

                result.Result = true;
                result.Messenger = string.Empty;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }
    }

    public class SaveExcelOMEGAService
    {
        private static string[] GetHeaders() => ConstantColumnExcel.HeaderOmega.ToArray();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new(StringComparer.OrdinalIgnoreCase);
        private static SemaphoreSlim GetLock(string filePath) => _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));

        private static void WriteCardToRow(IXLWorksheet ws, int row, CardOmega card)
        {
            ws.Cell(row, 1).SetValue(card.id.ToString());
            ws.Cell(row, 2).SetValue(card.name);
            ws.Cell(row, 3).SetValue(card.desc);
            ws.Cell(row, 4).SetValue(card.ot.ToString());
            ws.Cell(row, 5).SetValue(card.alias.ToString());
            ws.Cell(row, 6).SetValue(card.setcode.ToString());
            ws.Cell(row, 7).SetValue(card.type.ToString());
            ws.Cell(row, 8).SetValue(card.atk.ToString());
            ws.Cell(row, 9).SetValue(card.def.ToString());
            ws.Cell(row, 10).SetValue(card.level.ToString());
            ws.Cell(row, 11).SetValue(card.race.ToString());
            ws.Cell(row, 12).SetValue(card.attribute.ToString());
            ws.Cell(row, 13).SetValue(card.category.ToString());

            ws.Cell(row, 14).SetValue(card.genre);
            ws.Cell(row, 15).SetValue(card.script);
            ws.Cell(row, 16).SetValue(card.support);

            ws.Cell(row, 17).SetValue(card.str1);
            ws.Cell(row, 18).SetValue(card.str2);
            ws.Cell(row, 19).SetValue(card.str3);
            ws.Cell(row, 20).SetValue(card.str4);
            ws.Cell(row, 21).SetValue(card.str5);
            ws.Cell(row, 22).SetValue(card.str6);
            ws.Cell(row, 23).SetValue(card.str7);
            ws.Cell(row, 24).SetValue(card.str8);
            ws.Cell(row, 25).SetValue(card.str9);
            ws.Cell(row, 26).SetValue(card.str10);
            ws.Cell(row, 27).SetValue(card.str11);
            ws.Cell(row, 28).SetValue(card.str12);
            ws.Cell(row, 29).SetValue(card.str13);
            ws.Cell(row, 30).SetValue(card.str14);
            ws.Cell(row, 31).SetValue(card.str15);
            ws.Cell(row, 32).SetValue(card.str16);
        }

        private static int FindRowById(IXLWorksheet ws, ulong id, int lastRow)
        {
            for (int row = 2; row <= lastRow; row++)
            {
                var cell = ws.Cell(row, 1);
                if (!cell.IsEmpty() && ulong.TryParse(cell.GetString(), out ulong rowId) && rowId == id)
                    return row;
            }
            return -1;
        }

        // Mở file có sẵn, hoặc tạo mới nếu chưa tồn tại
        private static (XLWorkbook workbook, IXLWorksheet worksheet) OpenOrCreate(string filePath, bool? hasFlagHint = null)
        {
            if (File.Exists(filePath))
            {
                var workbook = new XLWorkbook(filePath);
                var ws = workbook.Worksheets.FirstOrDefault();
                CheckCardListResult checkResult = CheckExcel.CheckExcelCardListValidity(ws);
                if (!checkResult.Result || checkResult.Format == CardListFormat.YGONoFlag || checkResult.Format == CardListFormat.YGOHasFlag)
                {
                    workbook.Dispose();
                    throw new InvalidOperationException(string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }

                return (workbook, ws!);
            }
            else
            {
                var newWorkbook = new XLWorkbook();
                var newWorksheet = newWorkbook.Worksheets.Add("Cards");
                var headers = GetHeaders();
                for (int col = 0; col < headers.Length; col++)
                    newWorksheet.Cell(1, col + 1).SetValue(headers[col]);
                return (newWorkbook, newWorksheet);
            }
        }

        public static async Task<WriteResult> AddNewCard(CardOmega newCard, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws) = OpenOrCreate(filePath);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                    if (lastRow >= 2 && FindRowById(ws, newCard.id, lastRow) != -1)
                    {
                        result.Result = false;
                        result.Messenger = CMess.cardIDExist.ToText();
                        return result;
                    }

                    WriteCardToRow(ws, lastRow + 1, newCard);
                    await Task.Run(() => workbook.SaveAs(filePath));

                    result.Result = true;
                    result.Messenger = string.Empty;
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }
        public static async Task<WriteResult> ModifyCurrentCard(CardOmega currentCard, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws) = OpenOrCreate(filePath);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                    int row = FindRowById(ws, currentCard.id, lastRow);
                    if (row == -1)
                    {
                        result.Result = false;
                        result.Messenger = CMess.cardIDNotExist.ToText();
                        return result;
                    }

                    WriteCardToRow(ws, row, currentCard);
                    await Task.Run(() => workbook.SaveAs(filePath));

                    result.Result = true;
                    result.Messenger = string.Empty;
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();

            if (!File.Exists(filePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws) = OpenOrCreate(filePath);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                    int row = FindRowById(ws, cardId, lastRow);

                    if (row == -1)
                    {
                        result.Result = false;
                        result.Messenger = CMess.cardIDNotExist.ToText();
                        return result;
                    }

                    ws.Row(row).Delete();

                    await Task.Run(() => workbook.SaveAs(filePath));

                    result.Result = true;
                    result.Messenger = string.Empty;
                }
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }

        public static async Task<WriteResult> SaveCardList(List<CardOmega> cardList, string filePath, bool? hasFlagHint = null)
        {
            WriteResult result = new();

            if (cardList == null)
            {
                result.Result = false;
                result.Messenger = "Card list is null";
                return result;
            }

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Cards");

                var headers = GetHeaders();
                for (int col = 0; col < headers.Length; col++)
                    ws.Cell(1, col + 1).SetValue(headers[col]);

                for (int i = 0; i < cardList.Count; i++)
                    WriteCardToRow(ws, i + 2, cardList[i]);

                await Task.Run(() => workbook.SaveAs(filePath));

                result.Result = true;
                result.Messenger = string.Empty;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            finally
            {
                fileLock.Release();
            }
            return result;
        }
    }
}
