using System.Collections.Generic;
using SkiaSharp;

namespace CardEditor.ImagesConfig
{
    public class LinkArrowDirection
    {
        public ulong Mask { get; set; }
        public string Code { get; set; }
        public SKPoint Position { get; set; }
    }

    public class LinkArrowInfo
    {
        public ulong SkipMark { get; set; }

        public List<LinkArrowDirection> LinkArrowNormalInfo { get; set; }
        public List<LinkArrowDirection> LinkArrowPenInfo { get; set; }
    }
    public static class LinkArrow1Config
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrow2Config
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrow38Config
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrow9Config
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrow10Config
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrowRushConfig
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrowForKidConfig
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }
    public static class LinkArrowMangaConfig
    {
        public static LinkArrowInfo Create()
        {
            return new LinkArrowInfo
            {
                SkipMark = 0x1 | 0x2 | 0x4,

                LinkArrowNormalInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
                },

                LinkArrowPenInfo = new List<LinkArrowDirection>
                {
                    new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                    new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                    new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                    new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                    // 
                    new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                    new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                    new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                    new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
                }
            };
        }
    }


    public static class LinkArrowInfo1
    {
        public const ulong SkipMark = 0x1 | 0x2 | 0x4;
        public static List<LinkArrowDirection> LinkArrowNormalInfo { get; set; }
        public static List<LinkArrowDirection> LinkArrowPenInfo { get; set; }

        static LinkArrowInfo1()
        {
            LinkArrowNormalInfo = new List<LinkArrowDirection>
            {
                new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(97, 1332) },
                new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(557, 1417) },
                new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1128, 1332) },
                new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(79, 760) },
                // 
                new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1215, 760) },
                new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(97, 302) },
                new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(557, 280) },
                new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1128, 302) },
            };
            LinkArrowPenInfo = new List<LinkArrowDirection>
            {
                new LinkArrowDirection { Mask = 0x1, Code = "bl", Position = new SKPoint(30, 1805) },
                new LinkArrowDirection { Mask = 0x2, Code = "b",  Position = new SKPoint(544, 1900) },
                new LinkArrowDirection { Mask = 0x4, Code = "br", Position = new SKPoint(1195, 1805) },
                new LinkArrowDirection { Mask = 0x8, Code = "l",  Position = new SKPoint(-6, 986) },
                // 
                new LinkArrowDirection { Mask = 0x20, Code = "r",  Position = new SKPoint(1290, 986) },
                new LinkArrowDirection { Mask = 0x40, Code = "tl", Position = new SKPoint(29, 302) },
                new LinkArrowDirection { Mask = 0x80, Code = "t",  Position = new SKPoint(544, 276) },
                new LinkArrowDirection { Mask = 0x100, Code = "tr", Position = new SKPoint(1195, 302) },
            };
        }
    }
}
