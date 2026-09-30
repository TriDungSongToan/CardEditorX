using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

namespace CardEditor.ViewModels
{
    public class HomeImageViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<HomeImageViewModel> _instance = new Lazy<HomeImageViewModel>(() => new HomeImageViewModel());
        public static HomeImageViewModel Instance => _instance.Value;

        private readonly SemaphoreSlim _initLockTitle = new(1, 1);
        private readonly SemaphoreSlim _initLockLogo = new(1, 1);
        public bool IsLoadedTitle;
        public bool IsLoadedLogo;

        #region Data Storage
        private readonly List<ImageBrush> _cardBrushes;
        public IReadOnlyList<ImageBrush> CardBrushes => _cardBrushes;

        public BitmapImage LogoCircle { get; set; }
        public BitmapImage LogoX { get; set; }
        #endregion

        #region Constructor
        public HomeImageViewModel()
        {
            _cardBrushes = new List<ImageBrush>(10);
        }
        #endregion

        #region Methods
        public async Task<(bool Success, string Error)> LoadBrushes()
        {
            if (IsLoadedTitle) return (true, string.Empty);
            await _initLockTitle.WaitAsync();
            try
            {
                if (IsLoadedTitle) return (true, string.Empty);
                const int count = 10;

                var brushes = await Task.Run(() =>
                {
                    var result = new List<ImageBrush>(count);

                    for (int i = 0; i < count; i++)
                    {
                        string path = i == 0
                        ? "pack://application:,,,/Images/Home/cardBack.png"
                        : $"pack://application:,,,/Images/Home/cardFace{i}.png";

                        var bmp = new BitmapImage();

                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bmp.UriSource = new Uri(path, UriKind.Absolute);
                        bmp.DecodePixelWidth = 240;

                        bmp.EndInit();
                        bmp.Freeze();

                        var brush = new ImageBrush(bmp)
                        {
                            Stretch = Stretch.UniformToFill
                        };
                        brush.Freeze();

                        result.Add(brush);
                    }

                    return result;
                });

                _cardBrushes.AddRange(brushes);
                IsLoadedTitle = true;
                OnPropertyChanged(nameof(CardBrushes));
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                _initLockTitle.Release();
            }
        }
        public async Task<(bool Success, string Error)> LoadLogo()
        {
            if (IsLoadedLogo) return (true, string.Empty);
            await _initLockLogo.WaitAsync();
            try
            {
                if (IsLoadedLogo) return (true, string.Empty);

                var tasks = new[]
                {
                    Task.Run(() => LoadFrozen("pack://application:,,,/Images/Home/LogoCircle.png")),
                    Task.Run(() => LoadFrozen("pack://application:,,,/Images/Home/LogoX.png"))
                };

                var images = await Task.WhenAll(tasks);
                LogoCircle = images[0];
                LogoX = images[1];

                IsLoadedLogo = true;

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                _initLockLogo.Release();
            }
        }
        private static BitmapImage LoadFrozen(string uri)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.UriSource = new Uri(uri, UriKind.Absolute);
            bmp.DecodePixelWidth = 100;

            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _cardBrushes.Clear();
            LogoCircle = null;
            LogoX = null;
        }
        #endregion

    }
}
