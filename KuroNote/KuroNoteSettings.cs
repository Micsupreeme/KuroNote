using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace KuroNote
{
    /// <summary>
    /// conf.json is converted into an instance of KuroNoteSettings using JSON.NET
    /// </summary>
    public class KuroNoteSettings
    {
        private Log log;
        private string appPath = AppDomain.CurrentDomain.BaseDirectory;

        //attributes with default values
        public string fontFamily = "Arial";
        public int fontSize = 14;
        public FontWeight fontWeight = FontWeights.Normal;
        public FontStyle fontStyle = FontStyles.Normal;
        public string themeName = "Default";

        public KuroNoteSettings(Log mainLog)
        {
            log = mainLog;
        }

        /// <summary>
        /// Update this object according to the settings stored in conf.json
        /// </summary>
        public void RetrieveSettings()
        {
            try
            {
                log.addLog("Reading conf.json");
                using (StreamReader sr = new StreamReader(appPath + "conf/conf.json"))
                {
                    string json = sr.ReadToEnd();
                    KuroNoteSettings knsFile = JsonConvert.DeserializeObject<KuroNoteSettings>(json);

                    this.fontFamily = knsFile.fontFamily;
                    this.fontSize = knsFile.fontSize;
                    this.fontWeight = knsFile.fontWeight;
                    this.fontStyle = knsFile.fontStyle;
                    this.themeName = knsFile.themeName;
                    log.addLog("Successfully read conf.json");
                }
            }
            catch (Exception e)
            {
                log.addLog(e.ToString());
            }
        }

        /// <summary>
        /// Update conf.json according to the settings stored in this object
        /// </summary>
        public void UpdateSettings()
        {
            try
            {
                log.addLog("Updating conf.json");
                using (StreamWriter sw = new StreamWriter(appPath + "conf/conf.json"))
                {
                    string json = JsonConvert.SerializeObject(this);

                    sw.Write(json);
                    log.addLog("Successfully updated conf.json");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
