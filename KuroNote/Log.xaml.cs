using System;
using System.Collections.Generic;
using System.Reflection;
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
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        //Constants
        private const string WINDOW_NAME = "Log";

        //Globals
        private string appName;
        MainWindow main;

        private bool loggingEnabled = true;

        public Log(MainWindow _mainWin, bool _enabled)
        {
            InitializeComponent();
            main = _mainWin;
            loggingEnabled = _enabled;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
        }

        /// <summary>
        /// Generate the session ID and add content at the beginning of every log
        /// </summary>
        public void beginLog()
        {
            long sessionID = generateSessionId();
            SessionIdLbl.Content = "Session " + sessionID + " log:";
            addASCIIArt();
            addLog(
                "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (Dev)" + Environment.NewLine +
                DateTime.Now + ":" + DateTime.Now.Millisecond + ": SESSION " + sessionID + " BEGINS"
            , true);
        }

        private void addASCIIArt()
        {
            addLog(
                "888    d8P  888b    888" + Environment.NewLine +
                "888   d8P   8888b   888" + Environment.NewLine +
                "888  d8P    88888b  888" + Environment.NewLine +
                "888d88K     888Y88b 888" + Environment.NewLine +
                "8888888b    888 Y88b888" + Environment.NewLine +
                "888  Y88b   888  Y88888" + Environment.NewLine +
                "888   Y88b  888   Y8888" + Environment.NewLine +
                "888    Y88b 888    Y888" + Environment.NewLine + Environment.NewLine
            , true);
        }

        /// <summary>
        /// Generates a new session ID
        /// </summary>
        /// <returns>A new session ID based on the exact system time</returns>
        private long generateSessionId()
        {
            return DateTime.Now.ToBinary() * -1;
        }

        /// <summary>
        /// Adds a new log entry with an automatic timestamp, newline, and the specified text
        /// </summary>
        /// <param name="content">The text to add to the log</param>
        /// <param name="removeFormatting">Whether or not to omit the automatic timestamp and the newline</param>
        public void addLog(string content, bool removeFormatting = false)
        {
            if(loggingEnabled) {
                if (removeFormatting) {
                    LogTxt.Text += content;
                } else {
                    LogTxt.Text += Environment.NewLine + DateTime.Now.ToLongTimeString() + ": " + content;
                }
            }
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis)
            {
                addLog("Open Log");
                this.Visibility = Visibility.Visible;
            }
            else
            {
                addLog("Collapse Log");
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Collapse instead of close - log must maintain its content during the session
            //This is however overrided when the entire application closes
            e.Cancel = true;
            addLog("Collapse Log");
            this.Visibility = Visibility.Collapsed;
        }
    }
}
