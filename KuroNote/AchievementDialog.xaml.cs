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
    /// Interaction logic for AchievementDialog.xaml
    /// </summary>
    public partial class AchievementDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Achievement Unlocked";
        private const int REWARD_WINDOW_HEIGHT = 360;
        private const int NO_REWARD_WINDOW_HEIGHT = 265;

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        KuroNoteAchievement achievement;

        public AchievementDialog(MainWindow _mainWin, KuroNoteSettings _settings, KuroNoteAchievement _achievement, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _settings;
            achievement = _achievement;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            populateFields();
        }

        private void populateFields()
        {
            lblAchievementName.Content = achievement.achievementName;
            lblAchievementDesc.Content = achievement.achievementDesc;

            //Achievement images are named according to their achievementId
            try {
                string achievementImageUri = "pack://application:,,,/img/achievements/" + achievement.achievementId + ".png";
                imgAchievementIcon.Source = new BitmapImage(new Uri(achievementImageUri));
            } catch(Exception e) {
                log.addLog("ERROR: Could not access achievement icon " + achievement.achievementId);
            }

            if (achievement.rewardTheme != null) {
                //This achievement has a theme reward - make reward section visible and populate
                toggleWindowMode(true);
                lblRewardName.Content = achievement.rewardTheme.themeName;
            } else {
                //This achievement does not have a theme reward - hide reward section
                toggleWindowMode(false);
            }
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open AchievementDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse AchievementDialog");
                this.Visibility = Visibility.Collapsed;
                if (settings.floating) { //If floating is enabled, restore it when this dialog is dismissed
                    main.Topmost = true;
                }
            }
        }

        /// <summary>
        /// Expands/
        /// </summary>
        /// <param name="includesReward"></param>
        private void toggleWindowMode(bool rewardMode)
        {
            if (rewardMode) {
                stkRewardStack.Visibility = Visibility.Visible;
                this.Height = REWARD_WINDOW_HEIGHT;
                this.MinHeight = REWARD_WINDOW_HEIGHT;
                this.MaxHeight = REWARD_WINDOW_HEIGHT;
            } else {
                stkRewardStack.Visibility = Visibility.Collapsed;
                this.Height = NO_REWARD_WINDOW_HEIGHT;
                this.MinHeight = NO_REWARD_WINDOW_HEIGHT;
                this.MaxHeight = NO_REWARD_WINDOW_HEIGHT;
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
            if (settings.floating) { //If floating is enabled, restore it when this dialog is dismissed
                main.Topmost = true;
            }
            log.addLog("Close AchievementDialog");
        }
    }
}
