using System.Windows.Input;

namespace CardEditor.Commands
{
    public class CustomCommands
    {
        public static readonly RoutedCommand NewDatabase = new RoutedCommand("NewDatabase", typeof(CustomCommands));
        public static readonly RoutedCommand NewDeck = new RoutedCommand("NewDeck", typeof(CustomCommands));
        public static readonly RoutedCommand NewScript = new RoutedCommand("NewScript", typeof(CustomCommands));
        public static readonly RoutedCommand NewBanList = new RoutedCommand("NewBanList", typeof(CustomCommands));

        public static readonly RoutedCommand OpenArchive = new RoutedCommand("OpenArchive", typeof(CustomCommands));
        public static readonly RoutedCommand OpenDatabase = new RoutedCommand("OpenDatabase", typeof(CustomCommands));
        public static readonly RoutedCommand OpenDeck = new RoutedCommand("OpenDeck", typeof(CustomCommands));
        public static readonly RoutedCommand OpenScript = new RoutedCommand("OpenScript", typeof(CustomCommands));
        public static readonly RoutedCommand OpenBanList = new RoutedCommand("OpenBanList", typeof(CustomCommands));

        public static readonly RoutedCommand Save = new RoutedCommand("Save", typeof(CustomCommands));
        public static readonly RoutedCommand SaveAs = new RoutedCommand("SaveAs", typeof (CustomCommands));
        public static readonly RoutedCommand Setting = new RoutedCommand("Setting", typeof(CustomCommands));
        public static readonly RoutedCommand Exit = new RoutedCommand("Exit", typeof(CustomCommands));

        // public static readonly RoutedCommand SaveCeds = new RoutedCommand("SaveCeds", typeof(CustomCommands));
    }
}
