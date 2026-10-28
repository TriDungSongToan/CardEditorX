using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections;
using CardEditor.Enums;
using CardEditor.Models;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for FeatureCard.xaml
    /// </summary>
    public partial class FeatureCard : UserControl, IDisposable
    {
        public event EventHandler<RecentItem> ItemOpened;
        public event EventHandler<RecentFileType> CardTypeSelected;

        public static readonly DependencyProperty FeatureTypeProperty =
            DependencyProperty.Register(nameof(FeatureType), typeof(RecentFileType), typeof(FeatureCard));
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FeatureCard));
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(FeatureCard));
        public static readonly DependencyProperty FeatureNameProperty =
            DependencyProperty.Register(nameof(FeatureName), typeof(string), typeof(FeatureCard));
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(FeatureCard));

        public RecentFileType FeatureType { get => (RecentFileType)GetValue(FeatureTypeProperty); set => SetValue(FeatureTypeProperty, value); }
        public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
        public ImageSource Icon { get => (ImageSource)GetValue(IconProperty); set => SetValue(IconProperty, value); }
        public string FeatureName { get => (string)GetValue(FeatureNameProperty); set => SetValue(FeatureNameProperty, value); }
        public string EmptyText { get => (string)GetValue(EmptyTextProperty); set => SetValue(EmptyTextProperty, value); }

        public FeatureCard()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is RecentItem item)
                ItemOpened?.Invoke(this, item);
        }
        private void GrLogo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CardTypeSelected?.Invoke(this, FeatureType);
        }

        private void root_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {

        }
    }
}
