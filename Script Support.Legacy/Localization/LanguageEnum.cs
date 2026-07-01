using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Localization
{
    public enum Language : uint
    {
        #region Menu

        #region MenuItem
        File = 0x1,
        tNew = 0x2,
        Open = 0x3,
        Save = 0x4,
        SaveAs = 0x5,
        Recently = 0xe,
        ClearHistory = 0xf,
        Exit = 0x11,

        Window = 0x15,
        Home = 0x16,
        ImageEdit = 0x17,
        DataEdit = 0x18,
        CodeEdit = 0x19,
        DeckEdit = 0x1a,
        BanListEdit = 0x1b,
        ItemsEdit = 0x1c,
        ScriptSupport = 0x1d,

        Setting = 0x25,
        Config = 0x26,

        Card = 0x31,
        CopyCard = 0x32,
        PasteCard = 0x33,
        SaveCard = 0x34,
        FilCDB = 0x3a,
        FilYDK = 0x3b,

        Data = 0x45,
        ExpoZIP = 0x46,
        ExpoCeds = 0x47,
        ExpoExcel = 0x48,
        //ImpoCeds = 0x49,
        //ImpoExcel = 0x4a,

        Manager = 0x60,
        Help = 0x7a,
        LinScript = 0x7b,
        ChkUpdate = 0x7c,

        About = 0x81,
        Version = 0x82,
        Creator = 0x83,

        #endregion

        CardDB = 0x91,
        CardScript = 0x92,
        Deck = 0x93,
        BanListMn = 0x94,
        CardImg = 0x95,
        Ceds = 0x96,
        Excel = 0x97,

        #region Button
        load = 0x101,
        yes = 0x102,
        no = 0x103,
        ok = 0x104,
        cancel = 0x105,
        noSave = 0x106,
        add = 0x107,

        tlAdd = 0x111,
        tlModify = 0x112,
        tlUndo = 0x113,
        toolUndo = 0x114,
        tlFilter = 0x115,
        toolSearch = 0x116,
        clearFilter = 0x117,

        tlScript = 0x11c,
        Insert = 0x11d,
        Browse = 0x11e,
        UpLoad = 0x11f,
        tlReload = 0x120,
        Rename = 0x121,
        Refresh = 0x122,
        tlReset = 0x123,
        tlClear = 0x124,
        tlDelete = 0x125,
        Shuffle = 0x126,
        YDKE = 0x127,
        Import = 0x128,
        Export = 0x129,
        Replace = 0x12a,
        #endregion

        #region Tooltip
        newzip = 0x151,         //Create a new Archive.
        toolSaveDB = 0x152,     //Save all cards in the list to the original Card Database file.
        toolSaveCeds = 0x153,   //Save all cards in the list to the original CardEditorSet file.
        toolSaveExcel = 0x154,  //Save all cards in the list to the original Excel file.
        toolSaveAsDB = 0x155,   //Save cards to a new Card Database file.

        toolScriptSP = 0x15a,   //Open the Script Support Window.

        toolCopy = 0x161,       //Copy cards to the clipboard.
        toolPaste = 0x162,      //Paste cards from the clipboard into the list.

        toolFilterCDB = 0x16a,      //Filter and display cards on the list using a Card Database file.
        toolFilDupliCDB = 0x16b,    //Filter and display cards on the list that APPEAR in the selected Card Database.
        toolFilDiffCDB = 0x16c,     //Filter and display cards on the list that DO NOT APPEAR in the selected Card Database.
        toolFilterYDK = 0x16d,      //Filter and display cards on the list using a Deck file.
        toolFilDupliYDK = 0x16e,    //Filter and display cards on the list that APPEAR in the selected Deck.
        toolFilDiffYDK = 0x16f,     //Filter and display cards on the list that DO NOT APPEAR in the selected Deck.

        toolExportZIP = 0x175,      //Export Database (and all Images, Scripts of each card in Database) as ZIP file.

        toolExportCeds = 0x17a,     //Export cards as CardEditorSet file.
        toolImportCeds = 0x17b,     //Import cards from the CardEditorSet file into the list.

        toolExportExcel = 0x17c,    //Export cards as Excel file.
        toolImportExcel = 0x17d,    //Import cards from the Excel file into the list.

        toolAnalyze = 0x181,        //Analyze code for formatting issues and fix any detected inconsistencies.
        toolRegistry = 0x182,       //Export Windows Registry Key file.
        toolGitHub = 0x183,         //Source code on Github.

        toolCardImg = 0x18a,            //Default Image
        toolViewImg = 0x18b,            //View Image
        toolOpenLocalImg = 0x18c,       //Open Image File Location
        toolOpenDB = 0x18d,             //Open Card In Database
        toolOpenScript = 0x18e,         //Open Card Script
        toolOpenKonamiDB = 0x18f,       //Open Card in Konami Database Website
        toolOpenYugipedia = 0x190,      //Open Card in Yugipedia Website
        toolOpenYGOResources = 0x191,   //Open Card in YGO Resources Website

        toolPureAND = 0x19a,    //The card must satisfy all filter conditions.
        toolPureOR = 0x19b,     //The card only needs to satisfy any one of the filter conditions.
        toolMixedANDOR = 0x19c, //Applies AND between groups and OR within each group.
        toolMixedORAND = 0x19d, //Applies OR between groups and AND within each group.

        toolReplaceText = 0x1a1,    //Replaces the effect descriptions of the Cards.
        toolReplaceField = 0x1a2,   //Replace only the selected fields in the current list with data from the imported database file, matched by ID. New cards will be added only if “Add New Card” is turned on.
        toolImportData = 0x1a3,     //Import cards from the imported database file. Existing cards will only replace the selected fields, skip them if no fields are selected.



        #endregion

        #endregion

        #region Filter
        SelectCards = 0x201,        //Selected Cards
        FoundCards = 0x202,         //Found Cards
        AllCards = 0x203,           //All Cards
        toolSelectCards = 0x204,    //The currently selected cards in the list.
        toolFoundCards = 0x205,     //The currently displayed cards (after applying filters) in the List.
        toolAllCards = 0x206,       //All cards in the list.
        DupliCards = 0x207,         //Duplicate Cards
        DiffCards = 0x208,          //Different Cards
        #endregion

        #region Create Image
        CreateImage = 0x211,        //Create Image
        toolCreateImage = 0x212,    //Create a Card Image, using data from the Card list. Right click to open settings dialog.
        tlSelectImage = 0x213,      //Select Image
        toolSelectImg = 0x214,      //Select an available card image. Right click to open settings dialog.
        tlSelectArtwork = 0x215,    //Select Artwork
        toolSelectArt = 0x216,      //Select an available card Artwork.

        OriginalFolder = 0x217, //Original Image Folder
        ArtWorkFolder = 0x218,  //ArtWork Image Folder
        OutPutFolder = 0x219,   //Output Image Folder
        Series = 0x21a,         //Series
        Common = 0x21b,         //Common
        GoldRare = 0x21c,       //Gold
        PlatiumRare = 0x21d,    //Platium
        Secret = 0x21e,         //Secret
        IncludeRare = 0x21f,    //Includes Rarity
        FormatName = 0x220,     //Format Name
        FormatEffect = 0x221,   //Format Effect
        BackgroundArt = 0x222,  //Background ArtWork
        Foild = 0x223,          //Card Foild

        AWFullLocal = 0x225,    //ArtWork FullArt Location
        Opaque = 0x226,         //Opaque
        Transparent = 0x227,    //Transparent

        geneImgFail = 0x22a,    //{0} Image Generator Failure.
        failListID = 0x22b,     //List of Failed IDs:
        dupliListID = 0x22c,    //List of Duplicate IDs:
        #endregion

        #region Items Editor
        advanFil = 0x231,       //Advanced
        matchCase = 0x232,      //Match Case
        prefix = 0x233,         //Match Prefix
        suffix = 0x234,         //Match Suffix
        wildcards = 0x235,      //Use Wildcards
        matchWhole = 0x236,     //Whole Words
        ignpunct = 0x237,       //Ignore Punctuation
        ignspace = 0x238,       //Ignore White-Space

        replaceText = 0x241,    //Replace Text
        Find = 0x242,           //Find
        FindWhat = 0x243,       //Find What
        FindNext = 0x244,       //Find Next
        FindPrev = 0x245,       //Find Previous
        FindAll = 0x246,        //Find All
        Replacewith = 0x247,    //Replace With
        ReplaceNext = 0x248,    //Replace Next
        ReplaceAll = 0x249,     //Replace All

        replaceField = 0x251,           //Replace Field
        ReplaceFieldFilePath = 0x252,   //Replace File Path
        addNewCard = 0x253,             //Add New Card

        importData = 0x25a,             //Import Data
        ImportDataFilePath = 0x25b,     //Import File Path
        SelectDataField = 0x25c,        //Select Fields to Overwrite
        #endregion

        #region Rarity
        Rarity = 0x261,             //Rarity
        RarityManager = 0x262,      //Rarity Manager
        toolRarityManager = 0x263,  //Open the Rarity Manager Window.
        RarityListManager = 0x264,  //Rarity List Manager
        RarityCardManager = 0x265,  //Rarity Card Manager
        RarityIndex = 0x266,        //Index
        RarityName = 0x267,         //Rarity Name
        RarityCode = 0x268,         //Code
        RarityImgPath = 0x269,      //Image Path
        #endregion

        #region Genesys
        Genesys = 0x271,            //Genesys
        GenesysManager = 0x272,     //Genesys Manager
        toolGenesysManager = 0x273, //Open the Genesys Manager Window.
        genesysPoint = 0x274,       //Genesys Point
        #endregion

        #region Config
        UserSetting = 0x301,        //User Settings
        UserName = 0x302,           //User Name
        Language = 0x303,           //Language
        DataSource = 0x304,         //Data Source
        Game = 0x305,               //Game
        browserPath = 0x306,        //Browser Path

        DisplaySetting = 0x311,     //Display Settings
        Background = 0x312,         //Background
        Foreground = 0x313,         //Foreground
        Theme = 0x314,              //Theme
        FontFamily = 0x315,         //FontFamily
        FontSize = 0x316,           //FontSize
        Highlight = 0x317,          //Highlight
        FlowDirection = 0x318,      //Flow Direction
        LeftToRight = 0x319,        //Left To Right
        RightToLeft = 0x31a,        //Right To Left
        TextAlignment = 0x31b,      //Text Alignment
        AligLeft = 0x31c,           //Left
        AligRight = 0x31d,          //Right
        AligJustify = 0x31e,        //Justify
        AligCenter = 0x31f,         //Center
        WordWrap = 0x320,           //Word Wrap
        CodeFold = 0x321,           //Code Folding

        Sort = 0x32a,           //Sort
        SortSetting = 0x32b,    //Sort Setting
        SortBy = 0x32c,         //Sort By
        SortOrder = 0x32d,      //Order By
        Ascending = 0x32e,      //Ascending
        Descending = 0x32f,     //Descending

        DataHandling = 0x335,   //Data Handling
        WriteMode = 0x336,      //Write Mode
        AskMe = 0x337,          //Ask Me
        Overwrite = 0x338,      //OverWrite
        Appendwrite = 0x339,    //Append Write
        CreateNew = 0x33a,      //Create New

        FilterMode = 0x33e, //Filter Mode
        PureAND = 0x33f,    //Pure AND
        PureOR = 0x340,     //Pure OR
        MixedANDOR = 0x341, //Mixed AND-OR
        MixedORAND = 0x342, //Mixed OR-AND

        ConfirmClear = 0x343,   //Confirm Clear
        ConfirmDelete = 0x344,  //Confirm Delete
        ConfirmReSet = 0x345,   //Confirm ReSet
        ConfirmReLoad = 0x346,  //Confirm ReLoad

        ImageSetting = 0x349,   //Image Settings
        DownloadFolder = 0x34a, //Downloads Folder
        CardMaker = 0x34b,      //Card Maker
        ImgSize = 0x34c,        //Image Size
        StampSize = 0x34d,      //Stamp Size
        StampPos = 0x34e,       //Stamp Position
        StampMargin = 0x34f,    //Stamp Margin

        topLeft = 0x351,        //Top-Left
        topRight = 0x352,       //Top-Right
        bottomLeft = 0x353,     //Bottom-Left
        bottomRight = 0x354,    //Bottom-Right
        center = 0x355,         //Center
        unknown = 0x356,        //Unknown

        CodeEdiSetting = 0x358, //Code Editor Settings 

        DeckSetting = 0x35a,    //Deck Settings
        MaxMainDeck = 0x35b,    //Max Main Deck Size
        MaxExtraDeck = 0x35c,   //Max Extra Deck Size
        MaxSideDeck = 0x35d,    //Max Side Deck Size

        AlternateFormats = 0x365,   //Alternate Formats
        ListCardMode = 0x366,       //List Card Mode
        GridCardMode = 0x367,       //Grid Card Mode
        DisplayID = 0x368,          //Display ID/Scope
        DisplayArchetype = 0x369,   //Display Archetype
        DisplayGPoint = 0x36a,      //Display Genesys Point
        DisplayScope = 0x36b,       //Display Scope Image
        RitualPlaceExtra = 0x36c,   //Ritual Place Extra
        SaveName = 0x36d,           //Save Card Name
        IgnoreSize = 0x36e,         //Ignore Deck Size
        IgnoreContent = 0x36f,      //Ignore Deck Content

        MaxItem = 0x375,        //Max Item
        Header = 0x376,         //List Header
        AutoSearch = 0x377,     //Auto Search
        AllowSave = 0x378,      //Allow Save
        AllowNew = 0x379,       //Allow New
        #endregion

        #region Script Support
        Archetype = 0x391,      //Archetype: 
        CardType = 0x392,       //Card Type: 
        Attribute = 0x395,      //Attribute: 
        Race = 0x393,           //Race: 
        Char = 0x394,           //Character: 
        PenScale = 0x396,         //Pendulum Scale: 


        CardInfo = 0x396,       //Card Information
        SearchCard = 0x397,     //Search Card
        SearchScript = 0x398,   //Search Script
        SearchScrapiName = 0x399,   //Search Scrapi Name
        SearchScrapiDesc = 0x39a,   //Search Scrapi Desc
        #endregion

        #region DataEditor XAML
        dataGridID = 0x401,         //ID
        cardID = 0x402,             //ID
        cardAlias = 0x403,          //Alias
        cardName = 0x404,           //Card Name
        cardDesc = 0x405,           //Card Desc
        cardStr = 0x406,            //String
        cardLabelScope = 0x407,     //Scope
        cardLabelType = 0x408,      //Card Type
        cardLabelRace = 0x409,      //Monster Race
        cardLabelChar = 0x40a,      //Character
        cardLabelAttri = 0x40b,     //Attribute
        cardlabelSetCode = 0x40c,   //SetCode/Archetype
        cardLabelCategory = 0x40d,  //Category
        cardLabelFlag = 0x40e,      //Flag

        Level = 0x421,          //Level
        minuLv = 0x422,         //Minus Level
        Rank = 0x423,           //Rank
        LinkRat = 0x424,        //Link Rating
        LinkArr = 0x425,        //Link Arrows
        penScaleLabel = 0x426,  //Pendulum Scale
        Maximum = 0x427,        //Maximum
        Support = 0x428,        //Support

        cardatk = 0x431,        //ATK
        carddef = 0x432,        //DEF
        cardWidth = 0x433,      //Width
        cardHeight = 0x434,     //Height
        cardrare = 0x435,       //Rarity

        scaleLeft = 0x436,      //Left
        scaleRight = 0x437,     //Right
        #endregion

        #region Mess
        error = 0x501,      //Error
        warning = 0x502,    //Warning
        infoma = 0x503,     //Information
        notifi = 0x504,     //Notification
        questi = 0x505,     //Question

        dataSourceErr = 0x50a,      //Data Source Error
        dataSourceMiss = 0x50b,     //Data source is missing or does not exist, Check Update and restart application.

        errorOcc = 0x511,       //An error has occurred
        errorCreaCDB = 0x512,   //Error creating Card Database file:
        errorCreaCRRC = 0x513,  //Error creating Card Rare Database file:
        errorCreaCRRL = 0x514,  //Error creating List Rare Database file:
        errorCreaCGNS = 0x515,  //Error creating Genesys Card Database file:
        errorCreaLua = 0x516,   //Error creating Script file:
        errorCreaImg = 0x517,   //Error creating Image file:
        errorCreaCeds = 0x518,  //Error creating Ceds file:
        errorCreaDeck = 0x519,  //Error creating Deck file:
        errorRead = 0x51a,      //Error reading file:
        errorReadConf = 0x51b,  //Error reading configuration:
        errorSaveConf = 0x51c,  //Error saving configuration:
        errorWrite = 0x51d,     //Error writing file:
        errorCopy = 0x51e,      //Error copying card:
        errorPaste = 0x51f,     //Error pasting card:
        errorSave = 0x520,      //Error Saving:
        errorSaveCard = 0x521,  //Error saving cards:
        errorSaveScript = 0x522,//Error saving script:

        errorAddCard = 0x531,       //Error adding card:
        errorAddRare = 0x532,       //Error adding/modifying rarity:
        errorDeleteCard = 0x533,    //Error deleting card:
        errorDeleteRare = 0x534,    //Error deleting rarity:
        errorDeleteFile = 0x535,    //Error deleting file:
        errorModifyCard = 0x536,    //Error modifying card:
        errorCreateCard = 0x537,    //Error creating card:
        errorFilterCard = 0x538,    //Error filtering cards:
        errorExport = 0x539,        //Error exporting data:
        errorImport = 0x53a,        //Error importing data:
        errorDownload = 0x53b,      //Error downloading data:
        errorUpdate = 0x53c,        //Error updating data:
        errorFetData = 0x53d,       //Error fetching data:
        errorConDB = 0x53e,         //Error connecting to database:
        errorCloneRepo = 0x53f,     //Error cloning repository:
        needDeleteFolder = 0x540,   //You may need to delete the "data\CardData" folder and try again.
        errorLoadDB = 0x541,        //Error loading Card Database:
        errorLoadImageCache = 0x542,// Error loading Image Cache:

        invaFilePath = 0x54a,   //Invalid File Path.
        invaFileForm = 0x54b,   //Invalid File Format.
        invaFileHan = 0x54c,    //Invalid File Handle.
        invaFileOpe = 0x54d,    //Invalid File Operation.
        invaFolderPath = 0x54e, //Invalid Folder Path.

        invaDataPath = 0x555,   //Invalid Data Path.
        invaDataForm = 0x556,   //Invalid Data Format.
        invaDataSele = 0x557,   //Invalid Data Selection.

        invaCardID = 0x561,     //Invalid Card ID.
        invaCardAlias = 0x562,  //Invalid Card Alias.
        invaRareName = 0x563,   //Invalid Rarity name.
        invaSetting = 0x56a,    //Invalid Setting. Please select a Setting before saving.
        invaScope = 0x571,      //Invalid Scope.

        invaPermi = 0x581,      //Invalid Permission.
        invaEnco = 0x582,       //Invalid Encoding.
        outofrange = 0x583,     //out of allowed range.
        cannotEmpty = 0x584,    //cannot be empty.

        invaBackground = 0x58a,//Invalid Background Color Code.
        invaForeground = 0x58b,//Invalid Foreground Color Code.

        noCardFound = 0x591,    //No Cards found.
        noCardSelec = 0x592,    //No Cards selected.
        noCardCopy = 0x593,     //No Cards for copy.
        noCardExport = 0x594,   //No Cards for export.
        noCardFilter = 0x595,   //No Cards for filter.
        noCardSave = 0x596,     //No Cards for save.
        noCardReplace = 0x597,  //No Cards for Replaced.
        noRareSelec = 0x598,    //No Raritys selected.
        noFileFound = 0x599,    //No Files found.
        noDeckFound = 0x59a,    //No Decks found.
        noSelecWin = 0x59b,     //No selected Window.
        noSelecDB = 0x59c,      //No selected Card Database.
        noValiCardFound = 0x59d,//No valid Cards found.
        noValiDataFound = 0x59e,//No valid Data found.
        noValiDataClip = 0x59f, //No valid Data in Clipboard.
        noRegularUser = 0x5a0,  //Not intended for regular users.
        konamiIDnotFou = 0x5a1, //Konami ID not found.
        yugiPedianotFou = 0x5a2,//Unable to search Yugipedia for this Card.

        folderNotExit = 0x5b1,      //Folder does not exist.
        fileNotExit = 0x5b2,        //File does not exist.
        filealreadyExit = 0x5b3,    //File already exists.
        cardNotExit = 0x5b4,        //Card does not exist in database.
        cardIDExist = 0x5b5,        //Card ID already exists.
        cardIDNotExist = 0x5b6,     //Card ID does not exist.
        cardIDNotExistList = 0x5b7, //Card ID does not exist in the List.
        notFolder = 0x5b8,          //Not a Folder.
        notDatabase = 0x5b9,        //Not a Database file.
        notImage = 0x5ba,           //Not an Image file.
        whNotVali = 0x5bb,          //Width or Height value is not a valid number.

        luaFileNotFou = 0x5bc,      //Script File for ID {0} not found.
        luaFileEmp = 0x5bd,         //Script File for ID {0} is empty.
        descNotFou = 0x5be,         //Description for ID {0} not found.

        creaSuc = 0x5c6,        //{0} {1} created successfully!
        addSuc = 0x5c7,         //{0} {1} added successfully!
        updateSuc = 0x5c8,      //{0} {1} updated successfully!
        saveSuc = 0x5c9,        //{0} {1} saved successfully!
        copySuc = 0x5ca,        //{0] {1} copied successfully!
        pasteSuc = 0x5cb,       //{0} {1} pasted successfully!
        replaceSuc = 0x5cc,     //{0} {1} Replaced successfully!
        deleteSuc = 0x5cd,      //{0} {1} deleted successfully!
        refreshSuc = 0x5ce,     //{0} {1} refresh successfully!
        renameSuc = 0x5cf,      //{0} {1} renamed to {2} successfully!
        saveSettingSuc = 0x5d0, //Save Settings successfully!

        expoDataSuc = 0x5df,    //Data exported successfully!
        impoDataSuc = 0x5e0,    //Data imported successfully!
        expoZIPSuc = 0x5e1,     //Zip file exported successfully at:
        expoExcelSuc = 0x5e2,   //Excel file exported successfully at:
        registrySuc = 0x5e3,    //Registry Key created successfully at:
        setRegistry = 0x5e4,    //Double Click this file to set Registry Key.

        expoFunSuc = 0x5e5,     //Extract functions data successfully!
        expoConsSuc = 0x5e6,    //Extract constants data successfully!
        cloneRepoSuc = 0x5e7,   //Repository has been cloned successfully!
        dataUpdateSuc = 0x5e8,  //Data has been updated successfully!
        settingReset = 0x5e9,   //Settings have been reset to default. Restart application to apply.
        filterSuc = 0x5ea,      //Filter successful. Found {0} cards out of {1} total cards.
        apiKeyNotConfig = 0x5eb,//API Endpoint or API Key is not configured.

        cancelled = 0x5f1,      //The process has been canceled upon request.
        select = 0x5f2,         //Select
        selectFolder = 0x5f3,   //Select Folder
        saveDB = 0x5f4,         //Save Database File
        saveFile = 0x5f5,       //Save File
        saveDeck = 0x5f6,       //Save Deck File
        allfile = 0x5f7,        //All Files
        txtfile = 0x5f8,        //Text Documents
        mdfile = 0x5f9,         //Markdown File
        logfile = 0x5fa,        //Log File
        deckfile = 0x5fb,       //Deck File
        yamlfile = 0x5fc,       //YAML File
        configfile = 0x5fd,     //Configuration
        zipfile = 0x5fe,        //Zip file
        excelfile = 0x5ff,      //Excel file
        imagefile = 0x600,      //Image File
        videofile = 0x601,      //Video File

        conReset = 0x60a,   //Confirm Reset?
        conClear = 0x60b,   //Confirm Clear?
        conDelete = 0x60c,  //Confirm Delete?

        idChanged = 0x611,              //Card ID has been changed.
        quesSelectDelete = 0x612,       //Delete the selected card or the card with newly entered ID?
        quesSelectCreaScript = 0x613,   //Create the Card Script of the selected card or the card with newly entered ID?
        quesSelectCreaImg = 0x614,      //Create the Image of the selected card or the card with newly entered ID?
        selectedCard = 0x615,       //Selected Card
        selectedDeck = 0x616,       //Selected Deck
        selectedRare = 0x617,       //Selected Rarity
        selectedBanList = 0x618,    //Selected BanList
        newIDCard = 0x619,          //Newly entered ID
        originaData = 0x61a,        //Original Data
        unSavedData = 0x61b,        //Unsaved Data

        NumberCloseTab = 0x631,         //You are closing {0} tabs.
        HasUnSaveData = 0x632,          //There is unsaved data.
        HasDuplicateIDs = 0x633,        //There are {0} duplicate IDs in the Card List.
        QuestContinue = 0x634,          //Do you want to continue?
        QuestOpen = 0x635,              //Do you want to open it?
        QuestOverwrite = 0x636,         //Do you want to overwrite it?
        QuestSaveChange = 0x637,        //Do you want to save your changes for this file?
        confirmAdd = 0x638,             //Are you sure? Cards with 4 digit IDs or lower will be the game ignore.
        confirmClearAll = 0x639,        //Are you sure you want to Clear all {0}?
        confirmDelete = 0x63a,          //Are you sure you want to PERMANENTLY DELETE {0}?
        confirmReload = 0x63b,          //Are you sure you want to Reload {0}?
        confirmReset = 0x63c,           //Are you sure you want to Reset {0}?
        confirmResetSetting = 0x63d,    //Are you sure you want to reset settings to default? All changes will be lost.
        confirmClearHistory = 0x63e,    //Are you sure you want to clear the recently opened {0} history?
        confirmSaveBlank = 0x63f,       //Are you sure you want to save a blank file?
        quesDownloadUpdate = 0x640,     //Update Available, dowload it now?
        questionDownloadDataSource = 0x641,     //Data source is missing or invalid, download now?
        confirmWriteData = 0x642,       //How would you like to handle the selected data?
        noUpdate = 0x643,               //No updates found.
        updateCompe = 0x644,            //Update Complete!

        unableDelete = 0x645,           //Unable to delete existing file after multiple attempts.
        gitNotFound = 0x646,            //git.exe path not found, make sure Git is installed and using correct path in application configuration.
        gitPathMiss = 0x647,            //Git Path is missing or empty in configuration. Using default Git path.
        hightLightNotExist = 0x648,     //The syntax highlighting file does not exist.
        useDefault = 0x649,             //Using default path.

        #endregion

        #region Chat
        ChatList = 0x651,       //Chat List
        NewChat = 0x652,        //New Chat
        DeleteChat = 0x653,     //Delete Chat
        HintChat = 0x654,       //Ask ChatBot anything.
        FileChat = 0x655,       //Attach File
        ImageChat = 0x656,      //Attach Image
        VideoChat = 0x657,      //Attach Video

        ApiEndpoint = 0x658,    //Api Endpoint
        ApiKey = 0x659,         //Api Key
        #endregion

        #region BanList Editor
        BanList = 0x661,        //BanList: 
        BanListName = 0x662,    //Name: 
        WhiteList = 0x663,      //White List
        Limit = 0x664,          //Limit: 
        BannedCard = 0x665,     //Banned
        LimitedCard = 0x666,    //Limited
        SemiLimitedCard = 0x667,//Semi-Limited
        UnLimitedCard = 0x668,  //Unlimited
        #endregion

        #region Deck Editor
        cmbDeck = 0x681,        //Deck: 
        AllowedCard = 0x682,    //Allowed Card
        AllCard = 0x683,        //All Cards
        SearchCardDeck = 0x68a, //Search Card
        NewDeckName = 0x68b,    //New Name

        MainDeck = 0x691,       //Main Deck
        ExtraDeck = 0x692,      //Extra Deck
        SideDeck = 0x693,       //Side Deck

        Monster = 0x6a1,    //Monster:
        Spell = 0x6a2,      //Spell:
        Trap = 0x6a3,       //Trap:
        Skill = 0x6a4,      //Skill:

        Ritual = 0x6a5,     //Ritual:
        Fusion = 0x6a6,     //Fusion:
        Synchro = 0x6a7,    //Synchro:
        eXceed = 0x6a8,     //eXceed:
        Link = 0x6a9,       //Link:
        #endregion
    }

    public static class LanguageExtensions
    {
        // Phương thức mở rộng thông thường thay vì toán tử chuyển đổi
        public static string ToText(this Language language)
        {
            return LanguageManager.GetText((uint)language);
        }
    }
    public static class UIntExtensions
    {
        public static string ToLanguageString(this uint code)
        {
            return LanguageManager.GetText(code);
        }
    }
}