// ============================================================================
// ⚠️ IMPORTANT
//
// This enum is synchronized with the shared repository.
// DO NOT rename, remove, reorder, or change values directly in this project.
//
// Change procedure:
// 1. Update the shared repository.
// 2. Push the changes.
// 3. Update the shared dependency in this project.
// 4. Fix all references and rebuild the solution.
//
// Modifying this file without following the process will break
// localization, mappings, and other dependent components.
// ============================================================================

namespace ScriptSupport.Localization
{
    public enum Language : uint
    {
        #region Menu Item/Button

        #region menu Item
        File = 0x1,             //File
        tNew = 0x2,             //New
        Open = 0x3,             //Open
        Save = 0x4,             //Save
        SaveAs = 0x5,           //Save as
        Recently = 0xe,         //Recently Opened
        ClearHistory = 0xf,     //Clear History
        Exit = 0x11,            //Exit

        Window = 0x15,          //Window
        Home = 0x16,            //Home
        ImageEdit = 0x17,       //Image Editor
        DataEdit = 0x18,        //Data Editor
        CodeEdit = 0x19,        //Code Editor
        DeckEdit = 0x1a,        //Deck Editor
        BanListEdit = 0x1b,     //BanList Editor
        ItemsEdit = 0x1c,       //Items Editor
        ScriptSupport = 0x1d,   //Script Support

        Setting = 0x25,         //Setting
        Config = 0x26,          //Configuration

        Card = 0x31,            //Card
        CopyCard = 0x32,        //Copy Card
        PasteCard = 0x33,       //Paste Card
        SaveCard = 0x34,        //Save Card

        Data = 0x45,            //Data
        Export = 0x46,          //Export
        ExpoZIP = 0x47,         //Export as zip
        ExpoCeds = 0x48,        //Export CEDS
        ExpoExcel = 0x49,       //Export Excel

        Manager = 0x55,         //Manager

        Help = 0x6a,            //Help
        LinScript = 0x6b,       //Linter Script
        ChkUpdate = 0x6c,       //Check Update

        Cut = 0x81,             //Cut
        Copy = 0x82,            //Copy
        Paste = 0x83,           //Paste
        ToFull = 0x84,          //To Full Width
        ToHalf = 0x85,          //To Half Width
        ToSuper = 0x86,         //To SuperScript
        FromSuper = 0x87,       //From SuperScript
        ToSub = 0x88,           //To SubScript
        FromSub = 0x89,         //From SubScript
        SpecialChar = 0x8a,     //Special Characters

        #endregion

        #region Button
        ok = 0x101,             //OK
        cancel = 0x102,         //Cancel
        yes = 0x103,            //Yes
        no = 0x104,             //No
        load = 0x105,           //Load
        Apply = 0x106,          //Apply
        tlAdd = 0x107,          //Add
        add = 0x108,            //ADD IT, I Don't Care
        tlModify = 0x109,       //Modify
        Create = 0x10a,         //Create
        Insert = 0x10b,         //Insert
        tlClear = 0x10c,        //Clear
        tlDelete = 0x10d,       //Delete
        tlReset = 0x10e,        //Reset
        Refresh = 0x10f,        //Refresh
        Replace = 0x110,        //Replace
        Rename = 0x111,         //ReName
        tlReload = 0x112,       //ReLoad
        UpLoad = 0x113,         //UpLoad
        Update = 0x114,         //Update
        Remove = 0x115,         //Remove
        RollBack = 0x116,       //RollBack
        PreView = 0x117,        //Preview

        Read = 0x121,           //Read
        Write = 0x122,          //Write
        Import = 0x123,         //Import

        tlUndo = 0x124,         //Undo
        toolUndo = 0x125,       //Undo the last action to restore the previous state. Right click to redo.
        toolSearch = 0x126,     //Search/Filter Card in the list using data on the interface. Right click to open menu.
        clearFilter = 0x127,    //Clear Filter

