using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        //Constants
        private const string WINDOW_NAME = "Log";
        private const string LOG_EXT = ".log";

        //Globals
        private string appName;
        MainWindow main;

        private string logPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\Logs\\";
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

        /// <summary>
        /// Adds an ASCII art prefix to the beginning of the log
        /// </summary>
        private void addASCIIArt()
        {
            addLog(Environment.NewLine +
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
        /// Generates a date string to be used as the name for today's log file
        /// </summary>
        /// <returns>Today's log file name</returns>
        private string generateLogFileName()
        {
            string logFileName = String.Format("{0:dd-MM-yy}", DateTime.Now); // "09-03-08"
            logFileName += LOG_EXT;
            return logFileName;
        }

        /// <summary>
        /// Writes the specified text to the log file
        /// </summary>
        /// <param name="logEntry">The text to append</param>
        private void writeToLogFile(string logEntry)
        {
            try {
                //Append to today's file if it exists, otherwise create a new one
                using (StreamWriter sw = File.AppendText(logPath + generateLogFileName()))
                {
                    sw.Write(logEntry);
                }
            } catch(Exception e) {
                addLog(e.ToString());
            }
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
                    writeToLogFile(content);
                } else {
                    string logEntry = Environment.NewLine + DateTime.Now.ToLongTimeString() + ": " + content;
                    LogTxt.Text += logEntry;
                    writeToLogFile(logEntry);
                }
                LogTxt.ScrollToEnd();
            }
        }


        /// <summary>
        /// Starts a process to open the logs directory
        /// </summary>
        public void showLogFiles()
        {
            addLog("Request: Show Log Files");
            Process proc = new Process();
            proc.StartInfo.FileName = logPath;
            proc.StartInfo.UseShellExecute = true;
            //proc.StartInfo.Verb = "runas";
            proc.Start();
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

        /// <summary>
        /// When the Show Files... button is clicked
        /// </summary>
        private void ShowFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            showLogFiles();
        }

        /// <summary>
        /// When the X button is clicked
        /// </summary>
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
