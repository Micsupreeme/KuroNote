using System;
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
            floatingTs.IsChecked = settings.floating;
            windowsizeTs.IsChecked = settings.rememberWindowSize;
            useasciiTs.IsChecked = settings.useAscii;
            wordwrapTs.IsChecked = settings.wordWrap;
            fullfilepathTs.IsChecked = settings.fullFilePath;
            rememberfontupdnTs.IsChecked = settings.rememberFontUpDn;
            overridethemefontsizeTs.IsChecked = settings.overrideThemeFontSize;
        }

        /// <summary>
        /// Update the current KuroNoteSettings since a UI option has been changed
        /// </summary>
        private void saveSettingsChanges()
        {
            settings.gamification = (bool)gamificationTs.IsChecked;
            settings.logging = (bool)loggingTs.IsChecked;
            settings.floating = (bool)floatingTs.IsChecked;
            settings.rememberWindowSize = (bool)windowsizeTs.IsChecked;
            settings.useAscii = (bool)useasciiTs.IsChecked;
            settings.wordWrap = (bool)wordwrapTs.IsChecked;
            settings.fullFilePath = (bool)fullfilepathTs.IsChecked;
            settings.rememberFontUpDn = (bool)rememberfontupdnTs.IsChecked;
            settings.overrideThemeFontSize = (bool)overridethemefontsizeTs.IsChecked;
            settings.UpdateSettings();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            log.addLog("Update options according to OptionsDialog UI");
            saveSettingsChanges();
            main.processImmediateSettings(); //Apply settings that can take effect immediately
            toggleVisibility(false);
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
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void gamificationTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                gamificationLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void loggingTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                loggingLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void loggingTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                loggingLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void floatingTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                floatingLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void floatingTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                floatingLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void windowsizeTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                windowsizeLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void windowsizeTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                windowsizeLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void fullfilepathTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                fullfilepathLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void fullfilepathTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                fullfilepathLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void rememberfontupdnTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                rememberfontupdnLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void rememberfontupdnTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                rememberfontupdnLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void overridethemefontsizeTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                overridethemefontsizeLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void overridethemefontsizeTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                overridethemefontsizeLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void wordwrapTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                wordwrapLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void wordwrapTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                wordwrapLbl.Content = BOOL0;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Unchecked Event fired before object initialisation ");
            }
        }

        private void useasciiTs_Checked(object sender, RoutedEventArgs e)
        {
            try {
                useasciiLbl.Content = BOOL1;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ToggleSwitch_Checked Event fired before object initialisation ");
            }
        }

        private void useasciiTs_Unchecked(object sender, RoutedEventArgs e)
        {
            try {
                useasciiLbl.Content = BOOL0;
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
