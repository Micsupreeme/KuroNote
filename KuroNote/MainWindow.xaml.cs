using System;
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

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// TODO: holiday greetings
    /// TODO: option to toggle between showing full file path in the app title and just the file name
    /// TODO: options to remember window size
    /// TODO: option to change saving/loading encoding
    /// TODO: Drag and drop
    /// 
    /// TODO: check find/replace selecting the 1st occurance of any search term twice before continuing
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        //Constants
        private const string FILE_FILTER =  "Text Documents (*.txt, *.kuro)|*.txt; *.kuro|" +
                                            "KuroNotes (*.kuro)|*.kuro|" +
                                            "All Files (*.*)|*.*";  //For opening and saving files
        private const long FILE_MAX_SIZE = 1048576;                 //Maximum supported file size in bytes (1MB)
        private const string FILE_SEARCH_EXE = "*.exe";
        private const string CUSTOM_THEME_EXT = ".kurotheme";
        private const string INTERNAL_IMAGE_EXT = ".jpg";           //custom theme destination extension is always this
        private const int DEFAULT_THEME_ID = 0;                     //if a custom theme file cannot be accessed, revert back to this theme
        private const double DEFAULT_WINDOW_HEIGHT = 500;
        private const double DEFAULT_WINDOW_WIDTH = 750;

        //Globals
        public string appName = "KuroNote";
        public string appPath = AppDomain.CurrentDomain.BaseDirectory;
        private string customThemePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\CustomThemes\\";
        private KuroNoteSettings appSettings;
        private Log log;
        private string fileName = string.Empty;             //Name of the loaded file - null if no file loaded

        private bool editedFlag = false;                    //Are there any unsaved changes?
        private Encoding selectedEncoding = Encoding.UTF8;  //Encoding for opening and saving files (Encoding.ASCII blocks unicode)

        public KuroNoteTheme[] themeCollection;

        //UI Dictionaries for different languages
        public Dictionary<string, string> EnUIDict;

        //Error Dictionaries for different languages
        public Dictionary<int, string> EnErrMsgDict;
        public Dictionary<int, string> EnErrTitleDict;

        #region Code for reference
        /*
            JpUIDict = new Dictionary<string, object>();
                JpUIDict["NewMi"] = "しんき";
                JpUIDict["OpenMi"] = "あく";
                //etc.
                this.DataContext = JpUIDict;


                new ImageBrush { ImageSource = new BitmapImage(new Uri(appPath + "conf\\custom.jpg", UriKind.Absolute)),
                                     Opacity = 0.33 },
        */
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitialiseSettings();
            InitialiseLog();
            InitialiseUIDictionary();
            InitialiseErrorDictionary();
            InitialiseFont();
            InitialiseThemeCollection();
            InitialiseTheme();
            processCmdLineArgs();
            processImmediateSettings();
            if(appSettings.rememberWindowSize) {
                this.Height = appSettings.windowHeight;
                this.Width = appSettings.windowWidth;
            }
            purgeOrphanedThemeImages();

            toggleEdited(false);
            log.addLog(
                Environment.NewLine + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond + ": " +
                "Ready! Awaiting instructions"
            , true);
            setStatus("Welcome");
            /*
            if(appSettings.gamification) {
                processHolidayGreetings();
                InitialiseGamification();
            }
            */
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
                EnUIDict["NewMiTT"] = "Closes this file and creates a new file.";
            EnUIDict["NewWinMi"] = "New Window";
            EnUIDict["NewRegularWinMi"] = "New Window";
                EnUIDict["NewRegularWinMiTT"] = "Opens a new " + appName + " window.";
            EnUIDict["NewAdminWinMi"] = "New Administrator Window";
                EnUIDict["NewAdminWinMiTT"] = "Opens a new " + appName + " window with administrator rights.";
            EnUIDict["OpenMi"] = "Open...";
                EnUIDict["OpenMiTT"] = "Opens an existing file.";
            EnUIDict["SaveMi"] = "Save";
                EnUIDict["SaveMiTT"] = "Saves over this file if it already exists, otherwise saves this file as a specified file.";
            EnUIDict["SaveAsMi"] = "Save As...";
                EnUIDict["SaveAsMiTT"] = "Saves this file as a specified file.";
            EnUIDict["PrintMi"] = "Print...";
                EnUIDict["PrintMiTT"] = "Prints this file.";
            EnUIDict["ExitMi"] = "Exit";
                EnUIDict["ExitMiTT"] = "Closes " + appName + ".";
            //Edit
            EnUIDict["EditMi"] = "Edit";
            EnUIDict["CutMi"] = "Cut";
                EnUIDict["CutMiTT"] = "Moves any selected text to the clipboard.";
            EnUIDict["CopyMi"] = "Copy";
                EnUIDict["CopyMiTT"] = "Copies any selected text to the clipboard.";
            EnUIDict["PasteMi"] = "Paste";
                EnUIDict["PasteMiTT"] = "Pastes text from the clipboard over any selected text.";
            EnUIDict["UndoMi"] = "Undo";
                EnUIDict["UndoMiTT"] = "Undoes the last change made to this file.";
            EnUIDict["RedoMi"] = "Redo";
                EnUIDict["RedoMiTT"] = "Redoes the last undone change made to this file.";
            EnUIDict["FindMi"] = "Find...";
                EnUIDict["FindMiTT"] = "Searches this file for a specified phrase.";
            EnUIDict["ReplaceMi"] = "Replace...";
                EnUIDict["ReplaceMiTT"] = "Searches this file for a specified phrase and replaces it with another specified phrase.";
            EnUIDict["SelectAllMi"] = "Select All";
                EnUIDict["SelectAllMiTT"] = "Selects all text within this file.";
            //Format
            EnUIDict["FormatMi"] = "Format";
            EnUIDict["FontMi"] = "Font...";
                EnUIDict["FontMiTT"] = "Changes the font to a specified font.";
            //Tools
            EnUIDict["ToolsMi"] = "Tools";
            EnUIDict["AESMi"] = "AES Encryption";
            EnUIDict["AESEncMi"] = "AES Encrypt...";
                EnUIDict["AESEncMiTT"] = "Creates a copy of this file that is encrypted with a specified password by the Advanced Encryption Standard.";
            EnUIDict["AESDecMi"] = "AES Decrypt...";
                EnUIDict["AESDecMiTT"] = "Creates a copy of this file that is decrypted with a specified password by the Advanced Encryption Standard.";
            //Options
            EnUIDict["OptionsMi"] = string.Empty;
            EnUIDict["OptionsDialogMi"] = "Options...";
                EnUIDict["OptionsDialogMiTT"] = "Displays various options that you can change to optimise your experience using " + appName + ".";
            EnUIDict["ThemeMi"] = "Select Theme...";
                EnUIDict["ThemeMiTT"] = "Changes the theme to a specified theme.";
            EnUIDict["CustomThemesMi"] = "Custom Themes...";
                EnUIDict["CustomThemesMiTT"] = "Opens the custom themes manager.";
            EnUIDict["LoggingMi"] = "Logging";
            EnUIDict["ShowLogMi"] = "Show Log...";
                EnUIDict["ShowLogMiTT"] = "Opens the log for the current session.";
            EnUIDict["ShowLogFilesMi"] = "Show Log Files...";
                EnUIDict["ShowLogFilesMiTT"] = "Opens the directory where " + appName + " log files are stored.";
            EnUIDict["AboutMi"] = "About " + appName;
                EnUIDict["AboutMiTT"] = "Displays information about " + appName + ".";
            this.DataContext = EnUIDict;
        }

        /// <summary>
        /// Defines error message strings
        /// </summary>
        private void InitialiseErrorDictionary ()
        {
            EnErrMsgDict = new Dictionary<int, string>();
            EnErrTitleDict = new Dictionary<int, string>();

            EnErrMsgDict[1] =  appName + " is not designed to load files larger than " + FILE_MAX_SIZE + " B. " +
                "Files of this size may significantly compromise performance. " +
                "Are you sure you want to open this file which exceeds the limit?";
            EnErrTitleDict[1] = "File size exceeds limit";

            EnErrMsgDict[2] = "There are unsaved changes. Would you like to save this file before exiting?";
            EnErrTitleDict[2] = "Save before exit?";

            EnErrMsgDict[3] = "There are unsaved changes. Would you like to save this file before opening a new one?";
            EnErrTitleDict[3] = "Save before open?";

            EnErrMsgDict[4] = "There are unsaved changes. Would you like to save this file before creating a new one?";
            EnErrTitleDict[4] = "Save before new?";
        }

        /// <summary>
        /// Retrieves the specified error message and error message title
        /// </summary>
        /// <param name="_errorCode"></param>
        /// <returns>An array containing the error message [0], and the error message title [1]</returns>
        private string[] getErrorMessage(int _errorCode)
        {
            string[] errorMessage = new string[] {EnErrMsgDict[_errorCode], EnErrTitleDict[_errorCode]};
            return errorMessage;
        }

        /// <summary>
        /// Starts retrieving settings
        /// </summary>
        private void InitialiseSettings()
        {
            appSettings = new KuroNoteSettings(log);
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
            } else {
                ShowLogMi.IsEnabled = false; //No point in showing the log if we aren't logging
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
                doOpen(args[1]);
            }
        }

        /// <summary>
        /// Process and apply settings that can take effect immediately (i.e. without restart)
        /// </summary>
        public void processImmediateSettings()
        {
            //Float above other windows
            if (appSettings.floating) {
                this.Topmost = true;
            } else {
                this.Topmost = false;
            }
        }

        /// <summary>
        /// Check for images in the custom themes directory that don't have associated .kurotheme files and delete them
        /// </summary>
        private void purgeOrphanedThemeImages()
        {
            string[] customThemeFiles = Directory.GetFiles(customThemePath);

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
            //Declare array of themes
            themeCollection = new KuroNoteTheme[]
            {
                new KuroNoteTheme
                (
                    0, "Spectrum", "Image by Gradienta on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-gradienta-6985193.jpg", UriKind.Absolute)),
                                     Opacity = 0.5 },
                    new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                    new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    1, "Spectrum II", "Image by Gradienta on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-gradienta-6985045.jpg")),
                                     Opacity = 0.44 },
                    new SolidColorBrush(Color.FromRgb(254, 255, 232)),
                    new SolidColorBrush(Color.FromRgb(204, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    2, "Eternal", "Image by Skyler Ewing", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-skyler-ewing-5748311.jpg", UriKind.Absolute)),
                                     Opacity = 0.42 },
                    new SolidColorBrush(Color.FromRgb(244, 239, 235)),
                    new SolidColorBrush(Color.FromRgb(244, 239, 235)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Palatino Linotype", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    3, "Boundless Sky", "Image by Pixabay on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-258149.jpg")),
                                     Opacity = 0.28 },
                    new SolidColorBrush(Color.FromRgb(184, 215, 237)),
                    new SolidColorBrush(Color.FromRgb(229, 229, 230)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    4, "Overly Orangey", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4022107.jpg")),
                                     Opacity = 0.27 },
                    new SolidColorBrush(Color.FromRgb(249, 218, 186)),
                    new SolidColorBrush(Color.FromRgb(249, 218, 186)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    5, "Sunset Ripples", "Image by Ben Mack on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-ben-mack-5326909.jpg")),
                                     Opacity = 0.33 },
                    new SolidColorBrush(Color.FromRgb(242, 234, 234)),
                    new SolidColorBrush(Color.FromRgb(242, 234, 234)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    6, "Spotless Snow", "Image by Pixabay on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-60561.jpg")),
                                     Opacity = 0.36 },
                    new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    7, "Sakura", "Image by Antonio Janeski on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-antonio-janeski-cherry blossoms-4052701.jpg")),
                                     Opacity = 0.29 },
                    new SolidColorBrush(Color.FromRgb(231, 216, 240)),
                    new SolidColorBrush(Color.FromRgb(231, 216, 240)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    8, "Leafy Green", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4046687.jpg")),
                                     Opacity = 0.34 },
                    new SolidColorBrush(Color.FromRgb(208, 203, 169)),
                    new SolidColorBrush(Color.FromRgb(208, 203, 169)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    9, "Paradise Found", "Image by Asad Photo Maldives on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-asad-photo-maldives-3320516.jpg")),
                                     Opacity = 0.29 },
                    new SolidColorBrush(Color.FromRgb(201, 228, 255)),
                    new SolidColorBrush(Color.FromRgb(254, 254, 254)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    10, "Layers of Time", "Image by Fillipe Gomes on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-fillipe-gomes-5611219.jpg")),
                                     Opacity = 0.33 },
                    new SolidColorBrush(Color.FromRgb(255, 244, 231)),
                    new SolidColorBrush(Color.FromRgb(255, 244, 231)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    11, "Bold Gold", "Image by NaMaKuKi on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-namakuki-751374.jpg")),
                                     Opacity = 0.55 },
                    new SolidColorBrush(Color.FromRgb(255, 226, 51)),
                    new SolidColorBrush(Color.FromRgb(255, 226, 51)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    12, "Pleases and Sparkles", "Image by Sharon McCutcheon on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 236, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-sharon-mccutcheon-5922574.jpg")),
                                     Opacity = 0.30 },
                    new SolidColorBrush(Color.FromRgb(255, 216, 255)),
                    new SolidColorBrush(Color.FromRgb(254, 218, 243)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    13, "Abyss", "Distraction-free!", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                    new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                    new SolidColorBrush(Color.FromRgb(249, 249, 249)),
                    new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    14, "Onyx", "Sleepy-eye friendly!", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(15, 15, 15)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    "Verdana", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    15, "Notepad", "Mightier than the sword!", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(255, 255, 210)),
                    new SolidColorBrush(Color.FromRgb(220, 220, 175)),
                    new SolidColorBrush(Color.FromRgb(220, 220, 175)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Comic Sans MS", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    16, "Terminal", "if (this.geek == true) { chosenTheme = themeCollection[16]; }", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                    new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                    "Consolas", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    17, "Antiqua", "Image by Pixabay on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-235985.jpg")),
                                     Opacity = 0.48 },
                    new SolidColorBrush(Color.FromRgb(170, 152, 143)),
                    new SolidColorBrush(Color.FromRgb(170, 152, 143)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Book Antiqua", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    18, "Contour", "Image by David Yu on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-david-yu-2684383.jpg")),
                                     Opacity = 0.39 },
                    new SolidColorBrush(Color.FromRgb(219, 223, 229)),
                    new SolidColorBrush(Color.FromRgb(219, 223, 229)),
                    new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    "Arial", 17, FontWeights.Regular, FontStyles.Normal
                ),
                new KuroNoteTheme
                (
                    19, "Droplets of Hope", "Image by Karolina Grabowska on Pexels", 0,
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new ImageBrush { ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4194853.jpg")),
                                     Opacity = 0.45 },
                    new SolidColorBrush(Color.FromRgb(236, 233, 252)),
                    new SolidColorBrush(Color.FromRgb(236, 233, 252)),
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
        /// Sets the status text in the bottom-left corner to the specified text
        /// </summary>
        /// <param name="_text">The status text to display</param>
        private void setStatus(string _text)
        {
            StatusTb.Text = DateTime.Now.ToShortTimeString() + ": " + _text;
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
        private bool doNew()
        {
            log.addLog("Request: New File");
            if (editedFlag) {
                log.addLog("WARNING: New before saving");
                var res = MessageBox.Show(getErrorMessage(4)[0], getErrorMessage(4)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
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
            setStatus("New File");
            log.addLog("Content deleted");
            return true;
        }

        /// <summary>
        /// Menu > File > New > New File (or CTRL+N)
        /// </summary>
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doNew();
        }

        /// <summary>
        /// Loads a file into the RTB
        /// </summary>
        /// <param name="path">Optional: specify file to open instead of using file open dialog</param>
        /// <returns>True if the operation completed successfully, false otherwise</returns>
        private bool doOpen(string _path = "")
        {
            if (_path.Equals("")) {
                //No file specified - use dialog
                log.addLog("Request: Open");

                if (editedFlag) {
                    log.addLog("WARNING: Open before saving");
                    var res = MessageBox.Show(getErrorMessage(3)[0], getErrorMessage(3)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes || res == MessageBoxResult.Cancel) {
                        log.addLog("Open cancelled");
                        if (res == MessageBoxResult.Yes) {
                            doSave();       //save
                        }
                        return false;   //don't continue with open operation
                    }
                }

                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = FILE_FILTER
                };
                if (dlg.ShowDialog() == true) {
                    MemoryStream ms = new MemoryStream();
                    try {
                        if (fileTooBig(dlg.FileName)) {
                            log.addLog("WARNING: File size exceeds limit");
                            var res = MessageBox.Show(getErrorMessage(1)[0], getErrorMessage(1)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (res == MessageBoxResult.No) {
                                log.addLog("Open cancelled due to file size");
                                return false;
                            }
                        }
                        using (FileStream file = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read)) {
                            byte[] bytes = new byte[file.Length];
                            file.Read(bytes, 0, (int)file.Length);
                            ms.Write(bytes, 0, (int)file.Length);
                        }

                        TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                        range.Text = selectedEncoding.GetString(ms.ToArray());
                        log.addLog("Successfully opened " + dlg.FileName);
                        ms.Close();

                        fileName = dlg.FileName;
                        this.Title = fileName + " - " + appName;
                        toggleEdited(false);
                        setStatus("Opened");
                    } catch (Exception ex) {
                        //File cannot be accessed (e.g. used by another process)
                        log.addLog(ex.ToString());
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            else
            {
                //File specified - open without using a dialog
                log.addLog("Request: Open from cmd - " + _path);
                MemoryStream ms = new MemoryStream();
                try {
                    if (fileTooBig(_path)) {
                        log.addLog("WARNING: File size exceeds limit");
                        var res = MessageBox.Show(getErrorMessage(1)[0], getErrorMessage(1)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.No) {
                            log.addLog("Open cancelled due to file size");
                            return false;
                        }
                    }
                    using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);
                    }

                    TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                    range.Text = selectedEncoding.GetString(ms.ToArray());
                    log.addLog("Successfully opened " + _path);
                    ms.Close();

                    fileName = _path;
                    this.Title = fileName + " - " + appName;
                    toggleEdited(false);
                    setStatus("Opened");
                } catch (Exception ex) {
                    //File cannot be accessed (e.g. used by another process)
                    log.addLog(ex.ToString());
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
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
                        using (FileStream file = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write)) {
                            byte[] bytes = new byte[ms.Length];
                            ms.Read(bytes, 0, (int)ms.Length);
                            file.Write(bytes, 0, bytes.Length);
                            log.addLog("Successfully saved " + fileName);
                            ms.Close();
                        }
                        toggleEdited(false);
                        setStatus("Saved");
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
                Filter = FILE_FILTER
            };
            if (dlg.ShowDialog() == true) {
                TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                MemoryStream ms = new MemoryStream(selectedEncoding.GetBytes(range.Text));

                try {
                    using (FileStream file = new FileStream(dlg.FileName, FileMode.Create, System.IO.FileAccess.Write)) {

                        byte[] bytes = new byte[ms.Length];
                        ms.Read(bytes, 0, (int)ms.Length);
                        file.Write(bytes, 0, bytes.Length);
                        log.addLog("Successfully saved " + dlg.FileName);
                        ms.Close();
                    }

                    fileName = dlg.FileName;
                    this.Title = fileName + " - " + appName;
                    toggleEdited(false);
                    setStatus("Saved");
                } catch (Exception ex) {
                    //File cannot be accessed (e.g. used by another process)
                    log.addLog(ex.ToString());
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Menu > File > Open... (or CTRL+O)
        /// </summary>
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doOpen();
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
        /// 
        /// </summary>
        private void NewRegWin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: New Regular Window");
            string[] executables = getFilesFromDirectory(appPath, FILE_SEARCH_EXE);
            startProcess(executables[0]);
        }

        /// <summary>
        /// 
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
        /// https://stackoverflow.com/users/1137199/dotnet's Simple Print Proccedure
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

                //Use the currently selected font
                log.addLog("Using selected font");
                CloneDoc.FontFamily = new FontFamily(appSettings.fontFamily);
                CloneDoc.FontSize = appSettings.fontSize;
                CloneDoc.FontWeight = appSettings.fontWeight;
                CloneDoc.FontStyle = appSettings.fontStyle;

                IDocumentPaginatorSource idocument = CloneDoc as IDocumentPaginatorSource;

                pd.PrintDocument(idocument.DocumentPaginator, "Printing FlowDocument");
                log.addLog("Successfully printed " + fileName);
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
                var res = MessageBox.Show(getErrorMessage(2)[0], getErrorMessage(2)[1], MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
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
                TextRange cutRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
                string textToCut = cutRange.Text;
                cutRange.Text = String.Empty;
                Clipboard.SetData(DataFormats.UnicodeText, textToCut);
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
                TextRange copyRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
                string textToCopy = copyRange.Text;
                Clipboard.SetData(DataFormats.UnicodeText, textToCopy);
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
                string textToPaste = Clipboard.GetText();
                Clipboard.SetData(DataFormats.UnicodeText, textToPaste);
                //Default paste operation runs
            } catch (Exception e) {
                log.addLog("ERROR: Clipboard error during Paste");
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Captures events before they happen
        /// </summary>
        private void rtbEditor_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut) {
                e.Handled = true; //Disable default Cut operation - use my own
                Cut();
            } else if (e.Command == ApplicationCommands.Copy) {
                e.Handled = true; //Disable default Copy operation - use my own
                Copy();
            } else if (e.Command == ApplicationCommands.Paste) {
                Paste();
                //Allow default Paste operation afterwards
            } else if (e.Command == ApplicationCommands.SelectAll) {
                log.addLog("Request: Select All");
                //Allow default Select All operation
            } else if (e.Command == ApplicationCommands.Undo) {
                log.addLog("Request: Undo");
                //Allow default Undo operation
            } else if (e.Command == ApplicationCommands.Redo) {
                log.addLog("Request: Redo");
                //Allow default Redo operation
            }
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
                MainStatus.Background = themeCollection[_themeId].statusBrush;

                if (_includeFont) {
                    setFont(themeCollection[_themeId].fontFamily, themeCollection[_themeId].fontSize, themeCollection[_themeId].fontWeight, themeCollection[_themeId].fontStyle);
                } else
                {
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
                                Opacity = selectedCustomTheme.imgBrushOpacity
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
                    MainStatus.Background = new SolidColorBrush(Color.FromArgb(statusBrushArgb[0], statusBrushArgb[1], statusBrushArgb[2], statusBrushArgb[3]));

                    if (_includeFont) {
                        setFont(selectedCustomTheme.fontFamily, selectedCustomTheme.fontSize, selectedCustomTheme.fontWeight, selectedCustomTheme.fontStyle);
                    } else {
                        setFont(appSettings.fontFamily, (short)appSettings.fontSize, appSettings.fontWeight, appSettings.fontStyle);
                    }
                }
            }
        }

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
        }
        #endregion

        /// <summary>
        /// Menu > Options > Logging > Show Log...
        /// </summary>
        private void ShowLog_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (appSettings.logging) {
                log.addLog("Request: Show Log");
                log.toggleVisibility(true);
            }
        }

        /// <summary>
        /// Menu > Options > Logging > Show Log Files...
        /// </summary>
        private void ShowLogFiles_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.showLogFiles();
        }

        /// <summary>
        /// Menu > Options > About KuroNote...
        /// </summary>
        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: About");
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
            TextRange document = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            int characterCount = document.Text.Length - 2; //this includes the "start" and "end" characters which most consider to be "not real" characters
            if(characterCount < 0) {
                characterCount = 0;
            }

            MessageBox.Show(generateWordCount() + "\n" + characterCount + " Characters (including spaces)", "Word Count", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Creates a string based on the number of words in the RTB
        /// </summary>
        /// <returns>A string that describes the number of words in the RTB</returns>
        private string generateWordCount()
        {
            TextRange document = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            MatchCollection words = Regex.Matches(document.Text, @"\S+");

            string wordsPost;
            if (words.Count == 1) {
                wordsPost = "Word";
            } else {
                wordsPost = "Words";
            }

            return words.Count + " " + wordsPost;
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
        /// When the RTB caret moves
        /// </summary>
        private void MainRtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //
        }

        /// <summary>
        /// When the RTB is edited
        /// </summary>
        private void MainRtb_TextChanged(object sender, RoutedEventArgs e)
        {
            toggleEdited(true);
            WordCountTb.Text = generateWordCount();
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

        //Tools
        public static RoutedCommand AESEnc = new RoutedCommand();
        public static RoutedCommand AESDec = new RoutedCommand();

        //Options
        public static RoutedCommand Options = new RoutedCommand();
        public static RoutedCommand Theme = new RoutedCommand();
        public static RoutedCommand CustomThemes = new RoutedCommand();
        public static RoutedCommand ShowLog = new RoutedCommand();
        public static RoutedCommand ShowLogFiles = new RoutedCommand();
        public static RoutedCommand About = new RoutedCommand();
    }
}