        tlScript = 0x12a,       //Script
        Browse = 0x12b,         //Browse
        Shuffle = 0x12c,        //Shuffle
        YDKE = 0x12d,           //YDKE

        noSave = 0x12e,         //Don't Save
        noAsk = 0x12f,          //Don’t ask again
        CopyInfo = 0x130,       //Copy Info
        #endregion

        #endregion

        #region Config
        UserSetting = 0x201,        //User Settings
        UserName = 0x202,           //User Name
        Language = 0x203,           //Language
        DataSource = 0x204,         //Data Source
        Game = 0x205,               //Game
        browserPath = 0x206,        //Browser Path

        DisplaySetting = 0x211,     //Display Settings
        Background = 0x212,         //Background
        Foreground = 0x213,         //Foreground
        Theme = 0x214,              //Theme
        FontFamily = 0x215,         //FontFamily
        FontSize = 0x216,           //FontSize
        Highlight = 0x217,          //Highlight
        FlowDirection = 0x218,      //Flow Direction
        LeftToRight = 0x219,        //Left To Right
        RightToLeft = 0x21a,        //Right To Left
        TextAlignment = 0x21b,      //Text Alignment
        AligLeft = 0x21c,           //Left
        AligRight = 0x21d,          //Right
        AligJustify = 0x21e,        //Justify
        AligCenter = 0x21f,         //Center
        WordWrap = 0x220,           //Word Wrap
        CodeFold = 0x221,           //Code Folding

        Sort = 0x22a,           //Sort
        SortSetting = 0x22b,    //Sort Settings
        SortBy = 0x22c,         //Sort By
        SortOrder = 0x22d,      //Order By
        Ascending = 0x22e,      //Ascending
        Descending = 0x22f,     //Descending

        DataHandling = 0x235,   //Data Handling
        WriteMode = 0x236,      //Write Mode
        AskMe = 0x237,          //Ask Me
        OverwriteDupli = 0x238, //OverWrite Duplicate
        OverwriteAll = 0x239,   //OverWrite All
        Appendwrite = 0x23a,    //Append Write
        CreateNew = 0x23b,      //Create New
        Skip = 0x23c,           //Skip

        FilterMode = 0x23e, //Filter Mode
        PureAND = 0x23f,    //Pure AND
        PureOR = 0x240,     //Pure OR
        MixedANDOR = 0x241, //Mixed AND-OR
        MixedORAND = 0x242, //Mixed OR-AND

        ConfirmClear = 0x243,   //Confirm Clear
        ConfirmDelete = 0x244,  //Confirm Delete
        ConfirmReSet = 0x245,   //Confirm ReSet
        ConfirmReLoad = 0x246,  //Confirm ReLoad

        ImageSetting = 0x249,   //Image Settings
        DownloadFolder = 0x24a, //Downloads Folder
        CardMaker = 0x24b,      //Card Maker
        ImgSize = 0x24c,        //Image Size
        StampSize = 0x24d,      //Stamp Size
        StampPos = 0x24e,       //Stamp Position
        StampMargin = 0x24f,    //Stamp Margin

        topLeft = 0x251,        //Top-Left
        topRight = 0x252,       //Top-Right
        bottomLeft = 0x253,     //Bottom-Left
        bottomRight = 0x254,    //Bottom-Right
        center = 0x255,         //Center
        unknown = 0x256,        //Unknown

        CodeEdiSetting = 0x258, //CodeEditor Settings

        DeckSetting = 0x25a,    //Deck Settings
        MaxMainDeck = 0x25b,    //Max Main Deck Size
        MaxExtraDeck = 0x25c,   //Max Extra Deck Size
        MaxSideDeck = 0x25d,    //Max Side Deck Size

