using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Collections;

namespace CardEditor.Models.Settings
{
    public class ImageSettingSource : INotifyPropertyChanged
    {
        private BulkObservableCollection<string> _series;
        public BulkObservableCollection<string> Series
        {
            get => _series;
            set
            {
                if (!ReferenceEquals(_series, value))
                {
                    _series = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged(nameof(Series));
                }
            }
        }
        private BulkObservableCollection<string> _foildLists;
        public BulkObservableCollection<string> FoildLists
        {
            get => _foildLists;
            set
            {
                if (!ReferenceEquals(_foildLists, value))
                {
                    _foildLists = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged(nameof(FoildLists));
                }
            }
        }
        private BulkObservableCollection<string> _backgroundArts;
        public BulkObservableCollection<string> BackgroundArts
        {
            get => _backgroundArts;
            set
            {
                if (!ReferenceEquals(_backgroundArts, value))
                {
                    _backgroundArts = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged(nameof(BackgroundArts));
                }
            }
        }
        private BulkObservableCollection<CardMaker> _cardMakers;
        public BulkObservableCollection<CardMaker> CardMakers
        {
            get => _cardMakers;
            set
            {
                if (!ReferenceEquals(_cardMakers, value))
                {
                    _cardMakers = value ?? new BulkObservableCollection<CardMaker>();
                    OnPropertyChanged(nameof(CardMakers));
                }
            }
        }
        private BulkObservableCollection<ImageFormat> _imageFormats;
        public BulkObservableCollection<ImageFormat> ImageFormats
        {
            get => _imageFormats;
            set
            {
                if (!ReferenceEquals(_imageFormats, value))
                {
                    _imageFormats = value ?? new BulkObservableCollection<ImageFormat>();
                    OnPropertyChanged(nameof(ImageFormats));
                }
            }
        }
        private BulkObservableCollection<OutPutImage> _outPutImages;
        public BulkObservableCollection<OutPutImage> OutPutImages
        {
            get => _outPutImages;
            set
            {
                if (!ReferenceEquals(_outPutImages, value))
                {
                    _outPutImages = value ?? new BulkObservableCollection<OutPutImage>();
                    OnPropertyChanged(nameof(OutPutImages));
                }
            }
        }
        public ImageSettingSource()
        {
            _series = new BulkObservableCollection<string>();
            _foildLists = new BulkObservableCollection<string>();
            _backgroundArts = new BulkObservableCollection<string>();
            _cardMakers = new BulkObservableCollection<CardMaker>();
            _imageFormats = new BulkObservableCollection<ImageFormat>();
            _outPutImages = new BulkObservableCollection<OutPutImage>();
        }
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _series ??= new BulkObservableCollection<string>();
            _foildLists ??= new BulkObservableCollection<string>();
            _backgroundArts ??= new BulkObservableCollection<string>();
            _cardMakers ??= new BulkObservableCollection<CardMaker>();
            _imageFormats ??= new BulkObservableCollection<ImageFormat>();
            _outPutImages ??= new BulkObservableCollection<OutPutImage>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ImageSetting : INotifyPropertyChanged
    {
        private string _artWorkFolder = string.Empty;
        public string ArtworkFolder
        {
            get => _artWorkFolder;
            set
            {
                if (_artWorkFolder != value)
                {
                    _artWorkFolder = value;
                    OnPropertyChanged(nameof(ArtworkFolder));
                }
            }
        }
        private string _outPutFolder = string.Empty;
        public string OutPutFolder
        {
            get => _outPutFolder;
            set
            {
                if (_outPutFolder != value)
                {
                    _outPutFolder = value;
                    OnPropertyChanged(nameof(OutPutFolder));
                }
            }
        }
        private OutPutImage _selectedOutPut = OutPutImage.pics;
        public OutPutImage SelectedOutPut
        {
            get => _selectedOutPut;
            set
            {
                if (_selectedOutPut != value)
                {
                    _selectedOutPut = value;
                    OnPropertyChanged(nameof(SelectedOutPut));
                }
            }
        }
        private string _originalCardFolder = string.Empty;
        public string OriginalCardFolder
        {
            get => _originalCardFolder;
            set
            {
                if (_originalCardFolder != value)
                {
                    _originalCardFolder = value;
                    OnPropertyChanged(nameof(OriginalCardFolder));
                }
            }
        }
        private string _downloadedCardFolder = string.Empty;
        public string DownloadedCardFolder
        {
            get => _downloadedCardFolder;
            set
            {
                if (_downloadedCardFolder != value)
                {
                    _downloadedCardFolder = value;
                    OnPropertyChanged(nameof(DownloadedCardFolder));
                }
            }
        }
        private CardMaker _selectedCardMaker = new CardMaker();
        public CardMaker SelectedCardMaker
        {
            get => _selectedCardMaker;
            set
            {
                if (_selectedCardMaker != value)
                {
                    _selectedCardMaker = value;
                    OnPropertyChanged(nameof(SelectedCardMaker));
                }
            }
        }
        //////////////
        private Size _imageSize = new Size(1388, 2026);
        public Size ImageSize
        {
            get => _imageSize;
            set
            {
                if (_imageSize != value)
                {
                    _imageSize = value;
                    _imageSizeString = $"{_imageSize.Width},{_imageSize.Height}";
                    OnPropertyChanged(nameof(ImageSize));
                    OnPropertyChanged(nameof(ImageSizeString));
                }
            }
        }
        private string _imageSizeString = "1388,2026";
        [JsonIgnore]
        public string ImageSizeString
        {
            get => _imageSizeString;
            set
            {
                if (_imageSizeString != value)
                {
                    _imageSizeString = value;
                    var parts = _imageSizeString.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                    {
                        _imageSize = new Size(w, h);
                        OnPropertyChanged(nameof(ImageSize));
                    }
                    OnPropertyChanged(nameof(ImageSizeString));
                }
            }
        }
        //////////////
        private Size _stampSize = new Size(350, 100);
        public Size StampSize
        {
            get => _stampSize;
            set
            {
                if (_stampSize != value)
                {
                    _stampSize = value;
                    _stampSizeString = $"{_stampSize.Width},{_stampSize.Height}";
                    OnPropertyChanged(nameof(StampSize));
                    OnPropertyChanged(nameof(StampSizeString));
                }
            }
        }
        private string _stampSizeString = "350,100";
        [JsonIgnore]
        public string StampSizeString
        {
            get => _stampSizeString;
            set
            {
                if (_stampSizeString != value)
                {
                    _stampSizeString = value;
                    var parts = _stampSizeString.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                    {
                        _stampSize = new Size(w, h);
                        OnPropertyChanged(nameof(StampSize));
                    }
                    OnPropertyChanged(nameof(StampSizeString));
                }
            }
        }
        //////////////
        private int _stampPosition = 0;
        public int StampPosition
        {
            get => _stampPosition;
            set
            {
                if (_stampPosition != value)
                {
                    _stampPosition = value;
                    OnPropertyChanged(nameof(StampPosition));
                }
            }
        }
        //////////////
        private Point _stampMarrgin = new Point(0, 0);
        public Point StampMarrgin
        {
            get => _stampMarrgin;
            set
            {
                if (_stampMarrgin != value)
                {
                    _stampMarrgin = value;
                    _stampMarrginString = $"{_stampMarrgin.X},{_stampMarrgin.Y}";
                    OnPropertyChanged(nameof(StampMarrgin));
                    OnPropertyChanged(nameof(StampMarrginString));
                }
            }
        }
        private string _stampMarrginString = "0,0";
        [JsonIgnore]
        public string StampMarrginString
        {
            get => _stampMarrginString;
            set
            {
                if (_stampMarrginString != value)
                {
                    _stampMarrginString = value;
                    var parts = _stampMarrginString.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                    {
                        _stampMarrgin = new Point((int)x, (int)y);
                        OnPropertyChanged(nameof(StampMarrgin));
                    }
                    OnPropertyChanged(nameof(StampMarrginString));
                }
            }
        }
        //////////////
        private int _formatName = 0;
        public int FormatName
        {
            get => _formatName;
            set
            {
                if (_formatName != value)
                {
                    _formatName = value;
                    OnPropertyChanged(nameof(FormatName));
                }
            }
        }
        private int _formatEffect = 0;
        public int FormatEffect
        {
            get => _formatEffect;
            set
            {
                if (_formatEffect != value)
                {
                    _formatEffect = value;
                    OnPropertyChanged(nameof(FormatEffect));
                }
            }
        }
        private ImageFormat _outPutFormat = ImageFormat.PNG;
        public ImageFormat OutPutFormat
        {
            get => _outPutFormat;
            set
            {
                if (_outPutFormat != value)
                {
                    _outPutFormat = value;
                    OnPropertyChanged(nameof(OutPutFormat));
                }
            }
        }

        private int _series = 4;
        public int Series
        {
            get => _series;
            set
            {
                if(_series != value)
                {
                    _series = value;
                    OnPropertyChanged(nameof(Series));
                }
            }
        }
        private int _rare = 0;
        public int Rare
        {
            get => _rare;
            set
            {
                if (_rare != value)
                {
                    _rare = value;
                    OnPropertyChanged(nameof(Rare));
                }
            }
        }
        private int _secret = 0;
        public int Secret
        {
            get => _secret;
            set
            {
                if (_secret != value)
                {
                    _secret = value;
                    OnPropertyChanged(nameof(Secret));
                }
            }
        }
        private string _foild = "None";
        public string Foild
        {
            get => _foild;
            set
            {
                if (_foild != value)
                {
                    _foild = value;
                    OnPropertyChanged(nameof(Foild));
                }
            }
        }
        private string _backgroundArt = "None";
        public string BackgroundArt
        {
            get => _backgroundArt;
            set
            {
                if (_backgroundArt != value)
                {
                    _backgroundArt = value;
                    OnPropertyChanged(nameof(BackgroundArt));
                }
            }
        }
        private bool _includeRare = false;
        public bool IncludeRare
        {
            get => _includeRare;
            set
            {
                if (_includeRare != value)
                {
                    _includeRare = value;
                    OnPropertyChanged(nameof(IncludeRare));
                }
            }
        }

        private bool _fullArt = false;
        public bool FullArt
        {
            get => _fullArt;
            set
            {
                if (_fullArt != value)
                {
                    _fullArt = value;
                    OnPropertyChanged(nameof(FullArt));
                }
            }
        }
        /// <summary>
        /// /////////
        /// </summary>
        private int _yAWFullTrans = 115;
        public int YAWFullTrans
        {
            get => _yAWFullTrans;
            set
            {
                if (_yAWFullTrans != value)
                {
                    _yAWFullTrans = value;
                    _yAWFullTransString = value.ToString();
                    OnPropertyChanged(nameof(YAWFullTrans));
                    OnPropertyChanged(nameof(YAWFullTransString));
                }
            }
        }
        private string _yAWFullTransString = "115";
        [JsonIgnore]
        public string YAWFullTransString
        {
            get => _yAWFullTransString;
            set
            {
                if (_yAWFullTransString != value)
                {
                    _yAWFullTransString = value;
                    if (int.TryParse(value, out int v))
                    {
                        YAWFullTrans = v;
                    }
                    OnPropertyChanged(nameof(YAWFullTransString));
                }
            }
        }

        private int _yAWFullOpaque = 235;
        public int YAWFullOpaque
        {
            get => _yAWFullOpaque;
            set
            {
                if (_yAWFullOpaque != value)
                {
                    _yAWFullOpaque = value;
                    _yAWFullOpaqueString = value.ToString();
                    OnPropertyChanged(nameof(YAWFullOpaque));
                    OnPropertyChanged(nameof(YAWFullOpaqueString));
                }
            }
        }
        private string _yAWFullOpaqueString = "235";
        [JsonIgnore]
        public string YAWFullOpaqueString
        {
            get => _yAWFullOpaqueString;
            set
            {
                if (_yAWFullOpaqueString != value)
                {
                    _yAWFullOpaqueString = value;
                    if (int.TryParse(value, out int v))
                    {
                        YAWFullOpaque = v;
                    }
                    OnPropertyChanged(nameof(YAWFullOpaqueString));
                }
            }
        }

        private int _heightFullTrans = 1388;
        public int HeightFullTrans
        {
            get => _heightFullTrans;
            set
            {
                if (_heightFullTrans != value)
                {
                    _heightFullTrans = value;
                    _heightFullTransString = value.ToString();
                    OnPropertyChanged(nameof(HeightFullTrans));
                    OnPropertyChanged(nameof(HeightFullTransString));
                }
            }
        }
        private string _heightFullTransString = "1388";
        [JsonIgnore]
        public string HeightFullTransString
        {
            get => _heightFullTransString;
            set
            {
                if (_heightFullTransString != value)
                {
                    _heightFullTransString = value;
                    if (int.TryParse(value, out int v))
                    {
                        HeightFullTrans = v;
                    }
                    OnPropertyChanged(nameof(HeightFullTransString));
                }
            }
        }

        private int _heightFullOpaque = 1296;
        public int HeightFullOpaque
        {
            get => _heightFullOpaque;
            set
            {
                if (_heightFullOpaque != value)
                {
                    _heightFullOpaque = value;
                    _heightFullOpaqueString = value.ToString();
                    OnPropertyChanged(nameof(HeightFullOpaque));
                    OnPropertyChanged(nameof(HeightFullOpaqueString));
                }
            }
        }
        private string _heightFullOpaqueString = "1296";
        [JsonIgnore]
        public string HeightFullOpaqueString
        {
            get => _heightFullOpaqueString;
            set
            {
                if (_heightFullOpaqueString != value)
                {
                    _heightFullOpaqueString = value;
                    if (int.TryParse(value, out int v))
                    {
                        HeightFullOpaque = v;
                    }
                    OnPropertyChanged(nameof(HeightFullOpaqueString));
                }
            }
        }

        public ImageSetting Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<ImageSetting>(json)!;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class CardMaker
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
