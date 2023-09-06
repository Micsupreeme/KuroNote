using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;

namespace KuroNote
{
    /// <summary>
    /// conf.json is converted into an instance of KuroNoteSettings using JSON.NET
    /// NOTE: logging cannot happen here because logging is a preference, so the "logging" attribute here must be evaluated before a log can be initialised
    /// </summary>
    public class KuroNoteSettings
    {
        //meta
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
        public bool rtfMode = false;
        public bool rememberRecentFiles = true;
        public bool floating = false;
        public bool useAscii = false;
        public bool spellCheck = true;
        public bool rememberWindowSize = false;
        public bool fullFilePath = false;
        public bool stretchImages = false;
        public bool rememberFontUpDn = true;
        public bool wordWrap = true;
        public bool overTyping = true;
        public bool autoWordSelection = false;
        public double windowHeight = 500;
        public double windowWidth = 750;
        public List<int> achList = new List<int>() { };
        public int achLast = -1;
        public int achStartups = 0;
        public int achOpens = 0;
        public int achSaves = 0;
        public int achSaveAs = 0;
        public int achEncrypts = 0;
        public int achCustoms = 0;
        public int achFullscreens = 0;
        public int achOptions = 0;
        public int profL = 0;
        public int profAp = 0;
        public bool seenRtfWelcome = false;
        public bool encopen = true;

        /// <summary>
        /// Update this object according to the settings stored in conf.json
        /// </summary>
        public void RetrieveSettings()
        {
            try {
                using (StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\conf.json")) {
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
                    this.rtfMode = knsFile.rtfMode;
                    this.rememberRecentFiles = knsFile.rememberRecentFiles;
                    this.rememberWindowSize = knsFile.rememberWindowSize;
                    this.floating = knsFile.floating;
                    this.useAscii = knsFile.useAscii;
                    this.spellCheck = knsFile.spellCheck;
                    this.rememberFontUpDn = knsFile.rememberFontUpDn;
                    this.fullFilePath = knsFile.fullFilePath;
                    this.stretchImages = knsFile.stretchImages;
                    this.wordWrap = knsFile.wordWrap;
                    this.overTyping = knsFile.overTyping;
                    this.autoWordSelection = knsFile.autoWordSelection;
                    this.windowHeight = knsFile.windowHeight;
                    this.windowWidth = knsFile.windowWidth;
                    this.achList = knsFile.achList;
                    this.achLast = knsFile.achLast;
                    this.achStartups = knsFile.achStartups;
                    this.achOpens = knsFile.achOpens;
                    this.achSaves = knsFile.achSaves;
                    this.achSaveAs = knsFile.achSaveAs;
                    this.achEncrypts = knsFile.achEncrypts;
                    this.achCustoms = knsFile.achCustoms;
                    this.achFullscreens = knsFile.achFullscreens;
                    this.achOptions = knsFile.achOptions;
                    this.profL = knsFile.profL;
                    this.profAp = knsFile.profAp;
                    this.seenRtfWelcome = knsFile.seenRtfWelcome;
                    this.encopen = knsFile.encopen;
                }
            } catch (FileNotFoundException fnfe) {
                Debug.WriteLine("FileNotFound error during RetrieveSettings before log initialised:");
                Debug.WriteLine(fnfe.ToString());
                Debug.WriteLine("Creating new conf.json!");
                UpdateSettings(); //Creates a new conf file with default values (since the values weren't changed from the defaults)
            }  catch (Exception e) {
                Debug.WriteLine("Generic error during RetrieveSettings before log initialised:");
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Update conf.json according to the settings stored in this object
        /// </summary>
        public void UpdateSettings()
        {
            try {
                using (StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\KuroNote\\conf.json")) {
                    string json = JsonConvert.SerializeObject(this);

                    sw.Write(json);
                }
            } catch (Exception e) {
                Debug.WriteLine("Error during UpdateSettings before log initialised:");
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// If gamification is active: adds the specified amount of AP and can handle levelling up (one level at a time only)
        /// </summary>
        /// <param name="ap">The amount of AP to add (NOTE: must not be large enough to trigger multiple level-ups at once)</param>
        /// <param name="apMax">The AP limit of the current level</param>
        public void incrementAp(int ap, int apMax)
        {
            if (this.gamification) {
                if (this.profAp + ap >= apMax) {
                    //Incrementing will cause a level-up
                    int apOverflow = (this.profAp + ap) - apMax;    //amount of ap to apply after level-up
                    this.profL++;                                   //level-up
                    this.profAp = 0 + apOverflow;                   //reset ap to 0 (because new level) and add overflow
                } else {
                    //Incrementing won't cause a level-up
                    this.profAp += ap;                              //add ap
                }
                UpdateSettings();
            }
        }
    }
}