        AlternateFormats = 0x265,   //Alternate Formats
        ListCardMode = 0x266,       //List Card Mode
        GridCardMode = 0x267,       //Grid Card Mode
        DisplayID = 0x268,          //Display ID/Scope
        DisplayArchetype = 0x269,   //Display Archetype
        DisplayGPoint = 0x26a,      //Display Genesys Point
        DisplayScope = 0x26b,       //Display Scope Image
        RitualPlaceExtra = 0x26c,   //Ritual Place Extra
        SaveName = 0x26d,           //Save Card Name
        IgnoreSize = 0x26e,         //Ignore Deck Size
        IgnoreContent = 0x26f,      //Ignore Deck Content

        MaxItem = 0x275,        //Max Item
        AutoSearch = 0x277,     //Auto Search
        AllowSave = 0x278,      //Allow Save
        AllowNew = 0x279,       //Allow New
        #endregion

        #region Filter
        tlFilter = 0x301,           //Filter Card

        SelectCards = 0x302,        //Selected Cards
        FoundCards = 0x303,         //Found Cards
        AllCards = 0x304,           //All Cards
        SelectedDeck = 0x305,       //Selected Deck
        SelectedRare = 0x306,       //Selected Rarity
        SelectedBanList = 0x307,    //Selected BanList

        toolSelectCards = 0x30a,    //The currently selected cards in the list.
        toolFoundCards = 0x30b,     //The currently displayed cards (after applying filters) in the List.
        toolAllCards = 0x30c,       //All cards in the list.

        DupliCards = 0x310,         //Duplicate Cards
        DiffCards = 0x311,          //Different Cards

        ByCDBFile = 0x312,          //By CDB File
        ByYDKFile = 0x313,          //By YDK File
        ByLanguage = 0x314,         //By Language
        ByPenLanguage = 0x315,      //By Pendulum Language

        FilterFilepath = 0x316,     //Filter File Path

        IncludeLanguage = 0x317,    //Include Language
        ExcludeLanguage = 0x318,    //Exclude Language

        #endregion

        #region Create Image
        CreateImage = 0x321,        //Create Image
        toolCreateImage = 0x322,    //Create a Card Image, using data from the Card list. Right click to open settings dialog.
        tlSelectImage = 0x323,      //Select Image
        toolSelectImg = 0x324,      //Select an available card image. Right click to open settings dialog.
        tlSelectArtwork = 0x325,    //Select Artwork
        toolSelectArt = 0x326,      //Select an available card Artwork.

        OriginalFolder = 0x327, //Original Image Folder
        ArtWorkFolder = 0x328,  //ArtWork Image Folder
        OutPutFolder = 0x329,   //Output Image Folder
        Series = 0x32a,         //Series
        Common = 0x32b,         //Common
        GoldRare = 0x32c,       //Gold
        PlatiumRare = 0x32d,    //Platium
        Secret = 0x32e,         //Secret
        IncludeRare = 0x32f,    //Includes Rarity
        FormatName = 0x330,     //Format Name
        FormatEffect = 0x331,   //Format Effect
        BackgroundArt = 0x332,  //Background ArtWork
        Foild = 0x333,          //Card Foild
        FullArt = 0x334,        //Full Artwork

        AWFullLocal = 0x335,    //ArtWork FullArt Location
        Opaque = 0x336,         //Opaque
        Transparent = 0x337,    //Transparent

        geneImgFail = 0x33a,    //{0} Images Failed to Generate.
        failListID = 0x33b,     //List of Failed IDs:
        dupliListID = 0x33c,    //List of Duplicate IDs:
        #endregion

        #region Items Editor
        filterSetting = 0x351,  //Filter Settings
        advanFil = 0x352,       //Advanced
        matchCase = 0x353,      //Match Case
        prefix = 0x354,         //Match Prefix
        suffix = 0x355,         //Match Suffix
        wildcards = 0x356,      //Use Wildcards
        matchWhole = 0x357,     //Whole Words
        ignpunct = 0x358,       //Ignore Punctuation
        ignspace = 0x359,       //Ignore White-Space

        replaceText = 0x361,    //Replace Text
        Find = 0x362,           //Find
        FindWhat = 0x363,       //Find What
        FindNext = 0x364,       //Find Next
        FindPrev = 0x365,       //Find Previous
        FindAll = 0x366,        //Find All
        Replacewith = 0x367,    //Replace With
        ReplaceNext = 0x368,    //Replace Next
        ReplaceAll = 0x369,     //Replace All

