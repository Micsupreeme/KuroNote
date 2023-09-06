using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for SpellcheckDictionaryDialog.xaml
    /// </summary>
    public partial class SpellcheckDictionaryDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Spell Check Dictionary Editor";
        private const string DICTIONARY_SAVE_ENABLED = "Save";
        private const string DICTIONARY_SAVE_DISABLED = "Saved";

        //Globals
        private readonly string appName;
        private readonly string customDictionaryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\dictionary.lex";
        private readonly string localDictionaryCachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp\\WPF";
        private bool editedFlag = false;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;

        public SpellcheckDictionaryDialog(MainWindow _mainWin, KuroNoteSettings _settings, Log _mainLog)
        {
            InitializeComponent();
            //InitialiseTheme(); //set a theme for the dictionary textbox?
            main = _mainWin;
            settings = _settings;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            populateDictionary(); //load existing dictionary data so that it can be edited
        }

        /// <summary>
        /// Load existing dictionary data into the dictionary textbox (so that it can be edited)
        /// </summary>
        private void populateDictionary()
        {
            try {
                //Load dictionary data into the editor
                txtDictionary.Text = File.ReadAllText(customDictionaryPath, Encoding.UTF8);
                editedFlag = false;
                toggleSaveState(1);
                log.addLog("Successfully loaded " + customDictionaryPath);
            } catch (Exception e) {
                log.addLog("ERROR: " + customDictionaryPath + " could not be opened");
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Prepares the edited dictionary text to be saved to the dictionary.lex file by
        /// 1. Removing all spaces
        /// 2. Removing leading and trailing whitespace characters
        /// The processed text then replaces the raw text inside the textbox, which is then saved
        /// </summary>
        private void processDictionaryText()
        {
            string processedDictionary = txtDictionary.Text;

            //Remove any spaces
            if (Regex.IsMatch(processedDictionary, " ")) {
                processedDictionary = Regex.Replace(processedDictionary, " ", "");
                log.addLog("Dictionary check: removed one or more spaces");
            }

            //Remove the last character if it's a line return
            processedDictionary = processedDictionary.Trim();

            txtDictionary.Text = processedDictionary;
        }

        /// <summary>
        /// Updates the dictionary file with the contents of the dictionary textbox (saves changes)
        /// </summary>
        private void updateDictionary()
        {
            try {
                //Save dictionary textbox contents to the dictionary file
                File.WriteAllText(customDictionaryPath, txtDictionary.Text, Encoding.UTF8);
                log.addLog("Successfully saved " + customDictionaryPath);
            } catch (Exception e) {
                log.addLog("ERROR: " + customDictionaryPath + " could not be saved");
                log.addLog(e.ToString());
            }

            /*There's no point in letting the user choose to not clear the cache because then the dictionary just doesn't update in real time
             * So the checkbox is permenantly checked (the choice to clear or not clear the cache can therefore be restored whenever)*/
            if (chkClearCache.IsChecked == true) {
                clearLocalDictionaryCache();
            }
        }

        /// <summary>
        /// Starts a background worker that
        /// Wipes "<DRIVE>:\Users\<USERNAME>\AppData\Local\Temp\WPF" so that when words are removed from this dictionary,
        /// they are actually removed from Spell Check (for some reason Windows duplicates all WPF dictionaries here)
        /// </summary>
        private void clearLocalDictionaryCache()
        {

            BackgroundWorker cacheDeleter = new BackgroundWorker();
            cacheDeleter.DoWork += cacheDeleter_DoWork;
            cacheDeleter.RunWorkerCompleted += cacheDeleter_RunWorkerCompleted;
            cacheDeleter.WorkerReportsProgress = false;
            cacheDeleter.WorkerSupportsCancellation = false;
            log.addLog("Beginning deletion of " + localDictionaryCachePath + " and its contents");
            cacheDeleter.RunWorkerAsync();
        }

        /// <summary>
        /// When cacheDeleter is asked to run, this executes (asynchronously)
        /// Deletes the local dictionary cache folder (and all files inside), if it exists
        /// </summary>
        void cacheDeleter_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                if (Directory.Exists(localDictionaryCachePath)) {
                    Directory.Delete(localDictionaryCachePath, true); //delete the local cache directory and everything inside
                }
            } catch (Exception ex) {
                Debug.WriteLine("ERROR: unable to fully delete " + localDictionaryCachePath);
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// When cacheDeleter finishes _DoWork, this executes
        /// Here we check if the local dictionary cache still exists, if so then we know the deletion failed
        /// </summary>
        void cacheDeleter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Was it deleted?
            if (Directory.Exists(localDictionaryCachePath)) {
                //No
                log.addLog("WARN: local dictionary cache could not be fully cleared");
            } else {
                //Yes - success
                log.addLog("Successfully cleared local dictionary cache");
            }
        }

        /// <summary>
        /// Toggles between the save-related UI states
        /// </summary>
        /// <param name="state">The ID of the state to change to</param>
        private void toggleSaveState(int state)
        {
            switch (state) {
                case 0:
                    //Unsaved changes - ready to save
                    btnDictionarySave.IsEnabled = true;
                    btnDictionarySave.Content = DICTIONARY_SAVE_ENABLED;
                    btnDictionarySave.Visibility = Visibility.Visible;
                    lblDictionaryBadFormat.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    //Safe to exit - no need to save
                    btnDictionarySave.IsEnabled = false;
                    btnDictionarySave.Content = DICTIONARY_SAVE_DISABLED;
                    btnDictionarySave.Visibility = Visibility.Visible;
                    lblDictionaryBadFormat.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    //Dictionary contains illegal characters (SPACES) - not allowed to save
                    btnDictionarySave.IsEnabled = true;
                    btnDictionarySave.Content = DICTIONARY_SAVE_ENABLED;
                    btnDictionarySave.Visibility = Visibility.Visible;
                    lblDictionaryBadFormat.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open SpellcheckDictionaryDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse SpellcheckDictionaryDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Calls for the dictionary text to be prepared, written to the file, and then to refresh the spellcheck dictionary
        /// </summary>
        private void saveAndRefresh()
        {
            processDictionaryText();
            updateDictionary();
            if (editedFlag)
            {
                //Changes were made - refresh the dictionary for MainRtb
                main.ToggleCustomSpellcheckDictionaries(false);
                main.ToggleCustomSpellcheckDictionaries(true);
            }
            editedFlag = false;
            toggleSaveState(1);
        }

        /// <summary>
        /// Handles unsaved changes by asking the user if they wish to save their changes, then exits
        /// </summary>
        private void handleExit()
        {
            if (editedFlag) {
                //There are unsaved changes and there are no spaces - offer to save changes
                var res = MessageBox.Show("You have made changes to your custom dictionary that have not been saved. Would you like to save before exiting?", "Save before exit?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes) {
                    //Yes - exit and save changes
                    saveAndRefresh();
                    toggleVisibility(false);
                } else if (res == MessageBoxResult.No) {
                    //No - exit without saving changes
                    toggleVisibility(false);
                }
            } else {
                //Safe to exit
                toggleVisibility(false);
            }
        }

        /// <summary>
        /// When the user edits the dictionary textbox
        /// </summary>
        private void txtDictionary_TextChanged(object sender, TextChangedEventArgs e)
        {
            editedFlag = true;
            if (Regex.IsMatch(txtDictionary.Text, " "))
            {
                toggleSaveState(2); //bad format state
            } else {
                toggleSaveState(0); //ready to save state
            }
        }

        /// <summary>
        /// When the user clicks "Save"
        /// </summary>
        private void btnDictionarySave_Click(object sender, RoutedEventArgs e)
        {
            saveAndRefresh();
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER or hits ESC
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            handleExit();
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            handleExit();
            log.addLog("Close SpellcheckDictionaryDialog");
        }
    }
}
