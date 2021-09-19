using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector : Window
    {
        //Constants
        private const string WINDOW_NAME = "Theme";
        private const string CUSTOM_THEME_EXT = ".kurotheme";

        //Gamification constants
        private const int AP_SELECT_THEME = 18;

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        KuroNoteTheme[] themeCollection;
        Log log;

        private int previouslySelectedThemeId; //the theme to revert back to if the user clicks "Cancel" or "X"
        private string customThemePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\CustomThemes\\";

        public ThemeSelector(MainWindow _mainWin, KuroNoteSettings _currentSettings, KuroNoteTheme[] _themeCollection, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            themeCollection = _themeCollection;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;

            previouslySelectedThemeId = settings.themeId;
            saveCurrentRtbFontSize();
            loadPresetAndCustomThemes();
        }

        /// <summary>
        /// Remember exact font size if "remember font up/down" option is enabled
        /// </summary>
        private void saveCurrentRtbFontSize() {
            if (settings.rememberFontUpDn) {
                settings.fontSize = (int)main.MainRtb.FontSize;
                settings.UpdateSettings();
            }
        }

        /// <summary>
        /// Populate the dropdown menu with all available themes
        /// </summary>
        private void loadPresetAndCustomThemes() {
            cmbTheme.Items.Clear();

            try {
                string[] customThemeFiles = Directory.GetFiles(customThemePath);

                //Go through the array backwards when adding ComboBoxItems, so that the ComboBox is ordered "Newest themes descending"
                for (int i = (customThemeFiles.Length - 1); i >= 0; i--) {
                    //Only list .kurotheme files as selectable theme files
                    if (customThemeFiles[i].Contains(CUSTOM_THEME_EXT)) {
                        int customThemeId;
                        string customThemeName;

                        using (StreamReader sr = new StreamReader(customThemeFiles[i])) {
                            string json = sr.ReadToEnd();
                            KuroNoteCustomTheme kntFile = JsonConvert.DeserializeObject<KuroNoteCustomTheme>(json);
                            customThemeId = kntFile.themeId;
                            customThemeName = kntFile.themeName;
                        }

                        ComboBoxItem customThemeItem = new ComboBoxItem();
                        customThemeItem.Tag = customThemeId;
                        customThemeItem.Content = customThemeName;
                        if (customThemeId == settings.themeId) {
                            log.addLog("Selected theme: " + customThemeItem.Content + " (Custom)");
                            customThemeItem.IsSelected = true;
                        }
                        cmbTheme.Items.Add(customThemeItem);
                    }
                }
            } catch (Exception e) {
                log.addLog(e.ToString());
            }

            foreach (KuroNoteTheme theme in themeCollection) {
                //Add themes that are already unlocked OR the unlock code is stored in the achievements arry (i.e. previously locked, now unlocked)
                if (theme.unlockCode == 0 || settings.achList.Contains(theme.unlockCode)) {
                    ComboBoxItem themeItem = new ComboBoxItem();
                    themeItem.Tag = theme.themeId;
                    themeItem.Content = theme.themeName;
                    if (theme.themeId == settings.themeId) {
                        log.addLog("Selected theme: " + themeItem.Content);
                        themeItem.IsSelected = true;
                    }
                    cmbTheme.Items.Add(themeItem);
                }
            }
        }

        /// <summary>
        /// Temporarily apply the theme and display the corresponding theme description when the theme selection changes
        /// </summary>
        private void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int selectedThemeTag = (int)cmbTheme.SelectedValue; //tag stores the corresponding themeId

            if (selectedThemeTag < 1000) { //currently, only preset themes can have descriptions
                tbThemeDesc.Text = themeCollection[selectedThemeTag].themeDesc;
            } else {
                tbThemeDesc.Text = "Custom theme";
            }
            main.setTheme(selectedThemeTag, (bool)chkIncludeFont.IsChecked);
        }

        /// <summary>
        /// Update the theme preview to include/omit the theme font when the checkbox state is changed
        /// </summary>
        private void chkIncludeFont_CheckChanged(object sender, RoutedEventArgs e) {
            try {
                int selectedThemeTag = (int)cmbTheme.SelectedValue; //tag stores the corresponding themeId
                bool isChecked = (bool)chkIncludeFont.IsChecked;
                if (!isChecked) { //User doesn't want the theme font
                    //Apply the user's currently saved font instead of the theme font
                    main.setFont(settings.fontFamily, (short)settings.fontSize, settings.fontWeight, settings.fontStyle);
                }
                main.setTheme(selectedThemeTag, isChecked);
            } catch(Exception ex) {
                Console.Error.Write(ex.ToString()); //Attempted to apply theme before the combobox is initialised
            }
        }

        /// <summary>
        /// Update theme preferences to the specified theme name
        /// </summary>
        /// <param name="_selectedTheme">The theme name to change the selectedTheme setting to</param>
        /// <param name="_includesFont">Whether or not to use the font that comes with the theme</param>
        private void applyTheme(int _selectedThemeId, bool _includesFont) {
            log.addLog("Updating theme preference: Theme ID " + _selectedThemeId);

            settings.themeId = _selectedThemeId;
            settings.themeWithFont = _includesFont;
            settings.UpdateSettings(); //Write these changes to the file
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis) {
            if (vis) {
                log.addLog("Open ThemeSelector");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse ThemeSelector");
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            int newThemeId = (int)cmbTheme.SelectedValue; //tag stores the corresponding themeId
            bool themeIncludesFont = (bool)chkIncludeFont.IsChecked;

            main.setTheme(newThemeId, themeIncludesFont);
            applyTheme(newThemeId, themeIncludesFont);
            main.incrementAp(AP_SELECT_THEME);
            toggleVisibility(false);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            main.setTheme(previouslySelectedThemeId, settings.themeWithFont);
            toggleVisibility(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            main.setTheme(previouslySelectedThemeId, settings.themeWithFont);
            log.addLog("Close ThemeSelector");
        }
    }
}