        replaceField = 0x36a,           //Replace Field
        ReplaceFieldFilePath = 0x36b,   //Replace File Path
        addNewCard = 0x36c,             //Add New Card

        importData = 0x36d,             //Import Data
        ImportDataFilePath = 0x36e,     //Import File Path
        SelectDataField = 0x36f,        //Select Fields to Overwrite

        PendulumLanguage = 0x371,       //Pendulum Language
        changeLangFail = 0x372,         //{0} Card Failed to change Pendulum Language.
        emptyDesc = 0x373,              //{0} Card Missing descriptions.
        fallbackEDORule = 0x374,        //{0} Card Using EDOPro fallback rules.

        CreditTeam = 0x375,     //Credit Team
        CreditName = 0x376,     //Credit Name
        CreditDesc = 0x377,     //Credit Desc
        #endregion

        #region Rarity
        Rarity = 0x381,             //Rarity
        RarityManager = 0x382,      //Rarity Manager
        toolRarityManager = 0x383,  //Open the Rarity Manager Window.
        RarityListManager = 0x384,  //Rarity List Manager
        RarityCardManager = 0x385,  //Rarity Card Manager
        RarityIndex = 0x386,        //Index
        RarityName = 0x387,         //Rarity Name
        RarityCode = 0x388,         //Code
        RarityImgPath = 0x389,      //Image Path
        #endregion

        #region Genesys
        Genesys = 0x391,            //Genesys
        GenesysManager = 0x392,     //Genesys Manager
        toolGenesysManager = 0x393, //Open the Genesys Manager Window.
        genesysPoint = 0x394,       //Genesys Point
        #endregion

        #region About
        About = 0x39a,      //About
        Version = 0x39b,    //Version:
        Creator = 0x39c,    //Created by:
        #endregion

        #region Pendulum Language
        PendulumEffect = 0x3a1,         //Pendulum Effect
        MonsterEffect = 0x3a2,          //Monster Effect
        CardDescription = 0x3a3,        //Card Description
        NormalCard = 0x3a4,             //Normal Card

        #endregion

        #region Tooltip
        newzip = 0x501,         //Create a new Archive.
        toolSaveDB = 0x502,     //Save all cards in the list to the original Card Database file.
        toolSaveCeds = 0x503,   //Save all cards in the list to the original CardEditorSet file.
        toolSaveExcel = 0x504,  //Save all cards in the list to the original Excel file.
        toolSaveAsDB = 0x505,   //Save all cards to a new Card Database file.

        toolScriptSP = 0x50a,   //Open the Script Support Window.

        toolCopy = 0x511,       //Copy cards to the clipboard.
        toolPaste = 0x512,      //Paste cards from the clipboard into the list.

        toolFilterCDB = 0x51a,      //Filter and display cards on the list using a Card Database file.
        toolFilDupliCDB = 0x51b,    //Filter and display cards on the list that APPEAR in the selected Card Database.
        toolFilDiffCDB = 0x51c,     //Filter and display cards on the list that DO NOT APPEAR in the selected Card Database.

        toolFilterYDK = 0x51d,      //Filter and display cards on the list using a Deck file.
        toolFilDupliYDK = 0x51e,    //Filter and display cards on the list that APPEAR in the selected Deck.
        toolFilDiffYDK = 0x51f,     //Filter and display cards on the list that DO NOT APPEAR in the selected Deck.

        toolFilterLanguage = 0x520, //Filter and display cards on the list using a specified language.
        toolFilInclude = 0x521,     //Filter and display cards on the list that INCLUDE the specified language.
        toolFilExclude = 0x522,     //Filter and display cards on the list that EXCLUDE the specified language.

        toolExportZIP = 0x525,      //Export Database (and all Images, Scripts of each card in Database) as ZIP file.

