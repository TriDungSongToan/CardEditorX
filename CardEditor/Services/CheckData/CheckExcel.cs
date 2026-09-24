using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;

namespace CardEditor.Services.CheckData
{
    public static class CheckExcel
    {
        /// <summary>
        /// Check whether the Excel file's header structure (the first row) meets the requirements.
        /// Excel file with first be checked against HeaderOmega structure to ensure safety.
        /// Afterwards, Excel will be checked using HeaderWithFlag structure (where a string-type `flag` column immediately follows the `category` column).
        /// Finally, Excel file is checked against the HeaderNoFlag structure.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public static CheckCardListResult CheckExcelCardListValidity(IXLWorksheet worksheet)
        {
            var result = new CheckCardListResult();
            if (worksheet == null) return result;

            // 1. Check Omega first
            if (IsHeaderMatch(worksheet, ConstantColumnExcel.HeaderOmega))
            {
                result.Result = true;
                result.Format = CardListFormat.OMEGA;
                result.HasFlag = true;

                return result;
            }

            // 2. Check YGO with Flag
            if (IsHeaderMatch(worksheet, ConstantColumnExcel.HeaderWithFlag))
            {
                result.Result = true;
                result.Format = CardListFormat.YGOHasFlag;
                result.HasFlag = true;

                return result;
            }

            // 3. Check YGO without Flag
            if (IsHeaderMatch(worksheet, ConstantColumnExcel.HeaderNoFlag))
            {
                result.Result = true;
                result.Format = CardListFormat.YGONoFlag;
                result.HasFlag = false;

                return result;
            }

            // 4. Invalid
            return result;
        }
        public static CheckCardListResult CheckExcelListNameValidity(IXLWorksheet worksheet)
        {
            var result = new CheckCardListResult();
            if (worksheet == null) return result;

            if (IsHeaderMatch(worksheet, ConstantColumnExcel.HeaderListName))
            {
                result.Result = true;
                result.Format = CardListFormat.YGONoFlag;
                result.HasFlag = false;

                return result;
            }
            return result;
        }
        private static bool IsHeaderMatch(IXLWorksheet worksheet, IReadOnlyList<string> expectedHeaders)
        {
            for (int col = 1; col <= expectedHeaders.Count; col++)
            {
                var header = worksheet.Cell(1, col).GetString();
                if (!string.Equals(header, expectedHeaders[col - 1], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

    }
}
