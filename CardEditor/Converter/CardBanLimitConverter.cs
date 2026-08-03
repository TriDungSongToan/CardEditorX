using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using CardEditor.Models;
using CardEditor.ViewModels;

namespace CardEditor.Converter
{
    public class CardBanLimitConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = CardEX object
            // values[1] = SelectedBanList object

            if (values[0] == null || values[1] == null ||
                values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
            {
                return null; // Không hiển thị gì
            }

            if (!(values[0] is CardEX cardEx) || !(values[1] is BanList banList))
            {
                return null;
            }

            ulong cardId = cardEx.ID;

            // Kiểm tra card có trong CardList không
            bool cardInList = banList.CardList != null && banList.CardList.ContainsKey(cardId);

            if (!cardInList)
            {
                // Card không có trong danh sách
                if (banList.WhiteList)
                {
                    // WhiteList = true: Card không trong list -> hiển thị "0"
                    return "⓪";
                }
                else
                {
                    // WhiteList = false: Card không trong list -> không hiển thị
                    return null;
                }
            }

            // Card có trong danh sách
            int limitedCount = banList.CardList[cardId].LimitedCount;

            if (limitedCount <= 0)
            {
                return "⓪";
            }
            else if (limitedCount == 1)
            {
                return "①";
            }
            else if (limitedCount == 2)
            {
                return "②";
            }
            else if (limitedCount >= 3)
            {
                return null; // Không hiển thị
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardBanLimitImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null ||
                values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
            {
                return null;
            }

            if (!(values[0] is CardEX cardEx) || !(values[1] is BanList banList) || banList == null)
            {
                return null;
            }

            CardBanList banInfo = null;
            bool existInList = false;

            if (banList.CardList.TryGetValue(cardEx.ID, out banInfo))
            {
                existInList = true;
            }
            else if (banList.CardList.TryGetValue(cardEx.BaseCard.alias, out banInfo))
            {
                existInList = true;
            }

            BitmapImage result = null;

            if (banList.WhiteList) // Không có trong CardList => Cấm.
            {
                if (existInList)
                {
                    if (banInfo.LimitedCount <= 0) result = BanListRawDataViewModel.Instance.GetLimitImage(0);
                    else if (banInfo.LimitedCount == 1) result = BanListRawDataViewModel.Instance.GetLimitImage(1);
                    else if (banInfo.LimitedCount == 2) result = BanListRawDataViewModel.Instance.GetLimitImage(2);
                    else result = BanListRawDataViewModel.Instance.GetLimitImage(3);
                }
                else result = BanListRawDataViewModel.Instance.GetLimitImage(0);
            }
            else // Không có trong CardList => Không hiển thị.
            {
                if (existInList)
                {
                    if (banInfo.LimitedCount <= 0) result = BanListRawDataViewModel.Instance.GetLimitImage(0);
                    else if (banInfo.LimitedCount == 1) result = BanListRawDataViewModel.Instance.GetLimitImage(1);
                    else if (banInfo.LimitedCount == 2) result = BanListRawDataViewModel.Instance.GetLimitImage(2);
                    else result = null;
                }
                else return null;
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CardInstanceBanLimitImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null ||
                values[0] == DependencyProperty.UnsetValue ||
                values[1] == DependencyProperty.UnsetValue)
            {
                return null;
            }

            if (!(values[0] is CardInstance cardInstance) || !(values[1] is BanList banList) || banList == null)
            {
                return null;
            }

            CardBanList banInfo = null;
            bool existInList = false;

            if (banList.CardList.TryGetValue(cardInstance.Card.ID, out banInfo))
            {
                existInList = true;
            }
            else if (banList.CardList.TryGetValue(cardInstance.Card.BaseCard.alias, out banInfo))
            {
                existInList = true;
            }

            BitmapImage result = null;

            if (banList.WhiteList) // Không có trong CardList => Cấm.
            {
                if (existInList)
                {
                    if (banInfo.LimitedCount <= 0) result = BanListRawDataViewModel.Instance.GetLimitImage(0);
                    else if (banInfo.LimitedCount == 1) result = BanListRawDataViewModel.Instance.GetLimitImage(1);
                    else if (banInfo.LimitedCount == 2) result = BanListRawDataViewModel.Instance.GetLimitImage(2);
                    else result = BanListRawDataViewModel.Instance.GetLimitImage(3);
                }
                else result = BanListRawDataViewModel.Instance.GetLimitImage(0);
            }
            else // Không có trong CardList => Không hiển thị.
            {
                if (existInList)
                {
                    if (banInfo.LimitedCount <= 0) result = BanListRawDataViewModel.Instance.GetLimitImage(0);
                    else if (banInfo.LimitedCount == 1) result = BanListRawDataViewModel.Instance.GetLimitImage(1);
                    else if (banInfo.LimitedCount == 2) result = BanListRawDataViewModel.Instance.GetLimitImage(2);
                    else result = null;
                }
                else return null;
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BanLimitVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var limitText = new CardBanLimitConverter().Convert(values, typeof(string), parameter, culture) as string;

            return string.IsNullOrEmpty(limitText) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
