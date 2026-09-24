using System;
using System.Linq;
using System.Collections.Generic;
using CardEditor.Models;

namespace CardEditor.Converter
{
    public static class CardConverter
    {
        public static CardOmega CardToCardOmega(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            return new CardOmega
            {
                id = card.id,

                name = card.name,
                desc = card.desc,
                str1 = card.str1,
                str2 = card.str2,
                str3 = card.str3,
                str4 = card.str4,
                str5 = card.str5,
                str6 = card.str6,
                str7 = card.str7,
                str8 = card.str8,
                str9 = card.str9,
                str10 = card.str10,
                str11 = card.str11,
                str12 = card.str12,
                str13 = card.str13,
                str14 = card.str14,
                str15 = card.str15,
                str16 = card.str16,

                ot = card.ot,
                alias = card.alias,
                setcode = card.setcode,
                type = card.type,
                atk = card.atk,
                def = card.def,
                level = card.level,
                race = card.race,
                attribute = card.attribute,

                // Đảo ngược mapping
                genre = card.category,
                category = card.flag,

                // Card không có các field này
                script = string.Empty,
                support = 0
            };
        }
        public static List<CardOmega> ListCardToListCardOmega(List<Card> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            return cards.Select(CardToCardOmega).ToList();
        }

        public static Card CardOmegaToCard(CardOmega omega)
        {
            if (omega == null) throw new ArgumentNullException(nameof(omega));
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
                setcode = omega.setcode,
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
        public static List<Card> ListCardOmegaToListCard(List<CardOmega> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            return cards.Select(CardOmegaToCard).ToList();
        }

        public static CardBanList CardToCardBanList(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            return new CardBanList
            {
                Id = card.id,
                Name = card.name,
                LimitedCount = 3
            };
        }
        public static List<CardBanList> ListCardToListCardBanList(List<Card> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            return cards.Select(CardToCardBanList).ToList();
        }
        public static CardBanList CardOmegaToCardBanList(CardOmega omega)
        {
            if (omega == null) throw new ArgumentNullException(nameof(omega));
            return new CardBanList
            {
                Id = omega.id,
                Name = omega.name,
                LimitedCount = 3
            };
        }
        public static List<CardBanList> ListCardOmegaToListCardBanList(List<CardOmega> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            return cards.Select(CardOmegaToCardBanList).ToList();
        }

        public static RareCard CardToCardRare(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            return new RareCard
            {
                id = card.id,
                name = card.name,
                rare = 0
            };
        }
        public static List<RareCard> ListCardToListCardRare(List<Card> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            return cards.Select(CardToCardRare).ToList();
        }
        public static RareCard CardOmegaToCardRare(CardOmega omega)
        {
            if (omega == null) throw new ArgumentNullException(nameof(omega));
            return new RareCard
            {
                id = omega.id,
                name = omega.name,
            };
        }
        public static List<RareCard> ListCardOmegaToListCardRare(List<CardOmega> cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            return cards.Select(CardOmegaToCardRare).ToList();
        }
        public static List<RareCard> ListCardBanlistToListRareCard(List<CardBanList> cards)
        {
            List<RareCard> cardList = new();

            foreach (var card in cards)
            {
                RareCard rareCard = new RareCard
                {
                    id = card.Id,
                    name = card.Name,
                    rare = 0,
                };
                cardList.Add(rareCard);
            }
            return cardList;
        }
    }
}
