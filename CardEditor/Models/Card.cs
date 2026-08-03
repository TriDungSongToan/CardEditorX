using System;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Services;
using CardEditor.ViewModels;

namespace CardEditor.Models
{
    [Serializable]
    public class Card
    {
        public ulong id { get; set; }

        // Bảng texts
        public string name { get; set; } = string.Empty;
        public string desc { get; set; } = string.Empty;
        public string str1 { get; set; } = string.Empty;
        public string str2 { get; set; } = string.Empty;
        public string str3 { get; set; } = string.Empty;
        public string str4 { get; set; } = string.Empty;
        public string str5 { get; set; } = string.Empty;
        public string str6 { get; set; } = string.Empty;
        public string str7 { get; set; } = string.Empty;
        public string str8 { get; set; } = string.Empty;
        public string str9 { get; set; } = string.Empty;
        public string str10 { get; set; } = string.Empty;
        public string str11 { get; set; } = string.Empty;
        public string str12 { get; set; } = string.Empty;
        public string str13 { get; set; } = string.Empty;
        public string str14 { get; set; } = string.Empty;
        public string str15 { get; set; } = string.Empty;
        public string str16 { get; set; } = string.Empty;

        // Bảng datas
        public ulong ot { get; set; } = 0;
        public ulong alias { get; set; } = 0;
        public ulong setcode { get; set; } = 0;
        public ulong type { get; set; } = 0;
        public long atk { get; set; } = 0;
        public long def { get; set; } = 0;
        public ulong level { get; set; } = 0;
        public ulong race { get; set; } = 0;
        public ulong attribute { get; set; } = 0;
        public ulong category { get; set; } = 0;
        public ulong flag { get; set; } = 0;

        public void UpdateFrom(Card src, params Action<Card, Card>[] updaters)
        {
            if (src == null || updaters == null) return;

            foreach (var u in updaters)
                u(this, src);
        }
        public void UpdateFrom(Card src, IEnumerable<Action<Card, Card>> updaters)
        {
            if (src == null || updaters == null) return;
            foreach (var u in updaters)
                u(this, src);
        }
    }
    public class CardEX
    {
        public Card BaseCard { get; set; }
        public ulong ID => BaseCard.id;
        public long Rare { get; set; } = 0;
        public int GPoint { get; set; } = 0;

        public BitmapImage CardImage
        {
            get
            {
                if (!CardImageCacheViewModel.Instance.IsLoaded)
                {
                    return CardImageCacheViewModel.Instance.BlankImage;
                }
                var image = CardImageCacheViewModel.Instance.GetCardImage(ID);
                return image ?? CardImageCacheViewModel.Instance.BlankImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
    public class CardInstance : INotifyPropertyChanged
    {
        public CardEX Card { get; set; }

        public Guid UniqueID { get; } = Guid.NewGuid();

        private bool _isBeingDragged;
        public bool IsBeingDragged
        {
            get => _isBeingDragged;
            set
            {
                if (_isBeingDragged != value)
                {
                    _isBeingDragged = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public override bool Equals(object obj)
        {
            return obj is CardInstance other && UniqueID == other.UniqueID;
        }

        public override int GetHashCode() => UniqueID.GetHashCode();
    }

    public readonly struct CardItemInfo
    {
        public bool IsMonster { get; }
        public bool IsNormal { get; }
        public bool IsEffect { get; }
        public bool IsXyz { get; }
        public bool IsNonXyz { get; }
        public bool IsPendulum { get; }
        public bool IsLink { get; }
        public bool IsSkill { get; }
        public bool IsToken { get; }

        public CardItemInfo(ulong type)
        {
            IsMonster = FindIInfoService.CheckCardInfo(type, CardType.Monster);
            IsNormal = FindIInfoService.CheckCardInfo(type, CardType.Normal);
            IsEffect = FindIInfoService.CheckCardInfo(type, CardType.Effect);
            IsXyz = FindIInfoService.CheckCardInfo(type, CardType.eXceed);
            IsNonXyz = FindIInfoService.CheckCardInfo(type, CardType.Fusion, CardType.Ritual, CardType.Synchro);
            IsPendulum = FindIInfoService.CheckCardInfo(type, CardType.Pendulum);
            IsLink = FindIInfoService.CheckCardInfo(type, CardType.Link);
            IsSkill = FindIInfoService.CheckCardInfo(type, CardType.Skill);
            IsToken = FindIInfoService.CheckCardInfo(type, CardType.Token);
        }
    }
}
