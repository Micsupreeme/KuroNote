using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    //TODO (optional): change "Auto-Preview" visibility icon to eye with line through it and back to regular eye depending on toggle button state
    //TODO: warning message when an image source greater than 2MB (1MB advised) is selected for an image background - it will be slower to switch to the theme and slower to launch KuroNote

    //VERY OBSCURE KNOWN BUG: If you open ThemeSelector and CustomThemeManager at the same time, open a theme in CustomThemeManager,
    //  close ThemeSelector (triggers revert back theme change), then change the opacity of the image in CustomThemeManager
    //  (does a partial preview re-render- only changed background, not the menu/status), the custom theme preview will have
    //  the SolidColorBrushes of the currently set theme in settings instead of the custom theme being edited
    //  - until you change something that causes a full preview refresh (e.g. a SolidColorBrush)

    /// <summary>
    /// Interaction logic for CustomThemeManager.xaml
    /// </summary>
    public partial class CustomThemeManager : Window
    {
        //Constants
        private const string WINDOW_NAME = "Custom Themes";
        private const string CUSTOM_THEME_EXT = ".kurotheme";
        private const string INTERNAL_IMAGE_EXT = ".jpg";
        private const string BGIMAGE_FILE_FILTER =
            "JPEG (*.jpg;*.jpeg;*.jpe;*.jfif)|*.jpg;*.jpeg;*.jpe;*.jfif";

        //Globals
        private string appName, appPath;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        bool fieldsPopulated = false;
        bool newImageSet = false;

        private string customThemePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\CustomThemes\\";
        private KuroNoteCustomTheme currentTheme;

        public CustomThemeManager(MainWindow _mainWin, KuroNoteSettings _settings, Log _log)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _settings;
            log = _log;
            appName = main.appName;
            appPath = main.appPath;
            this.Title = WINDOW_NAME + " - " + appName;

            loadCustomThemeList();
            toggleCustomiseUI(false); //customise controls require a theme to be loaded first
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open CustomThemeManager");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse CustomThemeManager");
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void loadCustomThemeList()
        {
            cmbCustomTheme.Items.Clear();
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
                        cmbCustomTheme.Items.Add(customThemeItem);
                    }
                }
            } catch (Exception e) {
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Use an open file dialog to select an image file to use as an image background,
        /// </summary>
        private void selectBackgroundImage()
        {
            migrateThemeToNewId();

            OpenFileDialog ofd = new OpenFileDialog() {
                Filter = BGIMAGE_FILE_FILTER,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            if (ofd.ShowDialog() == true) {
                imageBrowseTxt.Text = ofd.FileName;

                //copy specified image to custom theme directory
                string newFilePath = customThemePath + currentTheme.themeId + INTERNAL_IMAGE_EXT;
                try {
                    File.Copy(ofd.FileName, newFilePath, true); //copy the specified image to the custom theme directory with the new themeId as the name
                    newImageSet = true;                         //trigger preview update on next mouse movement
                } catch (Exception e) {
                    MessageBox.Show(e.ToString(), "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    log.addLog(e.ToString());
                }
            }
        }

        /// <summary>
        /// Generates a new unique custom theme ID - the index stored in settings ensures no duplicate IDs
        /// </summary>
        /// <returns>A new unique custom theme ID</returns>
        private int generateNewThemeId()
        {
            settings.RetrieveSettings();
            int newThemeId = settings.customThemeIndex;
            settings.customThemeIndex++;
            settings.UpdateSettings();
            return newThemeId;
        }

        /// <summary>
        /// Generates a new theme object with default values
        /// </summary>
        private void generateNewTheme()
        {
            int themeId = generateNewThemeId();

            currentTheme = new KuroNoteCustomTheme
            (
                themeId, "Custom theme", true,
                "#FFFFFFFF", 0.25,
                "#FFFFFFFF",
                "#FFF0F0F0",
                "#FFF0F0F0",
                "#FF000000",
                "Verdana", 18, FontWeights.Regular, FontStyles.Normal
            );
        }

        /// <summary>
        /// Unable to replace internal image file (used by another process) workaround for changing image after it has already been set
        /// </summary>
        private void migrateThemeToNewId()
        {
            string fileToMigrate = customThemePath + currentTheme.themeId + CUSTOM_THEME_EXT;          
            KuroNoteCustomTheme oldThemeObject = currentTheme;
            int newThemeId = generateNewThemeId();

            currentTheme = new KuroNoteCustomTheme
            (
                newThemeId, oldThemeObject.themeName, true,
                oldThemeObject.bgBrush, oldThemeObject.imgBrushOpacity,
                oldThemeObject.solidBrush,
                oldThemeObject.menuBrush,
                oldThemeObject.statusBrush,
                oldThemeObject.textBrush,
                oldThemeObject.fontFamily, oldThemeObject.fontSize, oldThemeObject.fontWeight, oldThemeObject.fontStyle
            );

            updateThemeFile();
            loadCustomThemeList(); //refresh custom theme list because we've added a new file to the custom theme directory
            loadThemeObjectFromFile(newThemeId); //the theme that was just created
            if (tbtnAutoPreview.IsChecked == true) {
                main.setTheme(currentTheme.themeId, true); //always auto-preview with font
            }

            //delete old file
            File.Delete(fileToMigrate);
            //delete the combo box listing for the old id
            for (int i = 0; i < cmbCustomTheme.Items.Count; i++) {
                ComboBoxItem cmbItem = (ComboBoxItem)cmbCustomTheme.Items.GetItemAt(i);
                if ((int)cmbItem.Tag == oldThemeObject.themeId) {
                    cmbCustomTheme.Items.RemoveAt(i);
                }
            }
        }

        public void setFont(String _fontFamily, short _fontSize, FontWeight _fontWeight, FontStyle _fontStyle)
        {
            //update theme object here because these values are not yet stored in fields
            currentTheme.fontFamily = _fontFamily;
            currentTheme.fontSize = _fontSize;
            currentTheme.fontWeight = _fontWeight;
            currentTheme.fontStyle = _fontStyle;
            updateThemeFile();
            fontTxt.Text = currentTheme.fontFamily + ", " + currentTheme.fontSize + "pt (W: " + currentTheme.fontWeight + ", S: " + currentTheme.fontStyle + ")";

            if (tbtnAutoPreview.IsChecked == true) {
                main.setTheme(currentTheme.themeId, true); //always auto-preview with font
            }
        }

        /// <summary>
        /// Converts hex colour value strings to ARGB numbers
        /// </summary>
        /// <param name="colorHexIncludingHash">hex "#AARRGGBB" string value to convert to ARGB byte array</param>
        /// <returns>ARGB numbers in an array of bytes</returns>
        public byte[] getARGBFromHex(string colorHexIncludingHash)
        {
            byte[] argbValues = new byte[4];
            argbValues[0] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(1, 2), 16);
            argbValues[1] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(3, 2), 16);
            argbValues[2] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(5, 2), 16);
            argbValues[3] = (byte)Convert.ToInt64(colorHexIncludingHash.Substring(7, 2), 16);
            return argbValues;
        }

        /// <summary>
        /// Load values stored in a specified theme file into the currentTheme object
        /// </summary>
        /// <param name="themeId">The custom theme ID to load</param>
        public void loadThemeObjectFromFile(int themeId)
        {
            try {
                string themeFileName = customThemePath + themeId + CUSTOM_THEME_EXT;

                log.addLog("Reading " + themeFileName);
                using (StreamReader sr = new StreamReader(themeFileName)) {
                    string json = sr.ReadToEnd();
                    KuroNoteCustomTheme kntFile = JsonConvert.DeserializeObject<KuroNoteCustomTheme>(json);

                    //load image theme
                    currentTheme = new KuroNoteCustomTheme
                    (
                        kntFile.themeId, kntFile.themeName, kntFile.hasImage,
                        kntFile.bgBrush.ToString(), kntFile.imgBrushOpacity,
                        kntFile.solidBrush.ToString(),
                        kntFile.menuBrush.ToString(),
                        kntFile.statusBrush.ToString(),
                        kntFile.textBrush.ToString(),
                        kntFile.fontFamily, kntFile.fontSize, kntFile.fontWeight, kntFile.fontStyle
                    );

                    log.addLog("Custom theme file successfully loaded");
                    toggleCustomiseUI(true);
                    populateFields();
                }
            } catch (Exception e) {
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Set UI values according to the values in the currentTheme object
        /// </summary>
        public void populateFields()
        {
            fieldsPopulated = false;
            //in JSON format, colour values are stored as hex, convert them to ARGB so we can create new SolidColorBrush instances
            byte[] bgBrushArgb = getARGBFromHex(currentTheme.bgBrush.ToString());
            byte[] solidBrushArgb = getARGBFromHex(currentTheme.solidBrush.ToString());
            byte[] menuBrushArgb = getARGBFromHex(currentTheme.menuBrush.ToString());
            byte[] statusBrushArgb = getARGBFromHex(currentTheme.statusBrush.ToString());
            byte[] textBrushArgb = getARGBFromHex(currentTheme.textBrush.ToString());

            themeNameTxt.Text = currentTheme.themeName;
            fontTxt.Text = currentTheme.fontFamily + ", " + currentTheme.fontSize + "pt (W: " + currentTheme.fontWeight + ", S: " + currentTheme.fontStyle + ")";
            bgBrushCol.SetColor(Color.FromArgb(bgBrushArgb[0], bgBrushArgb[1], bgBrushArgb[2], bgBrushArgb[3]));
            menuBrushCol.SetColor(Color.FromArgb(menuBrushArgb[0], menuBrushArgb[1], menuBrushArgb[2], menuBrushArgb[3]));
            statusBrushCol.SetColor(Color.FromArgb(statusBrushArgb[0], statusBrushArgb[1], statusBrushArgb[2], statusBrushArgb[3]));
            imageOpacitySlide.Value = currentTheme.imgBrushOpacity;
            solidBrushCol.SetColor(Color.FromArgb(solidBrushArgb[0], solidBrushArgb[1], solidBrushArgb[2], solidBrushArgb[3]));
            textBrushCol.SetColor(Color.FromArgb(textBrushArgb[0], textBrushArgb[1], textBrushArgb[2], textBrushArgb[3]));

            //toggleImageSolidUI(currentTheme.hasImage);
            imageBrowseTxt.Text = string.Empty;
            if (currentTheme.hasImage) {

                //Show confirmation in the browse text box that an image has actually been set
                string internalImagePath = customThemePath + currentTheme.themeId + INTERNAL_IMAGE_EXT;
                if (File.Exists(internalImagePath)) {
                    //Image has already been set
                    imageBrowseTxt.Text = internalImagePath;
                }

                imageBgRadio.IsChecked = true;
                solidBgRadio.IsChecked = false;
            } else {
                imageBgRadio.IsChecked = false;
                solidBgRadio.IsChecked = true;
            }
            fieldsPopulated = true;
        }

        /// <summary>
        /// Update currentTheme's theme file with values from the object
        /// </summary>
        public void updateThemeFile()
        {
            try {
                string themeFileName = customThemePath + currentTheme.themeId + CUSTOM_THEME_EXT;

                using (StreamWriter sw = new StreamWriter(themeFileName)) {
                    string json = JsonConvert.SerializeObject(currentTheme, Formatting.Indented);

                    sw.Write(json);
                    log.addLog("Custom theme file for theme" + currentTheme.themeId + " successfully updated");
                }
            } catch (Exception e) {
                log.addLog("WARN: " + e.GetType().ToString() + " occurred while updating theme file");
            }
        }

        /// <summary>
        /// Updates currentTheme's theme object with values from the UI fields
        /// </summary>
        private void updateThemeObject()
        {
            currentTheme.themeName = themeNameTxt.Text;
            currentTheme.bgBrush = bgBrushCol.Color.ToString();
            currentTheme.menuBrush = menuBrushCol.Color.ToString();
            currentTheme.statusBrush = statusBrushCol.Color.ToString();
            currentTheme.imgBrushOpacity = Math.Round(imageOpacitySlide.Value, 2);
            currentTheme.solidBrush = solidBrushCol.Color.ToString();
            currentTheme.textBrush = textBrushCol.Color.ToString();
            //font is updated by setFont()
        }

        /// <summary>
        /// Deletes the theme file associated with currentTheme
        /// and attempts to delete an internal image associated with currentTheme
        /// </summary>
        private void deleteThemeFile()
        {
            try {
                File.Delete(customThemePath + currentTheme.themeId + CUSTOM_THEME_EXT);
            } catch (Exception e) {
                MessageBox.Show(e.ToString(), "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.addLog(e.ToString());
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            generateNewTheme();
            updateThemeFile();
            loadCustomThemeList(); //refresh custom theme list because we've added a new file to the custom theme directory
            loadThemeObjectFromFile(settings.customThemeIndex - 1); //the theme that was just created
            if (tbtnAutoPreview.IsChecked == true) {
                main.setTheme(currentTheme.themeId, true); //always auto-preview with font
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            updateThemeObject();
            updateThemeFile();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult conf = MessageBox.Show("Are you sure you want to delete \"" + currentTheme.themeName + "\"?", "Delete custom theme?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (conf == MessageBoxResult.Yes) {
                deleteThemeFile();
                loadCustomThemeList();      //refresh custom theme list because we've removed a new file from the custom theme directory
                toggleCustomiseUI(false);   //the theme was just deleted so now there is no theme loaded
            }
        }

        private void imageBgRadio_Checked(object sender, RoutedEventArgs e)
        {
            toggleImageSolidUI(true);
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    if (tbtnAutoPreview.IsChecked == true) {
                        main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Radio_Checked Event fired before object initialisation ");
            }
        }

        private void solidBgRadio_Checked(object sender, RoutedEventArgs e)
        {
            toggleImageSolidUI(false);
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    if (tbtnAutoPreview.IsChecked == true) {
                        main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Radio_Checked Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// Hides or shows all of the UI elements that require a custom theme to be loaded first (which is most of them)
        /// </summary>
        /// <param name="enable">Whether or not to enable UI elements that require a loaded theme</param>
        private void toggleCustomiseUI(bool enable)
        {
            if (enable) {
                customiseGrid.Opacity = 1;
                customiseGrid.IsEnabled = true;
                btnDelete.IsEnabled = true;
            } else {
                customiseGrid.Opacity = 0.33;
                customiseGrid.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }
        }

        /// <summary>
        /// Hides the solid colour controls for custom themes with images
        /// or hides the image controls for custom themes without images
        /// </summary>
        /// <param name="hasImage">Whether or not the selected theme uses an image background</param>
        private void toggleImageSolidUI(bool hasImage)
        {
            try {
                if (hasImage) {
                    currentTheme.hasImage = true;
                    imageNoteLbl.Visibility = Visibility.Visible;
                    imageControlGrid.Visibility = Visibility.Visible;
                    imageOpacityLbl.Visibility = Visibility.Visible;
                    imageOpacitySlide.Visibility = Visibility.Visible;
                    solidBrushCol.Visibility = Visibility.Hidden;
                } else {
                    currentTheme.hasImage = false;
                    imageNoteLbl.Visibility = Visibility.Hidden;
                    imageControlGrid.Visibility = Visibility.Hidden;
                    imageOpacityLbl.Visibility = Visibility.Hidden;
                    imageOpacitySlide.Visibility = Visibility.Hidden;
                    solidBrushCol.Visibility = Visibility.Visible;
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Radio_Checked Event fired before object initialisation ");
            }
        }

        private void cmbCustomTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                loadThemeObjectFromFile((int)cmbCustomTheme.SelectedValue);
                if (tbtnAutoPreview.IsChecked == true) {
                    main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Radio_Checked Event fired before object initialisation ");
            }
        }

        private void bgBrushCol_OnPick(object sender, EventArgs e)
        {
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    if (tbtnAutoPreview.IsChecked == true)
                    {
                        main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: OnPick Event fired before object initialisation ");
            }
        }

        private void menuBrushCol_OnPick(object sender, EventArgs e)
        {
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    if (tbtnAutoPreview.IsChecked == true) {
                        main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: OnPick Event fired before object initialisation ");
            }
        }

        private void statusBrushCol_OnPick(object sender, EventArgs e)
        {
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    if (tbtnAutoPreview.IsChecked == true) {
                        main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: OnPick Event fired before object initialisation ");
            }
        }

        private void imageBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            selectBackgroundImage();
        }

        /// <summary>
        /// When the mouse moves anywhere in the content of this Window
        /// </summary>
        private void masterStack_MouseMove(object sender, MouseEventArgs e)
        {
            if (newImageSet) {
                log.addLog("test");
                newImageSet = false;
                if (tbtnAutoPreview.IsChecked == true) {
                    main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                }
            }
        }

        private void solidBrushCol_OnPick(object sender, EventArgs e)
        {
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    if (tbtnAutoPreview.IsChecked == true) {
                        main.setTheme(currentTheme.themeId, true); //always auto-preview with font
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: OnPick Event fired before object initialisation ");
            }
        }

        private void imageOpacitySlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try {
                imageOpacityLbl.Content = "Opacity: " + Math.Round(imageOpacitySlide.Value, 2);
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                    //Only attempt to render a preview if the user has chosen auto-preview AND there is an image set for the ImageBrush
                    if (tbtnAutoPreview.IsChecked == true && imageBrowseTxt.Text.Length > 0) {
                        //A full re-render preview every time the opacity value changes is too much, this is a minimal version for opacity changes only
                        try {
                            main.MainRtb.Background = new ImageBrush
                            {
                                ImageSource = new BitmapImage(new Uri(customThemePath + currentTheme.themeId + INTERNAL_IMAGE_EXT, UriKind.Absolute)),
                                Opacity = currentTheme.imgBrushOpacity
                            };
                        } catch (Exception ex) {
                            log.addLog("Custom theme opacity change lightweight preview failed");
                            log.addLog(ex.ToString());
                        }
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: ValueChanged Event fired before object initialisation ");
            }
        }

        private void themeNameTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                if (fieldsPopulated) {
                    updateThemeObject();
                    updateThemeFile();
                }
                //no need to update preview, themeName is metadata only
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: TextChanged Event fired before object initialisation ");
            }
        }

        private void fontBtn_Click(object sender, RoutedEventArgs e)
        {
            FontDialog fontDialog = new FontDialog(main, settings, log, this, currentTheme);
            fontDialog.toggleVisibility(true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main.setTheme(settings.themeId, settings.themeWithFont); //ensure that the theme selected in settings is restored when custom theme manager is closed
            log.addLog("Close Custom Theme Manager");
        }
    }
}
