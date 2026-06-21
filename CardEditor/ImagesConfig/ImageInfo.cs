using SkiaSharp;

namespace CardEditor.ImagesConfig
{
    /// <summary>
    /// Original information for Image Generator
    /// </summary>

    public class ImageInfo
    {
        #region Card Size
        public int CardWidth;
        public int CardHeight;
        #endregion

        #region BackGround
        public int BackgroundWidth = 1296;
        public int BackgroundHeight = 1934;
        public SKRect BackgroundRect;
        public SKRect FrameRect;
        #endregion

        #region ArtWork
        public int AWNorWidth;
        public int AWPenWidth;
        public int AWPenMinHeight;
        public SKPoint PenPosition;
        public SKRect AWNormalRect;
        public SKRect AWPendulumRect;
        public SKRect? AWFullSquareTransparenRect { get; set; }
        public SKRect? AWFullSquareOpaqueRect { get; set; }
        #endregion

        #region Effect Box
        public SKRect EffectBoxNor;
        public SKRect EffectBoxPen;
        #endregion

        #region Attribute
        public SKRect AttriRect;
        public SKRect SpellTrap;
        #endregion

        #region Level
        public SKRect LevelNormal;
        public SKRect LevelLink;
        #endregion

        #region Rarity Label
        public SKRect? RarityLabelRect;
        #endregion

        #region Name
        public SKRect NameBoxRect;
        public SKRect NameRectOCG;
        public SKRect NameRectTCG;
        #endregion

        #region Pendulum
        public SKRect PenScaleLeftRect;
        public SKRect PenScaleRightRect;
        public SKRect PenEffectRect;
        #endregion

        #region Description
        public SKRect PenDescRect;
        public SKRect MonsterTypeRect;
        public SKRect MonsterDescRect;
        public SKRect SpellTrapDescRect;
        #endregion

        #region Power
        public SKRect PowerRect;
        public SKRect ATK1Rect;
        public SKRect ATK2Rect;
        public SKRect DEFRect;
        public SKRect LinkRect;
        #endregion

        #region Pass
        public SKRect PassRect;
        public SKRect PassRectPenLink;
        #endregion

        #region Font Size
        public int FontSizeName;
        public int FontSizePenScale;
        public int FontSizePenEffect;
        public int FontSizeDesc;
        public int FontSizeMonsterType;
        public int FontSizeATK;
        public int FontSizeDEF;
        public int FontSizeLink;
        public int FontSizePass;
        #endregion
    }

