using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
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
    /// Interaction logic for EncryptDialog.xaml
    /// </summary>
    public partial class EncryptDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "AES Encryption";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        string content;                 //a copy from MainRtb of the text to encrypt

        BackgroundWorker encWorker;
        string AESencryptedContent = string.Empty;

        //While an app specific salt is not the best practice for
        //password based encryption, it's probably safe enough as long as it is uncommon.
        private static byte[] _salt;

        public EncryptDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog, string _content)
        {
            InitializeComponent();
            main = _mainWin;
            settings = _currentSettings;
            log = _mainLog;
            content = _content;
            appName = main.appName;
            this.Title = WINDOW_NAME + " - " + appName;

            _salt = Encoding.ASCII.GetBytes("hi this is kuronote salt");
        }

        private void beginAESEnc()
        {
            btnOk.IsEnabled = false;

            encWorker = new BackgroundWorker();
            encWorker.DoWork += encWorker_DoWork;
            //encWorker.ProgressChanged += encWorker_ProgressChanged;
            encWorker.RunWorkerCompleted += encWorker_RunWorkerCompleted;
            encWorker.WorkerReportsProgress = true;
            encWorker.WorkerSupportsCancellation = true;

            AESEncArgs args = new AESEncArgs();
            args.content = content;
            args.key = KeyPw.Password.ToString();

            log.addLog("AES Encryption Starting");
            encWorker.RunWorkerAsync(args);
        }

        
        void encWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker encWorker = sender as BackgroundWorker;
            AESEncArgs args = e.Argument as AESEncArgs;

            //https://stackoverflow.com/users/188474/brett's Simple SimpleEncryptWithPassword
            RijndaelManaged aesAlg = null;
            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(args.key, _salt);
                //allow iteration count to change?

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(args.content);
                        }
                    }
                    AESencryptedContent = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }
        }

        void encWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) {
                log.addLog("AES Encryption Cancelled");
            } else {
                if (!AESencryptedContent.Equals(string.Empty)) {
                    log.addLog("AES Encryption Completed");


                    log.addLog("Request: EncDecFinish");
                    EncDecFinishDialog edf = new EncDecFinishDialog(main, settings, log, 0, AESencryptedContent);
                    toggleVisibility(false);
                    edf.toggleVisibility(true);

                } else {
                    log.addLog("AES Encryption Failed");

                    log.addLog("Request: EncDecFinish");
                    EncDecFinishDialog edf = new EncDecFinishDialog(main, settings, log, 1, AESencryptedContent);
                    toggleVisibility(false);
                    edf.toggleVisibility(true);
                }      
            }
            btnOk.IsEnabled = true;
        }

        /// <summary>
        /// Open/close this window
        /// </summary>
        public void toggleVisibility(bool vis)
        {
            if (vis)
            {
                log.addLog("Open EncryptionDialog");
                this.Visibility = Visibility.Visible;
            }
            else
            {
                log.addLog("Collapse EncryptionDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Checks if both of the password fields match and are not empty
        /// </summary>
        /// <returns>True if both password fields match and are not empty, false otherwise</returns>
        private bool passwordsMatch()
        {
            if(KeyPw.Password.ToString().Length > 0) {
                return KeyPw.Password.ToString().Equals(KeyRepeatPw.Password.ToString());
            } else {
                return false;
            }
        }

        /// <summary>
        /// Generates a score based on the length of the password field content
        /// </summary>
        /// <returns>A password length score from 0 to 3</returns>
        private int getPassLengthScore()
        {
            //Thresholds
            const int FAIR_LENGTH = 6;
            const int STRONG_LENGTH = 9;
            const int EXCEL_LENGTH = 14;

            int passLength = KeyPw.Password.ToString().Length;

            if(passLength >= FAIR_LENGTH) {
                if(passLength >= STRONG_LENGTH) {
                    if(passLength >= EXCEL_LENGTH) {
                        //excel
                        return 3;
                    }
                    //strong
                    return 2;
                }
                // fair
                return 1;
            } else {
                //weak
                return 0;
            }
        }

        /// <summary>
        /// Generates a score based on the number of lowercase letters in the password field content
        /// </summary>
        /// <returns>A password lowercase score from 0 to 3</returns>
        private int getPassLowerScore()
        {
            //Thresholds
            const int FAIR_LOWER = 1;
            const int STRONG_LOWER = 4;
            const int EXCEL_LOWER = 8;

            MatchCollection lowercase = Regex.Matches(KeyPw.Password.ToString(), @"([a-z])");

            if (lowercase.Count >= FAIR_LOWER) {
                if (lowercase.Count >= STRONG_LOWER) {
                    if (lowercase.Count >= EXCEL_LOWER) {
                        //excel
                        return 3;
                    }
                    //strong
                    return 2;
                }
                //fair
                return 1;
            } else {
                //weak
                return 0;
            }
        }

        /// <summary>
        /// Generates a score based on the number of uppercase letters in the password field content
        /// </summary>
        /// <returns>A password UPPERCASE score from 0 to 3</returns>
        private int getPassUpperScore()
        {
            //Thresholds
            const int FAIR_UPPER = 1;
            const int STRONG_UPPER = 3;
            const int EXCEL_UPPER = 7;

            MatchCollection uppercase = Regex.Matches(KeyPw.Password.ToString(), @"([A-Z])");

            if (uppercase.Count >= FAIR_UPPER) {
                if (uppercase.Count >= STRONG_UPPER) {
                    if (uppercase.Count >= EXCEL_UPPER) {
                        //excel
                        return 3;
                    }
                    //strong
                    return 2;
                }
                //fair
                return 1;
            } else {
                //weak
                return 0;
            }
        }

        /// <summary>
        /// Generates a score based on the number of numeric characters in the password field content
        /// </summary>
        /// <returns>A password numb3r score from 0 to 3</returns>
        private int getPassNumberScore()
        {
            //Thresholds
            const int FAIR_NUMBER = 1;
            const int STRONG_NUMBER = 3;
            const int EXCEL_NUMBER = 5;

            MatchCollection numbers = Regex.Matches(KeyPw.Password.ToString(), @"([0-9])");

            if (numbers.Count >= FAIR_NUMBER) {
                if (numbers.Count >= STRONG_NUMBER) {
                    if (numbers.Count >= EXCEL_NUMBER) {
                        //excel
                        return 3;
                    }
                    //strong
                    return 2;
                }
                //fair
                return 1;
            } else {
                //weak
                return 0;
            }
        }

        /// <summary>
        /// Generates a score based on the number of symbols in the password field content
        /// </summary>
        /// <returns>A password $ymbol score from 0 to 3</returns>
        private int getPassSymbolScore()
        {
            //Thresholds
            const int FAIR_SYMBOLS = 1;
            const int STRONG_SYMBOLS = 2;
            const int EXCEL_SYMBOLS = 3;

            MatchCollection symbols = Regex.Matches(KeyPw.Password.ToString(), @"([-!$%^&*()_+|~=`{}\[\]:"";'<>?,.\\/@#£¬])");

            if(symbols.Count >= FAIR_SYMBOLS) {
                if(symbols.Count >= STRONG_SYMBOLS) {
                    if(symbols.Count >= EXCEL_SYMBOLS) {
                        //excel
                        return 3;
                    }
                    //strong
                    return 2;
                }
                //fair
                return 1;
            } else {
                //weak
                return 0;
            }
        }

        /// <summary>
        /// Generates a password strength score based on 5 small tests
        /// </summary>
        /// <param name="_passwordsMatch">The rating for the password</param>
        /// <returns></returns>
        private string getPassScore(bool _passwordsMatch)
        {
            //Thresholds
            const int FAIR_SCORE = 5;
            const int STRONG_SCORE = 9;
            const int EXCEL_SCORE = 13;

            //Progress bar colours
            SolidColorBrush noMatchBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            SolidColorBrush weakBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            SolidColorBrush fairBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));
            SolidColorBrush strongBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            SolidColorBrush excelBrush = new SolidColorBrush(Color.FromRgb(0, 255, 255));

            int strengthScore = 0;
            strengthScore += getPassLengthScore();
            strengthScore += getPassLowerScore();
            strengthScore += getPassUpperScore();
            strengthScore += getPassNumberScore();
            strengthScore += getPassSymbolScore();

            if(_passwordsMatch) {
                
                if(strengthScore >= FAIR_SCORE) {
                    if(strengthScore >= STRONG_SCORE) {
                        if(strengthScore >= EXCEL_SCORE) {
                            KeyStrengthProg.Foreground = excelBrush;
                            KeyStrengthLbl.Content = "Excellent";
                            KeyStrengthProg.Value = strengthScore;
                            return "Excellent";
                        }
                        KeyStrengthProg.Foreground = strongBrush;
                        KeyStrengthLbl.Content = "Strong";
                        KeyStrengthProg.Value = strengthScore;
                        return "Strong";
                    }
                    KeyStrengthProg.Foreground = fairBrush;
                    KeyStrengthLbl.Content = "Fair";
                    KeyStrengthProg.Value = strengthScore;
                    return "Fair";
                } else {
                    KeyStrengthProg.Foreground = weakBrush;
                    KeyStrengthLbl.Content = "Weak";
                    KeyStrengthProg.Value = strengthScore;
                    return "Weak";
                }
            } else {
                KeyStrengthProg.Foreground = noMatchBrush;
                KeyStrengthLbl.Content = "NA";
                KeyStrengthProg.Value = strengthScore;
                return "NA";
            }
        }



        /// <summary>
        /// When either of the password fields are changed
        /// </summary>
        private void KeyPw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(passwordsMatch()) {
                btnOk.IsEnabled = true;
                KeyStrengthLbl.Content = getPassScore(true);
            } else {
                btnOk.IsEnabled = false;
                KeyStrengthLbl.Content = getPassScore(false);
            }
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            beginAESEnc();
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
            log.addLog("Close EncryptionDialog");
        }
    }

    /// <summary>
    /// Used to pass arguments to encWorker
    /// </summary>
    public class AESEncArgs
    {
        public string content;
        public string key;
    }
}
