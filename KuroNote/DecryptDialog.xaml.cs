using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace KuroNote
{
    /// <summary>
    /// Interaction logic for DecryptDialog.xaml
    /// </summary>
    public partial class DecryptDialog : Window
    {
        //Constants
        private const string WINDOW_NAME = "AES Decryption";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        string content;                 //a copy from MainRtb of the text to encrypt

        BackgroundWorker decWorker;
        string AESdecryptedContent = string.Empty;

        //While an app specific salt is not the best practice for
        //password based encryption, it's probably safe enough as long as it is truly uncommon.
        private static byte[] _salt;

        public DecryptDialog(MainWindow _mainWin, KuroNoteSettings _currentSettings, Log _mainLog, string _content)
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

        private void beginAESDec()
        {
            btnOk.IsEnabled = false;

            decWorker = new BackgroundWorker();
            decWorker.DoWork += decWorker_DoWork;
            //decWorker.ProgressChanged += decWorker_ProgressChanged;
            decWorker.RunWorkerCompleted += decWorker_RunWorkerCompleted;
            decWorker.WorkerReportsProgress = true;
            decWorker.WorkerSupportsCancellation = true;

            AESDecArgs args = new AESDecArgs();
            args.content = content;
            args.key = KeyPw.Password.ToString();

            log.addLog("AES Decryption Starting");
            decWorker.RunWorkerAsync(args);
        }

        void decWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker encWorker = sender as BackgroundWorker;
            AESDecArgs args = e.Argument as AESDecArgs;

            //https://stackoverflow.com/users/188474/brett's Simple SimpleDecryptWithPassword
            RijndaelManaged aesAlg = null;
            try {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(args.key, _salt);
                //allow iteration count to change?

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(args.content);
                using (MemoryStream msDecrypt = new MemoryStream(bytes)) {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            try {
                                AESdecryptedContent = srDecrypt.ReadToEnd();
                            } catch (CryptographicException ctEx) {
                                //Incorrect decryption key
                                Console.Error.WriteLine(ctEx.ToString());
                            }
                    }
                }
            } finally {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length) {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length) {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        void decWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) {
                log.addLog("AES Decryption Cancelled");
            } else {
                if (!AESdecryptedContent.Equals(string.Empty)) {
                    log.addLog("AES Decryption Completed");

                    log.addLog("Request: EncDecFinish");
                    EncDecFinishDialog edf = new EncDecFinishDialog(main, settings, log, 2, AESdecryptedContent);
                    toggleVisibility(false);
                    edf.toggleVisibility(true);
                } else {
                    log.addLog("AES Decryption Failed: Incorrect Password");

                    log.addLog("Request: EncDecFinish");
                    EncDecFinishDialog edf = new EncDecFinishDialog(main, settings, log, 3, AESdecryptedContent);
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
            if (vis) {
                log.addLog("Open DecryptionDialog");
                this.Visibility = Visibility.Visible;
            } else {
                log.addLog("Collapse DecryptionDialog");
                this.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            beginAESDec();
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
            log.addLog("Close DecryptionDialog");
        }
    }

    /// <summary>
    /// Used to pass arguments to decWorker
    /// </summary>
    public class AESDecArgs
    {
        public string content;
        public string key;
    }
}
