using System;
using System.Linq;
using System.Windows;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class EnumsViewModel : IDisposable
    {
        private static readonly Lazy<EnumsViewModel> _instance = new Lazy<EnumsViewModel>(() => new EnumsViewModel());
        public static EnumsViewModel Instance => _instance.Value;

        public BulkObservableCollection<FlowDirectionItem> FlowDirectionItems { get; set; }
        public BulkObservableCollection<TextAlignmentItem> TextAlignmentItems { get; set; }
        public BulkObservableCollection<StampPositionItem> StampPositionItems { get; set; }
        public BulkObservableCollection<SortItem> SortCardItems { get; set; }

        public EnumsViewModel()
        {

        }

        public void InitializeEnumsList()
        {
            FlowDirectionItems = new BulkObservableCollection<FlowDirectionItem>
            {
                new FlowDirectionItem{Direction = CardEditor.Enums.FlowDirection.LeftToRight, DisplayName = CardEditor.Enums.FlowDirection.LeftToRight.ToFriendlyString() },
                new FlowDirectionItem{Direction = CardEditor.Enums.FlowDirection.RightToLeft, DisplayName = CardEditor.Enums.FlowDirection.RightToLeft.ToFriendlyString() }
            };
            TextAlignmentItems = new BulkObservableCollection<TextAlignmentItem>
            {
                new TextAlignmentItem{Alignment = CardEditor.Enums.TextAlignment.Left, DisplayName = CardEditor.Enums.TextAlignment.Left.ToFriendlyString() },
                new TextAlignmentItem{Alignment = CardEditor.Enums.TextAlignment.Right, DisplayName = CardEditor.Enums.TextAlignment.Right.ToFriendlyString() },
                new TextAlignmentItem{Alignment = CardEditor.Enums.TextAlignment.Justify, DisplayName = CardEditor.Enums.TextAlignment.Justify.ToFriendlyString() },
                new TextAlignmentItem{Alignment = CardEditor.Enums.TextAlignment.Center, DisplayName = CardEditor.Enums.TextAlignment.Center.ToFriendlyString() }
            };
            StampPositionItems = new BulkObservableCollection<StampPositionItem>
            {
                new StampPositionItem { Position = StampPosition.TopLeft, DisplayName = StampPosition.TopLeft.ToFriendlyString() },
                new StampPositionItem { Position = StampPosition.TopRight, DisplayName = StampPosition.TopRight.ToFriendlyString() },
                new StampPositionItem { Position = StampPosition.BottomLeft, DisplayName = StampPosition.BottomLeft.ToFriendlyString() },
                new StampPositionItem { Position = StampPosition.BottomRight, DisplayName = StampPosition.BottomRight.ToFriendlyString() },
                new StampPositionItem { Position = StampPosition.Center, DisplayName = StampPosition.Center.ToFriendlyString() },
                new StampPositionItem { Position = StampPosition.Unknown, DisplayName = StampPosition.Unknown.ToFriendlyString() }
            };
            SortCardItems = new BulkObservableCollection<SortItem>
            {
                new SortItem {Sort = SortType.ID, Name = "id", DisplayName = SortType.ID.ToFriendlyString() },
                new SortItem {Sort = SortType.NAME, Name = "name", DisplayName = SortType.NAME.ToFriendlyString() },
                new SortItem {Sort = SortType.RULE, Name = "ot", DisplayName = SortType.RULE.ToFriendlyString() },
                new SortItem {Sort = SortType.ALIAS, Name = "alias", DisplayName = SortType.ALIAS.ToFriendlyString() },
                new SortItem {Sort = SortType.SETCODE, Name = "setcode", DisplayName = SortType.SETCODE.ToFriendlyString() },
                new SortItem {Sort = SortType.TYPE, Name = "type", DisplayName = SortType.TYPE.ToFriendlyString() },
                new SortItem {Sort = SortType.ATK, Name = "atk", DisplayName = SortType.ATK.ToFriendlyString() },
                new SortItem {Sort = SortType.DEF, Name = "def", DisplayName = SortType.DEF.ToFriendlyString() },
                new SortItem {Sort = SortType.LEVEL, Name = "level", DisplayName = SortType.LEVEL.ToFriendlyString() },
                new SortItem {Sort = SortType.RACE, Name = "race", DisplayName = SortType.RACE.ToFriendlyString() },
                new SortItem {Sort = SortType.ATTRIBUTE, Name = "attribute", DisplayName = SortType.ATTRIBUTE.ToFriendlyString() },
                new SortItem {Sort = SortType.CATEGORY, Name = "category", DisplayName = SortType.CATEGORY.ToFriendlyString() },
                new SortItem {Sort = SortType.RARE, Name = "Rare", DisplayName = SortType.RARE.ToFriendlyString() },
                new SortItem {Sort = SortType.GPOINT, Name = "GPoint", DisplayName = SortType.GPOINT.ToFriendlyString() },
            };
        }

        public void ReLoadDisplayName()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in FlowDirectionItems ?? Enumerable.Empty<FlowDirectionItem>())
                {
                    string text = item.Direction.ToFriendlyString();
                    item.DisplayName = string.IsNullOrWhiteSpace(text) ? CMess.unknown.ToText() : text;
                }
                foreach (var item in TextAlignmentItems ?? Enumerable.Empty<TextAlignmentItem>())
                {
                    string text = item.Alignment.ToFriendlyString();
                    item.DisplayName = string.IsNullOrWhiteSpace(text) ? CMess.unknown.ToText() : text;
                }
                foreach (var item in SortCardItems ?? Enumerable.Empty<SortItem>())
                {
                    string text = item.Sort.ToFriendlyString();
                    item.DisplayName = string.IsNullOrWhiteSpace(text) ? CMess.unknown.ToText() : text;
                }

                foreach (var item in StampPositionItems ?? Enumerable.Empty<StampPositionItem>())
                {
                    string text = item.Position.ToFriendlyString();
                    item.DisplayName = string.IsNullOrWhiteSpace(text) ? CMess.unknown.ToText() : text;
                }
            });
        }

        public void Dispose()
        {
            FlowDirectionItems.Clear();
            TextAlignmentItems.Clear();
            StampPositionItems.Clear();
            SortCardItems.Clear();

            FlowDirectionItems = null;
            TextAlignmentItems = null;
            StampPositionItems = null;
            SortCardItems = null;
        }
    }
}
