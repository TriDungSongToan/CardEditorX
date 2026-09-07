using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Enums
{
    #region Image
    public enum AppImage
    {
        Error,
        Information,
        Notification,
        Question,
        Warning,

        Logo,
        Blank,
        LevelStar,
        RankStar,
        LevelRankStar,
    }
    public enum ImageFormat
    {
        PNG,
        JPG,
        JPEG,
        WEBP,
        DNG,
        HEIF,
        AVIF,
        JPEGXL
    }
    public enum OutPutImage
    {
        pics,
        picsGene,
    }
    #endregion

    #region StampPosition
    public enum StampPosition
    {
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4,
        Center = 5,
        Unknown = 6
    }
    public static class StampPositionExtensions
    {
        public static string ToFriendlyString(this StampPosition position)
        {
            return position switch
            {
                StampPosition.TopLeft => CMess.topLeft.ToText(),
                StampPosition.TopRight => CMess.topRight.ToText(),
                StampPosition.BottomLeft => CMess.bottomLeft.ToText(),
                StampPosition.BottomRight => CMess.bottomRight.ToText(),
                StampPosition.Center => CMess.center.ToText(),
                _ => CMess.unknown.ToText(),
            };
        }
    }
    #endregion
}
