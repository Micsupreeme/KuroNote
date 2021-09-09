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
    /// Interaction logic for ProfileDialog.xaml
    /// </summary>
    public partial class ProfileDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Profile";
        private const int MAX_RANK = 25; //Ranks beyond this are treated as this value in the backend

        //Globals
        private string appName;
        KuroNoteRank[] rankCollection;
        KuroNoteAchievement[] achievementCollection;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;

        public ProfileDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, KuroNoteRank[] _rankCollection, KuroNoteAchievement[] _achievementCollection, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            rankCollection = _rankCollection;
            achievementCollection = _achievementCollection;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            populateFields();
        }

        /// <summary>
        /// Update the UI to match the current KuroNoteSettings
        /// </summary>
        private void populateFields()
        {
            //Rank image (named according their rankId - unless rank >= 25)
            try {
                string rankImageUri;
                if (settings.profL >= MAX_RANK) {
                    //apply max "x" rank image
                    rankImageUri = "pack://application:,,,/img/ranks/x.png";
                } else {
                    //apply numbered rank image
                    rankImageUri = "pack://application:,,,/img/ranks/" + settings.profL + ".png";
                }
                imgRankIcon.Source = new BitmapImage(new Uri(rankImageUri));
            } catch (Exception) {
                log.addLog("ERROR: Could not access rank icon " + settings.profL);
            }

            //Ranks (Ranks >= 25 are treated as "25" in the backend)
            if (settings.profL >= MAX_RANK) {
                lblRankName.Content = rankCollection[MAX_RANK].rankName + ((settings.profL + 1) - MAX_RANK); //e.g. Rank 28 = "Grand Notemaster +3"
                prgRankAp.Maximum = rankCollection[MAX_RANK].apToNext;
                lblRankAp.Content = settings.profAp + "/" + rankCollection[MAX_RANK].apToNext;
            } else {
                lblRankName.Content = rankCollection[settings.profL].rankName;
                prgRankAp.Maximum = rankCollection[settings.profL].apToNext;
                lblRankAp.Content = settings.profAp + "/" + rankCollection[settings.profL].apToNext;
            }
            prgRankAp.Value = settings.profAp;

            //Achievements
            lblAchievementsCount.Content = settings.achList.Count + "/" + achievementCollection.Length;
            if(settings.achLast != -1) {
                lblMostRecentName.Content = getAchievement(settings.achLast).achievementName;
            }
        }

        /// <summary>
        /// Retrieves the specified achievement information
        /// </summary>
        /// <param name="_achievementId">The unique achievmentId of the achievement to retrieve (NOTE: disregards EnAchDict's array index)</param>
        /// <returns>The specified KuroNoteAchievment object</returns>
        private KuroNoteAchievement getAchievement(int _achievementId)
        {
            foreach (KuroNoteAchievement achievement in achievementCollection) {
                if (achievement.achievementId == _achievementId) {
                    return achievement;
                }
            }
            return null;
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open ProfileDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse ProfileDialog");
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
        /// When the user clicks "Achievements"
        /// </summary>
        private void btnAchievements_Click(object sender, RoutedEventArgs e)
        {
            log.addLog("Request: Achievements...");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close ProfileDialog");
        }
    }
}
