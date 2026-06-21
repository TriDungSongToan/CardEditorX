using System.Linq;
using System.Windows;

namespace CardEditor.Services
{
    public interface IImageViewerService
    {
        void ShowImage(string imagePath);
    }
    public class ImageViewerService : IImageViewerService
    {
        public void ShowImage(string imagePath)
        {
            var viewer = new ImageViewerWindow(imagePath);
            viewer.Owner = Application.Current.Windows
                            .OfType<Window>()
                            .FirstOrDefault(w => w.IsActive);
            viewer.ShowDialog();
        }
    }
}
