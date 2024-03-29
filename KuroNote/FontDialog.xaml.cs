﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Font";
        private static readonly short[] FONT_SIZES = { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        private const int MAX_PREVIEW_FONT_SIZE = 80;
        private const string NO_PREVIEW_TEXT = "Cannot preview at specified font size";
        private const int NO_PREVIEW_FONT_SIZE = 17;

        //Gamification constants
        private const int AP_CHANGE_FONT = 15;

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        CustomThemeManager customThemeManager = null;
        KuroNoteCustomTheme customThemeObject = null;

        private string selectedFontFamily;
        private FontWeight selectedFontWeight;
        private FontStyle selectedFontStyle;
        private short selectedFontSize;

        public FontDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog, CustomThemeManager _customThemeManager = null, KuroNoteCustomTheme _customThemeObject = null)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            if (_customThemeManager != null && _customThemeObject != null) {
                customThemeManager = _customThemeManager;
                customThemeObject = _customThemeObject;
            }
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;

            if (customThemeManager != null & customThemeObject != null) {
                log.addLog("FontDialog called from CustomThemeManager");
            }
            saveCurrentRtbFontSize();
            loadFontFamilies();
            loadFontStyles();
            loadFontSizes();
            log.addLog("Font lists loaded");
            updateFont();
        }

        /// <summary>
        /// Remember exact font size if "remember font up/down" option is enabled
        /// And it's not a theme font
        /// </summary>
        private void saveCurrentRtbFontSize() {
            if (settings.rememberFontUpDn && !settings.themeWithFont) {
                settings.fontSize = (int)main.MainRtb.FontSize;
                settings.UpdateSettings();
            }
        }

        /// <summary>
        /// Populate the Font Families list with the installed system fonts
        /// </summary>
        private void loadFontFamilies()
        {
            lisFontFamilies.Items.Clear();
            foreach (FontFamily ff in Fonts.SystemFontFamilies) {
                ListBoxItem item = new ListBoxItem();
                item.Content = ff.Source;
                item.FontFamily = ff;

                if (customThemeManager != null & customThemeObject != null) {
                    //Select the font family that is currently in the custom theme object
                    if (item.Content.Equals(customThemeObject.fontFamily)) {
                        log.addLog("Selected family: " + item.Content);
                        txtFontFamily.Text = (string)item.Content;
                        item.IsSelected = true;
                    }
                } else {
                    //Select the font family that is currently in settings
                    if (item.Content.Equals(settings.fontFamily)) {
                        log.addLog("Selected family: " + item.Content);
                        txtFontFamily.Text = (string)item.Content;
                        item.IsSelected = true;
                    }
                }

                lisFontFamilies.Items.Add(item);
            }
        }

        /// <summary>
        /// Populate the font styles list with the preset font styles
        /// </summary>
        private void loadFontStyles()
        {
            lisFontStyles.Items.Clear();
            ListBoxItem lbiNormal = new ListBoxItem();
            lbiNormal.Content = "Normal";
            ListBoxItem lbiBold = new ListBoxItem();
            lbiBold.Content = "Bold";
            lbiBold.FontWeight = FontWeights.Bold;
            ListBoxItem lbiItalic = new ListBoxItem();
            lbiItalic.Content = "Italic";
            lbiItalic.FontStyle = FontStyles.Italic;
            ListBoxItem lbiBoldItalic = new ListBoxItem();
            lbiBoldItalic.Content = "Bold Italic";
            lbiBoldItalic.FontWeight = FontWeights.Bold;
            lbiBoldItalic.FontStyle = FontStyles.Italic;

            if (customThemeManager != null & customThemeObject != null) {
                //Select the font style that is currently in the custom theme object
                if (customThemeObject.fontWeight == FontWeights.Bold) {
                    if (customThemeObject.fontStyle == FontStyles.Italic) {
                        log.addLog("Selected Style: " + lbiBoldItalic.Content);
                        txtFontStyle.Text = (string)lbiBoldItalic.Content;
                        lbiBoldItalic.IsSelected = true;
                    } else {
                        log.addLog("Selected Style: " + lbiBold.Content);
                        txtFontStyle.Text = (string)lbiBold.Content;
                        lbiBold.IsSelected = true;
                    }
                } else {
                    if (customThemeObject.fontStyle == FontStyles.Italic) {
                        log.addLog("Selected Style: " + lbiItalic.Content);
                        txtFontStyle.Text = (string)lbiItalic.Content;
                        lbiItalic.IsSelected = true;
                    } else {
                        log.addLog("Selected Style: " + lbiNormal.Content);
                        txtFontStyle.Text = (string)lbiNormal.Content;
                        lbiNormal.IsSelected = true;
                    }
                }
            } else {
                //Select the font style that is currently in settings
                if (settings.fontWeight == FontWeights.Bold) {
                    if (settings.fontStyle == FontStyles.Italic) {
                        log.addLog("Selected Style: " + lbiBoldItalic.Content);
                        txtFontStyle.Text = (string)lbiBoldItalic.Content;
                        lbiBoldItalic.IsSelected = true;
                    } else {
                        log.addLog("Selected Style: " + lbiBold.Content);
                        txtFontStyle.Text = (string)lbiBold.Content;
                        lbiBold.IsSelected = true;
                    }
                } else {
                    if (settings.fontStyle == FontStyles.Italic) {
                        log.addLog("Selected Style: " + lbiItalic.Content);
                        txtFontStyle.Text = (string)lbiItalic.Content;
                        lbiItalic.IsSelected = true;
                    } else {
                        log.addLog("Selected Style: " + lbiNormal.Content);
                        txtFontStyle.Text = (string)lbiNormal.Content;
                        lbiNormal.IsSelected = true;
                    }
                }
            }

            lisFontStyles.Items.Add(lbiNormal);
            lisFontStyles.Items.Add(lbiBold);
            lisFontStyles.Items.Add(lbiItalic);
            lisFontStyles.Items.Add(lbiBoldItalic);
        }

        /// <summary>
        /// Populate the font sizes list with the preset font sizes
        /// </summary>
        private void loadFontSizes()
        {
            lisFontSizes.Items.Clear();

            foreach (short size in FONT_SIZES) {
                ListBoxItem item = new ListBoxItem();
                item.Content = size;

                if (customThemeManager != null & customThemeObject != null) {
                    //Select the font size that is currently in custom theme object
                    if (size == customThemeObject.fontSize) {
                        //log.addLog("Selected Size: " + item.Content);
                        //txtFontSize.Text = (short)item.Content + "";
                        item.IsSelected = true;
                        selectedFontSize = size;
                    }
                } else {
                    //Select the font size that is currently in settings
                    if (size == settings.fontSize) {
                        //log.addLog("Selected Size: " + item.Content);
                        //txtFontSize.Text = (short)item.Content + "";
                        item.IsSelected = true;
                        selectedFontSize = size;
                    }
                }

                lisFontSizes.Items.Add(item);
            }

            //txtFontSize now displays the actual font size being used
            //even when it's a non-standard size
            log.addLog("Selected Size: " + (short)main.MainRtb.FontSize);
            txtFontSize.Text = (short)main.MainRtb.FontSize + "";
        }

        /// <summary>
        /// Update font preferences according to the current field selection
        /// </summary>
        private void updateFont()
        {
            log.addLog("Updating Font Preview");
            //Get values from field selection
            selectedFontFamily = txtFontFamily.Text;
            switch (txtFontStyle.Text)
            {
                case "Regular":
                    selectedFontWeight = FontWeights.Normal;
                    selectedFontStyle = FontStyles.Normal;
                    break;
                case "Bold":
                    selectedFontWeight = FontWeights.Bold;
                    selectedFontStyle = FontStyles.Normal;
                    break;
                case "Italic":
                    selectedFontWeight = FontWeights.Normal;
                    selectedFontStyle = FontStyles.Italic;
                    break;
                case "Bold Italic":
                    selectedFontWeight = FontWeights.Bold;
                    selectedFontStyle = FontStyles.Italic;
                    break;
                default:
                    selectedFontWeight = FontWeights.Normal;
                    selectedFontStyle = FontStyles.Normal;
                    break;
            }
            
            try {
                if (!txtFontSize.Text.Equals(String.Empty)) {
                    selectedFontSize = short.Parse(txtFontSize.Text);
                } 
            } catch (FormatException e) {
                MessageBox.Show(e.ToString());
            }

            //Apply the values to the preview
            txtFontPreview.FontWeight = selectedFontWeight;
            txtFontPreview.FontStyle = selectedFontStyle;
            if (!selectedFontFamily.Equals(String.Empty)) {
                txtFontPreview.FontFamily = new FontFamily(selectedFontFamily);
            }
            if (selectedFontSize > 0 && selectedFontSize <= MAX_PREVIEW_FONT_SIZE) {
                txtFontPreview.FontSize = selectedFontSize;
                txtFontPreview.Text = appName;
            } else {
                txtFontPreview.FontSize = NO_PREVIEW_FONT_SIZE;
                txtFontPreview.Text = NO_PREVIEW_TEXT;
            }
        }

        /// <summary>
        /// Change the font in the main rtb to the currently selected font, and save the changes
        /// </summary>
        private void applyFont()
        {
            if (selectedFontFamily.Equals("Wingdings")) {
                main.unlockAchievement(14);
            }
            if (customThemeManager != null & customThemeObject != null) {
                //apply for CustomThemeManager
                log.addLog("Sending [" + selectedFontFamily + " (" + txtFontStyle.Text + ") " + selectedFontSize + "] to CustomThemeManager");
                customThemeManager.setFont(selectedFontFamily, selectedFontSize, selectedFontWeight, selectedFontStyle);
            } else {
                //apply for MainWindow
                log.addLog("Applying font: " + selectedFontFamily + " (" + txtFontStyle.Text + ") " + selectedFontSize);
                main.setFont(selectedFontFamily, selectedFontSize, selectedFontWeight, selectedFontStyle);

                settings.fontFamily = selectedFontFamily;
                settings.fontSize = selectedFontSize;
                settings.fontWeight = selectedFontWeight;
                settings.fontStyle = selectedFontStyle;
                settings.themeWithFont = false; //user just overrided theme font
                settings.UpdateSettings(); //Write these changes to the file
            }
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open FontDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse FontDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// When the selection for any of the 3 lists changes
        /// </summary>
        private void lis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //txtFontSize is updated programatically instead of bound to lisFontSizes
            //because it has to display both preset font values and non-standard values
            try {
                txtFontSize.Text = lisFontSizes.SelectedValue.ToString();
            } catch (NullReferenceException) {
                log.addLog("WARN: lis_SelectionChanged Event fired before object initialisation");
            }

            updateFont();
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            applyFont();
            main.incrementAp(AP_CHANGE_FONT);
            toggleVisibility(false);
        }

        /// <summary>
        /// When the user clicks "Cancel" or hits ESC
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close FontDialog");
        }
    }
}
