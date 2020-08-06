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
        private const string WINDOW_NAME = "Font";

        //Globals
        private string appName;
        MainWindow main;
        KuroNoteSettings settings;
        Log log;
        string content;                 //a copy from MainRtb of the text to encrypt

        BackgroundWorker encWorker;
        string AESencryptedContent = string.Empty;

        BackgroundWorker decWorker;
        string AESdecryptedContent = string.Empty;

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
            args.key = KeyTxt.Text;

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
                    log.addLog(AESencryptedContent);
                } else {
                    log.addLog("AES Encryption Failed");
                }      
            }
            btnOk.IsEnabled = true;
        }

        private void beginAESDec()
        {
            btnTempDec.IsEnabled = false;

            decWorker = new BackgroundWorker();
            decWorker.DoWork += decWorker_DoWork;
            //decWorker.ProgressChanged += decWorker_ProgressChanged;
            decWorker.RunWorkerCompleted += decWorker_RunWorkerCompleted;
            decWorker.WorkerReportsProgress = true;
            decWorker.WorkerSupportsCancellation = true;

            AESDecArgs args = new AESDecArgs();
            args.content = AESencryptedContent;             //temp: encrypt first then this will decrypt the result
            args.key = KeyTxt.Text;

            log.addLog("AES Decryption Starting");
            decWorker.RunWorkerAsync(args);
        }

        void decWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker encWorker = sender as BackgroundWorker;
            AESDecArgs args = e.Argument as AESDecArgs;

            //https://stackoverflow.com/users/188474/brett's Simple SimpleDecryptWithPassword
            RijndaelManaged aesAlg = null;
            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(args.key, _salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(args.content);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            try
                            {
                                AESdecryptedContent = srDecrypt.ReadToEnd();
                            } catch(CryptographicException ctEx)
                            {
                                //Incorrect decryption key
                                Console.Error.Write(ctEx.ToString());
                                MessageBox.Show("Incorrect Decryption Key!");
                            }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        void decWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                log.addLog("AES Decryption Cancelled");
            }
            else
            {
                if(!AESdecryptedContent.Equals(string.Empty)) {
                    log.addLog("AES Decryption Completed");
                    log.addLog(AESdecryptedContent);
                } else {
                    log.addLog("AES Decryption Failed: Incorrect Password");
                }
            }
            btnTempDec.IsEnabled = true;
        }

        private void btnTempDec_Click(object sender, RoutedEventArgs e)
        {
            //toggleVisibility(false);
            beginAESDec();
        }

        /// <summary>
        /// When the user clicks "OK" or hits ENTER
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //toggleVisibility(false);
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
            log.addLog("Close EncryptDialog");
        }
    }

    public class AESEncArgs
    {
        public string content;
        public string key;
    }

    public class AESDecArgs
    {
        public string content;
        public string key;
    }
}
