using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for AchievementListDialog.xaml
    /// </summary>
    public partial class AchievementListDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Achievements";

        //Globals
        private int ACHIEVEMENT_ROW_HEIGHT = 64;
        private byte[] DEFAULT_TEXT_COLOR = { 255, 238, 238, 238 };
        private byte[] DEFAULT_BACK_COLOR = { 255, 64, 64, 64 };

        StackPanel achFullStack;
        List<StackPanel> achRowStacks;

        private string appName;
        KuroNoteSettings settings;
        KuroNoteAchievement[] achievementCollection;
        MainWindow main;
        Log log;

        public AchievementListDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, KuroNoteAchievement[] _achievementCollection, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            achievementCollection = _achievementCollection;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            createDynamicControls();
        }

        /// <summary>
        /// Create dynamic controls to represent the current achievement list
        /// </summary>
        private void createDynamicControls()
        {
            //Ensure canvas is reset before drawing
            achCanvas.Children.Clear();
            achCanvas.Height = 0;
            int numberOfAchievements = achievementCollection.Length;
            achRowStacks = new List<StackPanel>();

            //Draw controls
            for (int ach = 0; ach < numberOfAchievements; ach++) {

                //Account for unnecessary top margin in the 1st row by adding (TopMargin) less height
                if (ach == numberOfAchievements - 1) {
                    achCanvas.Height += ACHIEVEMENT_ROW_HEIGHT;
                } else {
                    achCanvas.Height += ACHIEVEMENT_ROW_HEIGHT + 10;
                }

                //TextBox for name/description
                var achNameDescTxt = new TextBox();
                achNameDescTxt.Name = "ach" + ach + "NameTxt";
                achNameDescTxt.FontSize = 14;
                achNameDescTxt.Height = ACHIEVEMENT_ROW_HEIGHT;
                achNameDescTxt.Width = 270;
                achNameDescTxt.HorizontalContentAlignment = HorizontalAlignment.Left;
                achNameDescTxt.VerticalContentAlignment = VerticalAlignment.Center;
                achNameDescTxt.Foreground = new SolidColorBrush(Color.FromArgb(DEFAULT_TEXT_COLOR[0], DEFAULT_TEXT_COLOR[1], DEFAULT_TEXT_COLOR[2], DEFAULT_TEXT_COLOR[3]));
                achNameDescTxt.Background = new SolidColorBrush(Color.FromArgb(DEFAULT_BACK_COLOR[0], DEFAULT_BACK_COLOR[1], DEFAULT_BACK_COLOR[2], DEFAULT_BACK_COLOR[3]));
                achNameDescTxt.Margin = new Thickness(10, 0, 0, 0);
                achNameDescTxt.BorderThickness = new Thickness(0);
                achNameDescTxt.TextWrapping = TextWrapping.Wrap;
                achNameDescTxt.IsReadOnlyCaretVisible = false;
                achNameDescTxt.IsReadOnly = true;
                achNameDescTxt.Cursor = Cursors.Arrow;

                //Image
                var achIconImg = new Image();
                achIconImg.Name = "ach" + ach + "IconTb";
                try { //Achievement images are named according to their achievementId
                    string achievementImageUri;
                    if (settings.achList.Contains(achievementCollection[ach].achievementId)) {
                        //This achievement is unlocked
                        achievementImageUri = "pack://application:,,,/img/achievements/" + achievementCollection[ach].achievementId + ".png";
                        achNameDescTxt.Text = achievementCollection[ach].achievementName;
                        achNameDescTxt.ToolTip = achievementCollection[ach].achievementDesc;
                        achIconImg.Opacity = 1;
                    } else {
                        //This achievement is locked
                        achievementImageUri = "pack://application:,,,/img/achievements/locked.png";
                        achNameDescTxt.Text = achievementCollection[ach].achievementDesc;
                        achIconImg.Opacity = 0.5;
                    }
                    achIconImg.Source = new BitmapImage(new Uri(achievementImageUri));
                } catch (Exception) {
                    log.addLog("ERROR: Could not access achievement icon " + achievementCollection[ach].achievementId);
                }
                achIconImg.Height = ACHIEVEMENT_ROW_HEIGHT;
                achIconImg.Width = ACHIEVEMENT_ROW_HEIGHT;

                //Put the TextBox and Image together as a row
                StackPanel achSingleStack = new StackPanel();
                achSingleStack.Orientation = Orientation.Horizontal;
                achSingleStack.Margin = new Thickness(5);
                achSingleStack.Children.Add(achIconImg);
                achSingleStack.Children.Add(achNameDescTxt);
                achRowStacks.Add(achSingleStack);
                achRowStacks[ach].Name = "ach" + ach + "RowStk";
            }

            //Put the rows together and add them to the canvas
            achFullStack = new StackPanel();
            achFullStack.Orientation = Orientation.Vertical;
            foreach (StackPanel achRow in achRowStacks)
            {
                achFullStack.Children.Add(achRow);
            }
            achCanvas.Children.Add(achFullStack);
            Canvas.SetLeft(achFullStack, 0);
            Canvas.SetTop(achFullStack, -5);
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open AchievementListDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse AchievementListDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close AchievementListDialog");
        }
    }
}