        toolExportCeds = 0x52a,     //Export cards as CardEditorSet file.
        toolImportCeds = 0x52b,     //Import cards from the CardEditorSet file into the list.

        toolExportExcel = 0x52c,    //Export cards as Excel file.
        toolImportExcel = 0x52d,    //Import cards from the Excel file into the list.

        toolAnalyze = 0x531,        //Analyze code for formatting issues and fix any detected inconsistencies.
        toolRegistry = 0x532,       //Export Windows Registry Key file.
        toolGitHub = 0x533,         //Source code on Github.

        toolCardImg = 0x53a,            //Default Image
        toolViewImg = 0x53b,            //View Image
        toolOpenLocalImg = 0x53c,       //Open Image File Location
        toolOpenDB = 0x53d,             //Open Card In Database
        toolOpenScript = 0x53e,         //Open Card Script
        toolOpenKonamiDB = 0x53f,       //Open Card in Konami Database Website
        toolOpenYugipedia = 0x540,      //Open Card in Yugipedia Website
        toolOpenYGOResources = 0x541,   //Open Card in YGO Resources Website

        selectOpenFile = 0x545,         //Select File to Open:

        toolPureAND = 0x54a,    //The card must satisfy all filter conditions.
        toolPureOR = 0x54b,     //The card only needs to satisfy any one of the filter conditions.
        toolMixedANDOR = 0x54c, //Applies AND between groups and OR within each group.
        toolMixedORAND = 0x54d, //Applies OR between groups and AND within each group.

        toolReplaceText = 0x551,    //Replaces the effect descriptions of the Cards.
        toolReplaceField = 0x552,   //Replace only the selected fields in the current list with data from the imported database file, matched by ID. New cards will be added only if “Add New Card” is turned on.
        toolImportData = 0x553,     //Import cards from the imported database file. Existing cards will only replace the selected fields, skip them if no fields are selected.

        #endregion

        #region Object
        PlaceholderFIle = 0x5a1,    //{0} File
        PlaceholderSave = 0x5a2,    //Save {0}
        PlaceholderSelect = 0x5a3,  //Select {0}

        CardArchive = 0x5a4,//Card Archive
        CardDB = 0x5a5,     //Card Database
        CardRareDB = 0x5a6, //Card Rare Database
        ListRareDB = 0x5a7, //List Rare Database
        GenesysDB = 0x5a8,  //Genesys Card Database
        CreditDB = 0x5a9,   //Credit Database
        CardScript = 0x5aa, //Card Script
        Script = 0x5ab,     //Script
        Image = 0x5ac,      //Image
        Ceds = 0x5ad,       //Ceds
        Deck = 0x5ae,       //Deck
        BanList = 0x5af,    //Banlist
        Text = 0x5b0,       //Text Documents
        Md = 0x5b1,         //Markdown
        Log = 0x5b2,        //Log
        Yaml = 0x5b3,       //YAML
        Zip = 0x5b4,        //Zip
        Excel = 0x5b5,      //Excel
        Video = 0x5b6,      //Video
        Json = 0x5b7,       //JSON
        All = 0x5b8,        //All
        UnknownFile = 0x5b9,//Unknown
        Folder = 0x5ba,     //Folder

        Path = 0x5bb,       //Path
        Format = 0x5bc,     //Format
        Handle = 0x5bd,     //Handle
        Operation = 0x5be,  //Operation
        Permission = 0x5bf, //Permission
        Encoding = 0x5c0,   //Encoding
        #endregion

        #region Card Element
        dataGridID = 0x5d1,         //ID
        cardID = 0x5d2,             //Identification
        cardAlias = 0x5d3,          //Alias
        cardName = 0x5d4,           //Card Name
        cardDesc = 0x5d5,           //Card Desc
        cardStr = 0x5d6,            //String
        cardLabelScope = 0x5d7,     //Scope
        cardLabelType = 0x5d8,      //Card Type
        cardLabelRace = 0x5d9,      //Monster Race
        cardLabelChar = 0x5da,      //Character
        cardLabelAttri = 0x5db,     //Attribute
        cardlabelSetCode = 0x5dc,   //SetCode/Archetype
        cardLabelCategory = 0x5dd,  //Category
        cardLabelFlag = 0x5de,      //Flag

