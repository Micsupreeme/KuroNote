using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Options";
        private const string RESTART_DISCLAIMER_PRE = "Changes apply upon next ";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;

        Timer gamificationTimer;
        bool anythingChanged = false;
        bool previousRtfModeState = false;

        public OptionsDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            restartDisclaimerLbl.Content = RESTART_DISCLAIMER_PRE + appName + " launch";
            populateFields();
            previousRtfModeState = settings.rtfMode;
            if (settings.gamification) {
                initialiseOptionsGamification();
            }
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open OptionsDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse OptionsDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Update the UI to match the current KuroNoteSettings
        /// </summary>
        private void populateFields()
        {
            gamificationTs.IsChecked = settings.gamification;
            loggingTs.IsChecked = settings.logging;
            windowsizeTs.IsChecked = settings.rememberWindowSize;
            floatingTs.IsChecked = settings.floating;
            useasciiTs.IsChecked = settings.useAscii;
            spellcheckTs.IsChecked = settings.spellCheck;
            wordwrapTs.IsChecked = settings.wordWrap;
            overtypingTs.IsChecked = settings.overTyping;
            autowordselectionTs.IsChecked = settings.autoWordSelection;
            fullfilepathTs.IsChecked = settings.fullFilePath;
            stretchimagesTs.IsChecked = settings.stretchImages;
            rememberfontupdnTs.IsChecked = settings.rememberFontUpDn;
            rememberrecentfilesTs.IsChecked = settings.rememberRecentFiles;
            rtfmodeTs.IsChecked = settings.rtfMode;
            encopenTs.IsChecked = settings.encOpen;
            searchbingTs.IsChecked = settings.searchBing;
            searchbraveTs.IsChecked = settings.searchBrave;
            searchduckduckgoTs.IsChecked = settings.searchDuckDuckGo;
            searchgoogleTs.IsChecked = settings.searchGoogle;
            searchgooglescholarTs.IsChecked = settings.searchGoogleScholar;
            searchstackoverflowTs.IsChecked = settings.searchStackOverflow;
            searchstartpageTs.IsChecked = settings.searchStartpage;
            searchwikipediaTs.IsChecked = settings.searchWikipedia;
            searchyahooTs.IsChecked = settings.searchYahoo;
            searchyoutubeTs.IsChecked = settings.searchYouTube;
        }

        /// <summary>
        /// Update the current KuroNoteSettings since a UI option has been changed
        /// </summary>
        private void saveSettingsChanges()
        {
            settings.gamification = (bool)gamificationTs.IsChecked;
            settings.logging = (bool)loggingTs.IsChecked;
            settings.rememberWindowSize = (bool)windowsizeTs.IsChecked;
            settings.floating = (bool)floatingTs.IsChecked;
            settings.useAscii = (bool)useasciiTs.IsChecked;
            settings.spellCheck = (bool)spellcheckTs.IsChecked;
            settings.wordWrap = (bool)wordwrapTs.IsChecked;
            settings.overTyping = (bool)overtypingTs.IsChecked;
            settings.autoWordSelection = (bool)autowordselectionTs.IsChecked;
            settings.fullFilePath = (bool)fullfilepathTs.IsChecked;
            settings.stretchImages = (bool)stretchimagesTs.IsChecked;
            settings.rememberFontUpDn = (bool)rememberfontupdnTs.IsChecked;
            settings.rememberRecentFiles = (bool)rememberrecentfilesTs.IsChecked;
            settings.rtfMode = (bool)rtfmodeTs.IsChecked;
            settings.encOpen = (bool)encopenTs.IsChecked;
            settings.searchBing = (bool)searchbingTs.IsChecked;
            settings.searchBrave = (bool)searchbraveTs.IsChecked;
            settings.searchDuckDuckGo = (bool)searchduckduckgoTs.IsChecked;
            settings.searchGoogle = (bool)searchgoogleTs.IsChecked;
            settings.searchGoogleScholar = (bool)searchgooglescholarTs.IsChecked;
            settings.searchStackOverflow = (bool)searchstackoverflowTs.IsChecked;
            settings.searchStartpage = (bool)searchstartpageTs.IsChecked;
            settings.searchWikipedia = (bool)searchwikipediaTs.IsChecked;
            settings.searchYahoo = (bool)searchyahooTs.IsChecked;
            settings.searchYouTube = (bool)searchyoutubeTs.IsChecked;
            settings.UpdateSettings();
        }

        #region "user has changed a setting" flag
        //The "checked" and "unchecked" events fire before the window is even initialised,
        //which makes it look like the user has changed an option during every visit to OptionsDialog.
        //Dirty workaround - reset the "anythingChanged" flag after 100ms
        //(i.e. only change(s) to options registered after this incredibly short interval will count towards achievement)

        /// <summary>
        /// Starts the gamifiaction timer
        /// </summary>
        private void initialiseOptionsGamification()
        {
            gamificationTimer = new Timer();
            gamificationTimer.Elapsed += GamificationTimer_Elapsed;
            gamificationTimer.Interval = 100;
            gamificationTimer.Enabled = true;
        }

        /// <summary>
        /// When the gamification timer elapses
        /// </summary>
        private void GamificationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            anythingChanged = false;
            gamificationTimer.Stop();
        }
        #endregion

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (anythingChanged) {
                log.addLog("Update options according to OptionsDialog UI");
                saveSettingsChanges();

                if (previousRtfModeState != settings.rtfMode) {
                    //RTF Mode was actually changed
                    main.processImmediateSettings(true, true); //Apply settings that can take effect immediately (and do RTF transition)
                } else {
                    main.processImmediateSettings(true, false); //Apply settings that can take effect immediately (and don't do RTF transition)
                }

                if (settings.gamification) {
                    settings.achOptions++;
                    settings.UpdateSettings();

                    switch (settings.achOptions) {
                        case 5:
                            main.unlockAchievement(20);
                            break;
                    }
                }
            }
            toggleVisibility(false);
        }

        /// <summary>
        /// When the user clicks "Cancel" or hits ESC
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Doesn't save changes
            toggleVisibility(false);
        }

        /// <summary>
        /// When any of the options ToggleSwitches is changed to a checked state
        /// </summary>
        private void optionTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                anythingChanged = true;
            } catch (NullReferenceException) {
                Debug.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// When any of the options ToggleSwitches is changed to an unchecked state
        /// </summary>
        private void optionTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                anythingChanged = true;
            } catch (NullReferenceException) {
                Debug.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close OptionsDialog");
        }
    }
}
