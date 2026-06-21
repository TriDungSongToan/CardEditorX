using CardEditor.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using SkiaSharp;
using MessageBox = System.Windows.MessageBox;
using System.Net.NetworkInformation;
using ControlzEx.Standard;
using MaterialDesignThemes.Wpf;

namespace CardEditor.ViewModels
{
    //public class DataViewModel : INotifyPropertyChanged
    //{
        //private static DataViewModel _instance;
        //private static DataViewModel Instance => _instance ??= new DataViewModel();
        //private string ExeFilePath => CardEditor.Models.AppContext.Instance.ExeFilePath;
        //private string DataFolderPath => CardEditor.Models.AppContext.Instance.DataFolderPath;

        //private Brush _background;
        //private Brush _foreground;
        //private Brush _themeColor;
        //private FontFamily _fontFamily;
        //private int _fontSize;

        //private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        //private TextAlignment _textAlignment = TextAlignment.Left;



        //private DataViewModel()
        //{

        //}


        
        //public Brush Background
        //{
        //    get => _background;
        //    set
        //    {
        //        if (_background != value)
        //        {
        //            _background = value;
        //            OnPropertyChanged(nameof(Background));
        //        }
        //    }
        //}
        //public Brush Foreground
        //{
        //    get => _foreground;
        //    set
        //    {
        //        if(_foreground != value)
        //        {
        //            _foreground = value;
        //            OnPropertyChanged(nameof(Foreground));
        //        }
        //    }
        //}
        //public Brush ThemeColor
        //{
        //    get => _themeColor;
        //    set
        //    {
        //        if(_themeColor != value)
        //        {
        //            _themeColor = value;
        //            OnPropertyChanged(nameof(ThemeColor));
        //        }
        //    }
        //}
        //public FontFamily FontFamily
        //{
        //    get => _fontFamily;
        //    set
        //    {
        //        if(_fontFamily != value)
        //        {
        //            _fontFamily = value;
        //            OnPropertyChanged(nameof(FontFamily));
        //        }
        //    }
        //}
        //public int FontSize
        //{
        //    get => _fontSize;
        //    set
        //    {
        //        if (_fontSize != value)
        //        {
        //            _fontSize = value;
        //            OnPropertyChanged(nameof(FontSize));
        //        }
        //    }
        //}

        //public FlowDirection FlowDirectionC
        //{
        //    get => _flowDirection;
        //    set
        //    {
        //        if (_flowDirection != value)
        //        {
        //            _flowDirection = value;
        //            OnPropertyChanged(nameof(FlowDirectionC));
        //        }
        //    }
        //}
        //public TextAlignment TextAlignmentC
        //{
        //    get => _textAlignment;
        //    set
        //    {
        //        if (_textAlignment != value)
        //        {
        //            _textAlignment = value;
        //            OnPropertyChanged(nameof(TextAlignmentC));
        //        }
        //    }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
        //public event EventHandler<string> ThemeChanged;
        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

    //}
}
