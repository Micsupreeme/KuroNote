﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KuroNote
{
    public class KuroNoteTheme
    {
        //meta
        public int themeId;
        public string themeName;
        public string themeDesc;
        public bool hasImage;

        //colours
        public SolidColorBrush bgBrush;         //background for the whole window
        public ImageBrush imgBrush;             //background image OR
        public SolidColorBrush solidBrush;      //solid background colour
        public SolidColorBrush menuBrush;       //top menu colour
        public SolidColorBrush statusBrush;     //status bar colour
        public SolidColorBrush textBrush;       //foreground colour

        //font
        public string fontFamily;
        public short fontSize;
        public FontWeight fontWeight;
        public FontStyle fontStyle;

        /// <summary>
        /// Creates a new KuroNoteTheme with an image background
        /// </summary>
        public KuroNoteTheme(int _themeId, string _themeName, string _themeDesc,
                            SolidColorBrush _bgBrush, ImageBrush _imgBrush,
                            SolidColorBrush _menuBrush, SolidColorBrush _statusBrush, SolidColorBrush _textBrush,
                            string _fontFamily, short _fontSize, FontWeight _fontWeight, FontStyle _fontStyle)
        {
            this.themeId = _themeId;
            this.themeName = _themeName;
            this.themeDesc = _themeDesc;
            this.hasImage = true;
            this.bgBrush = _bgBrush;
            this.imgBrush = _imgBrush;
            this.solidBrush = null;
            this.menuBrush = _menuBrush;
            this.statusBrush = _statusBrush;
            this.textBrush = _textBrush;
            this.fontFamily = _fontFamily;
            this.fontSize = _fontSize;
            this.fontWeight = _fontWeight;
            this.fontStyle = _fontStyle;
        }

        /// <summary>
        /// Creates a new KuroNoteTheme with a solid colour background
        /// </summary>
        public KuroNoteTheme(int _themeId, string _themeName, string _themeDesc,
                            SolidColorBrush _bgBrush, SolidColorBrush _solidBrush,
                            SolidColorBrush _menuBrush, SolidColorBrush _statusBrush, SolidColorBrush _textBrush,
                            string _fontFamily, short _fontSize, FontWeight _fontWeight, FontStyle _fontStyle)
        {
            this.themeId = _themeId;
            this.themeName = _themeName;
            this.themeDesc = _themeDesc;
            this.hasImage = false;
            this.bgBrush = _bgBrush;
            this.imgBrush = null;
            this.solidBrush = _solidBrush;
            this.menuBrush = _menuBrush;
            this.statusBrush = _statusBrush;
            this.textBrush = _textBrush;
            this.fontFamily = _fontFamily;
            this.fontSize = _fontSize;
            this.fontWeight = _fontWeight;
            this.fontStyle = _fontStyle;
        }
    }
}
