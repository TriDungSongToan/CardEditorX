using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Enums
{
    #region FlowDirection
    public enum FlowDirection
    {
        LeftToRight = 1,
        RightToLeft = 2
    }
    public static class FlowDirectionExtensions
    {
        public static string ToFriendlyString(this FlowDirection direction)
        {
            return direction switch
            {
                FlowDirection.LeftToRight => CMess.LeftToRight.ToText(),
                FlowDirection.RightToLeft => CMess.RightToLeft.ToText(),
                _ => CMess.LeftToRight.ToText()
            };
        }
    }
    #endregion

    #region TextAlignment
    public enum TextAlignment
    {
        Left = 1,
        Right = 2,
        Justify = 3,
        Center = 4,
    }
    public static class TextAlignmentExtensions
    {
        public static string ToFriendlyString(this TextAlignment alignment)
        {
            return alignment switch
            {
                TextAlignment.Left => CMess.AligLeft.ToText(),
                TextAlignment.Right => CMess.AligRight.ToText(),
                TextAlignment.Justify => CMess.AligJustify.ToText(),
                TextAlignment.Center => CMess.AligCenter.ToText(),

                _ => CMess.AligLeft.ToText()
            };
        }
    }

    #endregion

}
