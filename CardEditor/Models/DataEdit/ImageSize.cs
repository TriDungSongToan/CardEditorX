using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Models
{
    public class ImageSize : INotifyPropertyChanged
    {
        private bool _isCalculating = false;
        private readonly double defaultWidth = 1388;
        private readonly double defaultHeight = 2026;

        private string _widthString = "1388";
        private string _heightString = "2026";
        private Size _size = new Size(1388, 2026);
        public string WidthString
        {
            get => _widthString;
            set
            {
                if (_widthString != value)
                {
                    _widthString = value;
                    OnPropertyChanged(nameof(WidthString));
                    if (!_isCalculating) CalculateImageSize();
                }
            }
        }
        public string HeightString
        {
            get => _heightString;
            set
            {
                if(_heightString != value)
                {
                    _heightString = value;
                    OnPropertyChanged(nameof(HeightString));
                    if (!_isCalculating) CalculateImageSize();
                }
            }
        }
        public Size Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    _isCalculating = true;
                    _widthString = _size.Width.ToString();
                    _heightString = _size.Height.ToString();

                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(WidthString));
                    OnPropertyChanged(nameof(HeightString));

                    _isCalculating = false;
                }
            }
        }

        public void CalculateImageSize()
        {
            _isCalculating = true;
            bool hasWidth = int.TryParse(_widthString, out int w);
            bool hasHeight = int.TryParse(_heightString, out int h);

            if (hasWidth && !hasHeight) // Chỉ có width → giữ width, tính height dựa trên tỉ lệ mặc định
            {
                _size.Width = w;
                _size.Height = (int)Math.Round(_size.Width * (defaultHeight / defaultWidth));
            }
            else if (!hasWidth && hasHeight) // Chỉ có height → giữ height, tính width dựa trên tỉ lệ mặc định
            {
                _size.Height = h;
                _size.Width = (int)Math.Round(_size.Height * (defaultWidth / defaultHeight));
            }
            else if (hasWidth && hasHeight) // Cả hai đều có giá trị → nếu width > height, hoán đổi
            {
                if (w > h)
                {
                    _size.Width = h;
                    _size.Height = w;
                }
                else
                {
                    _size.Width = w;
                    _size.Height = h;
                }
            }
            else
            {
                // Cả hai trống → dùng giá trị mặc định
                _size.Width = (int)defaultWidth;
                _size.Height = (int)defaultHeight;
            }
            _widthString = _size.Width.ToString();
            _heightString = _size.Height.ToString();

            OnPropertyChanged(nameof(WidthString));
            OnPropertyChanged(nameof(HeightString));
            OnPropertyChanged(nameof(Size));
            _isCalculating = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
