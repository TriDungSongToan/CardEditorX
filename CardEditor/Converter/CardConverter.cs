using CardEditor.Models;
using CardEditor.Helpers;

namespace CardEditor.Converter
{
    public static class CardConverter
    {
        public static Card ToCard(CardOmega omega)
        {
            return new Card
            {
                id = omega.id,

                name = omega.name,
                desc = omega.desc,
                str1 = omega.str1,
                str2 = omega.str2,
                str3 = omega.str3,
                str4 = omega.str4,
                str5 = omega.str5,
                str6 = omega.str6,
                str7 = omega.str7,
                str8 = omega.str8,
                str9 = omega.str9,
                str10 = omega.str10,
                str11 = omega.str11,
                str12 = omega.str12,
                str13 = omega.str13,
                str14 = omega.str14,
                str15 = omega.str15,
                str16 = omega.str16,

                ot = omega.ot,
                alias = omega.alias,
                setcode = OmegaBlobHelper.BlobToUInt64(omega.setcode),
                type = omega.type,
                atk = omega.atk,
                def = omega.def,
                level = omega.level,
                race = omega.race,
                attribute = omega.attribute,

                // Khác tên giữa hai schema
                category = omega.genre,
                flag = omega.category
            };
        }
    }
}
