using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;

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
        public string fontFamily = "Verdana";
        public int fontSize = 17;
        public FontWeight fontWeight = FontWeights.Normal;
        public FontStyle fontStyle = FontStyles.Normal;
        public int themeId = 0;
        public bool themeWithFont = true;
        public int customThemeIndex = 1000;
        public bool gamification = true;
        public bool logging = false;
        public bool floating = false;
        public bool useAscii = false;
        public bool rememberWindowSize = false;
        public bool fullFilePath = false;
        public double windowHeight = 500;
        public double windowWidth = 750;
        public List<int> achList = new List<int>() { };
        public int achStartups = 0;
        public int achOpens = 0;
        public int achSaves = 0;
        public int achSaveAs = 0;
        public int achEncrypts = 0;
        public int achCustoms = 0;

        public KuroNoteSettings(Log mainLog)
        {
            log = mainLog;
        }

        /// <summary>
        /// Update this object according to the settings stored in conf.json
        /// </summary>
        public void RetrieveSettings()
        {
            try {
                if(log != null) {
                    log.addLog("Reading conf.json");
                }
                using (StreamReader sr = new StreamReader(appPath + "conf/conf.json")) {
                    string json = sr.ReadToEnd();
                    KuroNoteSettings knsFile = JsonConvert.DeserializeObject<KuroNoteSettings>(json);

                    this.fontFamily = knsFile.fontFamily;
                    this.fontSize = knsFile.fontSize;
                    this.fontWeight = knsFile.fontWeight;
                    this.fontStyle = knsFile.fontStyle;
                    this.themeId = knsFile.themeId;
                    this.themeWithFont = knsFile.themeWithFont;
                    this.customThemeIndex = knsFile.customThemeIndex;
                    this.gamification = knsFile.gamification;
                    this.logging = knsFile.logging;
                    this.floating = knsFile.floating;
                    this.useAscii = knsFile.useAscii;
                    this.rememberWindowSize = knsFile.rememberWindowSize;
                    this.fullFilePath = knsFile.fullFilePath;
                    this.windowHeight = knsFile.windowHeight;
                    this.windowWidth = knsFile.windowWidth;
                    this.achList = knsFile.achList;
                    this.achStartups = knsFile.achStartups;
                    this.achOpens = knsFile.achOpens;
                    this.achSaves = knsFile.achSaves;
                    this.achSaveAs = knsFile.achSaveAs;
                    this.achEncrypts = knsFile.achEncrypts;
                    this.achCustoms = knsFile.achCustoms;
                    if (log != null) {
                        log.addLog("Successfully read conf.json");
                    }
                }
            } catch (Exception e) {
                if (log != null) {
                    log.addLog(e.ToString());
                } else {
                    Console.Error.WriteLine("Error during RetrieveSettings before log initialised:");
                    Console.Error.WriteLine(e.ToString());
                }
                UpdateSettings(); //Creates a new conf file with default values (since the values weren't changed from the defaults)
            }
        }

        /// <summary>
        /// Update conf.json according to the settings stored in this object
        /// </summary>
        public void UpdateSettings()
        {
            try {
                if(log != null) {
                    log.addLog("Updating conf.json");
                }
                using (StreamWriter sw = new StreamWriter(appPath + "conf/conf.json")) {
                    string json = JsonConvert.SerializeObject(this);

                    sw.Write(json);
                    if(log != null) {
                        log.addLog("Successfully updated conf.json");
                    }
                }
            } catch (Exception e) {
                if (log != null) {
                    log.addLog(e.ToString());
                } else {
                    Console.Error.WriteLine("Error during UpdateSettings before log initialised:");
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }
    }
}
