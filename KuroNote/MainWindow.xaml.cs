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

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// TODO: Options window
    /// TODO: Replace All
    /// TODO: Indent
    /// TODO: Find - Scroll to selected text (a method that actually works)
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        //Constants
        private const string FILE_FILTER =  "Text Documents (*.txt, *.kuro)|*.txt; *.kuro|" +
                                            "KuroNotes (*.kuro)|*.kuro|" +
                                            "All Files (*.*)|*.*";  //For opening and saving files
        private const long FILE_MAX_SIZE = 1048576; //Maximum supported file size in bytes
        private const string FILE_SEARCH_EXE = "*.exe";

        //Globals
        public string appName = "KuroNote";
        private string appPath = AppDomain.CurrentDomain.BaseDirectory;
        private KuroNoteSettings appSettings;
        private Log log;
        private string fileName = string.Empty;             //Name of the loaded file - null if no file loaded

        private bool editedFlag = false;                    //Are there any unsaved changes?
        private Encoding selectedEncoding = Encoding.UTF8;  //Encoding for opening and saving files (Encoding.ASCII blocks unicode)

        private bool temporaryLogEnabledFlag = true;

        //UI Dictionaries for different languages
        public Dictionary<string, string> EnUIDict;

        //Error Dictionaries for different languages
        public Dictionary<int, string> EnErrMsgDict;
        public Dictionary<int, string> EnErrTitleDict;

        #region Code for reference
        /*
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            MessageBox.Show("Arg: " + arg);
        }
        
        //Example controls
        MainRtb.SpellCheck.IsEnabled = true;

        //MessageBoxResult result = MessageBox.Show("My Message Question", "My Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        Function GenerateWordCount()
            Dim Words As MatchCollection = Regex.Matches(rtb.Text, "\S+")
            txtWc.Text = Words.Count & " Words"
            Return (CInt(Words.Count))
        End Function

            JpUIDict = new Dictionary<string, object>();
                JpUIDict["NewMi"] = "しんき";
                JpUIDict["OpenMi"] = "あく";
                //etc.
                this.DataContext = JpUIDict;

        */
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitialiseLog(); 
            InitialiseSettings();
            InitialiseUIDictionary();
            InitialiseErrorDictionary();
            InitialiseFont();
            InitialiseTheme();
            processCmdLineArgs();

            toggleEdited(false);
            log.addLog(
                Environment.NewLine + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond + ": " +
                "Ready! Awaiting instructions"
            , true);
            setStatus("Welcome");
        }

        /// <summary>
        /// Defines UI strings
        /// </summary>
        private void InitialiseUIDictionary()
        {
            EnUIDict = new Dictionary<string, string>();
            //File
            EnUIDict["FileMi"] = "New";
            EnUIDict["NewMi"] = "New";
            EnUIDict["NewWinMi"] = "New Window";
            EnUIDict["NewRegularWinMi"] = "New Window";
            EnUIDict["NewAdminWinMi"] = "New Administrator Window";
            EnUIDict["OpenMi"] = "Open...";
            EnUIDict["SaveMi"] = "Save";
            EnUIDict["SaveAsMi"] = "Save As...";
            EnUIDict["PrintMi"] = "Print...";
            EnUIDict["ExitMi"] = "Exit";
            //Edit
            EnUIDict["EditMi"] = "Edit";
            EnUIDict["CutMi"] = "Cut";
            EnUIDict["CopyMi"] = "Copy";
            EnUIDict["PasteMi"] = "Paste";
            EnUIDict["UndoMi"] = "Undo";
            EnUIDict["RedoMi"] = "Redo";
            EnUIDict["FindMi"] = "Find...";
            EnUIDict["SelectAllMi"] = "Select All";
            //Format
            EnUIDict["FormatMi"] = "Format";
            EnUIDict["FontMi"] = "Font...";
            //Tools
            EnUIDict["ToolsMi"] = "Tools";
            EnUIDict["AESMi"] = "AES Encryption";
            EnUIDict["AESEncMi"] = "AES Encrypt...";
            EnUIDict["AESDecMi"] = "AES Decrypt...";
            //Options
            EnUIDict["OptionsMi"] = string.Empty;
            EnUIDict["ThemeMi"] = "Theme...";
            EnUIDict["CustomThemesMi"] = "Custom Themes...";
            EnUIDict["LoggingMi"] = "Logging";
            EnUIDict["ShowLogMi"] = "Show Log...";
            EnUIDict["ShowLogFilesMi"] = "Show Log Files...";
            EnUIDict["AboutMi"] = "About " + appName;
            this.DataContext = EnUIDict;
        }

        /// <summary>
        /// Defines error message strings
        /// </summary>
        private void InitialiseErrorDictionary ()
        {
            EnErrMsgDict = new Dictionary<int, string>();
            EnErrTitleDict = new Dictionary<int, string>();

            EnErrMsgDict[1] = "KuroNote is not designed to load files larger than " + FILE_MAX_SIZE + " B. " +
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
            log.addLog("Settings initialised");
        }

        /// <summary>
        /// Starts logging if logging is enabled
        /// </summary>
        private void InitialiseLog()
        {
            log = new Log(this, temporaryLogEnabledFlag);
            if (temporaryLogEnabledFlag)
            {
                log.beginLog();
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
        /// <returns>The number of bytes in the file</returns>
        private long getFileSize(string _path)
        {
            if(!_path.Equals(string.Empty)) {
                return new FileInfo(_path).Length;
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Determines whether or not there is a file in the command line args to be opened automatically
        /// </summary>
        /// <returns>True if there's a file in command line argument 1, false otherwise</returns>
        private bool hasCmdLineFile()
        {
            //The file arg is arg[1]. arg[0] contains the KuroNote dll
            if(Environment.GetCommandLineArgs().Length == 2) {
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
            if(hasCmdLineFile()) {
                string[] args = Environment.GetCommandLineArgs();
                doOpen(args[1]);
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
        /// Configure the visual settings
        /// </summary>
        private void InitialiseTheme()
        {
            this.Title = "New File - " + appName;

            setTheme(appSettings.themeName, appSettings.themeWithFont);

            //Set additional properties

            //main.MainRtb.SelectionBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            MainRtb.BorderThickness = new Thickness(0, 0, 0, 0); //No blue outline when you click inside the RTB
            MainRtb.Padding = new Thickness(5, 5, 5, 5); //So you don't start typing right on the edge of the RTB
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
            if (_edited)
            {
                editedFlag = true;
                SaveStatusTb.Text = "Unsaved Changes";
            }
            else
            {
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
            if (editedFlag)
            {
                log.addLog("WARNING: New before saving");
                var res = MessageBox.Show(getErrorMessage(4)[0], getErrorMessage(4)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    log.addLog("New cancelled");
                    doSave();       //save
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
            if (_path.Equals(""))
            {
                //No file specified - use dialog
                log.addLog("Request: Open");

                if (editedFlag)
                {
                    log.addLog("WARNING: Open before saving");
                    var res = MessageBox.Show(getErrorMessage(3)[0], getErrorMessage(3)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                    {
                        log.addLog("Open cancelled");
                        doSave();       //save
                        return false;   //don't continue with open operation
                    }
                }

                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = FILE_FILTER
                };
                if (dlg.ShowDialog() == true)
                {
                    MemoryStream ms = new MemoryStream();
                    try
                    {
                        if(fileTooBig(dlg.FileName))
                        {
                            log.addLog("WARNING: File size exceeds limit");
                            var res = MessageBox.Show(getErrorMessage(1)[0], getErrorMessage(1)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if(res == MessageBoxResult.No)
                            {
                                log.addLog("Open cancelled due to file size");
                                return false;
                            }
                        }
                        using (FileStream file = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                        {
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
                    }
                    catch (Exception ex)
                    {
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
                try
                {
                    if (fileTooBig(_path))
                    {
                        log.addLog("WARNING: File size exceeds limit");
                        var res = MessageBox.Show(getErrorMessage(1)[0], getErrorMessage(1)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.No)
                        {
                            log.addLog("Open cancelled due to file size");
                            return false;
                        }
                    }
                    using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read))
                    {
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
                }
                catch (Exception ex)
                {
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
            if (editedFlag)
            {
                log.addLog("Request: Save");
                if(fileName.Equals(string.Empty)) {
                    log.addLog("File does not exist yet");
                    doSaveAs();
                } else {
                    TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                    MemoryStream ms = new MemoryStream(selectedEncoding.GetBytes(range.Text));

                    try
                    {
                        using (FileStream file = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write))
                        {
                            byte[] bytes = new byte[ms.Length];
                            ms.Read(bytes, 0, (int)ms.Length);
                            file.Write(bytes, 0, bytes.Length);
                            log.addLog("Successfully saved " + fileName);
                            ms.Close();
                        }
                        toggleEdited(false);
                        setStatus("Saved");
                    }
                    catch (Exception ex)
                    {
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
            if (dlg.ShowDialog() == true)
            {
                TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                MemoryStream ms = new MemoryStream(selectedEncoding.GetBytes(range.Text));

                try
                {
                    using (FileStream file = new FileStream(dlg.FileName, FileMode.Create, System.IO.FileAccess.Write))
                    {

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
                }
                catch (Exception ex)
                {
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
                foreach (string file in files)
                {
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
                if (_asAdmin)
                {
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
            if(editedFlag) {
                log.addLog("WARNING: Exit before saving");
                var res = MessageBox.Show(getErrorMessage(2)[0], getErrorMessage(2)[1], MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes) {
                    log.addLog("Exit cancelled");
                    return false;
                } else {
                    log.addLog("Exiting");
                    return true;
                }
            } else {
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
            log.addLog("Request: Cut");
            TextRange cutRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
            string textToCut = cutRange.Text;
            cutRange.Text = String.Empty;
            Clipboard.SetData(DataFormats.UnicodeText, textToCut);
        }

        /// <summary>
        /// Custom plaintext implementation of "Copy"
        /// </summary>
        private void Copy()
        {
            log.addLog("Request: Copy");
            TextRange copyRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
            string textToCopy = copyRange.Text;
            Clipboard.SetData(DataFormats.UnicodeText, textToCopy);
        }

        /// <summary>
        /// Make the clipboard text plain text to prepare for the default paste operation
        /// </summary>
        private void Paste()
        {
            log.addLog("Request: Paste");
            string textToPaste = Clipboard.GetText();
            Clipboard.SetData(DataFormats.UnicodeText, textToPaste);
            //Default paste operation runs
        }

        /// <summary>
        /// Captures events before they happen
        /// </summary>
        private void rtbEditor_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true; //Disable default Cut operation - use my own
                Cut();
            }
            else if (e.Command == ApplicationCommands.Copy)
            {
                e.Handled = true; //Disable default Copy operation - use my own
                Copy();
            }
            else if (e.Command == ApplicationCommands.Paste)
            {
                Paste();
                //Allow default Paste operation afterwards
            }
            else if (e.Command == ApplicationCommands.SelectAll)
            {
                log.addLog("Request: Select All");
                //Allow default Select All operation
            }
            else if (e.Command == ApplicationCommands.Undo)
            {
                log.addLog("Request: Undo");
                //Allow default Undo operation
            }
            else if (e.Command == ApplicationCommands.Redo)
            {
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
            FindDialog findDialog = new FindDialog(this, appSettings, log);
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
        /// <param name="_themeName">The name of the theme to change to</param>
        /// <param name="_includeFont">True if you want to change the font to the font that comes with the theme, false otherwise</param>
        public void setTheme(String _themeName, bool _includeFont)
        {
            if(_includeFont) {
                log.addLog("Changing theme to: " + _themeName + " (w/ font)");
            } else {
                log.addLog("Changing theme to: " + _themeName + " (w/o font)");
            }
            

            SolidColorBrush bgBrush = new SolidColorBrush();        //background for the whole window

            ImageBrush imgBrush = new ImageBrush();                 //background image OR
            SolidColorBrush solidBrush = new SolidColorBrush();     //background colour

            SolidColorBrush menuBrush = new SolidColorBrush();      //Menu bar background colour
            SolidColorBrush statusBrush = new SolidColorBrush();    //Status bar background colour

            SolidColorBrush textBrush = new SolidColorBrush();      //foreground colour
            

            bool hasImage = true;   //Does this theme have an image?

            switch(_themeName)
            {
                case "Default":
                    hasImage = true;

                    bgBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    textBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                    //Load image from file
                    var defaultBackground = new BitmapImage(new Uri(appPath + "conf\\custom.jpg", UriKind.Absolute));
                    imgBrush.ImageSource = defaultBackground;
                    imgBrush.Opacity = 0.40;

                    menuBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                    statusBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));

                    if (_includeFont)
                    {
                        setFont("Consolas", 18, FontWeights.Regular, FontStyles.Normal);
                    }
                    break;
                case "Morning Dew":
                    hasImage = true;

                    bgBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    textBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                    //Static resource
                    BitmapImage webBackground = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-pixabay-276205.jpg"));
                    imgBrush.ImageSource = webBackground;
                    imgBrush.Opacity = 0.33;

                    menuBrush = new SolidColorBrush(Color.FromRgb(255, 241, 219));
                    statusBrush = new SolidColorBrush(Color.FromRgb(255, 241, 219));

                    if (_includeFont)
                    {
                        setFont("Verdana", 18, FontWeights.Regular, FontStyles.Normal);
                    }                   
                    break;
                case "Wooden":
                    hasImage = true;

                    bgBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    textBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                    //Static resource
                    BitmapImage woodBackground = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-fwstudio-168447.jpg"));
                    imgBrush.ImageSource = woodBackground;
                    imgBrush.Opacity = 0.37;

                    menuBrush = new SolidColorBrush(Color.FromRgb(220, 193, 179));
                    statusBrush = new SolidColorBrush(Color.FromRgb(220, 193, 179));

                    if (_includeFont)
                    {
                        setFont("Verdana", 18, FontWeights.Regular, FontStyles.Normal);
                    }
                    break;
                case "Leafage":
                    hasImage = true;

                    bgBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    textBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

                    //Static resource
                    BitmapImage leafBackground = new BitmapImage(new Uri("pack://application:,,,/img/bgs/pexels-karolina-grabowska-4046687.jpg"));
                    imgBrush.ImageSource = leafBackground;
                    imgBrush.Opacity = 0.40;

                    menuBrush = new SolidColorBrush(Color.FromRgb(208, 201, 153));
                    statusBrush = new SolidColorBrush(Color.FromRgb(208, 201, 153));

                    if (_includeFont)
                    {
                        setFont("Verdana", 18, FontWeights.Regular, FontStyles.Normal);
                    }
                    break;
            }

            this.Background = bgBrush;
            MainRtb.Foreground = textBrush;
            if (hasImage) {
                MainRtb.Background = imgBrush;
            } else {
                MainRtb.Background = solidBrush;
            }
            MainMenu.Background = menuBrush;
            MainStatus.Background = statusBrush;
        }

        /// <summary>
        /// Menu > Options > Theme...
        /// </summary>
        private void Theme_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Theme...");
            ThemeSelector themeSelector = new ThemeSelector(this, appSettings, log);
            themeSelector.toggleVisibility(true);
        }

        /// <summary>
        /// Menu > Options > Custom Themes...
        /// </summary>
        private void CustomThemes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: Custom Themes...");
        }
        #endregion

        /// <summary>
        /// Menu > Options > Logging > Show Log...
        /// </summary>
        private void ShowLog_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (temporaryLogEnabledFlag)
            {
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
        /// Creates a string based on the number of words in the RTB
        /// </summary>
        /// <returns>A string that describes the number of words in the RTB</returns>
        private string generateWordCount()
        {
            TextRange document = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            MatchCollection words = Regex.Matches(document.Text, @"\S+");
            
            const string WORDS_POST = " Words";
            return words.Count + WORDS_POST;
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
        /// When the red X is clicked
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!doExit()) {
                e.Cancel = true;  //cancel the exit
                doSave();         //save instead
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

        //Format
        public static RoutedCommand Font = new RoutedCommand();

        //Tools
        public static RoutedCommand AESEnc = new RoutedCommand();
        public static RoutedCommand AESDec = new RoutedCommand();

        //Options
        public static RoutedCommand Theme = new RoutedCommand();
        public static RoutedCommand CustomThemes = new RoutedCommand();
        public static RoutedCommand ShowLog = new RoutedCommand();
        public static RoutedCommand ShowLogFiles = new RoutedCommand();
        public static RoutedCommand About = new RoutedCommand();
    }
}
