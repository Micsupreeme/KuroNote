using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for EncDecFinishDialog.xaml
    /// </summary>
    public partial class EncDecFinishDialog : Window
    {
        //Constants
        private const string FILE_FILTER = "KuroNotes (*.kuro)|*.kuro|Text files (*.txt)|*.txt|All files (*.*)|*.*";  //For opening and saving encrypted/decrypted files

        //Gamification constants
        private const int AP_ENCRYPT = 30;
        private const int AP_DECRYPT = 35;

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;

        string processedContent;                 //a copy of the encrypted/decrypted text to process (e.g. by saving it)
        int statusCode;                          //a number that indicates which operation has just ocurred (e.g. AES Encrypt Complete)
        string statusDescription;

        public EncDecFinishDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog, int _statusCode, string _content)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            statusCode = _statusCode;
            processedContent = _content;
            appName = main.appName;
            processStatus();
        }

        private string getFormattedSize()
        {
            double size = Encoding.UTF8.GetByteCount(processedContent);
            string formattedSize = Math.Round(size, 2) + " B";

            if (size > 1024) {
                size /= 1024;
                formattedSize = Math.Round(size, 2) + " KB";
                if (size > 1024) {
                    size /= 1024;
                    formattedSize = Math.Round(size, 2) + " MB";
                }
            } else {
                return formattedSize;
            }
            return formattedSize;
        }

        /// <summary>
        /// Changes the text according to the statusCode sent
        /// </summary>
        private void processStatus()
        {
            switch(statusCode)
            {
                case 0:
                    //AES Encrypt Success
                    this.Title = "AES Encryption - " + appName;
                    statusDescription = "AES Encryption Complete" + Environment.NewLine + Environment.NewLine + "File Size: " + getFormattedSize();
                    btnSave.IsEnabled = true;

                    if (settings.gamification) {
                        settings.achEncrypts++;
                        settings.UpdateSettings();

                        switch(settings.achEncrypts) {
                            case 10:
                                main.unlockAchievement(9);
                                break;
                            case 50:
                                main.unlockAchievement(10);
                                break;
                        }
                    }

                    break;
                case 1:
                    //AES Encrypt Failure
                    this.Title = "AES Encryption - " + appName;
                    statusDescription = "AES Encryption Failed" + Environment.NewLine + Environment.NewLine + "File Size: " + getFormattedSize();
                    btnSave.IsEnabled = false;
                    break;
                case 2:
                    //AES Decrypt Success
                    this.Title = "AES Decryption - " + appName;
                    statusDescription = "AES Decryption Complete" + Environment.NewLine + Environment.NewLine + "File Size: " + getFormattedSize();
                    btnSave.IsEnabled = true;
                    break;
                case 3:
                    //AES Decrypt Failure
                    this.Title = "AES Decryption - " + appName;
                    statusDescription = "AES Decryption Failed" + Environment.NewLine + Environment.NewLine + "File Size: " + getFormattedSize();
                    btnSave.IsEnabled = false;
                    break;
                default:
                    //Invalid
                    log.addLog("EncDecFinishDialog has invalid status");
                    this.Title = "Invalid - " + appName;                 
                    statusDescription = "Invalid";
                    btnSave.IsEnabled = false;
                    break;
            }
            StatusLbl.Content = statusDescription;
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open EncDecFinishDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse EncDecFinishDialog");
                this.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// Saves a new file with the processed content, using a file selection dialog
        /// </summary>
        private void doSaveAs()
        {
            log.addLog("Request: Save Processed Content As");

            string dialogTitle = "Save As";
            switch(statusCode)
            {
                case 0:
                    dialogTitle = "Save AES Encrypted Content As";
                    main.incrementAp(AP_ENCRYPT);
                    break;
                case 2:
                    dialogTitle = "Save AES Decrypted Content As";
                    main.incrementAp(AP_DECRYPT);
                    break;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = dialogTitle,
                DefaultExt = ".kuro",
                AddExtension = true,
                Filter = FILE_FILTER
            };
            if (dlg.ShowDialog() == true) {
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(processedContent));

                try {
                    using (FileStream file = new FileStream(dlg.FileName, FileMode.Create, System.IO.FileAccess.Write)) {

                        byte[] bytes = new byte[ms.Length];
                        ms.Read(bytes, 0, (int)ms.Length);
                        file.Write(bytes, 0, bytes.Length);
                        log.addLog("Successfully saved " + dlg.FileName);
                        ms.Close();
                    }
                    //Automatically opens the recently encrypted/decrypted file if the corresponding option is enabled
                    if (settings.encopen) {
                        startProcess(dlg.FileName);
                    }
                } catch (Exception ex) {
                    //File cannot be accessed (e.g. used by another process)
                    log.addLog(ex.ToString());
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// When the user clicks "Save"
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            doSaveAs();
            toggleVisibility(false);
        }

        /// <summary>
        /// When the user clicks "Discard"
        /// </summary>
        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close EncDecFinishDialog");
        }
    }
}
