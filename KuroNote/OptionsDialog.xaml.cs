using System;
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
        private const string BOOL0 = "Off";
        private const string BOOL1 = "On";
        private const string RESTART_DISCLAIMER_PRE = "Changes apply upon next ";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;

        Timer gamificationTimer;
        bool anythingChanged = false;

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
            if(settings.gamification) {
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
            fullfilepathTs.IsChecked = settings.fullFilePath;
            stretchimagesTs.IsChecked = settings.stretchImages;
            rememberfontupdnTs.IsChecked = settings.rememberFontUpDn;
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
            settings.fullFilePath = (bool)fullfilepathTs.IsChecked;
            settings.stretchImages = (bool)stretchimagesTs.IsChecked;
            settings.rememberFontUpDn = (bool)rememberfontupdnTs.IsChecked;
            settings.UpdateSettings();
        }

        #region "user has changed a setting" flag
        //The "checked" and "unchecked" events fire before the window is even initialised,
        //which makes it look like the user has changed an option during every visit to OptionsDialog.
        //Dirty workaround - reset the "anythingChanged" flag after 100ms
        //(i.e. only change(s) to options registered after this incredibly short interval will count towards achievement)
        private void initialiseOptionsGamification()
        {
            gamificationTimer = new Timer();
            gamificationTimer.Elapsed += GamificationTimer_Elapsed;
            gamificationTimer.Interval = 100;
            gamificationTimer.Enabled = true;
        }

        private void GamificationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            anythingChanged = false;
            gamificationTimer.Stop();
        }
        #endregion

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            log.addLog("Update options according to OptionsDialog UI");
            saveSettingsChanges();
            main.processImmediateSettings(); //Apply settings that can take effect immediately
            toggleVisibility(false);

            if (settings.gamification && anythingChanged) {
                settings.achOptions++;
                settings.UpdateSettings();

                switch (settings.achOptions)
                {
                    case 5:
                        main.unlockAchievement(19);
                        break;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Doesn't save changes
            toggleVisibility(false);
        }

        private void gamificationTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                gamificationLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void gamificationTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                gamificationLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void loggingTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                loggingLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void loggingTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                loggingLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void spellcheckTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                spellcheckLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void spellcheckTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                spellcheckLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void floatingTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                floatingLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void floatingTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                floatingLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void windowsizeTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                windowsizeLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void windowsizeTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                windowsizeLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void fullfilepathTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                fullfilepathLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void fullfilepathTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                fullfilepathLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void stretchimagesTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                stretchimagesLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void stretchimagesTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                stretchimagesLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void rememberfontupdnTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                rememberfontupdnLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void rememberfontupdnTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                rememberfontupdnLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void wordwrapTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                wordwrapLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void wordwrapTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                wordwrapLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void useasciiTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                useasciiLbl.Content = BOOL1;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void useasciiTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                useasciiLbl.Content = BOOL0;
                anythingChanged = true;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close OptionsDialog");
        }
    }
}
