using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace KuroNote
{
    //TODO: confirmation for deleting themes
    //TODO: disable all customisation controls while no theme selected or theme just deleted

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
        }

        private void loadCustomThemeList()
        {
            cmbCustomTheme.Items.Clear();
            try {
                string[] customThemeFiles = Directory.GetFiles(customThemePath);

                //Go through the array backwards when adding ComboBoxItems, so that the ComboBox is ordered "Newest themes descending"
                for (int i = (customThemeFiles.Length - 1); i >= 0; i--)
                {
                    //Only list .kurotheme files as selectable theme files
                    if (customThemeFiles[i].Contains(CUSTOM_THEME_EXT))
                    {
                        int customThemeId;
                        string customThemeName;

                        using (StreamReader sr = new StreamReader(customThemeFiles[i]))
                        {
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
            OpenFileDialog ofd = new OpenFileDialog() {
                Filter = BGIMAGE_FILE_FILTER,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            if (ofd.ShowDialog() == true) {
                imageBrowseTxt.Text = ofd.FileName;

                //copy specified image to custom theme directory
                string newFilePath = customThemePath + currentTheme.themeId + INTERNAL_IMAGE_EXT;
                try {
                    File.Copy(ofd.FileName, newFilePath, true); //copy the specified image to the custom theme directory and overwrite the destination if needed
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
                themeId, "Custom theme", "", true,
                "#FFFFFFFF", 0.25,
                "#FFFFFFFF",
                "#FFF0F0F0",
                "#FFF0F0F0",
                "#FF000000",
                "Consolas", 18, FontWeights.Regular, FontStyles.Normal
            );
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
            try
            {
                string themeFileName = customThemePath + themeId + CUSTOM_THEME_EXT;

                log.addLog("Reading " + themeFileName);
                using (StreamReader sr = new StreamReader(themeFileName))
                {
                    string json = sr.ReadToEnd();
                    KuroNoteCustomTheme kntFile = JsonConvert.DeserializeObject<KuroNoteCustomTheme>(json);

                    //load image theme
                    currentTheme = new KuroNoteCustomTheme
                    (
                        kntFile.themeId, kntFile.themeName, kntFile.themeDesc, kntFile.hasImage,
                        kntFile.bgBrush.ToString(), kntFile.imgBrushOpacity,
                        kntFile.solidBrush.ToString(),
                        kntFile.menuBrush.ToString(),
                        kntFile.statusBrush.ToString(),
                        kntFile.textBrush.ToString(),
                        kntFile.fontFamily, 18, FontWeights.Regular, FontStyles.Normal
                    );

                    log.addLog("Custom theme file successfully loaded");
                    populateFields();
                }
            }
            catch (Exception e)
            {
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Set UI values according to the values in the currentTheme object
        /// </summary>
        public void populateFields()
        {
            //in JSON format, colour values are stored as hex, convert them to ARGB so we can create new SolidColorBrush instances
            byte[] bgBrushArgb = getARGBFromHex(currentTheme.bgBrush.ToString());
            byte[] solidBrushArgb = getARGBFromHex(currentTheme.solidBrush.ToString());
            byte[] menuBrushArgb = getARGBFromHex(currentTheme.menuBrush.ToString());
            byte[] statusBrushArgb = getARGBFromHex(currentTheme.statusBrush.ToString());
            byte[] textBrushArgb = getARGBFromHex(currentTheme.textBrush.ToString());

            themeNameTxt.Text = currentTheme.themeName;
            themeDescTxt.Text = currentTheme.themeDesc;
            bgBrushCol.SetColor(Color.FromRgb(bgBrushArgb[1], bgBrushArgb[2], bgBrushArgb[3]));
            menuBrushCol.SetColor(Color.FromRgb(menuBrushArgb[1], menuBrushArgb[2], menuBrushArgb[3]));
            statusBrushCol.SetColor(Color.FromRgb(statusBrushArgb[1], statusBrushArgb[2], statusBrushArgb[3]));
            imageOpacitySlide.Value = currentTheme.imgBrushOpacity;
            solidBrushCol.SetColor(Color.FromRgb(solidBrushArgb[1], solidBrushArgb[2], solidBrushArgb[3]));
            textBrushCol.SetColor(Color.FromRgb(textBrushArgb[1], textBrushArgb[2], textBrushArgb[3]));
            fontTxt.Text = currentTheme.fontFamily;

            //toggleImageSolidUI(currentTheme.hasImage);
            if(currentTheme.hasImage) {

                //Show confirmation in the browse text box that an image has actually been set
                string internalImagePath = customThemePath + currentTheme.themeId + INTERNAL_IMAGE_EXT;
                if(File.Exists(internalImagePath)) {
                    //Image has already been set
                    imageBrowseTxt.Text = internalImagePath;
                } else {
                    //Image has never been set for this theme
                    imageBrowseTxt.Text = "Not set";
                }

                imageBgRadio.IsChecked = true;
                solidBgRadio.IsChecked = false;
            } else {
                imageBgRadio.IsChecked = false;
                solidBgRadio.IsChecked = true;
            }
        }

        /// <summary>
        /// Update currentTheme's theme file with values from the object
        /// </summary>
        public void updateThemeFile()
        {
            try
            {
                string themeFileName = customThemePath + currentTheme.themeId + CUSTOM_THEME_EXT;

                log.addLog("Updating " + themeFileName);
                using (StreamWriter sw = new StreamWriter(themeFileName))
                {
                    string json = JsonConvert.SerializeObject(currentTheme, Formatting.Indented);

                    sw.Write(json);
                    log.addLog("Custom theme file successfully updated");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Updates currentTheme's theme object with values from the UI fields
        /// </summary>
        private void updateThemeObject()
        {
            currentTheme.themeName = themeNameTxt.Text;
            currentTheme.themeDesc = themeDescTxt.Text;
            currentTheme.bgBrush = bgBrushCol.Color.ToString();
            currentTheme.menuBrush = menuBrushCol.Color.ToString();
            currentTheme.statusBrush = statusBrushCol.Color.ToString();
            currentTheme.imgBrushOpacity = Math.Round(imageOpacitySlide.Value, 2);
            currentTheme.solidBrush = solidBrushCol.Color.ToString();
            currentTheme.textBrush = textBrushCol.Color.ToString();
            currentTheme.fontFamily = fontTxt.Text;
        }

        /// <summary>
        /// Deletes the theme file associated with currentTheme
        /// and attempts to delete an internal image associated with currentTheme
        /// </summary>
        private void deleteThemeFile()
        {
            try {
                File.Delete(customThemePath + currentTheme.themeId + CUSTOM_THEME_EXT);
                File.Delete(customThemePath + currentTheme.themeId + INTERNAL_IMAGE_EXT);
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
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            updateThemeObject();
            updateThemeFile();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            deleteThemeFile();
            loadCustomThemeList(); //refresh custom theme list because we've removed a new file from the custom theme directory
        }

        private void imageBgRadio_Checked(object sender, RoutedEventArgs e)
        {
            toggleImageSolidUI(true);
        }

        private void solidBgRadio_Checked(object sender, RoutedEventArgs e)
        {
            toggleImageSolidUI(false);
        }

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
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Radio_Checked Event fired before object initialisation ");
            }
        }

        private void bgBrushCol_OnPick(object sender, EventArgs e)
        {

        }

        private void menuBrushCol_OnPick(object sender, EventArgs e)
        {

        }

        private void statusBrushCol_OnPick(object sender, EventArgs e)
        {

        }

        private void imageBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            selectBackgroundImage();
        }

        private void solidBrushCol_OnPick(object sender, EventArgs e)
        {

        }

        private void imageOpacitySlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try {
                imageOpacityLbl.Content = "Opacity: " + Math.Round(imageOpacitySlide.Value, 2);
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Radio_Checked Event fired before object initialisation ");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close Custom Theme Manager");
        }
    }
}
