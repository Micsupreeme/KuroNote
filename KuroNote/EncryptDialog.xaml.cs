using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
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
        //password based encryption, it's probably safe enough as long as it is truly uncommon.
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
        /// When either of the password fields are changed
        /// </summary>
        private void KeyPw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(passwordsMatch()) {
                btnOk.IsEnabled = true;
            } else {
                btnOk.IsEnabled = false;
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
