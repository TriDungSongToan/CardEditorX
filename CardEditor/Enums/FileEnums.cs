namespace CardEditor.Enums
{
    public enum CardListFormat
    {
        YGONoFlag = 0,
        YGOHasFlag = 1,
        OMEGA = 2,
        BanList = 3,
    }
    public enum FileType
    {
        Database = 0,
        Excel = 1,
        Ceds = 2,
    }
    public enum FileLocation
    {
        PhysicalFile = 0,
        ZipEntry = 1,
        DBCell = 2,
        DBCellZipEntry = 3,
    }
}
