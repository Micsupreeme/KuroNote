﻿using System;
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
            "Morning Dew",
            "Wooden",
            "Leafage"
        };
        private static readonly string[] THEME_DESCS = { 
            "Classic", 
            "(Image by Pixabay)",
            "(Image by FWStudio on Pexels)",
            "(Image by Karolina Grabowska on Pexels)"
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
                case "Morning Dew":
                    tbThemeDesc.Text = THEME_DESCS[1];
                    break;
                case "Wooden":
                    tbThemeDesc.Text = THEME_DESCS[2];
                    break;
                case "Leafage":
                    tbThemeDesc.Text = THEME_DESCS[3];
                    break;
            }
            main.setTheme(cmbTheme.SelectedValue.ToString(), (bool)chkIncludeFont.IsChecked);
        }

        /// <summary>
        /// Update the theme preview to include/omit the theme font when the checkbox state is changed
        /// </summary>
        private void chkIncludeFont_CheckChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isChecked = (bool)chkIncludeFont.IsChecked;
                if (!isChecked) { //User doesn't want the theme font
                    //Apply the user's currently saved font instead of the theme font
                    main.setFont(settings.fontFamily, (short)settings.fontSize, settings.fontWeight, settings.fontStyle);
                }
                main.setTheme(cmbTheme.SelectedValue.ToString(), isChecked);
            } catch(Exception ex) {
                Console.Error.Write(ex.ToString()); //Attempted to apply theme before the combobox is initialised
            }
        }

        /// <summary>
        /// Update theme preferences to the specified theme name
        /// </summary>
        /// <param name="_selectedTheme">The theme name to change the selectedTheme setting to</param>
        /// <param name="_includesFont">Whether or not to use the font that comes with the theme</param>
        private void applyTheme(string _selectedTheme, bool _includesFont) {
            log.addLog("Updating theme preference: " + _selectedTheme);

            settings.themeName = _selectedTheme;
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
            string newThemeName = cmbTheme.SelectedValue.ToString();
            bool themeIncludesFont = (bool)chkIncludeFont.IsChecked;

            main.setTheme(newThemeName, themeIncludesFont);
            applyTheme(newThemeName, themeIncludesFont);
            toggleVisibility(false);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            main.setTheme(previouslySelectedTheme, settings.themeWithFont);
            toggleVisibility(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main.setTheme(previouslySelectedTheme, settings.themeWithFont);
            log.addLog("Close ThemeSelector");
        }
    }
}