        Level = 0x5e1,          //Level
        minuLv = 0x5e2,         //Minus Level
        Rank = 0x5e3,           //Rank
        LinkRat = 0x5e4,        //Link Rating
        LinkArr = 0x5e5,        //Link Arrows
        penScaleLabel = 0x5e6,  //Pendulum Scale
        Maximum = 0x5e7,        //Maximum
        Support = 0x5e8,        //Support

        cardatk = 0x5e9,        //ATK
        carddef = 0x5ea,        //DEF
        cardWidth = 0x5eb,      //Width
        cardHeight = 0x5ec,     //Height
        cardrare = 0x5ed,       //Rarity

        scaleLeft = 0x5ee,      //Left
        scaleRight = 0x5ef,     //Right
        #endregion

        #region Script Support
        CardText = 0x5f8,       //Card Text
        CardData = 0x5f9,       //Card Data
        CardInfo = 0x5fa,       //Card Information

        SearchCard = 0x5fb,     //Search Card
        SearchScript = 0x5fc,   //Search Script
        SearchScrapiName = 0x5fd,   //Search Scrapi Name
        SearchScrapiDesc = 0x5fe,   //Search Scrapi Desc

        ColonChar = 0x5ff,      //Colon Character
        #endregion

        #region Mess

        #region Header
        error = 0x621,      //Error
        warning = 0x622,    //Warning
        infoma = 0x623,     //Information
        notifi = 0x624,     //Notification
        questi = 0x625,     //Question

        conReset = 0x626,   //Confirm Reset?
        conClear = 0x627,   //Confirm Clear?
        conDelete = 0x628,  //Confirm Delete?

        #endregion

        dataSourceErr = 0x62a,      //Data Source Error
        dataSourceMiss = 0x62b,     //Data source is missing or does not exist, Check Update and restart application.
        errorOcc = 0x62c,           //An error has occurred:

        TwoPlaceholder = 0x631,     //{0} {1}:
        ThreePlaceholder = 0x632,   //{0} {1} {2}:
        FourPlaceholder = 0x633,    //{0} {1} {2} {3}:

        TwoPlaceholderSuccess = 0x635,      //{0} {1} successfully!
        ThreePlaceholderSuccess = 0x636,    //{0} {1} {2} successfully!
        FourPlaceholderSuccess = 0x637,     //{0} {1} {2} {3} successfully!

        PlaceholderError = 0x638,           //Error {0}:
        TwoPlaceholderError = 0x639,        //Error {0} {1}:
        ThreePlaceholderError = 0x63a,      //Error {0} {1} {2}:
        FourPlaceholderError = 0x63b,       //Error {0} {1} {2} {3}:

        PlaceholderInva = 0x63c,            //Invalid {0}:
        TwoPlaceholderInva = 0x63d,         //Invalid {0} {1}:
        ThreePlaceholderInva = 0x63e,       //Invalid {0} {1} {2}:

        cancelled = 0x63f,          //The process has been canceled upon request.
        rollbacked = 0x640,         //A serious error has occurred; all changes have been reverted.

        errorFetData = 0x651,       //Error fetching data:
        errorConDB = 0x652,         //Error connecting to database:
        errorCloneRepo = 0x653,     //Error cloning repository:
        needDeleteFolder = 0x654,   //You may need to delete the "data\CardData" folder and try again.
        errorLoadDB = 0x655,        //Error loading Card Database:
        errorLoadImageCache = 0x656,//Error loading Image Cache:

        outofrange = 0x65a,         //out of allowed range.
        cannotEmpty = 0x65b,        //cannot be empty.

        invaBackground = 0x65c,     //Invalid Background Color Code.
        invaForeground = 0x65d,     //Invalid Foreground Color Code.

