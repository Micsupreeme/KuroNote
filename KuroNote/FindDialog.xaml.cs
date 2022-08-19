using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for FindDialog.xaml
    /// </summary>
    public partial class FindDialog : Window
    {
        //Constants
        private const int FIND_MODE_HEIGHT = 180;
        private const int REPLACE_MODE_HEIGHT = 230;

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        bool replaceMode;

        int searchPosition = 0; //start at 0, then increase to the position at the end of each match

        public FindDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog, bool _replaceMode)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            replaceMode = _replaceMode;
            appName = main.appName;

            string windowName;
            if (replaceMode) {
                windowName = "Replace";
            } else {
                windowName = "Find";
            }
            this.Title = windowName + " - " + appName;

            toggleReplaceUI(replaceMode);
            txtFindWhat.Focus();
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
        /// Sets the UI to either "Find" or "Find and Replace" mode
        /// </summary>
        /// <param name="enable">True to set Replace UI, false to set Find UI</param>
        private void toggleReplaceUI(bool replace)
        {
            if (replace) {
                lblReplaceWith.Visibility = Visibility.Visible;
                txtReplaceWith.Visibility = Visibility.Visible;
                btnReplace.Visibility = Visibility.Visible;
                btnReplaceAll.Visibility = Visibility.Visible;
                btnCount.Visibility = Visibility.Collapsed;
                this.Height = REPLACE_MODE_HEIGHT;
                this.MinHeight = REPLACE_MODE_HEIGHT;
                this.MaxHeight = REPLACE_MODE_HEIGHT;
            } else {
                lblReplaceWith.Visibility = Visibility.Collapsed;
                txtReplaceWith.Visibility = Visibility.Collapsed;
                btnReplace.Visibility = Visibility.Collapsed;
                btnReplaceAll.Visibility = Visibility.Collapsed;
                btnCount.Visibility = Visibility.Visible;
                this.Height = FIND_MODE_HEIGHT;
                this.MinHeight = FIND_MODE_HEIGHT;
                this.MaxHeight = FIND_MODE_HEIGHT;
            }
        }

        /// <summary>
        /// Enables/disables the "Find Next" and "Count" buttons
        /// </summary>
        /// <param name="enable">True to enable controls, false otherwise</param>
        private void toggleEnabled(bool enable)
        {
            if (enable) {
                btnFind.IsEnabled = true;
                btnCount.IsEnabled = true;
                btnReplaceAll.IsEnabled = true;
            } else {
                btnFind.IsEnabled = false;
                btnCount.IsEnabled = false;
                btnReplaceAll.IsEnabled = false;
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
            if (caseSensitive) {
                match = Regex.Match(document.Text, findPhrase);
            } else {
                match = Regex.Match(document.Text, findPhrase, RegexOptions.IgnoreCase);
            }
            
            if (match.Success) {
                //MessageBox.Show("found at pos " + (searchPosition + match.Index));
                selectionStart = document.Start.GetPositionAtOffset(match.Index);
                selectionEnd = selectionStart.GetPositionAtOffset(findPhrase.Length);
                main.MainRtb.Selection.Select(selectionStart, selectionEnd);

                //GetPositionAtOffset includes invisible non-text characters (line returns) that can make the selection start too early
                //Fix the pointers here
                int correctionOffset = 0;
                if (caseSensitive) {
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

                //we have a match selected, we can now optionally replace it
                log.addLog("Match selected!");
                btnReplace.IsEnabled = true;
            } else {
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
            MessageBox.Show("There are " + getCount() + " occurances of the search term \"" + txtFindWhat.Text + "\".", "Count results", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (caseSensitive) {
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

        /// <summary>
        /// When the user changes the search term or replace term
        /// </summary>
        private void txtFindReplace_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtFindWhat.Text.Length > 0) { //don't let the user search for null
                toggleEnabled(true);
            } else {
                toggleEnabled(false);
            }
        }

        /// <summary>
        /// When the user clicks "Replace"
        /// </summary>
        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            string findPhrase = txtFindWhat.Text;
            string replacePhrase = txtReplaceWith.Text;
            if (main.MainRtb.Selection.Text == findPhrase) { //ensure the selection is correct before replacing it
                main.MainRtb.Selection.Text = replacePhrase;
                log.addLog("Single replace completed: \"" + findPhrase + "\" -> \"" + replacePhrase + "\"");
            } else {
                log.addLog("Unable to perform single replace, the selection did not match the findPhrase \"" + findPhrase + "\" (selection length: " + main.MainRtb.Selection.Text.Length + ")");
            }
            btnReplace.IsEnabled = false; //Once completed, there is no need to repeat a single replace operation, it will be enabled again if a new match is found
        }

        /// <summary>
        /// When the user clicks "Replace All"
        /// </summary>
        private void btnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult replaceAllConfirmation = MessageBox.Show("Are you sure you want to replace the " + getCount() + " occurances of \"" + txtFindWhat.Text + "\" with \"" + txtReplaceWith.Text + "\"?", "Replace All?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (replaceAllConfirmation == MessageBoxResult.Yes) {
                replaceAll();
            }
        }

        /// <summary>
        /// Replaces all occurances of the findPhrase with the replacePhrase
        /// </summary>
        private void replaceAll()
        {
            string findPhrase = txtFindWhat.Text;
            string replacePhrase = txtReplaceWith.Text;
            bool caseSensitive = (bool)chkCaseSensitive.IsChecked;
            log.addLog("Replace All \"" + findPhrase + "\" with \"" + replacePhrase + "\"");
            TextRange document = new TextRange(main.MainRtb.Document.ContentStart, main.MainRtb.Document.ContentEnd);

            if (caseSensitive) {
                document.Text = Regex.Replace(document.Text, findPhrase, replacePhrase); //REGEX is case sensitive by default
            } else {
                document.Text = Regex.Replace(document.Text, findPhrase, replacePhrase, RegexOptions.IgnoreCase);
            }

            //Select everything, replace it with the treated text, then deselect (put the caret at the end)
            main.MainRtb.Selection.Select(main.MainRtb.Document.ContentStart, main.MainRtb.Document.ContentEnd);
            main.MainRtb.Selection.Text = document.Text;
            main.MainRtb.Selection.Select(main.MainRtb.Document.ContentEnd, main.MainRtb.Document.ContentEnd);
        }

        /// <summary>
        /// While the window is closing
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            log.addLog("Close FindDialog");
        }
    }
}
