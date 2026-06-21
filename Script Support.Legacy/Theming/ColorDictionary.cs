using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Theming
{
    public class ColorDictionary
    {
        // Mảng hai chiều: Dòng 1 là tên màu, Dòng 2 là mã hex
        private readonly string[,] colorData = new string[,]
        {
            { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" },
            { "#FFC107", "#2196F3", "#607D8B", "#795548", "#00BCD4", "#FF5722", "#673AB7", "#4CAF50", "#9E9E9E", "#3F51B5", "#03A9F4", "#8BC34A", "#CDDC39", "#FF9800", "#E91E63", "#9C27B0", "#F44336", "#009688", "#FFEB3B" }
        };

        // Hàm tìm kiếm mã hex dựa trên tên màu
        public string GetHexCode(string colorName)
        {
            for (int i = 0; i < colorData.GetLength(1); i++)
            {
                if (colorData[0, i].Equals(colorName, StringComparison.OrdinalIgnoreCase))
                {
                    return colorData[1, i];  // Trả về mã hex tương ứng
                }
            }
            return "#673AB7";
        }
    }
}