        noCardFound = 0x661,        //No Cards found.
        noCardSelec = 0x662,        //No Cards selected.
        noCardCopy = 0x663,         //No Cards for copy.
        noCardExport = 0x664,       //No Cards for export.
        noCardFilter = 0x665,       //No Cards for filter.
        noCardSave = 0x666,         //No Cards for save.
        noCardReplace = 0x667,      //No Cards for Replaced.
        noRareSelec = 0x668,        //No Raritys selected.
        noFileFound = 0x669,        //No Files found.
        noDeckFound = 0x66a,        //No Decks found.
        noSelecWin = 0x66b,         //No selected Window.
        noSelecDB = 0x66c,          //No selected Card Database.
        noValiCardFound = 0x66d,    //No valid Cards found.
        noValiDataFound = 0x66e,    //No valid Data found.
        noValiDataClip = 0x66f,     //No valid Data in Clipboard.
        noRegularUser = 0x670,      //Not intended for regular users.
        konamiIDnotFou = 0x671,     //Konami ID not found.
        yugiPedianotFou = 0x672,    //Unable to search Yugipedia for this Card.

        folderNotExit = 0x681,      //Folder does not exist.
        fileNotExit = 0x682,        //File does not exist.
        filealreadyExit = 0x683,    //File already exists.
        cardNotExit = 0x684,        //Card does not exist in database.
        cardIDExist = 0x685,        //Card ID already exists.
        cardIDNotExist = 0x686,     //Card ID does not exist.
        cardIDNotExistList = 0x687, //Card ID does not exist in the List.
        notFolder = 0x688,          //Not a Folder.
        notDatabase = 0x689,        //Not a Database file.
        notImage = 0x68a,           //Not an Image file.
        whNotVali = 0x68b,          //Width or Height value is not a valid number.

        luaFileNotFou = 0x691,      //Script File for ID {0} not found.
        luaFileEmp = 0x692,         //Script File for ID {0} is empty.
        descNotFou = 0x693,         //Description for ID {0} not found.

        expoZIPSuc = 0x6a1,     //Zip file exported successfully at:
        expoExcelSuc = 0x6a2,   //Excel file exported successfully at:
        registrySuc = 0x6a3,    //Registry Key File (.reg) created successfully at:
        registryType = 0x6a4,   //This file registers CardEditorX as a handler for the following file types:
        openDirectly = 0x6a5,   //Opens directly on double-click:
        openWithOnly = 0x6a6,   //Appears in Open With menu:
        setRegistry = 0x6a7,    //Double-click it to apply — Windows will ask for confirmation before writing to the Registry.
        unRegistrySuc = 0x6a8,  //Unregister Registry successfully!

        expoFunSuc = 0x6aa,     //Extract functions data successfully!
        expoConsSuc = 0x6ab,    //Extract constants data successfully!
        cloneRepoSuc = 0x6ac,   //Repository has been cloned successfully!
        dataUpdateSuc = 0x6ad,  //Data has been updated successfully!
        settingReset = 0x6ae,   //Settings have been reset to default. Restart application to apply.
        filterSuc = 0x6af,      //Filter successfully! Found {0} cards out of {1} total cards.
        replaceSuc = 0x6b0,     //Replace successfully! Replaced {0} Cards out of {1} total Cards.
        changeSuc = 0x6b1,      //{0} changed successfully! Applied to {1} Cards out of {2} total cards.

        renameSuc = 0x6b2,      //{0} {1} renamed to {2} successfully!

        apiKeyNotConfig = 0x6b5,//API Endpoint or API Key is not configured.

        idChanged = 0x6e1,              //Card ID has been changed.
        quesSelectDelete = 0x6e2,       //Delete the selected card or the card with newly entered ID?
        quesSelectCreaScript = 0x6e3,   //Create the Card Script of the selected card or the card with newly entered ID?
        quesSelectCreaImg = 0x6e4,      //Create the Image of the selected card or the card with newly entered ID?

