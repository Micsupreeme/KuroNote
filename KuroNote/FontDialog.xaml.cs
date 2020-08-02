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
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Font";
        private static readonly short[] FONT_SIZES = { 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        private string selectedFontFamily;
        private FontWeight selectedFontWeight;
        private FontStyle selectedFontStyle;
        private short selectedFontSize;

        public FontDialog(MainWindow mainWin, KuroNoteSettings currentSettings)
        {
            InitializeComponent();
            this.Visibility = Visibility.Visible;
            main = mainWin;
            settings = currentSettings;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;

            loadFontFamilies();
            loadFontStyles();
            loadFontSizes();
        }

        /// <summary>
        /// Populate the Font Families list with the installed system fonts
        /// </summary>
        private void loadFontFamilies()
        {
            lisFontFamilies.Items.Clear();
            foreach (FontFamily ff in Fonts.SystemFontFamilies)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = ff.Source;
                item.FontFamily = ff;

                //Select the font family that is currently in settings
                if (item.Content.Equals(settings.fontFamily))
                {
                    item.IsSelected = true;
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

            //Select the font style that is currently in settings
            if(settings.fontWeight == FontWeights.Bold){
                if(settings.fontStyle == FontStyles.Italic)
                {
                    lbiBoldItalic.IsSelected = true;
                } else {
                    lbiBold.IsSelected = true;
                } 
            } else {
                if(settings.fontStyle == FontStyles.Italic)
                {
                    lbiItalic.IsSelected = true;
                } else {
                    lbiNormal.IsSelected = true;
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
            foreach (short size in FONT_SIZES)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = size;

                //Select the font size that is currently in settings
                if (size == settings.fontSize)
                {
                    item.IsSelected = true;
                    selectedFontSize = size;
                }

                lisFontSizes.Items.Add(item);
            }
        }

        /// <summary>
        /// Update font preferences according to the current field selection
        /// </summary>
        private void updateFont()
        {
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

            try
            {
                if(!txtFontSize.Text.Equals(String.Empty))
                {
                    selectedFontSize = short.Parse(txtFontSize.Text);
                } 
            }
            catch (FormatException e)
            {
                MessageBox.Show(e.ToString());
            }

            //Apply the values to the preview
            lblFontPreview.FontWeight = selectedFontWeight;
            lblFontPreview.FontStyle = selectedFontStyle;
            if (!selectedFontFamily.Equals(String.Empty))
            {
                lblFontPreview.FontFamily = new FontFamily(selectedFontFamily);
            }
            if (selectedFontSize > 0)
            {
                lblFontPreview.FontSize = selectedFontSize;
                lblFontPreview.Content = appName;
            }
        }

        /// <summary>
        /// Change the font in the main rtb to the currently selected font, and save the changes
        /// </summary>
        private void applyFont()
        {
            main.setFont(selectedFontFamily, selectedFontSize, selectedFontWeight, selectedFontStyle);

            settings.fontFamily = selectedFontFamily;
            settings.fontSize = selectedFontSize;
            settings.fontWeight = selectedFontWeight;
            settings.fontStyle = selectedFontStyle;
            settings.UpdateSettings(); //Write these changes to the file
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis)
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// When the selection for any of the 3 lists changes
        /// </summary>
        private void lis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateFont();
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            applyFont();
            toggleVisibility(false);
        }

        /// <summary>
        /// When the user clicks "Cancel" or hits ESC
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }
    }
}
