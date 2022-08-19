using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for UpdatesDialog.xaml
    /// </summary>
    public partial class UpdatesDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Updates";

        //Globals
        string APPLICATION_NAME = Assembly.GetExecutingAssembly().GetName().Name.ToString();
        string APPLICATION_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string HTML_FROM_PATTERN = "<title>";
        string HTML_TO_PATTERN = "</title>";
        string VERSION_URL = "https://micdev.myfreesites.net/kuronotever";
        string UPDATE_URL = "https://github.com/Micsupreeme/KuroNote/releases";
        string WAIT_TEXT = "Checking...";
        string FAIL_TEXT = "Failed";

        string htmlResultString;
        string latestVersionString;

        private string appName;
        public string appPath = AppDomain.CurrentDomain.BaseDirectory;
        MainWindow main;
        Log log;

        public UpdatesDialog(MainWindow _mainWin, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            currentVersionNoTxt.Text = APPLICATION_VERSION;
            getLatestVersionNumber();
        }

        /// <summary>
        /// Opens the specified fileName or path with the system's default program
        /// </summary>
        /// <param name="fileName">The file or path to open</param>
        private void startProcess(string fileName)
        {
            try {
                var psi = new ProcessStartInfo {
                    FileName = fileName,
                    UseShellExecute = true
                };
                Process.Start(psi);
            } catch (Exception e) {
                log.addLog("ERROR: Failed to start " + fileName + " " + e.ToString());
            }
        }

        /// <summary>
        /// Starts an async process to get the latest KuroNote version number from a designated webpage
        /// </summary>
        private void getLatestVersionNumber()
        {
            //Reset UI and variables
            updateIconImg.Visibility = Visibility.Hidden;
            updateLinkTb.Visibility = Visibility.Collapsed;
            updateMessageTb.Visibility = Visibility.Collapsed;
            latestVersionNoTxt.Text = WAIT_TEXT;
            htmlResultString = "";
            latestVersionString = "";

            //Start async check for updates
            BackgroundWorker updateChecker = new BackgroundWorker();
            updateChecker.DoWork += updateChecker_DoWork;
            updateChecker.RunWorkerCompleted += updateChecker_RunWorkerCompleted;
            updateChecker.WorkerReportsProgress = false;
            updateChecker.WorkerSupportsCancellation = false;
            updateChecker.RunWorkerAsync();
        }

        /// <summary>
        /// Download the HTML from the designated version number webpage
        /// </summary>
        private void updateChecker_DoWork(object sender, DoWorkEventArgs e) 
        {
            //https://stackoverflow.com/users/4390133/hakan-f%c4%b1st%c4%b1k's HTML downloader
            try {
                HttpClient httpClient = new HttpClient();
                using (HttpResponseMessage httpResponse = httpClient.GetAsync(VERSION_URL).Result) {
                    using (HttpContent httpContent = httpResponse.Content) {
                        htmlResultString = httpContent.ReadAsStringAsync().Result;
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Unable to retrieve " + appName + " updates information from " + VERSION_URL + ". Either this machine or the web page are offline.", "Check for Updates Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine("Check for Updates Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// Extract the version number from the title element of the downloaded HTML
        /// and compare it to the current version number
        /// </summary>
        private void updateChecker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (htmlResultString.Length > 0) { //If we have version information, evaluate it
                //Get version number string
                Match titleOpenMatch = Regex.Match(htmlResultString, HTML_FROM_PATTERN);
                Match titleCloseMatch = Regex.Match(htmlResultString, HTML_TO_PATTERN);
                latestVersionString = htmlResultString.Substring(titleOpenMatch.Index + HTML_FROM_PATTERN.Length, titleCloseMatch.Index - (titleOpenMatch.Index + HTML_FROM_PATTERN.Length));

                //Convert version number strings to ints by removing the dots
                int currentVersionNumber = int.Parse(Regex.Replace(APPLICATION_VERSION, @"\.", ""));
                int latestVersionNumber = int.Parse(Regex.Replace(latestVersionString, @"\.", ""));
                log.addLog("Current version: " + currentVersionNumber + " | Latest version: " + latestVersionNumber);

                //Compare version numbers to determine whether or not an update is required
                if (currentVersionNumber < latestVersionNumber) {
                    //update required
                    setUpdateState(true);
                } else {
                    //up to date OR secret developer version
                    setUpdateState(false);
                }
            } else {
                latestVersionNoTxt.Text = FAIL_TEXT;
            }
        }

        /// <summary>
        /// Sets the UI to a preset state
        /// </summary>
        /// <param name="stateId">The stateId to set the UI to</param>
        private void setUpdateState(bool requiresUpdate)
        {
            if (requiresUpdate) {
                log.addLog("This " + appName + " installation is out of date");
                latestVersionNoTxt.Text = latestVersionString;

                try {
                    updateIconImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_error_outline_white_48dp.png.png"));
                } catch (Exception) {
                    log.addLog("ERROR: Could not access \"update required\" icon");
                }
                updateIconImg.Visibility = Visibility.Visible;
                updateLinkTb.Visibility = Visibility.Visible;
                updateMessageTb.Visibility = Visibility.Collapsed;
            } else {
                log.addLog("This " + appName + " installation is up to date");
                latestVersionNoTxt.Text = latestVersionString;

                try {
                    updateIconImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_check_circle_eee_48dp.png"));
                } catch (Exception) {
                    log.addLog("ERROR: Could not access \"up to date\" icon");
                }
                updateIconImg.Visibility = Visibility.Visible;
                updateLinkTb.Visibility = Visibility.Collapsed;
                updateMessageTb.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// When the user clicks the update link
        /// </summary>
        private void updateLinkTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess(UPDATE_URL);
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open UpdatesDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse UpdatesDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER or hits ESC
        /// </summary>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            log.addLog("Close UpdatesDialog");
        }
    }
}
