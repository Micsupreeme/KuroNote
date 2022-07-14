using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "About";

        //Globals
        string APPLICATION_NAME = Assembly.GetExecutingAssembly().GetName().Name.ToString();
        string APPLICATION_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private string appName;
        public string appPath = AppDomain.CurrentDomain.BaseDirectory;
        MainWindow main;
        Log log;

        public AboutDialog(MainWindow _mainWin, Log _mainLog)
        {
            InitializeComponent();
            InitialiseBackground();
            main = _mainWin;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
            populateFields();
        }

        /// <summary>
        /// Sets the background of the window to the "about background" image
        /// </summary>
        private void InitialiseBackground()
        {
            this.Background = new ImageBrush {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/kuronote-fuji.png", UriKind.Absolute)),
                Stretch = Stretch.UniformToFill
            };
        }

        /// <summary>
        /// Fill the UI with application details
        /// </summary>
        private void populateFields() {
            //Write application details to UI
            appNameTb.Text = appName + " ";
            appVersionTb.Text = APPLICATION_VERSION;
            appCopyrightTb.Text = "Copyright \u00A9 ";
            appPublisherTb.Text = "Micsupreeme";
            appCopyrightYearTb.Text = " 2022";
            appDescriptionTxt.Text =
                "A text editor that allows you to set any picture you like as a background!\n\n" +

                "With a focus on customisability and intuitiveness, " + appName + " is a " +
                "Notepad-like text editor with personality. It's designed for the quick creation of " +
                "beautiful notes. Custom theme profiles and optional gamification elements like ranks " +
                "and achievements make note-writing more enjoyable.\n\n" +

                "You can expect all the essential features like: save as; print; find; " +
                "replace; spell check; and word count. Additionally, " + appName + " offers AES encryption " +
                "that enables you to password-protect your notes.\n\n"+

                "Double-click here for an achievement!";
            appBrandingTb.Text = "Branding design by Luke";
            appIconsTb.Text = "Generic icons by ";
            appIconsLinkTb.Text = "Material Design";
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open AboutDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse AboutDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Opens the specified fileName or path with the system's default program
        /// </summary>
        /// <param name="fileName">The file or path to open</param>
        private void startProcess(string fileName) {
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

        private void appVersionTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdatesDialog updatesDialog = new UpdatesDialog(main, log);
            updatesDialog.toggleVisibility(true);
        }

        private void appPublisherTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://github.com/Micsupreeme");
        }

        private void appIconsLinkTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://fonts.google.com/icons?selected=Material+Icons");
        }

        private void appDescriptionTxt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            main.unlockAchievement(1);
        }

        private void dependenciesBtn_Click(object sender, RoutedEventArgs e)
        {
            DependenciesDialog dependenciesDialog = new DependenciesDialog(main, log);
            dependenciesDialog.toggleVisibility(true);
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close AboutDialog");
        }
    }
}
