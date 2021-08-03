using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for FindDialog.xaml
    /// </summary>
    public partial class FindDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "Find";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;

        int searchPosition = 0; //start at 0, then increase to the position at the end of each match

        public FindDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis) {
                log.addLog("Open FindDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse FindDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Enables/disables the "Find Next" and "Count" buttons
        /// </summary>
        /// <param name="enable"></param>
        private void toggleEnabled(bool enable)
        {
            if(enable) {
                btnFind.IsEnabled = true;
                btnCount.IsEnabled = true;
            } else {
                btnFind.IsEnabled = false;
                btnCount.IsEnabled = false;
            }
        }

        /// <summary>
        /// When the user clicks "Find Next" or hits ENTER
        /// </summary>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            doFind();
        }

        /// <summary>
        /// Selects the next occurance of the search term
        /// </summary>
        private void doFind()
        {
            string findPhrase = txtFindWhat.Text;
            bool caseSensitive = (bool)chkCaseSensitive.IsChecked;
            bool wrapAround = (bool)chkWrapAround.IsChecked;
            log.addLog("Find: " + findPhrase);
            TextRange document;
            TextPointer selectionStart, selectionEnd;

            try {
                document = new TextRange(main.MainRtb.Document.ContentStart.GetPositionAtOffset(searchPosition), main.MainRtb.Document.ContentEnd);
            } catch(ArgumentNullException) {
                //no offset
                document = new TextRange(main.MainRtb.Document.ContentStart, main.MainRtb.Document.ContentEnd);
            }

            Match match;
            if(caseSensitive) {
                match = Regex.Match(document.Text, findPhrase);
            } else {
                match = Regex.Match(document.Text, findPhrase, RegexOptions.IgnoreCase);
            }
            
            if (match.Success)
            {
                //MessageBox.Show("found at pos " + (searchPosition + match.Index));
                selectionStart = document.Start.GetPositionAtOffset(match.Index);
                selectionEnd = selectionStart.GetPositionAtOffset(findPhrase.Length);
                main.MainRtb.Selection.Select(selectionStart, selectionEnd);

                //GetPositionAtOffset includes invisible non-text characters that can make the selection start too early
                //Fix the pointers here
                int correctionOffset = 0;
                if (caseSensitive)
                {
                    while (!main.MainRtb.Selection.Text.Equals(findPhrase))
                    {
                        selectionStart = selectionStart.GetPositionAtOffset(1);
                        selectionEnd = selectionStart.GetPositionAtOffset(findPhrase.Length);
                        correctionOffset++;
                        main.MainRtb.Selection.Select(selectionStart, selectionEnd);
                    }
                } else {
                    while (!main.MainRtb.Selection.Text.ToLower().Equals(findPhrase.ToLower()))
                    {
                        selectionStart = selectionStart.GetPositionAtOffset(1);
                        selectionEnd = selectionStart.GetPositionAtOffset(findPhrase.Length);
                        correctionOffset++;
                        main.MainRtb.Selection.Select(selectionStart, selectionEnd);
                    }
                }

                //Add a rectangle object to the selected text, then scroll to it
                Rect selectionRect = selectionStart.GetCharacterRect(LogicalDirection.Forward);
                main.MainRtb.ScrollToVerticalOffset(selectionRect.Y);

                main.Activate(); //bring the main window to the front
                searchPosition = searchPosition + match.Index + correctionOffset + findPhrase.Length;
            }
            else
            {
                if (searchPosition == 0) {
                    MessageBox.Show("There are no occurances of this search term.", "Find results", MessageBoxButton.OK, MessageBoxImage.Information);
                } else {
                    //no match here - reset
                    searchPosition = 0;
                    if (wrapAround) {
                        doFind();
                    }  else {
                        MessageBox.Show("Reached end of the file.", "Find results", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        /// <summary>
        /// When the user clicks "Count"
        /// </summary>
        private void btnCount_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("There are " + getCount() + " occurances of this search term.", "Count results", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Gets the number of matches for the search term in the document
        /// </summary>
        /// <returns>The number of matches for the search term in the document</returns>
        private int getCount()
        {
            string findPhrase = txtFindWhat.Text;
            bool caseSensitive = (bool)chkCaseSensitive.IsChecked;
            log.addLog("Count occurances of: " + findPhrase);
            TextRange document = new TextRange(main.MainRtb.Document.ContentStart, main.MainRtb.Document.ContentEnd);

            if(caseSensitive) {
                MatchCollection csMatches = Regex.Matches(document.Text, findPhrase); //REGEX is case sensitive by default
                log.addLog("Result: " + csMatches.Count);
                return csMatches.Count;
            } else {
                MatchCollection ciMatches = Regex.Matches(document.Text, findPhrase, RegexOptions.IgnoreCase);
                log.addLog("Result: " + ciMatches.Count);
                return ciMatches.Count;
            }
        }

        /// <summary>
        /// When the user clicks "Cancel" or hits ESC
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibility(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close FindDialog");
        }

        /// <summary>
        /// When the user changes the search term
        /// </summary>
        private void txtFindWhat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtFindWhat.Text.Length > 0) { //don't let the user search for null
                toggleEnabled(true);
            } else {
                toggleEnabled(false);
            }
        }
    }
}