        newIDCard = 0x6e5,              //Newly entered ID
        originaData = 0x6e6,            //Original Data
        unSavedData = 0x6e7,            //Unsaved Data

        NumberCloseTab = 0x701,         //You are closing {0} tabs.
        HasUnSaveData = 0x702,          //There is unsaved data.
        HasSnapshot = 0x703,            //There is one snapshot that hasn't been rolled back from the previous run.
        HasDuplicateIDs = 0x704,        //There are {0} duplicate IDs in the Card List.
        QuestContinue = 0x705,          //Do you want to continue?
        QuestOpen = 0x706,              //Do you want to open it?
        QuestOverwrite = 0x707,         //Do you want to overwrite it?
        QuestSaveChange = 0x708,        //Do you want to save your changes for this file?
        confirmAdd = 0x709,             //Are you sure? Cards with 4 digit IDs or lower will be the game ignore.
        TwoPlaceholderConfirm = 0x70a,  //Are you sure you want to {0} {1}?

        confirmDelete = 0x70b,          //Are you sure you want to PERMANENTLY DELETE {0}?
        confirmResetSetting = 0x70c,    //Are you sure you want to reset settings to default? All changes will be lost.
        confirmClearHistory = 0x70d,    //Are you sure you want to clear the recently opened {0} history?
        confirmSaveBlank = 0x70e,       //Are you sure you want to save a blank file?
        confirmUnregisterReg = 0x70f,   //Are you sure you want to unregister the CardEditorX file types from the Windows Registry?
        quesDownloadUpdate = 0x710,     //Update Available, dowload it now?
        quesDownloadCardImage = 0x711,  //Download Card Resource Pack now?
        confirmWriteData = 0x712,       //How would you like to handle the selected data?
        noUpdate = 0x713,               //No updates found.
        updateCompe = 0x714,            //Update Complete!

        unableDelete = 0x725,           //Unable to delete existing file after multiple attempts.
        gitNotFound = 0x726,            //git.exe path not found, make sure Git is installed and using correct path in application configuration.
        gitPathMiss = 0x727,            //Git Path is missing or empty in configuration. Using default Git path.
        hightLightNotExist = 0x728,     //The syntax highlighting file does not exist.
        useDefault = 0x729,             //Using default path.

        #endregion

        #region Chat
        ChatList = 0x751,       //Chat List
        NewChat = 0x752,        //New Chat
        DeleteChat = 0x753,     //Delete Chat
        HintChat = 0x754,       //Ask ChatBot anything.
        FileChat = 0x755,       //Attach File
        ImageChat = 0x756,      //Attach Image
        VideoChat = 0x757,      //Attach Video

        ApiEndpoint = 0x758,    //Api Endpoint
        ApiKey = 0x759,         //Api Key
        #endregion

        #region BanList Editor
        BanListName = 0x771,    //Name: 
        WhiteList = 0x772,      //White List
        Limit = 0x773,          //Limit:
        BannedCard = 0x774,     //Banned
        LimitedCard = 0x775,    //Limited
        SemiLimitedCard = 0x776,//Semi-Limited
        UnLimitedCard = 0x777,  //Unlimited
        #endregion

        #region Deck Editor
        cmbDeck = 0x781,        //Deck:
        AllowedCard = 0x782,    //Allowed Card
        SearchCardDeck = 0x78a, //Search Card
        NewDeckName = 0x78b,    //New Name

        MainDeck = 0x791,       //Main Deck
        ExtraDeck = 0x792,      //Extra Deck
        SideDeck = 0x793,       //Side Deck

        Monster = 0x7a1,    //Monster:
        Spell = 0x7a2,      //Spell:
        Trap = 0x7a3,       //Trap:
        Skill = 0x7a4,      //Skill:

        Ritual = 0x7a5,     //Ritual:
        Fusion = 0x7a6,     //Fusion:
        Synchro = 0x7a7,    //Synchro:
        eXceed = 0x7a8,     //eXceed:
        Link = 0x7a9,       //Link:
        #endregion

    }
}