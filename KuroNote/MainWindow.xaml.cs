using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Constants
        private const string FILE_FILTER = "Text files (*.txt)|*.txt|All files (*.*)|*.*";  //For opening and saving files

        //Globals
        public string appName = "KuroNote";
        private string appPath = AppDomain.CurrentDomain.BaseDirectory;
        private KuroNoteSettings appSettings;
        private string fileName = string.Empty;             //Name of the loaded file - null if no file loaded

        private bool editedFlag = false;                    //Are there any unsaved changes?
        private Encoding selectedEncoding = Encoding.UTF8;  //Encoding for opening and saving files (Encoding.ASCII blocks unicode)

        #region Code for reference
        /*
        foreach (string arg in Environment.GetCommandLineArgs())
        {
            MessageBox.Show("Arg: " + arg);
        }
        
        //Example controls
        MainRtb.SpellCheck.IsEnabled = true;

        //MessageBoxResult result = MessageBox.Show("My Message Question", "My Title", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        */
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            appSettings = new KuroNoteSettings();
            appSettings.RetrieveSettings();
            InitialiseFont();
            InitialiseTheme();
            //open file in cmd line args
            toggleEdited(false);
            //Ready!
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
        }

        /// <summary>
        /// Configure the visual settings
        /// </summary>
        private void InitialiseTheme()
        {
            this.Title = appName;

            //Background for the whole window
            SolidColorBrush backgroundBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            this.Background = backgroundBrush;

            
            //Load image from file
            var resCustomBackground = new BitmapImage(new Uri(appPath + "conf/custom.jpg", UriKind.Absolute));

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
        /// Set the edited state to on/off
        /// </summary>
        private void toggleEdited(bool edited)
        {
            if (edited)
            {
                editedFlag = true;
                SaveStatusLbl.Content = "Unsaved Changes";
            }
            else
            {
                editedFlag = false;
                SaveStatusLbl.Content = "Safe to Exit";
            }
        }

        #region Open, Save and Save As...
        /// <summary>
        /// Loads a file into the RTB
        /// </summary>
        /// <param name="path">Optional: specify file to open instead of using file open dialog</param>
        private void doOpen(string path = "")
        {
            if (path.Equals(""))
            {
                //No file specified - use dialog
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
                        ms.Close();

                        fileName = dlg.FileName;
                        this.Title = fileName + " - " + appName;
                        toggleEdited(false);
                    }
                    catch (IOException ioEx)
                    {
                        //File cannot be accessed (e.g. used by another process)
                        MessageBox.Show(ioEx.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                //File specified - open without using a dialog
                MemoryStream ms = new MemoryStream();
                try
                {
                    using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);
                    }

                    TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                    range.Text = selectedEncoding.GetString(ms.ToArray());
                    ms.Close();

                    fileName = path;
                    this.Title = fileName + " - " + appName;
                    toggleEdited(false);
                }
                catch (IOException ioEx)
                {
                    //File cannot be accessed (e.g. used by another process)
                    MessageBox.Show(ioEx.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                TextRange range = new TextRange(MainRtb.Document.ContentStart, MainRtb.Document.ContentEnd);
                MemoryStream ms = new MemoryStream(selectedEncoding.GetBytes(range.Text));

                try
                {
                    using (FileStream file = new FileStream(fileName, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        byte[] bytes = new byte[ms.Length];
                        ms.Read(bytes, 0, (int)ms.Length);
                        file.Write(bytes, 0, bytes.Length);
                        ms.Close();
                    }
                    toggleEdited(false);
                }
                catch (IOException ioEx)
                {
                    //File cannot be accessed (e.g. used by another process)
                    MessageBox.Show(ioEx.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Saves a new file with the contents of the RTB, using a file selection dialog
        /// </summary>
        private void doSaveAs()
        {
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
                        ms.Close();
                    }

                    fileName = dlg.FileName;
                    this.Title = fileName + " - " + appName;
                    toggleEdited(false);
                }
                catch (IOException ioEx)
                {
                    //File cannot be accessed (e.g. used by another process)
                    MessageBox.Show(ioEx.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// Menu > File > Exit (or ALT+F4)
        /// </summary>
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region Cut, Copy and Paste
        /// <summary>
        /// Custom plaintext implementation of "Cut"
        /// </summary>
        private void Cut()
        {
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
            TextRange copyRange = new TextRange(MainRtb.Selection.Start, MainRtb.Selection.End);
            string textToCopy = copyRange.Text;
            Clipboard.SetData(DataFormats.UnicodeText, textToCopy);
        }

        /// <summary>
        /// Make the clipboard text plain text to prepare for the default paste operation
        /// </summary>
        private void Paste()
        {
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
                //cut()
            }
            else if (e.Command == ApplicationCommands.Copy)
            {
                e.Handled = true; //Disable default Copy operation - use my own
                //copy()
            }
            else if (e.Command == ApplicationCommands.Paste)
            {
                //paste()
                //Allow default Paste operation afterwards
            }
        }
        #endregion

        #region Font
        /// <summary>
        /// FontDialog uses this method to change the font
        /// </summary>
        public void setFont(String fontFamily, short fontSize, FontWeight fontWeight, FontStyle fontStyle)
        {
            MainRtb.FontFamily = new FontFamily(fontFamily);
            MainRtb.FontSize = fontSize;
            MainRtb.FontWeight = fontWeight;
            MainRtb.FontStyle = fontStyle;
        }

        /// <summary>
        /// Menu > Format > Font...
        /// </summary>
        private void Font_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FontDialog fontDialog = new FontDialog(this, appSettings);
        }
        #endregion

        /// <summary>
        /// When the RTB caret moves
        /// </summary>
        private void MainRtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            const string SELECT_LENGTH_PRE = "Selection is ";
            const string SELECT_LENGTH_POST = " character(s) long";

            TextRange tempRange = new TextRange(MainRtb.Document.ContentStart, MainRtb.Selection.Start);
            SelectLengthLbl.Content = SELECT_LENGTH_PRE + MainRtb.Selection.Text.Length + SELECT_LENGTH_POST;
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
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Commands
    /// </summary>
    public static class CustomCommands
    {
        //File
        public static RoutedCommand Open = new RoutedCommand();
        public static RoutedCommand Save = new RoutedCommand();
        public static RoutedCommand SaveAs = new RoutedCommand();
        public static RoutedCommand Exit = new RoutedCommand();

        //Edit
        public static RoutedCommand Cut = new RoutedCommand();
        public static RoutedCommand Copy = new RoutedCommand();
        public static RoutedCommand Paste = new RoutedCommand();

        //Foramat
        public static RoutedCommand Font = new RoutedCommand();

        //Define more commands here
    }
}
