﻿using System;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Globalization;
using System.Collections;
using System.Text.Encodings.Web;
using File = System.IO.File;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// TODO: Detect encrypted files:
    /// 1. .kuro extension
    /// 2. 1 word
    /// 3. characters per word == characters && characters == characters(with spaces)
    /// 4. the last 2 characters are "=="
    /// 
    /// TODO: Vanity options? (Font Preview Text, AppName)
    /// TODO: Upgrade CustomThemeManager "opacitySlideDelay" to DispatcherTimer
    /// TODO: AES Salt Manager... (random salt, custom salt?, or default) (add global secret key?)
    /// TODO: Add more error messages to language dictionary
    /// TODO: check (again) find/replace selecting the 1st occurance of any search term twice before continuing
    /// TODO: Hashing tool
    /// TODO: Setting(s) for: MainRtb.BorderThickness
    /// TODO: Auto-backup every 3,5,10,15 minutes? (in a seperate thread!)
    /// 
    /// TODO Lo: File filter string generator
    /// TODO Lo: List maker (.kurolist)
    /// TODO Lo: Some kind of PDF tool
    /// TODO Lo: Custom (up to 10 character) font dialog preview text

    /// </summary>
    public partial class MainWindow : Window
    {
        //Constants
        private const string OPEN_FILE_FILTER = "Common Plain Text Files (*.txt, *.kuro, *.bat, *.css, *.csv, *.html, *.js, *.json, *.md, *.py, *.sql, *.xml)|*.txt; *.kuro; *.bat; *.css; *.csv; *.html; *.js; *.json; *.md; *.py; *.sql; *.xml|" +
                                            "Text Files (*.txt)|*.txt|" +
                                            "KuroNotes (*.kuro)|*.kuro|" +
                                            "Batch Files (*.bat)|*.bat|" +
                                            "CSS Files (*.css)|*.css|" +
                                            "CSV Files (*.csv)|*.csv|" +
                                            "HTML Files (*.html)|*.html|" +
                                            "JavaScript Files (*.js)|*.js|" +
                                            "JSON Files (*.json)|*.json|" +
                                            "Markdown Files (*.md)|*.md|" +
                                            "Python Files (*.py)|*.py|" +
                                            "SQL Files (*.sql)|*.sql|" +
                                            "XML Files (*.xml)|*.xml|" +
                                            "All Files (*.*)|*.*";
        private const string OPEN_FILE_FILTER_RTF = "RTF Files (*.rtf)|*.rtf|" +
                                            "Common Plain Text Files (*.txt, *.kuro, *.bat, *.css, *.csv, *.html, *.js, *.json, *.md, *.py, *.sql, *.xml)|*.txt; *.kuro; *.bat; *.css; *.csv; *.html; *.js; *.json; *.md; *.py; *.sql; *.xml|" +
                                            "Text Files (*.txt)|*.txt|" +
                                            "KuroNotes (*.kuro)|*.kuro|" +
                                            "Batch Files (*.bat)|*.bat|" +
                                            "CSS Files (*.css)|*.css|" +
                                            "CSV Files (*.csv)|*.csv|" +
                                            "HTML Files (*.html)|*.html|" +
                                            "JavaScript Files (*.js)|*.js|" +
                                            "JSON Files (*.json)|*.json|" +
                                            "Markdown Files (*.md)|*.md|" +
                                            "Python Files (*.py)|*.py|" +
                                            "SQL Files (*.sql)|*.sql|" +
                                            "XML Files (*.xml)|*.xml|" +
                                            "All Files (*.*)|*.*";
        private const string SAVE_FILE_FILTER = "Text File (*.txt)|*.txt|" +
                                            "KuroNote (*.kuro)|*.kuro|" +
                                            "Batch File (*.bat)|*.bat|" +
                                            "CSS File (*.css)|*.css|" +
                                            "CSV File (*.csv)|*.csv|" +
                                            "HTML File (*.html)|*.html|" +
                                            "JavaScript File (*.js)|*.js|" +
                                            "JSON File (*.json)|*.json|" +
                                            "Markdown File (*.md)|*.md|" +
                                            "Python File (*.py)|*.py|" +
                                            "SQL File (*.sql)|*.sql|" +
                                            "XML File (*.xml)|*.xml|" +
                                            "All Files (*.*)|*.*";
        private const string SAVE_FILE_FILTER_RTF = "RTF File (*.rtf)|*.rtf|" +
                                            "Text File (*.txt)|*.txt|" +
                                            "KuroNote (*.kuro)|*.kuro|" +
                                            "Batch File (*.bat)|*.bat|" +
                                            "CSS File (*.css)|*.css|" +
                                            "CSV File (*.csv)|*.csv|" +
                                            "HTML File (*.html)|*.html|" +
                                            "JavaScript File (*.js)|*.js|" +
                                            "JSON File (*.json)|*.json|" +
                                            "Markdown File (*.md)|*.md|" +
                                            "Python File (*.py)|*.py|" +
                                            "SQL File (*.sql)|*.sql|" +
                                            "XML File (*.xml)|*.xml|" +
                                            "All Files (*.*)|*.*";
        private const long FILE_MAX_SIZE = 1048576;                 //Maximum supported file size in bytes (1MB)
        private const string FILE_SEARCH_EXE = "*.exe";
        private const string CUSTOM_THEME_EXT = ".kurotheme";
        private const string INTERNAL_IMAGE_EXT = ".jpg";           //custom theme destination extension is always this
        private const double PAGE_WIDTH_MAX = 1000000;
        private const double PAGE_WIDTH_RIGHT_MARGIN = 25;          //Number of width units to add (as a padding/buffer) in addition to the measured width of the content
        private const int DEFAULT_THEME_ID = 0;                     //if a custom theme file cannot be accessed, revert back to this theme
        private const int SPELL_CHECK_SUGGESTION_LIMIT = 3;         //Up to this number of spellcheck suggestions can be shown in the context menu at any given time

        //"Search with" constants
        public const string BING_SEARCH_DOMAIN = "https://www.bing.com/";
        public const string BRAVE_SEARCH_DOMAIN = "https://search.brave.com/";
        public const string DUCKDUCKGO_SEARCH_DOMAIN = "https://duckduckgo.com/";
        public const string GOOGLE_SEARCH_DOMAIN = "https://www.google.com/";
        public const string GOOGLESCHOLAR_SEARCH_DOMAIN = "https://scholar.google.com/";
        public const string STACKOVERFLOW_SEARCH_DOMAIN = "https://stackoverflow.com/";
        public const string STARTPAGE_SEARCH_DOMAIN = "https://www.startpage.com/";
        public const string WIKIPEDIA_SEARCH_DOMAIN = "https://en.wikipedia.org/";
        public const string YAHOO_SEARCH_DOMAIN = "https://search.yahoo.com/";
        public const string YOUTUBE_SEARCH_DOMAIN = "https://www.youtube.com/";
        public const string BING_SEARCH_PREFIX = "search?q=";
        public const string BRAVE_SEARCH_PREFIX = "search?q=";
        public const string DUCKDUCKGO_SEARCH_PREFIX = "?q=";
        public const string GOOGLE_SEARCH_PREFIX = "search?q=";
        public const string GOOGLESCHOLAR_SEARCH_PREFIX = "scholar?q=";
        public const string STACKOVERFLOW_SEARCH_PREFIX = "search?q=";
        public const string STARTPAGE_SEARCH_PREFIX = "sp/search?q=";
        public const string WIKIPEDIA_SEARCH_PREFIX = "wiki/";
        public const string YAHOO_SEARCH_PREFIX = "search?p=";
        public const string YOUTUBE_SEARCH_PREFIX = "results?search_query=";

        public const int SEARCH_WITH_MAX_QUERY_LENGTH = 3000;
        public const int SEARCH_WITH_MAX_TOOLTIP_LENGTH = 300;

        //RTF constants
        private const string RTF_DEFAULT_FONT_FAMILY = "Verdana";
        private const short RTF_DEFAULT_FONT_SIZE = 17;
        private static readonly short[] RTF_FONT_SIZES = { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        //Gamification constants
        private const int MAX_RANK = 25; //Ranks beyond this are treated as this value in the backend
        private const int AP_COPY = 2;
        private const int AP_CUT = 10;
        private const int AP_SAVE = 12;
        private const int AP_OPEN = 15;
        private const int AP_LAUNCH = 15;
        private const int AP_SAVE_AS = 30;
        private const int AP_PRINT = 40;
        private const int AP_ACHIEVEMENT = 125;

        //Globals
        public string appName = "KuroNote";
        public string appPath = AppDomain.CurrentDomain.BaseDirectory;
        private string customThemePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\CustomThemes\\";
        private string customDictionaryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\dictionary.lex";
        private KuroNoteSettings appSettings;
        private KuroNoteRecentFiles appRecents;
        private Log log;
        private string fileName = string.Empty;                     //Name of the loaded file - null if no file loaded

        private bool editedFlag = false;                            //Are there any unsaved changes?
        private Encoding selectedEncoding = Encoding.UTF8;          //Encoding for opening and saving files (Encoding.ASCII blocks unicode)
        public bool rtfFontSizeReadyFlag = false;                   //This flag prevents unwanted RTF font size changes triggered by "_SelectionChanged" by only activating when the font size dropdown opens
        public bool rtfFontFamilyReadyFlag = false;                 //This flag prevents unwanted RTF font family changes triggered by "_SelectionChanged"...

        public KuroNoteTheme[] themeCollection;
        public KuroNoteRank[] rankCollection;

        //English UI dictionary
        public Dictionary<string, string> EnUIDict;

        //English Error dictionary
        public Dictionary<int, string> EnMsgDict;
        public Dictionary<int, string> EnTitleDict;

        //English Achievment dictionary
        public KuroNoteAchievement[] EnAchDict;

        public MainWindow()
        {
            InitializeComponent();
            if (!isSetup()) {
                MessageBox.Show("Please run \"KuroNote Setup\" to complete installation.", "KuroNote is Not Fully Installed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            InitialiseSettings();
            InitialiseLog();
            InitialiseThemeCollection();
            setStatus("Welcome", true);
            if (appSettings.gamification) {
                InitialiseRankCollection();
                InitialiseAchievementDictionary();
                processHolidayGreetings();
                processStartupAchievements();
            }
            InitialiseUIDictionary();
            InitialiseMessageDictionary();
            ToggleCustomSpellcheckDictionaries(true);
            InitialiseFont();
            InitialiseTheme();
            if(appSettings.rememberRecentFiles) {
                InitialiseRecentFiles();
            }
            processImmediateSettings(false, false);
            processStartupSettings();

            processCmdLineArgs();
            toggleEdited(false);
            log.addLog(Environment.NewLine + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond + ": " + "Ready! Awaiting instructions", true);
        }

        /// <summary>
        /// Checks whether or not the setup file exists
        /// </summary>
        /// <returns>True if the setup file exists, false otherwise</returns>
        private bool isSetup()
        {
            return File.Exists(appPath + "conf/CD101.kuro");
        }

        /// <summary>
        /// Defines UI strings
        /// </summary>
        private void InitialiseUIDictionary()
        {
            EnUIDict = new Dictionary<string, string>();
            //File
            EnUIDict["FileMi"] = "File";
            EnUIDict["NewMi"] = "New";
                EnUIDict["NewMiTT"] = "Closes this file and creates a new file";
            EnUIDict["NewWinMi"] = "New Window";
            EnUIDict["NewRegularWinMi"] = "New Window";
                EnUIDict["NewRegularWinMiTT"] = "Opens a new " + appName + " window";
            EnUIDict["NewAdminWinMi"] = "New Administrator Window";
                EnUIDict["NewAdminWinMiTT"] = "Opens a new " + appName + " window with administrator rights";
            EnUIDict["OpenMi"] = "Open...";
                EnUIDict["OpenMiTT"] = "Opens an existing file.";
            EnUIDict["OpenRecentMi"] = "Open Recent";
            EnUIDict["PlaceholderRecentMi"] = "No Recent Files";
            EnUIDict["ClearRecentMi"] = "Clear Recent Files";
                EnUIDict["ClearRecentMiTT"] = "Clears all Recent Files";
            EnUIDict["SaveMi"] = "Save";
                EnUIDict["SaveMiTT"] = "Saves over this file if it already exists, otherwise saves this file as a specified file";
            EnUIDict["SaveAsMi"] = "Save As...";
                EnUIDict["SaveAsMiTT"] = "Saves this file as a specified file";
            EnUIDict["PrintMi"] = "Print...";
                EnUIDict["PrintMiTT"] = "Prints this file";
            EnUIDict["ExitMi"] = "Exit";
                EnUIDict["ExitMiTT"] = "Closes " + appName;
            //Edit
            EnUIDict["EditMi"] = "Edit";
            EnUIDict["CutMi"] = "Cut";
                EnUIDict["CutMiTT"] = "Moves any selected text to the clipboard";
            EnUIDict["CopyMi"] = "Copy";
                EnUIDict["CopyMiTT"] = "Copies any selected text to the clipboard";
            EnUIDict["PasteMi"] = "Paste";
                EnUIDict["PasteMiTT"] = "Pastes text from the clipboard over any selected text";
            EnUIDict["BingMi"] = "Search with Bing...";
                EnUIDict["BingMiTT1"] = "Searches for \"\" on Bing";
                EnUIDict["BingMiTT2"] = "Opens the Bing homepage";
            EnUIDict["BraveMi"] = "Search with Brave...";
                EnUIDict["BraveMiTT1"] = "Searches for \"\" on Brave";
                EnUIDict["BraveMiTT2"] = "Opens the Brave homepage";
            EnUIDict["DuckDuckGoMi"] = "Search with DuckDuckGo...";
                EnUIDict["DuckDuckGoMiTT1"] = "Searches for \"\" on DuckDuckGo";
                EnUIDict["DuckDuckGoMiTT2"] = "Opens the DuckDuckGo homepage";
            EnUIDict["GoogleMi"] = "Search with Google...";
                EnUIDict["GoogleMiTT1"] = "Searches for \"\" on Google";
                EnUIDict["GoogleMiTT2"] = "Opens the Google homepage";
            EnUIDict["GoogleScholarMi"] = "Search with Google Scholar...";
                EnUIDict["GoogleScholarMiTT1"] = "Searches for \"\" on Google Scholar";
                EnUIDict["GoogleScholarMiTT2"] = "Opens the Google Scholar homepage";
            EnUIDict["StackOverflowMi"] = "Search with Stack Overflow...";
                EnUIDict["StackOverflowMiTT1"] = "Searches for \"\" on Stack Overflow";
                EnUIDict["StackOverflowMiTT2"] = "Opens the Stack Overflow homepage";
            EnUIDict["StartpageMi"] = "Search with Startpage...";
                EnUIDict["StartpageMiTT1"] = "Searches for \"\" on Startpage";
                EnUIDict["StartpageMiTT2"] = "Opens the Startpage homepage";
            EnUIDict["WikipediaMi"] = "Search with Wikipedia...";
                EnUIDict["WikipediaMiTT1"] = "Searches for \"\" on Wikipedia";
                EnUIDict["WikipediaMiTT2"] = "Opens the Wikipedia homepage";
            EnUIDict["YahooMi"] = "Search with Yahoo!...";
                EnUIDict["YahooMiTT1"] = "Searches for \"\" on Yahoo!";
                EnUIDict["YahooMiTT2"] = "Opens the Yahoo! homepage";
            EnUIDict["YouTubeMi"] = "Search with YouTube...";
                EnUIDict["YouTubeMiTT1"] = "Searches for \"\" on YouTube";
                EnUIDict["YouTubeMiTT2"] = "Opens the YouTube homepage";
            EnUIDict["UndoMi"] = "Undo";
                EnUIDict["UndoMiTT"] = "Undoes the last change made to this file";
            EnUIDict["RedoMi"] = "Redo";
                EnUIDict["RedoMiTT"] = "Redoes the last undone change made to this file";
            EnUIDict["FindMi"] = "Find...";
                EnUIDict["FindMiTT"] = "Searches this file for a specified phrase";
            EnUIDict["ReplaceMi"] = "Replace...";
                EnUIDict["ReplaceMiTT"] = "Searches this file for a specified phrase and replaces it with another specified phrase";
            EnUIDict["SelectAllMi"] = "Select All";
                EnUIDict["SelectAllMiTT"] = "Selects all text within this file";
            //Format
            EnUIDict["FormatMi"] = "Format";
            EnUIDict["FontMi"] = "Font...";
                EnUIDict["FontMiTT"] = "Changes the font to a specified font";
            EnUIDict["FontUpMi"] = "Font Up";
                EnUIDict["FontUpMiTT"] = "Increments the font size";
            EnUIDict["FontDownMi"] = "Font Down";
                EnUIDict["FontDownMiTT"] = "Decrements the font size";
            //Tools
            EnUIDict["ToolsMi"] = "Tools";
            EnUIDict["AESMi"] = "AES Encryption";
            EnUIDict["AESEncMi"] = "AES Encryptor...";
                EnUIDict["AESEncMiTT"] = "Creates a copy of this file that is encrypted with a specified password by the Advanced Encryption Standard";
            EnUIDict["AESDecMi"] = "AES Decryptor...";
                EnUIDict["AESDecMiTT"] = "Creates a copy of this file that is decrypted with a specified password by the Advanced Encryption Standard";
            EnUIDict["SpellcheckDictionaryManagerMi"] = "Spell Check Dictionary Editor...";
                EnUIDict["SpellcheckDictionaryManagerMiTT"] = "Changes the list of custom words that Spell Check recognises";
            //Fullscreen
                EnUIDict["FullscreenMiTT0"] = "Enters Fullscreen";
                EnUIDict["FullscreenMiTT1"] = "Exits Fullscreen";
            //Options
                EnUIDict["OptionsMiTT"] = "Options and information";
            EnUIDict["ProfileMi"] = "My Profile...";
                EnUIDict["ProfileMiTT"] = "Displays your " + appName + " level and achievements";
            EnUIDict["OptionsDialogMi"] = "Options...";
                EnUIDict["OptionsDialogMiTT"] = "Displays various options that you can change to optimise your experience using " + appName;
            EnUIDict["ThemeMi"] = "Select Theme...";
                EnUIDict["ThemeMiTT"] = "Changes the theme to a specified theme";
            EnUIDict["CustomThemesMi"] = "Custom Themes...";
                EnUIDict["CustomThemesMiTT"] = "Opens the custom themes manager";
            EnUIDict["LoggingMi"] = "Logging";
            EnUIDict["ShowLogMi"] = "Show Log...";
                EnUIDict["ShowLogMiTT"] = "Opens the log for the current session";
            EnUIDict["ShowLogFilesMi"] = "Show Log Files...";
                EnUIDict["ShowLogFilesMiTT"] = "Opens the directory where " + appName + " log files are stored";
            EnUIDict["UpdatesMi"] = "Check for Updates...";
                EnUIDict["UpdatesMiTT"] = "Checks if this " + appName + " installation is up to date";
            EnUIDict["AboutMi"] = "About " + appName;
                EnUIDict["AboutMiTT"] = "Displays information about " + appName;
            //RTF Mode
                EnUIDict["rtfBoldRibbonTT"] = "Toggles bold font weight for the selection";
                EnUIDict["rtfItalicRibbonTT"] = "Toggles italic font style for the selection";
                EnUIDict["rtfUnderlineRibbonTT"] = "Toggles underline text decoration for the selection";
                EnUIDict["rtfFontFamilyCmbTT"] = "Changes the font family of the selection";
                EnUIDict["rtfFontSizeCmbTT"] = "Changes the font size of the selection";
                EnUIDict["rtfFontUpMiTT"] = "Increments the font size of the selection";
                EnUIDict["rtfFontDownMiTT"] = "Decrements the font size of the selection";
                EnUIDict["rtfApplySelectedColourMiTT"] = "Changes the font colour of the selection to the chosen colour";
                EnUIDict["rtfChooseColourMiTT"] = "Changes the chosen font colour (and applies it if there is a selection)";
                EnUIDict["rtfLeftAlignRibbonTT"] = "Applies left alignment to the selection";
                EnUIDict["rtfCenterAlignRibbonTT"] = "Applies center alignment to the selection";
                EnUIDict["rtfRightAlignRibbonTT"] = "Applies right alignment to the selection";
                EnUIDict["rtfJustifyAlignRibbonTT"] = "Applies justified alignment to the selection";
            //Spell Check
                EnUIDict["spellcheckSuggestionTT"] = "Replaces the spelling error text with "; //proceeded by "<replacement word>"
                EnUIDict["spellcheckNoSuggestionsTT"] = "Spell Check could not find any suggestions";
            EnUIDict["spellcheckIgnoreAll"] = "Ignore All";
                EnUIDict["spellcheckIgnoreAllTT"] = "Ignores all occurances of \"\" for this session"; //requires insert of <error phrase> at char 27
            EnUIDict["spellcheckAddToDictionary"] = "Add to Dictionary";
                EnUIDict["spellcheckAddToDictionaryTT"] = "Adds \"\" to your custom Spell Check dictionary"; //requires insert of <error phrase> at char 6

            /*
                JpUIDict = new Dictionary<string, object>();
                    JpUIDict["NewMi"] = "しんき";
                    JpUIDict["OpenMi"] = "あく";
                    //etc.
                    this.DataContext = JpUIDict;
            */
            FullscreenMi.ToolTip = EnUIDict["FullscreenMiTT0"];
            this.DataContext = EnUIDict;
        }

        /// <summary>
        /// Defines message strings
        /// </summary>
        private void InitialiseMessageDictionary ()
        {
            EnMsgDict = new Dictionary<int, string>();
            EnTitleDict = new Dictionary<int, string>();

            EnMsgDict[1] =  appName + " is not designed to load files larger than " + FILE_MAX_SIZE + " B. " +
                "Files of this size may significantly compromise performance. " +
                "Are you sure you want to open this file which exceeds the limit?";
            EnTitleDict[1] = "File size exceeds limit";

            EnMsgDict[2] = "There are unsaved changes. Would you like to save this file before exiting?";
            EnTitleDict[2] = "Save before exit?";

            EnMsgDict[3] = "There are unsaved changes. Would you like to save this file before opening a new one?";
            EnTitleDict[3] = "Save before open?";

            EnMsgDict[4] = "There are unsaved changes. Would you like to save this file before creating a new one?";
            EnTitleDict[4] = "Save before new?";

            EnMsgDict[5] = "Unsaved changes will be lost. Before you disable RTF Mode, would you like to save this file?";
            EnTitleDict[5] = "Save before disabling RTF Mode?";
            EnMsgDict[6] = "Would you like to save this file as an RTF File?";
            EnTitleDict[6] = "Save as an RTF File?";

            EnMsgDict[7] = "Would you like to save this file as a plain text file before enabling RTF Mode?";
            EnTitleDict[7] =  "Save plain text before enabling RTF Mode?";

            EnMsgDict[8] = "You can now open RTF files; saving will now include text formatting data.\n\n" +
                "NOTE: Text formatting data will now be added to plain text files (e.g. \".txt\") if they are overwritten.\n\n" +
                "You can enable or disable RTF Mode whenever you want to - no restart required!";
            EnTitleDict[8] = "Welcome to RTF Mode!";

            EnMsgDict[9] = "The file may have moved, or you may lack the permissions necessary to access it.";
            EnTitleDict[9] = "File cannot be accessed";

            EnMsgDict[10] = "RTF mode is currently disabled, but you are attempting to open an RTF file. Would you like to enable RTF mode before opening the file?";
            EnTitleDict[10] = "Enable RTF mode before open?";

            EnMsgDict[11] = "Are you sure you want to clear all recent files?";
            EnTitleDict[11] = "Clear recent files?";

            EnMsgDict[12] = "The list of recent files has been reset.\n\n" +
                "NOTE: You can enable or disable recent files in Options.";
            EnTitleDict[12] = "Recent files cleared";
        }

        /// <summary>
        /// Adds or removes both custom dictionaries (the static pack:// one and the appdata one)
        /// to/from MainRtb's spellchecker
        /// </summary>
        /// <param name="add">True if you want to add the dictionary, false otherwise</param>
        public void ToggleCustomSpellcheckDictionaries(bool add)
        {
            IList dictionaries = SpellCheck.GetCustomDictionaries(MainRtb);

            if (add) {
                //add the static custom dictionary
                dictionaries.Add(new Uri("pack://application:,,,/kuronote-spellcheck-custom.lex"));
                //If it exists, add the user custom dictionary
                if (File.Exists(customDictionaryPath)) {
                    dictionaries.Add(new Uri(customDictionaryPath));
                }
            } else {
                //Remove custom dictionaries
                dictionaries.Clear();
            }
        }

        /// <summary>
        /// Retrieves the specified error message and error message title
        /// </summary>
        /// <param name="_errorCode">The unique code of the error message to retrieve</param>
        /// <returns>An array containing the error message [0], and the error message title [1]</returns>
        private string[] getMessage(int _errorCode)
        {
            string[] errorMessage = new string[] {EnMsgDict[_errorCode], EnTitleDict[_errorCode]};
            return errorMessage;
        }

        /// <summary>
        /// Defines achievement names, descriptions and reward themes
        /// </summary>
        private void InitialiseAchievementDictionary()
        {
            EnAchDict = new KuroNoteAchievement[] {
                //Holiday achievmenets
                new KuroNoteAchievement(11, "New Year's Day", "Launch KuroNote on January 1st", false),
                new KuroNoteAchievement(214, "Valentine's Day", "Launch KuroNote on February 14th", false, themeCollection[25]), //"Hearts" theme
                new KuroNoteAchievement(317, "Saint Patrick's Day", "Launch KuroNote on March 17th", false),
                new KuroNoteAchievement(320, "International Day of Happiness", "Launch KuroNote on March 20th", false, themeCollection[27]), //"Yellow" theme
                new KuroNoteAchievement(422, "Earth Day", "Launch KuroNote on April 22nd", false, themeCollection[10]), //"Earth" theme
                new KuroNoteAchievement(54, "Star Wars Day", "Launch KuroNote on May 4th", false),
                new KuroNoteAchievement(621, "World Music Day", "Launch KuroNote on June 21st", false),
                new KuroNoteAchievement(720, "National Moon Day", "Launch KuroNote on July 20th", false, themeCollection[14]), //"Moon" theme
                new KuroNoteAchievement(88, "International Cat Day", "Launch KuroNote on August 8th", false),
                new KuroNoteAchievement(826, "International Dog Day", "Launch KuroNote on August 26th", false),
                new KuroNoteAchievement(921, "International Day of Peace", "Launch KuroNote on September 21st", false, themeCollection[3]), //"Eternal" theme
                new KuroNoteAchievement(1031, "Halloween", "Launch KuroNote on October 31st", false),
                new KuroNoteAchievement(1111, "Origami Day", "Launch KuroNote on November 11th", false, themeCollection[6]), //"Origami" theme
                new KuroNoteAchievement(1225, "Christmas Day", "Launch KuroNote on December 25th", false),
                //Other achievements
                new KuroNoteAchievement(1, "You actually read it", "Read KuroNote's product description", false),
                new KuroNoteAchievement(100, "Centurion", "Launch KuroNote 100 times", false, themeCollection[1]), //"Spectrum II" theme
                new KuroNoteAchievement(1000, "Startup Millenium", "Launch KuroNote 1000 times", false, themeCollection[2]), //"Spectrum III" theme
                new KuroNoteAchievement(5000, "1001110001000", "Launch KuroNote 5000 times", false),
                new KuroNoteAchievement(2, "Salvare", "\"Save As...\" 500 times", false),
                new KuroNoteAchievement(3, "Creator of Worlds", "\"Save As...\" 2000 times", false, themeCollection[16]), //"Creation Magic" theme
                new KuroNoteAchievement(4, "Make it Yours", "Create 5 Custom Themes", false),
                new KuroNoteAchievement(5, "Customs Connoisseur", "Create 15 Custom Themes", false),
                new KuroNoteAchievement(6060, "Physical Manifestation", "Print something", true),
                new KuroNoteAchievement(9, "Ne0phyt3", "\"AES Encrypt...\" 10 times", false),
                new KuroNoteAchievement(10, "Crypt0r", "\"AES Encrypt...\" 50 times", false, themeCollection[22]), //"<C0de Red/>" theme
                new KuroNoteAchievement(12, "Qualified CTRL+S'er", "\"Save\" 1000 times", false),
                new KuroNoteAchievement(13, "Better Save than Sorry", "\"Save\" 10000 times", false),
                new KuroNoteAchievement(14, "Nobody has ever done that", "Set the font to \"Wingdings\"", false),
                new KuroNoteAchievement(15, "Open Sesame", "\"Open...\" 2500 times", false),
                new KuroNoteAchievement(16, "CTRL+Outstanding", "\"Open...\" 7500 times", false),
                new KuroNoteAchievement(17, "Immerse", "Enter Fullscreen 10 times", false, themeCollection[8]), //"Immerse" theme
                new KuroNoteAchievement(18, "Deep Dive", "Enter Fullscreen 50 times", false),
                new KuroNoteAchievement(19, "James Cameron", "Enter Fullscreen 500 times", false),
                new KuroNoteAchievement(20, "Tinkerer", "Change Options 5 times", true, themeCollection[30]), //"PCB" theme
                new KuroNoteAchievement(42, "The Meaning of Life", "We rolled the dice, you got 42!", true)
            };
        }

        /// <summary>
        /// Initiates preset ranks
        /// </summary>
        private void InitialiseRankCollection()
        {
            //Declare array of ranks
            //NOTE: rankId = displayed level - 1 (e.g. level 25 is rankId 24)
            rankCollection = new KuroNoteRank[]
            {
                new KuroNoteRank(0, "Apprentice Wordcrafter", 1000, "#FFEEEEEE"),
                new KuroNoteRank(1, "Wordcrafter Initiate", 1200, "#FFF3F3FF"),
                new KuroNoteRank(2, "FileMode.Open", 1440, "#FFE3E3FF"),
                new KuroNoteRank(3, "FileMode.Create", 1728, "#FFD3D3FF"),
                new KuroNoteRank(4, ".TXT Enthusiast", 1987, "#FFC3C3FF"),
                new KuroNoteRank(5, "Wordcrafter", 2285, "#FFC3FFC3"),
                new KuroNoteRank(6, "ASCII Associate", 2628, "#FFB9FFB9"),
                new KuroNoteRank(7, ".TXT Specialist", 3022, "#FFAFFFAF"),
                new KuroNoteRank(8, "Master Wordcrafter", 3476, "#FFA5FFA5"),
                new KuroNoteRank(9, "ASCIIknight", 3997, "#FF9BFF9B"),
                new KuroNoteRank(10, "Unicoder", 4317, "#FF91FF91"),
                new KuroNoteRank(11, ".TXT Aficionado", 4662, "#FFFFFFC3"),
                new KuroNoteRank(12, "Apex ASCIIknight", 5035, "#FFFFFFB9"),
                new KuroNoteRank(13, "Notemaster", 5438, "#FFFFFFAF"),
                new KuroNoteRank(14, "UTF-8 Supremo", 5873, "#FFFFFFA5"),
                new KuroNoteRank(15, "Plain Text Pro", 6343, "#FFFFFF9B"),
                new KuroNoteRank(16, "Veteran Notemaster", 7294, "#FFFFFF91"),
                new KuroNoteRank(17, "Editor Extraordinaire", 8388, "#FFFFC3C3"),
                new KuroNoteRank(18, "Editor in Chief", 9646, "#FFFFB9B9"),
                new KuroNoteRank(19, "IO", 11093, "#FFFFAFAF"),
                new KuroNoteRank(20, "Plain Text Paragon", 12757, "#FFFFA5A5"),
                new KuroNoteRank(21, "Notemaster Shiro", 14671, "#FFFF9B9B"),
                new KuroNoteRank(22, "Notemaster Kuro", 16872, "#FFFF9191"),
                new KuroNoteRank(23, "Infinite ISOnaut", 25000, "#FFFF8787"),
                new KuroNoteRank(24, "Grand Notemaster", 25000, "#FFFF87FF"),
                new KuroNoteRank(25, "Grand Notemaster +", 25000, "#FFFF5FFF")
            };
        }

        /// <summary>
        /// Calls Settings to add the specified amount of AP and can handle levelling up (one level at a time only).
        /// This wrapper method accomodates MAX_RANK AP and automatically fills in the AP limit for each rank.
        /// </summary>
        /// <param name="ap">The amount of AP to add (NOTE: must not be large enough trigger multiple level-ups at once)</param>
        public void incrementAp(int ap)
        {
            if (appSettings.profL >= MAX_RANK) {
                appSettings.incrementAp(ap, rankCollection[MAX_RANK].apToNext);
            } else {
                appSettings.incrementAp(ap, rankCollection[appSettings.profL].apToNext);
            }
        }

        /// <summary>
        /// Retrieves the specified achievement information
        /// </summary>
        /// <param name="_achievementId">The unique achievmentId of the achievment to retrieve (NOTE: disregards EnAchDict's array index)</param>
        /// <returns>The specified KuroNoteAchievement object</returns>
        private KuroNoteAchievement getAchievement(int _achievementId)
        {
            foreach (KuroNoteAchievement achievement in EnAchDict) {
                if (achievement.achievementId == _achievementId) {
                    return achievement;
                }
            }
            return null;
        }

        /// <summary>
        /// Starts retrieving settings
        /// </summary>
        private void InitialiseSettings()
        {
            appSettings = new KuroNoteSettings();
            appSettings.RetrieveSettings();
            //Cannot be logged because logging is a settings choice that must first be retrieved here
        }

        /// <summary>
        /// Starts logging if logging is enabled
        /// </summary>
        private void InitialiseLog()
        {
            log = new Log(this, appSettings.logging);
            if (appSettings.logging) {
                log.beginLog();
                log.addLog("Executing from: " + appPath);
            } else {
                ShowLogMi.IsEnabled = false; //No point in showing the log if we aren't logging
            }
        }

        /// <summary>
        /// Determines whether or not the file exists and is accessible
        /// </summary>
        /// <param name="_path">The path to the file that will be tested</param>
        /// <returns>True if the file exists and is accessible, false otherwise</returns>
        private bool fileExists(string _path)
        {
            try {
                if (File.Exists(_path)) {
                    return true;
                } else {
                    return false;
                }
            } catch (IOException) {
                //file either doesn't exist or is not accessible
                return false;
            }
        }

        /// <summary>
        /// Determines whether or not the contents of a specified file exceed KuroNote's maximum file size
        /// </summary>
        /// <param name="_path">The path to the file that will be measured</param>
        /// <returns>True if the path is valid and the file exceeds FILE_MAX_SIZE, false otherwise</returns>
        private bool fileTooBig(string _path)
        {
            if (!_path.Equals(string.Empty))  {
                if (getFileSize(_path) > FILE_MAX_SIZE) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        /// <summary>
        /// Gets the size of the contents of a specified file
        /// </summary>
        /// <param name="path">The path to the file that will be measured</param>
        /// <returns>The number of bytes in the file, or -1 if a blank path is provided</returns>
        private long getFileSize(string _path)
        {
            if (!_path.Equals(string.Empty)) {
                return new FileInfo(_path).Length;
            } else {
                return -1;
            }
        }

        /// <summary>
        /// Determines whether or not there is a file in the command line args to be opened automatically
        /// </summary>
        /// <returns>True if there's a file in command line argument 1, false otherwise</returns>
        private bool hasCmdLineFile()
        {
            //The file arg is arg[1]. arg[0] contains the KuroNote dll
            if (Environment.GetCommandLineArgs().Length == 2) {
                log.addLog("File specified in command line arg[1]");
                return true;
            } else {
                log.addLog("No file specified in command line arg[1]");
                return false;
            }
        }

        /// <summary>
        /// If there is one, opens the file stored in command line argument 1
        /// </summary>
        private void processCmdLineArgs()
        {
            if (hasCmdLineFile()) {
                string[] args = Environment.GetCommandLineArgs();
                doOpen(false, args[1]);
            }
        }

        ///<summary>
        ///Adds the specified achievement code to the achievement array if it does not already exist
        ///</summary>
        public void unlockAchievement(int achievementCode)
        {
            if(!appSettings.achList.Contains(achievementCode)) {
                this.Topmost = false; //If KuroNote is topmost, this prevents AchievementDialog from appearing
                log.addLog("Achievement unlocked: " + achievementCode);
                appSettings.achList.Add(achievementCode);
                appSettings.achLast = achievementCode;
                incrementAp(AP_ACHIEVEMENT);
                appSettings.UpdateSettings();

                //Call AchievementDialog to notify the user of the new achievement unlock
                AchievementDialog achievementDialog = new AchievementDialog(this, appSettings, getAchievement(achievementCode), log);
                achievementDialog.toggleVisibility(true);
            } else {
                log.addLog("Achievement repeat: " + achievementCode);
            }
        }

        /// <summary>
        /// Process and apply settings that can take effect immediately (i.e. without restart)
        /// </summary>
        /// <param name="calledFromOptions">Whether or not this method was called from OptionsDialog</param>
        /// <param name="rtfChanged">Whether or not the RTF Mode state was just changed</param>
        public void processImmediateSettings(bool calledFromOptions, bool rtfChanged)
        {
            //Spell check
            MainRtb.SpellCheck.IsEnabled = appSettings.spellCheck;

            //Auto word selection
            MainRtb.AutoWordSelection = appSettings.autoWordSelection;

            //Float above other windows
            this.Topmost = appSettings.floating;

            //Use ASCII instead of UTF-8
            if (appSettings.useAscii) {
                selectedEncoding = Encoding.ASCII;
            } else {
                selectedEncoding = Encoding.UTF8;
            }

            //Word wrap
            processPageWidth();

            //Show full file path in title
            updateAppTitle();

            //Remember recent files (just disabled - don't show the menu item for the rest of this session)
            if (!appSettings.rememberRecentFiles) {
                OpenRecentMi.Visibility = Visibility.Collapsed;
            }

            //RTF Mode
            if (appSettings.rtfMode) {
                if (calledFromOptions) {
                    if (rtfChanged) {
                        //user just enabled RTF mode
                        handlePlainToRtfModeTransition();
                    }
                } else {
                    //this is running on startup because RTF mode was already set to activate
                    RtfMenu.Visibility = Visibility.Visible;
                    FontMi.IsEnabled = false; //RTF controls override the standard global font controls
                    populateRtfMenu();
                }
            } else {
                if (calledFromOptions) {
                    if (rtfChanged) {
                        //user just disabled RTF mode
                        handleRtfToPlainModeTransition();
                        setTheme(appSettings.themeId, appSettings.themeWithFont); //clear all RTF formatting by refreshing theme
                    }
                } else {
                    //this is running on startup because RTF mode was already set to be inactive
                    RtfMenu.Visibility = Visibility.Collapsed;
                    FontMi.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// To transition from normal mode to RTF mode:
        /// 1a). UNSAVED CHANGES: must offer to save any outstanding changes as Plain
        /// 1b). NO UNSAVED CHANGES: must reload the existing document as RTF if it has .rtf extension
        /// 2). must make rtfMenu visible
        /// <param name="enableRtfModeInOptions">Whether or not to update the options to enable RTF mode, if the transition completes</param>
        /// </summary>
        private void handlePlainToRtfModeTransition(bool showMessageBoxes = true, bool enableRtfModeInOptions = false)
        {
            if (editedFlag && showMessageBoxes) {
                //rtf mode enabled while there are unsaved changes

                var res = MessageBox.Show(getMessage(7)[0], getMessage(7)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes) {
                    //**save changes
                    appSettings.rtfMode = false; //temporarily re-disable rtfMode so plain-only save can be done
                    doSave();
                    appSettings.rtfMode = true;
                    doNew(true);
                    RtfMenu.Visibility = Visibility.Visible;
                    FontMi.IsEnabled = false;
                    populateRtfMenu();
                    if (enableRtfModeInOptions) {
                        appSettings.rtfMode = true;
                        appSettings.UpdateSettings();
                    }
                    if (!appSettings.seenRtfWelcome) {
                        //if first time toggling RTF Mode - show introductory message
                        MessageBox.Show(getMessage(8)[0], getMessage(8)[1], MessageBoxButton.OK, MessageBoxImage.Information);
                        appSettings.seenRtfWelcome = true;
                        appSettings.UpdateSettings();
                    }
                }
                else if (res == MessageBoxResult.No) {
                    //**don't save changes
                    RtfMenu.Visibility = Visibility.Visible;
                    FontMi.IsEnabled = false;
                    populateRtfMenu();
                    if (enableRtfModeInOptions) {
                        appSettings.rtfMode = true;
                        appSettings.UpdateSettings();
                    }
                    if (!appSettings.seenRtfWelcome) {
                        //if first time toggling RTF Mode - show introductory message
                        MessageBox.Show(getMessage(8)[0], getMessage(8)[1], MessageBoxButton.OK, MessageBoxImage.Information);
                        appSettings.seenRtfWelcome = true;
                        appSettings.UpdateSettings();
                    }
                } else if (res == MessageBoxResult.Cancel) {
                    //**I change my mind - re-disable RTF Mode
                    appSettings.rtfMode = false; //re-disable rtfMode
                    appSettings.UpdateSettings();
                }
            } else {
                //If RTF document is currently opened
                if (fileName.Length > 0 && isRTFFile(fileName)) {
                    //RTF document is currently opened in Plain Text mode - reload it as RTF
                    doOpen(false, fileName);
                }

                RtfMenu.Visibility = Visibility.Visible;
                FontMi.IsEnabled = false;
                populateRtfMenu();
                if (enableRtfModeInOptions) {
                    appSettings.rtfMode = true;
                    appSettings.UpdateSettings();
                }
                if (!appSettings.seenRtfWelcome && showMessageBoxes) {
                    //if first time toggling RTF Mode - show introductory message
                    MessageBox.Show(getMessage(8)[0], getMessage(8)[1], MessageBoxButton.OK, MessageBoxImage.Information);
                    appSettings.seenRtfWelcome = true;
                    appSettings.UpdateSettings();
                }
            }
        }

        /// <summary>
        /// To transition from RTF Mode to normal mode:
        /// 1). must offer to save any outstanding changes as RTF OR Plain
        /// 2). must re-open the previously open file (or force a "new" if there wasn't a file opened previously)
        /// 3). must collapse rtfMenu
        /// 4). must remove all remaining RTF formatting by reapplying the currently selected theme
        /// </summary>
        private void handleRtfToPlainModeTransition()
        {
            if (editedFlag) {
                log.addLog("WARNING: RTF mode disabled while there are unsaved changes");
                var resErr5 = MessageBox.Show(getMessage(5)[0], getMessage(5)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (resErr5 == MessageBoxResult.Yes) {
                    //**save changes, reopen

                    if (isRTFFile(fileName)) {
                        //the currently open file is an RTF file
                        //obviously save changes to an RTF file AS an RTF file
                        appSettings.rtfMode = true; //temporarily re-enable rtfMode so RTF-friendly save can be done
                        doSave();
                    } else {
                        //the currently open file is not an RTF file
                        //do you want to save changes AS an RTF file?
                        var resErr6 = MessageBox.Show(getMessage(6)[0], getMessage(6)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (resErr6 == MessageBoxResult.Yes) {
                            //save as an RTF file
                            appSettings.rtfMode = true; //temporarily re-enable rtfMode so RTF-friendly save can be done
                            doSave();
                        } else {
                            //save as a Plain file
                            appSettings.rtfMode = false; //disable rtfMode to do Plain-only save
                            doSave();
                        }
                    }
                    appSettings.rtfMode = false; //disable rtfMode to do Plain-only open

                    //if there's a file to reopen, open it
                    if (fileName.Length > 0) {
                        appSettings.rtfMode = false;
                        doOpen(false, fileName);
                    } else {
                        doNew(true);
                    }
                    RtfMenu.Visibility = Visibility.Collapsed;
                    FontMi.IsEnabled = true;
                } else if (resErr5 == MessageBoxResult.No) {
                    //**don't save changes, reopen

                    //if there's a file to reopen, open it
                    if (fileName.Length > 0) {
                        appSettings.rtfMode = false;
                        doOpen(false, fileName);
                    } else {
                        doNew(true);
                    }
                    RtfMenu.Visibility = Visibility.Collapsed;
                    FontMi.IsEnabled = true;
                } else {
                    //I change my mind - re-enable RTF mode!
                    appSettings.rtfMode = true; //re-enable rtfMode
                    appSettings.UpdateSettings();
                }
            } else {
                log.addLog("RTF mode disabled while safe to do so");
                //if there's a file to reopen, open it
                if (fileName.Length > 0) {
                    appSettings.rtfMode = false;
                    doOpen(false, fileName);
                } else {
                    doNew(true);
                }
                RtfMenu.Visibility = Visibility.Collapsed;
                FontMi.IsEnabled = true;
            }
        }

        /// <summary>
        /// Clears and populates rtfFontFamilyCmb and rtfFontSizeCmb
        /// </summary>
        private void populateRtfMenu()
        {
            //rtfFontFamilyCmb
            foreach (FontFamily ff in Fonts.SystemFontFamilies)
            {
                //Populate the rtfFontFamilyCmb drop down with all the installed system fonts
                ComboBoxItem item = new ComboBoxItem();
                item.Content = ff.Source;
                item.FontFamily = ff;
                rtfFontFamilyCmb.Items.Add(item);
            }
            rtfFontFamilyCmb.SelectedValue = RTF_DEFAULT_FONT_FAMILY;

            //rtfFontSizeCmb
            rtfFontSizeCmb.Items.Clear();
            foreach (short size in RTF_FONT_SIZES) {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = size;
                rtfFontSizeCmb.Items.Add(item);
            }
            rtfFontSizeCmb.SelectedValue = RTF_DEFAULT_FONT_SIZE;
        }

        /// <summary>
        /// Process and apply settings that only take effect upon launch (i.e. not immediately)
        /// </summary>
        private void processStartupSettings()
        {
            //Holidays and gamification
            if (appSettings.gamification) {
                ProfileMi.IsEnabled = true;
            }

            //Remember window size
            if (appSettings.rememberWindowSize) {
                this.Height = appSettings.windowHeight;
                this.Width = appSettings.windowWidth;
            }
        }

        /// <summary>
        /// Process holiday greetings and achievements
        /// </summary>
        private void processHolidayGreetings()
        {
            int nowDay = DateTime.Now.Day;
            int nowMonth = DateTime.Now.Month;
            int nowYear = DateTime.Now.Year;
            string user = Environment.UserName;

            switch (nowMonth)
            {
                //January
                case 1:
                    //New Years Day
                    if (nowDay == 1) {
                        appName = nowYear + "Note";
                        setStatus("Happy " + nowYear + " " + user + "!", false);
                        unlockAchievement(11);
                    }
                    break;
                //February
                case 2:
                    //Valentine's Day
                    if (nowDay == 14) {
                        setStatus("Happy Valentine's " + user + "!", false);
                        unlockAchievement(214); //Unlocks "Hearts" theme
                    }
                    break;
                //March
                case 3:
                    //St. Patrick's Day, Happiness Day
                    if (nowDay == 17) {
                        setStatus("Happy St. Patrick's Day " + user + "!", false);
                        unlockAchievement(317);
                    } else if (nowDay == 20) {
                        appName = "HappyNote";
                        setStatus("Happy Happiness Day " + user + "!", false);
                        unlockAchievement(320); //Unlocks "Yellow" theme
                    }
                    break;
                //April
                case 4:
                    //Earth Day
                    if (nowDay == 22) {
                        setStatus("Happy Earth Day " + user + "!", false);
                        unlockAchievement(422);
                    }
                    break;
                //May
                case 5:
                    //Star Wars Day
                    if (nowDay == 4) {
                        setStatus("May the fourth be with you " + user + "!", false);
                        unlockAchievement(54);
                    }
                    break;
                //June
                case 6:
                    //Music Day
                    if (nowDay == 21) {
                        setStatus("Happy Music Day " + user + "!", false);
                        unlockAchievement(621);
                    }
                    break;
                //July
                case 7:
                    //Moon Day
                    if (nowDay == 20) {
                        appName = "LunarNote";
                        setStatus("Happy Moon Day " + user + "!", false);
                        unlockAchievement(720);
                    }
                    break;
                //August
                case 8:
                    //World Cat Day, International Dog Day
                    if (nowDay == 8) {
                        appName = "NekoNote";
                        setStatus("Happy World Cat Day " + user + "!", false);
                        unlockAchievement(88);
                    } else if (nowDay == 26) {
                        appName = "InuNote";
                        setStatus("Happy International Dog Day " + user + "!", false);
                        unlockAchievement(826);
                    }
                    break;
                //September
                case 9:
                    //International Peace Day
                    if (nowDay == 21) {
                        setStatus("Happy Peace Day " + user + "!", false);
                        unlockAchievement(921); //Unlocks "Eternal" theme
                    }
                    break;
                //October
                case 10:
                    //Halloween
                    if (nowDay == 31) {
                        appName = "DeathNote";
                        setStatus("Happy Halloween " + user + "!", false);
                        unlockAchievement(1031);
                    }
                    break;
                //November
                case 11:
                    //Origami day
                    if (nowDay == 11) {
                        setStatus("Happy Origami day " + user + "!", false);
                        unlockAchievement(1111); //Unlocks "Origami" theme
                    }
                    break;
                //December
                case 12:
                    //Christmas, Creator Birthday
                    if (nowDay == 25) {
                        appName = "FestiveNote";
                        setStatus("Merry Christmas " + user + "!", false);
                        unlockAchievement(1225);
                    } else if (nowDay == 29) {
                        appName = "ShiroNote";
                    }
                    break;
            }
        }

        /// <summary>
        /// Process achievements obtainable by starting KuroNote
        /// </summary>
        private void processStartupAchievements()
        {
            incrementAp(AP_LAUNCH);
            appSettings.achStartups++;
            appSettings.UpdateSettings();

            switch (appSettings.achStartups) {
                case 100:
                    unlockAchievement(100);
                    break;
                case 1000:
                    unlockAchievement(1000);
                    break;
                case 5000:
                    unlockAchievement(5000);
                    break;
            }

            Random rnd = new Random();
            int luckyStartup = rnd.Next(1, 101); //Random between 1 and 100
            if (luckyStartup == 42) {
                unlockAchievement(42);
            }
        }

        /// <summary>
        /// Check for images in the custom themes directory that don't have associated .kurotheme files and delete them
        /// </summary>
        private void purgeOrphanedThemeImages()
        {
            string[] customThemeFiles;
            try {
                customThemeFiles = Directory.GetFiles(customThemePath);
            } catch (DirectoryNotFoundException) {
                log.addLog("WARN: \"CustomThemes\" directory does not exist, creating it");
                Directory.CreateDirectory(customThemePath);
                customThemeFiles = Directory.GetFiles(customThemePath);
            }

            for (int i = 0; i < customThemeFiles.Length; i++) {
                //Is the file an image?
                if (Path.GetExtension(customThemeFiles[i]).Equals(INTERNAL_IMAGE_EXT)) {
                    //Does the image have an associated .kurotheme file with the same file name?
                    if (!File.Exists(customThemePath + Path.GetFileNameWithoutExtension(customThemeFiles[i]) + CUSTOM_THEME_EXT)) {
                        //Associated .kurotheme file does not exist - delete the image
                        log.addLog("Purging orphaned image in custom theme directory: " + customThemeFiles[i]);
                        File.Delete(customThemeFiles[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Set the font according to the current font settings
        /// </summary>
        private void InitialiseFont()
        {
            MainRtb.FontFamily = new FontFamily(appSettings.fontFamily);
            MainRtb.FontSize = appSettings.fontSize;
            MainRtb.FontWeight = appSettings.fontWeight;
            MainRtb.FontStyle = appSettings.fontStyle;

            log.addLog("Font initialised: " +
                appSettings.fontFamily +
                " (" + appSettings.fontWeight + " " + appSettings.fontStyle + ") " +
                appSettings.fontSize);
        }

        /// <summary>
        /// Instantiates preset themes
        /// </summary>
        private void InitialiseThemeCollection()
        {
            Stretch stretchMode;
            if (appSettings.stretchImages) {
                stretchMode = Stretch.Fill;
            } else {
                stretchMode = Stretch.UniformToFill;
            }

            //Declare array of themes
            themeCollection = new KuroNoteTheme[]
            {
                new KuroNoteTheme
                (
                    0, "Spectrum", "Image by Gradienta on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-gradienta-6985193.jpg")),
                                     Opacity = 0.47,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                    new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    1, "Spectrum II", "Image by Gradienta on Pexels", 100,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-gradienta-6985045.jpg")),
                                     Opacity = 0.44,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(254, 255, 232)),
                    new SolidColorBrush(Color.FromRgb(204, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    2, "Spectrum III", "Image by Katie Rainbow on Pexels", 1000,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-katie-rainbow-8593815.jpg")),
                                     Opacity = 0.26,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(255, 252, 239)),
                    new SolidColorBrush(Color.FromRgb(255, 234, 248)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    3, "Eternal", "Image by Skyler Ewing on Pexels", 921,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-skyler-ewing-5748311.jpg")),
                                     Opacity = 0.3,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(247, 244, 241)),
                    new SolidColorBrush(Color.FromRgb(247, 244, 241)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Palatino Linotype", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    4, "Boundless Sky", "Image by Pixabay on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-258149.jpg")),
                                     Opacity = 0.28,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(184, 215, 237)),
                    new SolidColorBrush(Color.FromRgb(229, 229, 230)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    5, "Overly Orangey", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 228, 202)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4022107.jpg")),
                                     Opacity = 0.25,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(249, 201, 151)),
                    new SolidColorBrush(Color.FromRgb(251, 203, 151)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    6, "Origami", "Image by David Yu on Pexels", 1111,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-david-yu-1631516.jpg")),
                                     Opacity = 0.33,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(224, 211, 201)),
                    new SolidColorBrush(Color.FromRgb(230, 218, 210)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    7, "Sunset Ripples", "Image by Ben Mack on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 246, 237)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-ben-mack-5326909.jpg")),
                                     Opacity = 0.2,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(251, 240, 232)),
                    new SolidColorBrush(Color.FromRgb(242, 230, 222)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    8, "Immerse", "Image by Jess Loiterton on Pexels", 17,
                    new SolidColorBrush(Color.FromRgb(167, 232, 254)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-jess-loiterton-5007946.jpg")),
                                     Opacity = 0.33,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(136, 194, 211)),
                    new SolidColorBrush(Color.FromRgb(120, 183, 200)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    9, "Blossom", "Image by Antonio Janeski on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(254, 211, 254)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-antonio-janeski-cherry blossoms-4052701.jpg")),
                                     Opacity = 0.2,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(245, 204, 247)),
                    new SolidColorBrush(Color.FromRgb(235, 195, 237)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    10, "Earth", "Image by Olha Ruskykh on Pexels", 422,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-olha-ruskykh-7166020.jpg")),
                                     Opacity = 0.2,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(247, 245, 242)),
                    new SolidColorBrush(Color.FromRgb(251, 249, 247)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Bold, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    11, "Leafy Green", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(202, 253, 123)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4046687.jpg")),
                                     Opacity = 0.25,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(179, 214, 92)),
                    new SolidColorBrush(Color.FromRgb(174, 211, 93)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    12, "Paradise Found", "Image by Asad Photo Maldives on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-asad-photo-maldives-3320516.jpg")),
                                     Opacity = 0.2,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(212, 235, 254)),
                    new SolidColorBrush(Color.FromRgb(254, 254, 254)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    13, "Layers of Time", "Image by Fillipe Gomes on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-fillipe-gomes-5611219.jpg")),
                                     Opacity = 0.31,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(254, 245, 231)),
                    new SolidColorBrush(Color.FromRgb(254, 245, 231)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    14, "Moon", "Image by David Selbert on Pexels", 720,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-david-selbert-6468238.jpg")),
                                     Opacity = 0.32,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(178, 209, 219)),
                    new SolidColorBrush(Color.FromRgb(167, 175, 180)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    15, "Deep Red", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(160, 3, 3)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-66757560.jpg")),
                                     Opacity = 0.85,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(252, 120, 120)),
                    new SolidColorBrush(Color.FromRgb(252, 120, 120)),
                    new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    "Verdana", 17, FontWeights.Bold, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    16, "Creation Magic", "Image by Tamanna Rumee on Pexels", 2,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-tamanna-rumee-7986299.jpg")),
                                     Opacity = 0.38,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(205, 235, 248)),
                    new SolidColorBrush(Color.FromRgb(251, 230, 158)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    17, "Droplets of Hope", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4194853.jpg")),
                                     Opacity = 0.33,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(242, 239, 253)),
                    new SolidColorBrush(Color.FromRgb(237, 234, 253)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    18, "Abyss", "Distraction-free!", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                    new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                    new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                    new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    19, "Onyx", "Sleepy-eye friendly!", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(15, 15, 15)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    20, "Notepad", "Mightier than the sword!", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(255, 255, 210)),
                    new SolidColorBrush(Color.FromRgb(220, 220, 175)),
                    new SolidColorBrush(Color.FromRgb(220, 220, 175)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Comic Sans MS", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    21, "Terminal", "if (this.geek == true) { chosenTheme = themeCollection[21]; }", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                    "Consolas", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    22, "<C0de Red/>", "Bleed your message at the tone", 10,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(8, 8, 8)),
                    new SolidColorBrush(Color.FromRgb(255, 55, 55)),
                    new SolidColorBrush(Color.FromRgb(255, 55, 55)),
                    new SolidColorBrush(Color.FromRgb(255, 8, 8)),
                    "Consolas", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    23, "Antiqua", "Image by Pixabay on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-235985.jpg")),
                                     Opacity = 0.38,
                                     Stretch = stretchMode},
                    new SolidColorBrush(Color.FromRgb(188, 173, 166)),
                    new SolidColorBrush(Color.FromRgb(188, 173, 166)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Book Antiqua", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    24, "Contour", "Image by David Yu on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-david-yu-2684383.jpg")),
                                     Opacity = 0.3,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(228, 230, 235)),
                    new SolidColorBrush(Color.FromRgb(228, 230, 235)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Arial", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    25, "Hearts", "Image by Monstera on Pexels", 214,
                    new SolidColorBrush(Color.FromRgb(255, 194, 235)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-monstera-5874714.jpg")),
                                     Opacity = 0.33,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(204, 170, 215)),
                    new SolidColorBrush(Color.FromRgb(204, 170, 215)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Italic
                ),
                new KuroNoteTheme
                (
                    26, "Geometric", "Image by Damir Mijailovic on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(212, 243, 241)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-damir-mijailovic-3695238.jpg")),
                                     Opacity = 0.15,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(211, 243, 241)),
                    new SolidColorBrush(Color.FromRgb(208, 237, 235)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    27, "Yellow", "Image by Luis Quintero on Pexels", 320,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-luis-quintero-3101527.jpg")),
                                     Opacity = 0.38,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(255, 229, 158)),
                    new SolidColorBrush(Color.FromRgb(255, 215, 158)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    28, "Cotton Cloudy", "Image by Luis Quintero on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 236, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-luis-quintero-2842734.jpg")),
                                     Opacity = 0.4,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(153, 222, 238)),
                    new SolidColorBrush(Color.FromRgb(154, 206, 225)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    29, "Holographic", "Image by Miodrag Kitanović on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-miodrag-kitanović-4286932.jpg")),
                                     Opacity = 0.82,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(225, 251, 232)),
                    new SolidColorBrush(Color.FromRgb(253, 225, 250)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    30, "PCB", "Image by Pixabay on Pexels", 20,
                    new SolidColorBrush(Color.FromRgb(193, 255, 182)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-50711.jpg")),
                                     Opacity = 0.11,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(185, 245, 175)),
                    new SolidColorBrush(Color.FromRgb(185, 245, 175)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Consolas", 17, FontWeights.Bold, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    31, "Tight-knit", "Image by Mariakray on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-mariakray-9643032.jpg")),
                                     Opacity = 0.19,
                                     Stretch = stretchMode },
                    new SolidColorBrush(Color.FromRgb(254, 254, 253)),
                    new SolidColorBrush(Color.FromRgb(254, 254, 253)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                )
            };
        }

        /// <summary>
        /// Configure the visual settings
        /// </summary>
        private void InitialiseTheme()
        {
            this.Title = "New File - " + appName;

            setTheme(appSettings.themeId, appSettings.themeWithFont);

            //Set additional properties

            //main.MainRtb.SelectionBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            MainRtb.BorderThickness = new Thickness(0, 0, 0, 0); //No blue outline when you click inside the RTB
            MainRtb.Padding = new Thickness(5, 9, 5, 9); //So you don't start typing right on the edge of the RTB
            MainRtb.SetValue(Paragraph.LineHeightProperty, 1.0);
        }

        /// <summary>
        /// Sets up "Open Recent File" functionality
        /// </summary>
        private void InitialiseRecentFiles()
        {
            OpenRecentMi.Visibility = Visibility.Visible;
            appRecents = new KuroNoteRecentFiles(log);
            appRecents.retrieveRecentFiles();
        }

        /// <summary>
        /// Sets the status text in the bottom-left corner to the specified text
        /// </summary>
        /// <param name="_text">The status text to display</param>
        private void setStatus(string _text, bool _includeTime)
        {
            if(_includeTime) {
                StatusTb.Text = DateTime.Now.ToShortTimeString() + ": " + _text;
            } else {
                StatusTb.Text = _text;
            }
        }

        /// <summary>
        /// Sets the app title to "fileName - appName"
        /// Accounting for the "fullFilePath" preference which determines if the full path or just the name are shown
        /// </summary>
        private void updateAppTitle()
        {
            if (fileName != string.Empty) {
                if (appSettings.fullFilePath) {
                    this.Title = fileName + " - " + appName;
                } else {
                    this.Title = Path.GetFileName(fileName) + " - " + appName;
                }
            }
        }

        /// <summary>
        /// Set the edited state to on/off
        /// </summary>
        private void toggleEdited(bool _edited)
        {
            if (_edited) {
                editedFlag = true;
                SaveStatusTb.Text = "Unsaved Changes";
            } else {
                editedFlag = false;
                SaveStatusTb.Text = "Safe to Exit";
            }
        }

        #region New, Open, Save and Save As...
        /// <summary>
        /// Closes the current file and opens a new file
        /// </summary>
        /// <returns>True if the operation completed successfully, false otherwise</returns>
        /// <param name="forcedNew">If true, bypasses user consent</param>
        private bool doNew(bool forcedNew)
        {
            log.addLog("Request: New File");
            if (editedFlag && !forcedNew) {
                log.addLog("WARNING: New before saving");
                var res = MessageBox.Show(getMessage(4)[0], getMessage(4)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes || res == MessageBoxResult.Cancel) {
                    log.addLog("New cancelled");
                    if (res == MessageBoxResult.Yes) {
                        doSave();   //save
                    }
                    return false;   //don't continue with new operation
                }
            }
            
            fileName = string.Empty;
            TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            range.Text = string.Empty;
            this.Title = "New File - " + appName;
            toggleEdited(false);
            setStatus("New File", true);
            log.addLog("Content deleted");
            return true;
        }

        /// <summary>
        /// Menu > File > New > New File (or CTRL+N)
        /// </summary>
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doNew(false);
        }

        /// <summary>
        /// Returns true if the specified file name ends with ".rtf", false otherwise
        /// </summary>
        /// <param name="fileName">The file name to test</param>
        /// <returns>True if the specified file name ends with ".rtf", false otherwise</returns>
        public bool isRTFFile(string fileName)
        {
            return Path.GetExtension(fileName).Equals(".rtf");
        }

        /// <summary>
        /// Asks the user if they wish to enable RTF mode before opening the RTF file. Depending on the response, this method either:
        /// 1) Enables RTF mode, then opens the RTF file
        /// 2) Opens the RTF file in plain text mode
        /// 3) Cancels the open operation
        /// </summary>
        /// <param name="rtfFileName"></param>
        /// <returns>True if the RTF file was opened, false otherwise</returns>
        private bool handleOpenRtfFileInPlainMode(string rtfFileName)
        {
            //RTF mode is OFF but the user is attempting to open an RTF file
            log.addLog("WARNING: Open RTF file while RTF Mode is disabled");
            var res = MessageBox.Show(getMessage(10)[0], getMessage(10)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes) {
                //enable RTF mode, then open the RTF file
                log.addLog("Enabling RTF mode before opening the file");
                handlePlainToRtfModeTransition(false, true);
                doOpenRTF(rtfFileName);
                return true;
            } else if (res == MessageBoxResult.No) {
                //proceed with opening RTF file in plain text mode
                log.addLog("Proceed with opening the RTF file in plain text mode");
                doOpenPlain(rtfFileName);
                return true;
            } else {
                //do not proceed with operation
                log.addLog("Open operation cancelled");
                return false;
            }
        }

        /// <summary>
        /// Loads a file into the RTB
        /// </summary>
        /// <param name="showOptionalWarnings">If true, asks about unsaved changes and/or enabling RTF mode when attempting to open an RTF file, before opening</param>
        /// <param name="directOpenPath">Optional: specify file to open instead of using a file open dialog</param>
        /// <returns></returns>
        private bool doOpen(bool showOptionalWarnings, string directOpenPath = "")
        {            
            log.addLog("Request: Open");

            //If set to ask about unsaved changes and there are unsaved changes
            //ask if they want to a) proceed anyway b) proceed but save first c) cancel the operation
            if (editedFlag && showOptionalWarnings) {
                log.addLog("WARNING: Open before saving");
                var res = MessageBox.Show(getMessage(3)[0], getMessage(3)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes || res == MessageBoxResult.Cancel) {
                    log.addLog("Open cancelled");
                    if (res == MessageBoxResult.Yes) {
                        doSave();       //save
                    }
                    return false;   //don't continue with open operation
                }
            }

            if (directOpenPath.Equals("")) {
                //No file specified - use dialog to choose a file

                OpenFileDialog dlg = new OpenFileDialog();
                if (appSettings.rtfMode) {
                    dlg.Filter = OPEN_FILE_FILTER_RTF;
                } else {
                    dlg.Filter = OPEN_FILE_FILTER;
                }

                if (dlg.ShowDialog() == true) {
                    try {
                        if (fileExists(dlg.FileName)) {
                            if (fileTooBig(dlg.FileName)) {
                                log.addLog("WARNING: File size exceeds limit");
                                var res = MessageBox.Show(getMessage(1)[0], getMessage(1)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                if (res == MessageBoxResult.No) {
                                    log.addLog("Open cancelled due to file size");
                                    return false;
                                }
                            }
                        } else {
                            log.addLog("ERROR: \"" + dlg.FileName + "\" cannot be accessed - cancel open");
                            MessageBox.Show(getMessage(9)[0], getMessage(9)[1], MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        if (appSettings.rtfMode) {
                            doOpenRTF(dlg.FileName);
                        } else {
                            if (isRTFFile(dlg.FileName) && showOptionalWarnings) {
                                if (handleOpenRtfFileInPlainMode(dlg.FileName) == false) {
                                    //User cancelled the open operation, return early
                                    return false;
                                }
                            } else {
                                doOpenPlain(dlg.FileName);
                            }
                        }

                    } catch (Exception ex) {
                        //File cannot be accessed (e.g. used by another process)
                        log.addLog(ex.ToString());
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

            } else {
                //File specified - open that file
                log.addLog("Request: Open from cmd/drop - " + directOpenPath);

                MemoryStream ms = new MemoryStream();
                try {
                    if (fileExists(directOpenPath)) {
                        if (fileTooBig(directOpenPath)) {
                            log.addLog("WARNING: File size exceeds limit");
                            var res = MessageBox.Show(getMessage(1)[0], getMessage(1)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (res == MessageBoxResult.No) {
                                log.addLog("Open cancelled due to file size");
                                return false;
                            }
                        }
                    } else {
                        log.addLog("ERROR: \"" + directOpenPath + "\" cannot be accessed - cancel open");
                        MessageBox.Show(getMessage(9)[0], getMessage(9)[1], MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    if (appSettings.rtfMode) {
                        doOpenRTF(directOpenPath);
                    } else {
                        if (isRTFFile(directOpenPath) && showOptionalWarnings) {
                            if (handleOpenRtfFileInPlainMode(directOpenPath) == false) {
                                //User cancelled the open operation, return early
                                return false;
                            }
                        } else {
                            doOpenPlain(directOpenPath);
                        }
                    }
                
                } catch (Exception ex) {
                    //File cannot be accessed (e.g. used by another process)
                    log.addLog(ex.ToString());
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            updateAppTitle();
            toggleEdited(false);
            setStatus("Opened", true);
            if (!fileName.Equals("") && !fileName.Equals(null)) {
                appRecents.addRecentFile(fileName);
            }

            if (appSettings.gamification) {
                incrementAp(AP_OPEN);
                appSettings.achOpens++;
                appSettings.UpdateSettings();

                switch (appSettings.achOpens)
                {
                    case 2500:
                        unlockAchievement(15);
                        break;
                    case 7500:
                        unlockAchievement(16);
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Opens the specified file in Plain Text mode
        /// </summary>
        /// <param name="path">The full path of the file to open</param>
        private bool doOpenPlain(string path)
        {
            try {
                MemoryStream ms = new MemoryStream();
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }

                TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                range.Text = selectedEncoding.GetString(ms.ToArray());

                ms.Close();
                log.addLog("Successfully opened " + path);
                fileName = path;
                return true;

            } catch (Exception e) {
                log.addLog(e.ToString());
                MessageBox.Show(getMessage(9)[0], getMessage(9)[1], MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Opens the specified file in RTF mode
        /// </summary>
        /// <param name="path">The full path of the file to open</param>
        private bool doOpenRTF(string path)
        {
            try {
                TextRange rtfRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                FileStream rtfStream = new FileStream(path, FileMode.Open);
                rtfRange.Load(rtfStream, DataFormats.Rtf);
                rtfStream.Close();
                log.addLog("Successfully opened (RTF) " + path);
                fileName = path;
                return true;

            } catch (Exception e) {
                log.addLog(e.ToString());
                MessageBox.Show(getMessage(9)[0], getMessage(9)[1], MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Overrites the existing file with the contents of the RTB
        /// </summary>
        private void doSave()
        {
            if (editedFlag) {
                log.addLog("Request: Save");
                if (fileName.Equals(string.Empty)) {
                    log.addLog("File does not exist yet");
                    doSaveAs();
                } else {
                    TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                    MemoryStream ms = new MemoryStream(selectedEncoding.GetBytes(range.Text));

                    try {

                        if (appSettings.rtfMode) {
                            //RTF-capable save method
                            TextRange rtfRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                            FileStream rtfStream = new FileStream(fileName, FileMode.Create);
                            rtfRange.Save(rtfStream, DataFormats.Rtf);
                            rtfStream.Close();
                            log.addLog("Successfully saved (RTF) " + fileName);

                        } else {
                            //Plaintext-only save method
                            using (FileStream file = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write))
                            {
                                byte[] bytes = new byte[ms.Length];
                                ms.Read(bytes, 0, (int)ms.Length);
                                file.Write(bytes, 0, bytes.Length);

                                log.addLog("Successfully saved " + fileName);
                                ms.Close();
                            }
                        }

                        toggleEdited(false);
                        setStatus("Saved", true);

                        if (appSettings.gamification) {
                            incrementAp(AP_SAVE);
                            appSettings.achSaves++;
                            appSettings.UpdateSettings();

                            switch (appSettings.achSaves) {
                                case 1000:
                                    unlockAchievement(12);
                                    break;
                                case 10000:
                                    unlockAchievement(13);
                                    break;
                            }
                        }

                    } catch (Exception ex) {
                        //File cannot be accessed (e.g. used by another process)
                        log.addLog(ex.ToString());
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Saves a new file with the contents of the RTB, using a file selection dialog
        /// </summary>
        private void doSaveAs()
        {
            log.addLog("Request: Save As");
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExt = ".txt",
                AddExtension = true,
            };
            if (appSettings.rtfMode) {
                dlg.Filter = SAVE_FILE_FILTER_RTF;
            } else {
                dlg.Filter = SAVE_FILE_FILTER;
            }
            if (dlg.ShowDialog() == true) {
                TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                MemoryStream ms = new MemoryStream(selectedEncoding.GetBytes(range.Text));

                try {

                    if(appSettings.rtfMode) {
                        //RTF-capable save method
                        TextRange rtfRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                        FileStream rtfStream = new FileStream(dlg.FileName, FileMode.Create);
                        rtfRange.Save(rtfStream, DataFormats.Rtf);
                        rtfStream.Close();
                        log.addLog("Successfully saved (as) (RTF) " + dlg.FileName);

                    } else {
                        //Plaintext-only save method
                        using (FileStream file = new FileStream(dlg.FileName, FileMode.Create, System.IO.FileAccess.Write))
                        {
                            byte[] bytes = new byte[ms.Length];
                            ms.Read(bytes, 0, (int)ms.Length);
                            file.Write(bytes, 0, bytes.Length);
                            log.addLog("Successfully saved (as) " + dlg.FileName);
                            ms.Close();
                        }
                    }

                    fileName = dlg.FileName;
                    updateAppTitle();
                    toggleEdited(false);
                    setStatus("Saved", true);
                    appRecents.addRecentFile(fileName);

                    if (appSettings.gamification) {
                        incrementAp(AP_SAVE_AS);
                        appSettings.achSaveAs++;
                        appSettings.UpdateSettings();

                        switch (appSettings.achSaveAs) {
                            case 500:
                                unlockAchievement(2);
                                break;
                            case 2000:
                                unlockAchievement(3);
                                break;
                        }
                    }

                } catch (Exception ex) {
                    //File cannot be accessed (e.g. used by another process)
                    log.addLog(ex.ToString());
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Gets the colour-coded icon file path associated with the specified extension string
        /// </summary>
        /// <param name="extensionString">The file extension string to get the icon file for (e.g. ".txt")</param>
        /// <returns>The file path of the colour-coded icon associated with the specified extension string</returns>
        private string getFileIconUriForExt(string extensionString)
        {
            string iconsUriPrefix = "pack://application:,,,/img/icons/";
            string iconFileName = "recent_file_white_18dp.png"; //default icon for no extension or unrecognised extension

            switch (extensionString) {
                case ".txt":
                    iconFileName = "recent_file_inverted_18dp.png";
                    break;
                case ".kuro":
                    iconFileName = "recent_file_inverted_18dp.png";
                    break;
                case ".rtf":
                    iconFileName = "recent_file_rtf_18dp.png";
                    break;
                case ".bat":
                    iconFileName = "recent_file_red_18dp.png";
                    break;
                case ".css":
                    iconFileName = "recent_file_pink_18dp.png";
                    break;
                case ".csv":
                    iconFileName = "recent_file_blue_18dp.png";
                    break;
                case ".html":
                    iconFileName = "recent_file_orange_18dp.png";
                    break;
                case ".js":
                    iconFileName = "recent_file_purple_18dp.png";
                    break;
                case ".json":
                    iconFileName = "recent_file_sky_18dp.png";
                    break;
                case ".md":
                    iconFileName = "recent_file_green_18dp.png";
                    break;
                case ".py":
                    iconFileName = "recent_file_lime_18dp.png";
                    break;
                case ".sql":
                    iconFileName = "recent_file_yellow_18dp.png";
                    break;
                case ".xml":
                    iconFileName = "recent_file_brown_18dp.png";
                    break;
            }
            return iconsUriPrefix + iconFileName;
        }

        /// <summary>
        /// Updates Menu > File > Open Recent
        /// with a list of interactive Recent File menu items
        /// according to the appRecents object
        /// </summary>
        private void updateRecentFilesUI()
        {
            List<MenuItem> recentFileMis = new List<MenuItem>();

            //Create the MenuItems
            foreach (var recentFile in appRecents.recentFiles)
            {
                //Set MenuItem icon image
                Image imgRecentFileMi = new Image();
                imgRecentFileMi.Source = new BitmapImage(new Uri(getFileIconUriForExt(Path.GetExtension(recentFile))));

                //Create MenuItem
                MenuItem recentFileMi = new MenuItem()
                {
                    Header = recentFile,
                    ToolTip = "Opens " + recentFile,
                    Icon = imgRecentFileMi
                };
                recentFileMi.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(recentFileMi_Click));
                recentFileMis.Add(recentFileMi);
            }

            //Clear existing MenuItems
            OpenRecentMi.Items.Clear();

            //Insert the MenuItems
            for (int i = (recentFileMis.Count - 1); i >= 0; i--) {
                OpenRecentMi.Items.Add(recentFileMis[i]);
            }

            //If there are Recent Files, add the "Clear Recent Files" MenuItem,
            //else add the placeholder
            if (appRecents.recentFiles.Count > 0) {
                addClearRecentMiUI();             
            } else {
                addPlaceholderRecentMiUI();
            }
        }

        /// <summary>
        /// Adds the "No Recent Files" menu item to the
        /// Open Recent submenu
        /// </summary>
        private void addPlaceholderRecentMiUI()
        {
            MenuItem placeholderRecentMi = new MenuItem()
            {
                Header = EnUIDict["PlaceholderRecentMi"],
                IsEnabled = false
            };
            OpenRecentMi.Items.Add(placeholderRecentMi);
        }

        /// <summary>
        /// Adds the "Clear Recent Files" interactive menu item to the bottom of the
        /// Open Recent submenu
        /// </summary>
        private void addClearRecentMiUI()
        {
            MenuItem clearRecentMi = new MenuItem()
            {
                Header = EnUIDict["ClearRecentMi"],
                ToolTip = EnUIDict["ClearRecentMiTT"],
                Icon = new Image()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/clear_recent_outline_black_18dp.png"))
                }
            };
            clearRecentMi.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(ClearRecentMi_Click));

            OpenRecentMi.Items.Add(new Separator());
            OpenRecentMi.Items.Add(clearRecentMi);
        }

        /// <summary>
        /// When one of the dynamic Recent File menu items is clicked
        /// </summary>
        private void recentFileMi_Click(object sender, RoutedEventArgs e)
        {
            MenuItem thisRecentFileMi = (MenuItem)e.Source;
            if (doOpen(true, thisRecentFileMi.Header.ToString()) != true) {
                //Recent file failed to open
                log.addLog("WARN: Recent File \"" + thisRecentFileMi.Header.ToString() + "\" failed to open - removing it");
                appRecents.deleteRecentFile(thisRecentFileMi.Header.ToString());
            }           
        }

        /// <summary>
        /// Menu > File > Open Recent > Clear Recent Files
        /// </summary>
        private void ClearRecentMi_Click(object sender, RoutedEventArgs e)
        {
            log.addLog("Request: Clear Recent Files");
            var res = MessageBox.Show(getMessage(11)[0], getMessage(11)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes) {
                appRecents.clearRecentFiles();
                MessageBox.Show(getMessage(12)[0], getMessage(12)[1], MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                log.addLog("Clear recent files cancelled");
            }
        }

        /// <summary>
        /// Menu > File > Open... (or CTRL+O)
        /// </summary>
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doOpen(true);
        }

        /// <summary>
        /// When the user mouses over Menu > File
        /// This will always execute before a Recent File is opened
        /// </summary>
        private void FileMi_MouseEnter(object sender, MouseEventArgs e)
        {
            if (appSettings.rememberRecentFiles) {
                try {
                    appRecents.retrieveRecentFiles();
                    updateRecentFilesUI();
                } catch (NullReferenceException) {
                    log.addLog("WARN: Attempted to access appRecents, which does not exist");
                }
            }
        }

        /// <summary>
        /// Menu > File > Save (or CTRL+S)
        /// </summary>
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doSave();
        }

        /// <summary>
        /// Menu > File > Save As... (or CTRL+SHIFT+S)
        /// </summary>
        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doSaveAs();
        }
        #endregion

        /// <summary>
        /// Gets file names (and paths) from a specified folder that optionally match a specified search term
        /// </summary>
        /// <param name="_directory">The directory to search</param>
        /// <param name="_searchTerm">Optional: only get files that match a non-regex search string</param>
        /// <returns>A string array of matching file names (and paths)</returns>
        private string[] getFilesFromDirectory(string _directory, string _searchTerm = "*")
        {
            string[] files = {""};
            try {
                //Only get files that match the search term (e.g. "c*" gets files that begin with c)
                files = Directory.GetFiles(_directory, _searchTerm);
                log.addLog("Searching " + _directory + " with searchTerm '" + _searchTerm + "'");
                foreach (string file in files) {
                    log.addLog(">> " + file);
                }
                return files;
            } catch (Exception e) {
                log.addLog("ERROR: " + e.ToString());
                return files;
            }
        }

        /*
         THE FOLLOWING METHOD IS NOW OBSELETE SINCE ALL CONTEXT MENU ITEMS NOW REFRESH WHEN THE CONTEXT MENU OPENS

        /// <summary>
        /// Deletes existing spellcheck items (so that new ones can be added)
        /// NOTE: this method actually just deletes all context menu items that have a tag (because spellcheck items always have tags)
        /// this will need reworking in the unlikely event that other context menu items start having tags
        /// (e.g. check icon/header instead? || always rebuild cut,copy,paste)
        /// NOTE: an exception is thrown if you attempt to iterate through a collection WHILE modifying it
        /// </summary>
        private void purgePreviousSpellcheckItems()
        {
            List<MenuItem> deletionMenuItemList = new List<MenuItem>(); //list of context menu items to remove
            List<Separator> deletionSeparatorList = new List<Separator>(); //list of context menu separators to remove

            //find context menu items to remove (i.e. items with tags (spellcheck suggestion items))
            //and add them to the deletion list
            foreach (object contextItem in MainRtbContext.Items) {
                if (contextItem.GetType() == typeof(MenuItem)) {
                    //It's a menu item, add it to the appropriate deletion list if it has a tag
                    MenuItem contextMi = (MenuItem)contextItem;
                    if (contextMi.Tag != null) {
                        deletionMenuItemList.Add(contextMi);
                    }
                } else if (contextItem.GetType() == typeof(Separator)) {
                    //It's a separator, add it to the appropriate deletion list
                    Separator contextSep = (Separator)contextItem;
                    deletionSeparatorList.Add(contextSep);
                }
            }

            //go through the deletion lists and delete the old spellcheck suggestion related items
            foreach (MenuItem deletionItem in deletionMenuItemList) {
                MainRtbContext.Items.Remove(deletionItem);
            }
            foreach (Separator deletionItem in deletionSeparatorList) {
                MainRtbContext.Items.Remove(deletionItem);
            }
        }

        */

        /// <summary>
        /// If there is a spelling error at the caret,
        /// adds any available spellcheck suggestion context menu items to MainRtbContext
        /// </summary>
        /// <returns>True if there is a spelling error at the caret, false otherwise</returns>
        private bool addSpellcheckContextItems()
        {
            SpellingError se = MainRtb.GetSpellingError(MainRtb.CaretPosition);

            //is there a spelling error?
            if (se != null)
            {
                //there is a spelling error
                short suggestionCounter = 0;
                foreach (string suggestion in se.Suggestions) {
                    //add suggestion context menu items up until the suggestion limit
                    if (suggestionCounter < SPELL_CHECK_SUGGESTION_LIMIT) { //add "&& limitSuggestionsFlag" to control with settings
                        log.addLog("Spell Check suggests [" + suggestionCounter + "]: " + suggestion);
                        MainRtbContext.Items.Add(generateSpellcheckMenuItem(se, suggestion));
                        suggestionCounter++;
                    }
                }

                if (suggestionCounter == 0) {
                    //spelling error identified, but no suggestions available - show "no suggestions" item
                    MainRtbContext.Items.Add(generateNoSuggestionsItem(se));
                }

                MainRtbContext.Items.Add(new Separator());
                MainRtbContext.Items.Add(generateIgnoreAllItem(se));
                MainRtbContext.Items.Add(generateAddToDictionaryItem(se));
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Creates a spellcheck suggestion context menu item
        /// with the specified correction phrase
        /// </summary>
        /// <param name="suggestion">The specified correction phrase</param>
        /// <returns>A spellcheck suggestion context menu item</returns>
        private MenuItem generateSpellcheckMenuItem(SpellingError spellcheckError, string spellcheckSuggestion)
        {
            MenuItem suggestionItem = new MenuItem();
            suggestionItem.Tag = spellcheckError;
            suggestionItem.Header = spellcheckSuggestion;
            suggestionItem.ToolTip = EnUIDict["spellcheckSuggestionTT"] + "\"" + spellcheckSuggestion + "\"";
            suggestionItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_spellcheck_black_18dp.png")) };
            suggestionItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(spellcheckSuggestion_Click));
            return suggestionItem;
        }

        /// <summary>
        /// Creates a context menu item that simply states there are no suggestions available
        /// </summary>
        /// <returns>A spellcheck "no suggestions" context menu item</returns>
        private MenuItem generateNoSuggestionsItem(SpellingError spellcheckError)
        {
            MenuItem suggestionItem = new MenuItem();
            suggestionItem.Tag = spellcheckError;
            suggestionItem.Header = EnUIDict["spellcheckNoSuggestionsTT"];
            suggestionItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_spellcheck_black_18dp.png")) };
            suggestionItem.IsEnabled = false;
            return suggestionItem;
        }

        /// <summary>
        /// Creates a context menu item that does a default "Ignore All"
        /// </summary>
        /// <returns>A spellcheck "Ignore All" context menu item</returns>
        private MenuItem generateIgnoreAllItem(SpellingError spellcheckError)
        {
            MenuItem ignoreAllItem = new MenuItem();
            ignoreAllItem.Tag = spellcheckError;
            ignoreAllItem.Header = EnUIDict["spellcheckIgnoreAll"];
            ignoreAllItem.ToolTip = EnUIDict["spellcheckIgnoreAllTT"].Insert(27, MainRtb.GetSpellingErrorRange(MainRtb.CaretPosition).Text);
            ignoreAllItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_front_hand_black_18dp.png")) };
            ignoreAllItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(spellcheckIgnoreAll_Click));
            return ignoreAllItem;
        }

        /// <summary>
        /// Creates a spellcheck context menu item that adds the error phrase to a custom dictionary
        /// </summary>
        /// <param name="suggestion">The specified correction phrase</param>
        /// <returns>A spellcheck "Add to Dictionary" context menu item</returns>
        private MenuItem generateAddToDictionaryItem(SpellingError spellcheckError)
        {
            MenuItem addToDictionaryItem = new MenuItem();
            addToDictionaryItem.Tag = spellcheckError;
            addToDictionaryItem.Header = EnUIDict["spellcheckAddToDictionary"];
            addToDictionaryItem.ToolTip = EnUIDict["spellcheckAddToDictionaryTT"].Insert(6, MainRtb.GetSpellingErrorRange(MainRtb.CaretPosition).Text);
            addToDictionaryItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_menu_book_black_18dp.png")) };
            addToDictionaryItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(spellcheckAddToDictionary_Click));
            return addToDictionaryItem;
        }

        /// <summary>
        /// Adds the context menu items that precede the spellcheck items to MainRtbContext
        /// </summary>
        private void addUpperContextItems()
        {
            MainRtbContext.Items.Add(generateUndoItem());
            MainRtbContext.Items.Add(new Separator());
            MainRtbContext.Items.Add(generateCutItem());
            MainRtbContext.Items.Add(generateCopyItem());
            MainRtbContext.Items.Add(generatePasteItem());
        }

        /// <summary>
        /// Creates a context menu item that does a default "Undo"
        /// </summary>
        /// <returns>An "Undo" context menu item</returns>
        private MenuItem generateUndoItem()
        {
            MenuItem undoItem = new MenuItem();
            undoItem.Header = EnUIDict["UndoMi"];
            undoItem.ToolTip = EnUIDict["UndoMiTT"];
            undoItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_undo_black_18dp.png")) };
            undoItem.Command = ApplicationCommands.Undo;
            return undoItem;
        }

        /// <summary>
        /// Creates a context menu item that does a default "Cut"
        /// </summary>
        /// <returns>A "Cut" context menu item</returns>
        private MenuItem generateCutItem()
        {
            MenuItem cutItem = new MenuItem();
            cutItem.Header = EnUIDict["CutMi"];
            cutItem.ToolTip = EnUIDict["CutMiTT"];
            cutItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_content_cut_black_18dp.png")) };
            cutItem.Command = ApplicationCommands.Cut;
            return cutItem;
        }

        /// <summary>
        /// Creates a context menu item that does a default "Copy"
        /// </summary>
        /// <returns>A "Copy" context menu item</returns>
        private MenuItem generateCopyItem()
        {
            MenuItem copyItem = new MenuItem();
            copyItem.Header = EnUIDict["CopyMi"];
            copyItem.ToolTip = EnUIDict["CopyMiTT"];
            copyItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_content_copy_black_18dp.png")) };
            copyItem.Command = ApplicationCommands.Copy;
            return copyItem;
        }

        /// <summary>
        /// Creates a context menu item that does a default "Paste"
        /// </summary>
        /// <returns>A "Paste" context menu item</returns>
        private MenuItem generatePasteItem()
        {
            MenuItem pasteItem = new MenuItem();
            pasteItem.Header = EnUIDict["PasteMi"];
            pasteItem.ToolTip = EnUIDict["PasteMiTT"];
            pasteItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_content_paste_black_18dp.png")) };
            pasteItem.Command = ApplicationCommands.Paste;
            return pasteItem;
        }

        /// <summary>
        /// Adds the context menu items that proceed the spellcheck items to MainRtbContext
        /// </summary>
        private void addLowerContextItems()
        {
            MainRtbContext.Items.Add(generateSelectAllItem());
            addSearchWithMenuItems(true);
        }

        /// <summary>
        /// Adds "Search with..." MenuItems - those specified by appSettings, to either the context menu, or the "Edit" menu
        /// </summary>
        /// <param name="forContextMenu">If true, adds MenuItems to the context menu, otherwise adds MenuItems to the "Edit" menu</param>
        private void addSearchWithMenuItems(bool forContextMenu)
        {
            if (forContextMenu) {
                //Context menu
                MainRtbContext.Items.Add(new Separator());
                if (appSettings.searchBing) {
                    MainRtbContext.Items.Add(generateBingItem());
                }
                if (appSettings.searchBrave) {
                    MainRtbContext.Items.Add(generateBraveItem());
                }
                if (appSettings.searchDuckDuckGo) {
                    MainRtbContext.Items.Add(generateDuckDuckGoItem());
                }
                if (appSettings.searchGoogle) {
                    MainRtbContext.Items.Add(generateGoogleItem());
                }
                if (appSettings.searchGoogleScholar) {
                    MainRtbContext.Items.Add(generateGoogleScholarItem());
                }
                if (appSettings.searchStackOverflow) {
                    MainRtbContext.Items.Add(generateStackOverflowItem());
                }
                if (appSettings.searchStartpage) {
                    MainRtbContext.Items.Add(generateStartpageItem());
                }
                if (appSettings.searchWikipedia) {
                    MainRtbContext.Items.Add(generateWikipediaItem());
                }
                if (appSettings.searchYahoo) {
                    MainRtbContext.Items.Add(generateYahooItem());
                }
                if (appSettings.searchYouTube) {
                    MainRtbContext.Items.Add(generateYouTubeItem());
                }
            } else {
                //Edit menu
                Separator searchWithSep = new Separator();
                searchWithSep.Tag = "Search"; //This tag groups this separator with other "Search with..." items, preventing duplicate separators in the "Edit" menu
                EditMi.Items.Add(searchWithSep);
                if (appSettings.searchBing) {
                    EditMi.Items.Add(generateBingItem());
                }
                if (appSettings.searchBrave) {
                    EditMi.Items.Add(generateBraveItem());
                }
                if (appSettings.searchDuckDuckGo) {
                    EditMi.Items.Add(generateDuckDuckGoItem());
                }
                if (appSettings.searchGoogle) {
                    EditMi.Items.Add(generateGoogleItem());
                }
                if (appSettings.searchGoogleScholar) {
                    EditMi.Items.Add(generateGoogleScholarItem());
                }
                if (appSettings.searchStackOverflow) {
                    EditMi.Items.Add(generateStackOverflowItem());
                }
                if (appSettings.searchStartpage) {
                    EditMi.Items.Add(generateStartpageItem());
                }
                if (appSettings.searchWikipedia) {
                    EditMi.Items.Add(generateWikipediaItem());
                }
                if (appSettings.searchYahoo) {
                    EditMi.Items.Add(generateYahooItem());
                }
                if (appSettings.searchYouTube) {
                    EditMi.Items.Add(generateYouTubeItem());
                }
            }
        }

        /// <summary>
        /// Creates a context menu item that does a default "Select All"
        /// </summary>
        /// <returns>A "Select All" context menu item</returns>
        private MenuItem generateSelectAllItem()
        {
            MenuItem selectAllItem = new MenuItem();
            selectAllItem.Header = EnUIDict["SelectAllMi"];
            selectAllItem.ToolTip = EnUIDict["SelectAllMiTT"];
            selectAllItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_select_all_black_18dp.png")) };
            selectAllItem.Command = ApplicationCommands.SelectAll;
            return selectAllItem;
        }

        /// <summary>
        /// Removes all "Search with..." MenuItems from the "Edit" menu
        /// </summary>
        private void purgeSearchWithMenuItems()
        {
            log.addLog("Purging existing \"Search with...\" menu items");
            int index = 0;
            while (index < EditMi.Items.Count) {
                MenuItem menuItem;
                try {
                    menuItem = (MenuItem)EditMi.Items[index];
                    string menuItemTag = (string)menuItem.Tag;
                    if (menuItemTag != null && menuItemTag.Contains("Search")) {
                        //Found "Search with..." MenuItem, remove it so it can be re-added later
                        EditMi.Items.RemoveAt(index); //this item was removed, don't increment index otherwise it will skip over some items
                    } else {
                        index++; //this item was ignored, increment index
                    }
                } catch (InvalidCastException) {
                    //This is a "Separator" menu item - ignore all except the "Search with..." separator
                    Separator sepItem = (Separator)EditMi.Items[index];
                    string sepItemTag = (string)sepItem.Tag;
                    if (sepItemTag != null && sepItemTag.Contains("Search")) {
                        //Found "Search with..." Separator, remove it so it can be re-added later
                        EditMi.Items.RemoveAt(index); //this item was removed, don't increment index otherwise it will skip over some items
                    } else {
                        index++; //this item was ignored, increment index
                    }
                } catch (Exception e) {
                    log.addLog("ERROR: " + e.ToString());
                    break;
                }
            }
        }

        /// <summary>
        /// Create a "Search with Bing" context menu item that opens a Bing search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Bing" context menu item</returns>
        private MenuItem generateBingItem()
        {
            MenuItem bingItem = new MenuItem();
            bingItem.Header = EnUIDict["BingMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                bingItem.ToolTip = EnUIDict["BingMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            }
            else {
                //There is no selection
                bingItem.ToolTip = EnUIDict["BingMiTT2"];
            }
            bingItem.Tag = "Search Bing";
            bingItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/bing.png")) };
            bingItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return bingItem;
        }

        /// <summary>
        /// Create a "Search with Brave" context menu item that opens a Brave search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Brave" context menu item</returns>
        private MenuItem generateBraveItem()
        {
            MenuItem braveItem = new MenuItem();
            braveItem.Header = EnUIDict["BraveMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                braveItem.ToolTip = EnUIDict["BraveMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                braveItem.ToolTip = EnUIDict["BraveMiTT2"];
            }
            braveItem.Tag = "Search Brave";
            braveItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/brave.png")) };
            braveItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return braveItem;
        }

        /// <summary>
        /// Create a "Search with DuckDuckGo" context menu item that opens a DuckDuckGo search URL for any specified text
        /// </summary>
        /// <returns>A "Search with DuckDuckGo" context menu item</returns>
        private MenuItem generateDuckDuckGoItem()
        {
            MenuItem duckDuckGoItem = new MenuItem();
            duckDuckGoItem.Header = EnUIDict["DuckDuckGoMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                duckDuckGoItem.ToolTip = EnUIDict["DuckDuckGoMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                duckDuckGoItem.ToolTip = EnUIDict["DuckDuckGoMiTT2"];
            }
            duckDuckGoItem.Tag = "Search DuckDuckGo";
            duckDuckGoItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/duckduckgo.png")) };
            duckDuckGoItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return duckDuckGoItem;
        }

        /// <summary>
        /// Create a "Search with Google" context menu item that opens a Google search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Google" context menu item</returns>
        private MenuItem generateGoogleItem()
        {
            MenuItem googleItem = new MenuItem();
            googleItem.Header = EnUIDict["GoogleMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                googleItem.ToolTip = EnUIDict["GoogleMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                googleItem.ToolTip = EnUIDict["GoogleMiTT2"];
            }
            googleItem.Tag = "Search Google";
            googleItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/google.png")) };
            googleItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return googleItem;
        }

        /// <summary>
        /// Create a "Search with Google Scholar" context menu item that opens a Google Scholar search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Google Scholar" context menu item</returns>
        private MenuItem generateGoogleScholarItem()
        {
            MenuItem googleScholarItem = new MenuItem();
            googleScholarItem.Header = EnUIDict["GoogleScholarMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                googleScholarItem.ToolTip = EnUIDict["GoogleScholarMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                googleScholarItem.ToolTip = EnUIDict["GoogleScholarMiTT2"];
            }
            googleScholarItem.Tag = "Search Google Scholar";
            googleScholarItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/googlescholar.png")) };
            googleScholarItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return googleScholarItem;
        }

        /// <summary>
        /// Create a "Search with Stack Overflow" context menu item that opens a Stack Overflow search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Stack Overflow" context menu item</returns>
        private MenuItem generateStackOverflowItem()
        {
            MenuItem stackOverflowItem = new MenuItem();
            stackOverflowItem.Header = EnUIDict["StackOverflowMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                stackOverflowItem.ToolTip = EnUIDict["StackOverflowMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                stackOverflowItem.ToolTip = EnUIDict["StackOverflowMiTT2"];
            }
            stackOverflowItem.Tag = "Search StackOverflow";
            stackOverflowItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/stackoverflow.png")) };
            stackOverflowItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return stackOverflowItem;
        }

        /// <summary>
        /// Create a "Search with Startpage" context menu item that opens a Startpage search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Startpage" context menu item</returns>
        private MenuItem generateStartpageItem()
        {
            MenuItem startpageItem = new MenuItem();
            startpageItem.Header = EnUIDict["StartpageMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                startpageItem.ToolTip = EnUIDict["StartpageMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                startpageItem.ToolTip = EnUIDict["StartpageMiTT2"];
            }
            startpageItem.Tag = "Search Startpage";
            startpageItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/startpage.png")) };
            startpageItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return startpageItem;
        }

        /// <summary>
        /// Create a "Search with Wikipedia" context menu item that opens a Wikipedia search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Wikipedia" context menu item</returns>
        private MenuItem generateWikipediaItem()
        {
            MenuItem wikipediaItem = new MenuItem();
            wikipediaItem.Header = EnUIDict["WikipediaMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                wikipediaItem.ToolTip = EnUIDict["WikipediaMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                wikipediaItem.ToolTip = EnUIDict["WikipediaMiTT2"];
            }
            wikipediaItem.Tag = "Search Wikipedia";
            wikipediaItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/wikipedia.png")) };
            wikipediaItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return wikipediaItem;
        }

        /// <summary>
        /// Create a "Search with Yahoo!" context menu item that opens a Yahoo! search URL for any specified text
        /// </summary>
        /// <returns>A "Search with Yahoo!" context menu item</returns>
        private MenuItem generateYahooItem()
        {
            MenuItem yahooItem = new MenuItem();
            yahooItem.Header = EnUIDict["YahooMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                yahooItem.ToolTip = EnUIDict["YahooMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                yahooItem.ToolTip = EnUIDict["YahooMiTT2"];
            }
            yahooItem.Tag = "Search Yahoo!";
            yahooItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/yahoo.png")) };
            yahooItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return yahooItem;
        }

        /// <summary>
        /// Create a "Search with YouTube" context menu item that opens a YouTube search URL for any specified text
        /// </summary>
        /// <returns>A "Search with YouTube" context menu item</returns>
        private MenuItem generateYouTubeItem()
        {
            MenuItem youtubeItem = new MenuItem();
            youtubeItem.Header = EnUIDict["YouTubeMi"];
            if (MainRtb.Selection.Text.Length > 0) {
                //There is a selection
                youtubeItem.ToolTip = EnUIDict["YouTubeMiTT1"].Insert(14, limitString(MainRtb.Selection.Text, SEARCH_WITH_MAX_TOOLTIP_LENGTH, true));
            } else {
                //There is no selection
                youtubeItem.ToolTip = EnUIDict["YouTubeMiTT2"];
            }
            youtubeItem.Tag = "Search YouTube";
            youtubeItem.Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/youtube.png")) };
            youtubeItem.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(searchWith_Click));
            return youtubeItem;
        }

        /// <summary>
        /// When the context menu opens (i.e. the user right-clicks in MainRtb)
        /// </summary>
        private void MainRtbContext_Opened(object sender, RoutedEventArgs e)
        {
            MainRtbContext.Items.Clear(); //The context menu repopulates every time it is opened
            addUpperContextItems();
            MainRtbContext.Items.Add(new Separator());
            if (addSpellcheckContextItems()) {
                //Spell check context menu items were added, need an extra separator
                MainRtbContext.Items.Add(new Separator());
            }         
            addLowerContextItems();
        }

        /// <summary>
        /// When a dynamic spellcheck suggestion context menu item is clicked
        /// (this is where the correction occurs)
        /// </summary>
        private void spellcheckSuggestion_Click(object sender, RoutedEventArgs e)
        {
            MenuItem sourceMenuItem = (MenuItem)sender;
            SpellingError sourceSpellcheckError = (SpellingError)sourceMenuItem.Tag;
            string sourceSpellcheckSuggestion = sourceMenuItem.Header.ToString();

            //replace the error text with the suggestion text
            sourceSpellcheckError.Correct(sourceSpellcheckSuggestion);
            log.addLog("Spell Check implement suggestion: " + sourceSpellcheckSuggestion);
        }

        /// <summary>
        /// When a dynamic spellcheck "Ignore All" menu item is clicked
        /// (does a standard "ignore all" for this session)
        /// </summary>
        private void spellcheckIgnoreAll_Click(object sender, RoutedEventArgs e)
        {
            MenuItem sourceMenuItem = (MenuItem)sender;
            SpellingError sourceSpellcheckError = (SpellingError)sourceMenuItem.Tag;
            string currentSpellcheckError = MainRtb.GetSpellingErrorRange(MainRtb.CaretPosition).Text;

            //Do a standard "Ignore All" - ignore this particular spelling error everywhere but only for this session
            sourceSpellcheckError.IgnoreAll();
            log.addLog("Spell Check ignore all: " + currentSpellcheckError);
        }

        /// <summary>
        /// When a dynamic "Search with..." menu item is clicked
        /// Opens the default browser with a search link for the specified search engine
        /// </summary>
        private void searchWith_Click(object sender, RoutedEventArgs e)
        {
            MenuItem sourceMenuItem = (MenuItem)sender;
            string menuItemTag = (string)sourceMenuItem.Tag;
            string rawSearchTerm = MainRtb.Selection.Text;
            string encodedSearchTerm = "";
            if (rawSearchTerm.Length > 0) {
                log.addLog("Search term: " + rawSearchTerm);
                encodedSearchTerm = UrlEncoder.Create().Encode(rawSearchTerm);
                log.addLog("Encoded search term: " + encodedSearchTerm);
            } else {
                log.addLog("No search term provided");
            }

            switch(menuItemTag) {
                case "Search Bing":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(BING_SEARCH_DOMAIN + BING_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(BING_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search Brave":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(BRAVE_SEARCH_DOMAIN + BRAVE_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(BRAVE_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search DuckDuckGo":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(DUCKDUCKGO_SEARCH_DOMAIN + DUCKDUCKGO_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(DUCKDUCKGO_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search Google":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(GOOGLE_SEARCH_DOMAIN + GOOGLE_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(GOOGLE_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search Google Scholar":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(GOOGLESCHOLAR_SEARCH_DOMAIN + GOOGLESCHOLAR_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(GOOGLESCHOLAR_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search StackOverflow":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(STACKOVERFLOW_SEARCH_DOMAIN + STACKOVERFLOW_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(STACKOVERFLOW_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search Startpage":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(STARTPAGE_SEARCH_DOMAIN + STARTPAGE_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(STARTPAGE_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search Wikipedia":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(WIKIPEDIA_SEARCH_DOMAIN + WIKIPEDIA_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(WIKIPEDIA_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search Yahoo!":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(YAHOO_SEARCH_DOMAIN + YAHOO_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(YAHOO_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
                case "Search YouTube":
                    if (rawSearchTerm.Length > 0) {
                        //search with search term
                        startProcess(limitString(YOUTUBE_SEARCH_DOMAIN + YOUTUBE_SEARCH_PREFIX + encodedSearchTerm, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    } else {
                        //search without search term
                        startProcess(limitString(YOUTUBE_SEARCH_DOMAIN, SEARCH_WITH_MAX_QUERY_LENGTH, false));
                    }
                    break;
            }
        }

        /// <summary>
        /// When a dynamic spellcheck "Add to Dictionary" menu item is clicked
        /// (this adds the error phrase to the dictionary,
        /// then removes and re-adds the dictionary to MainRtb so the new additions are immediately recognised)
        /// </summary>
        private void spellcheckAddToDictionary_Click(object sender, RoutedEventArgs e)
        {
            MenuItem sourceMenuItem = (MenuItem)sender;
            SpellingError sourceSpellcheckError = (SpellingError)sourceMenuItem.Tag;
            string currentSpellcheckError = MainRtb.GetSpellingErrorRange(MainRtb.CaretPosition).Text;

            //Adds the word to the dictionary
            //then removes and re-adds the dictionary so that new additions are immediately recognised
            addWordToSpellcheckDictionary(currentSpellcheckError);
            ToggleCustomSpellcheckDictionaries(false); //remove and re-add the custom dictionary (i.e. refresh it)
            ToggleCustomSpellcheckDictionaries(true);  //
            log.addLog("Added \"" + currentSpellcheckError + "\" to the custom Spell Check dictionary");
        }

        /// <summary>
        /// Adds a specified word to the custom Spell Check dictionary
        /// (if the dictionary file (appdata/roaming/kuronote/dictionary.lex) does not exist, it creates a new one)
        /// </summary>
        /// <param name="word">The word to add to the custom Spell Check dictionary</param>
        void addWordToSpellcheckDictionary(string word)
        {
            try {
                //Append to the custom Spell Check dictionary file if it exists, otherwise create a new one
                using (StreamWriter sw = File.AppendText(customDictionaryPath)) {
                    sw.Write("\n" + word);
                };
            }
            catch (Exception e) {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Tools > Spell Check Dictionary Editor
        /// </summary>
        private void SpellcheckDictionaryManager_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: SpellcheckDictionaryDialog");
            SpellcheckDictionaryDialog spellcheckDictionaryDialog = new SpellcheckDictionaryDialog(this, appSettings, log);
            spellcheckDictionaryDialog.toggleVisibility(true);
        }

        /// <summary>
        /// Takes a string and returns a string limited to X number of characters, optionally with elipses
        /// </summary>
        /// <param name="maxLength">The maximum character length of the string to be returned (not including the 3 characters for optional elipses)</param>
        /// <param name="withElipses">Whether or not to append elipses to the returned limited string</param>
        /// <returns></returns>
        private string limitString(string inputString, int maxLength, bool withElipses)
        {
            try {
                if (withElipses) {
                    return inputString.Substring(0, maxLength) + "...";
                } else {
                    return inputString.Substring(0, maxLength);
                }
            } catch (ArgumentOutOfRangeException) {
                //string was less than maxLength
                return inputString;
            } catch (Exception e) {
                log.addLog("ERROR: " + e.ToString());
                return inputString;
            }
        }

        /// <summary>
        /// Executes the specified file, optionally with admin permissions
        /// </summary>
        /// <param name="_fileName">The filepath to execute</param>
        /// <param name="_asAdmin">Optional: specify true to run as admin</param>
        private void startProcess(string _fileName, bool _asAdmin = false)
        {
            try {
                log.addLog("Launching " + _fileName);
                Process process = new Process();
                process.StartInfo.FileName = _fileName;
                process.StartInfo.UseShellExecute = true; //enables opening things like folders without specifying the program to use
                if (_asAdmin) {
                    process.StartInfo.Verb = "runas";
                }
                process.Start();
            } catch (Exception e) {
                log.addLog("ERROR: " + e.ToString());
            }
        }

        #region New Window
        /// <summary>
        /// File > New Window > New Window
        /// </summary>
        private void NewRegWin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: New Regular Window");
            string[] executables = getFilesFromDirectory(appPath, FILE_SEARCH_EXE);
            startProcess(executables[0]);
        }

        /// <summary>
        /// File > New Window > New Administrator Window
        /// </summary>
        private void NewAdminWin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: New Admin Window");
            string[] executables = getFilesFromDirectory(appPath, FILE_SEARCH_EXE);
            startProcess(executables[0], true);
        }
        #endregion

        #region Print
        /// <summary>
        /// https://stackoverflow.com/users/1137199/dotnet's Simple Print Procedure
        /// If the font is small enough, the text will automatically use a multi-column layout
        /// </summary>
        private void doPrint()
        {
            log.addLog("Request: Print");
            //Clone the source document
            var str = XamlWriter.Save(MainRtb.Document);
            var stringReader = new StringReader(str);
            var xmlReader = XmlReader.Create(stringReader);
            var CloneDoc = XamlReader.Load(xmlReader) as FlowDocument;
            log.addLog("Successfully cloned FlowDocument");

            //Now print using PrintDialog
            log.addLog("Show PrintDialog");
            var pd = new PrintDialog();

            if (pd.ShowDialog().Value)
            {
                CloneDoc.PageHeight = pd.PrintableAreaHeight;
                CloneDoc.PageWidth = pd.PrintableAreaWidth;
                CloneDoc.PagePadding = new Thickness(10);

                //Use the currently selected font - depending on user selections, it will either be:
                //1. A preset theme font with a custom size (remember font size)
                //2. A preset theme font with a fixed preset size
                //3. A user-defined font and font size (defined outside of the context of any theme)

                //NOTE: These font settings should be automatically overridden by any existing formatting information in the document
                //i.e. RTF Mode
                if (appSettings.themeWithFont) {
                    //Use the font associated with the preset theme
                    if (appSettings.rememberFontUpDn) {
                        //Use the remembered custom font size
                        log.addLog("Using selected theme font (custom size)");
                        CloneDoc.FontFamily = new FontFamily(themeCollection[appSettings.themeId].fontFamily);
                        CloneDoc.FontSize = (short)appSettings.fontSize;
                        CloneDoc.FontWeight = themeCollection[appSettings.themeId].fontWeight;
                        CloneDoc.FontStyle = themeCollection[appSettings.themeId].fontStyle;

                    } else {
                        //Use the default theme font size
                        log.addLog("Using selected theme font (preset size)");
                        CloneDoc.FontFamily = new FontFamily(themeCollection[appSettings.themeId].fontFamily);
                        CloneDoc.FontSize = themeCollection[appSettings.themeId].fontSize;
                        CloneDoc.FontWeight = themeCollection[appSettings.themeId].fontWeight;
                        CloneDoc.FontStyle = themeCollection[appSettings.themeId].fontStyle;
                    }
                } else {
                    //Use appsettings font
                    log.addLog("Using user-defined font");
                    CloneDoc.FontFamily = new FontFamily(appSettings.fontFamily);
                    CloneDoc.FontSize = (short)appSettings.fontSize;
                    CloneDoc.FontWeight = appSettings.fontWeight;
                    CloneDoc.FontStyle = appSettings.fontStyle;
                }

                IDocumentPaginatorSource idocument = CloneDoc as IDocumentPaginatorSource;

                pd.PrintDocument(idocument.DocumentPaginator, "Printing FlowDocument");
                log.addLog("Successfully printed " + fileName);

                if (appSettings.gamification) {
                    incrementAp(AP_PRINT);
                    unlockAchievement(6060);
                    appSettings.UpdateSettings();
                }
            }
        }

        /// <summary>
        /// Menu > File > Print
        /// </summary>
        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doPrint();
        }
        #endregion

        /// <summary>
        /// If there are unsaved changes: uses a dialog to confirm weather or not the user wants to exit
        /// </summary>
        /// <returns>True if the user wants to exit, false otherwise</returns>
        private bool doExit()
        {
            if (editedFlag) {
                log.addLog("WARNING: Exit before saving");
                var res = MessageBox.Show(getMessage(2)[0], getMessage(2)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes || res == MessageBoxResult.Cancel) {
                    log.addLog("Exit cancelled");
                    if (res == MessageBoxResult.Yes) {
                        doSave();
                    }
                    return false;
                } else {
                    //Remember window size if the option is enabled
                    if (appSettings.rememberWindowSize) {
                        appSettings.windowHeight = this.Height;
                        appSettings.windowWidth = this.Width;
                        appSettings.UpdateSettings();
                    }

                    //Remember exact font size if "remember font up down size" enabled
                    if (appSettings.rememberFontUpDn) {
                        appSettings.fontSize = (int)MainRtb.FontSize;
                        appSettings.UpdateSettings();
                    }
                    log.addLog("Exiting");
                    return true;
                }
            } else {
                //Remember window size if the option is enabled
                if (appSettings.rememberWindowSize) {
                    appSettings.windowHeight = this.Height;
                    appSettings.windowWidth = this.Width;
                    appSettings.UpdateSettings();
                }
                log.addLog("Exiting");
                return true;
            }
        }

        /// <summary>
        /// Menu > File > Exit (or ALT+F4)
        /// </summary>
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        #region Cut, Copy, Paste, Undo, Redo and Select All
        /// <summary>
        /// Custom plaintext implementation of "Cut"
        /// </summary>
        private void Cut()
        {
            try {
                log.addLog("Request: Cut");
                if (!appSettings.rtfMode) {
                    //Force plaintext cut
                    TextRange cutRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
                    string textToCut = cutRange.Text;
                    cutRange.Text = String.Empty;
                    Clipboard.SetData(DataFormats.UnicodeText, textToCut);
                }
                incrementAp(AP_CUT);
            } catch (Exception e) {
                log.addLog("ERROR: Clipboard error during Cut");
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Custom plaintext implementation of "Copy"
        /// </summary>
        private void Copy()
        {
            try {
                log.addLog("Request: Copy");
                if (!appSettings.rtfMode) {
                    //Force plaintext copy
                    TextRange copyRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
                    string textToCopy = copyRange.Text;
                    Clipboard.SetData(DataFormats.UnicodeText, textToCopy);
                }
                incrementAp(AP_COPY);
            } catch (Exception e) {
                log.addLog("ERROR: Clipboard error during Copy");
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Make the clipboard text plain text to prepare for the default paste operation
        /// </summary>
        private void Paste()
        {
            try {
                log.addLog("Request: Paste");
                if (!appSettings.rtfMode) {
                    //Force plaintext paste
                    string textToPaste = Clipboard.GetText();
                    Clipboard.SetData(DataFormats.UnicodeText, textToPaste);
                }
                //Default paste operation runs
            }
            catch (Exception e) {
                log.addLog("ERROR: Clipboard error during Paste");
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Captures events before they happen - overrides default behaviour where necessary
        /// Note: Switch doesn't seem to be possible here - ApplicationCommands.X are not constant
        /// and the default .toString() is useless
        /// </summary>
        private void rtbEditor_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //*Overridden*
            if (e.Command == ApplicationCommands.Cut) {
                if (!appSettings.rtfMode) {
                    e.Handled = true; //Disable default Copy operation - use my own
                }
                Cut();
            } else if (e.Command == ApplicationCommands.Copy) {
                if (!appSettings.rtfMode) {
                    e.Handled = true; //Disable default Copy operation - use my own
                }
                Copy();
            } else if (e.Command == ApplicationCommands.Paste) {
                //Cancel Paste operation if the clipboard contains non-text data
                if (Clipboard.ContainsFileDropList() == true || Clipboard.ContainsImage() == true || Clipboard.ContainsAudio() == true) {
                    e.Handled = true;
                    log.addLog("Clipboard contains non-text data - paste cancelled");
                } else {
                    Paste();
                    //Allow default Paste operation afterwards
                }

                //*Disallowed unless RTF Mode*
            } else if (e.Command == EditingCommands.AlignCenter ||
                       e.Command == EditingCommands.AlignJustify ||
                       e.Command == EditingCommands.AlignLeft ||
                       e.Command == EditingCommands.AlignRight ||
                       e.Command == EditingCommands.DecreaseIndentation ||
                       e.Command == EditingCommands.IncreaseIndentation ||
                       e.Command == EditingCommands.ToggleBold ||
                       e.Command == EditingCommands.ToggleItalic ||
                       e.Command == EditingCommands.ToggleUnderline ||
                       e.Command == EditingCommands.ToggleBullets ||
                       e.Command == EditingCommands.ToggleNumbering ||
                       e.Command == EditingCommands.ToggleSubscript ||
                       e.Command == EditingCommands.ToggleSuperscript ||
                       e.Command == EditingCommands.DecreaseFontSize ||
                       e.Command == EditingCommands.IncreaseFontSize) {
                if (!appSettings.rtfMode) {
                    e.Handled = true; //Disable
                }

            //*"Overtyping" can be enabled/disabled by the user*
            } else if (e.Command == EditingCommands.ToggleInsert) {
                if (!appSettings.overTyping) {
                    e.Handled = true; //Disable
                }
 
            //*Always disallowed*
            } /*else if (e.Command == EditingCommands.DecreaseFontSize || //These 2 completely mess up existing font size functionality
                       e.Command == EditingCommands.IncreaseFontSize) {
                e.Handled = true; //Disable
            }*/

            //*Commands not listed are always allowed*
            //*Editing.ToggleBullets and Editing.ToggleNumbering could arguably be made available for plaintext mode*
        }
        #endregion

        /// <summary>
        /// Menu > Edit > Find...
        /// </summary>
        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Find");
            FindDialog findDialog = new FindDialog(this, appSettings, log, false);
            findDialog.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Edit > Replace...
        /// </summary>
        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Replace");
            FindDialog findDialog = new FindDialog(this, appSettings, log, true);
            findDialog.toggleVisibility(true);
        }

        #region Font
        /// <summary>
        /// Change the font to the specified font
        /// </summary>
        /// <param name="_fontFamily">The name of the font to apply</param>
        /// <param name="_fontSize">The size (in "pts") of the font to apply</param>
        /// <param name="_fontWeight">The degree of boldness of the font to apply</param>
        /// <param name="_fontStyle">The degree of italic of the font to apply</param>
        public void setFont(String _fontFamily, short _fontSize, FontWeight _fontWeight, FontStyle _fontStyle)
        {
            MainRtb.FontFamily = new FontFamily(_fontFamily);
            MainRtb.FontSize = _fontSize;
            MainRtb.FontWeight = _fontWeight;
            MainRtb.FontStyle = _fontStyle;
        }

        /// <summary>
        /// Menu > Format > Font Up
        /// </summary>
        private void FontUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doFontUp();
        }

        /// <summary>
        /// Increase font size by 1
        /// </summary>
        private void doFontUp()
        {
            if (appSettings.rtfMode) {
                log.addLog("Request: FontUp (RTF)");
                TextPointer currentCaretPos = MainRtb.Document.ContentStart;
                try {
                    currentCaretPos = MainRtb.CaretPosition;
                } catch(Exception) {
                    //cannot get caret, use previously assigned default ContentStart position
                }
                MainRtb.SelectAll();
                EditingCommands.IncreaseFontSize.Execute(null, MainRtb);
                MainRtb.Selection.Select(currentCaretPos, currentCaretPos);
            } else {
                short currentFontSize = (short)MainRtb.FontSize;
                log.addLog("Request: FontUp [" + currentFontSize + " -> " + (currentFontSize + 1) + "]");
                try {
                    MainRtb.FontSize += 1;
                } catch (Exception){
                    log.addLog("ERROR: Cannot increase font size from " + MainRtb.FontSize);
                }
            }
        }

        /// <summary>
        /// Menu > Format > Font Down
        /// </summary>
        private void FontDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doFontDown();
        }

        /// <summary>
        /// Decrease font size by 1
        /// </summary>
        private void doFontDown()
        {
            if (appSettings.rtfMode) {
                log.addLog("Request: FontDown (RTF)");
                TextPointer currentCaretPos = MainRtb.Document.ContentStart;
                try {
                    currentCaretPos = MainRtb.CaretPosition;
                } catch (Exception) {
                    //cannot get caret, use previously assigned default ContentStart position
                }
                MainRtb.SelectAll();
                EditingCommands.DecreaseFontSize.Execute(null, MainRtb);
                MainRtb.Selection.Select(currentCaretPos, currentCaretPos);
            } else {
                short currentFontSize = (short)MainRtb.FontSize;
                log.addLog("Request: FontDown [" + currentFontSize + " -> " + (currentFontSize - 1) + "]");
                try {
                    MainRtb.FontSize -= 1;
                } catch (Exception) {
                    log.addLog("ERROR: Cannot increase font size from " + MainRtb.FontSize);
                }
            }
        }

        /// <summary>
        /// Menu > Format > Font...
        /// </summary>
        private void Font_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Font");
            FontDialog fontDialog = new FontDialog(this, appSettings, log);
            fontDialog.toggleVisibility(true);
        }
        #endregion

        /// <summary>
        /// Menu > Security > AES Encryption > AES Encrypt...
        /// </summary>
        private void AESEnc_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: AES Encrypt");
            TextRange encRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            EncryptDialog enc = new EncryptDialog(this, appSettings, log, encRange.Text);
            enc.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Security > AES Encryption > AES Decrypt...
        /// </summary>
        private void AESDec_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: AES Decrypt");
            TextRange decRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            DecryptDialog dec = new DecryptDialog(this, appSettings, log, decRange.Text);
            dec.toggleVisibility(true);
        }

        #region Theme
        /// <summary>
        /// Change the theme to the specified theme, optionally with the corresponding theme font
        /// </summary>
        /// <param name="_themeId">The ID of the theme to change to</param>
        /// <param name="_includeFont">True if you want to change the font to the font that comes with the theme, false otherwise</param>
        public void setTheme(int _themeId, bool _includeFont)
        {
            if (_includeFont) {
                log.addLog("Changing theme to: Theme ID " + _themeId + " (w/ font)");
            } else {
                log.addLog("Changing theme to: Theme ID " + _themeId + " (w/o font)");
            }

            if (_themeId < 1000) { //Custom themes start at ID 1000
                //Preset themen because selected id is < 1000

                //Apply the selected theme information to the UI
                this.Background = themeCollection[_themeId].bgBrush;
                MainRtb.Foreground = themeCollection[_themeId].textBrush;
                if (themeCollection[_themeId].hasImage) {
                    MainRtb.Background = themeCollection[_themeId].imgBrush;
                } else {
                    MainRtb.Background = themeCollection[_themeId].solidBrush;
                }
                MainMenu.Background = themeCollection[_themeId].menuBrush;
                RtfMenu.Background = themeCollection[_themeId].menuBrush;
                RightMenu.Background = themeCollection[_themeId].menuBrush;
                MainStatus.Background = themeCollection[_themeId].statusBrush;

                if (_includeFont) {
                    //Use the font associated with the preset theme
                    if (appSettings.rememberFontUpDn) {
                        //Use the remembered custom font size
                        setFont(themeCollection[_themeId].fontFamily, (short)appSettings.fontSize, themeCollection[_themeId].fontWeight, themeCollection[_themeId].fontStyle);
                    } else {
                        //Use the default theme font size
                        setFont(themeCollection[_themeId].fontFamily, themeCollection[_themeId].fontSize, themeCollection[_themeId].fontWeight, themeCollection[_themeId].fontStyle);
                    }
                } else {
                    //Use appsettings font
                    setFont(appSettings.fontFamily, (short)appSettings.fontSize, appSettings.fontWeight, appSettings.fontStyle);
                }
            } else {
                //Custom theme because selected id is >= 1000
                KuroNoteCustomTheme selectedCustomTheme = null;
                
                //Get the custom theme file specified by its themeId
                try {
                    using (StreamReader sr = new StreamReader(customThemePath + _themeId + CUSTOM_THEME_EXT)) {
                        string json = sr.ReadToEnd();
                        KuroNoteCustomTheme kntFile = JsonConvert.DeserializeObject<KuroNoteCustomTheme>(json);
                        selectedCustomTheme = kntFile;
                    }
                } catch (Exception e) {
                    //unable to read custom theme, it might have been deleted
                    log.addLog(e.ToString());

                    //setting a default theme instead
                    appSettings.themeId = DEFAULT_THEME_ID;
                    appSettings.UpdateSettings();
                    setTheme(appSettings.themeId, true);
                }

                if (selectedCustomTheme != null) {

                    Stretch stretchMode;
                    if (appSettings.stretchImages) {
                        stretchMode = Stretch.Fill;
                    } else {
                        stretchMode = Stretch.UniformToFill;
                    }

                    //in JSON format, colour values are stored as hex, convert them to ARGB so we can create new SolidColorBrush instances
                    byte[] bgBrushArgb = getARGBFromHex(selectedCustomTheme.bgBrush.ToString());
                    byte[] solidBrushArgb = getARGBFromHex(selectedCustomTheme.solidBrush.ToString());
                    byte[] menuBrushArgb = getARGBFromHex(selectedCustomTheme.menuBrush.ToString());
                    byte[] statusBrushArgb = getARGBFromHex(selectedCustomTheme.statusBrush.ToString());
                    byte[] textBrushArgb = getARGBFromHex(selectedCustomTheme.textBrush.ToString());

                    //Apply the selected theme information to the UI
                    this.Background = new SolidColorBrush(Color.FromArgb(bgBrushArgb[0], bgBrushArgb[1], bgBrushArgb[2], bgBrushArgb[3]));
                    MainRtb.Foreground = new SolidColorBrush(Color.FromArgb(textBrushArgb[0], textBrushArgb[1], textBrushArgb[2], textBrushArgb[3]));
                    if (selectedCustomTheme.hasImage) {
                        try {
                            MainRtb.Background = new ImageBrush
                            {
                                ImageSource = new BitmapImage(new Uri(customThemePath + selectedCustomTheme.themeId + INTERNAL_IMAGE_EXT, UriKind.Absolute)),
                                Opacity = selectedCustomTheme.imgBrushOpacity,
                                Stretch = stretchMode
                            };
                        } catch (Exception e) {
                            log.addLog("Custom theme has an image but the internal image file could not be accessed, it might be not set");
                            log.addLog(e.ToString());
                            //Apply the solid brush instead
                            MainRtb.Background = new SolidColorBrush(Color.FromArgb(solidBrushArgb[0], solidBrushArgb[1], solidBrushArgb[2], solidBrushArgb[3]));
                        }
                    } else {
                        MainRtb.Background = new SolidColorBrush(Color.FromArgb(solidBrushArgb[0], solidBrushArgb[1], solidBrushArgb[2], solidBrushArgb[3]));
                    }
                    MainMenu.Background = new SolidColorBrush(Color.FromArgb(menuBrushArgb[0], menuBrushArgb[1], menuBrushArgb[2], menuBrushArgb[3]));
                    RtfMenu.Background = new SolidColorBrush(Color.FromArgb(menuBrushArgb[0], menuBrushArgb[1], menuBrushArgb[2], menuBrushArgb[3]));
                    RightMenu.Background = new SolidColorBrush(Color.FromArgb(menuBrushArgb[0], menuBrushArgb[1], menuBrushArgb[2], menuBrushArgb[3]));
                    MainStatus.Background = new SolidColorBrush(Color.FromArgb(statusBrushArgb[0], statusBrushArgb[1], statusBrushArgb[2], statusBrushArgb[3]));


                    if (_includeFont) {
                        //Use the font associated with the custom theme
                        if (appSettings.rememberFontUpDn) {
                            //Use the remembered custom font size
                            setFont(selectedCustomTheme.fontFamily, (short)appSettings.fontSize, selectedCustomTheme.fontWeight, selectedCustomTheme.fontStyle);
                        } else {
                            //Use the default theme font size
                            setFont(selectedCustomTheme.fontFamily, selectedCustomTheme.fontSize, selectedCustomTheme.fontWeight, selectedCustomTheme.fontStyle);
                        }
                    } else {
                        //Use appsettings font
                        setFont(appSettings.fontFamily, (short)appSettings.fontSize, appSettings.fontWeight, appSettings.fontStyle);
                    }
                }
            }
        }

        /// <summary>
        /// Menu > Fullscreen
        /// </summary>
        private void Fullscreen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Fullscreen");
            toggleFullscreen();
        }

        /// <summary>
        /// Detects whether or not the application is currently fullscreen
        /// If not, enters fullscreen mode
        /// else exits fullscreen mode
        /// </summary>
        private void toggleFullscreen()
        {
            if(this.WindowStyle == WindowStyle.SingleBorderWindow)
            {
                //Enter Fullscreen Mode
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                //change fullscreen icon
                string exitFullscreenIconUri = "pack://application:,,,/img/icons/outline_fullscreen_exit_black_18dp.png";
                imgFullscreenIcon.Source = new BitmapImage(new Uri(exitFullscreenIconUri));
                //change fullscreen tooltip
                FullscreenMi.ToolTip = EnUIDict["FullscreenMiTT1"];

                if (appSettings.gamification) {
                    appSettings.achFullscreens++;
                    appSettings.UpdateSettings();

                    switch (appSettings.achFullscreens)
                    {
                        case 10:
                            unlockAchievement(17);
                            break;
                        case 50:
                            unlockAchievement(18);
                            break;
                        case 500:
                            unlockAchievement(19);
                            break;
                    }
                }

            } else {
                //Exit Fullscreen Mode
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                //change fullscreen icon
                string enterFullscreenIconUri = "pack://application:,,,/img/icons/outline_fullscreen_black_18dp.png";
                imgFullscreenIcon.Source = new BitmapImage(new Uri(enterFullscreenIconUri));
                //change fullscreen tooltip
                FullscreenMi.ToolTip = EnUIDict["FullscreenMiTT0"];
            }
        }

        #region RtfMenu
        /// <summary>
        /// RtfMenu > Bold (RibbonToggleButton)
        /// </summary>
        private void RtfBold_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfBold");
            EditingCommands.ToggleBold.Execute(null, MainRtb);
        }

        /// <summary>
        /// RtfMenu > Italic (RibbonToggleButton)
        /// </summary>
        private void RtfItalic_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfItalic");
            EditingCommands.ToggleItalic.Execute(null, MainRtb);
        }

        /// <summary>
        /// Lessons learned thanks to https://stackoverflow.com/users/6630084/victor
        /// FlowDocuments don't permenantly save TextDecorations in Inlines - this is evident when examining FlowDocuments after being saved, closed and reopened.
        /// TextDecorations are instead often stored in the parents of Inlines (i.e. Spans/Paragraphs).
        /// 
        /// This method retrieves the TextDecorationCollection for the current Inline.
        /// If empty, it returns the value for its parent, failing that, its grandparent.
        /// If there are no TextDecorations, it will return an empty TDC.
        /// </summary>
        /// <param name="rtb">MainRtb</param>
        /// <returns></returns>
        private TextDecorationCollection getTextDecorationCollection(RichTextBox rtb)
        {
            TextDecorationCollection decors = rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            if (decors == null || decors.Count == 0)
            {
                if (rtb.Selection.Start.Parent is Run run)
                {
                    if (run.Parent is Span span)
                    {
                        decors = span.TextDecorations;
                    }
                    else if (run.Parent is Paragraph para)
                    {
                        decors = para.TextDecorations;
                    }
                }
            }

            if (decors is TextDecorationCollection tdc) {
                return tdc;
            } else {
                return null;
            }
        }

        /// <summary>
        /// RtfMenu > Underline (RibbonToggleButton)
        /// 
        /// KNOWN MINOR BUG: After saving, closing and reopening an RTF file with underlines,
        /// the first ToggleUnderline operation to a previously underlined inline will appear to do nothing
        /// because the ToggleUnderline method always goes "Apply, Remove, Apply, Remove" and there
        /// doesn't appear to be a way to force an apply/remove on the first toggle.
        /// This is at least better than the alternative of correct first time response but applying it
        /// to the wrong element entirely (i.e. entire span/paragraph while attempting to edit a small inline).
        /// </summary>
        private void RtfUnderline_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfUnderline");
            EditingCommands.ToggleUnderline.Execute(null, MainRtb);

            //After operation, it is now underlined or not? - fixes synchronisation issues caused by minor bug
            if (getTextDecorationCollection(MainRtb).Count > 0) {
                rtfUnderlineRibbon.IsChecked = true;
            } else {
                rtfUnderlineRibbon.IsChecked = false;
            }
        }

        /// <summary>
        /// RtfMenu > Font Up
        /// </summary>
        private void RtfFontUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfFontUp");
            EditingCommands.IncreaseFontSize.Execute(null, MainRtb);
            updateRtfFontSizeCmb();
        }

        /// <summary>
        /// RtfMenu > Font Down
        /// </summary>
        private void RtfFontDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfFontDown");
            EditingCommands.DecreaseFontSize.Execute(null, MainRtb);
            updateRtfFontSizeCmb();
        }

        /// <summary>
        /// RtfMenu > Apply Colour
        /// </summary>
        private void RtfApplyColour_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfApplyColour: " + rtfApplySelectedColourRect.Fill.ToString());
            MainRtb.Selection.ApplyPropertyValue(ForegroundProperty, rtfApplySelectedColourRect.Fill);

            //Auto-focus - the user can continue typing after changing font colour without having to re-select MainRtb
            MainRtb.Focus();
            MainRtb.Selection.Select(MainRtb.Selection.Start, MainRtb.Selection.End);
        }

        /// <summary>
        /// RtfMenu > Choose Colour
        /// </summary>
        private void RtfChooseColour_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfChooseColour");
            Color outputColor;
            bool newColorPicked = ColorPickerWPF.ColorPickerWindow.ShowDialog(out outputColor);
            if (newColorPicked) {
                //new color was chosen (i.e. user didn't just close the dialog)
                rtfApplySelectedColourRect.Fill = new SolidColorBrush(outputColor);
                log.addLog("New RTF font colour picked: " + outputColor.ToString());
                if (MainRtb.Selection.Text.Length > 0) {
                    //If the user had selected text before selecting a colour, automatically apply the colour
                    log.addLog("Request: RtfApplyColour: " + rtfApplySelectedColourRect.Fill.ToString());
                    MainRtb.Selection.ApplyPropertyValue(ForegroundProperty, rtfApplySelectedColourRect.Fill);
                }
            }
        }

        /// <summary>
        /// RtfMenu > Left Align (RibbonToggleButton)
        /// </summary>
        private void RtfLeftAlign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfLeftAlign");
            EditingCommands.AlignLeft.Execute(null, MainRtb);
            updateAlignmentRibbons(false);
        }

        /// <summary>
        /// RtfMenu > Center Align (RibbonToggleButton)
        /// </summary>
        private void RtfCenterAlign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfCenterAlign");
            EditingCommands.AlignCenter.Execute(null, MainRtb);
            updateAlignmentRibbons(false);
        }

        /// <summary>
        /// RtfMenu > Right Align (RibbonToggleButton)
        /// </summary>
        private void RtfRightAlign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfRightAlign");
            EditingCommands.AlignRight.Execute(null, MainRtb);
            updateAlignmentRibbons(false);
        }

        /// <summary>
        /// RtfMenu > Justify Align (RibbonToggleButton)
        /// </summary>
        private void RtfJustifyAlign_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: RtfJustifyAlign");
            EditingCommands.AlignJustify.Execute(null, MainRtb);
            updateAlignmentRibbons(false);
        }
        #endregion

        /// <summary>
        /// Menu > Options > Options...
        /// </summary>
        private void Options_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Options...");
            OptionsDialog optionsDialog = new OptionsDialog(this, appSettings, log);
            optionsDialog.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Options > My Profile...
        /// NOTE: this option will be disabled in the UI if KuroNote launches with gamification = false
        /// </summary>
        private void Profile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: My Profile...");
            ProfileDialog profileDialog = new ProfileDialog(this, appSettings, rankCollection, EnAchDict, log);
            profileDialog.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Options > Theme...
        /// </summary>
        private void Theme_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Theme...");
            ThemeSelector themeSelector = new ThemeSelector(this, appSettings, themeCollection, log);
            themeSelector.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Options > Custom Themes...
        /// </summary>
        private void CustomThemes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Custom Themes...");
            CustomThemeManager customThemeManager = new CustomThemeManager(this, appSettings, log);
            customThemeManager.toggleVisibility(true);
            purgeOrphanedThemeImages(); //doesn't need to run on every startup, but cleanup the custom themes folder
            //when the custom themes manager opens
        }
        #endregion

        /// <summary>
        /// Menu > Options > Logging > Show Log...
        /// NOTE: this option will be disabled in the UI if KuroNote launches with gamification = false
        /// </summary>
        private void ShowLog_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Show Log");
            log.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Options > Logging > Show Log Files...
        /// </summary>
        private void ShowLogFiles_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.showLogFiles();
        }

        /// <summary>
        /// Menu > Options > Check for Updates...
        /// </summary>
        private void Updates_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Updates");
            UpdatesDialog updatesDialog = new UpdatesDialog(this, log);
            updatesDialog.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Options > About KuroNote...
        /// </summary>
        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: About");
            AboutDialog aboutDialog = new AboutDialog(this, log);
            aboutDialog.toggleVisibility(true);
        }

        /// <summary>
        /// StatusBar > Safe to Exit/Unsaved Changes
        /// </summary>
        private void SaveStatusItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (editedFlag) {
                MessageBox.Show("Some changes have been made to this file.\n" + appName + " will offer to save these changes when you close the file.", SaveStatusTb.Text, MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                MessageBox.Show("No changes have been made to this file.\nYou can safely close the file at any time.", SaveStatusTb.Text, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// StatusBar > X Words
        /// </summary>
        private void WordCountItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int wordCount = getWordCount();
            int charCountNoSpaces = getCharacterCount(false);
            int charCountSpaces = getCharacterCount(true);

            float averageCharPerWord;
            if (wordCount == 0 || charCountNoSpaces == 0) {
                //prevent divide-by-0
                averageCharPerWord = 0;
            } else {
                averageCharPerWord = (float)charCountNoSpaces / (float)wordCount;
            }

            string wordString, charStringSpaces, charStringNoSpaces;

            if (wordCount == 1) {
                wordString = wordCount + " word\n";
            } else {
                wordString = wordCount + " words\n";
            }

            if (charCountNoSpaces == 1) {
                charStringNoSpaces = charCountNoSpaces + " character\n";
            } else {
                charStringNoSpaces = charCountNoSpaces + " characters\n";
            }

            if (charCountSpaces == 1) {
                charStringSpaces = charCountSpaces + " character (including spaces)\n";
            } else {
                charStringSpaces = charCountSpaces + " characters (including spaces)\n";
            }

            MessageBox.Show(
                wordString + charStringNoSpaces + charStringSpaces + Math.Round(averageCharPerWord, 2) + " average characters per word",
                "Word Count", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Calculates the number of words in MainRtb
        /// </summary>
        /// <returns>The number of words in MainRtb</returns>
        private int getWordCount()
        {
            TextRange document = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            MatchCollection words = Regex.Matches(document.Text, @"\S+");
            return words.Count;
        }

        /// <summary>
        /// Calculates the number of characters in MainRtb (with or without spaces)
        /// </summary>
        /// <param name="includeSpaces">True if you want to include spaces in the count, false otherwise</param>
        /// <returns>The number of characters in MainRtb</returns>
        private int getCharacterCount(bool includeSpaces)
        {
            TextRange document = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            int characterCount;

            if (includeSpaces) {
                characterCount = document.Text.Length - 2; //does not include the "start" and "end" document characters
            } else {
                characterCount = document.Text.Replace(" ", "").Length - 2; //does not include the "start" and "end" characters
            }

            if (characterCount < 0) {
                return 0;
            } else {
                return characterCount;
            }
        }

        /// <summary>
        /// If word wrap is enabled: ensures page width is automatically determined (NaN)
        /// else: automatically adjusts the page width (and horizontal scrollbar) to match the width of the content
        /// </summary>
        private void processPageWidth()
        {
            try {
                if (appSettings.wordWrap) {
                    //Word wrap enabled - determine page width automatically
                    MainRtb.Document.PageWidth = double.NaN;
                } else {
                    //Word wrap disabled - determine ideal page width
                    try {
                        //Attempt to set page width according to content width (i.e. make the horizontal scrollbar always appropriate)
                        FormattedText rtbText = new FormattedText(
                            new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd).Text,
                            CultureInfo.GetCultureInfo("en-us"), //Can plug a localisation setting string here, else use "CultureInfo.InvariantCulture"
                            FlowDirection.LeftToRight,
                            new Typeface(MainRtb.FontFamily.ToString()),
                            MainRtb.FontSize,
                            MainRtb.Foreground,
                            VisualTreeHelper.GetDpi(this).PixelsPerDip);
                        MainRtb.Document.PageWidth = rtbText.WidthIncludingTrailingWhitespace + PAGE_WIDTH_RIGHT_MARGIN;
                    } catch (ArgumentException) {
                        //Attempted to set PageWidth to an invalid value (e.g. >1000000)
                        MainRtb.Document.PageWidth = PAGE_WIDTH_MAX;
                    } catch (Exception ue) {
                        //Unknown error
                        log.addLog("ERROR: Failed to process page width: " + ue.ToString());
                    }
                }
            } catch (NullReferenceException) {
                Debug.WriteLine("WARN: Attempted to access appSettings before initialisation ");
            }
        }

        /// <summary>
        /// Converts hex colour value strings to ARGB numbers
        /// </summary>
        /// <param name="colorHexIncludingHash">hex "#AARRGGBB" string value to convert to ARGB byte array</param>
        /// <returns>ARGB numbers in an array of bytes</returns>
        public byte[] getARGBFromHex(string colorHexIncludingHash)
        {
            byte[] argbValues = new byte[4];
            argbValues[0] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(1, 2), 16);
            argbValues[1] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(3, 2), 16);
            argbValues[2] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(5, 2), 16);
            argbValues[3] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(7, 2), 16);
            return argbValues;
        }

        /// <summary>
        /// When the mouse wheel moves while the cursor is in MainRtb
        /// </summary>
        private void MainRtb_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Check for CTRL modifier
            //The full keybinding to do stuff is CTRL+MWheelUp/Down
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                if (e.Delta < 0) {
                    doFontDown();
                } else {
                    doFontUp();
                }
            }
        }

        /// <summary>
        /// When an item is dragged into/over MainRtb
        /// </summary>
        private void MainRtb_PreviewDragEnterOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// When a dragged item is dropped into MainRtb
        /// </summary>
        private void MainRtb_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {  //If the item is a file (or multiple files)...                      
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 1) {                        //If there is only 1 file...
                    log.addLog("Request: Open dropped file");
                    doOpen(true, files[0]);
                } else {
                    log.addLog("WARNING: Attempted to drop multiple files");
                }
            }
        }

        /// <summary>
        /// Updates the UI to ensure that rtfFontSizeCmb displays the (Math.Rounded) font size of the current selection,
        /// regardless of whether or not that font size is in RTF_FONT_SIZES.
        /// NOTE: When a selection has multiple font sizes, it will display nothing.
        /// </summary>
        private void updateRtfFontSizeCmb()
        {
            var fontSizeProperty = MainRtb.Selection.GetPropertyValue(FontSizeProperty);
            if (fontSizeProperty == DependencyProperty.UnsetValue) {
                //multiple font sizes in selection
                try {
                    //Try to select ComboBoxItem "", if it doesn't exist, create it first
                    rtfFontSizeCmb.SelectedValue = String.Empty;
                } catch (FormatException) {
                    ComboBoxItem currentFontItem = new ComboBoxItem();
                    currentFontItem.Content = String.Empty;
                    rtfFontSizeCmb.Items.Insert(0, currentFontItem);
                    rtfFontSizeCmb.SelectedValue = String.Empty;
                }
            } else {

                //If the first ComboBoxItem is not the same as the first RTF_FONT_SIZE (or is String.Empty), delete it
                string firstDefaultFontSize = RTF_FONT_SIZES[0].ToString();
                ComboBoxItem firstComboBoxItem = (ComboBoxItem)rtfFontSizeCmb.Items[0];
                if (!firstComboBoxItem.Content.ToString().Equals(firstDefaultFontSize)) {
                    rtfFontSizeCmb.Items.RemoveAt(0);
                    //removed temporary font size item
                }

                //If the font size matches a default font size, select it, otherwise, create a temporary ComboBoxItem and select that
                double fontSizePropertyResolved = Math.Round((double)fontSizeProperty);
                short fontSizePropertyShort;
                try {
                    fontSizePropertyShort = (short)fontSizePropertyResolved;

                    if (Array.IndexOf(RTF_FONT_SIZES, fontSizePropertyShort) >= 0) {
                        //This font size is a default font size - select existing ComboBoxItem
                        rtfFontSizeCmb.SelectedValue = fontSizePropertyResolved;
                        //font size was found
                    } else {
                        //This font size is a custom font size - use a temporary ComboBoxItem
                        ComboBoxItem currentFontItem = new ComboBoxItem();
                        currentFontItem.Content = fontSizePropertyResolved;
                        rtfFontSizeCmb.Items.Insert(0, currentFontItem);
                        rtfFontSizeCmb.SelectedIndex = 0;
                        //custom font size item was created
                    }
                } catch (Exception) {
                    //if it cannot be converted to a short, it's obviously not a default font size
                    ComboBoxItem currentFontItem = new ComboBoxItem();
                    currentFontItem.Content = fontSizePropertyResolved;
                    rtfFontSizeCmb.Items.Insert(0, currentFontItem);
                    rtfFontSizeCmb.SelectedIndex = 0;
                    //custom font size item was created
                }
            }
        }

        /// <summary>
        /// Updates the UI to ensure that rtfFontFamilyCmb displays the font family of the current selection,
        /// regardless of whether or not that font size is in Fonts.SystemFontFamilies.
        /// NOTE: When a selection has multiple font families, it will display nothing.
        /// </summary>
        private void updateRtfFontFamilyCmb()
        {
            var fontFamilyProperty = MainRtb.Selection.GetPropertyValue(FontFamilyProperty);
            if (fontFamilyProperty == DependencyProperty.UnsetValue) {
                //multiple font families in selection
                try {
                    //Try to select ComboBoxItem "", if it doesn't exist, create it first
                    rtfFontFamilyCmb.SelectedValue = String.Empty;
                } catch (FormatException) {
                    ComboBoxItem currentFontItem = new ComboBoxItem();
                    currentFontItem.Content = String.Empty;
                    rtfFontFamilyCmb.Items.Insert(0, currentFontItem);
                    rtfFontFamilyCmb.SelectedValue = String.Empty;
                }
            } else {

                //If the first ComboBoxItem is String.Empty, delete it
                ComboBoxItem firstComboBoxItem = (ComboBoxItem)rtfFontFamilyCmb.Items[0];
                if (firstComboBoxItem.Content.ToString().Length < 1) {
                    rtfFontFamilyCmb.Items.RemoveAt(0);
                    //removed temporary font size item
                }

                //If the font family matches an existing font family, select it, otherwise, create a temporary ComboBoxItem and select that
                try {
                    //Try to select a ComboBoxItem with a value of fontFamilyProperty, if it doesn't exist, create it first
                    rtfFontFamilyCmb.SelectedValue = fontFamilyProperty;
                } catch (FormatException) {
                    //This should not happen because all fonts in the system array get added to the rtfFontFamilyCmb dropdown
                    log.addLog("WARN: FontFamily \"" + fontFamilyProperty + "\" was not found in Fonts.SystemFontFamilies - creating temp item");
                    ComboBoxItem currentFontItem = new ComboBoxItem();
                    currentFontItem.Content = fontFamilyProperty;
                    rtfFontFamilyCmb.Items.Insert(0, currentFontItem);
                    rtfFontFamilyCmb.SelectedValue = fontFamilyProperty;
                }
            }
        }

        /// <summary>
        /// Updates the UI to ensure that only the ribbontogglebutton
        /// corresponding to the current alignment is checked
        /// </summary>
        private void updateAlignmentRibbons(bool uncheckAll)
        {
            if (uncheckAll) {
                rtfLeftAlignRibbon.IsChecked = false;
                rtfCenterAlignRibbon.IsChecked = false;
                rtfRightAlignRibbon.IsChecked = false;
                rtfJustifyAlignRibbon.IsChecked = false;
            } else {
                if (MainRtb.Selection.GetPropertyValue(Block.TextAlignmentProperty).Equals(TextAlignment.Left)) {
                    rtfLeftAlignRibbon.IsChecked = true;
                    rtfCenterAlignRibbon.IsChecked = false;
                    rtfRightAlignRibbon.IsChecked = false;
                    rtfJustifyAlignRibbon.IsChecked = false;
                } else if (MainRtb.Selection.GetPropertyValue(Block.TextAlignmentProperty).Equals(TextAlignment.Center)) {
                    rtfLeftAlignRibbon.IsChecked = false;
                    rtfCenterAlignRibbon.IsChecked = true;
                    rtfRightAlignRibbon.IsChecked = false;
                    rtfJustifyAlignRibbon.IsChecked = false;
                } else if (MainRtb.Selection.GetPropertyValue(Block.TextAlignmentProperty).Equals(TextAlignment.Right)) {
                    rtfLeftAlignRibbon.IsChecked = false;
                    rtfCenterAlignRibbon.IsChecked = false;
                    rtfRightAlignRibbon.IsChecked = true;
                    rtfJustifyAlignRibbon.IsChecked = false;
                } else {
                    rtfLeftAlignRibbon.IsChecked = false;
                    rtfCenterAlignRibbon.IsChecked = false;
                    rtfRightAlignRibbon.IsChecked = false;
                    rtfJustifyAlignRibbon.IsChecked = true;
                }
            }
        }
        
        /// <summary>
        /// When the RTB caret moves
        /// Update RTFMenu if RTFMode
        /// </summary>
        private void MainRtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //If RTFMode and has a selection
            if (appSettings.rtfMode && MainRtb.Selection != null) {

                //Bold
                var fontWeightProperty = MainRtb.Selection.GetPropertyValue(FontWeightProperty);
                if (fontWeightProperty == DependencyProperty.UnsetValue) {
                    //multiple font weights in selection
                    rtfBoldRibbon.IsChecked = false;
                } else {
                    if (fontWeightProperty.Equals(FontWeights.Bold)) {
                        //selection is all bold
                        rtfBoldRibbon.IsChecked = true;
                    } else {
                        //selection is all not bold
                        rtfBoldRibbon.IsChecked = false;
                    }
                }

                //Italic
                var fontStyleProperty = MainRtb.Selection.GetPropertyValue(FontStyleProperty);
                if (fontStyleProperty == DependencyProperty.UnsetValue) {
                    //multiple font styles in selection
                    rtfItalicRibbon.IsChecked = false;
                } else {
                    if (fontStyleProperty.Equals(FontStyles.Italic)) {
                        //selection is all italic
                        rtfItalicRibbon.IsChecked = true;
                    } else {
                        //selection is all not italic
                        rtfItalicRibbon.IsChecked = false;
                    }
                }

                //Underline
                try {
                    if (getTextDecorationCollection(MainRtb).Count > 0) {
                        rtfUnderlineRibbon.IsChecked = true;
                    } else {
                        rtfUnderlineRibbon.IsChecked = false;
                    }
                } catch (NullReferenceException) {
                    log.addLog("WARN: Empty TextDecorationCollection");
                }

                //Font Size
                updateRtfFontSizeCmb();

                //Font Family
                updateRtfFontFamilyCmb();

                //Align
                if (MainRtb.Selection.Text.Length > 0) {
                    //all ribbons off
                    updateAlignmentRibbons(true);
                } else {
                    //current align ribbon on
                    updateAlignmentRibbons(false);
                }

                //Font Colour (foreground)
                /* Foreground detection is not strictly necessary - rtfApplySelectedColourRect only needs to react when the user
                 * selects a new colour to be applied (rtfChooseColour)
                var fontColorProperty = MainRtb.Selection.GetPropertyValue(ForegroundProperty);
                try {
                    rtfApplySelectedColourRect.Fill = (Brush)fontColorProperty;
                } catch (InvalidCastException) {
                    //Multiple font colours in selection
                    rtfApplySelectedColourRect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                */
            }
        }

        /// <summary>
        /// When the value of the RTF FontFamily ComboBox changes
        /// </summary>
        private void rtfFontFamilyCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                //If the user just selected a new font family, apply it to the current selection
                if (rtfFontFamilyCmb.SelectedValue.ToString().Length > 0 && rtfFontFamilyReadyFlag == true) {
                    MainRtb.Selection.ApplyPropertyValue(FontFamilyProperty, rtfFontFamilyCmb.SelectedValue.ToString());

                    //Auto-focus - the user can continue typing after changing font family without having to re-select MainRtb
                    MainRtb.Focus();
                    MainRtb.Selection.Select(MainRtb.Selection.Start, MainRtb.Selection.End);
                    rtfFontFamilyReadyFlag = false; //a font family was just applied, don't allow another until the drop down is opened again
                }
            } catch (NullReferenceException) {
                Debug.WriteLine("WARN: SelectionChanged Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// When the RTF Font Family ComboBox is opened
        /// </summary>
        private void rtfFontFamilyCmb_DropDownOpened(object sender, EventArgs e)
        {
            rtfFontFamilyCmb.Items.Clear();
            //Populate the rtfFontFamilyCmb drop down with all the installed system fonts
            foreach (FontFamily ff in Fonts.SystemFontFamilies) {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = ff.Source;
                item.FontFamily = ff;
                rtfFontFamilyCmb.Items.Add(item);
            }
            rtfFontFamilyReadyFlag = true; //the user opened the font family drop down menu, allow a font family to be applied
        }

        /// <summary>
        /// When the value of the RTF FontSize ComboBox changes
        /// </summary>
        private void rtfFontSizeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                //If the user just selected a new font size, apply it to the current selection
                if (rtfFontSizeCmb.SelectedValue.ToString().Length > 0 && rtfFontSizeReadyFlag == true) {
                    double parsedValue = double.Parse(rtfFontSizeCmb.SelectedValue.ToString());
                    MainRtb.Selection.ApplyPropertyValue(FontSizeProperty, parsedValue);

                    //Auto-focus - the user can continue typing after changing font size without having to re-select MainRtb
                    MainRtb.Focus();
                    MainRtb.Selection.Select(MainRtb.Selection.Start, MainRtb.Selection.End);
                    rtfFontSizeReadyFlag = false; //a font size was just applied, don't allow another until the drop down is opened again
                }
            } catch (NullReferenceException) {
                Debug.WriteLine("WARN: SelectionChanged Event fired before object initialisation ");
            } catch (FormatException) {
                Debug.WriteLine("WARN: Unable to parse rtfFontSizeCmb value");
            }
        }

        /// <summary>
        /// When the RTF FontSize ComboBox is opened
        /// </summary>
        private void rtfFontSizeCmb_DropDownOpened(object sender, EventArgs e)
        {
            //Ensure there are no temporary items created during selection font size detection (updateRtfFontSizeCmb)
            //by resetting the RTF_FONT_SIZES dropdown
            rtfFontSizeCmb.Items.Clear();
            foreach (short size in RTF_FONT_SIZES) {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = size;
                rtfFontSizeCmb.Items.Add(item);
            }
            rtfFontSizeReadyFlag = true; //the user opened the font size drop down menu, allow a font size to be applied
        }

        /// <summary>
        /// When the "Edit" menu is opened
        /// </summary>
        private void EditMi_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            purgeSearchWithMenuItems();
            addSearchWithMenuItems(false);
        }

        /// <summary>
        /// When the RTB is edited
        /// </summary>
        private void MainRtb_TextChanged(object sender, RoutedEventArgs e)
        {
            toggleEdited(true);
            processPageWidth();

            //update StatusBar word count display
            int wordCount = getWordCount();
            string wordString;

            if (wordCount == 1) {
                wordString = wordCount + " word";
            } else {
                wordString = wordCount + " words";
            }
            WordCountTb.Text = wordString;
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!doExit()) {
                e.Cancel = true;  //cancel the exit
            }
        }
    }

    /// <summary>
    /// Commands
    /// </summary>
    public static class CustomCommands
    {
        //File
        public static RoutedCommand New = new RoutedCommand();
        public static RoutedCommand NewRegWin = new RoutedCommand();
        public static RoutedCommand NewAdminWin = new RoutedCommand();
        public static RoutedCommand Open = new RoutedCommand();
        public static RoutedCommand Save = new RoutedCommand();
        public static RoutedCommand SaveAs = new RoutedCommand();
        public static RoutedCommand Print = new RoutedCommand();
        public static RoutedCommand Exit = new RoutedCommand();

        //Edit
        public static RoutedCommand Cut = new RoutedCommand();
        public static RoutedCommand Copy = new RoutedCommand();
        public static RoutedCommand Paste = new RoutedCommand();
        public static RoutedCommand Find = new RoutedCommand();
        public static RoutedCommand Replace = new RoutedCommand();

        //Format
        public static RoutedCommand Font = new RoutedCommand();
        public static RoutedCommand FontUp = new RoutedCommand();
        public static RoutedCommand FontDown = new RoutedCommand();

        //Tools
        public static RoutedCommand AESEnc = new RoutedCommand();
        public static RoutedCommand AESDec = new RoutedCommand();
        public static RoutedCommand SpellcheckDictionaryManager = new RoutedCommand();

        //Fullscreen
        public static RoutedCommand Fullscreen = new RoutedCommand();

        //Options
        public static RoutedCommand Profile = new RoutedCommand();
        public static RoutedCommand Options = new RoutedCommand();
        public static RoutedCommand Theme = new RoutedCommand();
        public static RoutedCommand CustomThemes = new RoutedCommand();
        public static RoutedCommand ShowLog = new RoutedCommand();
        public static RoutedCommand ShowLogFiles = new RoutedCommand();
        public static RoutedCommand Updates = new RoutedCommand();
        public static RoutedCommand About = new RoutedCommand();

        //RTF
        public static RoutedCommand RtfBold = new RoutedCommand();
        public static RoutedCommand RtfItalic = new RoutedCommand();
        public static RoutedCommand RtfUnderline = new RoutedCommand();
        public static RoutedCommand RtfFontUp = new RoutedCommand();
        public static RoutedCommand RtfFontDown = new RoutedCommand();
        public static RoutedCommand RtfApplyColour = new RoutedCommand();
        public static RoutedCommand RtfChooseColour = new RoutedCommand();
        public static RoutedCommand RtfLeftAlign = new RoutedCommand();
        public static RoutedCommand RtfCenterAlign = new RoutedCommand();
        public static RoutedCommand RtfRightAlign = new RoutedCommand();
        public static RoutedCommand RtfJustifyAlign = new RoutedCommand();
    }
}
