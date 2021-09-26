using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for DependenciesDialog.xaml
    /// </summary>
    public partial class DependenciesDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Dependencies";

        //Globals
        private string appName;
        public string appPath = AppDomain.CurrentDomain.BaseDirectory;
        MainWindow main;
        Log log;

        public DependenciesDialog(MainWindow _mainWin, Log _mainLog)
        {
            InitializeComponent();
            InitialiseBackground();
            main = _mainWin;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
        }

        /// <summary>
        /// Sets the background of the window to the "about background" image
        /// </summary>
        private void InitialiseBackground()
        {
            this.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/bgs/kuronote-bars.png", UriKind.Absolute))
            };
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open DependenciesDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse DependenciesDialog");
                this.Visibility = Visibility.Collapsed;
            }
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
        /// When the user clicks the netcore licence link
        /// </summary>
        private void netcoreLicenceTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess(appPath + "licences\\dotnetcore LICENCE.txt");
        }

        /// <summary>
        /// When the user clicks the netcore URL
        /// </summary>
        private void netcoreUrlTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://github.com/dotnet/core");
        }

        /// <summary>
        /// When the user clicks the wpf licence link
        /// </summary>
        private void wpfLicenceTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess(appPath + "licences\\dotnetwpf LICENCE.txt");
        }

        /// <summary>
        /// When the user clicks the wpf URL
        /// </summary>
        private void wpfUrlTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://github.com/dotnet/wpf");
        }

        /// <summary>
        /// When the user clicks the colorPickerWpf licence link
        /// </summary>
        private void colorPickerWpfLicenceTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess(appPath + "licences\\colorpickerwpf LICENCE.txt");
        }

        /// <summary>
        /// When the user clicks the colorPickerWpf URL
        /// </summary>
        private void colorPickerWpfUrlTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://github.com/drogoganor/ColorPickerWPF");
        }

        /// <summary>
        /// When the user clicks the newtonsoft licence link
        /// </summary>
        private void newtonsoftLicenceTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess(appPath + "licences\\newtonsoftjson LICENCE.txt");
        }

        /// <summary>
        /// When the user clicks the newtonsoft URL
        /// </summary>
        private void newtonsoftUrlTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://www.newtonsoft.com/json");
        }

        /// <summary>
        /// When the user clicks the wpfSpark licence link
        /// </summary>
        private void wpfSparkLicenceTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess(appPath + "licences\\wpfspark LICENCE.txt");
        }

        /// <summary>
        /// When the user clicks the wpfSpark URL
        /// </summary>
        private void wpfSparkUrlTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            startProcess("https://github.com/ratishphilip/wpfspark");
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
            log.addLog("Close DependenciesDialog");
        }
    }
}
