using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for ThemeSelector.xaml
    /// </summary>
    public partial class ThemeSelector : Window
    {
        //Constants
        private const string WINDOW_NAME = "Theme";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        KuroNoteTheme[] themeCollection;
        Log log;

        private int previouslySelectedThemeId; //the theme to revert back to if the user clicks "Cancel" or "X"

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
            loadThemes();
            //loadCustomThemes()
        }

        /// <summary>
        /// Populate the dropdown menu with the available built-in themes
        /// </summary>
        private void loadThemes()
        {
            cmbTheme.Items.Clear();
            foreach(KuroNoteTheme theme in themeCollection)
            {
                ComboBoxItem themeItem = new ComboBoxItem();
                themeItem.Content = theme.themeName;
                themeItem.Tag = theme.themeId;
                if(theme.themeId == settings.themeId) {
                    log.addLog("Selected theme: " + themeItem.Content);
                    themeItem.IsSelected = true;
                }
                cmbTheme.Items.Add(themeItem);
            }
        }

        /// <summary>
        /// Temporarily apply the theme and display the corresponding theme description when the theme selection changes
        /// </summary>
        private void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedThemeTag = (int)cmbTheme.SelectedValue; //tag stores the corresponding themeId

            tbThemeDesc.Text = themeCollection[selectedThemeTag].themeDesc;
            main.setTheme(selectedThemeTag, (bool)chkIncludeFont.IsChecked);
        }

        /// <summary>
        /// Update the theme preview to include/omit the theme font when the checkbox state is changed
        /// </summary>
        private void chkIncludeFont_CheckChanged(object sender, RoutedEventArgs e)
        {
            try
            {
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
        public void toggleVisibility(bool vis)
        {
            if (vis)
            {
                log.addLog("Open ThemeSelector");
                this.Visibility = Visibility.Visible;
            }
            else
            {
                log.addLog("Collapse ThemeSelector");
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int newThemeId = (int)cmbTheme.SelectedValue; //tag stores the corresponding themeId
            bool themeIncludesFont = (bool)chkIncludeFont.IsChecked;

            main.setTheme(newThemeId, themeIncludesFont);
            applyTheme(newThemeId, themeIncludesFont);
            toggleVisibility(false);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            main.setTheme(previouslySelectedThemeId, settings.themeWithFont);
            toggleVisibility(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main.setTheme(previouslySelectedThemeId, settings.themeWithFont);
            log.addLog("Close ThemeSelector");
        }
    }
}
