using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Services.CheckData;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadExcelService
    {
        public static async Task<LoadCardDataResult> LoadExcelCard(string filePath)
        {
            var result = new LoadCardDataResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var workbook = new XLWorkbook(memoryStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                CheckCardListResult checkResult = CheckExcel.CheckExcelCardListValidity(worksheet);
                if (!checkResult.Result)
                {
                    result.Result = false;
                    result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                    return result;
                }

                result.Format = checkResult.Format;
                result.HasFlag = checkResult.HasFlag;

                switch (checkResult.Format)
                {
                    case CardListFormat.YGONoFlag:
                    case CardListFormat.YGOHasFlag:
                        await LoadYGOExcelCard(worksheet, result, result.HasFlag);
                        break;
                    case CardListFormat.OMEGA:
                        await LoadOMEGAExcelCard(worksheet, result);
                        break;
                    default:
                        result.Result = false;
                        result.Message = $"Unsupported database type: {checkResult.Format}";
                        return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
        }
        private static async Task LoadYGOExcelCard(IXLWorksheet worksheet, LoadCardDataResult result, bool hasFlag)
        {
            try
            {
                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                var importedCards = new List<Card>();
                int errorCount = 0;

                int strOffset = hasFlag ? 14 : 13;

                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var idCell = worksheet.Cell(row, 1);

                        if (idCell.IsEmpty()) continue;

                        if (!ulong.TryParse(idCell.GetString(), out ulong id)) continue;

                        var card = new Card
                        {
                            id = id,    //A

                            name = worksheet.Cell(row, 2).GetString(),  //B
                            desc = worksheet.Cell(row, 3).GetString(),  //C

                            ot = ulong.TryParse(worksheet.Cell(row, 4).GetString(), out ulong otvar) ? otvar : 0UL,                         //D
                            alias = ulong.TryParse(worksheet.Cell(row, 5).GetString(), out ulong aliasvar) ? aliasvar : 0UL,                //E
                            setcode = ulong.TryParse(worksheet.Cell(row, 6).GetString(), out ulong setcodevar) ? setcodevar : 0UL,          //F
                            type = ulong.TryParse(worksheet.Cell(row, 7).GetString(), out ulong typevar) ? typevar : 0UL,                   //G
                            atk = long.TryParse(worksheet.Cell(row, 8).GetString(), out long atkvar) ? atkvar : 0L,                         //H
                            def = long.TryParse(worksheet.Cell(row, 9).GetString(), out long defvar) ? defvar : 0L,                         //I
                            level = ulong.TryParse(worksheet.Cell(row, 10).GetString(), out ulong levelvar) ? levelvar : 0UL,               //J
                            race = ulong.TryParse(worksheet.Cell(row, 11).GetString(), out ulong racevar) ? racevar : 0UL,                  //K
                            attribute = ulong.TryParse(worksheet.Cell(row, 12).GetString(), out ulong attributevar) ? attributevar : 0UL,   //L
                            category = ulong.TryParse(worksheet.Cell(row, 13).GetString(), out ulong categoryvar) ? categoryvar : 0UL,      //M
                            flag = hasFlag && ulong.TryParse(worksheet.Cell(row, 14).GetString(), out ulong flagvar) ? flagvar : 0UL,       //N
                            str1 = worksheet.Cell(row, strOffset + 1).GetString(),          //N     O
                            str2 = worksheet.Cell(row, strOffset + 2).GetString(),          //O     P
                            str3 = worksheet.Cell(row, strOffset + 3).GetString(),          //P     Q
                            str4 = worksheet.Cell(row, strOffset + 4).GetString(),          //Q     R
                            str5 = worksheet.Cell(row, strOffset + 5).GetString(),          //R     S
                            str6 = worksheet.Cell(row, strOffset + 6).GetString(),          //S     T
                            str7 = worksheet.Cell(row, strOffset + 7).GetString(),          //T     U
                            str8 = worksheet.Cell(row, strOffset + 8).GetString(),          //U     V
                            str9 = worksheet.Cell(row, strOffset + 9).GetString(),          //V     W
                            str10 = worksheet.Cell(row, strOffset + 10).GetString(),        //W     X
                            str11 = worksheet.Cell(row, strOffset + 11).GetString(),        //X     Y
                            str12 = worksheet.Cell(row, strOffset + 12).GetString(),        //Y     Z
                            str13 = worksheet.Cell(row, strOffset + 13).GetString(),        //Z     AA
                            str14 = worksheet.Cell(row, strOffset + 14).GetString(),        //AA    AB
                            str15 = worksheet.Cell(row, strOffset + 15).GetString(),        //AB    AC
                            str16 = worksheet.Cell(row, strOffset + 16).GetString()         //AC    AD
                        };

                        importedCards.Add(card);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;

                        File.AppendAllText(
                            CardEditor.Models.AppContext.Instance.ErrorLogFilePath,
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} " +
                            $"Row: {row} {ex.Message}{Environment.NewLine}");

                        continue;
                    }
                }

                result.CardList = importedCards;
                result.Message = errorCount.ToString();
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
            }
        }
        private static async Task LoadOMEGAExcelCard(IXLWorksheet worksheet, LoadCardDataResult result)
        {
            try
            {
                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                var importedCards = new List<CardOmega>();
                int errorCount = 0;

                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var idCell = worksheet.Cell(row, 1);

                        if (idCell.IsEmpty()) continue;

                        if (!ulong.TryParse(idCell.GetString(), out ulong id)) continue;

                        var card = new CardOmega
                        {
                            id = id,        //A
                            name = worksheet.Cell(row, 2).GetString(),  //B
                            desc = worksheet.Cell(row, 3).GetString(),  //C

                            ot = ulong.TryParse(worksheet.Cell(row, 4).GetString(), out ulong otvar) ? otvar : 0UL,                         //D
                            alias = ulong.TryParse(worksheet.Cell(row, 5).GetString(), out ulong aliasvar) ? aliasvar : 0UL,                //E
                            setcode = ulong.TryParse(worksheet.Cell(row, 6).GetString(), out ulong setcodeValue) ? setcodeValue : 0UL,      //F
                            type = ulong.TryParse(worksheet.Cell(row, 7).GetString(), out ulong typevar) ? typevar : 0UL,                   //G
                            atk = long.TryParse(worksheet.Cell(row, 8).GetString(), out long atkvar) ? atkvar : 0L,                         //H
                            def = long.TryParse(worksheet.Cell(row, 9).GetString(), out long defvar) ? defvar : 0L,                         //I
                            level = ulong.TryParse(worksheet.Cell(row, 10).GetString(), out ulong levelvar) ? levelvar : 0UL,               //J
                            race = ulong.TryParse(worksheet.Cell(row, 11).GetString(), out ulong racevar) ? racevar : 0UL,                  //K
                            attribute = ulong.TryParse(worksheet.Cell(row, 12).GetString(), out ulong attributevar) ? attributevar : 0UL,   //L
                            // Omega category = Flags
                            category = ulong.TryParse(worksheet.Cell(row, 13).GetString(), out ulong categoryvar) ? categoryvar : 0UL,      //M
                            // Omega genre = Effect category
                            genre = ulong.TryParse(worksheet.Cell(row, 14).GetString(), out ulong genrevar) ? genrevar : 0UL,               //N
                            script = worksheet.Cell(row, 15).GetString(),                                                                   //O
                            support = ulong.TryParse(worksheet.Cell(row, 16).GetString(), out ulong supportvar) ? supportvar : 0UL,         //P

                            str1 = worksheet.Cell(row, 17).GetString(),         //Q
                            str2 = worksheet.Cell(row, 18).GetString(),         //R
                            str3 = worksheet.Cell(row, 19).GetString(),         //S
                            str4 = worksheet.Cell(row, 20).GetString(),         //T
                            str5 = worksheet.Cell(row, 21).GetString(),         //U
                            str6 = worksheet.Cell(row, 22).GetString(),         //V
                            str7 = worksheet.Cell(row, 23).GetString(),         //W
                            str8 = worksheet.Cell(row, 24).GetString(),         //X
                            str9 = worksheet.Cell(row, 25).GetString(),         //Y
                            str10 = worksheet.Cell(row, 26).GetString(),        //Z
                            str11 = worksheet.Cell(row, 27).GetString(),        //AA
                            str12 = worksheet.Cell(row, 28).GetString(),        //AB
                            str13 = worksheet.Cell(row, 29).GetString(),        //AC
                            str14 = worksheet.Cell(row, 30).GetString(),        //AD
                            str15 = worksheet.Cell(row, 31).GetString(),        //AE
                            str16 = worksheet.Cell(row, 32).GetString()         //AF
                        };

                        importedCards.Add(card);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;

                        File.AppendAllText(
                            CardEditor.Models.AppContext.Instance.ErrorLogFilePath,
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} " +
                            $"Row: {row} {ex.Message}{Environment.NewLine}");

                        continue;
                    }
                }

                result.OmegaCardList = importedCards;
                result.Message = errorCount.ToString();
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
            }
        }
    }
}