    public static class Image1Config
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class Image2Config
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class Image38Config
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class Image9Config
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,

                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class Image10Config
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class ImageRushConfig
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class ImageForKidConfig
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }
    public static class ImageMangaConfig
    {
        public static ImageInfo Create()
        {
            return new ImageInfo
            {
                CardWidth = 1388,
                CardHeight = 2026,

                BackgroundWidth = 1296,
                BackgroundHeight = 1934,
                BackgroundRect = new SKRect(46, 46, 46 + 1296, 46 + 1934),
                FrameRect = new SKRect(0, 0, 1388, 2026),

                AWNorWidth = 1053,
                AWPenWidth = 1205,
                AWPenMinHeight = 1139,
                PenPosition = new SKPoint(90, 362),
                AWNormalRect = new SKRect(167, 371, 167 + 1053, 371 + 1053),
                AWPendulumRect = new SKRect(90, 362, 90 + 1205, 362 + 1139),

                EffectBoxNor = new SKRect(0, 1455, 1388, 1455 + 505),
                EffectBoxPen = new SKRect(0, 1215, 1388, 1215 + 760),

                AttriRect = new SKRect(1158, 88, 1158 + 134, 88 + 134),
                SpellTrap = new SKRect(0, 0, 1315, 350),

                LevelNormal = new SKRect(0, 244, 1388, 244 + 88),
                LevelLink = new SKRect(0, 238, 1388, 238 + 88),

                NameBoxRect = new SKRect(0, 0, 1388, 295),
                NameRectOCG = SKRect.Create(104, 91, 1034, 130),
                NameRectTCG = SKRect.Create(104, 91, 1034, 130),

                PenScaleLeftRect = SKRect.Create(91, 1384, 97, 90),
                PenScaleRightRect = SKRect.Create(1198, 1384, 97, 90),
                PenEffectRect = SKRect.Create(105, 126, 1030, 90),

                PenDescRect = SKRect.Create(215, 1280, 955, 215),
                MonsterTypeRect = SKRect.Create(100, 1508, 1190, 80),
                MonsterDescRect = SKRect.Create(105, 1580, 1173, 260),
                SpellTrapDescRect = SKRect.Create(105, 1527, 1173, 362),

                PowerRect = SKRect.Create(0, 1831, 1388, 76),
                ATK1Rect = SKRect.Create(590, 1852, 130, 45),
                ATK2Rect = SKRect.Create(873, 1852, 130, 45),
                DEFRect = SKRect.Create(1155, 1852, 130, 45),
                LinkRect = SKRect.Create(1240, 1848, 40, 45),

                PassRect = SKRect.Create(62, 1927, 200, 50),
                PassRectPenLink = SKRect.Create(185, 1927, 200, 50),

                FontSizeName = 165,
                FontSizePenScale = 100,
                FontSizePenEffect = 42,
                FontSizeDesc = 42,
                FontSizeMonsterType = 52,
                FontSizeATK = 78,
                FontSizeDEF = 78,
                FontSizeLink = 50,
                FontSizePass = 40,
            };
        }
    }









    public class Image10Info1
    {
        #region Card Size
        public const int CardWidth = 1388;
        public const int CardHeight = 2026;
        #endregion

        #region BackGround
        public const int BackgroundWidth = 1296;
        public const int BackgroundHeight = 1934;
        public static SKRect BackgroundRect { get; } = new SKRect(46, 46, 46 + 1296, 46 + 1934);
        public static SKRect FrameRect { get; } = new SKRect(0, 0, 1388, 2026);
        #endregion

        #region ArtWork
        public static int AW10NorWidth { get; } = 1053;
        public static int AW10PenWidth { get; } = 1205;
        public static int AW10PenMinHeight { get; } = 1139;
        public static SKPoint PenPosition { get; } = new SKPoint(90, 362);
        public static SKRect AWNormalRect { get; } = new SKRect(167, 371, 167 + 1053, 371 + 1053);
        public static SKRect AWPendulumRect { get; } = new SKRect(90, 362, 90 + 1205, 362 + 1139);
        //public static SKRect? AWFullSquareTransparenRect { get; } = new SKRect(0, 115, 1388, 115 + 1388);
        public static SKRect? AWFullSquareTransparenRect { get; set; }
        //public static SKRect? AWFullSquareOpaqueRect { get; } = new SKRect(46, 235, 46 + 1296, 235 + 1296);
        public static SKRect? AWFullSquareOpaqueRect { get; set; }
        #endregion

        #region Effect Box
        public static SKRect EffectBoxNor { get; } = new SKRect(0, 1455, 1388, 1455 + 505);
        public static SKRect EffectBoxPen { get; } = new SKRect(0, 1215, 1388, 1215 + 760);
        #endregion

        #region Attribute
        public static SKRect AttriRect { get; } = new SKRect(1158, 88, 1158 + 134, 88 + 134);
        public static SKRect SpellTrap { get; } = new SKRect(0, 0, 1315, 350);
        #endregion

        #region Level
        public static SKRect Level (bool isLink)
        {
            float x = 0;
            float y = isLink ? 238 : 244;
            float width = 1388;
            float height = y + 88;

            return new SKRect(x, y, width, height);
        }
        #endregion

        #region Rarity Label
        public static SKRect? RarityLabelRect { get; set; }
        #endregion

        //public struct TextLayoutConfig
        //{
        //    public float FontSize;
        //    public float WordSpacing;
        //    public float LineSpacing;
        //}

        public static SKRect NameBoxRect { get; } = new SKRect(0, 0, 1388, 295);
        public static SKRect NameRectOCG { get; set; } = SKRect.Create(104, 91, 1034, 130);
        public static SKRect NameRectTCG { get; set; } = SKRect.Create(104, 91, 1034, 130);

        #region Pendulum
        public static SKRect PenScaleLeftRect { get; set; } = SKRect.Create(91, 1384, 97, 90);
        public static SKRect PenScaleRightRect { get; set; } = SKRect.Create(1198, 1384, 97, 90);
        public static SKRect PenEffectRect { get; set; } = SKRect.Create(105, 126, 1030, 90);
        #endregion

        #region Description
        public static SKRect PenDescRect { get; set; } = SKRect.Create(215, 1280, 955, 215);
        public static SKRect MonsterTypeRect { get; set; } = SKRect.Create(100, 1508, 1190, 80);
        public static SKRect MonsterDescRect { get; set; } = SKRect.Create(105, 1580, 1173, 260);
        public static SKRect SpellTrapDescRect { get; set; } = SKRect.Create(105, 1527, 1173, 362);
        #endregion

        #region Power
        public static SKRect PowerRect { get; } = SKRect.Create(0, 1831, 1388, 76);


        // ATK location of ATK-DEF-Link
        public static SKRect ATK1Rect { get; } = SKRect.Create(590, 1852, 130, 45);

        // ATK location of ATK-DEF/ATK-Link
        // DEF location of ATK-DEF-Link/DEF-Link
        public static SKRect ATK2Rect { get; } = SKRect.Create(873, 1852, 130, 45);

        // DEF location of ATK-DEF
        public static SKRect DEFRect { get; } = SKRect.Create(1155, 1852, 130, 45);

        // LINK location of Link
        public static SKRect LinkRect { get; } = SKRect.Create(1240, 1848, 40, 45);
        #endregion

        #region Pass
        public static SKRect PassRect { get; set; } = SKRect.Create(62, 1927, 200, 50);
        public static SKRect PassRectPenLink { get; set; } = SKRect.Create(185, 1927, 200, 50);

        #endregion

        #region Font Size
        public static int FontSizeName = 165;
        public static int FontSizePenScale = 100;
        public static int FontSizePenEffect = 42;
        public static int FontSizeDesc = 42;
        public static int FontSizeMonsterType = 52;
        public static int FontSizeATK = 78;
        public static int FontSizeDEF = 78;
        public static int FontSizeLink = 50;
        public static int FontSizePass = 40;
        #endregion

    }
}
