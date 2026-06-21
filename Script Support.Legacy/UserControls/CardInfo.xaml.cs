using System;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using ScriptSupport.Legacy.Models;
using ScriptSupport.Legacy.Services;
using ScriptSupport.Legacy.ViewModels;
using ScriptSupport.Legacy.Localization;
using CMess = ScriptSupport.Legacy.Localization.Language;

namespace ScriptSupport.Legacy.UserControls
{
    /// <summary>
    /// Interaction logic for CardInfo.xaml
    /// </summary>
    public partial class CardInfo : UserControl, INotifyPropertyChanged
    {
        private CancellationTokenSource CardInfoCTS;
        private Card _selectedCard;
        public Card SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    OnPropertyChanged(nameof(SelectedCard));

                    _ = HandleSelectedCardChanged();
                }
            }
        }

        public CardInfo()
        {
            InitializeComponent();
            CardInfoCTS = new CancellationTokenSource();
        }

        public void SetLabel()
        {
            SetCodeLabel.Text = $"{CMess.cardlabelSetCode.ToText()} :";
            CardTypeLabel.Text = $"{CMess.cardLabelType.ToText()} :";
            CardAttriLabel.Text = $"{CMess.cardLabelAttri.ToText()} :";
            PenScaleLabel.Text = $"{CMess.penScaleLabel.ToText()} :";
            LinkRatLabel.Text = $"{CMess.LinkRat.ToText()}: ";
            LinkArrLabel.Text = $"{CMess.LinkArr.ToText()}: ";
            AtkLabel.Text = $"{CMess.cardatk.ToText()} :";
            DefLabel.Text = $"{CMess.carddef.ToText()} :";
        }

        private async Task DebounceCardInfo(Func<Task> action, int delay)
        {
            CardInfoCTS?.Cancel();
            CardInfoCTS?.Dispose();
            CardInfoCTS = new CancellationTokenSource();
            try
            {
                await Task.Delay(delay, CardInfoCTS.Token);
                await action();
            }
            catch { }
        }
        private async Task HandleSelectedCardChanged()
        {
            CardInfoCTS?.Cancel();
            CardInfoCTS?.Dispose();
            CardInfoCTS = new CancellationTokenSource();
            try
            {
                await Task.Delay(200, CardInfoCTS.Token);
                OnCurrentCardChanged();
            }
            catch { }
        }
        private void OnCurrentCardChanged()
        {
            if (SelectedCard == null) return;
            var CurrentCard = SelectedCard;
            var CardInfo = new CardItemInfo(CurrentCard.type);

            string setcodefind = FindIInfoService.FindSetcode(CardInfoViewModel.Instance.listsetcode, CurrentCard.setcode);
            if (string.IsNullOrWhiteSpace(setcodefind)) grsetcode.Visibility = Visibility.Collapsed;
            else
            {
                grsetcode.Visibility = Visibility.Visible;
                tblsetcode.Text = setcodefind;
            }

            string typefind = FindIInfoService.FindCardInfo(CardInfoViewModel.Instance.listtype, CurrentCard.type, true);
            if (string.IsNullOrWhiteSpace(typefind)) grcardtype.Visibility = Visibility.Collapsed;
            else
            {
                grcardtype.Visibility = Visibility.Visible;
                tblcardtype.Text = typefind;
            }

            string attributefind = FindIInfoService.FindCardInfo(CardInfoViewModel.Instance.listattr, CurrentCard.attribute, true);
            if (string.IsNullOrWhiteSpace(attributefind)) grattribute.Visibility = Visibility.Collapsed;
            else
            {
                grattribute.Visibility = Visibility.Visible;
                tblmonsattri.Text = attributefind;
            }

            if (CurrentCard.race != 0)
            {
                grrace.Visibility = Visibility.Visible;
                if (CardInfo.IsSkill)
                {
                    CardRaceLabel.Text = $"{CMess.cardLabelChar.ToText()}: ";
                    tblmonsrace.Text = FindIInfoService.FindCardInfo(CardInfoViewModel.Instance.listchar, CurrentCard.race, true);
                }
                else
                {
                    CardRaceLabel.Text = $"{CMess.cardLabelRace.ToText()}: ";
                    tblmonsrace.Text = FindIInfoService.FindCardInfo(CardInfoViewModel.Instance.listrace, CurrentCard.race, true);
                }
            }

            var lvpen = FindIInfoService.FindLevelPenScale(CurrentCard.level, CardInfo.IsLink);
            if (CardInfo.IsPendulum)
            {
                grpendulum.Visibility = Visibility.Visible;
                tblPenLeft.Text = lvpen.Item4.ToString();
                tblPenRight.Text = lvpen.Item3.ToString();
            }
            else
            {
                grpendulum.Visibility = Visibility.Collapsed;
            }

            grlvrk.Visibility = (CardInfo.IsMonster || CardInfo.IsLink) ? Visibility.Visible : Visibility.Collapsed;
            if (CardInfoViewModel.Instance.darksynchrolist.Contains(CurrentCard.id))
            {
                LvRkLabel.Text = $" :{CMess.minuLv.ToText()}";
                LvRkLabel.LayoutTransform = new ScaleTransform(-1, 1);
                LvRkValue.Text = CardInfo.IsLink ? $"-{lvpen.Item2} " : $"-{lvpen.Item1} ";
                imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/minuslevel.ico", UriKind.RelativeOrAbsolute));
            }
            else
            {
                if (CardInfo.IsXyz && !CardInfo.IsNonXyz)
                {
                    LvRkLabel.Text = $"{CMess.Rank.ToText()}: ";
                    imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/rankstart.ico", UriKind.RelativeOrAbsolute));
                }
                else if (CardInfo.IsNonXyz && CardInfo.IsXyz)
                {
                    LvRkLabel.Text = $"{CMess.Level.ToText()}/{CMess.Rank.ToText()}: ";
                    imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/levelrank.ico", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    LvRkLabel.Text = $"{CMess.Level.ToText()}: ";
                    imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/levelstar.ico", UriKind.RelativeOrAbsolute));
                }
                LvRkLabel.LayoutTransform = new ScaleTransform(1, 1);
                LvRkValue.Text = CardInfo.IsLink ? $"{lvpen.Item2} " : $"{lvpen.Item1} ";
            }

            grmonspow.Visibility = CardInfo.IsMonster ? Visibility.Visible : Visibility.Collapsed;

            if (CardInfo.IsLink)
            {
                grlink.Visibility = Visibility.Visible;

                var (DecoLinkarrow, DecoDef, notHasATK) = GetInfoService.DecodeDef(CurrentCard.def);
                gratk.Visibility = notHasATK ? Visibility.Collapsed : Visibility.Visible;
                grdef.Visibility = DecoDef.HasValue ? Visibility.Visible : Visibility.Collapsed;

                AtkValue.Text = notHasATK ? string.Empty : (CurrentCard.atk >= 0 ? CurrentCard.atk.ToString() : "?");
                DefValue.Text = DecoDef.HasValue ? (DecoDef >= 0 ? DecoDef.ToString() : "?") : string.Empty;

                LinkRatValue.Text = lvpen.Item1.ToString();
                LinkArrValue.Text = FindIInfoService.FindCardInfo(CardInfoViewModel.Instance.listlinkarrow, (ulong)CurrentCard.def, false);
            }
            else
            {
                grlink.Visibility = Visibility.Collapsed;
                gratk.Visibility = Visibility.Visible;
                grdef.Visibility = Visibility.Visible;

                AtkValue.Text = CurrentCard.atk >= 0 ? CurrentCard.atk.ToString() : "?";
                DefValue.Text = CurrentCard.def >= 0 ? CurrentCard.def.ToString() : "?";
            }

            IDValue.Text = CurrentCard.alias > 0 ? $"ID: {CurrentCard.id} | Alias: {CurrentCard.alias}" : $"ID: {CurrentCard.id}";

            int? konamiID = null;
            if (CurrentCard.id < 100000000) konamiID = GetIDService.GetKonamiOfficialID(CurrentCard.id.ToString());
            else if (CurrentCard.id >= 160000000 && CurrentCard.id < 300000000) konamiID = GetIDService.GetKonamiRushID(CurrentCard.id.ToString());
            KonamiIDValue.Text = konamiID.HasValue ? $" | Konami ID: {konamiID}" : string.Empty;

            string RuleFind = FindIInfoService.FindCardInfo(CardInfoViewModel.Instance.listrule, CurrentCard.ot, true);
            if (!string.IsNullOrWhiteSpace(RuleFind))
            {
                RuleValue.Text = $"[{RuleFind}]";
                RuleValue.Visibility = Visibility.Visible;
            }
            else
            {
                RuleValue.Text = string.Empty;
                RuleValue.Visibility = Visibility.Collapsed;
            }
        }

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {
            SelectedCard = null;
            CardInfoCTS?.Cancel();
            CardInfoCTS?.Dispose();
        }
        #endregion

    }
}
