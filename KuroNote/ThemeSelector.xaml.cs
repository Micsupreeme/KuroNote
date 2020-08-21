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
        private static readonly string[] THEME_NAMES = { 
            "Default",
            "Water" 
        };
        private static readonly string[] THEME_DESCS = { 
            "Classic", 
            "Fancy" 
        };

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        private string previouslySelectedTheme; //the theme to revert back to if the user clicks "Cancel" or "X"

        public ThemeSelector(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;

            previouslySelectedTheme = settings.themeName;
            loadThemes();
            //loadCustomThemes()
        }

        /// <summary>
        /// Populate the dropdown menu with the available built-in themes
        /// </summary>
        private void loadThemes()
        {
            cmbTheme.Items.Clear();
            foreach(string themeName in THEME_NAMES)
            {
                ComboBoxItem themeItem = new ComboBoxItem();
                themeItem.Content = themeName;
                if(themeName.Equals(settings.themeName)) {
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
            switch(cmbTheme.SelectedValue)
            {
                case "Default":
                    tbThemeDesc.Text = THEME_DESCS[0];
                    break;
                case "Water":
                    tbThemeDesc.Text = THEME_DESCS[1];
                    break;
            }
            main.setTheme(cmbTheme.SelectedValue.ToString());
        }

        /// <summary>
        /// Update theme preferences to the specified theme name
        /// </summary>
        /// <param name="_selectedTheme">The theme name to change the selectedTheme setting to</param>
        private void applyTheme(string _selectedTheme) {
            log.addLog("Updating theme preference: " + _selectedTheme);

            settings.themeName = _selectedTheme;
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
            string newThemeName = cmbTheme.SelectedValue.ToString();

            main.setTheme(newThemeName);
            applyTheme(newThemeName);
            toggleVisibility(false);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            main.setTheme(previouslySelectedTheme);
            toggleVisibility(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main.setTheme(previouslySelectedTheme);
            log.addLog("Close ThemeSelector");
        }
    }
}
