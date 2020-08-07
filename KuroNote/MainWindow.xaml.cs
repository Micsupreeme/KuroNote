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

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// TODO: Show log files
    /// TODO: Password complexity measurer for AES Encryption
    /// TODO: File Size Check (max 1MB?) before open or open with command line
    /// TODO: Word count
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        //Constants
        private const string FILE_FILTER = "Text files (*.txt)|*.txt|KuroNotes (*.kuro)|*.kuro|All files (*.*)|*.*";  //For opening and saving files

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
        public Dictionary<string, object> EnUIDict;

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

        private void InitialiseUIDictionary()
        {
            EnUIDict = new Dictionary<string, object>();
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
            EnUIDict["SelectAllMi"] = "Select All";
            //Format
            EnUIDict["FormatMi"] = "Format";
            EnUIDict["FontMi"] = "Font...";
            //Security
            EnUIDict["SecurityMi"] = "Security";
            EnUIDict["AESMi"] = "AES Encryption";
            EnUIDict["AESEncMi"] = "AES Encrypt...";
            EnUIDict["AESDecMi"] = "AES Decrypt...";
            //Options
            EnUIDict["OptionsMi"] = "Options";
            EnUIDict["LoggingMi"] = "Logging";
            EnUIDict["ShowLogMi"] = "Show Log";
            EnUIDict["ShowLogFilesMi"] = "Show Log Files";
            EnUIDict["AboutMi"] = "About " + appName;
            this.DataContext = EnUIDict;
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

            //Background for the whole window
            SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.Background = backgroundBrush;

            
            //Load image from file
            var resCustomBackground = new BitmapImage(new Uri(appPath + "conf\\custom.jpg", UriKind.Absolute));

            //Static resource
            BitmapImage bmi = new BitmapImage(new Uri("pack://application:,,,/img/water.png"));


            //Image background for the text box
            ImageBrush imgBrush = new ImageBrush
            {
                ImageSource = resCustomBackground,
                Opacity = 0.40
            };
            MainRtb.Background = imgBrush;

            //Text colour
            SolidColorBrush textColourBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            MainRtb.Foreground = textColourBrush;

            //Set additional properties
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
        private void doNew()
        {
            log.addLog("Request: New File");
            fileName = string.Empty;
            TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
            range.Text = string.Empty;
            this.Title = "New File - " + appName;
            toggleEdited(false);
            setStatus("New File");
            log.addLog("Content deleted");
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
        private void doOpen(string _path = "")
        {
            if (_path.Equals(""))
            {
                //No file specified - use dialog
                log.addLog("Request: Open");
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Filter = FILE_FILTER
                };
                if (dlg.ShowDialog() == true)
                {
                    MemoryStream ms = new MemoryStream();
                    try
                    {
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
                }
            }
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

        private void doExit()
        {
            log.addLog("Exiting");
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Menu > File > Exit (or ALT+F4)
        /// </summary>
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            doExit();
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

        #region Font
        /// <summary>
        /// FontDialog uses this method to change the font
        /// </summary>
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

        private void About_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            log.addLog("Request: About");
            MessageBox.Show("abt");
        }

        /// <summary>
        /// When the RTB caret moves
        /// </summary>
        private void MainRtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            const string SELECT_LENGTH_PRE = "Selection is ";
            const string SELECT_LENGTH_POST = " character(s) long";

            TextRange tempRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Selection.Start);
            SelectLengthTb.Text = SELECT_LENGTH_PRE + MainRtb.Selection.Text.Length + SELECT_LENGTH_POST;
        }

        /// <summary>
        /// When the RTB is edited
        /// </summary>
        private void MainRtb_TextChanged(object sender, RoutedEventArgs e)
        {
            toggleEdited(true);
        }

        /// <summary>
        /// When the red X is clicked
        /// </summary>
        private void Win_Closed(object sender, EventArgs e)
        {
            doExit();
        }
    }

    /// <summary>
    /// Commands
    /// </summary>
    public static class CustomCommands
    {
        //File
        public static RoutedCommand New = new RoutedCommand();
        public static RoutedCommand Open = new RoutedCommand();
        public static RoutedCommand Save = new RoutedCommand();
        public static RoutedCommand SaveAs = new RoutedCommand();
        public static RoutedCommand Print = new RoutedCommand();
        public static RoutedCommand Exit = new RoutedCommand();

        //Edit
        public static RoutedCommand Cut = new RoutedCommand();
        public static RoutedCommand Copy = new RoutedCommand();
        public static RoutedCommand Paste = new RoutedCommand();

        //Format
        public static RoutedCommand Font = new RoutedCommand();

        //Security
        public static RoutedCommand AESEnc = new RoutedCommand();
        public static RoutedCommand AESDec = new RoutedCommand();

        //Options
        public static RoutedCommand ShowLog = new RoutedCommand();
        public static RoutedCommand About = new RoutedCommand();
    }
}
